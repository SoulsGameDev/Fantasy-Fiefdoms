using UnityEngine;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Commands;
using Inventory.Utilities;

namespace Inventory.Examples
{
    /// <summary>
    /// Example demonstrating advanced inventory features including unique items,
    /// durability, modifiers, crafting, sorting, and quick-stacking.
    ///
    /// Setup Instructions:
    /// 1. Add this script to an empty GameObject
    /// 2. Create test ItemType and CraftingRecipe ScriptableObjects
    /// 3. Assign them in the inspector
    /// 4. Press Play and use keyboard shortcuts
    ///
    /// Keyboard Shortcuts:
    /// - I: Create unique item with random modifiers
    /// - D: Damage random item durability
    /// - R: Repair random item
    /// - C: Craft from recipe
    /// - S: Auto-sort inventory
    /// - Q: Quick-stack items
    /// - T: Quick-transfer to storage
    /// - K: Compact inventory
    /// - P: Print inventory stats
    /// - 1-6: Sort by different criteria
    /// </summary>
    public class AdvancedFeaturesExample : MonoBehaviour
    {
        [Header("Test Items")]
        [SerializeField] private ItemType[] testItems;

        [Header("Crafting")]
        [SerializeField] private CraftingRecipe[] testRecipes;
        [SerializeField] private int playerCraftingLevel = 5;

        // Inventories
        private Inventory.Core.Inventory playerInventory;
        private Inventory.Core.Inventory storageInventory;
        private CommandHistory commandHistory;

        // Unique items tracking
        private List<ItemInstance> uniqueItems = new List<ItemInstance>();

        #region Initialization

        private void Start()
        {
            Debug.Log("=== Advanced Features Example Started ===");
            Debug.Log("Keyboard Shortcuts:");
            Debug.Log("  I - Create Unique Item");
            Debug.Log("  D - Damage Item Durability");
            Debug.Log("  R - Repair Item");
            Debug.Log("  C - Craft Item");
            Debug.Log("  S - Auto-Sort");
            Debug.Log("  Q - Quick-Stack");
            Debug.Log("  T - Quick-Transfer to Storage");
            Debug.Log("  K - Compact Inventory");
            Debug.Log("  P - Print Stats");
            Debug.Log("  1-6 - Sort by Name/Type/Rarity/Value/Weight/Quantity");

            SetupInventories();
            SetupCommandHistory();
            AddTestItems();
        }

        private void SetupInventories()
        {
            playerInventory = new Inventory.Core.Inventory("player_advanced", maxSlots: 30, maxWeight: 200f);
            storageInventory = new Inventory.Core.Inventory("storage", maxSlots: 50, maxWeight: 500f);

            InventoryManager.Instance.RegisterInventory(playerInventory);
            InventoryManager.Instance.RegisterInventory(storageInventory);

            Debug.Log("Inventories created");
        }

        private void SetupCommandHistory()
        {
            commandHistory = new CommandHistory(maxHistorySize: 50);
        }

        private void AddTestItems()
        {
            if (testItems == null || testItems.Length == 0)
            {
                Debug.LogWarning("No test items assigned!");
                return;
            }

            // Add regular items
            foreach (var item in testItems)
            {
                if (item != null)
                {
                    int quantity = Random.Range(2, 8);
                    ItemStack stack = item.CreateStack(quantity);
                    playerInventory.TryAddItem(stack, out _);
                }
            }

            Debug.Log("Added test items to inventory");
        }

        #endregion

        #region Input Handling

        private void Update()
        {
            // Create unique item (I key)
            if (Input.GetKeyDown(KeyCode.I))
            {
                CreateUniqueItem();
            }

            // Damage durability (D key)
            if (Input.GetKeyDown(KeyCode.D))
            {
                DamageRandomItem();
            }

            // Repair (R key)
            if (Input.GetKeyDown(KeyCode.R))
            {
                RepairRandomItem();
            }

            // Craft (C key)
            if (Input.GetKeyDown(KeyCode.C))
            {
                CraftRandomRecipe();
            }

            // Auto-sort (S key)
            if (Input.GetKeyDown(KeyCode.S))
            {
                AutoSort();
            }

            // Quick-stack (Q key)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                QuickStack();
            }

            // Quick-transfer (T key)
            if (Input.GetKeyDown(KeyCode.T))
            {
                QuickTransfer();
            }

            // Compact (K key)
            if (Input.GetKeyDown(KeyCode.K))
            {
                CompactInventory();
            }

            // Print stats (P key)
            if (Input.GetKeyDown(KeyCode.P))
            {
                PrintStats();
            }

