using UnityEngine;
using System.Collections.Generic;
using Inventory.Effects;

namespace Inventory.Data
{
    /// <summary>
    /// ScriptableObject that defines an equipment item type.
    /// Equipment items can be equipped in specific slots and provide stat effects.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "Inventory/Equipment Type", order = 1)]
    public class EquipmentType : ItemType
    {
        [Header("Equipment Configuration")]
        [Tooltip("Which equipment slot this item can be equipped in")]
        [field: SerializeField] public EquipmentSlot EquipmentSlot { get; private set; } = EquipmentSlot.MainHand;

        [Tooltip("Whether this is a two-handed weapon (occupies both hands)")]
        [field: SerializeField] public bool IsTwoHanded { get; private set; } = false;

        [Header("Requirements")]
        [Tooltip("Minimum level required to equip")]
        [field: SerializeField] public int RequiredLevel { get; private set; } = 1;

        [Tooltip("Required classes (empty = any class can equip)")]
        [field: SerializeField] public List<string> RequiredClasses { get; private set; } = new List<string>();

        [Header("Stats")]
        [Tooltip("Armor value (for armor pieces)")]
        [field: SerializeField] public int Armor { get; private set; } = 0;

        [Tooltip("Damage value (for weapons)")]
        [field: SerializeField] public int Damage { get; private set; } = 0;

        [Tooltip("Attack speed (for weapons, 1.0 = normal)")]
        [field: SerializeField] public float AttackSpeed { get; private set; } = 1.0f;

        [Tooltip("Critical hit chance bonus (0-1 range, 0.05 = 5%)")]
        [field: SerializeField] public float CriticalChance { get; private set; } = 0f;

        [Header("Stat Modifiers")]
        [Tooltip("Strength bonus")]
        [field: SerializeField] public int StrengthBonus { get; private set; } = 0;

        [Tooltip("Dexterity bonus")]
        [field: SerializeField] public int DexterityBonus { get; private set; } = 0;

        [Tooltip("Intelligence bonus")]
        [field: SerializeField] public int IntelligenceBonus { get; private set; } = 0;

        [Tooltip("Vitality bonus")]
        [field: SerializeField] public int VitalityBonus { get; private set; } = 0;

        [Tooltip("Movement speed multiplier (1.0 = normal, 1.1 = 10% faster)")]
        [field: SerializeField] public float MovementSpeedMultiplier { get; private set; } = 1.0f;

        [Header("Effects")]
        [Tooltip("List of effects applied when this equipment is equipped")]
        [SerializeField] private List<ItemEffectData> equippedEffects = new List<ItemEffectData>();

        /// <summary>
        /// Gets the list of effects applied when equipped.
        /// </summary>
        public List<ItemEffectData> EquippedEffects => equippedEffects;

        /// <summary>
        /// Gets whether this equipment has any stat bonuses.
        /// </summary>
        public bool HasStatBonuses
        {
            get
            {
                return StrengthBonus != 0 ||
                       DexterityBonus != 0 ||
                       IntelligenceBonus != 0 ||
                       VitalityBonus != 0 ||
                       Armor != 0 ||
                       Damage != 0 ||
                       CriticalChance != 0 ||
                       MovementSpeedMultiplier != 1.0f;
            }
        }

        /// <summary>
        /// Gets whether this equipment has any special effects.
        /// </summary>
        public bool HasEffects => equippedEffects != null && equippedEffects.Count > 0;

        /// <summary>
        /// Checks if a player meets the requirements to equip this item.
        /// </summary>
        /// <param name="playerLevel">Player's current level</param>
        /// <param name="playerClass">Player's class</param>
        /// <param name="reason">Reason if requirements not met</param>
        /// <returns>True if requirements are met</returns>
        public bool MeetsRequirements(int playerLevel, string playerClass, out string reason)
        {
            // Check level requirement
            if (playerLevel < RequiredLevel)
            {
                reason = $"Requires level {RequiredLevel}";
                return false;
            }

            // Check class requirement (if any specified)
            if (RequiredClasses != null && RequiredClasses.Count > 0)
            {
                if (!RequiredClasses.Contains(playerClass))
                {
                    reason = $"Requires class: {string.Join(" or ", RequiredClasses)}";
                    return false;
                }
            }

            reason = string.Empty;
            return true;
        }

