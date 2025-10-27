using UnityEngine;
using System.Collections.Generic;
using Inventory.Core;
using Inventory.Data;
using Inventory.Commands;
using Inventory.Guards;
using Inventory.Effects;

namespace Inventory.Examples
{
    /// <summary>
    /// Example demonstrating the consumable item system with effects, cooldowns, and buffs.
    /// Shows using potions, scrolls, food, and other consumable items.
    ///
    /// Setup Instructions:
    /// 1. Add this script to an empty GameObject
    /// 2. Create some test ConsumableType ScriptableObjects:
    ///    - Health Potion (Heal effect)
    ///    - Mana Potion (RestoreMana effect)
    ///    - Strength Buff (StatModifier with duration)
    ///    - Poison (DamageOverTime effect)
    ///    - Regeneration Potion (HealOverTime effect)
    /// 3. Assign them to the testConsumables array
    /// 4. Press Play and use keyboard shortcuts
    ///
    /// Keyboard Shortcuts:
    /// - H: Use Health Potion
    /// - M: Use Mana Potion
    /// - B: Use Buff Potion
    /// - P: Use Poison (DoT)
    /// - R: Use Regeneration Potion (HoT)
    /// - A: Add random consumable to inventory
    /// - C: Clear all cooldowns
    /// - E: Print active effects
    /// - U: Undo last use
    /// - Y: Redo last use
    /// </summary>
    public class ConsumableSystemExample : MonoBehaviour
    {
        [Header("Test Consumables")]
        [SerializeField] private ConsumableType[] testConsumables;

        [Header("Player Object")]
        [SerializeField] private GameObject player;

        // Game components
        private Inventory.Core.Inventory playerInventory;
        private CooldownTracker cooldownTracker;
        private EffectManager effectManager;
        private CommandHistory commandHistory;

        #region Initialization

        private void Start()
        {
            Debug.Log("=== Consumable System Example Started ===");
            Debug.Log("Keyboard Shortcuts:");
            Debug.Log("  H - Use Health Potion");
            Debug.Log("  M - Use Mana Potion");
            Debug.Log("  B - Use Buff Potion");
            Debug.Log("  P - Use Poison (DoT)");
            Debug.Log("  R - Use Regeneration Potion (HoT)");
            Debug.Log("  A - Add Random Consumable");
            Debug.Log("  C - Clear Cooldowns");
            Debug.Log("  E - Print Active Effects");
            Debug.Log("  U - Undo Last Use");
            Debug.Log("  Y - Redo Last Use");

            SetupPlayer();
            SetupInventory();
            SetupCommandHistory();
            AddTestItems();
        }

        private void SetupPlayer()
        {
            // Create player GameObject if not assigned
            if (player == null)
            {
                player = new GameObject("Player");
                Debug.Log("Created player GameObject");
            }

            // Add CooldownTracker
            cooldownTracker = player.GetComponent<CooldownTracker>();
            if (cooldownTracker == null)
            {
                cooldownTracker = player.AddComponent<CooldownTracker>();
            }

            // Add EffectManager
            effectManager = player.GetComponent<EffectManager>();
            if (effectManager == null)
            {
                effectManager = player.AddComponent<EffectManager>();
            }

            // Subscribe to events
            cooldownTracker.OnCooldownStarted += OnCooldownStarted;
            cooldownTracker.OnCooldownExpired += OnCooldownExpired;
            effectManager.OnEffectApplied += OnEffectApplied;
            effectManager.OnEffectRemoved += OnEffectRemoved;
            effectManager.OnEffectTicked += OnEffectTicked;

            Debug.Log("Player components setup complete");
        }

        private void SetupInventory()
        {
            // Create player inventory
            playerInventory = new Inventory.Core.Inventory("player_consumables", maxSlots: 20, maxWeight: 50f);
            InventoryManager.Instance.RegisterInventory(playerInventory);

            Debug.Log("Player inventory created");
        }

        private void SetupCommandHistory()
        {
            commandHistory = new CommandHistory(maxHistorySize: 20);
        }

        private void AddTestItems()
        {
            if (testConsumables == null || testConsumables.Length == 0)
            {
                Debug.LogWarning("No test consumables assigned! Create ConsumableType ScriptableObjects and assign them.");
                return;
            }

            // Add a few of each consumable to inventory
            foreach (var consumable in testConsumables)
            {
                if (consumable != null)
                {
                    int quantity = Random.Range(2, 5);
                    ItemStack stack = consumable.CreateStack(quantity);
                    playerInventory.TryAddItem(stack, out _);
                    Debug.Log($"Added {quantity}x {consumable.Name} to inventory");
                }
            }
        }

        private void OnDestroy()
        {
            if (cooldownTracker != null)
            {
                cooldownTracker.OnCooldownStarted -= OnCooldownStarted;
                cooldownTracker.OnCooldownExpired -= OnCooldownExpired;
            }

            if (effectManager != null)
            {
                effectManager.OnEffectApplied -= OnEffectApplied;
                effectManager.OnEffectRemoved -= OnEffectRemoved;
                effectManager.OnEffectTicked -= OnEffectTicked;
            }
        }

