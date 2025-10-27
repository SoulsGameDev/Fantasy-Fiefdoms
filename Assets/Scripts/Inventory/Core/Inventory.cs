using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Data;
using UnityEngine;

namespace Inventory.Core
{
    /// <summary>
    /// Base class for all inventory containers.
    /// Handles slot-based storage with events for UI updates.
    /// Optimized with Dictionary lookup for O(1) item finding.
    /// </summary>
    [Serializable]
    public class Inventory
    {
        #region Fields

        [SerializeField] private string inventoryID;
        [SerializeField] private int maxSlots;
        [SerializeField] private float maxWeight;
        [SerializeField] private InventorySlot[] slots;

        // Quick lookup: ItemID -> List of slot indices containing that item
        [NonSerialized] private Dictionary<string, List<int>> itemLocationCache;
        [NonSerialized] private bool cacheInitialized = false;

        #endregion

        #region Properties

        /// <summary>Unique identifier for this inventory</summary>
        public string InventoryID => inventoryID;

        /// <summary>Maximum number of slots (-1 for unlimited)</summary>
        public int MaxSlots => maxSlots;

        /// <summary>Maximum weight capacity (-1 for unlimited)</summary>
        public float MaxWeight => maxWeight;

        /// <summary>Current number of slots</summary>
        public int SlotCount => slots?.Length ?? 0;

        /// <summary>Gets whether weight limiting is enabled</summary>
        public bool HasWeightLimit => maxWeight > 0;

        #endregion

        #region Events

        /// <summary>Fired when a specific slot changes</summary>
        public event Action<int> OnSlotChanged;

        /// <summary>Fired when an item is added</summary>
        public event Action<ItemStack> OnItemAdded;

        /// <summary>Fired when an item is removed</summary>
        public event Action<ItemStack> OnItemRemoved;

        /// <summary>Fired when the inventory is cleared</summary>
        public event Action OnInventoryCleared;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new inventory with the specified configuration.
        /// </summary>
        /// <param name="inventoryID">Unique identifier</param>
        /// <param name="maxSlots">Maximum number of slots</param>
        /// <param name="maxWeight">Maximum weight (-1 for unlimited)</param>
        public Inventory(string inventoryID, int maxSlots, float maxWeight = -1f)
        {
            this.inventoryID = inventoryID;
            this.maxSlots = maxSlots;
            this.maxWeight = maxWeight;

            // Initialize slots
            slots = new InventorySlot[maxSlots];
            for (int i = 0; i < maxSlots; i++)
            {
                slots[i] = InventorySlot.Empty;
            }

            InitializeCache();
        }

