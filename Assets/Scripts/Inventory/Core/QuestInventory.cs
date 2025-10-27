using System;
using System.Collections.Generic;
using Inventory.Data;
using UnityEngine;

namespace Inventory.Core
{
    /// <summary>
    /// Specialized inventory for quest items only.
    /// Prevents dropping, selling, or trading quest items.
    /// Automatically removes items when quests are completed.
    /// </summary>
    [Serializable]
    public class QuestInventory : Inventory
    {
        [Header("Quest Settings")]
        [SerializeField] private List<string> activeQuestIDs = new List<string>();

        // Track which items belong to which quests
        private Dictionary<string, List<string>> questItemMap = new Dictionary<string, List<string>>();

        #region Events

        /// <summary>Fired when a quest is started</summary>
        public event Action<string> OnQuestStarted;

        /// <summary>Fired when a quest is completed</summary>
        public event Action<string> OnQuestCompleted;

        /// <summary>Fired when a quest item is added</summary>
        public event Action<string, ItemStack> OnQuestItemAdded;

        /// <summary>Fired when a quest item is removed</summary>
        public event Action<string, ItemStack> OnQuestItemRemoved;

        #endregion

        #region Properties

        /// <summary>List of active quest IDs</summary>
        public List<string> ActiveQuestIDs => new List<string>(activeQuestIDs);

        /// <summary>Number of active quests</summary>
        public int ActiveQuestCount => activeQuestIDs.Count;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new quest inventory.
        /// </summary>
        public QuestInventory(string ownerID, int maxSlots = 20)
            : base($"{ownerID}_quests", maxSlots, maxWeight: -1f)
        {
            questItemMap = new Dictionary<string, List<string>>();
        }

        #endregion

        #region Quest Management

        /// <summary>
        /// Starts a new quest.
        /// </summary>
        public void StartQuest(string questID)
        {
            if (string.IsNullOrEmpty(questID))
            {
                Debug.LogWarning("Quest ID cannot be empty");
                return;
            }

            if (activeQuestIDs.Contains(questID))
            {
                Debug.LogWarning($"Quest '{questID}' is already active");
                return;
            }

            activeQuestIDs.Add(questID);

            if (!questItemMap.ContainsKey(questID))
            {
                questItemMap[questID] = new List<string>();
            }

            OnQuestStarted?.Invoke(questID);
            Debug.Log($"Quest started: {questID}");
        }

        /// <summary>
        /// Completes a quest and removes its items.
        /// </summary>
        public void CompleteQuest(string questID, bool removeItems = true)
        {
            if (!activeQuestIDs.Contains(questID))
            {
                Debug.LogWarning($"Quest '{questID}' is not active");
                return;
            }

            // Remove quest items if requested
            if (removeItems && questItemMap.ContainsKey(questID))
            {
                var itemIDs = new List<string>(questItemMap[questID]);
                foreach (string itemID in itemIDs)
                {
                    int quantity = GetItemQuantity(itemID);
                    if (quantity > 0)
                    {
                        TryRemoveItem(itemID, quantity, out _);
                    }
                }
            }

            // Remove quest from active list
            activeQuestIDs.Remove(questID);
            questItemMap.Remove(questID);

            OnQuestCompleted?.Invoke(questID);
            Debug.Log($"Quest completed: {questID}");
        }

        /// <summary>
        /// Abandons a quest without removing items.
        /// </summary>
        public void AbandonQuest(string questID)
        {
            CompleteQuest(questID, removeItems: false);
        }

        /// <summary>
        /// Checks if a quest is active.
        /// </summary>
        public bool IsQuestActive(string questID)
        {
            return activeQuestIDs.Contains(questID);
        }

        #endregion

        #region Quest Item Management

