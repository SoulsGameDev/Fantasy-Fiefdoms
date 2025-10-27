using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Inventory.Data;

namespace Inventory.Effects
{
    /// <summary>
    /// Manages active effects on a character, including duration tracking,
    /// periodic ticking (DoT/HoT), and effect cleanup.
    ///
    /// Attach this component to any character that can receive temporary effects.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

        // Events
        public event System.Action<ActiveEffect> OnEffectApplied;
        public event System.Action<ActiveEffect> OnEffectRemoved;
        public event System.Action<ActiveEffect> OnEffectTicked;

        // Optional: Reference to character stats component
        private ICharacterStats characterStats;

        #region Unity Lifecycle

        private void Awake()
        {
            // Try to find character stats component
            characterStats = GetComponent<ICharacterStats>();
        }

        private void Update()
        {
            UpdateActiveEffects();
        }

        #endregion

        #region Effect Application

        /// <summary>
        /// Applies a new effect to this character.
        /// If an effect of the same type and stat already exists, it will be refreshed.
        /// </summary>
        public void ApplyEffect(ActiveEffect effect)
        {
            if (effect == null)
            {
                Debug.LogWarning("Attempted to apply null effect");
                return;
            }

            // Check if we already have this effect
            ActiveEffect existing = FindEffect(effect.EffectType, effect.StatName);

            if (existing != null)
            {
                // Refresh the existing effect
                existing.StartTime = Time.time;
                existing.Duration = effect.Duration;
                existing.Value = effect.Value;
                existing.NextTickTime = Time.time + existing.TickRate;

                Debug.Log($"Refreshed effect: {effect.Description} on {gameObject.name}");
            }
            else
            {
                // Add new effect
                effect.NextTickTime = Time.time + effect.TickRate;
                activeEffects.Add(effect);

                // Apply immediate stat modification for buffs/debuffs
                if ((effect.EffectType == ItemEffectType.Buff || effect.EffectType == ItemEffectType.Debuff)
                    && characterStats != null && !string.IsNullOrEmpty(effect.StatName))
                {
                    if (effect.IsPercentage)
                    {
                        characterStats.AddPercentageModifier(effect.StatName, effect.Value);
                    }
                    else
                    {
                        characterStats.AddFlatModifier(effect.StatName, effect.Value);
                    }
                }

                Debug.Log($"Applied effect: {effect.Description} on {gameObject.name} for {effect.Duration}s");
                OnEffectApplied?.Invoke(effect);
            }
        }

        /// <summary>
        /// Removes a specific effect by type and stat name.
        /// </summary>
        public void RemoveEffect(ItemEffectType effectType, string statName)
        {
            ActiveEffect effect = FindEffect(effectType, statName);
            if (effect != null)
            {
                RemoveEffect(effect);
            }
        }

        /// <summary>
        /// Removes a specific active effect.
        /// </summary>
        public void RemoveEffect(ActiveEffect effect)
        {
            if (effect == null || !activeEffects.Contains(effect))
                return;

            // Remove stat modification for buffs/debuffs
            if ((effect.EffectType == ItemEffectType.Buff || effect.EffectType == ItemEffectType.Debuff)
                && characterStats != null && !string.IsNullOrEmpty(effect.StatName))
            {
                if (effect.IsPercentage)
                {
                    characterStats.RemovePercentageModifier(effect.StatName, effect.Value);
                }
                else
                {
                    characterStats.RemoveFlatModifier(effect.StatName, effect.Value);
                }
            }

            activeEffects.Remove(effect);
            Debug.Log($"Removed effect: {effect.Description} from {gameObject.name}");
            OnEffectRemoved?.Invoke(effect);
        }

        /// <summary>
        /// Removes all effects of a specific type.
        /// </summary>
        public void RemoveAllEffectsOfType(ItemEffectType effectType)
        {
            var effectsToRemove = activeEffects.Where(e => e.EffectType == effectType).ToList();
            foreach (var effect in effectsToRemove)
            {
                RemoveEffect(effect);
            }
        }

        /// <summary>
        /// Removes all active effects.
        /// </summary>
        public void RemoveAllEffects()
        {
            var effectsToRemove = activeEffects.ToList();
            foreach (var effect in effectsToRemove)
            {
                RemoveEffect(effect);
            }
        }

