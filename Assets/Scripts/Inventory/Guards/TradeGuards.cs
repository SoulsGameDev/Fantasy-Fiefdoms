using Inventory.Core;
using Inventory.Data;
using UnityEngine;

namespace Inventory.Guards
{
    /// <summary>
    /// Guard that validates if a player can buy an item from a merchant.
    /// </summary>
    public class CanBuyGuard : GuardBase
    {
        public override string Name => "CanBuy";
        public override string Description => "Validates if an item can be bought from a merchant";

        public override GuardResult Evaluate(GuardContext context)
        {
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            // Check if we have necessary inventories
            if (invContext.Inventory == null)
            {
                return Deny("Player inventory is null");
            }

            if (invContext.TargetInventory == null || !(invContext.TargetInventory is MerchantInventory))
            {
                return Deny("Target inventory is not a merchant");
            }

            MerchantInventory merchant = invContext.TargetInventory as MerchantInventory;

            // Check if item exists
            if (invContext.ItemStack.IsEmpty || invContext.ItemStack.Type == null)
            {
                return Deny("Invalid item");
            }

            // Check if merchant has stock (if not infinite)
            if (!merchant.InfiniteStock)
            {
                if (!merchant.ContainsItem(invContext.ItemStack.Type.ItemID, invContext.Quantity))
                {
                    return Deny("Merchant does not have enough stock");
                }
            }

            // Check if player can afford
            int price = merchant.GetBuyPrice(invContext.ItemStack);
            int playerGold = PlayerPrefs.GetInt("PlayerGold", 0); // TODO: Replace with actual gold system

            if (playerGold < price)
            {
                return Deny($"Not enough gold. Need: {price}, Have: {playerGold}");
            }

            // Check if player has inventory space
            if (!invContext.Inventory.HasSpaceForItem(invContext.ItemStack))
            {
                return Deny("Not enough inventory space");
            }

            return Allow();
        }
    }

    /// <summary>
    /// Guard that validates if a player can sell an item to a merchant.
    /// </summary>
    public class CanSellGuard : GuardBase
    {
        public override string Name => "CanSell";
        public override string Description => "Validates if an item can be sold to a merchant";

        public override GuardResult Evaluate(GuardContext context)
        {
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            // Check if we have necessary inventories
            if (invContext.Inventory == null)
            {
                return Deny("Player inventory is null");
            }

            if (invContext.TargetInventory == null || !(invContext.TargetInventory is MerchantInventory))
            {
                return Deny("Target inventory is not a merchant");
            }

            MerchantInventory merchant = invContext.TargetInventory as MerchantInventory;

            // Check if item exists
            if (invContext.ItemStack.IsEmpty || invContext.ItemStack.Type == null)
            {
                return Deny("Invalid item");
            }

            // Check if item can be sold
            if (!invContext.ItemStack.Type.CanSell)
            {
                return Deny($"{invContext.ItemStack.Type.Name} cannot be sold");
            }

            // Check if player has the item
            if (!invContext.Inventory.ContainsItem(invContext.ItemStack.Type.ItemID, invContext.Quantity))
            {
                return Deny("Player does not have this item");
            }

            // Check if merchant can afford
            int price = merchant.GetSellPrice(invContext.ItemStack);
            if (!merchant.CanAfford(price))
            {
                return Deny($"Merchant cannot afford this ({price} gold)");
            }

            // Check if merchant has space (if not infinite stock)
            if (!merchant.InfiniteStock && !merchant.HasSpaceForItem(invContext.ItemStack))
            {
                return Deny("Merchant inventory is full");
            }

            return Allow();
        }
    }

    /// <summary>
    /// Guard that validates if a container can be opened.
    /// </summary>
    public class CanOpenContainerGuard : GuardBase
    {
        private readonly Inventory playerInventory;

        public override string Name => "CanOpenContainer";
        public override string Description => "Validates if a container can be opened";

        public CanOpenContainerGuard(Inventory playerInventory = null)
        {
            this.playerInventory = playerInventory;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            if (invContext.TargetInventory == null || !(invContext.TargetInventory is ContainerInventory))
            {
                return Deny("Target is not a container");
            }

            ContainerInventory container = invContext.TargetInventory as ContainerInventory;

            // Check if locked
            if (container.IsLocked)
            {
                // Check for key
                if (!string.IsNullOrEmpty(container.RequiredKeyID) && playerInventory != null)
                {
                    if (!playerInventory.ContainsItem(container.RequiredKeyID, 1))
                    {
                        return Deny($"Requires key: {container.RequiredKeyID}");
                    }
                }
                else
                {
                    return Deny($"Container is locked (Level {container.LockLevel})");
                }
            }

            return Allow();
        }
    }
}
