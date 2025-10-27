using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Inventory.Data;
using Inventory.Commands;

namespace Inventory.UI
{
    /// <summary>
    /// Singleton manager for handling drag and drop operations between inventory slots.
    /// Creates a visual representation of the dragged item and handles drop validation.
    /// </summary>
    public class DragDropManager : Singleton<DragDropManager>
    {
        [Header("Drag Visual")]
        [SerializeField] private Canvas dragCanvas;
        [SerializeField] private Image dragImage;
        [SerializeField] private float dragImageAlpha = 0.8f;

        [Header("Settings")]
        [SerializeField] private bool useCommandSystem = true;

        // Drag state
        private ItemSlotUI currentDragSlot;
        private GameObject dragObject;
        private RectTransform dragRectTransform;

        #region Properties

        /// <summary>The slot currently being dragged</summary>
        public ItemSlotUI CurrentDragSlot => currentDragSlot;

        /// <summary>Whether a drag is currently in progress</summary>
        public bool IsDragging => currentDragSlot != null;

        #endregion

        #region Initialization

        protected override void Init()
        {
            base.Init();

            // Create drag canvas if not assigned
            if (dragCanvas == null)
            {
                CreateDragCanvas();
            }

            // Create drag image if not assigned
            if (dragImage == null)
            {
                CreateDragImage();
            }

            if (dragObject != null)
            {
                dragObject.SetActive(false);
            }
        }

        private void CreateDragCanvas()
        {
            // Find or create a canvas for dragging
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (var canvas in canvases)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    dragCanvas = canvas;
                    break;
                }
            }

