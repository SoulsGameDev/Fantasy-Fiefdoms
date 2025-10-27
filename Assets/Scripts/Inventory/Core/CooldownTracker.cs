using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Inventory.Core
{
    /// <summary>
    /// Tracks item cooldowns for a character.
    /// Manages when items can be used again after consumption.
    ///
    /// Attach this component to any character that needs to track item cooldowns.
    /// </summary>
    public class CooldownTracker : MonoBehaviour
    {
        // Dictionary mapping item ID to cooldown end time
        private Dictionary<string, float> cooldowns = new Dictionary<string, float>();

        // Events
        public event System.Action<string, float> OnCooldownStarted;
        public event System.Action<string> OnCooldownExpired;

        #region Unity Lifecycle

        private void Update()
        {
            UpdateCooldowns();
        }

        #endregion

        #region Cooldown Management

        /// <summary>
        /// Starts a cooldown for an item.
        /// </summary>
        /// <param name="itemID">The item's unique ID</param>
        /// <param name="cooldownSeconds">Duration of the cooldown in seconds</param>
        public void StartCooldown(string itemID, float cooldownSeconds)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogWarning("Attempted to start cooldown with null or empty itemID");
                return;
            }

            if (cooldownSeconds <= 0f)
            {
                Debug.LogWarning($"Invalid cooldown duration: {cooldownSeconds}s for item {itemID}");
                return;
            }

            float endTime = Time.time + cooldownSeconds;

            if (cooldowns.ContainsKey(itemID))
            {
                cooldowns[itemID] = endTime;
            }
            else
            {
                cooldowns.Add(itemID, endTime);
            }

            Debug.Log($"Started cooldown for {itemID}: {cooldownSeconds}s");
            OnCooldownStarted?.Invoke(itemID, cooldownSeconds);
        }

        /// <summary>
        /// Clears the cooldown for a specific item.
        /// </summary>
        /// <param name="itemID">The item's unique ID</param>
        public void ClearCooldown(string itemID)
        {
            if (cooldowns.ContainsKey(itemID))
            {
                cooldowns.Remove(itemID);
                Debug.Log($"Cleared cooldown for {itemID}");
                OnCooldownExpired?.Invoke(itemID);
            }
        }

        /// <summary>
        /// Clears all active cooldowns.
        /// </summary>
        public void ClearAllCooldowns()
        {
            var itemIDs = cooldowns.Keys.ToList();
            cooldowns.Clear();

            foreach (var itemID in itemIDs)
            {
                OnCooldownExpired?.Invoke(itemID);
            }

            Debug.Log("Cleared all cooldowns");
        }

        /// <summary>
        /// Reduces the remaining time on a cooldown.
        /// Useful for abilities or effects that reduce cooldowns.
        /// </summary>
        /// <param name="itemID">The item's unique ID</param>
        /// <param name="reduction">Amount to reduce in seconds</param>
        public void ReduceCooldown(string itemID, float reduction)
        {
            if (!cooldowns.ContainsKey(itemID))
                return;

            cooldowns[itemID] = Mathf.Max(Time.time, cooldowns[itemID] - reduction);
            Debug.Log($"Reduced cooldown for {itemID} by {reduction}s");
        }

        #endregion

        #region Cooldown Queries

        /// <summary>
        /// Checks if an item is currently on cooldown.
        /// </summary>
        /// <param name="itemID">The item's unique ID</param>
        /// <returns>True if the item is on cooldown</returns>
        public bool IsOnCooldown(string itemID)
        {
            if (!cooldowns.ContainsKey(itemID))
                return false;

            return Time.time < cooldowns[itemID];
        }

        /// <summary>
        /// Gets the remaining cooldown time for an item.
        /// </summary>
        /// <param name="itemID">The item's unique ID</param>
        /// <returns>Remaining time in seconds, or 0 if not on cooldown</returns>
        public float GetRemainingCooldown(string itemID)
        {
            if (!cooldowns.ContainsKey(itemID))
                return 0f;

            float remaining = cooldowns[itemID] - Time.time;
            return Mathf.Max(0f, remaining);
        }

        /// <summary>
        /// Gets the cooldown progress as a value from 0 (just started) to 1 (finished).
        /// </summary>
        /// <param name="itemID">The item's unique ID</param>
        /// <param name="totalDuration">The total cooldown duration</param>
        /// <returns>Progress value from 0 to 1</returns>
        public float GetCooldownProgress(string itemID, float totalDuration)
        {
            if (totalDuration <= 0f)
                return 1f;

            float remaining = GetRemainingCooldown(itemID);
            float elapsed = totalDuration - remaining;
            return Mathf.Clamp01(elapsed / totalDuration);
        }

        /// <summary>
        /// Gets all items currently on cooldown.
        /// </summary>
        /// <returns>List of item IDs on cooldown</returns>
        public List<string> GetAllItemsOnCooldown()
        {
            return cooldowns
                .Where(kvp => Time.time < kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();
        }

        /// <summary>
        /// Gets the total number of items on cooldown.
        /// </summary>
        public int CooldownCount => GetAllItemsOnCooldown().Count;

        #endregion

        #region Update Loop

        private void UpdateCooldowns()
        {
            // Check for expired cooldowns
            var expiredCooldowns = new List<string>();

            foreach (var kvp in cooldowns)
            {
                if (Time.time >= kvp.Value)
                {
                    expiredCooldowns.Add(kvp.Key);
                }
            }

            // Remove expired cooldowns
            foreach (var itemID in expiredCooldowns)
            {
                cooldowns.Remove(itemID);
                OnCooldownExpired?.Invoke(itemID);
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Gets a formatted string of all active cooldowns for debugging.
        /// </summary>
        public string GetCooldownSummary()
        {
            var activeCooldowns = GetAllItemsOnCooldown();

            if (activeCooldowns.Count == 0)
                return "No active cooldowns";

            var summary = new System.Text.StringBuilder();
            summary.AppendLine($"Active Cooldowns ({activeCooldowns.Count}):");

            foreach (var itemID in activeCooldowns)
            {
                float remaining = GetRemainingCooldown(itemID);
                summary.AppendLine($"  - {itemID}: {remaining:F1}s remaining");
            }

            return summary.ToString();
        }

        #endregion

        #region Editor Debugging

#if UNITY_EDITOR
        [ContextMenu("Print Active Cooldowns")]
        private void PrintActiveCooldowns()
        {
            Debug.Log(GetCooldownSummary());
        }

        [ContextMenu("Clear All Cooldowns")]
        private void ClearAllCooldownsDebug()
        {
            ClearAllCooldowns();
        }
#endif

        #endregion
    }
}
