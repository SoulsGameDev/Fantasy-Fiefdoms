using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inventory.Core;
using Inventory.Data;

namespace Inventory.Utilities
{
    /// <summary>
    /// Utility functions for inventory operations like sorting, stacking, and organization.
    /// </summary>
    public static class InventoryUtilities
    {
        #region Auto-Sort

        /// <summary>
        /// Sorts inventory by the specified criteria.
        /// </summary>
        public static void AutoSort(Inventory.Core.Inventory inventory, SortCriteria criteria = SortCriteria.ByType)
        {
            if (inventory == null)
            {
                Debug.LogWarning("Cannot sort null inventory");
                return;
            }

            // Get all items
            var allItems = inventory.GetAllItems();
            if (allItems.Count == 0)
                return;

            // Sort based on criteria
            var sortedItems = SortItems(allItems, criteria);

            // Clear inventory
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                inventory.SetStack(i, new ItemStack(), out _);
            }

            // Re-add items in sorted order
            foreach (var stack in sortedItems)
            {
                inventory.TryAddItem(stack, out _);
            }

            Debug.Log($"Sorted inventory by {criteria}");
        }

        private static List<ItemStack> SortItems(List<ItemStack> items, SortCriteria criteria)
        {
            return criteria switch
            {
                SortCriteria.ByName => items.OrderBy(s => s.Type.Name).ToList(),
                SortCriteria.ByType => items.OrderBy(s => s.Type.Category).ThenBy(s => s.Type.Name).ToList(),
                SortCriteria.ByRarity => items.OrderByDescending(s => s.Type.Rarity).ThenBy(s => s.Type.Name).ToList(),
                SortCriteria.ByValue => items.OrderByDescending(s => s.TotalValue).ToList(),
                SortCriteria.ByWeight => items.OrderBy(s => s.TotalWeight).ToList(),
                SortCriteria.ByQuantity => items.OrderByDescending(s => s.Quantity).ToList(),
                _ => items
            };
        }

        #endregion

        #region Quick-Stack

        /// <summary>
        /// Quickly stacks all items of the same type together, consolidating partial stacks.
        /// </summary>
        public static int QuickStack(Inventory.Core.Inventory inventory)
        {
            if (inventory == null)
            {
                Debug.LogWarning("Cannot quick-stack null inventory");
                return 0;
            }

            int stacksMerged = 0;

            // Group items by type
            var itemGroups = new Dictionary<string, List<int>>();

            for (int i = 0; i < inventory.SlotCount; i++)
            {
                ItemStack stack = inventory.GetStack(i);
                if (!stack.IsEmpty && stack.CanStack)
                {
                    string itemID = stack.Type.ItemID;

                    if (!itemGroups.ContainsKey(itemID))
                    {
                        itemGroups[itemID] = new List<int>();
                    }

                    itemGroups[itemID].Add(i);
                }
            }

            // Consolidate each group
            foreach (var group in itemGroups.Values)
            {
                if (group.Count > 1)
                {
                    stacksMerged += ConsolidateStacks(inventory, group);
                }
            }

            Debug.Log($"Quick-stacked inventory: {stacksMerged} stacks merged");
            return stacksMerged;
        }

        private static int ConsolidateStacks(Inventory.Core.Inventory inventory, List<int> slotIndices)
        {
            int mergeCount = 0;
            slotIndices.Sort(); // Process in order

            for (int i = 0; i < slotIndices.Count; i++)
            {
                int sourceSlot = slotIndices[i];
                ItemStack sourceStack = inventory.GetStack(sourceSlot);

                if (sourceStack.IsEmpty)
                    continue;

                // Try to merge with subsequent stacks
                for (int j = i + 1; j < slotIndices.Count; j++)
                {
                    int targetSlot = slotIndices[j];
                    ItemStack targetStack = inventory.GetStack(targetSlot);

                    if (targetStack.IsEmpty)
                        continue;

                    // Try to merge
                    if (targetStack.TryAddToStack(sourceStack.Quantity, out int remainder))
                    {
                        // Update target
                        inventory.SetStack(targetSlot, targetStack, out _);

                        if (remainder == 0)
                        {
                            // Fully merged - clear source
                            inventory.SetStack(sourceSlot, new ItemStack(), out _);
                            mergeCount++;
                            break;
                        }
                        else
                        {
                            // Partially merged - update source
                            sourceStack = sourceStack.Type.CreateStack(remainder);
                            inventory.SetStack(sourceSlot, sourceStack, out _);
                            mergeCount++;
                        }
                    }
                }
            }

            return mergeCount;
        }

        #endregion

        #region Quick-Transfer