            if (dragCanvas == null)
            {
                GameObject canvasObj = new GameObject("DragCanvas");
                dragCanvas = canvasObj.AddComponent<Canvas>();
                dragCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                dragCanvas.sortingOrder = 1000; // Render on top
                canvasObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObj);
            }
        }

        private void CreateDragImage()
        {
            dragObject = new GameObject("DragImage");
            dragObject.transform.SetParent(dragCanvas.transform, false);

            dragImage = dragObject.AddComponent<Image>();
            dragImage.raycastTarget = false; // Don't block raycasts

            dragRectTransform = dragObject.GetComponent<RectTransform>();
            dragRectTransform.sizeDelta = new Vector2(64, 64); // Default size

            dragObject.SetActive(false);
        }

        #endregion

        #region Drag Operations

        /// <summary>
        /// Starts a drag operation from the specified slot.
        /// </summary>
        public void StartDrag(ItemSlotUI slot, PointerEventData eventData)
        {
            if (slot == null || slot.IsEmpty)
                return;

            currentDragSlot = slot;

            // Setup drag visual
            if (dragImage != null && slot.CurrentStack.Type != null)
            {
                dragObject.SetActive(true);
                dragImage.sprite = slot.CurrentStack.Type.Icon;

                // Set alpha
                Color color = dragImage.color;
                color.a = dragImageAlpha;
                dragImage.color = color;

                // Position at cursor
                UpdateDragPosition(eventData.position);
            }
        }

        /// <summary>
        /// Updates the drag visual position.
        /// </summary>
        public void UpdateDrag(PointerEventData eventData)
        {
            if (!IsDragging || dragObject == null)
                return;

            UpdateDragPosition(eventData.position);
        }

        /// <summary>
        /// Ends the drag operation and handles the drop.
        /// </summary>
        public void EndDrag(PointerEventData eventData)
        {
            if (!IsDragging)
                return;

            // Hide drag visual
            if (dragObject != null)
            {
                dragObject.SetActive(false);
            }

            // Find what we're hovering over
            var results = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            ItemSlotUI targetSlot = null;
            foreach (var result in results)
            {
                targetSlot = result.gameObject.GetComponent<ItemSlotUI>();
                if (targetSlot != null)
                    break;
            }

            // Handle the drop
            if (targetSlot != null && targetSlot != currentDragSlot)
            {
                HandleDrop(currentDragSlot, targetSlot);
            }

            // Clear drag state
            currentDragSlot = null;
        }

        private void UpdateDragPosition(Vector2 screenPosition)
        {
            if (dragRectTransform == null || dragCanvas == null)
                return;

            // Convert screen position to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragCanvas.transform as RectTransform,
                screenPosition,
                dragCanvas.worldCamera,
                out Vector2 localPoint);

            dragRectTransform.localPosition = localPoint;
        }

        #endregion

        #region Drop Handling

        /// <summary>
        /// Handles dropping an item from source slot to target slot.
        /// </summary>
        private void HandleDrop(ItemSlotUI sourceSlot, ItemSlotUI targetSlot)
        {
            if (sourceSlot == null || targetSlot == null)
                return;

            // Check if it's the same inventory or cross-inventory
            bool sameInventory = sourceSlot.Inventory == targetSlot.Inventory;

            if (useCommandSystem)
            {
                HandleDropWithCommands(sourceSlot, targetSlot, sameInventory);
            }
            else
            {
                HandleDropDirect(sourceSlot, targetSlot, sameInventory);
            }
        }

        /// <summary>
        /// Handles drop using the command system (supports undo/redo).
        /// </summary>
        private void HandleDropWithCommands(ItemSlotUI sourceSlot, ItemSlotUI targetSlot, bool sameInventory)
        {
            // Create move command
            MoveItemCommand moveCmd = new MoveItemCommand(
                sourceSlot.Inventory,
                sourceSlot.SlotIndex,
                targetSlot.Inventory,
                targetSlot.SlotIndex,
                quantity: -1 // Move all
            );

            // Execute (or add to command history for undo/redo)
            if (moveCmd.CanExecute())
            {
                bool success = moveCmd.Execute();
                if (success)
                {
                    Debug.Log($"Moved item from slot {sourceSlot.SlotIndex} to {targetSlot.SlotIndex}");
                }
                else
                {
                    Debug.LogWarning("Failed to move item");
                }
            }
            else
            {
                Debug.LogWarning("Cannot move item - validation failed");
            }
        }

        /// <summary>
        /// Handles drop directly without command system.
        /// </summary>
        private void HandleDropDirect(ItemSlotUI sourceSlot, ItemSlotUI targetSlot, bool sameInventory)
        {
            ItemStack sourceStack = sourceSlot.CurrentStack;
            ItemStack targetStack = targetSlot.CurrentStack;

            if (sameInventory)
            {
                // Swap within same inventory
                Core.Inventory inv = sourceSlot.Inventory;
                inv.SetStack(sourceSlot.SlotIndex, targetStack, out _);
                inv.SetStack(targetSlot.SlotIndex, sourceStack, out _);
            }
            else
            {
                // Move between inventories
                // Remove from source
                sourceSlot.Inventory.RemoveFromSlot(sourceSlot.SlotIndex, sourceStack.Quantity, out _);

                // Add to target (if empty) or swap
                if (targetStack.IsEmpty)
                {
                    targetSlot.Inventory.SetStack(targetSlot.SlotIndex, sourceStack, out _);
                }
                else
                {
                    // Swap
                    targetSlot.Inventory.SetStack(targetSlot.SlotIndex, sourceStack, out _);
                    sourceSlot.Inventory.SetStack(sourceSlot.SlotIndex, targetStack, out _);
                }
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Checks if an item can be dropped on the target slot.
        /// </summary>
        public bool CanDropOnSlot(ItemSlotUI sourceSlot, ItemSlotUI targetSlot)
        {
            if (sourceSlot == null || targetSlot == null)
                return false;

            if (sourceSlot == targetSlot)
                return false;

            if (sourceSlot.IsEmpty)
                return false;

            // Check slot restrictions
            var targetSlotObj = targetSlot.Inventory.GetSlot(targetSlot.SlotIndex);
            if (!targetSlotObj.MeetsRestriction(sourceSlot.CurrentStack))
                return false;

            return true;
        }

        #endregion

        #region Settings

        /// <summary>
        /// Sets whether to use the command system for drag/drop operations.
        /// </summary>
        public void SetUseCommandSystem(bool use)
        {
            useCommandSystem = use;
        }

        /// <summary>
        /// Sets the alpha transparency of the drag image.
        /// </summary>
        public void SetDragImageAlpha(float alpha)
        {
            dragImageAlpha = Mathf.Clamp01(alpha);
        }

        #endregion
    }
}