        #endregion

        #region Input Handling

        private void Update()
        {
            // Use Health Potion (H key)
            if (Input.GetKeyDown(KeyCode.H))
            {
                UseConsumableByName("Health", "Potion");
            }

            // Use Mana Potion (M key)
            if (Input.GetKeyDown(KeyCode.M))
            {
                UseConsumableByName("Mana", "Potion");
            }

            // Use Buff (B key)
            if (Input.GetKeyDown(KeyCode.B))
            {
                UseConsumableByName("Strength", "Buff", "Speed", "Power");
            }

            // Use Poison (P key)
            if (Input.GetKeyDown(KeyCode.P))
            {
                UseConsumableByName("Poison", "DoT", "Damage");
            }

            // Use Regeneration (R key)
            if (Input.GetKeyDown(KeyCode.R))
            {
                UseConsumableByName("Regen", "Regeneration", "HoT");
            }

            // Add random consumable (A key)
            if (Input.GetKeyDown(KeyCode.A))
            {
                AddRandomConsumable();
            }

            // Clear cooldowns (C key)
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearAllCooldowns();
            }

            // Print effects (E key)
            if (Input.GetKeyDown(KeyCode.E))
            {
                PrintActiveEffects();
            }

            // Undo (U key)
            if (Input.GetKeyDown(KeyCode.U))
            {
                UndoLastUse();
            }

            // Redo (Y key)
            if (Input.GetKeyDown(KeyCode.Y))
            {
                RedoLastUse();
            }
        }

        #endregion

        #region Consumable Actions

        private void UseConsumableByName(params string[] keywords)
        {
            // Find a consumable in inventory matching any keyword
            var allItems = playerInventory.GetAllItems();

            foreach (var stack in allItems)
            {
                if (stack.Type is ConsumableType)
                {
                    string itemName = stack.Type.Name.ToLower();
                    foreach (var keyword in keywords)
                    {
                        if (itemName.Contains(keyword.ToLower()))
                        {
                            UseConsumable(stack.Type.ItemID);
                            return;
                        }
                    }
                }
            }

            Debug.LogWarning($"No consumable found matching keywords: {string.Join(", ", keywords)}");
        }

