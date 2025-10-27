using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for removing items from an inventory.
    /// Supports undo/redo and integrates with guard validation.
    /// </summary>
    public class RemoveItemCommand : CommandBase
    {
        private readonly Inventory.Core.Inventory inventory;
        private readonly string itemID;
        private readonly int quantity;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private List<SlotStateSnapshot> removedItems = new List<SlotStateSnapshot>();
        private int actualRemovedQuantity = 0;

        public override string Description =>
            $"Remove {quantity}x {itemID} from {inventory.InventoryID}";

        /// <summary>
        /// Creates a new RemoveItemCommand with default guards.
        /// </summary>
        public RemoveItemCommand(Inventory.Core.Inventory inventory, string itemID, int quantity)
        {
            this.inventory = inventory;
            this.itemID = itemID;
            this.quantity = quantity;
            this.guards = new List<ITransitionGuard>
            {
                new CanRemoveItemGuard()
            };
        }

        /// <summary>
        /// Creates a new RemoveItemCommand with custom guards.
        /// </summary>
        public RemoveItemCommand(Inventory.Core.Inventory inventory, string itemID, int quantity, List<ITransitionGuard> customGuards)
        {
            this.inventory = inventory;
            this.itemID = itemID;
            this.quantity = quantity;
            this.guards = customGuards ?? new List<ITransitionGuard>();
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (inventory == null) return false;
            if (string.IsNullOrEmpty(itemID)) return false;
            if (quantity <= 0) return false;

            // Check if inventory contains enough items
            if (!inventory.ContainsItem(itemID, quantity))
            {
                Debug.LogWarning($"Inventory does not contain {quantity}x {itemID}");
                return false;
            }

            // Evaluate all guards
            InventoryGuardContext context = InventoryGuardContext.ForRemoveItem(inventory, itemID, quantity);

            foreach (ITransitionGuard guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"RemoveItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("RemoveItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Snapshot items before removal
                removedItems.Clear();
                SaveRemovedItems();

                // Attempt to remove items
                bool success = inventory.TryRemoveItem(itemID, quantity, out actualRemovedQuantity);

                if (success && actualRemovedQuantity == quantity)
                {
                    isExecuted = true;
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to remove requested quantity. Removed: {actualRemovedQuantity}/{quantity}");

                    // If partial removal, restore what was removed
                    if (actualRemovedQuantity > 0)
                    {
                        RestoreRemovedItems();
                    }

                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"RemoveItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("RemoveItemCommand cannot undo - not executed or already undone");
                return false;
            }

            try
            {
                // Restore removed items
                RestoreRemovedItems();

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"RemoveItemCommand Undo failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves snapshots of items that will be removed.
        /// </summary>
        private void SaveRemovedItems()
        {
            removedItems.Clear();

            // Find all slots containing the item
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                InventorySlot slot = inventory.GetSlot(i);
                if (!slot.IsEmpty && slot.Stack.Type != null && slot.Stack.Type.ItemID == itemID)
                {
                    removedItems.Add(new SlotStateSnapshot(i, slot.Stack));
                }
            }
        }

        /// <summary>
        /// Restores previously removed items.
        /// </summary>
        private void RestoreRemovedItems()
        {
            foreach (SlotStateSnapshot snapshot in removedItems)
            {
                inventory.SetStack(snapshot.SlotIndex, snapshot.Stack, out _);
            }
        }

        /// <summary>
        /// Snapshot of a slot's state for undo purposes.
        /// </summary>
        private struct SlotStateSnapshot
        {
            public int SlotIndex;
            public ItemStack Stack;

            public SlotStateSnapshot(int slotIndex, ItemStack stack)
            {
                SlotIndex = slotIndex;
                Stack = stack;
            }
        }
    }

    /// <summary>
    /// Guard that validates if an item can be removed from inventory.
    /// </summary>
    public class CanRemoveItemGuard : GuardBase
    {
        public override string Name => "CanRemoveItem";
        public override string Description => "Validates if an item can be removed from the inventory";

        public override GuardResult Evaluate(GuardContext context)
        {
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            if (invContext.Inventory == null)
            {
                return Deny("Inventory is null");
            }

            if (invContext.Quantity <= 0)
            {
                return Deny("Quantity must be positive");
            }

            // Additional validation can be added here
            // For example: check if items are locked, equipped, etc.

            return Allow();
        }
    }
}
