using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for equipping items to an equipment inventory.
    /// Supports undo/redo and automatically handles unequipping replaced items.
    /// </summary>
    public class EquipItemCommand : CommandBase
    {
        private readonly EquipmentInventory equipmentInventory;
        private readonly Inventory.Core.Inventory sourceInventory;
        private readonly ItemStack itemToEquip;
        private readonly int sourceSlotIndex;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private List<ItemStack> unequippedItems = new List<ItemStack>();
        private EquipmentSlot equippedSlot = EquipmentSlot.None;

        public override string Description =>
            $"Equip {itemToEquip.Type?.Name ?? "Unknown"} to {equipmentInventory.OwnerID}";

        /// <summary>
        /// Creates a new EquipItemCommand.
        /// </summary>
        /// <param name="equipmentInventory">The equipment inventory to equip to</param>
        /// <param name="sourceInventory">The source inventory (can be null)</param>
        /// <param name="itemToEquip">The item to equip</param>
        /// <param name="sourceSlotIndex">The slot index in source inventory (if applicable)</param>
        /// <param name="playerLevel">Player level for requirement checks</param>
        /// <param name="playerClass">Player class for requirement checks</param>
        public EquipItemCommand(
            EquipmentInventory equipmentInventory,
            Inventory.Core.Inventory sourceInventory,
            ItemStack itemToEquip,
            int sourceSlotIndex = -1,
            int playerLevel = 999,
            string playerClass = "")
        {
            this.equipmentInventory = equipmentInventory;
            this.sourceInventory = sourceInventory;
            this.itemToEquip = itemToEquip;
            this.sourceSlotIndex = sourceSlotIndex;

            // Set up guards
            this.guards = new List<ITransitionGuard>
            {
                new CanEquipGuard(playerLevel, playerClass)
            };
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (equipmentInventory == null) return false;
            if (itemToEquip.IsEmpty) return false;

            // Evaluate all guards
            InventoryGuardContext context = new InventoryGuardContext(
                sourceInventory,
                itemToEquip,
                InventoryOperation.Equip);

            foreach (ITransitionGuard guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"EquipItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("EquipItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Remember which slot the item was equipped to
                EquipmentType equipment = itemToEquip.Type as EquipmentType;
                equippedSlot = equipment.EquipmentSlot;

                // Try to equip the item
                bool success = equipmentInventory.TryEquip(itemToEquip, out string reason, out unequippedItems);

                if (!success)
                {
                    Debug.LogError($"Failed to equip item: {reason}");
                    return false;
                }

                // If equipping from a source inventory, remove the item from source
                if (sourceInventory != null && sourceSlotIndex >= 0)
                {
                    sourceInventory.RemoveFromSlot(sourceSlotIndex, 1, out _);
                }

                // If items were unequipped, add them back to source inventory
                if (sourceInventory != null && unequippedItems.Count > 0)
                {
                    foreach (var unequipped in unequippedItems)
                    {
                        sourceInventory.TryAddItem(unequipped, out _);
                    }
                }

                isExecuted = true;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"EquipItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("EquipItemCommand cannot undo - not executed or already undone");
                return false;
            }

            try
            {
                // Unequip the item we equipped
                if (equipmentInventory.TryUnequip(equippedSlot, out ItemStack unequipped, out string reason))
                {
                    // Add the unequipped item back to source inventory
                    if (sourceInventory != null)
                    {
                        sourceInventory.TryAddItem(unequipped, out _);
                    }

                    // Re-equip the items that were unequipped
                    foreach (var item in unequippedItems)
                    {
                        equipmentInventory.TryEquip(item, out _, out _);

                        // Remove from source inventory if they were added there
                        if (sourceInventory != null)
                        {
                            sourceInventory.TryRemoveItem(item.Type.ItemID, 1, out _);
                        }
                    }

                    isExecuted = false;
                    return true;
                }
                else
                {
                    Debug.LogError($"Failed to undo equip: {reason}");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"EquipItemCommand Undo failed: {e.Message}");
                return false;
            }
        }
    }
}
