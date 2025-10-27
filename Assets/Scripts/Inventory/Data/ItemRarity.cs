namespace Inventory.Data
{
    /// <summary>
    /// Defines the rarity tiers for items.
    /// Rarity affects item power, value, and visual appearance.
    /// </summary>
    public enum ItemRarity
    {
        /// <summary>Common items - basic equipment and materials (white/gray)</summary>
        Common = 0,

        /// <summary>Uncommon items - slightly enhanced items (green)</summary>
        Uncommon = 1,

        /// <summary>Rare items - significantly powerful items (blue)</summary>
        Rare = 2,

        /// <summary>Epic items - very powerful items (purple)</summary>
        Epic = 3,

        /// <summary>Legendary items - extremely rare and powerful (orange)</summary>
        Legendary = 4,

        /// <summary>Mythic items - unique artifacts (red/pink)</summary>
        Mythic = 5
    }
}
