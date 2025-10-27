using System;
using System.Collections.Generic;

namespace Pathfinding.DataStructures
{
    /// <summary>
    /// A min-heap based priority queue implementation optimized for pathfinding.
    /// Items with lower priority values are dequeued first.
    /// </summary>
    /// <typeparam name="T">Type of items stored in the queue</typeparam>
    public class PriorityQueue<T> where T : class
    {
        private List<(T item, int priority)> heap;
        private Dictionary<T, int> itemToIndex;  // Fast lookup for Contains and UpdatePriority

        /// <summary>
        /// Number of items currently in the queue
        /// </summary>
        public int Count => heap.Count;

        /// <summary>
        /// Whether the queue is empty
        /// </summary>
        public bool IsEmpty => heap.Count == 0;

        /// <summary>
        /// Creates a new priority queue with optional initial capacity
        /// </summary>
        public PriorityQueue(int initialCapacity = 16)
        {
            heap = new List<(T, int)>(initialCapacity);
            itemToIndex = new Dictionary<T, int>(initialCapacity);
        }

        /// <summary>
        /// Adds an item with a given priority to the queue
        /// </summary>
        /// <param name="item">Item to add</param>
        /// <param name="priority">Priority value (lower = higher priority)</param>
        public void Enqueue(T item, int priority)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Add to end of heap
            heap.Add((item, priority));
            int index = heap.Count - 1;
            itemToIndex[item] = index;

            // Bubble up to maintain heap property
            BubbleUp(index);
        }

        /// <summary>
        /// Removes and returns the item with the lowest priority
        /// </summary>
        /// <returns>The item with lowest priority</returns>
        public T Dequeue()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Priority queue is empty");

            T result = heap[0].item;
            itemToIndex.Remove(result);

            // Move last element to root
            int lastIndex = heap.Count - 1;
            if (lastIndex > 0)
            {
                heap[0] = heap[lastIndex];
                itemToIndex[heap[0].item] = 0;
            }
            heap.RemoveAt(lastIndex);

            // Bubble down to maintain heap property
            if (heap.Count > 0)
                BubbleDown(0);

            return result;
        }

        /// <summary>
        /// Returns the item with lowest priority without removing it
        /// </summary>
        public T Peek()
        {
            if (IsEmpty)
                throw new InvalidOperationException("Priority queue is empty");

            return heap[0].item;
        }

        /// <summary>
        /// Checks if the queue contains a specific item
        /// </summary>
        public bool Contains(T item)
        {
            return itemToIndex.ContainsKey(item);
        }

        /// <summary>
        /// Updates the priority of an existing item in the queue.
        /// If the new priority is lower, the item will bubble up.
        /// If higher, it will bubble down.
        /// </summary>
        /// <param name="item">Item to update</param>
        /// <param name="newPriority">New priority value</param>
        /// <returns>True if the item was found and updated</returns>
        public bool UpdatePriority(T item, int newPriority)
        {
            if (!itemToIndex.TryGetValue(item, out int index))
                return false;

            int oldPriority = heap[index].priority;
            heap[index] = (item, newPriority);

            // Determine which direction to bubble
            if (newPriority < oldPriority)
                BubbleUp(index);
            else if (newPriority > oldPriority)
                BubbleDown(index);

            return true;
        }

        /// <summary>
        /// Removes all items from the queue
        /// </summary>
        public void Clear()
        {
            heap.Clear();
            itemToIndex.Clear();
        }

        /// <summary>
        /// Bubbles an item up the heap until heap property is restored
        /// </summary>
        private void BubbleUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;

                // If parent has lower priority, heap property is satisfied
                if (heap[parentIndex].priority <= heap[index].priority)
                    break;

                // Swap with parent
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        /// <summary>
        /// Bubbles an item down the heap until heap property is restored
        /// </summary>
        private void BubbleDown(int index)
        {
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                // Find smallest among node and its children
                if (leftChild < heap.Count && heap[leftChild].priority < heap[smallest].priority)
                    smallest = leftChild;

                if (rightChild < heap.Count && heap[rightChild].priority < heap[smallest].priority)
                    smallest = rightChild;

                // If current node is smallest, heap property is satisfied
                if (smallest == index)
                    break;

                // Swap with smallest child
                Swap(index, smallest);
                index = smallest;
            }
        }

        /// <summary>
        /// Swaps two elements in the heap and updates the index map
        /// </summary>
        private void Swap(int i, int j)
        {
            var temp = heap[i];
            heap[i] = heap[j];
            heap[j] = temp;

            // Update index map
            itemToIndex[heap[i].item] = i;
            itemToIndex[heap[j].item] = j;
        }

        /// <summary>
        /// Returns all items in the queue (unordered)
        /// </summary>
        public IEnumerable<T> GetItems()
        {
            foreach (var (item, _) in heap)
                yield return item;
        }

        /// <summary>
        /// Validates the heap property (for debugging/testing)
        /// </summary>
        public bool ValidateHeap()
        {
            for (int i = 0; i < heap.Count; i++)
            {
                int leftChild = 2 * i + 1;
                int rightChild = 2 * i + 2;

                if (leftChild < heap.Count && heap[i].priority > heap[leftChild].priority)
                    return false;

                if (rightChild < heap.Count && heap[i].priority > heap[rightChild].priority)
                    return false;

                // Validate index map
                if (!itemToIndex.TryGetValue(heap[i].item, out int mappedIndex) || mappedIndex != i)
                    return false;
            }
            return true;
        }
    }
}