        private void UseConsumable(string itemID)
        {
            // Create and execute use command
            var useCommand = new UseItemCommand(
                inventory: playerInventory,
                itemID: itemID,
                user: player,
                target: player,
                cooldownTracker: cooldownTracker
            );

            // Validate with guard
            var guard = new CanUseItemGuard(playerInventory, itemID, player, cooldownTracker);
            var guardResult = guard.Evaluate(null);

            if (!guardResult.Success)
            {
                Debug.LogWarning($"<color=yellow>Cannot use item: {guardResult.Reason}</color>");
                return;
            }

            // Execute command
            if (useCommand.CanExecute())
            {
                bool success = useCommand.Execute();

                if (success)
                {
                    commandHistory.AddCommand(useCommand);
                    ItemType itemType = InventoryManager.Instance.FindItemType(itemID);
                    Debug.Log($"<color=green>Used {itemType.Name}!</color>");
                }
                else
                {
                    Debug.LogError($"Failed to use item: {useCommand.ErrorMessage}");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>Cannot execute use command: {useCommand.ErrorMessage}</color>");
            }
        }

        private void AddRandomConsumable()
        {
            if (testConsumables == null || testConsumables.Length == 0)
            {
                Debug.LogWarning("No test consumables assigned");
                return;
            }

            ConsumableType random = testConsumables[Random.Range(0, testConsumables.Length)];
            ItemStack stack = random.CreateStack(1);
            bool success = playerInventory.TryAddItem(stack, out int remaining);

            if (success)
            {
                Debug.Log($"Added {random.Name} to inventory");
            }
            else
            {
                Debug.LogWarning("Inventory is full!");
            }
        }

        private void ClearAllCooldowns()
        {
            cooldownTracker.ClearAllCooldowns();
            Debug.Log("<color=cyan>Cleared all cooldowns</color>");
        }

        private void PrintActiveEffects()
        {
            string summary = effectManager.GetEffectSummary();
            Debug.Log($"<color=cyan>{summary}</color>");
        }

        private void UndoLastUse()
        {
            if (commandHistory.CanUndo())
            {
                commandHistory.Undo();
                Debug.Log("<color=yellow>Undid last consumable use</color>");
            }
            else
            {
                Debug.LogWarning("Nothing to undo");
            }
        }

        private void RedoLastUse()
        {
            if (commandHistory.CanRedo())
            {
                commandHistory.Redo();
                Debug.Log("<color=yellow>Redid consumable use</color>");
            }
            else
            {
                Debug.LogWarning("Nothing to redo");
            }
        }

        #endregion

        #region Event Handlers

        private void OnCooldownStarted(string itemID, float duration)
        {
            ItemType itemType = InventoryManager.Instance.FindItemType(itemID);
            string name = itemType != null ? itemType.Name : itemID;
            Debug.Log($"<color=cyan>[Cooldown]</color> Started {duration:F1}s cooldown for {name}");
        }

        private void OnCooldownExpired(string itemID)
        {
            ItemType itemType = InventoryManager.Instance.FindItemType(itemID);
            string name = itemType != null ? itemType.Name : itemID;
            Debug.Log($"<color=cyan>[Cooldown]</color> {name} is ready to use");
        }

        private void OnEffectApplied(ActiveEffect effect)
        {
            Debug.Log($"<color=green>[Effect Applied]</color> {effect.Description} ({effect.Duration:F1}s)");
        }

        private void OnEffectRemoved(ActiveEffect effect)
        {
            Debug.Log($"<color=yellow>[Effect Removed]</color> {effect.Description}");
        }

        private void OnEffectTicked(ActiveEffect effect)
        {
            Debug.Log($"<color=magenta>[Effect Tick]</color> {effect.Description} - {effect.Value}");
        }

        #endregion

        #region GUI

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.Box("Consumable System Example");

            // Player info
            GUILayout.Label("<b>Player Inventory</b>");
            var consumables = GetConsumablesInInventory();
            GUILayout.Label($"Consumables: {consumables.Count}");

            GUILayout.Space(5);

            // List consumables with quantities
            if (consumables.Count > 0)
            {
                GUILayout.Label("<b>Available Consumables:</b>");
                foreach (var kvp in consumables)
                {
                    ConsumableType consumable = kvp.Key;
                    int quantity = kvp.Value;

                    string cooldownText = "";
                    if (cooldownTracker.IsOnCooldown(consumable.ItemID))
                    {
                        float remaining = cooldownTracker.GetRemainingCooldown(consumable.ItemID);
                        cooldownText = $" (CD: {remaining:F1}s)";
                    }

                    GUILayout.Label($"  {consumable.Name} x{quantity}{cooldownText}");

                    // Show effects
                    if (consumable.ConsumableEffects != null && consumable.ConsumableEffects.Count > 0)
                    {
                        GUILayout.Label($"    <i>{consumable.GetEffectDescription()}</i>");
                    }
                }
            }
            else
            {
                GUILayout.Label("<i>No consumables in inventory</i>");
            }

            GUILayout.Space(10);

            // Active effects
            GUILayout.Label("<b>Active Effects</b>");
            var activeEffects = effectManager.GetAllActiveEffects();
            GUILayout.Label($"Count: {activeEffects.Count}");

            if (activeEffects.Count > 0)
            {
                foreach (var effect in activeEffects)
                {
                    GUILayout.Label($"  {effect.Description}: {effect.RemainingDuration:F1}s");
                }
            }
            else
            {
                GUILayout.Label("<i>No active effects</i>");
            }

            GUILayout.Space(10);

            // Cooldowns
            GUILayout.Label("<b>Active Cooldowns</b>");
            var cooldowns = cooldownTracker.GetAllItemsOnCooldown();
            GUILayout.Label($"Count: {cooldowns.Count}");

            if (cooldowns.Count > 0)
            {
                foreach (var itemID in cooldowns)
                {
                    float remaining = cooldownTracker.GetRemainingCooldown(itemID);
                    ItemType itemType = InventoryManager.Instance.FindItemType(itemID);
                    string name = itemType != null ? itemType.Name : itemID;
                    GUILayout.Label($"  {name}: {remaining:F1}s");
                }
            }
            else
            {
                GUILayout.Label("<i>No cooldowns active</i>");
            }

            GUILayout.Space(10);

            // Command history
            GUILayout.Label("<b>Command History</b>");
            GUILayout.Label($"Can Undo: {(commandHistory.CanUndo() ? "YES" : "NO")}");
            GUILayout.Label($"Can Redo: {(commandHistory.CanRedo() ? "YES" : "NO")}");

            GUILayout.Space(10);

            // Buttons
            if (GUILayout.Button("Add Random Consumable (A)"))
            {
                AddRandomConsumable();
            }

            if (GUILayout.Button("Clear All Cooldowns (C)"))
            {
                ClearAllCooldowns();
            }

            if (GUILayout.Button("Print Active Effects (E)"))
            {
                PrintActiveEffects();
            }

            GUILayout.EndArea();
        }

        private Dictionary<ConsumableType, int> GetConsumablesInInventory()
        {
            var result = new Dictionary<ConsumableType, int>();
            var allItems = playerInventory.GetAllItems();

            foreach (var stack in allItems)
            {
                if (stack.Type is ConsumableType consumable)
                {
                    if (result.ContainsKey(consumable))
                    {
                        result[consumable] += stack.Quantity;
                    }
                    else
                    {
                        result[consumable] = stack.Quantity;
                    }
                }
            }

            return result;
        }

        #endregion
    }
}
