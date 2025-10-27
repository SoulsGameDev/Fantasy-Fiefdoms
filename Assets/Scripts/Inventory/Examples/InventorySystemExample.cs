using UnityEngine;
using Inventory.Core;
using Inventory.Data;
using Inventory.Commands;

namespace Inventory.Examples
{
    /// <summary>
    /// Example script demonstrating the inventory system usage.
    /// Attach this to a GameObject in your scene to test the inventory system.
    /// </summary>
    public class InventorySystemExample : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private ItemType testItemType; // Assign in inspector
        [SerializeField] private int testQuantity = 5;

        private Inventory.Core.Inventory playerInventory;

        private void Start()
        {
            Debug.Log("=== Inventory System Example Started ===");

            // Example 1: Create an inventory
            ExampleCreateInventory();

            // Example 2: Add items using commands
            if (testItemType != null)
            {
                ExampleAddItems();
            }
            else
            {
                Debug.LogWarning("No test item type assigned. Create an ItemType ScriptableObject and assign it in the inspector.");
            }

            // Example 3: Query inventory
            ExampleQueryInventory();

            // Example 4: Remove items
            if (testItemType != null)
            {
                ExampleRemoveItems();
            }

            Debug.Log("=== Inventory System Example Complete ===");
        }

        /// <summary>
        /// Example: Creating an inventory
        /// </summary>
        private void ExampleCreateInventory()
        {
            Debug.Log("\n--- Example 1: Creating Inventory ---");

            // Method 1: Create directly
            playerInventory = new Inventory.Core.Inventory("player_main", maxSlots: 20, maxWeight: 100f);
            Debug.Log($"Created inventory: {playerInventory.InventoryID}");
            Debug.Log($"  Max Slots: {playerInventory.MaxSlots}");
            Debug.Log($"  Max Weight: {playerInventory.MaxWeight}");

            // Method 2: Create via InventoryManager
            InventoryManager.Instance.RegisterInventory(playerInventory);
            Debug.Log($"Registered inventory with InventoryManager");

            // Method 3: Use InventoryManager helper
            Inventory.Core.Inventory secondInventory = InventoryManager.Instance.CreatePlayerInventory("player_02");
            Debug.Log($"Created second inventory via manager: {secondInventory.InventoryID}");
        }

        /// <summary>
        /// Example: Adding items using commands
        /// </summary>
        private void ExampleAddItems()
        {
            Debug.Log("\n--- Example 2: Adding Items ---");

            if (playerInventory == null || testItemType == null)
            {
                Debug.LogWarning("Inventory or test item not set up");
                return;
            }

            // Create an item stack
            ItemStack itemStack = testItemType.CreateStack(testQuantity);
            Debug.Log($"Created item stack: {itemStack}");

            // Create and execute add command
            AddItemCommand addCommand = new AddItemCommand(playerInventory, itemStack);

            if (addCommand.CanExecute())
            {
                Debug.Log("Command validation passed. Executing...");
                bool success = addCommand.Execute();

                if (success)
                {
                    Debug.Log("✓ Successfully added items to inventory");
                    Debug.Log($"  Total items: {playerInventory.GetItemQuantity(testItemType.ItemID)}");
                }
                else
                {
                    Debug.LogError("✗ Failed to add items");
                }
            }
            else
            {
                Debug.LogWarning("Command validation failed - cannot add items");
            }

            // You can also use CommandHistory for undo/redo support
            // CommandHistory.Instance.ExecuteCommand(addCommand);
        }

        /// <summary>
        /// Example: Querying inventory information
        /// </summary>
        private void ExampleQueryInventory()
        {
            Debug.Log("\n--- Example 3: Querying Inventory ---");

            if (playerInventory == null)
            {
                Debug.LogWarning("Inventory not set up");
                return;
            }

            // Get inventory statistics
            Debug.Log($"Empty slots: {playerInventory.GetEmptySlotCount()}");
            Debug.Log($"Total weight: {playerInventory.GetTotalWeight():F2}/{playerInventory.MaxWeight}");
            Debug.Log($"Total value: {playerInventory.GetTotalValue()}");

            // Check for specific items
            if (testItemType != null)
            {
                int quantity = playerInventory.GetItemQuantity(testItemType.ItemID);
                Debug.Log($"Quantity of {testItemType.Name}: {quantity}");

                bool hasEnough = playerInventory.ContainsItem(testItemType.ItemID, 3);
                Debug.Log($"Has at least 3 {testItemType.Name}: {hasEnough}");
            }

            // List all items
            var allItems = playerInventory.GetAllItems();
            Debug.Log($"\nAll items in inventory ({allItems.Count} stacks):");
            foreach (var stack in allItems)
            {
                Debug.Log($"  - {stack}");
            }

            // Inspect individual slots
            Debug.Log("\nSlot contents:");
            for (int i = 0; i < Mathf.Min(5, playerInventory.SlotCount); i++)
            {
                InventorySlot slot = playerInventory.GetSlot(i);
                if (!slot.IsEmpty)
                {
                    Debug.Log($"  Slot {i}: {slot}");
                }
            }
        }

        /// <summary>
        /// Example: Removing items using commands
        /// </summary>
        private void ExampleRemoveItems()
        {
            Debug.Log("\n--- Example 4: Removing Items ---");

            if (playerInventory == null || testItemType == null)
            {
                Debug.LogWarning("Inventory or test item not set up");
                return;
            }

            // Remove some items
            int removeQuantity = 2;
            RemoveItemCommand removeCommand = new RemoveItemCommand(
                playerInventory,
                testItemType.ItemID,
                removeQuantity
            );

            if (removeCommand.CanExecute())
            {
                Debug.Log($"Removing {removeQuantity}x {testItemType.Name}...");
                bool success = removeCommand.Execute();

                if (success)
                {
                    Debug.Log("✓ Successfully removed items");
                    Debug.Log($"  Remaining: {playerInventory.GetItemQuantity(testItemType.ItemID)}");
                }
                else
                {
                    Debug.LogError("✗ Failed to remove items");
                }
            }
            else
            {
                Debug.LogWarning("Command validation failed - cannot remove items");
            }
        }

        /// <summary>
        /// Example: Using InventoryManager for global operations
        /// </summary>
        private void OnGUI()
        {
            if (GUILayout.Button("Debug: Log All Inventories"))
            {
                InventoryManager.Instance.DebugLogInventories();
            }

            if (testItemType != null && playerInventory != null)
            {
                if (GUILayout.Button($"Add {testItemType.Name}"))
                {
                    ItemStack stack = testItemType.CreateStack(1);
                    AddItemCommand cmd = new AddItemCommand(playerInventory, stack);
                    cmd.Execute();
                }

                if (GUILayout.Button($"Remove {testItemType.Name}"))
                {
                    RemoveItemCommand cmd = new RemoveItemCommand(playerInventory, testItemType.ItemID, 1);
                    cmd.Execute();
                }
            }
        }
    }
}
