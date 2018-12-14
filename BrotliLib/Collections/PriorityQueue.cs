using System;
using System.Collections.Generic;

namespace BrotliLib.Collections{
    /// <summary>
    /// Represents a priority queue implemented as a binary heap. Supports <see cref="Insert(T)"/> and <see cref="ExtractMin"/> operations.
    /// </summary>
    public sealed class PriorityQueue<T> where T : IComparable<T>{
        public int Count => items.Count;
        public bool IsEmpty => items.Count == 0;

        private readonly List<T> items = new List<T>();

        /// <summary>
        /// Inserts a new item to the queue.
        /// </summary>
        /// <param name="newItem">Item to add.</param>
        public void Insert(T newItem){
            items.Add(newItem);

            int newIndex = items.Count - 1;

            while(newIndex > 0){
                int parentIndex = (newIndex - 1) / 2;
                T parentItem = items[parentIndex];

                if (newItem.CompareTo(parentItem) >= 0){
                    break;
                }
                else{
                    items[newIndex] = parentItem;
                    items[parentIndex] = newItem;
                    newIndex = parentIndex;
                }
            }
        }

        /// <summary>
        /// Returns the item which has the lowest priority, and removes it from the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when there are no items in the queue.</exception>
        public T ExtractMin(){
            if (IsEmpty){
                throw new InvalidOperationException("Queue is empty.");
            }

            T minItem = items[0];

            int endIndex = items.Count - 1;
            items[0] = items[endIndex];
            items.RemoveAt(endIndex);

            if (!IsEmpty){
                int parentIndex = 0;

                while(true){
                    int maxIndex = parentIndex;
                    T maxItem = items[maxIndex];

                    int leftIndex = (2 * parentIndex) + 1;
                    int rightIndex = (2 * parentIndex) + 2;

                    if (leftIndex < endIndex && items[leftIndex].CompareTo(maxItem) < 0){
                        maxItem = items[maxIndex = leftIndex];
                    }

                    if (rightIndex < endIndex && items[rightIndex].CompareTo(maxItem) < 0){
                        maxItem = items[maxIndex = rightIndex];
                    }

                    if (maxIndex == parentIndex){
                        break;
                    }
                    else{
                        items[maxIndex] = items[parentIndex];
                        items[parentIndex] = maxItem;
                        parentIndex = maxIndex;
                    }
                }
            }

            return minItem;
        }
    }
}
