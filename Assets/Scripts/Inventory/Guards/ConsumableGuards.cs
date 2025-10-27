using UnityEngine;
using Inventory.Core;
using Inventory.Data;

namespace Inventory.Guards
{
    /// <summary>
    /// Guard that validates whether an item can be used/consumed.
    /// Checks inventory, cooldowns, combat state, and item requirements.
    /// </summary>
    public class CanUseItemGuard : GuardBase
    {
        private readonly Inventory.Core.Inventory inventory;
        private readonly string itemID;
        private readonly GameObject user;
        private readonly CooldownTracker cooldownTracker;

        public CanUseItemGuard(
            Inventory.Core.Inventory inventory,
            string itemID,
            GameObject user,
            CooldownTracker cooldownTracker = null)
        {
            this.inventory = inventory;
            this.itemID = itemID;
            this.user = user;
            this.cooldownTracker = cooldownTracker;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            // Validate parameters
            if (inventory == null)
            {
                return new GuardResult(false, "Inventory is null");
            }

            if (string.IsNullOrEmpty(itemID))
            {
                return new GuardResult(false, "Item ID is null or empty");
            }

            if (user == null)
            {
                return new GuardResult(false, "User is null");
            }

            // Check if item exists in inventory
            int quantity = inventory.GetItemQuantity(itemID);
            if (quantity <= 0)
            {
                return new GuardResult(false, $"Item not found in inventory");
            }

            // Get item type
            ItemType itemType = InventoryManager.Instance.FindItemType(itemID);
            if (itemType == null)
            {
                return new GuardResult(false, $"Item type {itemID} not found in database");
            }

            // Check if it's a consumable
            if (!(itemType is ConsumableType consumable))
            {
                return new GuardResult(false, $"{itemType.Name} is not a consumable item");
            }

            // Check combat state restrictions
            bool isInCombat = GetCombatState(context);
            if (!consumable.CanUse(isInCombat, out string combatReason))
            {
                return new GuardResult(false, combatReason);
            }

            // Check cooldown
            if (cooldownTracker != null && cooldownTracker.IsOnCooldown(itemID))
            {
                float remaining = cooldownTracker.GetRemainingCooldown(itemID);
                return new GuardResult(false, $"{itemType.Name} is on cooldown ({remaining:F1}s remaining)");
            }

            // Check if user has required components for effects
            if (consumable.ConsumableEffects != null && consumable.ConsumableEffects.Count > 0)
            {
                // Check if effects require specific components
                foreach (var effectData in consumable.ConsumableEffects)
                {
                    if (RequiresCharacterStats(effectData) && !HasCharacterStats(user))
                    {
                        return new GuardResult(false, $"User does not have required components for {consumable.Name} effects");
                    }
                }
            }

            // All checks passed
            return new GuardResult(true);
        }

        private bool GetCombatState(GuardContext context)
        {
            // Try to get combat state from context
            if (context != null && context.Has("IsInCombat"))
            {
                return context.Get<bool>("IsInCombat");
            }

            // TODO: Get from game manager or combat system
            // For now, assume not in combat
            return false;
        }

        private bool RequiresCharacterStats(ItemEffectData effectData)
        {
            // Effects that modify stats require ICharacterStats
            return effectData.EffectType == ItemEffectType.StatModifier ||
                   effectData.EffectType == ItemEffectType.Buff ||
                   effectData.EffectType == ItemEffectType.Debuff ||
                   effectData.EffectType == ItemEffectType.DamageOverTime ||
                   effectData.EffectType == ItemEffectType.HealOverTime ||
                   effectData.EffectType == ItemEffectType.Heal ||
                   effectData.EffectType == ItemEffectType.RestoreMana;
        }

        private bool HasCharacterStats(GameObject target)
        {
            // Check if target has stats component or effect manager
            return target.GetComponent<ICharacterStats>() != null ||
                   target.GetComponent<Effects.EffectManager>() != null;
        }
    }

    /// <summary>
    /// Guard that validates whether a specific consumable effect can be applied.
    /// Checks target validity, effect requirements, and application conditions.
    /// </summary>
    public class CanApplyEffectGuard : GuardBase
    {
        private readonly GameObject target;
        private readonly ItemEffectData effectData;

        public CanApplyEffectGuard(GameObject target, ItemEffectData effectData)
        {
            this.target = target;
            this.effectData = effectData;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            if (target == null)
            {
                return new GuardResult(false, "Target is null");
            }

            if (effectData == null)
            {
                return new GuardResult(false, "Effect data is null");
            }

            // Check if target is valid for effect type
            switch (effectData.EffectType)
            {
                case ItemEffectType.Heal:
                case ItemEffectType.HealOverTime:
                    // Healing requires health to not be at max
                    // TODO: Check current health vs max health
                    break;

                case ItemEffectType.RestoreMana:
                    // Mana restore requires mana to not be at max
                    // TODO: Check current mana vs max mana
                    break;

                case ItemEffectType.DamageOverTime:
                case ItemEffectType.Debuff:
                    // Can't apply negative effects to self in some contexts
                    // This would depend on your game rules
                    break;
            }

            // Check if target already has this effect (for non-stacking effects)
            var effectManager = target.GetComponent<Effects.EffectManager>();
            if (effectManager != null && effectData.Duration > 0)
            {
                // Check if we allow effect stacking
                bool allowStacking = true; // TODO: Make this configurable per effect

                if (!allowStacking && effectManager.HasEffect(effectData.EffectType, effectData.StatName))
                {
                    return new GuardResult(false, $"Target already has {effectData.Description} effect");
                }
            }

            return new GuardResult(true);
        }
    }

    /// <summary>
    /// Guard that validates cooldown status for an item.
    /// </summary>
    public class IsNotOnCooldownGuard : GuardBase
    {
        private readonly CooldownTracker cooldownTracker;
        private readonly string itemID;

        public IsNotOnCooldownGuard(CooldownTracker cooldownTracker, string itemID)
        {
            this.cooldownTracker = cooldownTracker;
            this.itemID = itemID;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            if (cooldownTracker == null)
            {
                // No cooldown tracker means no cooldowns
                return new GuardResult(true);
            }

            if (string.IsNullOrEmpty(itemID))
            {
                return new GuardResult(false, "Item ID is null or empty");
            }

            if (cooldownTracker.IsOnCooldown(itemID))
            {
                float remaining = cooldownTracker.GetRemainingCooldown(itemID);
                return new GuardResult(false, $"Item is on cooldown ({remaining:F1}s remaining)");
            }

            return new GuardResult(true);
        }
    }
}
