using UnityEngine;
using System.Collections.Generic;

namespace Inventory.Data
{
    /// <summary>
    /// Defines a consumable item type that can be used (consumed) by the player.
    /// Consumables include potions, food, scrolls, and other single-use or multi-use items.
    /// </summary>
    [CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable Type")]
    public class ConsumableType : ItemType
    {
        [Header("Consumable Settings")]
        [SerializeField] private bool consumeOnUse = true;
        [SerializeField] private int usesPerStack = 1;
        [SerializeField] private float cooldownSeconds = 0f;
        [SerializeField] private bool canUseInCombat = true;
        [SerializeField] private bool canUseOutOfCombat = true;
        [SerializeField] private float castTimeSeconds = 0f;
        [SerializeField] private string useAnimation = "";
        [SerializeField] private AudioClip useSound;

        [Header("Effects")]
        [SerializeField] private List<ItemEffectData> consumableEffects = new List<ItemEffectData>();

        [Header("Targeting")]
        [SerializeField] private ConsumableTarget targetType = ConsumableTarget.Self;
        [SerializeField] private float targetRange = 0f;

        /// <summary>
        /// Whether the item is consumed (removed) after use.
        /// </summary>
        public bool ConsumeOnUse => consumeOnUse;

        /// <summary>
        /// Number of uses per stack before the item is consumed.
        /// For example, a bandage might have 3 uses per item.
        /// </summary>
        public int UsesPerStack => usesPerStack;

        /// <summary>
        /// Cooldown time in seconds before this item can be used again.
        /// </summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>
        /// Whether this item can be used during combat.
        /// </summary>
        public bool CanUseInCombat => canUseInCombat;

        /// <summary>
        /// Whether this item can be used outside of combat.
        /// </summary>
        public bool CanUseOutOfCombat => canUseOutOfCombat;

        /// <summary>
        /// Cast time in seconds. If > 0, the player must channel before the effect applies.
        /// </summary>
        public float CastTimeSeconds => castTimeSeconds;

        /// <summary>
        /// Animation trigger name for using this item.
        /// </summary>
        public string UseAnimation => useAnimation;

        /// <summary>
        /// Sound effect to play when using this item.
        /// </summary>
        public AudioClip UseSound => useSound;

        /// <summary>
        /// Effects that are applied when this consumable is used.
        /// </summary>
        public List<ItemEffectData> ConsumableEffects => consumableEffects;

        /// <summary>
        /// Target type for this consumable (self, ally, enemy, ground).
        /// </summary>
        public ConsumableTarget TargetType => targetType;

        /// <summary>
        /// Maximum range for targeted consumables (0 = no range limit or self-only).
        /// </summary>
        public float TargetRange => targetRange;

        /// <summary>
        /// Validates if this consumable can be used in the current context.
        /// </summary>
        /// <param name="isInCombat">Whether the user is currently in combat</param>
        /// <param name="reason">Reason why the item cannot be used</param>
        /// <returns>True if the item can be used</returns>
        public bool CanUse(bool isInCombat, out string reason)
        {
            if (isInCombat && !canUseInCombat)
            {
                reason = $"{Name} cannot be used in combat";
                return false;
            }

            if (!isInCombat && !canUseOutOfCombat)
            {
                reason = $"{Name} can only be used in combat";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets a description of what this consumable does.
        /// </summary>
        public string GetEffectDescription()
        {
            if (consumableEffects == null || consumableEffects.Count == 0)
            {
                return "No effects";
            }

            var descriptions = new List<string>();
            foreach (var effect in consumableEffects)
            {
                descriptions.Add(FormatEffectDescription(effect));
            }

            return string.Join("\n", descriptions);
        }

        private string FormatEffectDescription(ItemEffectData effectData)
        {
            switch (effectData.EffectType)
            {
                case ItemEffectType.Heal:
                    return $"Restores {effectData.Value} health";

                case ItemEffectType.RestoreMana:
                    return $"Restores {effectData.Value} mana";

                case ItemEffectType.StatModifier:
                    string statName = effectData.StatName;
                    float value = effectData.Value;
                    float duration = effectData.Duration;

                    string sign = value >= 0 ? "+" : "";
                    string durationText = duration > 0 ? $" for {duration}s" : "";

                    if (effectData.IsPercentage)
                    {
                        return $"{sign}{value * 100}% {statName}{durationText}";
                    }
                    else
                    {
                        return $"{sign}{value} {statName}{durationText}";
                    }

                case ItemEffectType.Passive:
                    return effectData.Description;

                case ItemEffectType.Buff:
                    return $"Buff: {effectData.Description} ({effectData.Duration}s)";

                case ItemEffectType.Debuff:
                    return $"Debuff: {effectData.Description} ({effectData.Duration}s)";

                case ItemEffectType.DamageOverTime:
                    return $"{effectData.Value} damage over {effectData.Duration}s";

                case ItemEffectType.HealOverTime:
                    return $"{effectData.Value} healing over {effectData.Duration}s";

                default:
                    return effectData.Description;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure consumables are in the Consumable category
            if (Category != ItemCategory.Consumable)
            {
                Debug.LogWarning($"ConsumableType {name} should have Category set to Consumable");
            }

            // Validate uses per stack
            if (usesPerStack < 1)
            {
                usesPerStack = 1;
            }

            // Validate cooldown
            if (cooldownSeconds < 0f)
            {
                cooldownSeconds = 0f;
            }

            // Validate cast time
            if (castTimeSeconds < 0f)
            {
                castTimeSeconds = 0f;
            }

            // Validate range
            if (targetRange < 0f)
            {
                targetRange = 0f;
            }
        }
#endif
    }

    /// <summary>
    /// Defines who or what can be targeted by a consumable.
    /// </summary>
    public enum ConsumableTarget
    {
        Self,           // Only the user
        Ally,           // Friendly target
        Enemy,          // Hostile target
        Ground,         // Ground-targeted (AoE)
        AnyCharacter    // Any character (friend or foe)
    }
}