        /// <summary>
        /// Adds a quest item to the inventory.
        /// </summary>
        public bool AddQuestItem(string questID, ItemStack stack)
        {
            if (!IsQuestActive(questID))
            {
                Debug.LogWarning($"Cannot add quest item - quest '{questID}' is not active");
                return false;
            }

            if (stack.IsEmpty || stack.Type == null)
            {
                Debug.LogWarning("Cannot add empty stack");
                return false;
            }

            // Verify it's a quest item
            if (stack.Type.Category != ItemCategory.Quest)
            {
                Debug.LogWarning($"{stack.Type.Name} is not a quest item");
                return false;
            }

            // Add to inventory
            bool success = TryAddItem(stack, out int remaining);

            if (success && remaining == 0)
            {
                // Track which quest this item belongs to
                if (!questItemMap.ContainsKey(questID))
                {
                    questItemMap[questID] = new List<string>();
                }

                if (!questItemMap[questID].Contains(stack.Type.ItemID))
                {
                    questItemMap[questID].Add(stack.Type.ItemID);
                }

                OnQuestItemAdded?.Invoke(questID, stack);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a quest item from the inventory.
        /// </summary>
        public bool RemoveQuestItem(string questID, string itemID, int quantity)
        {
            if (!IsQuestActive(questID))
            {
                Debug.LogWarning($"Cannot remove quest item - quest '{questID}' is not active");
                return false;
            }

            if (!questItemMap.ContainsKey(questID) || !questItemMap[questID].Contains(itemID))
            {
                Debug.LogWarning($"Item '{itemID}' does not belong to quest '{questID}'");
                return false;
            }

            bool success = TryRemoveItem(itemID, quantity, out int removed);

            if (success && removed == quantity)
            {
                // If all removed, remove from tracking
                if (GetItemQuantity(itemID) == 0)
                {
                    questItemMap[questID].Remove(itemID);
                }

                // Create a dummy stack for the event
                ItemType itemType = InventoryManager.Instance?.FindItemType(itemID);
                if (itemType != null)
                {
                    ItemStack removedStack = itemType.CreateStack(removed);
                    OnQuestItemRemoved?.Invoke(questID, removedStack);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all items for a specific quest.
        /// </summary>
        public List<ItemStack> GetQuestItems(string questID)
        {
            List<ItemStack> items = new List<ItemStack>();

            if (!questItemMap.ContainsKey(questID))
                return items;

            foreach (string itemID in questItemMap[questID])
            {
                int quantity = GetItemQuantity(itemID);
                if (quantity > 0)
                {
                    ItemType itemType = InventoryManager.Instance?.FindItemType(itemID);
                    if (itemType != null)
                    {
                        items.Add(itemType.CreateStack(quantity));
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Checks if a specific quest item requirement is met.
        /// </summary>
        public bool HasQuestItem(string questID, string itemID, int requiredQuantity)
        {
            if (!IsQuestActive(questID))
                return false;

            if (!questItemMap.ContainsKey(questID) || !questItemMap[questID].Contains(itemID))
                return false;

            return GetItemQuantity(itemID) >= requiredQuantity;
        }

        /// <summary>
        /// Gets the quantity of a quest item.
        /// </summary>
        public int GetQuestItemQuantity(string questID, string itemID)
        {
            if (!IsQuestActive(questID))
                return 0;

            if (!questItemMap.ContainsKey(questID) || !questItemMap[questID].Contains(itemID))
                return 0;

            return GetItemQuantity(itemID);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates that only quest items can be added.
        /// </summary>
        public new bool TryAddItem(ItemStack stack, out int remainingQuantity)
        {
            // Ensure it's a quest item
            if (stack.Type != null && stack.Type.Category != ItemCategory.Quest)
            {
                Debug.LogWarning($"Cannot add non-quest item to quest inventory: {stack.Type.Name}");
                remainingQuantity = stack.Quantity;
                return false;
            }

            return base.TryAddItem(stack, out remainingQuantity);
        }

        /// <summary>
        /// Prevents direct removal without quest context.
        /// Use RemoveQuestItem instead.
        /// </summary>
        public new bool TryRemoveItem(string itemID, int quantity, out int removedQuantity)
        {
            // Allow removal internally
            return base.TryRemoveItem(itemID, quantity, out removedQuantity);
        }

        #endregion

        #region Quest Progress

        /// <summary>
        /// Gets a summary of quest progress.
        /// </summary>
        public string GetQuestProgressSummary()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Active Quests: {activeQuestIDs.Count}");

            foreach (string questID in activeQuestIDs)
            {
                sb.AppendLine($"\n{questID}:");
                var items = GetQuestItems(questID);
                foreach (var item in items)
                {
                    sb.AppendLine($"  - {item.Type.Name} x{item.Quantity}");
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks if all requirements for a quest are met.
        /// </summary>
        public bool AreQuestRequirementsMet(string questID, Dictionary<string, int> requirements)
        {
            if (!IsQuestActive(questID))
                return false;

            foreach (var requirement in requirements)
            {
                if (!HasQuestItem(questID, requirement.Key, requirement.Value))
                    return false;
            }

            return true;
        }

        #endregion

        #region Utility

        /// <summary>
        /// Clears all quests and items.
        /// </summary>
        public void ClearAllQuests()
        {
            var questIDs = new List<string>(activeQuestIDs);
            foreach (string questID in questIDs)
            {
                CompleteQuest(questID, removeItems: true);
            }

            activeQuestIDs.Clear();
            questItemMap.Clear();
            Clear();
        }

        #endregion
    }
}
