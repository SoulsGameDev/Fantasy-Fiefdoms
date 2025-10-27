using System;
using System.Collections.Generic;
using Inventory.Data;
using Inventory.Effects;
using UnityEngine;

namespace Inventory.Core
{
    /// <summary>
    /// Specialized inventory for equipment with fixed slots.
    /// Each slot corresponds to a specific equipment slot (Head, Chest, etc.).
    /// Automatically applies/removes effects when items are equipped/unequipped.
    /// </summary>
    [Serializable]
    public class EquipmentInventory
    {
        [SerializeField] private string ownerID;
        [SerializeField] private GameObject ownerObject;

        // Dictionary mapping equipment slots to their current items
        private Dictionary<EquipmentSlot, ItemStack> equippedItems;

        #region Events

        /// <summary>Fired when an item is equipped</summary>
        public event Action<EquipmentSlot, ItemStack> OnItemEquipped;

        /// <summary>Fired when an item is unequipped</summary>
        public event Action<EquipmentSlot, ItemStack> OnItemUnequipped;

        /// <summary>Fired when equipment changes</summary>
        public event Action OnEquipmentChanged;

        #endregion

        #region Properties

        /// <summary>Owner identifier</summary>
        public string OwnerID => ownerID;

        /// <summary>Owner GameObject (for applying effects)</summary>
        public GameObject OwnerObject => ownerObject;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new equipment inventory.
        /// </summary>
        /// <param name="ownerID">Identifier for the owner</param>
        /// <param name="ownerObject">GameObject to apply effects to</param>
        public EquipmentInventory(string ownerID, GameObject ownerObject = null)
        {
            this.ownerID = ownerID;
            this.ownerObject = ownerObject;
            this.equippedItems = new Dictionary<EquipmentSlot, ItemStack>();

            // Initialize all slots as empty
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot != EquipmentSlot.None)
                {
                    equippedItems[slot] = ItemStack.Empty;
                }
            }
        }

        #endregion

        #region Equipment Operations

        /// <summary>
        /// Attempts to equip an item in the appropriate slot.
        /// </summary>
        /// <param name="itemStack">The item to equip</param>
        /// <param name="reason">Reason if equip fails</param>
        /// <param name="unequippedItems">Items that were unequipped to make room</param>
        /// <returns>True if successfully equipped</returns>
        public bool TryEquip(ItemStack itemStack, out string reason, out List<ItemStack> unequippedItems)
        {
            unequippedItems = new List<ItemStack>();
            reason = string.Empty;

            // Validate item
            if (itemStack.IsEmpty || itemStack.Type == null)
            {
                reason = "Invalid item stack";
                return false;
            }

            // Must be equipment
            if (!(itemStack.Type is EquipmentType equipment))
            {
                reason = "Item is not equipment";
                return false;
            }

            // Check if slot is valid
            EquipmentSlot targetSlot = equipment.EquipmentSlot;
            if (targetSlot == EquipmentSlot.None)
            {
                reason = "Equipment has no valid slot";
                return false;
            }

            // Handle two-handed weapons
            if (equipment.IsTwoHanded)
            {
                // Unequip both hands
                if (!equippedItems[EquipmentSlot.MainHand].IsEmpty)
                {
                    unequippedItems.Add(equippedItems[EquipmentSlot.MainHand]);
                    UnequipInternal(EquipmentSlot.MainHand);
                }
                if (!equippedItems[EquipmentSlot.OffHand].IsEmpty)
                {
                    unequippedItems.Add(equippedItems[EquipmentSlot.OffHand]);
                    UnequipInternal(EquipmentSlot.OffHand);
                }
            }
            else if (targetSlot == EquipmentSlot.MainHand || targetSlot == EquipmentSlot.OffHand)
            {
                // If equipping one-handed item but a two-handed item is equipped
                if (!equippedItems[EquipmentSlot.MainHand].IsEmpty)
                {
                    var mainHandItem = equippedItems[EquipmentSlot.MainHand].Type as EquipmentType;
                    if (mainHandItem != null && mainHandItem.IsTwoHanded)
                    {
                        unequippedItems.Add(equippedItems[EquipmentSlot.MainHand]);
                        UnequipInternal(EquipmentSlot.MainHand);
                        UnequipInternal(EquipmentSlot.OffHand);
                    }
                }
            }

            // Unequip current item in slot if any
            if (!equippedItems[targetSlot].IsEmpty)
            {
                unequippedItems.Add(equippedItems[targetSlot]);
                UnequipInternal(targetSlot);
            }

            // Equip the new item
            equippedItems[targetSlot] = itemStack;

            // For two-handed weapons, mark off-hand as occupied
            if (equipment.IsTwoHanded && targetSlot == EquipmentSlot.MainHand)
            {
                equippedItems[EquipmentSlot.OffHand] = itemStack; // Reference same item
            }

            // Apply effects
            if (ownerObject != null && equipment.HasEffects)
            {
                ItemEffectFactory.ApplyEquipmentEffects(ownerObject, equipment);
            }

            // Fire events
            OnItemEquipped?.Invoke(targetSlot, itemStack);
            OnEquipmentChanged?.Invoke();

            return true;
        }

        /// <summary>
        /// Attempts to unequip an item from a slot.
        /// </summary>
        /// <param name="slot">The slot to unequip from</param>
        /// <param name="unequippedItem">The item that was unequipped</param>
        /// <param name="reason">Reason if unequip fails</param>
        /// <returns>True if successfully unequipped</returns>
        public bool TryUnequip(EquipmentSlot slot, out ItemStack unequippedItem, out string reason)
        {
            reason = string.Empty;
            unequippedItem = ItemStack.Empty;

            // Check if slot has an item
            if (!equippedItems.ContainsKey(slot) || equippedItems[slot].IsEmpty)
            {
                reason = "Slot is empty";
                return false;
            }

            unequippedItem = equippedItems[slot];
            UnequipInternal(slot);

            return true;
        }

        /// <summary>
        /// Internal method to unequip an item without validation.
        /// </summary>
        private void UnequipInternal(EquipmentSlot slot)
        {
            if (!equippedItems.ContainsKey(slot) || equippedItems[slot].IsEmpty)
                return;

            ItemStack item = equippedItems[slot];

            // Remove effects
            if (ownerObject != null && item.Type is EquipmentType equipment && equipment.HasEffects)
            {
                ItemEffectFactory.RemoveEquipmentEffects(ownerObject, equipment);
            }

            // Clear slot
            ItemStack removedItem = equippedItems[slot];
            equippedItems[slot] = ItemStack.Empty;

            // Fire events
            OnItemUnequipped?.Invoke(slot, removedItem);
            OnEquipmentChanged?.Invoke();
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Gets the item equipped in a specific slot.
        /// </summary>
        public ItemStack GetEquippedItem(EquipmentSlot slot)
        {
            if (equippedItems.ContainsKey(slot))
            {
                return equippedItems[slot];
            }
            return ItemStack.Empty;
        }

        /// <summary>
        /// Checks if a slot is empty.
        /// </summary>
        public bool IsSlotEmpty(EquipmentSlot slot)
        {
            return !equippedItems.ContainsKey(slot) || equippedItems[slot].IsEmpty;
        }

        /// <summary>
        /// Checks if a specific item is currently equipped.
        /// </summary>
        public bool IsEquipped(string itemID)
        {
            foreach (var kvp in equippedItems)
            {
                if (!kvp.Value.IsEmpty && kvp.Value.Type != null && kvp.Value.Type.ItemID == itemID)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Gets all equipped items.
        /// </summary>
        public List<ItemStack> GetAllEquippedItems()
        {
            List<ItemStack> items = new List<ItemStack>();
            HashSet<string> addedTwoHanded = new HashSet<string>(); // Track two-handed items

            foreach (var kvp in equippedItems)
            {
                if (!kvp.Value.IsEmpty)
                {
                    // For two-handed weapons, only add once
                    if (kvp.Value.Type is EquipmentType eq && eq.IsTwoHanded)
                    {
                        if (!addedTwoHanded.Contains(kvp.Value.Type.ItemID))
                        {
                            items.Add(kvp.Value);
                            addedTwoHanded.Add(kvp.Value.Type.ItemID);
                        }
                    }
                    else
                    {
                        items.Add(kvp.Value);
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Gets the total armor value from all equipped items.
        /// </summary>
        public int GetTotalArmor()
        {
            int total = 0;
            foreach (var item in GetAllEquippedItems())
            {
                if (item.Type is EquipmentType equipment)
                {
                    total += equipment.Armor;
                }
            }
            return total;
        }

        /// <summary>
        /// Gets the total damage from equipped weapons.
        /// </summary>
        public int GetTotalDamage()
        {
            int total = 0;
            foreach (var item in GetAllEquippedItems())
            {
                if (item.Type is EquipmentType equipment)
                {
                    total += equipment.Damage;
                }
            }
            return total;
        }

        /// <summary>
        /// Gets a summary of all equipment bonuses.
        /// </summary>
        public string GetEquipmentSummary()
        {
            List<string> summary = new List<string>();

            summary.Add($"Total Armor: {GetTotalArmor()}");
            summary.Add($"Total Damage: {GetTotalDamage()}");

            int equippedCount = GetAllEquippedItems().Count;
            int totalSlots = equippedItems.Count;
            summary.Add($"Equipped: {equippedCount}/{totalSlots} slots");

            return string.Join("\n", summary);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Unequips all items.
        /// </summary>
        /// <returns>List of all unequipped items</returns>
        public List<ItemStack> UnequipAll()
        {
            List<ItemStack> unequippedItems = new List<ItemStack>();

            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot != EquipmentSlot.None && !IsSlotEmpty(slot))
                {
                    if (TryUnequip(slot, out ItemStack item, out _))
                    {
                        unequippedItems.Add(item);
                    }
                }
            }

            return unequippedItems;
        }

        /// <summary>
        /// Sets the owner GameObject (for applying effects).
        /// </summary>
        public void SetOwner(GameObject newOwner)
        {
            // Remove effects from old owner
            if (ownerObject != null)
            {
                foreach (var item in GetAllEquippedItems())
                {
                    if (item.Type is EquipmentType equipment && equipment.HasEffects)
                    {
                        ItemEffectFactory.RemoveEquipmentEffects(ownerObject, equipment);
                    }
                }
            }

            // Set new owner
            ownerObject = newOwner;

            // Apply effects to new owner
            if (ownerObject != null)
            {
                foreach (var item in GetAllEquippedItems())
                {
                    if (item.Type is EquipmentType equipment && equipment.HasEffects)
                    {
                        ItemEffectFactory.ApplyEquipmentEffects(ownerObject, equipment);
                    }
                }
            }
        }

        #endregion
    }
}
