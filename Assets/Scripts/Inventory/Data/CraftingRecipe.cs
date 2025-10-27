using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.Data
{
    /// <summary>
    /// Defines a crafting recipe for creating items from components.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Inventory/Crafting Recipe")]
    public class CraftingRecipe : ScriptableObject
    {
        [Header("Recipe Info")]
        [SerializeField] private string recipeID;
        [SerializeField] private string recipeName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;

        [Header("Requirements")]
        [SerializeField] private List<CraftingIngredient> ingredients = new List<CraftingIngredient>();
        [SerializeField] private int requiredCraftingLevel = 0;
        [SerializeField] private CraftingStation requiredStation = CraftingStation.None;

        [Header("Result")]
        [SerializeField] private ItemType resultItem;
        [SerializeField] private int resultQuantity = 1;
        [SerializeField] private float craftingTimeSeconds = 1f;

        [Header("Quality")]
        [SerializeField] private bool canProduceQualityVariants = false;
        [SerializeField] private float successChance = 1f; // 0-1, chance to succeed

        #region Properties

        public string RecipeID
        {
            get
            {
                if (string.IsNullOrEmpty(recipeID))
                {
                    recipeID = name;
                }
                return recipeID;
            }
        }

        public string RecipeName => recipeName;
        public string Description => description;
        public Sprite Icon => icon;
        public List<CraftingIngredient> Ingredients => ingredients;
        public int RequiredCraftingLevel => requiredCraftingLevel;
        public CraftingStation RequiredStation => requiredStation;
        public ItemType ResultItem => resultItem;
        public int ResultQuantity => resultQuantity;
        public float CraftingTimeSeconds => craftingTimeSeconds;
        public bool CanProduceQualityVariants => canProduceQualityVariants;
        public float SuccessChance => successChance;

        #endregion

        #region Validation

        /// <summary>
        /// Checks if a player can craft this recipe.
        /// </summary>
        public bool CanCraft(Core.Inventory inventory, int playerCraftingLevel, CraftingStation availableStation, out string reason)
        {
            // Check crafting level
            if (playerCraftingLevel < requiredCraftingLevel)
            {
                reason = $"Requires crafting level {requiredCraftingLevel}";
                return false;
            }

            // Check crafting station
            if (requiredStation != CraftingStation.None && availableStation != requiredStation)
            {
                reason = $"Requires {requiredStation}";
                return false;
            }

            // Check ingredients
            foreach (var ingredient in ingredients)
            {
                int available = inventory.GetItemQuantity(ingredient.ItemID);
                if (available < ingredient.Quantity)
                {
                    reason = $"Missing {ingredient.Quantity - available}x {ingredient.ItemID}";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets a list of missing ingredients.
        /// </summary>
        public List<CraftingIngredient> GetMissingIngredients(Core.Inventory inventory)
        {
            var missing = new List<CraftingIngredient>();

            foreach (var ingredient in ingredients)
            {
                int available = inventory.GetItemQuantity(ingredient.ItemID);
                int needed = ingredient.Quantity - available;

                if (needed > 0)
                {
                    missing.Add(new CraftingIngredient
                    {
                        ItemID = ingredient.ItemID,
                        Quantity = needed
                    });
                }
            }

            return missing;
        }

        #endregion

        #region Display

        /// <summary>
        /// Gets a formatted description of this recipe.
        /// </summary>
        public string GetFormattedDescription()
        {
            var desc = new System.Text.StringBuilder();

            desc.AppendLine($"<b>{recipeName}</b>");
            desc.AppendLine(description);
            desc.AppendLine();

            desc.AppendLine("<b>Ingredients:</b>");
            foreach (var ingredient in ingredients)
            {
                desc.AppendLine($"  {ingredient.Quantity}x {ingredient.ItemID}");
            }

            if (requiredCraftingLevel > 0)
            {
                desc.AppendLine($"\nRequired Level: {requiredCraftingLevel}");
            }

            if (requiredStation != CraftingStation.None)
            {
                desc.AppendLine($"Required Station: {requiredStation}");
            }

            desc.AppendLine($"\nResult: {resultQuantity}x {resultItem.Name}");
            desc.AppendLine($"Crafting Time: {craftingTimeSeconds:F1}s");

            if (successChance < 1f)
            {
                desc.AppendLine($"Success Chance: {successChance * 100:F0}%");
            }

            return desc.ToString();
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure recipe ID is set
            if (string.IsNullOrEmpty(recipeID))
            {
                recipeID = name;
            }

            // Validate success chance
            successChance = Mathf.Clamp01(successChance);

            // Validate crafting time
            if (craftingTimeSeconds < 0f)
            {
                craftingTimeSeconds = 0f;
            }
        }
#endif
    }

    /// <summary>
    /// Represents an ingredient required for crafting.
    /// </summary>
    [System.Serializable]
    public class CraftingIngredient
    {
        [Tooltip("Item ID of the ingredient")]
        public string ItemID;

        [Tooltip("Quantity required")]
        public int Quantity = 1;

        [Tooltip("Whether this ingredient is consumed (false = acts as a tool)")]
        public bool IsConsumed = true;
    }

    /// <summary>
    /// Types of crafting stations.
    /// </summary>
    public enum CraftingStation
    {
        None,           // Can craft anywhere
        Workbench,      // Basic crafting
        Forge,          // Metalworking
        Anvil,          // Advanced metalworking
        Alchemy,        // Potion brewing
        Enchanting,     // Enchanting items
        Cooking,        // Food preparation
        Tannery,        // Leather working
        Loom,           // Textile crafting
        Carpentry       // Woodworking
    }
}