        /// <summary>
        /// Initializes the item location cache for fast lookups.
        /// </summary>
        private void InitializeCache()
        {
            itemLocationCache = new Dictionary<string, List<int>>();
            cacheInitialized = true;

            // Build cache from existing slots
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    AddToCache(slots[i].Stack.Type.ItemID, i);
                }
            }
        }

        /// <summary>
        /// Ensures the cache is initialized (lazy initialization support).
        /// </summary>
        private void EnsureCacheInitialized()
        {
            if (!cacheInitialized)
            {
                InitializeCache();
            }
        }

        #endregion

        #region Slot Access

        /// <summary>
        /// Gets the slot at the specified index.
        /// </summary>
        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                Debug.LogWarning($"Invalid slot index: {index}");
                return InventorySlot.Empty;
            }

            return slots[index];
        }

        /// <summary>
        /// Gets the item stack at the specified index.
        /// </summary>
        public ItemStack GetStack(int index)
        {
            return GetSlot(index).Stack;
        }

        /// <summary>
        /// Sets the item stack at the specified index.
        /// </summary>
        public bool SetStack(int index, ItemStack stack, out string reason)
        {
            if (index < 0 || index >= slots.Length)
            {
                reason = "Invalid slot index";
                return false;
            }

            // Remove old item from cache
            if (!slots[index].IsEmpty)
            {
                RemoveFromCache(slots[index].Stack.Type.ItemID, index);
            }

            // Set new stack
            if (!slots[index].TrySetStack(stack, out reason))
            {
                return false;
            }

            // Add new item to cache
            if (!stack.IsEmpty)
            {
                AddToCache(stack.Type.ItemID, index);
            }

            OnSlotChanged?.Invoke(index);
            return true;
        }

        /// <summary>
        /// Clears the slot at the specified index.
        /// </summary>
        public bool ClearSlot(int index, out string reason)
        {
            if (index < 0 || index >= slots.Length)
            {
                reason = "Invalid slot index";
                return false;
            }

            // Remove from cache
            if (!slots[index].IsEmpty)
            {
                RemoveFromCache(slots[index].Stack.Type.ItemID, index);
            }

            if (!slots[index].TryClear(out reason))
            {
                return false;
            }

            OnSlotChanged?.Invoke(index);
            return true;
        }

        #endregion

        #region Item Operations

        /// <summary>
        /// Attempts to add an item stack to the inventory.
        /// Will merge with existing stacks if possible, then fill empty slots.
        /// </summary>
        /// <param name="stack">The item stack to add</param>
        /// <param name="remainingQuantity">The quantity that couldn't be added</param>
        /// <returns>True if at least some items were added</returns>
        public bool TryAddItem(ItemStack stack, out int remainingQuantity)
        {
            EnsureCacheInitialized();

            if (stack.IsEmpty || stack.Quantity <= 0)
            {
                remainingQuantity = 0;
                return false;
            }

            remainingQuantity = stack.Quantity;
            bool addedAny = false;

            // Step 1: Try to merge with existing stacks
            if (stack.Type.IsStackable)
            {
                List<int> existingSlots = FindSlotsWithItem(stack.Type.ItemID);
                foreach (int slotIndex in existingSlots)
                {
                    if (remainingQuantity <= 0) break;

                    ref InventorySlot slot = ref slots[slotIndex];
                    if (slot.IsLocked || !slot.Stack.CanStack) continue;

                    int before = remainingQuantity;
                    slot.Stack.TryAddToStack(remainingQuantity, out remainingQuantity);

                    if (before != remainingQuantity)
                    {
                        addedAny = true;
                        OnSlotChanged?.Invoke(slotIndex);
                    }
                }
            }

            // Step 2: Fill empty slots
            if (remainingQuantity > 0)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (remainingQuantity <= 0) break;

                    if (slots[i].IsEmpty && !slots[i].IsLocked)
                    {
                        int quantityToAdd = Mathf.Min(remainingQuantity, stack.Type.MaxStack);
                        ItemStack newStack = new ItemStack(stack.Type, quantityToAdd, stack.Durability);

                        if (slots[i].TrySetStack(newStack, out _))
                        {
                            AddToCache(stack.Type.ItemID, i);
                            remainingQuantity -= quantityToAdd;
                            addedAny = true;
                            OnSlotChanged?.Invoke(i);
                        }
                    }
                }
            }

            if (addedAny)
            {
                int addedAmount = stack.Quantity - remainingQuantity;
                OnItemAdded?.Invoke(new ItemStack(stack.Type, addedAmount, stack.Durability));
            }

            return addedAny;
        }

        /// <summary>
        /// Attempts to remove a specific quantity of an item type from the inventory.
        /// </summary>
        /// <param name="itemID">The item ID to remove</param>
        /// <param name="quantity">The quantity to remove</param>
        /// <param name="removedQuantity">The actual quantity removed</param>
        /// <returns>True if the full quantity was removed</returns>
        public bool TryRemoveItem(string itemID, int quantity, out int removedQuantity)
        {
            EnsureCacheInitialized();

            removedQuantity = 0;
            int remaining = quantity;

            List<int> slotsWithItem = FindSlotsWithItem(itemID);

            foreach (int slotIndex in slotsWithItem)
            {
                if (remaining <= 0) break;

                ref InventorySlot slot = ref slots[slotIndex];
                if (slot.IsLocked) continue;

                int removeAmount = Mathf.Min(remaining, slot.Stack.Quantity);
                slot.Stack.TryRemoveFromStack(removeAmount, out int actualRemoved);

                remaining -= actualRemoved;
                removedQuantity += actualRemoved;

                if (slot.Stack.Quantity <= 0)
                {
                    RemoveFromCache(itemID, slotIndex);
                    slot.TryClear(out _);
                }

                OnSlotChanged?.Invoke(slotIndex);
            }

            if (removedQuantity > 0)
            {
                // Create a dummy stack for the event (we don't have the ItemType reference here)
                // The caller should handle this properly
                OnItemRemoved?.Invoke(new ItemStack(null, removedQuantity));
            }

            return removedQuantity == quantity;
        }

        /// <summary>
        /// Removes an item from a specific slot.
        /// </summary>
        public bool RemoveFromSlot(int slotIndex, int quantity, out int removedQuantity)
        {
            removedQuantity = 0;

            if (slotIndex < 0 || slotIndex >= slots.Length)
                return false;

            ref InventorySlot slot = ref slots[slotIndex];
            if (slot.IsEmpty || slot.IsLocked)
                return false;

            string itemID = slot.Stack.Type.ItemID;
            int removeAmount = Mathf.Min(quantity, slot.Stack.Quantity);

            slot.Stack.TryRemoveFromStack(removeAmount, out removedQuantity);

            if (slot.Stack.Quantity <= 0)
            {
                RemoveFromCache(itemID, slotIndex);
                slot.TryClear(out _);
            }

            OnSlotChanged?.Invoke(slotIndex);

            return true;
        }

        #endregion

        #region Query Operations

        /// <summary>
        /// Checks if the inventory contains at least the specified quantity of an item.
        /// </summary>
        public bool ContainsItem(string itemID, int quantity = 1)
        {
            return GetItemQuantity(itemID) >= quantity;
        }

        /// <summary>
        /// Gets the total quantity of an item type in the inventory.
        /// </summary>
        public int GetItemQuantity(string itemID)
        {
            EnsureCacheInitialized();

            List<int> slotsWithItem = FindSlotsWithItem(itemID);
            int total = 0;

            foreach (int slotIndex in slotsWithItem)
            {
                total += slots[slotIndex].Stack.Quantity;
            }

            return total;
        }

        /// <summary>
        /// Gets the number of empty slots.
        /// </summary>
        public int GetEmptySlotCount()
        {
            int count = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty && !slots[i].IsLocked)
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Gets the total weight of all items in the inventory.
        /// </summary>
        public float GetTotalWeight()
        {
            float total = 0f;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    total += slots[i].Stack.TotalWeight;
                }
            }
            return total;
        }

        /// <summary>
        /// Gets the total value of all items in the inventory.
        /// </summary>
        public int GetTotalValue()
        {
            int total = 0;
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    total += slots[i].Stack.TotalValue;
                }
            }
            return total;
        }

        /// <summary>
        /// Checks if there's enough space to add an item.
        /// </summary>
        public bool HasSpaceForItem(ItemStack stack)
        {
            if (stack.IsEmpty) return false;

            // Check weight limit
            if (HasWeightLimit)
            {
                float currentWeight = GetTotalWeight();
                float newWeight = stack.TotalWeight;
                if (currentWeight + newWeight > maxWeight)
                    return false;
            }

            // Check if we can stack with existing items
            if (stack.Type.IsStackable)
            {
                List<int> existingSlots = FindSlotsWithItem(stack.Type.ItemID);
                foreach (int slotIndex in existingSlots)
                {
                    if (slots[slotIndex].Stack.CanStack)
                        return true; // Can merge with existing
                }
            }

            // Check for empty slots
            return GetEmptySlotCount() > 0;
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// Finds all slot indices containing a specific item.
        /// Uses cached Dictionary for O(1) lookup performance.
        /// </summary>
        private List<int> FindSlotsWithItem(string itemID)
        {
            EnsureCacheInitialized();

            if (itemLocationCache.TryGetValue(itemID, out List<int> indices))
            {
                return indices;
            }

            return new List<int>();
        }

        /// <summary>
        /// Adds a slot index to the cache for an item.
        /// </summary>
        private void AddToCache(string itemID, int slotIndex)
        {
            EnsureCacheInitialized();

            if (!itemLocationCache.ContainsKey(itemID))
            {
                itemLocationCache[itemID] = new List<int>();
            }

            if (!itemLocationCache[itemID].Contains(slotIndex))
            {
                itemLocationCache[itemID].Add(slotIndex);
            }
        }

        /// <summary>
        /// Removes a slot index from the cache for an item.
        /// </summary>
        private void RemoveFromCache(string itemID, int slotIndex)
        {
            EnsureCacheInitialized();

            if (itemLocationCache.TryGetValue(itemID, out List<int> indices))
            {
                indices.Remove(slotIndex);

                // Clean up empty lists
                if (indices.Count == 0)
                {
                    itemLocationCache.Remove(itemID);
                }
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clears all items from the inventory.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsLocked)
                {
                    slots[i].TryClear(out _);
                }
            }

            itemLocationCache?.Clear();
            OnInventoryCleared?.Invoke();
        }

        /// <summary>
        /// Gets all non-empty item stacks in the inventory.
        /// </summary>
        public List<ItemStack> GetAllItems()
        {
            List<ItemStack> items = new List<ItemStack>();

            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    items.Add(slots[i].Stack);
                }
            }

            return items;
        }

        #endregion
    }
}
