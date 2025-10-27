using UnityEngine;
using Inventory.Core;
using Inventory.Data;
using Inventory.Commands;

namespace Inventory.Examples
{
    /// <summary>
    /// Example script demonstrating the equipment system.
    /// Shows how to equip/unequip items and apply stat effects.
    /// </summary>
    public class EquipmentSystemExample : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private EquipmentType testWeapon;
        [SerializeField] private EquipmentType testArmor;
        [SerializeField] private EquipmentType testTwoHandedWeapon;

        [Header("Player Info")]
        [SerializeField] private int playerLevel = 10;
        [SerializeField] private string playerClass = "Warrior";

        private Inventory.Core.Inventory playerBag;
        private EquipmentInventory equipment;

        private void Start()
        {
            Debug.Log("=== Equipment System Example Started ===");

            // Example 1: Create inventories
            ExampleCreateInventories();

            // Example 2: Equip items
            if (testWeapon != null)
            {
                ExampleEquipWeapon();
            }

            if (testArmor != null)
            {
                ExampleEquipArmor();
            }

            // Example 3: Check equipment stats
            ExampleCheckEquipmentStats();

            // Example 4: Unequip items
            if (testWeapon != null)
            {
                ExampleUnequipItem();
            }

            // Example 5: Two-handed weapon
            if (testTwoHandedWeapon != null)
            {
                ExampleEquipTwoHanded();
            }

            Debug.Log("=== Equipment System Example Complete ===");
        }

        /// <summary>
        /// Example: Creating equipment inventory
        /// </summary>
        private void ExampleCreateInventories()
        {
            Debug.Log("\n--- Example 1: Creating Inventories ---");

            // Create a bag inventory
            playerBag = new Inventory.Core.Inventory("player_bag", maxSlots: 20, maxWeight: 100f);
            Debug.Log($"Created bag inventory: {playerBag.InventoryID}");

            // Create equipment inventory
            equipment = new EquipmentInventory("player_equipment", gameObject);
            Debug.Log($"Created equipment inventory for: {equipment.OwnerID}");
            Debug.Log($"  Owner GameObject: {equipment.OwnerObject?.name ?? "None"}");
        }

        /// <summary>
        /// Example: Equipping a weapon
        /// </summary>
        private void ExampleEquipWeapon()
        {
            Debug.Log("\n--- Example 2: Equipping Weapon ---");

            // Add weapon to bag first
            ItemStack weaponStack = testWeapon.CreateStack(1);
            playerBag.TryAddItem(weaponStack, out _);
            Debug.Log($"Added {testWeapon.Name} to bag");

            // Create equip command
            EquipItemCommand equipCmd = new EquipItemCommand(
                equipment,
                playerBag,
                weaponStack,
                sourceSlotIndex: 0, // First slot in bag
                playerLevel,
                playerClass
            );

            if (equipCmd.CanExecute())
            {
                Debug.Log("Equipping weapon...");
                bool success = equipCmd.Execute();

                if (success)
                {
                    Debug.Log("✓ Weapon equipped successfully");
                    Debug.Log($"  Slot: {testWeapon.EquipmentSlot}");
                    Debug.Log($"  Damage: {testWeapon.Damage}");
                    Debug.Log($"  Attack Speed: {testWeapon.AttackSpeed}");

                    // Check stat modifiers
                    var modStorage = GetComponent<Effects.StatModifierStorage>();
                    if (modStorage != null)
                    {
                        Debug.Log($"  Active modifiers: {modStorage.ActiveModifiers.Count}");
                    }
                }
                else
                {
                    Debug.LogError("✗ Failed to equip weapon");
                }
            }
            else
            {
                Debug.LogWarning("Cannot equip weapon - validation failed");
            }
        }

        /// <summary>
        /// Example: Equipping armor
        /// </summary>
        private void ExampleEquipArmor()
        {
            Debug.Log("\n--- Example 3: Equipping Armor ---");

            ItemStack armorStack = testArmor.CreateStack(1);
            playerBag.TryAddItem(armorStack, out _);

            // Find the slot index
            int armorSlotIndex = -1;
            for (int i = 0; i < playerBag.SlotCount; i++)
            {
                var stack = playerBag.GetStack(i);
                if (!stack.IsEmpty && stack.Type == testArmor)
                {
                    armorSlotIndex = i;
                    break;
                }
            }

            if (armorSlotIndex >= 0)
            {
                EquipItemCommand equipCmd = new EquipItemCommand(
                    equipment,
                    playerBag,
                    armorStack,
                    armorSlotIndex,
                    playerLevel,
                    playerClass
                );

                if (equipCmd.Execute())
                {
                    Debug.Log("✓ Armor equipped successfully");
                    Debug.Log($"  Slot: {testArmor.EquipmentSlot}");
                    Debug.Log($"  Armor: {testArmor.Armor}");
                    Debug.Log($"  Stat Bonuses:\n{testArmor.GetStatBonusSummary()}");
                }
            }
        }

