using UnityEngine;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Commands;
using Inventory.Guards;

namespace Inventory.Examples
{
    /// <summary>
    /// Example demonstrating the merchant trading system.
    /// Shows buying/selling items, price calculations, restock mechanics, and undo/redo.
    ///
    /// Setup Instructions:
    /// 1. Add this script to an empty GameObject
    /// 2. Create some test ItemType ScriptableObjects
    /// 3. Assign them to the testItems array in the inspector
    /// 4. Press Play and use keyboard shortcuts to test
    ///
    /// Keyboard Shortcuts:
    /// - B: Buy random item from merchant
    /// - S: Sell random item to merchant
    /// - R: Restock merchant inventory
    /// - G: Add 100 gold to player
    /// - T: Add random test item to player inventory
    /// - U: Undo last transaction
    /// - Y: Redo last transaction
    /// - M: Toggle infinite stock mode
    /// - P: Adjust merchant prices
    /// </summary>
    public class MerchantSystemExample : MonoBehaviour
    {
        [Header("Test Items")]
        [SerializeField] private ItemType[] testItems;

        [Header("Merchant Settings")]
        [SerializeField] private bool infiniteStock = false;
        [SerializeField] private float buyPriceMultiplier = 1.2f;  // Player buys at 120% of base value
        [SerializeField] private float sellPriceMultiplier = 0.5f; // Player sells at 50% of base value
        [SerializeField] private int merchantStartingGold = 5000;
        [SerializeField] private int playerStartingGold = 500;

        // Game data
        private Inventory.Core.Inventory playerInventory;
        private MerchantInventory merchantInventory;
        private CommandHistory commandHistory;
        private int playerGold;

        #region Initialization

        private void Start()
        {
            Debug.Log("=== Merchant System Example Started ===");
            Debug.Log("Keyboard Shortcuts:");
            Debug.Log("  B - Buy Random Item");
            Debug.Log("  S - Sell Random Item");
            Debug.Log("  R - Restock Merchant");
            Debug.Log("  G - Add 100 Gold");
            Debug.Log("  T - Add Test Item to Inventory");
            Debug.Log("  U - Undo Last Transaction");
            Debug.Log("  Y - Redo Last Transaction");
            Debug.Log("  M - Toggle Infinite Stock");
            Debug.Log("  P - Adjust Prices");

            SetupInventories();
            SetupCommandHistory();
            AddInitialItems();

            // Set initial player gold
            playerGold = playerStartingGold;
            PlayerPrefs.SetInt("PlayerGold", playerGold);
        }

        private void SetupInventories()
        {
            // Create player inventory
            playerInventory = new Inventory.Core.Inventory("player_bag", maxSlots: 30, maxWeight: 150f);
            InventoryManager.Instance.RegisterInventory(playerInventory);

            // Create merchant inventory
            merchantInventory = new MerchantInventory(
                inventoryID: "merchant_general",
                maxSlots: 50,
                maxWeight: 500f,
                infiniteStock: infiniteStock,
                buyPriceMultiplier: buyPriceMultiplier,
                sellPriceMultiplier: sellPriceMultiplier,
                merchantGold: merchantStartingGold
            );

            InventoryManager.Instance.RegisterInventory(merchantInventory);

            // Subscribe to merchant events
            merchantInventory.OnItemSold += OnItemSold;
            merchantInventory.OnItemBought += OnItemBought;
            merchantInventory.OnGoldChanged += OnMerchantGoldChanged;
            merchantInventory.OnRestocked += OnMerchantRestocked;

            Debug.Log($"Merchant created with {merchantInventory.MerchantGold} gold");
            Debug.Log($"Buy multiplier: {buyPriceMultiplier}x, Sell multiplier: {sellPriceMultiplier}x");
        }

        private void SetupCommandHistory()
        {
            commandHistory = new CommandHistory(maxHistorySize: 50);
        }

