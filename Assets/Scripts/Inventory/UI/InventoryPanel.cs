using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Core;
using Inventory.Data;

namespace Inventory.UI
{
    /// <summary>
    /// UI panel that displays an inventory with all its slots.
    /// Automatically creates slot UI elements and handles visual updates.
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform slotContainer;
        [SerializeField] private ItemSlotUI slotPrefab;
        [SerializeField] private ItemTooltip tooltip;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI capacityText;
        [SerializeField] private TextMeshProUGUI weightText;

        [Header("Layout")]
        [SerializeField] private int slotsPerRow = 5;
        [SerializeField] private float slotSpacing = 5f;
        [SerializeField] private Vector2 slotSize = new Vector2(64, 64);

        [Header("Settings")]
        [SerializeField] private bool autoCreateSlots = true;
        [SerializeField] private bool showTooltips = true;
        [SerializeField] private bool showCapacityInfo = true;

        // State
        private Inventory.Core.Inventory inventory;
        private List<ItemSlotUI> slotUIElements = new List<ItemSlotUI>();
        private ItemSlotUI selectedSlot;

        #region Properties

        public Inventory.Core.Inventory Inventory => inventory;
        public List<ItemSlotUI> SlotUIElements => slotUIElements;
        public ItemSlotUI SelectedSlot => selectedSlot;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the panel with an inventory.
        /// </summary>
        public void Initialize(Inventory.Core.Inventory inventory, string title = null)
        {
            this.inventory = inventory;

            // Set title
            if (titleText != null && !string.IsNullOrEmpty(title))
            {
                titleText.text = title;
            }
            else if (titleText != null)
            {
                titleText.text = inventory.InventoryID;
            }

            // Create slot UI elements
            if (autoCreateSlots)
            {
                CreateSlotUI();
            }

            // Subscribe to inventory events
            if (inventory != null)
            {
                inventory.OnSlotChanged += OnInventorySlotChanged;
                inventory.OnItemAdded += OnItemAdded;
                inventory.OnItemRemoved += OnItemRemoved;
                inventory.OnInventoryCleared += OnInventoryCleared;
            }

            // Initial update
            UpdateCapacityDisplay();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (inventory != null)
            {
                inventory.OnSlotChanged -= OnInventorySlotChanged;
                inventory.OnItemAdded -= OnItemAdded;
                inventory.OnItemRemoved -= OnItemRemoved;
                inventory.OnInventoryCleared -= OnInventoryCleared;
            }

            // Unsubscribe from slot events
            foreach (var slotUI in slotUIElements)
            {
                if (slotUI != null)
                {
                    UnsubscribeFromSlotEvents(slotUI);
                }
            }
        }

        #endregion

        #region Slot Creation

        /// <summary>
        /// Creates UI elements for all inventory slots.
        /// </summary>
        private void CreateSlotUI()
        {
            if (inventory == null || slotPrefab == null || slotContainer == null)
            {
                Debug.LogError("InventoryPanel: Missing required references for slot creation");
                return;
            }

            // Clear existing slots
            ClearSlots();

            // Setup grid layout if needed
            SetupGridLayout();

            // Create slot UI for each inventory slot
            for (int i = 0; i < inventory.SlotCount; i++)
            {
                CreateSlotUI(i);
            }
        }