        /// <summary>
        /// Quickly transfers all items from one inventory to another.
        /// </summary>
        public static int QuickTransfer(Inventory.Core.Inventory source, Inventory.Core.Inventory target, ItemCategory? filterCategory = null)
        {
            if (source == null || target == null)
            {
                Debug.LogWarning("Cannot quick-transfer: source or target is null");
                return 0;
            }

            int itemsTransferred = 0;
            var allItems = source.GetAllItems();

            foreach (var stack in allItems)
            {
                // Apply category filter if specified
                if (filterCategory.HasValue && stack.Type.Category != filterCategory.Value)
                    continue;

                // Try to remove from source
                if (source.TryRemoveItem(stack.Type.ItemID, stack.Quantity, out int removed))
                {
                    // Try to add to target
                    if (target.TryAddItem(stack.Type.CreateStack(removed), out int remaining))
                    {
                        itemsTransferred += (removed - remaining);

                        // If there's a remainder, add it back to source
                        if (remaining > 0)
                        {
                            source.TryAddItem(stack.Type.CreateStack(remaining), out _);
                        }
                    }
                    else
                    {
                        // Failed to add to target, restore to source
                        source.TryAddItem(stack.Type.CreateStack(removed), out _);
                    }
                }
            }

            Debug.Log($"Quick-transferred {itemsTransferred} items");
            return itemsTransferred;
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Removes all empty slots and compacts the inventory.
        /// </summary>
        public static void Compact(Inventory.Core.Inventory inventory)
        {
            if (inventory == null)
                return;

            var allItems = inventory.GetAllItems();

            // Clear inventory
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                inventory.SetStack(i, new ItemStack(), out _);
            }

            // Re-add all items (will naturally compact)
            foreach (var stack in allItems)
            {
                inventory.TryAddItem(stack, out _);
            }

            Debug.Log("Compacted inventory");
        }

        #endregion

        #region Search & Filter

        /// <summary>
        /// Finds all items matching a search query.
        /// </summary>
        public static List<ItemStack> FindItems(Inventory.Core.Inventory inventory, string searchQuery)
        {
            if (inventory == null || string.IsNullOrEmpty(searchQuery))
                return new List<ItemStack>();

            var results = new List<ItemStack>();
            var allItems = inventory.GetAllItems();
            string lowerQuery = searchQuery.ToLower();

            foreach (var stack in allItems)
            {
                if (stack.Type.Name.ToLower().Contains(lowerQuery) ||
                    stack.Type.ItemID.ToLower().Contains(lowerQuery))
                {
                    results.Add(stack);
                }
            }

            return results;
        }

        /// <summary>
        /// Gets all items of a specific category.
        /// </summary>
        public static List<ItemStack> GetItemsByCategory(Inventory.Core.Inventory inventory, ItemCategory category)
        {
            if (inventory == null)
                return new List<ItemStack>();

            return inventory.GetAllItems()
                .Where(s => s.Type.Category == category)
                .ToList();
        }

        /// <summary>
        /// Gets all items of a specific rarity.
        /// </summary>
        public static List<ItemStack> GetItemsByRarity(Inventory.Core.Inventory inventory, ItemRarity rarity)
        {
            if (inventory == null)
                return new List<ItemStack>();

            return inventory.GetAllItems()
                .Where(s => s.Type.Rarity == rarity)
                .ToList();
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Gets statistics about the inventory contents.
        /// </summary>
        public static InventoryStats GetStats(Inventory.Core.Inventory inventory)
        {
            if (inventory == null)
                return new InventoryStats();

            var stats = new InventoryStats
            {
                TotalSlots = inventory.SlotCount,
                UsedSlots = inventory.SlotCount - inventory.GetEmptySlotCount(),
                EmptySlots = inventory.GetEmptySlotCount(),
                TotalWeight = inventory.GetTotalWeight(),
                MaxWeight = inventory.MaxWeight,
                TotalValue = inventory.GetTotalValue(),
                TotalItems = 0
            };

            var allItems = inventory.GetAllItems();
            foreach (var stack in allItems)
            {
                stats.TotalItems += stack.Quantity;
            }

            // Count by category
            stats.ItemsByCategory = new Dictionary<ItemCategory, int>();
            foreach (var stack in allItems)
            {
                if (!stats.ItemsByCategory.ContainsKey(stack.Type.Category))
                {
                    stats.ItemsByCategory[stack.Type.Category] = 0;
                }
                stats.ItemsByCategory[stack.Type.Category] += stack.Quantity;
            }

            return stats;
        }

        #endregion
    }

    /// <summary>
    /// Criteria for sorting inventory.
    /// </summary>
    public enum SortCriteria
    {
        ByName,         // Alphabetical by name
        ByType,         // By category, then name
        ByRarity,       // By rarity (highest first), then name
        ByValue,        // By total value (highest first)
        ByWeight,       // By weight (lightest first)
        ByQuantity      // By quantity (most first)
    }

    /// <summary>
    /// Statistics about an inventory.
    /// </summary>
    public class InventoryStats
    {
        public int TotalSlots;
        public int UsedSlots;
        public int EmptySlots;
        public float TotalWeight;
        public float MaxWeight;
        public int TotalValue;
        public int TotalItems;
        public Dictionary<ItemCategory, int> ItemsByCategory;

        public float WeightPercent => MaxWeight > 0 ? (TotalWeight / MaxWeight) : 0f;
        public float UsagePercent => TotalSlots > 0 ? ((float)UsedSlots / TotalSlots) : 0f;

        public override string ToString()
        {
            return $"Slots: {UsedSlots}/{TotalSlots} ({UsagePercent * 100:F0}%), " +
                   $"Weight: {TotalWeight:F1}/{MaxWeight} ({WeightPercent * 100:F0}%), " +
                   $"Value: {TotalValue}g, Items: {TotalItems}";
        }
    }
}
