using System;
using Inventory.Data;

namespace Inventory.Core
{
    /// <summary>
    /// Represents a single slot in an inventory.
    /// Contains an item stack and optional restrictions/locks.
    /// </summary>
    [Serializable]
    public struct InventorySlot
    {
        /// <summary>The item stack contained in this slot</summary>
        public ItemStack Stack;

        /// <summary>Restriction on what types of items can be placed here</summary>
        public SlotRestriction Restriction;

        /// <summary>Whether this slot is locked (cannot add/remove items)</summary>
        public bool IsLocked;

        /// <summary>
        /// Creates a new inventory slot with optional restrictions.
        /// </summary>
        /// <param name="restriction">The slot restriction type</param>
        /// <param name="isLocked">Whether the slot starts locked</param>
        public InventorySlot(SlotRestriction restriction = SlotRestriction.None, bool isLocked = false)
        {
            Stack = ItemStack.Empty;
            Restriction = restriction;
            IsLocked = isLocked;
        }

        /// <summary>
        /// Creates a new inventory slot with an initial item stack.
        /// </summary>
        /// <param name="stack">The initial item stack</param>
        /// <param name="restriction">The slot restriction type</param>
        /// <param name="isLocked">Whether the slot starts locked</param>
        public InventorySlot(ItemStack stack, SlotRestriction restriction = SlotRestriction.None, bool isLocked = false)
        {
            Stack = stack;
            Restriction = restriction;
            IsLocked = isLocked;
        }

        /// <summary>
        /// Gets whether this slot is empty (no items).
        /// </summary>
        public bool IsEmpty => Stack.IsEmpty;

        /// <summary>
        /// Gets whether this slot has items.
        /// </summary>
        public bool HasItems => !Stack.IsEmpty;

        /// <summary>
        /// Gets whether this slot can be modified (not locked).
        /// </summary>
        public bool CanModify => !IsLocked;

        /// <summary>
        /// Checks if an item can be placed in this slot based on restrictions.
        /// </summary>
        /// <param name="itemStack">The item stack to check</param>
        /// <returns>True if the item meets the slot's restrictions</returns>
        public bool MeetsRestriction(ItemStack itemStack)
        {
            if (itemStack.IsEmpty)
                return false;

            return Restriction switch
            {
                SlotRestriction.None => true,
                SlotRestriction.QuestOnly => itemStack.Type.Category == ItemCategory.Quest,
                SlotRestriction.EquipmentOnly => itemStack.Type.Category == ItemCategory.Equipment,
                SlotRestriction.ConsumableOnly => itemStack.Type.Category == ItemCategory.Consumable,
                SlotRestriction.MaterialOnly => itemStack.Type.Category == ItemCategory.Material,
                SlotRestriction.CurrencyOnly => itemStack.Type.Category == ItemCategory.Currency,
                _ => false
            };
        }

        /// <summary>
        /// Checks if an item can be added to this slot.
        /// </summary>
        /// <param name="itemStack">The item stack to add</param>
        /// <param name="reason">The reason if it cannot be added</param>
        /// <returns>True if the item can be added</returns>
        public bool CanAddItem(ItemStack itemStack, out string reason)
        {
            if (IsLocked)
            {
                reason = "Slot is locked";
                return false;
            }

            if (itemStack.IsEmpty)
            {
                reason = "Item stack is empty";
                return false;
            }

            if (!MeetsRestriction(itemStack))
            {
                reason = $"Item does not meet slot restriction: {Restriction}";
                return false;
            }

            // If slot is empty, can always add
            if (IsEmpty)
            {
                reason = string.Empty;
                return true;
            }

            // If slot has items, check if we can stack
            if (!Stack.CanMergeWith(itemStack))
            {
                reason = "Cannot merge with existing stack";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Attempts to set the item stack in this slot, replacing any existing items.
        /// </summary>
        /// <param name="itemStack">The new item stack</param>
        /// <param name="reason">The reason if it fails</param>
        /// <returns>True if successful</returns>
        public bool TrySetStack(ItemStack itemStack, out string reason)
        {
            if (IsLocked)
            {
                reason = "Slot is locked";
                return false;
            }

            if (!itemStack.IsEmpty && !MeetsRestriction(itemStack))
            {
                reason = $"Item does not meet slot restriction: {Restriction}";
                return false;
            }

            Stack = itemStack;
            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Clears the slot (removes all items).
        /// </summary>
        /// <param name="reason">The reason if it fails</param>
        /// <returns>True if successful</returns>
        public bool TryClear(out string reason)
        {
            if (IsLocked)
            {
                reason = "Slot is locked";
                return false;
            }

            Stack = ItemStack.Empty;
            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Locks the slot, preventing modifications.
        /// </summary>
        public void Lock()
        {
            IsLocked = true;
        }

        /// <summary>
        /// Unlocks the slot, allowing modifications.
        /// </summary>
        public void Unlock()
        {
            IsLocked = false;
        }

        /// <summary>
        /// Creates an empty slot with no restrictions.
        /// </summary>
        public static InventorySlot Empty => new InventorySlot(SlotRestriction.None, false);

        public override string ToString()
        {
            string lockStatus = IsLocked ? " [LOCKED]" : "";
            string restrictionInfo = Restriction != SlotRestriction.None ? $" ({Restriction})" : "";

            if (IsEmpty)
                return $"Empty Slot{restrictionInfo}{lockStatus}";

            return $"{Stack}{restrictionInfo}{lockStatus}";
        }
    }
}
