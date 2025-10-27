using UnityEngine;
using Inventory.Core;
using Inventory.Data;
using Inventory.UI;

namespace Inventory.Examples
{
    /// <summary>
    /// Example demonstrating the complete UI system.
    /// Shows inventory panel, equipment panel, tooltips, and drag-and-drop.
    ///
    /// Setup Instructions:
    /// 1. Create a Canvas in your scene
    /// 2. Add this script to an empty GameObject
    /// 3. Create UI prefabs for ItemSlotUI and ItemTooltip
    /// 4. Create InventoryPanel and EquipmentPanel in your Canvas
    /// 5. Assign references in the inspector
    /// 6. Create some test ItemType and EquipmentType ScriptableObjects
    /// 7. Press Play and use keyboard shortcuts to test
    ///
    /// Keyboard Shortcuts:
    /// - I: Toggle inventory
    /// - E: Toggle equipment
    /// - T: Add test item
    /// - R: Remove random item
    /// - Q: Equip random weapon
    /// </summary>
    public class InventoryUIExample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private InventoryPanel inventoryPanel;
        [SerializeField] private EquipmentPanel equipmentPanel;
        [SerializeField] private ItemTooltip tooltip;

        [Header("Test Items")]
        [SerializeField] private ItemType[] testItems;
        [SerializeField] private EquipmentType[] testEquipment;

        // Game data
        private Inventory.Core.Inventory playerBag;
        private EquipmentInventory playerEquipment;

        #region Initialization

        private void Start()
        {
            Debug.Log("=== Inventory UI Example Started ===");
            Debug.Log("Keyboard Shortcuts:");
            Debug.Log("  I - Toggle Inventory");
            Debug.Log("  E - Toggle Equipment");
            Debug.Log("  T - Add Test Item");
            Debug.Log("  R - Remove Random Item");
            Debug.Log("  Q - Equip Random Weapon");

            SetupInventories();
            SetupUI();
            AddTestItems();
        }

        private void SetupInventories()
        {
            // Create player bag
            playerBag = new Inventory.Core.Inventory("player_bag", maxSlots: 20, maxWeight: 100f);
            InventoryManager.Instance.RegisterInventory(playerBag);

            // Create equipment inventory
            playerEquipment = new EquipmentInventory("player_equipment", gameObject);

            Debug.Log("Inventories created");
        }

        private void SetupUI()
        {
            // Initialize inventory panel
            if (inventoryPanel != null)
            {
                inventoryPanel.Initialize(playerBag, "Player Inventory");
                Debug.Log("Inventory panel initialized");
            }
            else
            {
                Debug.LogWarning("Inventory panel not assigned! Create one in your Canvas.");
            }

            // Initialize equipment panel
            if (equipmentPanel != null)
            {
                equipmentPanel.Initialize(playerEquipment, "Equipment");
                Debug.Log("Equipment panel initialized");
            }
            else
            {
                Debug.LogWarning("Equipment panel not assigned! Create one in your Canvas.");
            }

            // Note: DragDropManager is a Singleton and auto-initializes
            if (DragDropManager.Instance != null)
            {
                Debug.Log("DragDropManager ready");
            }
        }

        private void AddTestItems()
        {
            // Add some test items to inventory
            if (testItems != null && testItems.Length > 0)
            {
                foreach (var item in testItems)
                {
                    if (item != null)
                    {
                        ItemStack stack = item.CreateStack(Random.Range(1, 5));
                        playerBag.TryAddItem(stack, out _);
                    }
                }
                Debug.Log($"Added {testItems.Length} test items");
            }

            // Add test equipment
            if (testEquipment != null && testEquipment.Length > 0)
            {
                foreach (var equipment in testEquipment)
                {
                    if (equipment != null)
                    {
                        ItemStack stack = equipment.CreateStack(1);
                        playerBag.TryAddItem(stack, out _);
                    }
                }
                Debug.Log($"Added {testEquipment.Length} test equipment items");
            }
        }

        #endregion

        #region Input Handling

        private void Update()
        {
            // Toggle inventory (I key)
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (inventoryPanel != null)
                {
                    inventoryPanel.Toggle();
                    Debug.Log($"Inventory: {(inventoryPanel.gameObject.activeSelf ? "Shown" : "Hidden")}");
                }
            }