        /// <summary>
        /// Example: Checking equipment stats
        /// </summary>
        private void ExampleCheckEquipmentStats()
        {
            Debug.Log("\n--- Example 4: Equipment Stats ---");

            Debug.Log($"Total Armor: {equipment.GetTotalArmor()}");
            Debug.Log($"Total Damage: {equipment.GetTotalDamage()}");

            Debug.Log("\nEquipped items:");
            var equippedItems = equipment.GetAllEquippedItems();
            foreach (var item in equippedItems)
            {
                EquipmentType eq = item.Type as EquipmentType;
                Debug.Log($"  - {eq.Name} in {eq.EquipmentSlot} slot");
            }

            Debug.Log($"\n{equipment.GetEquipmentSummary()}");
        }

        /// <summary>
        /// Example: Unequipping an item
        /// </summary>
        private void ExampleUnequipItem()
        {
            Debug.Log("\n--- Example 5: Unequipping Item ---");

            // Unequip weapon
            UnequipItemCommand unequipCmd = new UnequipItemCommand(
                equipment,
                testWeapon.EquipmentSlot,
                playerBag // Put it back in the bag
            );

            if (unequipCmd.Execute())
            {
                Debug.Log($"✓ Unequipped item from {testWeapon.EquipmentSlot} slot");
                Debug.Log($"  Item returned to bag");
                Debug.Log($"  Total Damage now: {equipment.GetTotalDamage()}");
            }
        }

        /// <summary>
        /// Example: Equipping a two-handed weapon
        /// </summary>
        private void ExampleEquipTwoHanded()
        {
            Debug.Log("\n--- Example 6: Two-Handed Weapon ---");

            ItemStack twoHandedStack = testTwoHandedWeapon.CreateStack(1);
            playerBag.TryAddItem(twoHandedStack, out _);

            // Find the slot
            int slotIndex = -1;
            for (int i = 0; i < playerBag.SlotCount; i++)
            {
                var stack = playerBag.GetStack(i);
                if (!stack.IsEmpty && stack.Type == testTwoHandedWeapon)
                {
                    slotIndex = i;
                    break;
                }
            }

            if (slotIndex >= 0)
            {
                EquipItemCommand equipCmd = new EquipItemCommand(
                    equipment,
                    playerBag,
                    twoHandedStack,
                    slotIndex,
                    playerLevel,
                    playerClass
                );

                if (equipCmd.Execute())
                {
                    Debug.Log("✓ Two-handed weapon equipped");
                    Debug.Log($"  Occupies both MainHand and OffHand slots");
                    Debug.Log($"  Damage: {testTwoHandedWeapon.Damage}");

                    // Check that both hands are occupied
                    bool mainHandOccupied = !equipment.IsSlotEmpty(EquipmentSlot.MainHand);
                    bool offHandOccupied = !equipment.IsSlotEmpty(EquipmentSlot.OffHand);
                    Debug.Log($"  MainHand occupied: {mainHandOccupied}");
                    Debug.Log($"  OffHand occupied: {offHandOccupied}");
                }
            }
        }

        /// <summary>
        /// GUI for testing
        /// </summary>
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 500));

            GUILayout.Label("Equipment System Test", GUI.skin.box);

            if (equipment != null)
            {
                GUILayout.Label($"Total Armor: {equipment.GetTotalArmor()}");
                GUILayout.Label($"Total Damage: {equipment.GetTotalDamage()}");

                GUILayout.Space(10);

                // Equip buttons
                if (testWeapon != null && GUILayout.Button($"Equip {testWeapon.Name}"))
                {
                    ItemStack stack = testWeapon.CreateStack(1);
                    playerBag.TryAddItem(stack, out _);
                    var cmd = new EquipItemCommand(equipment, playerBag, stack, 0, playerLevel, playerClass);
                    cmd.Execute();
                }

                if (testArmor != null && GUILayout.Button($"Equip {testArmor.Name}"))
                {
                    ItemStack stack = testArmor.CreateStack(1);
                    playerBag.TryAddItem(stack, out _);
                    var cmd = new EquipItemCommand(equipment, playerBag, stack, 0, playerLevel, playerClass);
                    cmd.Execute();
                }

                GUILayout.Space(10);

                // Unequip buttons
                if (GUILayout.Button("Unequip All"))
                {
                    var unequipped = equipment.UnequipAll();
                    foreach (var item in unequipped)
                    {
                        playerBag.TryAddItem(item, out _);
                    }
                }

                GUILayout.Space(10);

                // Show equipped items
                GUILayout.Label("Equipped Items:", GUI.skin.box);
                var equipped = equipment.GetAllEquippedItems();
                foreach (var item in equipped)
                {
                    if (item.Type is EquipmentType eq)
                    {
                        GUILayout.Label($"  {eq.EquipmentSlot}: {eq.Name}");
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
