using Inventory.Core;
using Inventory.Data;

namespace Inventory.Guards
{
    /// <summary>
    /// Context information for inventory guard evaluations.
    /// Extends the base GuardContext pattern for inventory-specific operations.
    /// </summary>
    public class InventoryGuardContext : GuardContext
    {
        /// <summary>The inventory being operated on</summary>
        public Inventory.Core.Inventory Inventory { get; set; }

        /// <summary>The item stack involved in the operation</summary>
        public ItemStack ItemStack { get; set; }

        /// <summary>The slot index involved (if applicable)</summary>
        public int SlotIndex { get; set; } = -1;

        /// <summary>The quantity involved in the operation</summary>
        public int Quantity { get; set; }

        /// <summary>Secondary inventory for transfer operations</summary>
        public Inventory.Core.Inventory TargetInventory { get; set; }

        /// <summary>Secondary slot index for move operations</summary>
        public int TargetSlotIndex { get; set; } = -1;

        /// <summary>Operation type for context-aware validation</summary>
        public InventoryOperation Operation { get; set; }

        /// <summary>
        /// Creates a basic inventory guard context.
        /// </summary>
        public InventoryGuardContext(Inventory.Core.Inventory inventory, ItemStack itemStack, InventoryOperation operation)
            : base(null, default, default, default)
        {
            Inventory = inventory;
            ItemStack = itemStack;
            Operation = operation;
            Quantity = itemStack.Quantity;
        }

        /// <summary>
        /// Creates a guard context for add item operation.
        /// </summary>
        public static InventoryGuardContext ForAddItem(Inventory.Core.Inventory inventory, ItemStack itemStack)
        {
            return new InventoryGuardContext(inventory, itemStack, InventoryOperation.Add);
        }

        /// <summary>
        /// Creates a guard context for remove item operation.
        /// </summary>
        public static InventoryGuardContext ForRemoveItem(Inventory.Core.Inventory inventory, string itemID, int quantity)
        {
            return new InventoryGuardContext(inventory, ItemStack.Empty, InventoryOperation.Remove)
            {
                Quantity = quantity
            };
        }

        /// <summary>
        /// Creates a guard context for move item operation.
        /// </summary>
        public static InventoryGuardContext ForMoveItem(
            Inventory.Core.Inventory fromInventory,
            int fromSlot,
            Inventory.Core.Inventory toInventory,
            int toSlot)
        {
            return new InventoryGuardContext(fromInventory, fromInventory.GetStack(fromSlot), InventoryOperation.Move)
            {
                SlotIndex = fromSlot,
                TargetInventory = toInventory,
                TargetSlotIndex = toSlot
            };
        }
    }

    /// <summary>
    /// Defines the types of inventory operations that can be guarded.
    /// </summary>
    public enum InventoryOperation
    {
        Add,
        Remove,
        Move,
        Equip,
        Unequip,
        Use,
        Drop,
        Sell,
        Buy
    }
}
