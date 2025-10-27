using System.Collections.Generic;
using Inventory.Data;
using UnityEngine;

namespace Inventory.Core
{
    /// <summary>
    /// Singleton manager for all inventories in the game.
    /// Handles registration, lookup, and global inventory operations.
    /// Extends Singleton pattern from the main codebase.
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>
    {
        #region Fields

        [Header("Item Database")]
        [Tooltip("All available item types in the game")]
        public List<ItemType> ItemTypes = new List<ItemType>();

        [Header("Settings")]
        [Tooltip("Default capacity for player inventories")]
        public int DefaultPlayerInventorySize = 20;

        [Tooltip("Default weight limit for player inventories (-1 for unlimited)")]
        public float DefaultPlayerWeightLimit = 100f;

        // Registry of all active inventories
        private Dictionary<string, Inventory> inventoryRegistry = new Dictionary<string, Inventory>();

        #endregion

        #region Initialization

        protected override void Init()
        {
            base.Init();

            // Initialize the item database
            if (ItemTypes.Count > 0)
            {
                Debug.Log($"InventoryManager initialized with {ItemTypes.Count} item types");
            }
            else
            {
                Debug.LogWarning("InventoryManager: No item types loaded. Add ItemType ScriptableObjects to the manager.");
            }

            inventoryRegistry = new Dictionary<string, Inventory>();
        }

        #endregion

        #region Inventory Registry

        /// <summary>
        /// Registers an inventory with the manager.
        /// </summary>
        /// <param name="inventory">The inventory to register</param>
        /// <returns>True if registration successful</returns>
        public bool RegisterInventory(Inventory inventory)
        {
            if (inventory == null)
            {
                Debug.LogError("Cannot register null inventory");
                return false;
            }

            if (string.IsNullOrEmpty(inventory.InventoryID))
            {
                Debug.LogError("Cannot register inventory with empty ID");
                return false;
            }

            if (inventoryRegistry.ContainsKey(inventory.InventoryID))
            {
                Debug.LogWarning($"Inventory with ID '{inventory.InventoryID}' already registered. Overwriting.");
            }

            inventoryRegistry[inventory.InventoryID] = inventory;
            return true;
        }

        /// <summary>
        /// Unregisters an inventory from the manager.
        /// </summary>
        /// <param name="inventoryID">The inventory ID to unregister</param>
        /// <returns>True if unregistration successful</returns>
        public bool UnregisterInventory(string inventoryID)
        {
            return inventoryRegistry.Remove(inventoryID);
        }

        /// <summary>
        /// Gets an inventory by ID.
        /// </summary>
        /// <param name="inventoryID">The inventory ID</param>
        /// <returns>The inventory, or null if not found</returns>
        public Inventory GetInventory(string inventoryID)
        {
            if (inventoryRegistry.TryGetValue(inventoryID, out Inventory inventory))
            {
                return inventory;
            }

            Debug.LogWarning($"Inventory '{inventoryID}' not found");
            return null;
        }

        /// <summary>
        /// Checks if an inventory is registered.
        /// </summary>
        public bool HasInventory(string inventoryID)
        {
            return inventoryRegistry.ContainsKey(inventoryID);
        }

        /// <summary>
        /// Gets all registered inventory IDs.
        /// </summary>
        public List<string> GetAllInventoryIDs()
        {
            return new List<string>(inventoryRegistry.Keys);
        }

        /// <summary>
        /// Gets the count of registered inventories.
        /// </summary>
        public int GetInventoryCount()
        {
            return inventoryRegistry.Count;
        }

        #endregion

        #region Inventory Creation

        /// <summary>
        /// Creates a new inventory and registers it.
        /// </summary>
        /// <param name="inventoryID">Unique identifier</param>
        /// <param name="maxSlots">Maximum slots</param>
        /// <param name="maxWeight">Maximum weight (-1 for unlimited)</param>
        /// <returns>The created inventory</returns>
        public Inventory CreateInventory(string inventoryID, int maxSlots, float maxWeight = -1f)
        {
            if (HasInventory(inventoryID))
            {
                Debug.LogWarning($"Inventory '{inventoryID}' already exists. Returning existing inventory.");
                return GetInventory(inventoryID);
            }

            Inventory inventory = new Inventory(inventoryID, maxSlots, maxWeight);
            RegisterInventory(inventory);

            return inventory;
        }

        /// <summary>
        /// Creates a standard player inventory with default settings.
        /// </summary>
        /// <param name="playerID">Player identifier</param>
        /// <returns>The created player inventory</returns>
        public Inventory CreatePlayerInventory(string playerID)
        {
            string inventoryID = $"player_{playerID}_bag";
            return CreateInventory(inventoryID, DefaultPlayerInventorySize, DefaultPlayerWeightLimit);
        }

        #endregion

        #region Item Database

        /// <summary>
        /// Finds an item type by its ID.
        /// </summary>
        /// <param name="itemID">The item ID to search for</param>
        /// <returns>The ItemType, or null if not found</returns>
        public ItemType FindItemType(string itemID)
        {
            foreach (ItemType itemType in ItemTypes)
            {
                if (itemType != null && itemType.ItemID == itemID)
                {
                    return itemType;
                }
            }

            Debug.LogWarning($"ItemType with ID '{itemID}' not found in database");
            return null;
        }

        /// <summary>
        /// Finds item types by category.
        /// </summary>
        /// <param name="category">The category to filter by</param>
        /// <returns>List of matching item types</returns>
        public List<ItemType> FindItemsByCategory(ItemCategory category)
        {
            List<ItemType> results = new List<ItemType>();

            foreach (ItemType itemType in ItemTypes)
            {
                if (itemType != null && itemType.Category == category)
                {
                    results.Add(itemType);
                }
            }

            return results;
        }

        /// <summary>
        /// Finds item types by rarity.
        /// </summary>
        /// <param name="rarity">The rarity to filter by</param>
        /// <returns>List of matching item types</returns>
        public List<ItemType> FindItemsByRarity(ItemRarity rarity)
        {
            List<ItemType> results = new List<ItemType>();

            foreach (ItemType itemType in ItemTypes)
            {
                if (itemType != null && itemType.Rarity == rarity)
                {
                    results.Add(itemType);
                }
            }

            return results;
        }

        /// <summary>
        /// Adds an item type to the database.
        /// </summary>
        public void AddItemType(ItemType itemType)
        {
            if (itemType == null)
            {
                Debug.LogError("Cannot add null item type");
                return;
            }

            if (!ItemTypes.Contains(itemType))
            {
                ItemTypes.Add(itemType);
            }
        }

        /// <summary>
        /// Removes an item type from the database.
        /// </summary>
        public void RemoveItemType(ItemType itemType)
        {
            ItemTypes.Remove(itemType);
        }

        #endregion

        #region Global Operations

        /// <summary>
        /// Transfers an item from one inventory to another.
        /// </summary>
        /// <param name="fromInventoryID">Source inventory ID</param>
        /// <param name="toInventoryID">Destination inventory ID</param>
        /// <param name="itemID">Item ID to transfer</param>
        /// <param name="quantity">Quantity to transfer</param>
        /// <returns>True if transfer successful</returns>
        public bool TransferItem(string fromInventoryID, string toInventoryID, string itemID, int quantity)
        {
            Inventory fromInventory = GetInventory(fromInventoryID);
            Inventory toInventory = GetInventory(toInventoryID);

            if (fromInventory == null || toInventory == null)
            {
                Debug.LogError("Cannot transfer: One or both inventories not found");
                return false;
            }

            // Check if source has the item
            if (!fromInventory.ContainsItem(itemID, quantity))
            {
                Debug.LogWarning($"Source inventory does not contain {quantity}x {itemID}");
                return false;
            }

            // Try to remove from source
            if (!fromInventory.TryRemoveItem(itemID, quantity, out int removed))
            {
                Debug.LogWarning($"Failed to remove item from source inventory");
                return false;
            }

            // Try to add to destination
            ItemType itemType = FindItemType(itemID);
            if (itemType == null)
            {
                Debug.LogError($"ItemType '{itemID}' not found in database");
                // Restore items to source
                fromInventory.TryAddItem(itemType.CreateStack(removed), out _);
                return false;
            }

            ItemStack stack = itemType.CreateStack(removed);
            if (!toInventory.TryAddItem(stack, out int remaining))
            {
                // Failed to add any, restore all to source
                fromInventory.TryAddItem(stack, out _);
                return false;
            }

            // If some items couldn't be added, restore them to source
            if (remaining > 0)
            {
                fromInventory.TryAddItem(itemType.CreateStack(remaining), out _);
            }

            return remaining == 0;
        }

        /// <summary>
        /// Clears all registered inventories.
        /// </summary>
        public void ClearAllInventories()
        {
            foreach (var inventory in inventoryRegistry.Values)
            {
                inventory.Clear();
            }
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Logs information about all registered inventories.
        /// </summary>
        public void DebugLogInventories()
        {
            Debug.Log($"=== InventoryManager Debug Info ===");
            Debug.Log($"Registered Inventories: {inventoryRegistry.Count}");
            Debug.Log($"Item Types: {ItemTypes.Count}");

            foreach (var kvp in inventoryRegistry)
            {
                Inventory inv = kvp.Value;
                Debug.Log($"  [{kvp.Key}] Slots: {inv.SlotCount}, Weight: {inv.GetTotalWeight()}/{inv.MaxWeight}, Value: {inv.GetTotalValue()}");
            }
        }

        #endregion
    }
}
