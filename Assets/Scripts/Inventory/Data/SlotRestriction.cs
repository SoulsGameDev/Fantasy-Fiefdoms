namespace Inventory.Data
{
    /// <summary>
    /// Defines restrictions on what can be placed in an inventory slot.
    /// Used to create specialized inventories (quest-only, equipment-only, etc.)
    /// </summary>
    public enum SlotRestriction
    {
        /// <summary>No restrictions - any item can be placed here</summary>
        None = 0,

        /// <summary>Only quest items allowed</summary>
        QuestOnly = 1,

        /// <summary>Only equipment items allowed</summary>
        EquipmentOnly = 2,

        /// <summary>Only consumable items allowed</summary>
        ConsumableOnly = 3,

        /// <summary>Only material items allowed</summary>
        MaterialOnly = 4,

        /// <summary>Only currency items allowed</summary>
        CurrencyOnly = 5
    }
}
