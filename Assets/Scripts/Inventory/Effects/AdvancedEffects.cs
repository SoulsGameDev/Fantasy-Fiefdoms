using UnityEngine;
using Inventory.Data;
using System.Collections;

namespace Inventory.Effects
{
    /// <summary>
    /// Effect that applies a temporary buff to a character.
    /// Buffs increase stats or grant beneficial status effects.
    /// </summary>
    public class BuffEffect : ItemEffectBase
    {
        public override string Name => "Buff";
        public override string Description => "Applies a beneficial temporary effect";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            // Try to get effect manager
            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager == null)
            {
                effectManager = target.AddComponent<EffectManager>();
            }

            // Apply buff
            effectManager.ApplyEffect(new ActiveEffect
            {
                EffectType = ItemEffectType.Buff,
                StatName = effectData.StatName,
                Value = effectData.Value,
                IsPercentage = effectData.IsPercentage,
                Duration = effectData.Duration,
                Description = effectData.Description,
                StartTime = Time.time
            });

            LogApply(target, $"{effectData.Description} for {effectData.Duration}s");
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager != null)
            {
                effectManager.RemoveEffect(ItemEffectType.Buff, effectData.StatName);
            }

            LogRemove(target, effectData.Description);
        }
    }

    /// <summary>
    /// Effect that applies a temporary debuff to a character.
    /// Debuffs decrease stats or apply harmful status effects.
    /// </summary>
    public class DebuffEffect : ItemEffectBase
    {
        public override string Name => "Debuff";
        public override string Description => "Applies a harmful temporary effect";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager == null)
            {
                effectManager = target.AddComponent<EffectManager>();
            }

            effectManager.ApplyEffect(new ActiveEffect
            {
                EffectType = ItemEffectType.Debuff,
                StatName = effectData.StatName,
                Value = effectData.Value,
                IsPercentage = effectData.IsPercentage,
                Duration = effectData.Duration,
                Description = effectData.Description,
                StartTime = Time.time
            });

            LogApply(target, $"{effectData.Description} for {effectData.Duration}s");
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager != null)
            {
                effectManager.RemoveEffect(ItemEffectType.Debuff, effectData.StatName);
            }

            LogRemove(target, effectData.Description);
        }
    }

    /// <summary>
    /// Effect that deals damage over time (DoT).
    /// </summary>
    public class DamageOverTimeEffect : ItemEffectBase
    {
        public override string Name => "Damage Over Time";
        public override string Description => "Deals periodic damage";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager == null)
            {
                effectManager = target.AddComponent<EffectManager>();
            }

            effectManager.ApplyEffect(new ActiveEffect
            {
                EffectType = ItemEffectType.DamageOverTime,
                StatName = "Health",
                Value = -Mathf.Abs(effectData.Value), // Ensure negative for damage
                IsPercentage = effectData.IsPercentage,
                Duration = effectData.Duration,
                Description = effectData.Description,
                StartTime = Time.time,
                TickRate = 1f // Damage per second
            });

            LogApply(target, $"{Mathf.Abs(effectData.Value)} damage over {effectData.Duration}s");
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager != null)
            {
                effectManager.RemoveEffect(ItemEffectType.DamageOverTime, "Health");
            }

            LogRemove(target, "DoT effect");
        }
    }

    /// <summary>
    /// Effect that heals over time (HoT).
    /// </summary>
    public class HealOverTimeEffect : ItemEffectBase
    {
        public override string Name => "Heal Over Time";
        public override string Description => "Provides periodic healing";

        public override void Apply(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager == null)
            {
                effectManager = target.AddComponent<EffectManager>();
            }

            effectManager.ApplyEffect(new ActiveEffect
            {
                EffectType = ItemEffectType.HealOverTime,
                StatName = "Health",
                Value = Mathf.Abs(effectData.Value), // Ensure positive for healing
                IsPercentage = effectData.IsPercentage,
                Duration = effectData.Duration,
                Description = effectData.Description,
                StartTime = Time.time,
                TickRate = 1f // Heal per second
            });

            LogApply(target, $"{Mathf.Abs(effectData.Value)} healing over {effectData.Duration}s");
        }

        public override void Remove(GameObject target, ItemEffectData effectData)
        {
            if (target == null) return;

            var effectManager = target.GetComponent<EffectManager>();
            if (effectManager != null)
            {
                effectManager.RemoveEffect(ItemEffectType.HealOverTime, "Health");
            }

            LogRemove(target, "HoT effect");
        }
    }

    /// <summary>
    /// Represents an active effect on a character.
    /// </summary>
    [System.Serializable]
    public class ActiveEffect
    {
        public ItemEffectType EffectType;
        public string StatName;
        public float Value;
        public bool IsPercentage;
        public float Duration;
        public string Description;
        public float StartTime;
        public float TickRate = 1f; // For DoT/HoT effects
        public float NextTickTime;

        /// <summary>
        /// Gets the remaining duration of this effect.
        /// </summary>
        public float RemainingDuration => Mathf.Max(0, Duration - (Time.time - StartTime));

        /// <summary>
        /// Whether this effect has expired.
        /// </summary>
        public bool IsExpired => Time.time >= StartTime + Duration;

        /// <summary>
        /// Whether it's time for this effect to tick.
        /// </summary>
        public bool ShouldTick
        {
            get
            {
                if (EffectType != ItemEffectType.DamageOverTime && EffectType != ItemEffectType.HealOverTime)
                    return false;

                if (Time.time >= NextTickTime)
                {
                    NextTickTime = Time.time + TickRate;
                    return true;
                }

                return false;
            }
        }
    }
}