            // Sort by criteria (1-6 keys)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SortBy(SortCriteria.ByName);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SortBy(SortCriteria.ByType);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SortBy(SortCriteria.ByRarity);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SortBy(SortCriteria.ByValue);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                SortBy(SortCriteria.ByWeight);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                SortBy(SortCriteria.ByQuantity);
            }
        }

        #endregion

        #region Unique Items

        private void CreateUniqueItem()
        {
            if (testItems == null || testItems.Length == 0)
            {
                Debug.LogWarning("No test items available");
                return;
            }

            // Pick a random item type
            ItemType baseType = testItems[Random.Range(0, testItems.Length)];

            // Create unique instance with random quality
            ItemQuality[] qualities = System.Enum.GetValues(typeof(ItemQuality)) as ItemQuality[];
            ItemQuality randomQuality = qualities[Random.Range(0, qualities.Length)];

            ItemInstance uniqueItem = new ItemInstance(baseType, randomQuality);

            // Add random modifiers
            int modifierCount = Random.Range(1, 4);
            for (int i = 0; i < modifierCount; i++)
            {
                uniqueItem.AddModifier(GenerateRandomModifier());
            }

            // Set custom name
            string prefix = GetRandomPrefix();
            uniqueItem.SetCustomName($"{prefix} {baseType.Name}");

            // Track the unique item
            uniqueItems.Add(uniqueItem);

            // Add to inventory (as regular stack for now)
            ItemStack stack = baseType.CreateStack(1);
            playerInventory.TryAddItem(stack, out _);

            Debug.Log($"<color=cyan>Created unique item:</color>\n{uniqueItem.GetFullDescription()}");
        }

        private ItemModifier GenerateRandomModifier()
        {
            string[] stats = { "Strength", "Dexterity", "Intelligence", "Vitality", "Armor", "Damage" };
            string[] prefixes = { "of Power", "of Agility", "of Wisdom", "of Fortitude", "of Protection", "of Destruction" };

            string stat = stats[Random.Range(0, stats.Length)];
            string name = prefixes[Random.Range(0, prefixes.Length)];
            float value = Random.Range(5f, 25f);
            bool isPercentage = Random.value > 0.5f;
            ItemModifierTier tier = (ItemModifierTier)Random.Range(0, 4);

            return new ItemModifier(name, stat, value, isPercentage, tier);
        }

        private string GetRandomPrefix()
        {
            string[] prefixes = { "Mighty", "Swift", "Wise", "Fortified", "Enchanted", "Legendary", "Ancient", "Cursed" };
            return prefixes[Random.Range(0, prefixes.Length)];
        }

        #endregion

        #region Durability

        private void DamageRandomItem()
        {
            if (uniqueItems.Count == 0)
            {
                Debug.LogWarning("No unique items to damage");
                return;
            }

            ItemInstance item = uniqueItems[Random.Range(0, uniqueItems.Count)];
            int damage = Random.Range(5, 20);

            if (item.DamageDurability(damage))
            {
                Debug.Log($"<color=yellow>Damaged {item.DisplayName}:</color> Durability {item.CurrentDurability}/{item.MaxDurability}");

                if (item.IsBroken)
                {
                    Debug.Log($"<color=red>{item.DisplayName} is broken!</color>");
                }
            }
            else
            {
                Debug.Log($"Cannot damage {item.DisplayName} (indestructible or no durability)");
            }
        }

        private void RepairRandomItem()
        {
            if (uniqueItems.Count == 0)
            {
                Debug.LogWarning("No unique items to repair");
                return;
            }

            ItemInstance item = uniqueItems[Random.Range(0, uniqueItems.Count)];

            if (item.CurrentDurability < item.MaxDurability)
            {
                item.FullRepair();
                Debug.Log($"<color=green>Fully repaired {item.DisplayName}:</color> Durability {item.CurrentDurability}/{item.MaxDurability}");
            }
            else
            {
                Debug.Log($"{item.DisplayName} is already at full durability");
            }
        }

        #endregion

        #region Crafting

        private void CraftRandomRecipe()
        {
            if (testRecipes == null || testRecipes.Length == 0)
            {
                Debug.LogWarning("No crafting recipes assigned");
                return;
            }

            CraftingRecipe recipe = testRecipes[Random.Range(0, testRecipes.Length)];

            var craftCmd = new CraftItemCommand(
                inventory: playerInventory,
                recipe: recipe,
                playerCraftingLevel: playerCraftingLevel,
                availableStation: CraftingStation.Workbench
            );

            if (craftCmd.CanExecute())
            {
                bool success = craftCmd.Execute();

                if (success)
                {
                    commandHistory.AddCommand(craftCmd);
                    Debug.Log($"<color=green>Crafted {recipe.ResultQuantity}x {recipe.ResultItem.Name}!</color>");
                }
                else
                {
                    Debug.LogError($"Crafting failed: {craftCmd.ErrorMessage}");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>Cannot craft {recipe.RecipeName}:</color> {craftCmd.ErrorMessage}");
            }
        }

        #endregion

        #region Utilities

        private void AutoSort()
        {
            InventoryUtilities.AutoSort(playerInventory, SortCriteria.ByType);
            Debug.Log("<color=cyan>Inventory auto-sorted by type</color>");
        }

        private void SortBy(SortCriteria criteria)
        {
            InventoryUtilities.AutoSort(playerInventory, criteria);
            Debug.Log($"<color=cyan>Inventory sorted by {criteria}</color>");
        }

        private void QuickStack()
        {
            int merged = InventoryUtilities.QuickStack(playerInventory);
            Debug.Log($"<color=cyan>Quick-stacked:</color> {merged} stacks merged");
        }

        private void QuickTransfer()
        {
            int transferred = InventoryUtilities.QuickTransfer(playerInventory, storageInventory);
            Debug.Log($"<color=cyan>Quick-transferred:</color> {transferred} items to storage");
        }

        private void CompactInventory()
        {
            InventoryUtilities.Compact(playerInventory);
            Debug.Log("<color=cyan>Inventory compacted</color>");
        }

        private void PrintStats()
        {
            var stats = InventoryUtilities.GetStats(playerInventory);
            Debug.Log($"<color=cyan>Inventory Stats:</color>\n{stats}");

            if (stats.ItemsByCategory != null && stats.ItemsByCategory.Count > 0)
            {
                Debug.Log("Items by category:");
                foreach (var kvp in stats.ItemsByCategory)
                {
                    Debug.Log($"  {kvp.Key}: {kvp.Value} items");
                }
            }
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 450, 700));
            GUILayout.Box("Advanced Features Example");

            // Player inventory stats
            var stats = InventoryUtilities.GetStats(playerInventory);

            GUILayout.Label("<b>Player Inventory</b>");
            GUILayout.Label($"Slots: {stats.UsedSlots}/{stats.TotalSlots} ({stats.UsagePercent * 100:F0}%)");
            GUILayout.Label($"Weight: {stats.TotalWeight:F1}/{stats.MaxWeight} ({stats.WeightPercent * 100:F0}%)");
            GUILayout.Label($"Total Value: {stats.TotalValue} gold");
            GUILayout.Label($"Total Items: {stats.TotalItems}");

            GUILayout.Space(10);

            // Unique items
            GUILayout.Label($"<b>Unique Items: {uniqueItems.Count}</b>");
            if (uniqueItems.Count > 0)
            {
                foreach (var item in uniqueItems)
                {
                    string durabilityText = item.MaxDurability > 0
                        ? $" ({item.CurrentDurability}/{item.MaxDurability})"
                        : "";

                    string brokenText = item.IsBroken ? " <color=red>[BROKEN]</color>" : "";

                    GUILayout.Label($"  {item.DisplayName} ({item.Quality}){durabilityText}{brokenText}");
                    GUILayout.Label($"    Modifiers: {item.Modifiers.Count}, Value: {item.GetTotalValue()}g");
                }
            }

            GUILayout.Space(10);

            // Crafting
            GUILayout.Label("<b>Crafting</b>");
            GUILayout.Label($"Level: {playerCraftingLevel}");
            if (testRecipes != null && testRecipes.Length > 0)
            {
                GUILayout.Label($"Available Recipes: {testRecipes.Length}");
            }

            GUILayout.Space(10);

            // Storage
            var storageStats = InventoryUtilities.GetStats(storageInventory);
            GUILayout.Label("<b>Storage</b>");
            GUILayout.Label($"Items: {storageStats.TotalItems}");
            GUILayout.Label($"Slots: {storageStats.UsedSlots}/{storageStats.TotalSlots}");

            GUILayout.Space(10);

            // Buttons
            if (GUILayout.Button("Create Unique Item (I)"))
            {
                CreateUniqueItem();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Damage (D)"))
            {
                DamageRandomItem();
            }
            if (GUILayout.Button("Repair (R)"))
            {
                RepairRandomItem();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Craft Random (C)"))
            {
                CraftRandomRecipe();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Auto-Sort (S)"))
            {
                AutoSort();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Quick-Stack (Q)"))
            {
                QuickStack();
            }
            if (GUILayout.Button("Compact (K)"))
            {
                CompactInventory();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Transfer to Storage (T)"))
            {
                QuickTransfer();
            }

            if (GUILayout.Button("Print Stats (P)"))
            {
                PrintStats();
            }

            GUILayout.Space(5);

            // Sort buttons
            GUILayout.Label("<b>Sort By:</b>");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Name(1)"))
            {
                SortBy(SortCriteria.ByName);
            }
            if (GUILayout.Button("Type(2)"))
            {
                SortBy(SortCriteria.ByType);
            }
            if (GUILayout.Button("Rarity(3)"))
            {
                SortBy(SortCriteria.ByRarity);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Value(4)"))
            {
                SortBy(SortCriteria.ByValue);
            }
            if (GUILayout.Button("Weight(5)"))
            {
                SortBy(SortCriteria.ByWeight);
            }
            if (GUILayout.Button("Qty(6)"))
            {
                SortBy(SortCriteria.ByQuantity);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        #endregion
    }
}
