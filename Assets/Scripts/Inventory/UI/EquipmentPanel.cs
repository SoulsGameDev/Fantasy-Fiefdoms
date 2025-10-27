using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Core;
using Inventory.Data;

namespace Inventory.UI
{
    /// <summary>
    /// UI panel that displays equipment slots (Head, Chest, MainHand, etc.).
    /// Shows equipped items and allows equipping/unequipping via drag-and-drop.
    /// </summary>
    public class EquipmentPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ItemTooltip tooltip;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI statsText;

        [Header("Equipment Slot UI")]
        [SerializeField] private EquipmentSlotUI headSlot;
        [SerializeField] private EquipmentSlotUI chestSlot;
        [SerializeField] private EquipmentSlotUI legsSlot;
        [SerializeField] private EquipmentSlotUI handsSlot;
        [SerializeField] private EquipmentSlotUI feetSlot;
        [SerializeField] private EquipmentSlotUI mainHandSlot;
        [SerializeField] private EquipmentSlotUI offHandSlot;
        [SerializeField] private EquipmentSlotUI backSlot;
        [SerializeField] private EquipmentSlotUI beltSlot;
        [SerializeField] private EquipmentSlotUI accessory1Slot;
        [SerializeField] private EquipmentSlotUI accessory2Slot;
        [SerializeField] private EquipmentSlotUI accessory3Slot;

        [Header("Settings")]
        [SerializeField] private bool showTooltips = true;
        [SerializeField] private bool showStatsDisplay = true;

        // State
        private EquipmentInventory equipmentInventory;
        private Dictionary<EquipmentSlot, EquipmentSlotUI> slotUIMap;

        #region Properties

