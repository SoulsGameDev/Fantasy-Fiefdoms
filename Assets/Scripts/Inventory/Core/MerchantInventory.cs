using System;
using System.Collections.Generic;
using Inventory.Data;
using UnityEngine;

namespace Inventory.Core
{
    /// <summary>
    /// Specialized inventory for merchants with buy/sell pricing.
    /// Supports infinite stock, price multipliers, and restock functionality.
    /// </summary>
    [Serializable]
    public class MerchantInventory : Inventory
    {
        [Header("Merchant Settings")]
        [SerializeField] private bool infiniteStock = false;
        [SerializeField] private float buyPriceMultiplier = 1.0f;
        [SerializeField] private float sellPriceMultiplier = 0.5f;
        [SerializeField] private int merchantGold = 1000;

        [Header("Restock Settings")]
        [SerializeField] private bool autoRestock = false;
        [SerializeField] private float restockIntervalSeconds = 3600f; // 1 hour
        [SerializeField] private List<RestockItem> restockItems = new List<RestockItem>();

        private float lastRestockTime;

        #region Events

        /// <summary>Fired when a player buys an item</summary>
        public event Action<ItemStack, int> OnItemSold;

        /// <summary>Fired when a player sells an item to merchant</summary>
        public event Action<ItemStack, int> OnItemBought;

        /// <summary>Fired when merchant gold changes</summary>
        public event Action<int> OnGoldChanged;

        /// <summary>Fired when inventory restocks</summary>
        public event Action OnRestocked;

        #endregion

        #region Properties

        /// <summary>Whether this merchant has infinite stock</summary>
        public bool InfiniteStock => infiniteStock;

        /// <summary>Price multiplier when player buys from merchant</summary>
        public float BuyPriceMultiplier => buyPriceMultiplier;

        /// <summary>Price multiplier when player sells to merchant</summary>
        public float SellPriceMultiplier => sellPriceMultiplier;

        /// <summary>Current merchant gold</summary>
        public int MerchantGold => merchantGold;

        /// <summary>Whether auto-restock is enabled</summary>
        public bool AutoRestock => autoRestock;

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new merchant inventory.
        /// </summary>
        public MerchantInventory(
            string merchantID,
            int maxSlots,
            int startingGold = 1000,
            float buyMultiplier = 1.0f,
            float sellMultiplier = 0.5f,
            bool infiniteStock = false)
            : base(merchantID, maxSlots, maxWeight: -1f)
        {
            this.merchantGold = startingGold;
            this.buyPriceMultiplier = buyMultiplier;
            this.sellPriceMultiplier = sellMultiplier;
            this.infiniteStock = infiniteStock;
            this.lastRestockTime = Time.time;
        }

        #endregion

        #region Pricing

        /// <summary>
        /// Gets the price for a player to buy an item from this merchant.
        /// </summary>
        public int GetBuyPrice(ItemStack stack)
        {
            if (stack.IsEmpty || stack.Type == null)
                return 0;

            return Mathf.CeilToInt(stack.Type.Value * stack.Quantity * buyPriceMultiplier);
        }

        /// <summary>
        /// Gets the price the merchant will pay for an item.
        /// </summary>
        public int GetSellPrice(ItemStack stack)
        {
            if (stack.IsEmpty || stack.Type == null)
                return 0;

            return Mathf.FloorToInt(stack.Type.Value * stack.Quantity * sellPriceMultiplier);
        }

        /// <summary>
        /// Gets the per-unit buy price.
        /// </summary>
        public int GetUnitBuyPrice(ItemType itemType)
        {
            if (itemType == null)
                return 0;

            return Mathf.CeilToInt(itemType.Value * buyPriceMultiplier);
        }

        /// <summary>
        /// Gets the per-unit sell price.
        /// </summary>
        public int GetUnitSellPrice(ItemType itemType)
        {
            if (itemType == null)
                return 0;

            return Mathf.FloorToInt(itemType.Value * sellPriceMultiplier);
        }

        #endregion

        #region Merchant Operations

        /// <summary>
        /// Sells an item to a player (player buys from merchant).
        /// </summary>
        /// <param name="itemID">Item to sell</param>
        /// <param name="quantity">Quantity to sell</param>
        /// <param name="playerGold">Player's current gold</param>
        /// <param name="price">Total price charged</param>
        /// <returns>True if sale successful</returns>
        public bool SellToPlayer(string itemID, int quantity, ref int playerGold, out int price, out ItemStack soldStack)
        {
            soldStack = ItemStack.Empty;
            price = 0;

            // Find the item
            if (!ContainsItem(itemID, quantity))
            {
                if (!infiniteStock)
                {
                    Debug.LogWarning($"Merchant does not have {quantity}x {itemID}");
                    return false;
                }
            }

            // Get item type
            ItemType itemType = InventoryManager.Instance?.FindItemType(itemID);
            if (itemType == null)
            {
                Debug.LogError($"ItemType '{itemID}' not found");
                return false;
            }

            // Calculate price
            ItemStack tempStack = itemType.CreateStack(quantity);
            price = GetBuyPrice(tempStack);

            // Check player can afford
            if (playerGold < price)
            {
                Debug.LogWarning($"Player cannot afford {price} gold");
                return false;
            }

            // Deduct from merchant (if not infinite)
            if (!infiniteStock)
            {
                if (!TryRemoveItem(itemID, quantity, out int removed) || removed != quantity)
                {
                    Debug.LogError("Failed to remove item from merchant inventory");
                    return false;
                }
            }

            // Process transaction
            playerGold -= price;
            AddGold(price);
            soldStack = tempStack;

            OnItemSold?.Invoke(soldStack, price);
            return true;
        }

