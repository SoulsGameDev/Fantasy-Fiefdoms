namespace Inventory.Data
{
    /// <summary>
    /// Defines the high-level category of an item.
    /// Used for filtering, sorting, and determining item behavior.
    /// </summary>
    public enum ItemCategory
    {
        /// <summary>Equipment that can be worn (weapons, armor, accessories)</summary>
        Equipment = 0,

        /// <summary>Items that can be used/consumed (potions, food, scrolls)</summary>
        Consumable = 1,

        /// <summary>Quest-specific items that cannot be dropped or sold</summary>
        Quest = 2,

        /// <summary>Materials used for crafting</summary>
        Material = 3,

        /// <summary>Currency items (gold, tokens, gems)</summary>
        Currency = 4,

        /// <summary>Miscellaneous items (keys, books, etc.)</summary>
        Miscellaneous = 5
    }
}
