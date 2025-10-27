using UnityEngine;
using System;
using System.Collections.Generic;

namespace Inventory.Data
{
    /// <summary>
    /// Represents a unique item instance with modifiers, durability, and custom properties.
    /// Unlike ItemStack which is for stackable items, ItemInstance is for unique items
    /// that need individual tracking (e.g., legendary weapons with random stats).
    /// </summary>
    [System.Serializable]
    public class ItemInstance
    {
        // Base item reference
        [SerializeField] private ItemType itemType;

        // Unique identification
        [SerializeField] private Guid instanceID;

        // Durability system
        [SerializeField] private int currentDurability;
        [SerializeField] private int maxDurability;

        // Item modifiers (enchantments, random stats, etc.)
        [SerializeField] private List<ItemModifier> modifiers = new List<ItemModifier>();

        // Custom properties
        [SerializeField] private string customName;
        [SerializeField] private string customDescription;
        [SerializeField] private ItemQuality quality = ItemQuality.Normal;

        // Timestamps
        [SerializeField] private DateTime createdTime;
        [SerializeField] private DateTime lastModifiedTime;

        // Flags
        [SerializeField] private bool isSoulbound;
        [SerializeField] private bool isCursed;
        [SerializeField] private bool isIndestructible;

        #region Properties

        /// <summary>
        /// The base item type this instance is based on.
        /// </summary>
        public ItemType ItemType => itemType;

        /// <summary>
        /// Unique identifier for this item instance.
        /// </summary>
        public Guid InstanceID => instanceID;

        /// <summary>
        /// Current durability value.
        /// </summary>
        public int CurrentDurability => currentDurability;

        /// <summary>
        /// Maximum durability value.
        /// </summary>
        public int MaxDurability => maxDurability;

        /// <summary>
        /// All modifiers applied to this item.
        /// </summary>
        public List<ItemModifier> Modifiers => modifiers;

        /// <summary>
        /// Custom display name (if set, overrides base item name).
        /// </summary>
        public string CustomName => customName;

        /// <summary>
        /// Custom description (if set, overrides base item description).
        /// </summary>
        public string CustomDescription => customDescription;

        /// <summary>
        /// Item quality tier.
        /// </summary>
        public ItemQuality Quality => quality;

        /// <summary>
        /// When this item instance was created.
        /// </summary>
        public DateTime CreatedTime => createdTime;

        /// <summary>
        /// When this item was last modified.
        /// </summary>
        public DateTime LastModifiedTime => lastModifiedTime;

        /// <summary>
        /// Whether this item is bound to a specific player.
        /// </summary>
        public bool IsSoulbound => isSoulbound;

        /// <summary>
        /// Whether this item has a curse (cannot be unequipped/dropped).
        /// </summary>
        public bool IsCursed => isCursed;

        /// <summary>
        /// Whether this item can be destroyed (cannot lose durability).
        /// </summary>
        public bool IsIndestructible => isIndestructible;

        /// <summary>
        /// Display name (custom name if set, otherwise base item name).
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(customName) ? itemType.Name : customName;

        /// <summary>
        /// Whether this item is broken (durability at 0).
        /// </summary>
        public bool IsBroken => currentDurability <= 0 && maxDurability > 0;

        /// <summary>
        /// Durability as a percentage (0-1).
        /// </summary>
        public float DurabilityPercent => maxDurability > 0 ? (float)currentDurability / maxDurability : 1f;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new item instance from a base item type.
        /// </summary>
        public ItemInstance(ItemType itemType, ItemQuality quality = ItemQuality.Normal)
        {
            this.itemType = itemType;
            this.instanceID = Guid.NewGuid();
            this.quality = quality;
            this.createdTime = DateTime.Now;
            this.lastModifiedTime = DateTime.Now;

            // Initialize durability
            if (itemType.HasDurability)
            {
                this.maxDurability = CalculateBaseDurability();
                this.currentDurability = maxDurability;
            }
            else
            {
                this.maxDurability = 0;
                this.currentDurability = 0;
            }
        }

        /// <summary>
        /// Private constructor for deserialization.
        /// </summary>
        private ItemInstance() { }

        #endregion

        #region Durability Management

