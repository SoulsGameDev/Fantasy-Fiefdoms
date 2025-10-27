using UnityEngine;
using Inventory.Core;
using Inventory.Data;
using Inventory.Effects;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for using (consuming) an item from inventory.
    /// Handles cooldowns, effects, and item consumption.
    /// Supports undo/redo for reversible effects.
    /// </summary>
    public class UseItemCommand : CommandBase
    {
        private readonly Inventory.Core.Inventory inventory;
        private readonly string itemID;
        private readonly GameObject user;
        private readonly GameObject target; // For targeted consumables
        private readonly CooldownTracker cooldownTracker;

        // For undo
        private ItemStack consumedStack;
        private bool wasConsumed;
        private int usedFromSlot = -1;

        /// <summary>
        /// Creates a new command to use an item.
        /// </summary>
        /// <param name="inventory">Inventory containing the item</param>
        /// <param name="itemID">ID of the item to use</param>
        /// <param name="user">The character using the item</param>
        /// <param name="target">Target for the item (can be null for self-targeted)</param>
        /// <param name="cooldownTracker">Cooldown tracker for the user (optional)</param>
        public UseItemCommand(
            Inventory.Core.Inventory inventory,
            string itemID,
            GameObject user,
            GameObject target = null,
            CooldownTracker cooldownTracker = null)
        {
            this.inventory = inventory;
            this.itemID = itemID;
            this.user = user;
            this.target = target ?? user; // Default to self if no target
            this.cooldownTracker = cooldownTracker;
        }

        public override bool CanExecute()
        {
            // Check if inventory and item are valid
            if (inventory == null)
            {
                ErrorMessage = "Inventory is null";
                return false;
            }

            if (string.IsNullOrEmpty(itemID))
            {
                ErrorMessage = "Item ID is null or empty";
                return false;
            }

            if (user == null)
            {
                ErrorMessage = "User is null";
                return false;
            }

            // Check if item exists in inventory
            if (inventory.GetItemQuantity(itemID) <= 0)
            {
                ErrorMessage = $"Item {itemID} not found in inventory";
                return false;
            }

            // Get the item type
            ItemType itemType = InventoryManager.Instance.FindItemType(itemID);
            if (itemType == null)
            {
                ErrorMessage = $"Item type {itemID} not found";
                return false;
            }

            // Check if it's a consumable
            if (!(itemType is ConsumableType consumable))
            {
                ErrorMessage = $"{itemType.Name} is not a consumable item";
                return false;
            }

            // Check combat restrictions
            // Note: This is a placeholder - you'll need to implement combat state detection
            bool isInCombat = false; // TODO: Get from game manager or combat system
            if (!consumable.CanUse(isInCombat, out string reason))
            {
                ErrorMessage = reason;
                return false;
            }

            // Check cooldown
            if (cooldownTracker != null && cooldownTracker.IsOnCooldown(itemID))
            {
                float remaining = cooldownTracker.GetRemainingCooldown(itemID);
                ErrorMessage = $"{itemType.Name} is on cooldown ({remaining:F1}s remaining)";
                return false;
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning($"Cannot use item: {ErrorMessage}");
                return false;
            }

            // Get the consumable type
            ConsumableType consumable = InventoryManager.Instance.FindItemType(itemID) as ConsumableType;
            if (consumable == null)
                return false;

            try
            {
                // Find the slot containing the item
                usedFromSlot = FindItemSlot(itemID);
                if (usedFromSlot < 0)
                {
                    ErrorMessage = "Could not find item in inventory";
                    return false;
                }

                // Store the stack for undo
                consumedStack = inventory.GetStack(usedFromSlot);

                // Apply effects
                ApplyConsumableEffects(consumable);

                // Handle item consumption
                if (consumable.ConsumeOnUse)
                {
                    // Remove one item from inventory
                    bool removed = inventory.TryRemoveItem(itemID, 1, out int actuallyRemoved);
                    if (!removed || actuallyRemoved != 1)
                    {
                        ErrorMessage = "Failed to consume item";
                        return false;
                    }
                    wasConsumed = true;
                }

                // Start cooldown
                if (cooldownTracker != null && consumable.CooldownSeconds > 0)
                {
                    cooldownTracker.StartCooldown(itemID, consumable.CooldownSeconds);
                }

                // Play sound/animation
                if (consumable.UseSound != null)
                {
                    AudioSource.PlayClipAtPoint(consumable.UseSound, user.transform.position);
                }

                if (!string.IsNullOrEmpty(consumable.UseAnimation))
                {
                    var animator = user.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.SetTrigger(consumable.UseAnimation);
                    }
                }

                Debug.Log($"Used {consumable.Name}");
                return true;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Exception while using item: {ex.Message}";
                Debug.LogError(ErrorMessage);
                return false;
            }
        }

        public override bool Undo()
        {
            if (!wasConsumed)
            {
                Debug.LogWarning("Cannot undo - item was not consumed");
                return false;
            }

            // Restore the consumed item
            ItemStack restoreStack = consumedStack.Type.CreateStack(1);
            bool restored = inventory.TryAddItem(restoreStack, out int remaining);

            if (!restored || remaining > 0)
            {
                Debug.LogWarning("Failed to restore consumed item - inventory may be full");
                return false;
            }

            // Note: We don't remove the applied effects during undo, as they may have already
            // taken effect (heals applied, buffs started, etc.). Effects will naturally expire.
            // If you need to remove effects on undo, you'll need to track which effects were applied.

            // Clear cooldown
            if (cooldownTracker != null)
            {
                cooldownTracker.ClearCooldown(itemID);
            }

            Debug.Log($"Undid use of {consumedStack.Type.Name}");
            return true;
        }

        private int FindItemSlot(string itemID)
        {
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                ItemStack stack = inventory.GetStack(i);
                if (!stack.IsEmpty && stack.Type.ItemID == itemID)
                {
                    return i;
                }
            }
            return -1;
        }

        private void ApplyConsumableEffects(ConsumableType consumable)
        {
            if (consumable.ConsumableEffects == null || consumable.ConsumableEffects.Count == 0)
            {
                Debug.LogWarning($"Consumable {consumable.Name} has no effects");
                return;
            }

            // Apply all effects from the consumable to the target
            ItemEffectFactory.ApplyConsumableEffects(target, consumable);

            Debug.Log($"Applied {consumable.ConsumableEffects.Count} effects from {consumable.Name} to {target.name}");
        }
    }
}
