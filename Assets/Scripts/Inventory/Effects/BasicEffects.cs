using UnityEngine;
using Inventory.Data;

namespace Inventory.Effects
{
    /// <summary>
    /// Effect that heals the target.
    /// </summary>
    public class HealEffect : ItemEffectBase
    {
        public override string Name => "Heal";
        public override string Description => "Heals the target";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null || effectData == null)
                return;

            // Try to find a health component
            var health = target.GetComponent<IHealth>();
            if (health != null)
            {
                health.Heal(effectData.Value);
                LogApply(target, $"Healed {effectData.Value} HP");
            }
            else
            {
                LogApply(target, $"No IHealth component found, but effect applied (value: {effectData.Value})");
            }
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            // Heal effects are instant, nothing to remove
        }
    }

    /// <summary>
    /// Effect that restores mana/energy.
    /// </summary>
    public class RestoreManaEffect : ItemEffectBase
    {
        public override string Name => "RestoreMana";
        public override string Description => "Restores mana/energy";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null || effectData == null)
                return;

            // Try to find a mana component
            var mana = target.GetComponent<IMana>();
            if (mana != null)
            {
                mana.RestoreMana(effectData.Value);
                LogApply(target, $"Restored {effectData.Value} mana");
            }
            else
            {
                LogApply(target, $"No IMana component found, but effect applied (value: {effectData.Value})");
            }
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            // Mana restore effects are instant, nothing to remove
        }
    }

    /// <summary>
    /// Passive effect that remains active while equipped.
    /// </summary>
    public class PassiveEffect : ItemEffectBase
    {
        public override string Name => "Passive";
        public override string Description => "Passive aura or ongoing effect";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null || effectData == null)
                return;

            // For passive effects, we just log them
            // You can extend this to add actual passive effect components
            LogApply(target, $"{effectData.Description}");
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            if (target == null || effectData == null)
                return;

            LogRemove(target, $"{effectData.Description}");
        }
    }

    /// <summary>
    /// Interface for health systems.
    /// Implement this on your character/enemy to enable healing effects.
    /// </summary>
    public interface IHealth
    {
        /// <summary>
        /// Current health value.
        /// </summary>
        float CurrentHealth { get; }

        /// <summary>
        /// Maximum health value.
        /// </summary>
        float MaxHealth { get; }

        /// <summary>
        /// Heals the target by the specified amount.
        /// </summary>
        void Heal(float amount);

        /// <summary>
        /// Damages the target by the specified amount.
        /// </summary>
        void TakeDamage(float amount);
    }

    /// <summary>
    /// Interface for mana/energy systems.
    /// Implement this on your character to enable mana restore effects.
    /// </summary>
    public interface IMana
    {
        /// <summary>
        /// Current mana value.
        /// </summary>
        float CurrentMana { get; }

        /// <summary>
        /// Maximum mana value.
        /// </summary>
        float MaxMana { get; }

        /// <summary>
        /// Restores mana by the specified amount.
        /// </summary>
        void RestoreMana(float amount);

        /// <summary>
        /// Consumes mana by the specified amount.
        /// </summary>
        bool ConsumeMana(float amount);
    }
}