        /// <summary>
        /// Buys an item from a player (player sells to merchant).
        /// </summary>
        /// <param name="stack">Item stack to buy</param>
        /// <param name="playerGold">Player's gold (will be increased)</param>
        /// <param name="price">Price paid</param>
        /// <returns>True if purchase successful</returns>
        public bool BuyFromPlayer(ItemStack stack, ref int playerGold, out int price)
        {
            price = 0;

            if (stack.IsEmpty || stack.Type == null)
            {
                Debug.LogWarning("Cannot buy empty stack");
                return false;
            }

            // Check if item can be sold
            if (!stack.Type.CanSell)
            {
                Debug.LogWarning($"{stack.Type.Name} cannot be sold");
                return false;
            }

            // Calculate price
            price = GetSellPrice(stack);

            // Check merchant can afford
            if (merchantGold < price)
            {
                Debug.LogWarning($"Merchant cannot afford {price} gold");
                return false;
            }

            // Check space (if not infinite stock)
            if (!infiniteStock && !HasSpaceForItem(stack))
            {
                Debug.LogWarning("Merchant inventory is full");
                return false;
            }

            // Add to merchant inventory (if not infinite)
            if (!infiniteStock)
            {
                if (!TryAddItem(stack, out int remaining) || remaining > 0)
                {
                    Debug.LogError("Failed to add item to merchant inventory");
                    return false;
                }
            }

            // Process transaction
            RemoveGold(price);
            playerGold += price;

            OnItemBought?.Invoke(stack, price);
            return true;
        }

        #endregion

        #region Gold Management

        /// <summary>
        /// Adds gold to the merchant.
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            merchantGold += amount;
            OnGoldChanged?.Invoke(merchantGold);
        }

        /// <summary>
        /// Removes gold from the merchant.
        /// </summary>
        public void RemoveGold(int amount)
        {
            if (amount <= 0) return;

            merchantGold = Mathf.Max(0, merchantGold - amount);
            OnGoldChanged?.Invoke(merchantGold);
        }

        /// <summary>
        /// Sets the merchant's gold amount.
        /// </summary>
        public void SetGold(int amount)
        {
            merchantGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(merchantGold);
        }

        /// <summary>
        /// Checks if merchant can afford a purchase.
        /// </summary>
        public bool CanAfford(int price)
        {
            return merchantGold >= price;
        }

        #endregion

        #region Restocking

        /// <summary>
        /// Updates the merchant (call in Update loop if using auto-restock).
        /// </summary>
        public void Update()
        {
            if (!autoRestock) return;

            float currentTime = Time.time;
            if (currentTime - lastRestockTime >= restockIntervalSeconds)
            {
                Restock();
                lastRestockTime = currentTime;
            }
        }

        /// <summary>
        /// Manually triggers a restock.
        /// </summary>
        public void Restock()
        {
            if (restockItems == null || restockItems.Count == 0)
                return;

            foreach (var restockItem in restockItems)
            {
                if (restockItem.ItemType == null) continue;

                // Check current quantity
                int currentQty = GetItemQuantity(restockItem.ItemType.ItemID);

                // Restock if below minimum
                if (currentQty < restockItem.MinQuantity)
                {
                    int restockAmount = restockItem.RestockQuantity;
                    ItemStack stack = restockItem.ItemType.CreateStack(restockAmount);

                    TryAddItem(stack, out int remaining);

                    if (remaining > 0)
                    {
                        Debug.LogWarning($"Could only restock {restockAmount - remaining} of {restockItem.ItemType.Name}");
                    }
                }
            }

            OnRestocked?.Invoke();
            Debug.Log($"Merchant '{InventoryID}' restocked");
        }

        /// <summary>
        /// Adds an item to the restock list.
        /// </summary>
        public void AddRestockItem(ItemType itemType, int minQuantity, int restockQuantity)
        {
            if (itemType == null) return;

            restockItems.Add(new RestockItem
            {
                ItemType = itemType,
                MinQuantity = minQuantity,
                RestockQuantity = restockQuantity
            });
        }

        /// <summary>
        /// Clears the restock list.
        /// </summary>
        public void ClearRestockList()
        {
            restockItems.Clear();
        }

        #endregion

        #region Settings

        /// <summary>
        /// Sets the buy price multiplier.
        /// </summary>
        public void SetBuyPriceMultiplier(float multiplier)
        {
            buyPriceMultiplier = Mathf.Max(0, multiplier);
        }

        /// <summary>
        /// Sets the sell price multiplier.
        /// </summary>
        public void SetSellPriceMultiplier(float multiplier)
        {
            sellPriceMultiplier = Mathf.Max(0, multiplier);
        }

        /// <summary>
        /// Sets whether this merchant has infinite stock.
        /// </summary>
        public void SetInfiniteStock(bool infinite)
        {
            infiniteStock = infinite;
        }

        /// <summary>
        /// Sets whether auto-restock is enabled.
        /// </summary>
        public void SetAutoRestock(bool enabled, float intervalSeconds = 3600f)
        {
            autoRestock = enabled;
            restockIntervalSeconds = intervalSeconds;
        }

        #endregion
    }

    /// <summary>
    /// Configuration for restocking items.
    /// </summary>
    [Serializable]
    public class RestockItem
    {
        [Tooltip("Item type to restock")]
        public ItemType ItemType;

        [Tooltip("Minimum quantity before restocking")]
        public int MinQuantity = 1;

        [Tooltip("Quantity to add when restocking")]
        public int RestockQuantity = 5;
    }
}
