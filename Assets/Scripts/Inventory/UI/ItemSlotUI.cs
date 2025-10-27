using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Inventory.Data;

namespace Inventory.UI
{
    /// <summary>
    /// UI component representing a single inventory slot.
    /// Displays item icon, quantity, and handles click/drag events.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                               IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image rarityBorder;
        [SerializeField] private GameObject selectionHighlight;

        [Header("Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private Color occupiedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color canDropColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        [SerializeField] private Color cannotDropColor = new Color(0.7f, 0.3f, 0.3f, 1f);

        [Header("Settings")]
        [SerializeField] private bool showQuantity = true;
        [SerializeField] private bool showRarityBorder = true;
        [SerializeField] private bool allowDragDrop = true;

        // State
        private ItemStack currentStack;
        private int slotIndex;
        private Core.Inventory inventory;
        private bool isHovered;
        private bool isDragging;

        #region Events

        /// <summary>Fired when slot is clicked</summary>
        public event Action<ItemSlotUI> OnSlotClicked;

        /// <summary>Fired when slot is right-clicked</summary>
        public event Action<ItemSlotUI> OnSlotRightClicked;

        /// <summary>Fired when slot is hovered</summary>
        public event Action<ItemSlotUI, bool> OnSlotHovered;

        /// <summary>Fired when drag begins</summary>
        public event Action<ItemSlotUI> OnDragBegin;

        /// <summary>Fired when drag ends</summary>
        public event Action<ItemSlotUI> OnDragEnd;

        /// <summary>Fired when item is dropped on this slot</summary>
        public event Action<ItemSlotUI, ItemSlotUI> OnItemDropped;

        #endregion

        #region Properties

        public ItemStack CurrentStack => currentStack;
        public int SlotIndex => slotIndex;
        public Core.Inventory Inventory => inventory;
        public bool IsEmpty => currentStack.IsEmpty;
        public bool IsHovered => isHovered;
        public bool IsDragging => isDragging;

        #endregion

        #region Initialization

        private void Awake()
        {
            // Auto-find components if not assigned
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();

            if (iconImage == null)
                iconImage = transform.Find("Icon")?.GetComponent<Image>();

            if (quantityText == null)
                quantityText = GetComponentInChildren<TextMeshProUGUI>();

            if (selectionHighlight != null)
                selectionHighlight.SetActive(false);
        }

        /// <summary>
        /// Initializes the slot with inventory and index.
        /// </summary>
        public void Initialize(Core.Inventory inventory, int slotIndex)
        {
            this.inventory = inventory;
            this.slotIndex = slotIndex;

            // Subscribe to inventory events
            if (inventory != null)
            {
                inventory.OnSlotChanged += OnInventorySlotChanged;
            }

            // Initial update
            UpdateVisuals();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (inventory != null)
            {
                inventory.OnSlotChanged -= OnInventorySlotChanged;
            }
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates the slot visuals based on current item stack.
        /// </summary>
        public void UpdateVisuals()
        {
            if (inventory != null)
            {
                currentStack = inventory.GetStack(slotIndex);
            }

            UpdateIcon();
            UpdateQuantity();
            UpdateRarityBorder();
            UpdateBackground();
        }

        private void UpdateIcon()
        {
            if (iconImage == null) return;

            if (IsEmpty || currentStack.Type == null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = currentStack.Type.Icon;
            }
        }

        private void UpdateQuantity()
        {
            if (quantityText == null) return;

            if (!showQuantity || IsEmpty || currentStack.Quantity <= 1)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
            else
            {
                quantityText.enabled = true;
                quantityText.text = currentStack.Quantity.ToString();
            }
        }

        private void UpdateRarityBorder()
        {
            if (rarityBorder == null || !showRarityBorder) return;

            if (IsEmpty || currentStack.Type == null)
            {
                rarityBorder.enabled = false;
            }
            else
            {
                rarityBorder.enabled = true;
                rarityBorder.color = currentStack.Type.RarityColor;
            }
        }

        private void UpdateBackground()
        {
            if (backgroundImage == null) return;

            Color targetColor = IsEmpty ? emptyColor : occupiedColor;

            if (isHovered && !isDragging)
            {
                targetColor = highlightColor;
            }

            backgroundImage.color = targetColor;
        }

        /// <summary>
        /// Sets the item stack directly (for temporary display).
        /// </summary>
        public void SetStack(ItemStack stack)
        {
            currentStack = stack;
            UpdateVisuals();
        }

        #endregion

        #region Event Handlers

        private void OnInventorySlotChanged(int changedSlotIndex)
        {
            if (changedSlotIndex == slotIndex)
            {
                UpdateVisuals();
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateBackground();
            OnSlotHovered?.Invoke(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateBackground();
            OnSlotHovered?.Invoke(this, false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(this);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnSlotRightClicked?.Invoke(this);
            }
        }

        #endregion

        #region Drag and Drop

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!allowDragDrop || IsEmpty)
                return;

            isDragging = true;

            // Notify drag system
            DragDropManager.Instance?.StartDrag(this, eventData);
            OnDragBegin?.Invoke(this);

            // Make icon semi-transparent
            if (iconImage != null)
            {
                var color = iconImage.color;
                color.a = 0.6f;
                iconImage.color = color;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            // Let drag manager handle the visual
            DragDropManager.Instance?.UpdateDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            isDragging = false;

            // Restore icon
            if (iconImage != null)
            {
                var color = iconImage.color;
                color.a = 1f;
                iconImage.color = color;
            }

            // Notify drag system
            DragDropManager.Instance?.EndDrag(eventData);
            OnDragEnd?.Invoke(this);

            UpdateBackground();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!allowDragDrop)
                return;

            // Get the dragged slot
            ItemSlotUI draggedSlot = DragDropManager.Instance?.CurrentDragSlot;
            if (draggedSlot != null && draggedSlot != this)
            {
                OnItemDropped?.Invoke(this, draggedSlot);
            }
        }

        /// <summary>
        /// Shows visual feedback for whether an item can be dropped here.
        /// </summary>
        public void ShowDropFeedback(bool canDrop)
        {
            if (backgroundImage == null) return;

            backgroundImage.color = canDrop ? canDropColor : cannotDropColor;
        }

        /// <summary>
        /// Clears drop feedback.
        /// </summary>
        public void ClearDropFeedback()
        {
            UpdateBackground();
        }

        #endregion

        #region Selection

        /// <summary>
        /// Sets the selection state of this slot.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionHighlight != null)
            {
                selectionHighlight.SetActive(selected);
            }
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets the screen position of this slot.
        /// </summary>
        public Vector3 GetScreenPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Sets whether this slot allows drag and drop.
        /// </summary>
        public void SetDragDropEnabled(bool enabled)
        {
            allowDragDrop = enabled;
        }

        #endregion
    }
}
