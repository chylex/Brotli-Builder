using System;
using BrotliLib.Numbers;

namespace BrotliLib.Collections{
    /// <summary>
    /// A ring buffer implemented as a fixed-size queue. Uses bit mask to wrap around, meaning it requires a power-of-two amount of elements.
    /// </summary>
    public sealed class RingBufferFast<T>{
        /// <summary>
        /// Initializes a new ring buffer, with the values and size of the provided <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Initial values placed into the buffer. Element at [0] is at the back of the queue.</param>
        public static RingBufferFast<T> From(params T[] values){
            return new RingBufferFast<T>(values);
        }

        /// <summary>
        /// Amount of elements in the buffer.
        /// </summary>
        public int Length => mask + 1;

        /// <summary>
        /// Element at index [<see cref="Length"/> - 1].
        /// </summary>
        public T Front => this[mask];

        /// <summary>
        /// Element at index [0].
        /// </summary>
        public T Back => this[0];

        /// <summary>
        /// Element at the provided <paramref name="index"/>.
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">Thrown when the provided <paramref name="index"/> is negative.</exception>
        public T this[int index]{
            get{
                if (index < 0){
                    throw new IndexOutOfRangeException("Ring buffer index cannot be negative.");
                }

                return values[(index + accessOffset) & mask];
            }
        }

        private readonly T[] values;
        private readonly int mask;
        private int accessOffset;

        /// <summary>
        /// Initializes a new ring buffer, with the values and size of the provided <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Initial values placed into the buffer. Element at [0] is at the back of the queue.</param>
        public RingBufferFast(T[] values){
            int length = values.Length;

            if (length == 0){
                throw new ArgumentException("Ring buffer size cannot be zero.");
            }
            else if (length != 1 << Log2.Floor(length)){
                throw new ArgumentException("Fast ring buffer size must be a power of 2.");
            }

            this.values = values;
            this.mask = length - 1;
        }

        /// <summary>
        /// Initializes a new ring buffer, with a shallow copy of the contents of the provided <paramref name="original"/>.
        /// </summary>
        public RingBufferFast(RingBufferFast<T> original){
            this.values = (T[])original.values.Clone();
            this.mask = original.mask;
            this.accessOffset = original.accessOffset;
        }

        /// <summary>
        /// Pushes a new value to the front of the queue, removing the value at the back.
        /// </summary>
        public void Push(T value){
            values[accessOffset++ & mask] = value;
        }
    }
}
