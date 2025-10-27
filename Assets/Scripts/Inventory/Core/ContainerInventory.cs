using System;
using Inventory.Data;
using UnityEngine;

namespace Inventory.Core
{
    /// <summary>
    /// Specialized inventory for world containers like chests, crates, and corpses.
    /// Supports loot generation, persistence, and auto-despawn.
    /// </summary>
    [Serializable]
    public class ContainerInventory : Inventory
    {
        [Header("Container Settings")]
        [SerializeField] private ContainerType containerType = ContainerType.Chest;
        [SerializeField] private bool isLocked = false;
        [SerializeField] private string requiredKeyID = "";
        [SerializeField] private int lockLevel = 0;

        [Header("Persistence")]
        [SerializeField] private bool persistent = true;
        [SerializeField] private string worldPosition = "0,0,0";

        [Header("Despawn")]
        [SerializeField] private bool autoDespawn = false;
        [SerializeField] private float despawnTimeSeconds = 300f; // 5 minutes
        [SerializeField] private bool despawnWhenEmpty = false;

        private float spawnTime;
        private bool hasBeenOpened = false;

        #region Events

        /// <summary>Fired when container is opened</summary>
        public event Action<string> OnContainerOpened;

        /// <summary>Fired when container is closed</summary>
        public event Action<string> OnContainerClosed;

        /// <summary>Fired when container is locked/unlocked</summary>
        public event Action<string, bool> OnLockStateChanged;

        /// <summary>Fired when container should despawn</summary>
        public event Action<string> OnShouldDespawn;

        #endregion

        #region Properties

        /// <summary>Type of container</summary>
        public ContainerType ContainerType => containerType;

        /// <summary>Whether the container is locked</summary>
        public bool IsLocked => isLocked;

        /// <summary>Key item ID required to unlock</summary>
        public string RequiredKeyID => requiredKeyID;

        /// <summary>Lock difficulty level (for lockpicking)</summary>
        public int LockLevel => lockLevel;

        /// <summary>Whether this container persists across game sessions</summary>
        public bool IsPersistent => persistent;

        /// <summary>Whether the container has been opened at least once</summary>
        public bool HasBeenOpened => hasBeenOpened;

        /// <summary>Time remaining before despawn (if applicable)</summary>
        public float TimeUntilDespawn
        {
            get
            {
                if (!autoDespawn) return -1f;
                float elapsed = Time.time - spawnTime;
                return Mathf.Max(0, despawnTimeSeconds - elapsed);
            }
        }

        #endregion

        #region Construction

        /// <summary>
        /// Creates a new container inventory.
        /// </summary>
        public ContainerInventory(
            string containerID,
            ContainerType type,
            int maxSlots,
            bool locked = false,
            bool persistent = true)
            : base(containerID, maxSlots, maxWeight: -1f)
        {
            this.containerType = type;
            this.isLocked = locked;
            this.persistent = persistent;
            this.spawnTime = Time.time;
        }

        #endregion

        #region Container Operations

        /// <summary>
        /// Attempts to open the container.
        /// </summary>
        /// <param name="playerInventory">Player inventory (to check for key)</param>
        /// <param name="reason">Reason if opening fails</param>
        /// <returns>True if container was opened</returns>
        public bool TryOpen(Inventory playerInventory, out string reason)
        {
            reason = string.Empty;

            if (!isLocked)
            {
                hasBeenOpened = true;
                OnContainerOpened?.Invoke(InventoryID);
                return true;
            }

            // Check for key
            if (!string.IsNullOrEmpty(requiredKeyID) && playerInventory != null)
            {
                if (playerInventory.ContainsItem(requiredKeyID, 1))
                {
                    // Unlock with key (don't consume key)
                    Unlock();
                    hasBeenOpened = true;
                    OnContainerOpened?.Invoke(InventoryID);
                    return true;
                }
                else
                {
                    reason = $"Requires key: {requiredKeyID}";
                    return false;
                }
            }

            reason = $"Container is locked (Level {lockLevel})";
            return false;
        }

        /// <summary>
        /// Closes the container.
        /// </summary>
        public void Close()
        {
            OnContainerClosed?.Invoke(InventoryID);

            // Check despawn conditions
            if (despawnWhenEmpty && GetAllItems().Count == 0)
            {
                OnShouldDespawn?.Invoke(InventoryID);
            }
        }

        /// <summary>
        /// Locks the container.
        /// </summary>
        public void Lock(string keyID = "", int level = 1)
        {
            isLocked = true;
            requiredKeyID = keyID;
            lockLevel = level;
            OnLockStateChanged?.Invoke(InventoryID, true);
        }

        /// <summary>
        /// Unlocks the container.
        /// </summary>
        public void Unlock()
        {
            isLocked = false;
            OnLockStateChanged?.Invoke(InventoryID, false);
        }

