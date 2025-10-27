namespace Inventory.Data
{
    /// <summary>
    /// Defines the equipment slots where items can be equipped.
    /// Each slot has specific restrictions on what types of items can be placed there.
    /// </summary>
    public enum EquipmentSlot
    {
        /// <summary>No slot / not equipment</summary>
        None = 0,

        /// <summary>Head slot - helmets, hats, crowns</summary>
        Head = 1,

        /// <summary>Chest slot - armor, robes, shirts</summary>
        Chest = 2,

        /// <summary>Legs slot - pants, greaves, skirts</summary>
        Legs = 3,

        /// <summary>Hands slot - gloves, gauntlets</summary>
        Hands = 4,

        /// <summary>Feet slot - boots, shoes, sandals</summary>
        Feet = 5,

        /// <summary>Main hand slot - primary weapon, tool, or shield</summary>
        MainHand = 6,

        /// <summary>Off hand slot - secondary weapon, shield, or tool</summary>
        OffHand = 7,

        /// <summary>Accessory slot 1 - rings, amulets, trinkets</summary>
        Accessory1 = 8,

        /// <summary>Accessory slot 2 - rings, amulets, trinkets</summary>
        Accessory2 = 9,

        /// <summary>Accessory slot 3 - rings, amulets, trinkets</summary>
        Accessory3 = 10,

        /// <summary>Back slot - cloaks, capes</summary>
        Back = 11,

        /// <summary>Belt slot - belts, sashes</summary>
        Belt = 12
    }
}
