using UnityEngine;
using Inventory.Data;

namespace Inventory.Effects
{
    /// <summary>
    /// Interface for item effects that can be applied/removed.
    /// Used for equipment bonuses, consumable effects, etc.
    /// </summary>
    public interface IItemEffect
    {
        /// <summary>
        /// Apply the effect to a target.
        /// </summary>
        /// <param name="target">The GameObject to apply the effect to</param>
        /// <param name="effectData">Configuration data for the effect</param>
        void Apply(GameObject target, ItemEffectData effectData);

        /// <summary>
        /// Remove the effect from a target.
        /// </summary>
        /// <param name="target">The GameObject to remove the effect from</param>
        /// <param name="effectData">Configuration data for the effect</param>
        void Remove(GameObject target, ItemEffectData effectData);

        /// <summary>
        /// Gets whether this effect can be applied to the target.
        /// </summary>
        bool CanApply(GameObject target, ItemEffectData effectData);

        /// <summary>
        /// Name of the effect for debugging.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Description of what this effect does.
        /// </summary>
        string Description { get; }
    }

    /// <summary>
    /// Base class for item effects with common functionality.
    /// </summary>
    public abstract class ItemEffectBase : IItemEffect
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract void Apply(GameObject target, ItemEffectData effectData);
        public abstract void Remove(GameObject target, ItemEffectData effectData);

        public virtual bool CanApply(GameObject target, ItemEffectData effectData)
        {
            return target != null;
        }

        /// <summary>
        /// Helper to log effect application.
        /// </summary>
        protected void LogApply(GameObject target, string details)
        {
            Debug.Log($"[{Name}] Applied to {target.name}: {details}");
        }

        /// <summary>
        /// Helper to log effect removal.
        /// </summary>
        protected void LogRemove(GameObject target, string details)
        {
            Debug.Log($"[{Name}] Removed from {target.name}: {details}");
        }
    }

    /// <summary>
    /// Factory for creating item effects based on type.
    /// </summary>
    public static class ItemEffectFactory
    {
        /// <summary>
        /// Creates an effect instance based on the effect type.
        /// </summary>
        public static IItemEffect CreateEffect(ItemEffectType effectType)
        {
            return effectType switch
            {
                ItemEffectType.StatModifier => new StatModifierEffect(),
                ItemEffectType.Heal => new HealEffect(),
                ItemEffectType.RestoreMana => new RestoreManaEffect(),
                ItemEffectType.Passive => new PassiveEffect(),
                _ => new NullEffect()
            };
        }

        /// <summary>
        /// Applies all effects from an equipment item to a target.
        /// </summary>
        public static void ApplyEquipmentEffects(GameObject target, EquipmentType equipment)
        {
            if (target == null || equipment == null)
                return;

            foreach (ItemEffectData effectData in equipment.EquippedEffects)
            {
                IItemEffect effect = CreateEffect(effectData.EffectType);
                if (effect.CanApply(target, effectData))
                {
                    effect.Apply(target, effectData);
                }
            }
        }

        /// <summary>
        /// Removes all effects from an equipment item from a target.
        /// </summary>
        public static void RemoveEquipmentEffects(GameObject target, EquipmentType equipment)
        {
            if (target == null || equipment == null)
                return;

            foreach (ItemEffectData effectData in equipment.EquippedEffects)
            {
                IItemEffect effect = CreateEffect(effectData.EffectType);
                effect.Remove(target, effectData);
            }
        }
    }

    /// <summary>
    /// Null effect for unsupported effect types.
    /// </summary>
    public class NullEffect : ItemEffectBase
    {
        public override string Name => "NullEffect";
        public override string Description => "No effect";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            Debug.LogWarning($"NullEffect: Effect type {effectData.EffectType} not implemented");
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            // Nothing to remove
        }
    }
}
