using UnityEngine;

namespace Inventory.Data
{
    /// <summary>
    /// Base ScriptableObject that defines an item type.
    /// This is the template/definition for all items in the game.
    /// Create instances in Assets/ScriptableObjects/Items/.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Type", order = 0)]
    public class ItemType : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("Unique identifier for this item type")]
        [field: SerializeField] public string ItemID { get; private set; } = "";

        [Tooltip("Display name of the item")]
        [field: SerializeField] public string Name { get; private set; } = "New Item";

        [Tooltip("Description shown in tooltips")]
        [field: SerializeField, TextArea(3, 6)] public string Description { get; private set; } = "";

        [Header("Visual")]
        [Tooltip("Icon shown in inventory UI")]
        [field: SerializeField] public Sprite Icon { get; private set; }

        [Tooltip("3D prefab for world representation (optional)")]
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [Header("Classification")]
        [Tooltip("Item category (Equipment, Consumable, Quest, etc.)")]
        [field: SerializeField] public ItemCategory Category { get; private set; } = ItemCategory.Miscellaneous;

        [Tooltip("Item rarity tier")]
        [field: SerializeField] public ItemRarity Rarity { get; private set; } = ItemRarity.Common;

        [Header("Stacking")]
        [Tooltip("Maximum stack size (1 = non-stackable, 99+ for stackable items)")]
        [field: SerializeField] public int MaxStack { get; private set; } = 1;

        [Header("Properties")]
        [Tooltip("Weight per item (used for weight-based inventories)")]
        [field: SerializeField] public float Weight { get; private set; } = 1f;

        [Tooltip("Base value/price of the item")]
        [field: SerializeField] public int Value { get; private set; } = 1;

        [Header("Durability")]
        [Tooltip("Enable durability system for this item")]
        [field: SerializeField] public bool HasDurability { get; private set; } = false;

        [Tooltip("Maximum durability (only used if HasDurability is true)")]
        [field: SerializeField] public int MaxDurability { get; private set; } = 100;

        [Header("Restrictions")]
        [Tooltip("Can this item be dropped from inventory?")]
        [field: SerializeField] public bool CanDrop { get; private set; } = true;

        [Tooltip("Can this item be sold to merchants?")]
        [field: SerializeField] public bool CanSell { get; private set; } = true;

        [Tooltip("Can this item be traded to other players?")]
        [field: SerializeField] public bool CanTrade { get; private set; } = true;

        [Tooltip("Can this item be destroyed?")]
        [field: SerializeField] public bool CanDestroy { get; private set; } = true;

        /// <summary>
        /// Gets whether this item is stackable (max stack > 1).
        /// </summary>
        public bool IsStackable => MaxStack > 1;

        /// <summary>
        /// Gets whether this item is equipment.
        /// </summary>
        public bool IsEquipment => Category == ItemCategory.Equipment;

        /// <summary>
        /// Gets whether this item is consumable.
        /// </summary>
        public bool IsConsumable => Category == ItemCategory.Consumable;

        /// <summary>
        /// Gets whether this item is a quest item.
        /// </summary>
        public bool IsQuestItem => Category == ItemCategory.Quest;

        /// <summary>
        /// Gets the color associated with this item's rarity.
        /// </summary>
        public Color RarityColor
        {
            get
            {
                return Rarity switch
                {
                    ItemRarity.Common => new Color(0.8f, 0.8f, 0.8f), // Light gray
                    ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f), // Green
                    ItemRarity.Rare => new Color(0.2f, 0.5f, 1f), // Blue
                    ItemRarity.Epic => new Color(0.7f, 0.3f, 1f), // Purple
                    ItemRarity.Legendary => new Color(1f, 0.6f, 0.2f), // Orange
                    ItemRarity.Mythic => new Color(1f, 0.2f, 0.4f), // Red/Pink
                    _ => Color.white
                };
            }
        }

        /// <summary>
        /// Validates the item configuration on creation/edit.
        /// Called automatically by Unity when values change in the inspector.
        /// </summary>
        private void OnValidate()
        {
            // Ensure ItemID is set
            if (string.IsNullOrWhiteSpace(ItemID))
            {
                ItemID = name; // Use ScriptableObject filename as default
            }

            // Ensure valid stack size
            if (MaxStack < 1)
            {
                MaxStack = 1;
            }

            // Ensure valid weight
            if (Weight < 0)
            {
                Weight = 0;
            }

            // Ensure valid value
            if (Value < 0)
            {
                Value = 0;
            }

            // Ensure valid durability
            if (HasDurability && MaxDurability < 1)
            {
                MaxDurability = 1;
            }

            // Quest items should not be droppable or sellable by default
            if (Category == ItemCategory.Quest)
            {
                CanDrop = false;
                CanSell = false;
            }
        }

        /// <summary>
        /// Creates a new item stack of this type.
        /// </summary>
        /// <param name="quantity">The quantity to create (default: 1)</param>
        /// <returns>A new ItemStack</returns>
        public ItemStack CreateStack(int quantity = 1)
        {
            int durability = HasDurability ? MaxDurability : -1;
            return new ItemStack(this, quantity, durability);
        }

        public override string ToString()
        {
            return $"{Name} ({Rarity})";
        }
    }
}