        private void AddInitialItems()
        {
            if (testItems == null || testItems.Length == 0)
            {
                Debug.LogWarning("No test items assigned! Create some ItemType ScriptableObjects and assign them.");
                return;
            }

            // Add items to merchant inventory
            foreach (var item in testItems)
            {
                if (item != null)
                {
                    int quantity = Random.Range(3, 10);
                    ItemStack stack = item.CreateStack(quantity);
                    merchantInventory.TryAddItem(stack, out _);
                }
            }

            Debug.Log($"Added {testItems.Length} item types to merchant");

            // Add a few items to player inventory for testing selling
            for (int i = 0; i < Mathf.Min(3, testItems.Length); i++)
            {
                ItemStack stack = testItems[i].CreateStack(Random.Range(1, 3));
                playerInventory.TryAddItem(stack, out _);
            }

            Debug.Log("Added some items to player inventory for testing");

            // Setup restock configuration
            SetupRestockItems();
        }

        private void SetupRestockItems()
        {
            if (testItems == null || testItems.Length == 0) return;

            // Configure merchant to restock items
            foreach (var item in testItems)
            {
                if (item != null)
                {
                    merchantInventory.AddRestockItem(
                        itemID: item.ItemID,
                        quantity: Random.Range(3, 8),
                        restockEveryNSeconds: 300f // Restock every 5 minutes
                    );
                }
            }

            Debug.Log("Configured merchant restock items");
        }

        private void OnDestroy()
        {
            if (merchantInventory != null)
            {
                merchantInventory.OnItemSold -= OnItemSold;
                merchantInventory.OnItemBought -= OnItemBought;
                merchantInventory.OnGoldChanged -= OnMerchantGoldChanged;
                merchantInventory.OnRestocked -= OnMerchantRestocked;
            }
        }

        #endregion

        #region Input Handling

        private void Update()
        {
            // Buy item (B key)
            if (Input.GetKeyDown(KeyCode.B))
            {
                BuyRandomItem();
            }

            // Sell item (S key)
            if (Input.GetKeyDown(KeyCode.S))
            {
                SellRandomItem();
            }

            // Restock (R key)
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestockMerchant();
            }

            // Add gold (G key)
            if (Input.GetKeyDown(KeyCode.G))
            {
                playerGold += 100;
                PlayerPrefs.SetInt("PlayerGold", playerGold);
                Debug.Log($"Added 100 gold. Total: {playerGold}");
            }

            // Add test item (T key)
            if (Input.GetKeyDown(KeyCode.T))
            {
                AddRandomTestItem();
            }

            // Undo (U key)
            if (Input.GetKeyDown(KeyCode.U))
            {
                UndoLastTransaction();
            }

            // Redo (Y key)
            if (Input.GetKeyDown(KeyCode.Y))
            {
                RedoLastTransaction();
            }

            // Toggle infinite stock (M key)
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleInfiniteStock();
            }

