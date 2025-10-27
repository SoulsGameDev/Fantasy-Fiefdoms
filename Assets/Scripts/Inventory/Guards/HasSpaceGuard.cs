namespace Inventory.Guards
{
    /// <summary>
    /// Guard that validates if an inventory has space for new items.
    /// Checks slot availability and weight limits.
    /// </summary>
    public class HasSpaceGuard : GuardBase
    {
        public override string Name => "HasSpace";
        public override string Description => "Validates if the inventory has sufficient space";

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
            if (invContext.ItemStack.IsEmpty)
            {
                return Deny("Item stack is empty");
            }

            // Check weight limit
            if (invContext.Inventory.HasWeightLimit)
            {
                float currentWeight = invContext.Inventory.GetTotalWeight();
                float addedWeight = invContext.ItemStack.TotalWeight;
                float maxWeight = invContext.Inventory.MaxWeight;

                if (currentWeight + addedWeight > maxWeight)
                {
                    return Deny($"Not enough weight capacity. Current: {currentWeight:F1}, Adding: {addedWeight:F1}, Max: {maxWeight:F1}");
                }
            }

            // Check if there's space (either stackable with existing or empty slots)
            if (!invContext.Inventory.HasSpaceForItem(invContext.ItemStack))
            {
                return Deny("No available slots for item");
            }

            // All checks passed
            return Allow();
        }
    }
}
