namespace Inventory.Guards
{
    /// <summary>
    /// Guard that validates if an item can be added to an inventory.
    /// Checks for valid item, inventory state, and basic requirements.
    /// </summary>
    public class CanAddItemGuard : GuardBase
    {
        public override string Name => "CanAddItem";
        public override string Description => "Validates if an item can be added to the inventory";

        public override GuardResult Evaluate(GuardContext context)
        {
            // Cast to inventory-specific context
            if (!(context is InventoryGuardContext invContext))
            {
                return Deny("Invalid context type");
            }

            // Check if inventory exists
            if (invContext.Inventory == null)
            {
                return Deny("Inventory is null");
            }

            // Check if item stack is valid
            if (invContext.ItemStack.IsEmpty || !invContext.ItemStack.IsValid)
            {
                return Deny("Item stack is empty or invalid");
            }

            // Check if quantity is positive
            if (invContext.Quantity <= 0)
            {
                return Deny("Quantity must be positive");
            }

            // Check if item type exists
            if (invContext.ItemStack.Type == null)
            {
                return Deny("Item type is null");
            }

            // All checks passed
            return Allow();
        }
    }
}
