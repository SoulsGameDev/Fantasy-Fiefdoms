using System;

namespace Inventory.Data
{
    /// <summary>
    /// Represents a stack of items in an inventory slot.
    /// This is a lightweight struct optimized for memory efficiency.
    /// Use this for most items (stackable and non-stackable).
    /// </summary>
    [Serializable]
    public struct ItemStack : IEquatable<ItemStack>
    {
        /// <summary>The type definition of the item (ScriptableObject reference)</summary>
        public ItemType Type;

        /// <summary>The quantity of items in this stack (1 for non-stackable items)</summary>
        public int Quantity;

        /// <summary>Current durability of the item (-1 if durability not applicable)</summary>
        public int Durability;

        /// <summary>Unique identifier for this stack instance (used for save/load tracking)</summary>
        public Guid InstanceID;

        /// <summary>
        /// Creates a new item stack with the specified type and quantity.
        /// </summary>
        /// <param name="type">The item type definition</param>
        /// <param name="quantity">The quantity of items (default: 1)</param>
        /// <param name="durability">The durability value (default: -1 for N/A)</param>
        public ItemStack(ItemType type, int quantity = 1, int durability = -1)
        {
            Type = type;
            Quantity = quantity;
            Durability = durability;
            InstanceID = Guid.NewGuid();
        }

        /// <summary>
        /// Gets whether this stack is empty (null type or zero quantity).
        /// </summary>
        public bool IsEmpty => Type == null || Quantity <= 0;

        /// <summary>
        /// Gets whether this stack is valid (has a type and positive quantity).
        /// </summary>
        public bool IsValid => Type != null && Quantity > 0;

        /// <summary>
        /// Gets whether this stack can accept more items.
        /// </summary>
        public bool CanStack => Type != null && Quantity < Type.MaxStack;

        /// <summary>
        /// Gets the remaining space in this stack.
        /// </summary>
        public int RemainingSpace => Type != null ? Type.MaxStack - Quantity : 0;

        /// <summary>
        /// Gets whether this item has durability tracking enabled.
        /// </summary>
        public bool HasDurability => Durability >= 0;

        /// <summary>
        /// Gets the total weight of this stack.
        /// </summary>
        public float TotalWeight => Type != null ? Type.Weight * Quantity : 0f;

        /// <summary>
        /// Gets the total value of this stack.
        /// </summary>
        public int TotalValue => Type != null ? Type.Value * Quantity : 0;

        /// <summary>
        /// Attempts to add items to this stack.
        /// </summary>
        /// <param name="amount">The amount to add</param>
        /// <param name="remainder">The amount that couldn't be added</param>
        /// <returns>True if at least some items were added</returns>
        public bool TryAddToStack(int amount, out int remainder)
        {
            if (Type == null || amount <= 0)
            {
                remainder = amount;
                return false;
            }

            int maxCanAdd = Type.MaxStack - Quantity;
            int actualAdd = Math.Min(amount, maxCanAdd);

            Quantity += actualAdd;
            remainder = amount - actualAdd;

            return actualAdd > 0;
        }

        /// <summary>
        /// Attempts to remove items from this stack.
        /// </summary>
        /// <param name="amount">The amount to remove</param>
        /// <param name="removed">The actual amount removed</param>
        /// <returns>True if the requested amount was fully removed</returns>
        public bool TryRemoveFromStack(int amount, out int removed)
        {
            if (amount <= 0 || Quantity <= 0)
            {
                removed = 0;
                return false;
            }

            removed = Math.Min(amount, Quantity);
            Quantity -= removed;

            return removed == amount;
        }

        /// <summary>
        /// Splits this stack into two stacks.
        /// </summary>
        /// <param name="splitAmount">The amount to split off</param>
        /// <param name="newStack">The new stack containing the split items</param>
        /// <returns>True if the split was successful</returns>
        public bool TrySplit(int splitAmount, out ItemStack newStack)
        {
            if (splitAmount <= 0 || splitAmount >= Quantity)
            {
                newStack = default;
                return false;
            }

            newStack = new ItemStack(Type, splitAmount, Durability);
            Quantity -= splitAmount;

            return true;
        }

        /// <summary>
        /// Checks if this stack can merge with another stack.
        /// </summary>
        /// <param name="other">The other stack to check</param>
        /// <returns>True if the stacks can be merged</returns>
        public bool CanMergeWith(ItemStack other)
        {
            if (Type == null || other.Type == null)
                return false;

            // Must be same item type
            if (Type != other.Type)
                return false;

            // Must have space to merge
            if (Quantity >= Type.MaxStack)
                return false;

            // If durability matters, they should match (or both be N/A)
            if (HasDurability && other.HasDurability && Durability != other.Durability)
                return false;

            return true;
        }

        /// <summary>
        /// Attempts to merge another stack into this one.
        /// </summary>
        /// <param name="other">The stack to merge</param>
        /// <param name="remainder">The amount that couldn't be merged</param>
        /// <returns>True if at least some items were merged</returns>
        public bool TryMerge(ref ItemStack other, out int remainder)
        {
            if (!CanMergeWith(other))
            {
                remainder = other.Quantity;
                return false;
            }

            return TryAddToStack(other.Quantity, out remainder);
        }

        /// <summary>
        /// Reduces durability by the specified amount.
        /// </summary>
        /// <param name="amount">The amount to reduce (default: 1)</param>
        /// <returns>True if the item broke (durability reached 0)</returns>
        public bool ReduceDurability(int amount = 1)
        {
            if (!HasDurability)
                return false;

            Durability = Math.Max(0, Durability - amount);
            return Durability == 0;
        }

        /// <summary>
        /// Repairs the item's durability.
        /// </summary>
        /// <param name="amount">The amount to repair (default: restore to max)</param>
        public void Repair(int amount = -1)
        {
            if (!HasDurability || Type == null)
                return;

            if (amount < 0)
            {
                // Restore to max
                Durability = Type.MaxDurability;
            }
            else
            {
                Durability = Math.Min(Type.MaxDurability, Durability + amount);
            }
        }

        /// <summary>
        /// Creates an empty stack.
        /// </summary>
        public static ItemStack Empty => new ItemStack(null, 0);

        #region Equality

        public bool Equals(ItemStack other)
        {
            return Type == other.Type &&
                   Quantity == other.Quantity &&
                   Durability == other.Durability &&
                   InstanceID.Equals(other.InstanceID);
        }

        public override bool Equals(object obj)
        {
            return obj is ItemStack other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (Type != null ? Type.GetHashCode() : 0);
                hash = (hash * 397) ^ Quantity;
                hash = (hash * 397) ^ Durability;
                hash = (hash * 397) ^ InstanceID.GetHashCode();
                return hash;
            }
        }

        public static bool operator ==(ItemStack left, ItemStack right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemStack left, ItemStack right)
        {
            return !left.Equals(right);
        }

        #endregion

        public override string ToString()
        {
            if (IsEmpty)
                return "Empty Stack";

            string durabilityInfo = HasDurability ? $" [{Durability}/{Type.MaxDurability}]" : "";
            return $"{Type.Name} x{Quantity}{durabilityInfo}";
        }
    }
}
