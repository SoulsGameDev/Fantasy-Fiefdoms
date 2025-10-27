using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for moving items between inventory slots or between inventories.
    /// Supports undo/redo and handles item swapping.
    /// </summary>
    public class MoveItemCommand : CommandBase
    {
        private readonly Inventory.Core.Inventory sourceInventory;
        private readonly int sourceSlotIndex;
        private readonly Inventory.Core.Inventory targetInventory;
        private readonly int targetSlotIndex;
        private readonly int quantity;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private ItemStack sourceOriginal;
        private ItemStack targetOriginal;
        private ItemStack sourceAfter;
        private ItemStack targetAfter;

        public override string Description
        {
            get
            {
                if (sourceInventory == targetInventory)
                {
                    return $"Move item from slot {sourceSlotIndex} to slot {targetSlotIndex}";
                }
                else
                {
                    return $"Move item from {sourceInventory.InventoryID} to {targetInventory.InventoryID}";
                }
            }
        }

        /// <summary>
        /// Creates a new MoveItemCommand.
        /// </summary>
        /// <param name="sourceInventory">Source inventory</param>
        /// <param name="sourceSlotIndex">Source slot index</param>
        /// <param name="targetInventory">Target inventory</param>
        /// <param name="targetSlotIndex">Target slot index</param>
        /// <param name="quantity">Quantity to move (-1 for all)</param>
        public MoveItemCommand(
            Inventory.Core.Inventory sourceInventory,
            int sourceSlotIndex,
            Inventory.Core.Inventory targetInventory,
            int targetSlotIndex,
            int quantity = -1)
        {
            this.sourceInventory = sourceInventory;
            this.sourceSlotIndex = sourceSlotIndex;
            this.targetInventory = targetInventory;
            this.targetSlotIndex = targetSlotIndex;
            this.quantity = quantity;

            // Set up guards
            this.guards = new List<ITransitionGuard>();
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (sourceInventory == null || targetInventory == null) return false;

            // Can't move to the same slot
            if (sourceInventory == targetInventory && sourceSlotIndex == targetSlotIndex)
            {
                Debug.LogWarning("Cannot move item to the same slot");
                return false;
            }

            // Check if source slot has an item
            ItemStack sourceStack = sourceInventory.GetStack(sourceSlotIndex);
            if (sourceStack.IsEmpty)
            {
                Debug.LogWarning("Source slot is empty");
                return false;
            }

            // Validate quantity
            int moveQuantity = quantity <= 0 ? sourceStack.Quantity : quantity;
            if (moveQuantity > sourceStack.Quantity)
            {
                Debug.LogWarning($"Cannot move {moveQuantity} items - only {sourceStack.Quantity} available");
                return false;
            }

            // Evaluate guards
            InventoryGuardContext context = InventoryGuardContext.ForMoveItem(
                sourceInventory,
                sourceSlotIndex,
                targetInventory,
                targetSlotIndex);

            foreach (ITransitionGuard guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"MoveItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("MoveItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Save original states
                sourceOriginal = sourceInventory.GetStack(sourceSlotIndex);
                targetOriginal = targetInventory.GetStack(targetSlotIndex);

                int moveQuantity = quantity <= 0 ? sourceOriginal.Quantity : quantity;

                // Case 1: Target slot is empty - simple move
                if (targetOriginal.IsEmpty)
                {
                    ExecuteSimpleMove(moveQuantity);
                }
                // Case 2: Same item type - try to stack
                else if (sourceOriginal.Type == targetOriginal.Type && targetOriginal.Type.IsStackable)
                {
                    ExecuteStackMove(moveQuantity);
                }
                // Case 3: Different items - swap
                else
                {
                    ExecuteSwap(moveQuantity);
                }

                // Save final states
                sourceAfter = sourceInventory.GetStack(sourceSlotIndex);
                targetAfter = targetInventory.GetStack(targetSlotIndex);

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"MoveItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("MoveItemCommand cannot undo - not executed or already undone");
                return false;
            }

            try
            {
                // Restore original states
                sourceInventory.SetStack(sourceSlotIndex, sourceOriginal, out _);
                targetInventory.SetStack(targetSlotIndex, targetOriginal, out _);

                isExecuted = false;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"MoveItemCommand Undo failed: {e.Message}");
                return false;
            }
        }

        #region Move Strategies

        /// <summary>
        /// Simple move to empty slot.
        /// </summary>
        private void ExecuteSimpleMove(int moveQuantity)
        {
            ItemStack sourceStack = sourceOriginal;

            if (moveQuantity >= sourceStack.Quantity)
            {
                // Move entire stack
                targetInventory.SetStack(targetSlotIndex, sourceStack, out _);
                sourceInventory.ClearSlot(sourceSlotIndex, out _);
            }
            else
            {
                // Split stack
                sourceStack.TrySplit(moveQuantity, out ItemStack splitStack);
                targetInventory.SetStack(targetSlotIndex, splitStack, out _);
                sourceInventory.SetStack(sourceSlotIndex, sourceStack, out _);
            }
        }

        /// <summary>
        /// Move and stack with existing items.
        /// </summary>
        private void ExecuteStackMove(int moveQuantity)
        {
            ItemStack sourceStack = sourceOriginal;
            ItemStack targetStack = targetOriginal;

            // Try to add to target stack
            targetStack.TryAddToStack(moveQuantity, out int remaining);

            // Update target
            targetInventory.SetStack(targetSlotIndex, targetStack, out _);

            // Update source
            if (remaining == 0)
            {
                // All moved
                sourceStack.TryRemoveFromStack(moveQuantity, out _);
                if (sourceStack.Quantity <= 0)
                {
                    sourceInventory.ClearSlot(sourceSlotIndex, out _);
                }
                else
                {
                    sourceInventory.SetStack(sourceSlotIndex, sourceStack, out _);
                }
            }
            else
            {
                // Partial move
                int actualMoved = moveQuantity - remaining;
                sourceStack.TryRemoveFromStack(actualMoved, out _);
                sourceInventory.SetStack(sourceSlotIndex, sourceStack, out _);
            }
        }

        /// <summary>
        /// Swap items between slots.
        /// </summary>
        private void ExecuteSwap(int moveQuantity)
        {
            ItemStack sourceStack = sourceOriginal;
            ItemStack targetStack = targetOriginal;

            if (moveQuantity >= sourceStack.Quantity)
            {
                // Full swap
                sourceInventory.SetStack(sourceSlotIndex, targetStack, out _);
                targetInventory.SetStack(targetSlotIndex, sourceStack, out _);
            }
            else
            {
                // Can't partially swap different items
                Debug.LogWarning("Cannot partially swap different item types. Moving full stack.");
                sourceInventory.SetStack(sourceSlotIndex, targetStack, out _);
                targetInventory.SetStack(targetSlotIndex, sourceStack, out _);
            }
        }

        #endregion
    }
}
