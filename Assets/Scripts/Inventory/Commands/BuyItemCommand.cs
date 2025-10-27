using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for buying items from a merchant.
    /// Handles gold transaction, inventory transfer, and undo/redo.
    /// </summary>
    public class BuyItemCommand : CommandBase
    {
        private readonly MerchantInventory merchantInventory;
        private readonly Inventory playerInventory;
        private readonly string itemID;
        private readonly int quantity;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private int pricePaid;
        private ItemStack purchasedStack;
        private int playerGoldBefore;
        private int merchantGoldBefore;

        public override string Description =>
            $"Buy {quantity}x {itemID} from {merchantInventory.InventoryID}";

        /// <summary>
        /// Creates a new BuyItemCommand.
        /// </summary>
        /// <param name="merchantInventory">The merchant to buy from</param>
        /// <param name="playerInventory">The player's inventory</param>
        /// <param name="itemID">The item ID to buy</param>
        /// <param name="quantity">Quantity to buy</param>
        /// <param name="playerGold">Reference to player's gold amount</param>
        public BuyItemCommand(
            MerchantInventory merchantInventory,
            Inventory playerInventory,
            string itemID,
            int quantity)
        {
            this.merchantInventory = merchantInventory;
            this.playerInventory = playerInventory;
            this.itemID = itemID;
            this.quantity = quantity;

            // Setup guards
            this.guards = new List<ITransitionGuard>
            {
                new CanBuyGuard()
            };
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (merchantInventory == null || playerInventory == null) return false;
            if (string.IsNullOrEmpty(itemID)) return false;
            if (quantity <= 0) return false;

            // Get item type
            ItemType itemType = InventoryManager.Instance?.FindItemType(itemID);
            if (itemType == null)
            {
                Debug.LogWarning($"Item '{itemID}' not found");
                return false;
            }

            // Check merchant has item (if not infinite stock)
            if (!merchantInventory.InfiniteStock && !merchantInventory.ContainsItem(itemID, quantity))
            {
                Debug.LogWarning($"Merchant doesn't have {quantity}x {itemID}");
                return false;
            }

            // Calculate price
            ItemStack tempStack = itemType.CreateStack(quantity);
            int price = merchantInventory.GetBuyPrice(tempStack);

            // Create context for guards
            var context = new InventoryGuardContext(playerInventory, tempStack, InventoryOperation.Buy)
            {
                TargetInventory = merchantInventory
            };

            // Evaluate guards
            foreach (var guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"BuyItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("BuyItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Save state for undo
                playerGoldBefore = GetPlayerGold();
                merchantGoldBefore = merchantInventory.MerchantGold;

                // Perform the purchase
                int playerGold = playerGoldBefore;
                bool success = merchantInventory.SellToPlayer(
                    itemID,
                    quantity,
                    ref playerGold,
                    out pricePaid,
                    out purchasedStack);

                if (!success)
                {
                    Debug.LogError("Merchant sale failed");
                    return false;
                }

                // Update player gold
                SetPlayerGold(playerGold);

                // Add items to player inventory
                bool addSuccess = playerInventory.TryAddItem(purchasedStack, out int remaining);

                if (!addSuccess || remaining > 0)
                {
                    Debug.LogError($"Failed to add purchased items. {remaining} remaining.");
                    // Rollback
                    SetPlayerGold(playerGoldBefore);
                    merchantInventory.SetGold(merchantGoldBefore);
                    if (!merchantInventory.InfiniteStock)
                    {
                        merchantInventory.TryAddItem(purchasedStack, out _);
                    }
                    return false;
                }

                isExecuted = true;
                Debug.Log($"Purchased {quantity}x {itemID} for {pricePaid} gold");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"BuyItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("BuyItemCommand cannot undo");
                return false;
            }

            try
            {
                // Remove items from player
                playerInventory.TryRemoveItem(itemID, quantity, out _);

                // Return items to merchant (if not infinite)
                if (!merchantInventory.InfiniteStock)
                {
                    merchantInventory.TryAddItem(purchasedStack, out _);
                }

                // Restore gold
                SetPlayerGold(playerGoldBefore);
                merchantInventory.SetGold(merchantGoldBefore);

                isExecuted = false;
                Debug.Log($"Undid purchase of {quantity}x {itemID}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"BuyItemCommand Undo failed: {e.Message}");
                return false;
            }
        }

        #region Gold Management

        // These methods should be replaced with your actual gold management system
        private int GetPlayerGold()
        {
            // TODO: Replace with your player gold system
            // For now, we'll use a simple static variable or PlayerPrefs
            return PlayerPrefs.GetInt("PlayerGold", 1000);
        }

        private void SetPlayerGold(int amount)
        {
            // TODO: Replace with your player gold system
            PlayerPrefs.SetInt("PlayerGold", amount);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
