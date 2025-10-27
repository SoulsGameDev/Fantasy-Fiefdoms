using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for unequipping items from an equipment inventory.
    /// Supports undo/redo and can optionally move items to a target inventory.
    /// </summary>
    public class UnequipItemCommand : CommandBase
    {
        private readonly EquipmentInventory equipmentInventory;
        private readonly Inventory.Core.Inventory targetInventory;
        private readonly EquipmentSlot slotToUnequip;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private ItemStack unequippedItem;

        public override string Description =>
            $"Unequip item from {slotToUnequip} slot";

        /// <summary>
        /// Creates a new UnequipItemCommand.
        /// </summary>
        /// <param name="equipmentInventory">The equipment inventory to unequip from</param>
        /// <param name="slotToUnequip">Which slot to unequip</param>
        /// <param name="targetInventory">Where to place the unequipped item (can be null)</param>
        public UnequipItemCommand(
            EquipmentInventory equipmentInventory,
            EquipmentSlot slotToUnequip,
            Inventory.Core.Inventory targetInventory = null)
        {
            this.equipmentInventory = equipmentInventory;
            this.slotToUnequip = slotToUnequip;
            this.targetInventory = targetInventory;

            // Set up guards
            this.guards = new List<ITransitionGuard>
            {
                new CanUnequipGuard()
            };
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (equipmentInventory == null) return false;
            if (slotToUnequip == EquipmentSlot.None) return false;

            // Check if slot has an item
            if (equipmentInventory.IsSlotEmpty(slotToUnequip))
            {
                Debug.LogWarning("Cannot unequip - slot is empty");
                return false;
            }

            // Evaluate guards
            InventoryGuardContext context = new InventoryGuardContext(
                targetInventory,
                ItemStack.Empty,
                InventoryOperation.Unequip);

            foreach (ITransitionGuard guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"UnequipItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("UnequipItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Try to unequip the item
                bool success = equipmentInventory.TryUnequip(slotToUnequip, out unequippedItem, out string reason);

                if (!success)
                {
                    Debug.LogError($"Failed to unequip item: {reason}");
                    return false;
                }

                // If target inventory specified, try to add the item there
                if (targetInventory != null && !unequippedItem.IsEmpty)
                {
                    if (!targetInventory.TryAddItem(unequippedItem, out int remaining))
                    {
                        Debug.LogWarning("Failed to add unequipped item to target inventory");
                        // Re-equip the item since we couldn't place it
                        equipmentInventory.TryEquip(unequippedItem, out _, out _);
                        return false;
                    }

                    if (remaining > 0)
                    {
                        Debug.LogWarning($"Could not fit all items in target inventory. {remaining} remaining.");
                    }
                }

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"UnequipItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("UnequipItemCommand cannot undo - not executed or already undone");
                return false;
            }

            try
            {
                // Remove from target inventory if it was added there
                if (targetInventory != null && !unequippedItem.IsEmpty)
                {
                    targetInventory.TryRemoveItem(unequippedItem.Type.ItemID, 1, out _);
                }

                // Re-equip the item
                bool success = equipmentInventory.TryEquip(unequippedItem, out string reason, out _);

                if (!success)
                {
                    Debug.LogError($"Failed to undo unequip: {reason}");
                    return false;
                }

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"UnequipItemCommand Undo failed: {e.Message}");
                return false;
            }
        }
    }
}
