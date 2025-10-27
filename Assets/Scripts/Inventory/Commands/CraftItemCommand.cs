using UnityEngine;
using Inventory.Core;
using Inventory.Data;
using System.Collections.Generic;

namespace Inventory.Commands
{
    /// <summary>
    /// Command for crafting items from a recipe.
    /// Handles ingredient consumption, result creation, and undo/redo.
    /// </summary>
    public class CraftItemCommand : CommandBase
    {
        private readonly Inventory.Core.Inventory inventory;
        private readonly CraftingRecipe recipe;
        private readonly int playerCraftingLevel;
        private readonly CraftingStation availableStation;

        // For undo
        private List<ItemStack> consumedIngredients = new List<ItemStack>();
        private ItemStack craftedResult;
        private bool craftingSucceeded;

        /// <summary>
        /// Creates a new craft item command.
        /// </summary>
        public CraftItemCommand(
            Inventory.Core.Inventory inventory,
            CraftingRecipe recipe,
            int playerCraftingLevel = 0,
            CraftingStation availableStation = CraftingStation.None)
        {
            this.inventory = inventory;
            this.recipe = recipe;
            this.playerCraftingLevel = playerCraftingLevel;
            this.availableStation = availableStation;
        }

        public override bool CanExecute()
        {
            if (inventory == null)
            {
                ErrorMessage = "Inventory is null";
                return false;
            }

            if (recipe == null)
            {
                ErrorMessage = "Recipe is null";
                return false;
            }

            // Check if recipe can be crafted
            if (!recipe.CanCraft(inventory, playerCraftingLevel, availableStation, out string reason))
            {
                ErrorMessage = reason;
                return false;
            }

            return true;
        }

        public override bool Execute()
        {
            if (!CanExecute())
            {
                Debug.LogWarning($"Cannot craft: {ErrorMessage}");
                return false;
            }

            try
            {
                // Store consumed ingredients for undo
                consumedIngredients.Clear();

                // Consume ingredients
                foreach (var ingredient in recipe.Ingredients)
                {
                    if (ingredient.IsConsumed)
                    {
                        // Find and store the ingredient
                        ItemType ingredientType = InventoryManager.Instance.FindItemType(ingredient.ItemID);
                        if (ingredientType != null)
                        {
                            ItemStack consumedStack = ingredientType.CreateStack(ingredient.Quantity);
                            consumedIngredients.Add(consumedStack);
                        }

                        // Remove from inventory
                        bool removed = inventory.TryRemoveItem(ingredient.ItemID, ingredient.Quantity, out int actuallyRemoved);
                        if (!removed || actuallyRemoved != ingredient.Quantity)
                        {
                            ErrorMessage = $"Failed to consume ingredient {ingredient.ItemID}";
                            // Rollback - restore what we've consumed so far
                            RollbackIngredients();
                            return false;
                        }
                    }
                }

                // Determine success (if recipe has a chance to fail)
                craftingSucceeded = Random.value <= recipe.SuccessChance;

                if (craftingSucceeded)
                {
                    // Create result item
                    ItemStack resultStack = recipe.ResultItem.CreateStack(recipe.ResultQuantity);

                    // Apply quality variant if enabled
                    if (recipe.CanProduceQualityVariants)
                    {
                        // TODO: Create ItemInstance with random quality based on crafting level
                        // For now, just create a normal stack
                    }

                    craftedResult = resultStack;

                    // Add to inventory
                    bool added = inventory.TryAddItem(resultStack, out int remaining);
                    if (!added || remaining > 0)
                    {
                        ErrorMessage = "Failed to add crafted item - inventory full";
                        // Rollback
                        RollbackIngredients();
                        return false;
                    }

                    Debug.Log($"Successfully crafted {recipe.ResultQuantity}x {recipe.ResultItem.Name}");
                }
                else
                {
                    // Crafting failed - ingredients were consumed but nothing was produced
                    Debug.Log($"Crafting failed for {recipe.RecipeName}");
                }

                return true;
            }
            catch (System.Exception ex)
            {
                ErrorMessage = $"Exception while crafting: {ex.Message}";
                Debug.LogError(ErrorMessage);
                RollbackIngredients();
                return false;
            }
        }

        public override bool Undo()
        {
            if (consumedIngredients.Count == 0)
            {
                Debug.LogWarning("No ingredients to restore");
                return false;
            }

            // Remove crafted item if crafting succeeded
            if (craftingSucceeded && !craftedResult.IsEmpty)
            {
                bool removed = inventory.TryRemoveItem(
                    craftedResult.Type.ItemID,
                    craftedResult.Quantity,
                    out int actuallyRemoved);

                if (!removed || actuallyRemoved != craftedResult.Quantity)
                {
                    Debug.LogWarning("Failed to remove crafted item during undo");
                    return false;
                }
            }

            // Restore consumed ingredients
            return RollbackIngredients();
        }

        private bool RollbackIngredients()
        {
            bool allRestored = true;

            foreach (var ingredient in consumedIngredients)
            {
                bool restored = inventory.TryAddItem(ingredient, out int remaining);
                if (!restored || remaining > 0)
                {
                    Debug.LogWarning($"Failed to restore ingredient {ingredient.Type.Name}");
                    allRestored = false;
                }
            }

            return allRestored;
        }
    }
}