            // Adjust prices (P key)
            if (Input.GetKeyDown(KeyCode.P))
            {
                AdjustPrices();
            }
        }

        #endregion

        #region Merchant Actions

        private void BuyRandomItem()
        {
            // Get all items merchant has in stock
            var merchantItems = merchantInventory.GetAllItems();

            if (merchantItems.Count == 0)
            {
                Debug.LogWarning("Merchant has no items to sell!");
                return;
            }

            // Pick random item
            ItemStack randomStack = merchantItems[Random.Range(0, merchantItems.Count)];
            int buyQuantity = Random.Range(1, Mathf.Min(3, randomStack.Quantity + 1));

            // Calculate price first
            ItemStack tempStack = randomStack.Type.CreateStack(buyQuantity);
            int price = merchantInventory.GetBuyPrice(tempStack);

            Debug.Log($"Attempting to buy {buyQuantity}x {randomStack.Type.Name} for {price} gold");

            // Create and execute buy command
            BuyItemCommand buyCommand = new BuyItemCommand(
                merchantInventory: merchantInventory,
                playerInventory: playerInventory,
                itemID: randomStack.Type.ItemID,
                quantity: buyQuantity
            );

            if (buyCommand.CanExecute())
            {
                bool success = buyCommand.Execute();

                if (success)
                {
                    commandHistory.AddCommand(buyCommand);
                    playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
                    Debug.Log($"<color=green>Purchase successful!</color> New gold: {playerGold}");
                }
                else
                {
                    Debug.LogError("Purchase failed during execution");
                }
            }
            else
            {
                Debug.LogWarning("Cannot buy this item - validation failed (check gold/space)");
            }
        }

        private void SellRandomItem()
        {
            var playerItems = playerInventory.GetAllItems();

            if (playerItems.Count == 0)
            {
                Debug.LogWarning("Player has no items to sell!");
                return;
            }

            // Pick random item
            ItemStack randomStack = playerItems[Random.Range(0, playerItems.Count)];
            int sellQuantity = Random.Range(1, Mathf.Min(3, randomStack.Quantity + 1));

            // Calculate price first
            ItemStack tempStack = randomStack.Type.CreateStack(sellQuantity);
            int price = merchantInventory.GetSellPrice(tempStack);

            Debug.Log($"Attempting to sell {sellQuantity}x {randomStack.Type.Name} for {price} gold");

            // Create and execute sell command
            SellItemCommand sellCommand = new SellItemCommand(
                merchantInventory: merchantInventory,
                playerInventory: playerInventory,
                itemID: randomStack.Type.ItemID,
                quantity: sellQuantity
            );

            if (sellCommand.CanExecute())
            {
                bool success = sellCommand.Execute();

                if (success)
                {
                    commandHistory.AddCommand(sellCommand);
                    playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
                    Debug.Log($"<color=green>Sale successful!</color> New gold: {playerGold}");
                }
                else
                {
                    Debug.LogError("Sale failed during execution");
                }
            }
            else
            {
                Debug.LogWarning("Cannot sell this item - validation failed (check if item can be sold/merchant can afford)");
            }
        }

        private void RestockMerchant()
        {
            Debug.Log("Restocking merchant inventory...");
            merchantInventory.Restock();
        }

        private void AddRandomTestItem()
        {
            if (testItems == null || testItems.Length == 0)
            {
                Debug.LogWarning("No test items assigned");
                return;
            }

            ItemType randomItem = testItems[Random.Range(0, testItems.Length)];
            ItemStack stack = randomItem.CreateStack(Random.Range(1, 5));
            bool success = playerInventory.TryAddItem(stack, out int remaining);

            if (success)
            {
                Debug.Log($"Added {stack.Quantity - remaining}x {randomItem.Name} to player inventory");
            }
        }

        private void UndoLastTransaction()
        {
            if (commandHistory.CanUndo())
            {
                commandHistory.Undo();
                playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
                Debug.Log($"<color=yellow>Transaction undone</color>. Gold: {playerGold}");
            }
            else
            {
                Debug.LogWarning("Nothing to undo");
            }
        }

        private void RedoLastTransaction()
        {
            if (commandHistory.CanRedo())
            {
                commandHistory.Redo();
                playerGold = PlayerPrefs.GetInt("PlayerGold", 0);
                Debug.Log($"<color=yellow>Transaction redone</color>. Gold: {playerGold}");
            }
            else
            {
                Debug.LogWarning("Nothing to redo");
            }
        }

        private void ToggleInfiniteStock()
        {
            merchantInventory.SetInfiniteStock(!merchantInventory.InfiniteStock);
            Debug.Log($"Infinite stock: <color=cyan>{(merchantInventory.InfiniteStock ? "ON" : "OFF")}</color>");
        }

        private void AdjustPrices()
        {
            // Cycle through different price configurations
            float newBuyMultiplier = buyPriceMultiplier + 0.1f;
            if (newBuyMultiplier > 2.0f) newBuyMultiplier = 1.0f;

            float newSellMultiplier = sellPriceMultiplier + 0.1f;
            if (newSellMultiplier > 1.0f) newSellMultiplier = 0.3f;

            merchantInventory.SetPriceMultipliers(newBuyMultiplier, newSellMultiplier);
            buyPriceMultiplier = newBuyMultiplier;
            sellPriceMultiplier = newSellMultiplier;

            Debug.Log($"<color=cyan>Prices adjusted:</color> Buy {buyPriceMultiplier:F1}x, Sell {sellPriceMultiplier:F1}x");
        }

        #endregion

        #region Event Handlers

        private void OnItemSold(ItemStack stack, int price)
        {
            Debug.Log($"<color=green>[Merchant Event]</color> Sold {stack.Quantity}x {stack.Type.Name} for {price} gold");
        }

        private void OnItemBought(ItemStack stack, int price)
        {
            Debug.Log($"<color=green>[Merchant Event]</color> Bought {stack.Quantity}x {stack.Type.Name} for {price} gold");
        }

        private void OnMerchantGoldChanged(int newGoldAmount)
        {
            Debug.Log($"<color=yellow>[Merchant Event]</color> Gold changed to {newGoldAmount}");
        }

        private void OnMerchantRestocked()
        {
            Debug.Log($"<color=cyan>[Merchant Event]</color> Inventory restocked!");
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.Box("Merchant System Example");

            // Player info
            GUILayout.Label("<b>Player</b>");
            GUILayout.Label($"Gold: {playerGold}");
            GUILayout.Label($"Items: {playerInventory.SlotCount - playerInventory.GetEmptySlotCount()}/{playerInventory.SlotCount} slots");
            GUILayout.Label($"Weight: {playerInventory.GetTotalWeight():F1}/{playerInventory.MaxWeight}");

            GUILayout.Space(10);

            // Merchant info
            GUILayout.Label("<b>Merchant</b>");
            GUILayout.Label($"Gold: {merchantInventory.MerchantGold}");
            GUILayout.Label($"Items: {merchantInventory.SlotCount - merchantInventory.GetEmptySlotCount()}/{merchantInventory.SlotCount} slots");
            GUILayout.Label($"Infinite Stock: {(merchantInventory.InfiniteStock ? "YES" : "NO")}");
            GUILayout.Label($"Buy Price: {buyPriceMultiplier:F1}x");
            GUILayout.Label($"Sell Price: {sellPriceMultiplier:F1}x");

            GUILayout.Space(10);

            // Command history
            GUILayout.Label("<b>Command History</b>");
            GUILayout.Label($"Can Undo: {(commandHistory.CanUndo() ? "YES" : "NO")}");
            GUILayout.Label($"Can Redo: {(commandHistory.CanRedo() ? "YES" : "NO")}");

            GUILayout.Space(10);

            // Buttons
            if (GUILayout.Button("Buy Random Item (B)"))
            {
                BuyRandomItem();
            }

            if (GUILayout.Button("Sell Random Item (S)"))
            {
                SellRandomItem();
            }

            if (GUILayout.Button("Restock Merchant (R)"))
            {
                RestockMerchant();
            }

            if (GUILayout.Button("Add 100 Gold (G)"))
            {
                playerGold += 100;
                PlayerPrefs.SetInt("PlayerGold", playerGold);
            }

            if (GUILayout.Button("Add Test Item (T)"))
            {
                AddRandomTestItem();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Undo Transaction (U)"))
            {
                UndoLastTransaction();
            }

            if (GUILayout.Button("Redo Transaction (Y)"))
            {
                RedoLastTransaction();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Toggle Infinite Stock (M)"))
            {
                ToggleInfiniteStock();
            }

            if (GUILayout.Button("Adjust Prices (P)"))
            {
                AdjustPrices();
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