        /// <summary>
        /// Gets a summary of all stat bonuses as a formatted string.
        /// </summary>
        public string GetStatBonusSummary()
        {
            List<string> bonuses = new List<string>();

            if (Armor > 0) bonuses.Add($"+{Armor} Armor");
            if (Damage > 0) bonuses.Add($"+{Damage} Damage");
            if (StrengthBonus != 0) bonuses.Add($"{(StrengthBonus > 0 ? "+" : "")}{StrengthBonus} Strength");
            if (DexterityBonus != 0) bonuses.Add($"{(DexterityBonus > 0 ? "+" : "")}{DexterityBonus} Dexterity");
            if (IntelligenceBonus != 0) bonuses.Add($"{(IntelligenceBonus > 0 ? "+" : "")}{IntelligenceBonus} Intelligence");
            if (VitalityBonus != 0) bonuses.Add($"{(VitalityBonus > 0 ? "+" : "")}{VitalityBonus} Vitality");
            if (CriticalChance > 0) bonuses.Add($"+{CriticalChance * 100:F1}% Crit Chance");
            if (AttackSpeed != 1.0f) bonuses.Add($"{AttackSpeed:F2}x Attack Speed");
            if (MovementSpeedMultiplier != 1.0f) bonuses.Add($"{(MovementSpeedMultiplier - 1) * 100:F0}% Move Speed");

            return bonuses.Count > 0 ? string.Join("\n", bonuses) : "No stat bonuses";
        }

        /// <summary>
        /// Validates the equipment configuration.
        /// </summary>
        private void OnValidate()
        {
            // Force category to Equipment
            if (Category != ItemCategory.Equipment)
            {
                Debug.LogWarning($"{name}: EquipmentType category must be Equipment. Auto-correcting.");
            }

            // Equipment should not be stackable
            if (MaxStack > 1)
            {
                Debug.LogWarning($"{name}: Equipment should not be stackable. Setting MaxStack to 1.");
            }

            // Ensure valid equipment slot
            if (EquipmentSlot == EquipmentSlot.None)
            {
                Debug.LogWarning($"{name}: Equipment must have a valid equipment slot.");
            }

            // Ensure valid level requirement
            if (RequiredLevel < 1)
            {
                RequiredLevel = 1;
            }

            // Ensure valid attack speed
            if (AttackSpeed <= 0)
            {
                AttackSpeed = 1.0f;
            }

            // Ensure valid crit chance
            if (CriticalChance < 0)
            {
                CriticalChance = 0f;
            }
            if (CriticalChance > 1)
            {
                CriticalChance = 1f;
            }

            // Ensure valid movement speed
            if (MovementSpeedMultiplier < 0)
            {
                MovementSpeedMultiplier = 1.0f;
            }
        }

        public override string ToString()
        {
            string slotInfo = IsTwoHanded ? $"{EquipmentSlot} (Two-Handed)" : EquipmentSlot.ToString();
            return $"{Name} [{slotInfo}] ({Rarity})";
        }
    }

    /// <summary>
    /// Serializable data for item effects.
    /// Used to configure effects in the inspector.
    /// </summary>
    [System.Serializable]
    public class ItemEffectData
    {
        [Tooltip("Type of effect")]
        public ItemEffectType EffectType;

        [Tooltip("Stat to modify (if applicable)")]
        public string StatName;

        [Tooltip("Modifier value")]
        public float Value;

        [Tooltip("Whether this is a percentage modifier")]
        public bool IsPercentage;

        [Tooltip("Effect duration in seconds (-1 for permanent)")]
        public float Duration = -1f;

        [Tooltip("Effect description")]
        public string Description;
    }

    /// <summary>
    /// Types of item effects.
    /// </summary>
    public enum ItemEffectType
    {
        StatModifier,      // Modify a stat (strength, armor, etc.)
        Heal,              // Heal health
        RestoreMana,       // Restore mana/energy
        Buff,              // Apply a temporary buff
        Debuff,            // Apply a temporary debuff
        DamageOverTime,    // Apply damage over time
        HealOverTime,      // Apply healing over time
        Passive,           // Passive effect (aura, etc.)
        OnHit,             // Trigger on hit
        OnKill,            // Trigger on kill
        Custom             // Custom effect
    }
}