        /// <summary>
        /// Damages the item, reducing durability.
        /// </summary>
        /// <param name="amount">Amount of durability to remove</param>
        /// <returns>True if durability was reduced</returns>
        public bool DamageDurability(int amount)
        {
            if (isIndestructible || maxDurability <= 0)
                return false;

            int oldDurability = currentDurability;
            currentDurability = Mathf.Max(0, currentDurability - amount);
            lastModifiedTime = DateTime.Now;

            if (currentDurability != oldDurability)
            {
                Debug.Log($"Item {DisplayName} damaged: {oldDurability} -> {currentDurability}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Repairs the item, restoring durability.
        /// </summary>
        /// <param name="amount">Amount of durability to restore</param>
        /// <returns>True if durability was restored</returns>
        public bool RepairDurability(int amount)
        {
            if (maxDurability <= 0)
                return false;

            int oldDurability = currentDurability;
            currentDurability = Mathf.Min(maxDurability, currentDurability + amount);
            lastModifiedTime = DateTime.Now;

            if (currentDurability != oldDurability)
            {
                Debug.Log($"Item {DisplayName} repaired: {oldDurability} -> {currentDurability}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Fully repairs the item to max durability.
        /// </summary>
        public void FullRepair()
        {
            currentDurability = maxDurability;
            lastModifiedTime = DateTime.Now;
        }

        private int CalculateBaseDurability()
        {
            // Base durability varies by quality
            int baseDurability = 100;

            switch (quality)
            {
                case ItemQuality.Poor: baseDurability = 50; break;
                case ItemQuality.Normal: baseDurability = 100; break;
                case ItemQuality.Superior: baseDurability = 150; break;
                case ItemQuality.Exceptional: baseDurability = 200; break;
                case ItemQuality.Masterwork: baseDurability = 250; break;
                case ItemQuality.Legendary: baseDurability = 300; break;
            }

            return baseDurability;
        }

        #endregion

        #region Modifier Management

        /// <summary>
        /// Adds a modifier to this item.
        /// </summary>
        public void AddModifier(ItemModifier modifier)
        {
            if (modifier == null)
            {
                Debug.LogWarning("Cannot add null modifier");
                return;
            }

            modifiers.Add(modifier);
            lastModifiedTime = DateTime.Now;
            Debug.Log($"Added modifier {modifier.ModifierName} to {DisplayName}");
        }

        /// <summary>
        /// Removes a modifier from this item.
        /// </summary>
        public bool RemoveModifier(ItemModifier modifier)
        {
            if (modifiers.Remove(modifier))
            {
                lastModifiedTime = DateTime.Now;
                Debug.Log($"Removed modifier {modifier.ModifierName} from {DisplayName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes all modifiers from this item.
        /// </summary>
        public void ClearModifiers()
        {
            modifiers.Clear();
            lastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// Gets the total value of all modifiers for a specific stat.
        /// </summary>
        public float GetModifierTotal(string statName, bool percentageOnly = false)
        {
            float total = 0f;

            foreach (var modifier in modifiers)
            {
                if (modifier.StatName == statName)
                {
                    if (percentageOnly && modifier.IsPercentage)
                    {
                        total += modifier.Value;
                    }
                    else if (!percentageOnly && !modifier.IsPercentage)
                    {
                        total += modifier.Value;
                    }
                }
            }

            return total;
        }

        #endregion

        #region Custom Properties

        /// <summary>
        /// Sets a custom name for this item.
        /// </summary>
        public void SetCustomName(string name)
        {
            customName = name;
            lastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// Sets a custom description for this item.
        /// </summary>
        public void SetCustomDescription(string description)
        {
            customDescription = description;
            lastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// Sets whether this item is soulbound.
        /// </summary>
        public void SetSoulbound(bool soulbound)
        {
            isSoulbound = soulbound;
            lastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// Sets whether this item is cursed.
        /// </summary>
        public void SetCursed(bool cursed)
        {
            isCursed = cursed;
            lastModifiedTime = DateTime.Now;
        }

        /// <summary>
        /// Sets whether this item is indestructible.
        /// </summary>
        public void SetIndestructible(bool indestructible)
        {
            isIndestructible = indestructible;
            lastModifiedTime = DateTime.Now;
        }

        #endregion

        #region Value Calculation

        /// <summary>
        /// Gets the total value of this item including modifiers and quality.
        /// </summary>
        public int GetTotalValue()
        {
            float baseValue = itemType.Value;

            // Quality multiplier
            float qualityMultiplier = GetQualityMultiplier();
            baseValue *= qualityMultiplier;

            // Modifier value
            float modifierValue = 0f;
            foreach (var modifier in modifiers)
            {
                modifierValue += modifier.GetValue();
            }

            // Durability penalty
            float durabilityMultiplier = 1f;
            if (maxDurability > 0)
            {
                durabilityMultiplier = DurabilityPercent;
            }

            return Mathf.RoundToInt((baseValue + modifierValue) * durabilityMultiplier);
        }

        private float GetQualityMultiplier()
        {
            return quality switch
            {
                ItemQuality.Poor => 0.5f,
                ItemQuality.Normal => 1f,
                ItemQuality.Superior => 1.5f,
                ItemQuality.Exceptional => 2f,
                ItemQuality.Masterwork => 3f,
                ItemQuality.Legendary => 5f,
                _ => 1f
            };
        }

        #endregion

        #region Display

        /// <summary>
        /// Gets a formatted description of this item including all modifiers.
        /// </summary>
        public string GetFullDescription()
        {
            var desc = new System.Text.StringBuilder();

            desc.AppendLine($"<b>{DisplayName}</b>");
            desc.AppendLine($"Quality: {quality}");

            if (maxDurability > 0)
            {
                desc.AppendLine($"Durability: {currentDurability}/{maxDurability} ({DurabilityPercent * 100:F0}%)");
            }

            if (modifiers.Count > 0)
            {
                desc.AppendLine("\n<b>Modifiers:</b>");
                foreach (var modifier in modifiers)
                {
                    desc.AppendLine($"  {modifier.GetDisplayText()}");
                }
            }

            if (isSoulbound)
            {
                desc.AppendLine("\n<color=cyan>Soulbound</color>");
            }

            if (isCursed)
            {
                desc.AppendLine("<color=red>Cursed</color>");
            }

            if (isIndestructible)
            {
                desc.AppendLine("<color=green>Indestructible</color>");
            }

            desc.AppendLine($"\nValue: {GetTotalValue()} gold");

            return desc.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Represents a modifier applied to an item instance.
    /// </summary>
    [System.Serializable]
    public class ItemModifier
    {
        [SerializeField] private string modifierName;
        [SerializeField] private string statName;
        [SerializeField] private float value;
        [SerializeField] private bool isPercentage;
        [SerializeField] private ItemModifierTier tier;

        public string ModifierName => modifierName;
        public string StatName => statName;
        public float Value => value;
        public bool IsPercentage => isPercentage;
        public ItemModifierTier Tier => tier;

        public ItemModifier(string modifierName, string statName, float value, bool isPercentage = false, ItemModifierTier tier = ItemModifierTier.Minor)
        {
            this.modifierName = modifierName;
            this.statName = statName;
            this.value = value;
            this.isPercentage = isPercentage;
            this.tier = tier;
        }

        /// <summary>
        /// Gets the gold value of this modifier.
        /// </summary>
        public float GetValue()
        {
            float baseValue = Mathf.Abs(value) * 10f;

            // Tier multiplier
            float tierMultiplier = tier switch
            {
                ItemModifierTier.Minor => 1f,
                ItemModifierTier.Major => 2f,
                ItemModifierTier.Grand => 4f,
                ItemModifierTier.Legendary => 8f,
                _ => 1f
            };

            return baseValue * tierMultiplier;
        }

        /// <summary>
        /// Gets formatted display text for this modifier.
        /// </summary>
        public string GetDisplayText()
        {
            string sign = value >= 0 ? "+" : "";
            string valueText = isPercentage ? $"{value * 100:F0}%" : $"{value:F0}";
            string tierText = tier != ItemModifierTier.Minor ? $" ({tier})" : "";

            return $"{modifierName}: {sign}{valueText} {statName}{tierText}";
        }
    }

    /// <summary>
    /// Item quality tiers affecting base stats and value.
    /// </summary>
    public enum ItemQuality
    {
        Poor,           // 50% stats, 50% value
        Normal,         // 100% stats, 100% value
        Superior,       // 125% stats, 150% value
        Exceptional,    // 150% stats, 200% value
        Masterwork,     // 200% stats, 300% value
        Legendary       // 250% stats, 500% value
    }

    /// <summary>
    /// Modifier strength tiers.
    /// </summary>
    public enum ItemModifierTier
    {
        Minor,      // +1-5
        Major,      // +6-15
        Grand,      // +16-30
        Legendary   // +31+
    }
}
