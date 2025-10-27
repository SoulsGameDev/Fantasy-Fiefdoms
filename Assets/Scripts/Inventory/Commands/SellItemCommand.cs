using System;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Guards;
using UnityEngine;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for selling items to a merchant.
    /// Handles gold transaction, inventory transfer, and undo/redo.
    /// </summary>
    public class SellItemCommand : CommandBase
    {
        private readonly MerchantInventory merchantInventory;
        private readonly Inventory playerInventory;
        private readonly string itemID;
        private readonly int quantity;
        private readonly List<ITransitionGuard> guards;

        // State tracking for undo
        private int priceReceived;
        private ItemStack soldStack;
        private int playerGoldBefore;
        private int merchantGoldBefore;

        public override string Description =>
            $"Sell {quantity}x {itemID} to {merchantInventory.InventoryID}";

        /// <summary>
        /// Creates a new SellItemCommand.
        /// </summary>
        public SellItemCommand(
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
                new CanSellGuard()
            };
        }

        public override bool CanExecute()
        {
            if (isExecuted) return false;
            if (merchantInventory == null || playerInventory == null) return false;
            if (string.IsNullOrEmpty(itemID)) return false;
            if (quantity <= 0) return false;

            // Check player has item
            if (!playerInventory.ContainsItem(itemID, quantity))
            {
                Debug.LogWarning($"Player doesn't have {quantity}x {itemID}");
                return false;
            }

            // Get item type
            ItemType itemType = InventoryManager.Instance?.FindItemType(itemID);
            if (itemType == null)
            {
                Debug.LogWarning($"Item '{itemID}' not found");
                return false;
            }

            // Check if item can be sold
            if (!itemType.CanSell)
            {
                Debug.LogWarning($"{itemType.Name} cannot be sold");
                return false;
            }

            // Calculate price
            ItemStack tempStack = itemType.CreateStack(quantity);
            int price = merchantInventory.GetSellPrice(tempStack);

            // Check merchant can afford
            if (!merchantInventory.CanAfford(price))
            {
                Debug.LogWarning($"Merchant cannot afford {price} gold");
                return false;
            }

            // Create context for guards
            var context = new InventoryGuardContext(playerInventory, tempStack, InventoryOperation.Sell)
            {
                TargetInventory = merchantInventory
            };

            // Evaluate guards
            foreach (var guard in guards)
            {
                GuardResult result = guard.Evaluate(context);
                if (!result.Success)
                {
                    Debug.LogWarning($"SellItemCommand guard failed: {result.Reason}");
                    return false;
                }
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning("SellItemCommand cannot execute - validation failed");
                return false;
            }

            try
            {
                // Save state for undo
                playerGoldBefore = GetPlayerGold();
                merchantGoldBefore = merchantInventory.MerchantGold;

                // Get item type
                ItemType itemType = InventoryManager.Instance?.FindItemType(itemID);
                if (itemType == null)
                {
                    Debug.LogError($"Item '{itemID}' not found");
                    return false;
                }

                // Create stack to sell
                soldStack = itemType.CreateStack(quantity);

                // Remove from player inventory
                bool removeSuccess = playerInventory.TryRemoveItem(itemID, quantity, out int removed);

                if (!removeSuccess || removed != quantity)
                {
                    Debug.LogError($"Failed to remove items from player inventory");
                    return false;
                }

                // Perform the sale
                int playerGold = playerGoldBefore;
                bool success = merchantInventory.BuyFromPlayer(soldStack, ref playerGold, out priceReceived);

                if (!success)
                {
                    Debug.LogError("Merchant purchase failed");
                    // Rollback - add items back to player
                    playerInventory.TryAddItem(soldStack, out _);
                    return false;
                }

                // Update player gold
                SetPlayerGold(playerGold);

                isExecuted = true;
                Debug.Log($"Sold {quantity}x {itemID} for {priceReceived} gold");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SellItemCommand Execute failed: {e.Message}");
                return false;
            }
        }

        public override bool Undo()
        {
            if (!CanUndo())
            {
                Debug.LogWarning("SellItemCommand cannot undo");
                return false;
            }

            try
            {
                // Remove items from merchant (if not infinite)
                if (!merchantInventory.InfiniteStock)
                {
                    merchantInventory.TryRemoveItem(itemID, quantity, out _);
                }

                // Return items to player
                playerInventory.TryAddItem(soldStack, out _);

                // Restore gold
                SetPlayerGold(playerGoldBefore);
                merchantInventory.SetGold(merchantGoldBefore);

                isExecuted = false;
                Debug.Log($"Undid sale of {quantity}x {itemID}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SellItemCommand Undo failed: {e.Message}");
                return false;
            }
        }

        #region Gold Management

        // These methods should be replaced with your actual gold management system
        private int GetPlayerGold()
        {
            // TODO: Replace with your player gold system
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
