using UnityEngine;
using Inventory.Data;

namespace Inventory.Effects
{
    /// <summary>
    /// Effect that modifies a stat on the target.
    /// Can be used for equipment bonuses, buffs, debuffs, etc.
    /// </summary>
    public class StatModifierEffect : ItemEffectBase
    {
        public override string Name => "StatModifier";
        public override string Description => "Modifies a stat value";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null || effectData == null)
                return;

            // Try to find a character stats component
            // This is a placeholder - replace with your actual stats system
            var statsComponent = target.GetComponent<ICharacterStats>();
            if (statsComponent != null)
            {
                ApplyToStatsComponent(statsComponent, effectData);
                LogApply(target, $"{effectData.StatName} {(effectData.Value > 0 ? "+" : "")}{effectData.Value}{(effectData.IsPercentage ? "%" : "")}");
            }
            else
            {
                // Fallback: Store modifiers in a component for later use
                var modifierStorage = target.GetComponent<StatModifierStorage>();
                if (modifierStorage == null)
                {
                    modifierStorage = target.AddComponent<StatModifierStorage>();
                }
                modifierStorage.AddModifier(effectData);
                LogApply(target, $"Stored modifier: {effectData.StatName} {(effectData.Value > 0 ? "+" : "")}{effectData.Value}");
            }
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            if (target == null || effectData == null)
                return;

            var statsComponent = target.GetComponent<ICharacterStats>();
            if (statsComponent != null)
            {
                RemoveFromStatsComponent(statsComponent, effectData);
                LogRemove(target, $"{effectData.StatName} modifier removed");
            }
            else
            {
                var modifierStorage = target.GetComponent<StatModifierStorage>();
                if (modifierStorage != null)
                {
                    modifierStorage.RemoveModifier(effectData);
                    LogRemove(target, $"Removed stored modifier: {effectData.StatName}");
                }
            }
        }

        public override bool CanApply(GameObject target, ItemEffectData effectData)
        {
            if (!base.CanApply(target, effectData))
                return false;

            if (string.IsNullOrEmpty(effectData.StatName))
            {
                Debug.LogWarning("StatModifierEffect: StatName is null or empty");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Applies the modifier to a character stats component.
        /// Override this if you have a custom stats system.
        /// </summary>
        protected virtual void ApplyToStatsComponent(ICharacterStats stats, ItemEffectData effectData)
        {
            if (effectData.IsPercentage)
            {
                stats.AddPercentageModifier(effectData.StatName, effectData.Value);
            }
            else
            {
                stats.AddFlatModifier(effectData.StatName, effectData.Value);
            }
        }

        /// <summary>
        /// Removes the modifier from a character stats component.
        /// Override this if you have a custom stats system.
        /// </summary>
        protected virtual void RemoveFromStatsComponent(ICharacterStats stats, ItemEffectData effectData)
        {
            if (effectData.IsPercentage)
            {
                stats.RemovePercentageModifier(effectData.StatName, effectData.Value);
            }
            else
            {
                stats.RemoveFlatModifier(effectData.StatName, effectData.Value);
            }
        }
    }

    /// <summary>
    /// Interface for character stats systems.
    /// Implement this on your character/player controller to enable stat modification.
    /// </summary>
    public interface ICharacterStats
    {
        /// <summary>
        /// Adds a flat modifier to a stat (e.g., +10 Strength).
        /// </summary>
        void AddFlatModifier(string statName, float value);

        /// <summary>
        /// Removes a flat modifier from a stat.
        /// </summary>
        void RemoveFlatModifier(string statName, float value);

        /// <summary>
        /// Adds a percentage modifier to a stat (e.g., +10% Strength).
        /// </summary>
        void AddPercentageModifier(string statName, float value);

        /// <summary>
        /// Removes a percentage modifier from a stat.
        /// </summary>
        void RemovePercentageModifier(string statName, float value);

        /// <summary>
        /// Gets the final value of a stat after all modifiers.
        /// </summary>
        float GetStat(string statName);
    }

    /// <summary>
    /// Component that stores stat modifiers when no character stats component exists.
    /// This allows the inventory system to work without requiring a stats system.
    /// </summary>
    public class StatModifierStorage : MonoBehaviour
    {
        [SerializeField]
        private System.Collections.Generic.List<ItemEffectData> activeModifiers =
            new System.Collections.Generic.List<ItemEffectData>();

        /// <summary>
        /// Gets all active modifiers.
        /// </summary>
        public System.Collections.Generic.List<ItemEffectData> ActiveModifiers => activeModifiers;

        /// <summary>
        /// Adds a modifier to storage.
        /// </summary>
        public void AddModifier(ItemEffectData modifier)
        {
            if (modifier != null)
            {
                activeModifiers.Add(modifier);
            }
        }

        /// <summary>
        /// Removes a modifier from storage.
        /// </summary>
        public void RemoveModifier(ItemEffectData modifier)
        {
            if (modifier != null)
            {
                // Remove the first matching modifier
                for (int i = 0; i < activeModifiers.Count; i++)
                {
                    if (activeModifiers[i].StatName == modifier.StatName &&
                        activeModifiers[i].Value == modifier.Value &&
                        activeModifiers[i].IsPercentage == modifier.IsPercentage)
                    {
                        activeModifiers.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the total modifier for a specific stat.
        /// </summary>
        public float GetTotalModifier(string statName, bool percentage = false)
        {
            float total = 0f;
            foreach (var modifier in activeModifiers)
            {
                if (modifier.StatName == statName && modifier.IsPercentage == percentage)
                {
                    total += modifier.Value;
                }
            }
            return total;
        }

        /// <summary>
        /// Clears all modifiers.
        /// </summary>
        public void ClearAllModifiers()
        {
            activeModifiers.Clear();
        }
    }
}