        public EquipmentInventory EquipmentInventory => equipmentInventory;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the panel with an equipment inventory.
        /// </summary>
        public void Initialize(EquipmentInventory equipment, string title = null)
        {
            this.equipmentInventory = equipment;

            // Set title
            if (titleText != null && !string.IsNullOrEmpty(title))
            {
                titleText.text = title;
            }
            else if (titleText != null)
            {
                titleText.text = "Equipment";
            }

            // Build slot UI map
            BuildSlotMap();

            // Initialize each slot
            foreach (var kvp in slotUIMap)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.Initialize(equipment, kvp.Key);
                    SubscribeToSlotEvents(kvp.Value);
                }
            }

            // Subscribe to equipment events
            if (equipment != null)
            {
                equipment.OnItemEquipped += OnItemEquipped;
                equipment.OnItemUnequipped += OnItemUnequipped;
                equipment.OnEquipmentChanged += OnEquipmentChanged;
            }

            // Initial update
            RefreshAll();
        }

        private void OnDestroy()
        {
            // Unsubscribe from equipment events
            if (equipmentInventory != null)
            {
                equipmentInventory.OnItemEquipped -= OnItemEquipped;
                equipmentInventory.OnItemUnequipped -= OnItemUnequipped;
                equipmentInventory.OnEquipmentChanged -= OnEquipmentChanged;
            }

            // Unsubscribe from slot events
            if (slotUIMap != null)
            {
                foreach (var kvp in slotUIMap)
                {
                    if (kvp.Value != null)
                    {
                        UnsubscribeFromSlotEvents(kvp.Value);
                    }
                }
            }
        }

        private void BuildSlotMap()
        {
            slotUIMap = new Dictionary<EquipmentSlot, EquipmentSlotUI>();

            if (headSlot != null) slotUIMap[EquipmentSlot.Head] = headSlot;
            if (chestSlot != null) slotUIMap[EquipmentSlot.Chest] = chestSlot;
            if (legsSlot != null) slotUIMap[EquipmentSlot.Legs] = legsSlot;
            if (handsSlot != null) slotUIMap[EquipmentSlot.Hands] = handsSlot;
            if (feetSlot != null) slotUIMap[EquipmentSlot.Feet] = feetSlot;
            if (mainHandSlot != null) slotUIMap[EquipmentSlot.MainHand] = mainHandSlot;
            if (offHandSlot != null) slotUIMap[EquipmentSlot.OffHand] = offHandSlot;
            if (backSlot != null) slotUIMap[EquipmentSlot.Back] = backSlot;
            if (beltSlot != null) slotUIMap[EquipmentSlot.Belt] = beltSlot;
            if (accessory1Slot != null) slotUIMap[EquipmentSlot.Accessory1] = accessory1Slot;
            if (accessory2Slot != null) slotUIMap[EquipmentSlot.Accessory2] = accessory2Slot;
            if (accessory3Slot != null) slotUIMap[EquipmentSlot.Accessory3] = accessory3Slot;
        }

        #endregion

        #region Event Handlers

        private void SubscribeToSlotEvents(EquipmentSlotUI slotUI)
        {
            slotUI.OnSlotClicked += HandleSlotClicked;
            slotUI.OnSlotRightClicked += HandleSlotRightClicked;
            slotUI.OnSlotHovered += HandleSlotHovered;
        }

        private void UnsubscribeFromSlotEvents(EquipmentSlotUI slotUI)
        {
            slotUI.OnSlotClicked -= HandleSlotClicked;
            slotUI.OnSlotRightClicked -= HandleSlotRightClicked;
            slotUI.OnSlotHovered -= HandleSlotHovered;
        }

        private void HandleSlotClicked(EquipmentSlotUI slot)
        {
            Debug.Log($"Equipment slot clicked: {slot.EquipSlot}");
        }

        private void HandleSlotRightClicked(EquipmentSlotUI slot)
        {
            // Unequip on right-click
            if (!slot.IsEmpty)
            {
                Debug.Log($"Unequipping from {slot.EquipSlot}");
                // You can trigger unequip here if you have a target inventory
            }
        }

        private void HandleSlotHovered(EquipmentSlotUI slot, bool isHovered)
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

        private void OnItemEquipped(EquipmentSlot slot, ItemStack stack)
        {
            RefreshSlot(slot);
            UpdateStatsDisplay();
        }

        private void OnItemUnequipped(EquipmentSlot slot, ItemStack stack)
        {
            RefreshSlot(slot);
            UpdateStatsDisplay();
        }

        private void OnEquipmentChanged()
        {
            UpdateStatsDisplay();
        }

        #endregion

        #region Display Updates

        /// <summary>
        /// Refreshes a specific equipment slot.
        /// </summary>
        public void RefreshSlot(EquipmentSlot slot)
        {
            if (slotUIMap != null && slotUIMap.TryGetValue(slot, out EquipmentSlotUI slotUI))
            {
                if (slotUI != null)
                {
                    slotUI.UpdateVisuals();
                }
            }
        }

        /// <summary>
        /// Refreshes all equipment slots.
        /// </summary>
        public void RefreshAll()
        {
            if (slotUIMap != null)
            {
                foreach (var kvp in slotUIMap)
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.UpdateVisuals();
                    }
                }
            }

            UpdateStatsDisplay();
        }

        /// <summary>
        /// Updates the stats display showing total armor, damage, etc.
        /// </summary>
        private void UpdateStatsDisplay()
        {
            if (!showStatsDisplay || statsText == null || equipmentInventory == null)
                return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"<b>Total Stats:</b>");
            sb.AppendLine($"  Armor: {equipmentInventory.GetTotalArmor()}");
            sb.AppendLine($"  Damage: {equipmentInventory.GetTotalDamage()}");

            // You can add more aggregated stats here
            // For example, total strength from all equipped items, etc.

            statsText.text = sb.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the slot UI for a specific equipment slot.
        /// </summary>
        public EquipmentSlotUI GetSlotUI(EquipmentSlot slot)
        {
            if (slotUIMap != null && slotUIMap.TryGetValue(slot, out EquipmentSlotUI slotUI))
            {
                return slotUI;
            }
            return null;
        }

        /// <summary>
        /// Shows the equipment panel.
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshAll();
        }

        /// <summary>
        /// Hides the equipment panel.
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
        /// Toggles the equipment panel visibility.
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

    /// <summary>
    /// UI component for a single equipment slot.
    /// Similar to ItemSlotUI but specifically for equipment slots.
    /// </summary>
    public class EquipmentSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI slotLabelText;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color occupiedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

        // State
        private EquipmentInventory equipmentInventory;
        private EquipmentSlot equipSlot;
        private ItemStack currentStack;

        #region Events

        public event Action<EquipmentSlotUI> OnSlotClicked;
        public event Action<EquipmentSlotUI> OnSlotRightClicked;
        public event Action<EquipmentSlotUI, bool> OnSlotHovered;

        #endregion

        #region Properties

        public EquipmentSlot EquipSlot => equipSlot;
        public ItemStack CurrentStack => currentStack;
        public bool IsEmpty => currentStack.IsEmpty;

        #endregion

        /// <summary>
        /// Initializes the equipment slot.
        /// </summary>
        public void Initialize(EquipmentInventory equipment, EquipmentSlot slot)
        {
            this.equipmentInventory = equipment;
            this.equipSlot = slot;

            // Set label
            if (slotLabelText != null)
            {
                slotLabelText.text = slot.ToString();
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Updates the slot visuals.
        /// </summary>
        public void UpdateVisuals()
        {
            if (equipmentInventory != null)
            {
                currentStack = equipmentInventory.GetEquippedItem(equipSlot);
            }

            // Update icon
            if (iconImage != null)
            {
                if (IsEmpty || currentStack.Type == null)
                {
                    iconImage.enabled = false;
                }
                else
                {
                    iconImage.enabled = true;
                    iconImage.sprite = currentStack.Type.Icon;
                }
            }

            // Update background
            if (backgroundImage != null)
            {
                backgroundImage.color = IsEmpty ? emptyColor : occupiedColor;
            }
        }

        /// <summary>
        /// Gets the screen position of this slot.
        /// </summary>
        public Vector3 GetScreenPosition()
        {
            return transform.position;
        }
    }
}