        /// <summary>
        /// Attempts to pick the lock (requires lockpicking skill).
        /// </summary>
        public bool TryPickLock(int playerLockpickingSkill, out string reason)
        {
            reason = string.Empty;

            if (!isLocked)
            {
                reason = "Container is not locked";
                return false;
            }

            // Simple skill check
            if (playerLockpickingSkill >= lockLevel)
            {
                Unlock();
                hasBeenOpened = true;
                Debug.Log($"Successfully picked lock on {InventoryID}");
                return true;
            }
            else
            {
                reason = $"Lockpicking skill too low. Required: {lockLevel}, Current: {playerLockpickingSkill}";
                return false;
            }
        }

        #endregion

        #region Loot Generation

        /// <summary>
        /// Generates random loot based on container type.
        /// </summary>
        public void GenerateLoot(int lootLevel = 1)
        {
            // This is a simple example - you'd want a more sophisticated loot table system
            Clear();

            int itemCount = GetLootItemCount(lootLevel);

            for (int i = 0; i < itemCount; i++)
            {
                // Get random item from InventoryManager
                if (InventoryManager.Instance != null && InventoryManager.Instance.ItemTypes.Count > 0)
                {
                    ItemType randomItem = InventoryManager.Instance.ItemTypes[
                        UnityEngine.Random.Range(0, InventoryManager.Instance.ItemTypes.Count)
                    ];

                    if (randomItem != null)
                    {
                        int quantity = randomItem.IsStackable ? UnityEngine.Random.Range(1, 5) : 1;
                        ItemStack stack = randomItem.CreateStack(quantity);
                        TryAddItem(stack, out _);
                    }
                }
            }

            Debug.Log($"Generated {itemCount} loot items for {InventoryID}");
        }

        private int GetLootItemCount(int lootLevel)
        {
            return containerType switch
            {
                ContainerType.Chest => UnityEngine.Random.Range(3, 8) + lootLevel,
                ContainerType.Crate => UnityEngine.Random.Range(1, 4) + lootLevel,
                ContainerType.Corpse => UnityEngine.Random.Range(1, 3) + lootLevel,
                ContainerType.Barrel => UnityEngine.Random.Range(1, 3),
                _ => UnityEngine.Random.Range(1, 3)
            };
        }

        #endregion

        #region Despawn

        /// <summary>
        /// Updates the container (call in Update loop if using auto-despawn).
        /// </summary>
        public void Update()
        {
            if (!autoDespawn) return;

            if (TimeUntilDespawn <= 0)
            {
                OnShouldDespawn?.Invoke(InventoryID);
            }
        }

        /// <summary>
        /// Checks if the container should despawn.
        /// </summary>
        public bool ShouldDespawn()
        {
            if (autoDespawn && TimeUntilDespawn <= 0)
                return true;

            if (despawnWhenEmpty && GetAllItems().Count == 0)
                return true;

            return false;
        }

        /// <summary>
        /// Sets despawn settings.
        /// </summary>
        public void SetDespawnSettings(bool enabled, float timeSeconds = 300f, bool whenEmpty = false)
        {
            autoDespawn = enabled;
            despawnTimeSeconds = timeSeconds;
            despawnWhenEmpty = whenEmpty;
        }

        #endregion

        #region Persistence

        /// <summary>
        /// Sets the world position for persistence.
        /// </summary>
        public void SetWorldPosition(Vector3 position)
        {
            worldPosition = $"{position.x},{position.y},{position.z}";
        }

        /// <summary>
        /// Gets the world position.
        /// </summary>
        public Vector3 GetWorldPosition()
        {
            string[] parts = worldPosition.Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                return new Vector3(x, y, z);
            }
            return Vector3.zero;
        }

        /// <summary>
        /// Gets save data for persistence.
        /// </summary>
        public ContainerSaveData GetSaveData()
        {
            return new ContainerSaveData
            {
                ContainerID = InventoryID,
                ContainerType = containerType,
                IsLocked = isLocked,
                RequiredKeyID = requiredKeyID,
                LockLevel = lockLevel,
                WorldPosition = worldPosition,
                HasBeenOpened = hasBeenOpened,
                Items = GetAllItems()
            };
        }

        #endregion
    }

    /// <summary>
    /// Types of containers.
    /// </summary>
    public enum ContainerType
    {
        Chest,
        Crate,
        Barrel,
        Corpse,
        Bag,
        Locker,
        Safe
    }

    /// <summary>
    /// Save data for container persistence.
    /// </summary>
    [Serializable]
    public class ContainerSaveData
    {
        public string ContainerID;
        public ContainerType ContainerType;
        public bool IsLocked;
        public string RequiredKeyID;
        public int LockLevel;
        public string WorldPosition;
        public bool HasBeenOpened;
        public System.Collections.Generic.List<ItemStack> Items;
    }
}
