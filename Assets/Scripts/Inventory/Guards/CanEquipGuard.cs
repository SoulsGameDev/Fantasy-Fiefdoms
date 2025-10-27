using Inventory.Data;
using Inventory.Core;

namespace Inventory.Guards
{
    /// <summary>
    /// Guard that validates if an item can be equipped.
    /// Checks item type, slot availability, and requirements.
    /// </summary>
    public class CanEquipGuard : GuardBase
    {
        private readonly int playerLevel;
        private readonly string playerClass;

        public override string Name => "CanEquip";
        public override string Description => "Validates if an item can be equipped";

        /// <summary>
        /// Creates a CanEquipGuard with player information for requirement checks.
        /// </summary>
        /// <param name="playerLevel">Player's current level (default: 999 to skip checks)</param>
        /// <param name="playerClass">Player's class (default: empty to skip checks)</param>
        public CanEquipGuard(int playerLevel = 999, string playerClass = "")
        {
            this.playerLevel = playerLevel;
            this.playerClass = playerClass;
        }

        public override GuardResult Evaluate(GuardContext context)
        {
            // Cast to inventory-specific context
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            // Check if item stack is valid
            if (invContext.ItemStack.IsEmpty || invContext.ItemStack.Type == null)
            {
                return Deny("Item stack is invalid");
            }

            // Check if item is equipment
            if (!(invContext.ItemStack.Type is EquipmentType equipment))
            {
                return Deny("Item is not equipment");
            }

            // Check if equipment has a valid slot
            if (equipment.EquipmentSlot == EquipmentSlot.None)
            {
                return Deny("Equipment does not have a valid slot");
            }

            // Check level and class requirements
            if (!equipment.MeetsRequirements(playerLevel, playerClass, out string requirementReason))
            {
                return Deny(requirementReason);
            }

            // All checks passed
            return Allow();
        }
    }

    /// <summary>
    /// Guard that validates if an item can be unequipped.
    /// Checks if the slot has an item and if it can be removed.
    /// </summary>
    public class CanUnequipGuard : GuardBase
    {
        public override string Name => "CanUnequip";
        public override string Description => "Validates if an item can be unequipped";

        public override GuardResult Evaluate(GuardContext context)
        {
            // Cast to inventory-specific context
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            // Check if target inventory is provided (equipment inventory)
            if (invContext.TargetInventory == null)
            {
                return Deny("No equipment inventory specified");
            }

            // Additional checks can be added here
            // For example: cursed items that can't be unequipped
            // Or items that are quest-locked

            return Allow();
        }
    }
}