        private void CreateSlotUI(int slotIndex)
        {
            GameObject slotObj = Instantiate(slotPrefab.gameObject, slotContainer);
            ItemSlotUI slotUI = slotObj.GetComponent<ItemSlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(inventory, slotIndex);
                slotUIElements.Add(slotUI);

                // Subscribe to events
                SubscribeToSlotEvents(slotUI);
            }
        }

        private void ClearSlots()
        {
            foreach (var slotUI in slotUIElements)
            {
                if (slotUI != null)
                {
                    UnsubscribeFromSlotEvents(slotUI);
                    Destroy(slotUI.gameObject);
                }
            }

            slotUIElements.Clear();
        }

        private void SetupGridLayout()
        {
            GridLayoutGroup grid = slotContainer.GetComponent<GridLayoutGroup>();
            if (grid == null)
            {
                grid = slotContainer.gameObject.AddComponent<GridLayoutGroup>();
            }

            grid.cellSize = slotSize;
            grid.spacing = new Vector2(slotSpacing, slotSpacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = slotsPerRow;
        }

        #endregion

        #region Event Handlers

        private void SubscribeToSlotEvents(ItemSlotUI slotUI)
        {
            slotUI.OnSlotClicked += HandleSlotClicked;
            slotUI.OnSlotRightClicked += HandleSlotRightClicked;
            slotUI.OnSlotHovered += HandleSlotHovered;
            slotUI.OnItemDropped += HandleItemDropped;
        }

        private void UnsubscribeFromSlotEvents(ItemSlotUI slotUI)
        {
            slotUI.OnSlotClicked -= HandleSlotClicked;
            slotUI.OnSlotRightClicked -= HandleSlotRightClicked;
            slotUI.OnSlotHovered -= HandleSlotHovered;
            slotUI.OnItemDropped -= HandleItemDropped;
        }

        private void HandleSlotClicked(ItemSlotUI slot)
        {
            // Select slot
            SetSelectedSlot(slot);
        }

        private void HandleSlotRightClicked(ItemSlotUI slot)
        {
            // Quick action (e.g., use item, equip, etc.)
            Debug.Log($"Right-clicked slot {slot.SlotIndex}");
        }

        private void HandleSlotHovered(ItemSlotUI slot, bool isHovered)
        {
            if (showTooltips && tooltip != null)
            {
                if (isHovered && !slot.IsEmpty)
                {
                    tooltip.Show(slot.CurrentStack, slot.GetScreenPosition());
                }
                else
                {
                    tooltip.Hide();
                }
            }
        }

        private void HandleItemDropped(ItemSlotUI targetSlot, ItemSlotUI sourceSlot)
        {
            // Drag/drop is handled by DragDropManager
            // This is just for additional logic if needed
            Debug.Log($"Item dropped on slot {targetSlot.SlotIndex} from slot {sourceSlot.SlotIndex}");
        }

        private void OnInventorySlotChanged(int slotIndex)
        {
            // Slot UI will update itself via its own event subscription
            UpdateCapacityDisplay();
        }

        private void OnItemAdded(ItemStack stack)
        {
            UpdateCapacityDisplay();
        }

        private void OnItemRemoved(ItemStack stack)
        {
            UpdateCapacityDisplay();
        }

        private void OnInventoryCleared()
        {
            UpdateCapacityDisplay();
        }

        #endregion

        #region Selection

        /// <summary>
        /// Sets the selected slot.
        /// </summary>
        public void SetSelectedSlot(ItemSlotUI slot)
        {
            // Deselect previous
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(false);
            }

            // Select new
            selectedSlot = slot;
            if (selectedSlot != null)
            {
                selectedSlot.SetSelected(true);
            }
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        public void ClearSelection()
        {
            SetSelectedSlot(null);
        }

        #endregion

        #region Display Updates

        /// <summary>
        /// Updates the capacity display (items/weight).
        /// </summary>
        private void UpdateCapacityDisplay()
        {
            if (!showCapacityInfo || inventory == null)
                return;

            // Update capacity text
            if (capacityText != null)
            {
                int usedSlots = inventory.SlotCount - inventory.GetEmptySlotCount();
                capacityText.text = $"Slots: {usedSlots}/{inventory.SlotCount}";
            }

            // Update weight text
            if (weightText != null && inventory.HasWeightLimit)
            {
                float currentWeight = inventory.GetTotalWeight();
                float maxWeight = inventory.MaxWeight;
                weightText.text = $"Weight: {currentWeight:F1}/{maxWeight:F1}";

                // Color based on capacity
                float percent = currentWeight / maxWeight;
                if (percent >= 0.9f)
                {
                    weightText.color = Color.red;
                }
                else if (percent >= 0.7f)
                {
                    weightText.color = Color.yellow;
                }
                else
                {
                    weightText.color = Color.white;
                }
            }
        }

        /// <summary>
        /// Refreshes all slot visuals.
        /// </summary>
        public void RefreshAllSlots()
        {
            foreach (var slotUI in slotUIElements)
            {
                if (slotUI != null)
                {
                    slotUI.UpdateVisuals();
                }
            }

            UpdateCapacityDisplay();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the slot UI for a specific slot index.
        /// </summary>
        public ItemSlotUI GetSlotUI(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < slotUIElements.Count)
            {
                return slotUIElements[slotIndex];
            }
            return null;
        }

        /// <summary>
        /// Shows the inventory panel.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshAllSlots();
        }

        /// <summary>
        /// Hides the inventory panel.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);

            // Hide tooltip
            if (tooltip != null)
            {
                tooltip.Hide();
            }
        }

        /// <summary>
        /// Toggles the inventory panel visibility.
        /// </summary>
        public void Toggle()
        {
            if (gameObject.activeSelf)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        #endregion
    }
}