            // Toggle equipment (E key)
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (equipmentPanel != null)
                {
                    equipmentPanel.Toggle();
                    Debug.Log($"Equipment: {(equipmentPanel.gameObject.activeSelf ? "Shown" : "Hidden")}");
                }
            }

            // Add test item (T key)
            if (Input.GetKeyDown(KeyCode.T))
            {
                AddRandomTestItem();
            }

            // Remove random item (R key)
            if (Input.GetKeyDown(KeyCode.R))
            {
                RemoveRandomItem();
            }

            // Equip random weapon (Q key)
            if (Input.GetKeyDown(KeyCode.Q))
            {
                EquipRandomWeapon();
            }
        }

        #endregion

        #region Test Actions

        private void AddRandomTestItem()
        {
            if (testItems == null || testItems.Length == 0)
            {
                Debug.LogWarning("No test items assigned");
                return;
            }

            ItemType randomItem = testItems[Random.Range(0, testItems.Length)];
            if (randomItem != null)
            {
                ItemStack stack = randomItem.CreateStack(Random.Range(1, 10));
                bool success = playerBag.TryAddItem(stack, out int remaining);

                if (success)
                {
                    Debug.Log($"Added {stack.Quantity - remaining}x {randomItem.Name}");
                    if (remaining > 0)
                    {
                        Debug.LogWarning($"Could only add {stack.Quantity - remaining}, {remaining} remaining");
                    }
                }
                else
                {
                    Debug.LogError("Failed to add item - inventory full?");
                }
            }
        }

        private void RemoveRandomItem()
        {
            var allItems = playerBag.GetAllItems();
            if (allItems.Count == 0)
            {
                Debug.Log("Inventory is empty");
                return;
            }

            ItemStack randomStack = allItems[Random.Range(0, allItems.Count)];
            int removeAmount = Random.Range(1, randomStack.Quantity + 1);

            bool success = playerBag.TryRemoveItem(randomStack.Type.ItemID, removeAmount, out int removed);
            if (success)
            {
                Debug.Log($"Removed {removed}x {randomStack.Type.Name}");
            }
        }

        private void EquipRandomWeapon()
        {
            if (testEquipment == null || testEquipment.Length == 0)
            {
                Debug.LogWarning("No test equipment assigned");
                return;
            }

            // Find a weapon in equipment list
            EquipmentType weapon = null;
            foreach (var eq in testEquipment)
            {
                if (eq != null && (eq.EquipmentSlot == EquipmentSlot.MainHand || eq.EquipmentSlot == EquipmentSlot.OffHand))
                {
                    weapon = eq;
                    break;
                }
            }

            if (weapon == null)
            {
                Debug.LogWarning("No weapon found in test equipment");
                return;
            }

            // Add to bag first
            ItemStack weaponStack = weapon.CreateStack(1);
            playerBag.TryAddItem(weaponStack, out _);

            // Equip
            bool success = playerEquipment.TryEquip(weaponStack, out string reason, out var unequipped);
            if (success)
            {
                // Remove from bag
                playerBag.TryRemoveItem(weapon.ItemID, 1, out _);

                // Add unequipped items back to bag
                foreach (var item in unequipped)
                {
                    playerBag.TryAddItem(item, out _);
                }

                Debug.Log($"Equipped {weapon.Name}");
            }
            else
            {
                Debug.LogError($"Failed to equip: {reason}");
            }
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 250, 400));
            GUILayout.Box("Inventory UI Example");

            GUILayout.Label($"Inventory Slots: {playerBag.SlotCount - playerBag.GetEmptySlotCount()}/{playerBag.SlotCount}");
            GUILayout.Label($"Total Weight: {playerBag.GetTotalWeight():F1}/{playerBag.MaxWeight}");
            GUILayout.Label($"Total Value: {playerBag.GetTotalValue()}");

            GUILayout.Space(10);
            GUILayout.Label($"Total Armor: {playerEquipment.GetTotalArmor()}");
            GUILayout.Label($"Total Damage: {playerEquipment.GetTotalDamage()}");

            GUILayout.Space(10);

            if (GUILayout.Button("Toggle Inventory (I)"))
            {
                inventoryPanel?.Toggle();
            }

            if (GUILayout.Button("Toggle Equipment (E)"))
            {
                equipmentPanel?.Toggle();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Add Random Item (T)"))
            {
                AddRandomTestItem();
            }

            if (GUILayout.Button("Remove Random Item (R)"))
            {
                RemoveRandomItem();
            }

            if (GUILayout.Button("Equip Random Weapon (Q)"))
            {
                EquipRandomWeapon();
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Clear Inventory"))
            {
                playerBag.Clear();
                Debug.Log("Inventory cleared");
            }

            if (GUILayout.Button("Unequip All"))
            {
                var unequipped = playerEquipment.UnequipAll();
                foreach (var item in unequipped)
                {
                    playerBag.TryAddItem(item, out _);
                }
                Debug.Log($"Unequipped {unequipped.Count} items");
            }

            GUILayout.EndArea();
        }

        #endregion
    }
}
