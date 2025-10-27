using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for adding items to an inventory.
    /// Supports undo/redo and integrates with guard validation.
    /// </summary>
    public class AddItemCommand : CommandBase
    {
        private readonly Inventory.Core.Inventory inventory;
        private readonly ItemStack itemStack;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private int addedQuantity = 0;
        private List<SlotStateSnapshot> changedSlots = new List<SlotStateSnapshot>();

        public override string Description =>
            $"Add {itemStack.Quantity}x {itemStack.Type?.Name ?? "Unknown"} to {inventory.InventoryID}";

        /// <summary>
        /// Creates a new AddItemCommand with default guards.
        /// </summary>
        public AddItemCommand(Inventory.Core.Inventory inventory, ItemStack itemStack)
        {
            this.inventory = inventory;
            this.itemStack = itemStack;

            // Set up default guards
            this.guards = new List<ITransitionGuard>
            {
                new CanAddItemGuard(),
                new HasSpaceGuard()
            };
        }

        /// <summary>
        /// Creates a new AddItemCommand with custom guards.
        /// </summary>
        public AddItemCommand(Inventory.Core.Inventory inventory, ItemStack itemStack, List<ITransitionGuard> customGuards)
        {
            this.inventory = inventory;
            this.itemStack = itemStack;
            this.guards = customGuards ?? new List<ITransitionGuard>();
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (inventory == null) return false;
            if (itemStack.IsEmpty) return false;

            // Evaluate all guards
            InventoryGuardContext context = InventoryGuardContext.ForAddItem(inventory, itemStack);

            foreach (ITransitionGuard guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"AddItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("AddItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Snapshot current state before making changes
                changedSlots.Clear();
                SaveInventoryState();

                // Attempt to add items
                bool success = inventory.TryAddItem(itemStack, out int remainingQuantity);

                if (success)
                {
                    addedQuantity = itemStack.Quantity - remainingQuantity;

                    if (remainingQuantity > 0)
                    {
                        Debug.LogWarning($"Could only add {addedQuantity}/{itemStack.Quantity} items. {remainingQuantity} remaining.");
                    }

                    isExecuted = true;
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to add any items to inventory");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"AddItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("AddItemCommand cannot undo - not executed or already undone");
                return false;
            }

            try
            {
                // Restore inventory to previous state
                RestoreInventoryState();

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"AddItemCommand Undo failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves the current state of slots that will be modified.
        /// </summary>
        private void SaveInventoryState()
        {
            changedSlots.Clear();

            // Snapshot all slots (we don't know which will be modified yet)
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                InventorySlot slot = inventory.GetSlot(i);
                if (!slot.IsEmpty || slot.IsLocked)
                {
                    changedSlots.Add(new SlotStateSnapshot(i, slot.Stack));
                }
            }

            // Also save empty slots that might be filled
            int emptySlotsSaved = 0;
            int maxEmptySlotsToSave = Mathf.CeilToInt((float)itemStack.Quantity / itemStack.Type.MaxStack);

            for (int i = 0; i < inventory.SlotCount && emptySlotsSaved < maxEmptySlotsToSave; i++)
            {
                InventorySlot slot = inventory.GetSlot(i);
                if (slot.IsEmpty && !slot.IsLocked)
                {
                    changedSlots.Add(new SlotStateSnapshot(i, ItemStack.Empty));
                    emptySlotsSaved++;
                }
            }
        }

        /// <summary>
        /// Restores the inventory to the saved state.
        /// </summary>
        private void RestoreInventoryState()
        {
            foreach (SlotStateSnapshot snapshot in changedSlots)
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
}