        #endregion

        #region Effect Queries

        /// <summary>
        /// Finds an active effect by type and stat name.
        /// </summary>
        public ActiveEffect FindEffect(ItemEffectType effectType, string statName)
        {
            return activeEffects.FirstOrDefault(e =>
                e.EffectType == effectType &&
                e.StatName == statName);
        }

        /// <summary>
        /// Gets all active effects of a specific type.
        /// </summary>
        public List<ActiveEffect> GetEffectsOfType(ItemEffectType effectType)
        {
            return activeEffects.Where(e => e.EffectType == effectType).ToList();
        }

        /// <summary>
        /// Gets all currently active effects.
        /// </summary>
        public List<ActiveEffect> GetAllActiveEffects()
        {
            return new List<ActiveEffect>(activeEffects);
        }

        /// <summary>
        /// Checks if this character has a specific effect active.
        /// </summary>
        public bool HasEffect(ItemEffectType effectType, string statName)
        {
            return FindEffect(effectType, statName) != null;
        }

        /// <summary>
        /// Gets the total number of active effects.
        /// </summary>
        public int ActiveEffectCount => activeEffects.Count;

        #endregion

        #region Update Loop

        private void UpdateActiveEffects()
        {
            // Process effects in reverse order so we can safely remove during iteration
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveEffect effect = activeEffects[i];

                // Check if effect has expired
                if (effect.IsExpired)
                {
                    RemoveEffect(effect);
                    continue;
                }

                // Process ticking effects (DoT/HoT)
                if (effect.ShouldTick)
                {
                    ProcessEffectTick(effect);
                }
            }
        }

        private void ProcessEffectTick(ActiveEffect effect)
        {
            if (characterStats == null)
            {
                Debug.LogWarning($"Cannot tick effect {effect.Description} - no ICharacterStats component found");
                return;
            }

            // Apply the tick value
            switch (effect.EffectType)
            {
                case ItemEffectType.DamageOverTime:
                    // Damage is negative
                    ApplyHealthChange(effect.Value, effect.IsPercentage);
                    break;

                case ItemEffectType.HealOverTime:
                    // Healing is positive
                    ApplyHealthChange(effect.Value, effect.IsPercentage);
                    break;
            }

            OnEffectTicked?.Invoke(effect);
        }

        private void ApplyHealthChange(float value, bool isPercentage)
        {
            if (characterStats == null) return;

            if (isPercentage)
            {
                // TODO: Apply percentage-based health change
                // This would need max health information
                Debug.Log($"Applied {value * 100}% health change to {gameObject.name}");
            }
            else
            {
                // Apply flat health change
                // Note: Negative values for damage, positive for healing
                Debug.Log($"Applied {value} health change to {gameObject.name}");

                // In a real implementation, you would call something like:
                // characterStats.ModifyHealth(value);
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Gets a formatted string of all active effects for debugging.
        /// </summary>
        public string GetEffectSummary()
        {
            if (activeEffects.Count == 0)
                return "No active effects";

            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Active Effects ({activeEffects.Count}):");

            foreach (var effect in activeEffects)
            {
                summary.AppendLine($"  - {effect.Description} ({effect.RemainingDuration:F1}s remaining)");
            }

            return summary.ToString();
        }

        #endregion

        #region Editor Debugging

#if UNITY_EDITOR
        [ContextMenu("Print Active Effects")]
        private void PrintActiveEffects()
        {
            Debug.Log(GetEffectSummary());
        }

        [ContextMenu("Clear All Effects")]
        private void ClearAllEffects()
        {
            RemoveAllEffects();
            Debug.Log("Cleared all active effects");
        }
#endif

        #endregion
    }

    /// <summary>
    /// Interface for character stats. Implement this on your character/player class
    /// to enable stat modifications from items and effects.
    /// </summary>
    public interface ICharacterStats
    {
        void AddFlatModifier(string statName, float value);
        void RemoveFlatModifier(string statName, float value);
        void AddPercentageModifier(string statName, float value);
        void RemovePercentageModifier(string statName, float value);
    }
}
