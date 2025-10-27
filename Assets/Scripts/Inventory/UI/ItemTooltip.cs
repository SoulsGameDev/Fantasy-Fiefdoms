using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Inventory.Data;

namespace Inventory.UI
{
    /// <summary>
    /// Displays detailed information about an item when hovering over it.
    /// Shows name, description, stats, requirements, and effects.
    /// </summary>
    public class ItemTooltip : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform tooltipRect;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private TextMeshProUGUI requirementsText;
        [SerializeField] private TextMeshProUGUI effectsText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image borderImage;

        [Header("Layout")]
        [SerializeField] private float offsetX = 10f;
        [SerializeField] private float offsetY = -10f;
        [SerializeField] private float padding = 10f;
        [SerializeField] private float maxWidth = 300f;

        [Header("Colors")]
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.95f);

        private Canvas canvas;
        private bool isVisible;

        #region Initialization

        private void Awake()
        {
            // Find canvas
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            // Auto-find tooltip rect
            if (tooltipRect == null)
            {
                tooltipRect = GetComponent<RectTransform>();
            }

            // Setup background
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }

            Hide();
        }

        #endregion

        #region Show/Hide

        /// <summary>
        /// Shows the tooltip for the specified item stack.
        /// </summary>
        public void Show(ItemStack stack, Vector3 position)
        {
            if (stack.IsEmpty || stack.Type == null)
            {
                Hide();
                return;
            }

            UpdateContent(stack);
            UpdatePosition(position);

            gameObject.SetActive(true);
            isVisible = true;
        }

        /// <summary>
        /// Hides the tooltip.
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            isVisible = false;
        }

        /// <summary>
        /// Gets whether the tooltip is currently visible.
        /// </summary>
        public bool IsVisible => isVisible;

        #endregion

        #region Content

        /// <summary>
        /// Updates the tooltip content based on the item stack.
        /// </summary>
        private void UpdateContent(ItemStack stack)
        {
            ItemType item = stack.Type;

            // Item name with rarity color
            if (itemNameText != null)
            {
                itemNameText.text = item.Name;
                itemNameText.color = item.RarityColor;
            }

            // Item type and category
            if (itemTypeText != null)
            {
                itemTypeText.text = $"{item.Category} - {item.Rarity}";
            }

            // Description
            if (descriptionText != null)
            {
                descriptionText.text = item.Description;
            }

            // Stats (if equipment)
            if (statsText != null)
            {
                if (item is EquipmentType equipment)
                {
                    statsText.gameObject.SetActive(true);
                    statsText.text = FormatEquipmentStats(equipment);
                }
                else
                {
                    statsText.gameObject.SetActive(false);
                }
            }

            // Requirements (if equipment)
            if (requirementsText != null)
            {
                if (item is EquipmentType equipment)
                {
                    string reqText = FormatRequirements(equipment);
                    if (!string.IsNullOrEmpty(reqText))
                    {
                        requirementsText.gameObject.SetActive(true);
                        requirementsText.text = reqText;
                    }
                    else
                    {
                        requirementsText.gameObject.SetActive(false);
                    }
                }
                else
                {
                    requirementsText.gameObject.SetActive(false);
                }
            }

            // Effects (if equipment)
            if (effectsText != null)
            {
                if (item is EquipmentType equipment && equipment.HasEffects)
                {
                    effectsText.gameObject.SetActive(true);
                    effectsText.text = FormatEffects(equipment);
                }
                else
                {
                    effectsText.gameObject.SetActive(false);
                }
            }

            // Border color based on rarity
            if (borderImage != null)
            {
                borderImage.color = item.RarityColor;
            }

            // Additional info
            if (stack.Quantity > 1)
            {
                if (descriptionText != null)
                {
                    descriptionText.text += $"\n\nQuantity: {stack.Quantity}";
                }
            }

            if (stack.HasDurability)
            {
                if (descriptionText != null)
                {
                    descriptionText.text += $"\nDurability: {stack.Durability}/{stack.Type.MaxDurability}";
                }
            }
        }

        private string FormatEquipmentStats(EquipmentType equipment)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Stats:</b>");

            if (equipment.Armor > 0)
                sb.AppendLine($"  Armor: {equipment.Armor}");

            if (equipment.Damage > 0)
                sb.AppendLine($"  Damage: {equipment.Damage}");

            if (equipment.AttackSpeed != 1.0f)
                sb.AppendLine($"  Attack Speed: {equipment.AttackSpeed:F2}x");

            if (equipment.CriticalChance > 0)
                sb.AppendLine($"  Crit Chance: +{equipment.CriticalChance * 100:F1}%");

            if (equipment.StrengthBonus != 0)
                sb.AppendLine($"  {FormatBonus("Strength", equipment.StrengthBonus)}");

            if (equipment.DexterityBonus != 0)
                sb.AppendLine($"  {FormatBonus("Dexterity", equipment.DexterityBonus)}");

            if (equipment.IntelligenceBonus != 0)
                sb.AppendLine($"  {FormatBonus("Intelligence", equipment.IntelligenceBonus)}");

            if (equipment.VitalityBonus != 0)
                sb.AppendLine($"  {FormatBonus("Vitality", equipment.VitalityBonus)}");

            if (equipment.MovementSpeedMultiplier != 1.0f)
            {
                float percent = (equipment.MovementSpeedMultiplier - 1.0f) * 100f;
                sb.AppendLine($"  Move Speed: {(percent > 0 ? "+" : "")}{percent:F0}%");
            }

            return sb.ToString().TrimEnd();
        }

        private string FormatBonus(string statName, int value)
        {
            return $"{statName}: {(value > 0 ? "+" : "")}{value}";
        }

        private string FormatRequirements(EquipmentType equipment)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (equipment.RequiredLevel > 1)
            {
                sb.AppendLine($"<b>Requires Level {equipment.RequiredLevel}</b>");
            }

            if (equipment.RequiredClasses != null && equipment.RequiredClasses.Count > 0)
            {
                sb.AppendLine($"<b>Requires:</b> {string.Join(", ", equipment.RequiredClasses)}");
            }

            if (equipment.IsTwoHanded)
            {
                sb.AppendLine("<b>Two-Handed</b>");
            }

            return sb.ToString().TrimEnd();
        }

        private string FormatEffects(EquipmentType equipment)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>Special Effects:</b>");

            foreach (var effect in equipment.EquippedEffects)
            {
                if (!string.IsNullOrEmpty(effect.Description))
                {
                    sb.AppendLine($"  • {effect.Description}");
                }
                else
                {
                    string valueStr = effect.IsPercentage ? $"{effect.Value:F1}%" : effect.Value.ToString();
                    sb.AppendLine($"  • {effect.EffectType}: {effect.StatName} {(effect.Value > 0 ? "+" : "")}{valueStr}");
                }
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Positioning

        /// <summary>
        /// Updates the tooltip position relative to the cursor or slot.
        /// </summary>
        private void UpdatePosition(Vector3 worldPosition)
        {
            if (tooltipRect == null || canvas == null)
                return;

            // Convert world position to screen space
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera,
                worldPosition);

            // Convert to canvas space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPoint,
                canvas.worldCamera,
                out Vector2 localPoint);

            // Apply offset
            localPoint.x += offsetX;
            localPoint.y += offsetY;

            // Keep tooltip within screen bounds
            localPoint = ClampToScreen(localPoint);

            tooltipRect.localPosition = localPoint;
        }

        private Vector2 ClampToScreen(Vector2 position)
        {
            if (canvas == null || tooltipRect == null)
                return position;

            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector2 tooltipSize = tooltipRect.sizeDelta;

            // Get canvas bounds
            Vector2 canvasSize = canvasRect.sizeDelta;
            float minX = -canvasSize.x * 0.5f;
            float maxX = canvasSize.x * 0.5f;
            float minY = -canvasSize.y * 0.5f;
            float maxY = canvasSize.y * 0.5f;

            // Clamp position
            float clampedX = Mathf.Clamp(position.x, minX, maxX - tooltipSize.x);
            float clampedY = Mathf.Clamp(position.y, minY + tooltipSize.y, maxY);

            return new Vector2(clampedX, clampedY);
        }

        #endregion

        #region Update

        private void Update()
        {
            if (isVisible)
            {
                // Follow mouse cursor
                UpdatePosition(Input.mousePosition);
            }
        }

        #endregion
    }
}
