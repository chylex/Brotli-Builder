﻿using System;

namespace BrotliLib.Collections{
    /// <summary>
    /// A ring buffer implemented as a fixed-size queue.
    /// </summary>
    public sealed class RingBuffer<T>{
        /// <summary>
        /// Initializes a new ring buffer, with the values and size of the provided <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Initial values placed into the buffer. Element at [0] is at the back of the queue.</param>
        public static RingBuffer<T> From(params T[] values){
            return new RingBuffer<T>(values);
        }

        /// <summary>
        /// Amount of elements in the buffer.
        /// </summary>
        public int Length { get; }

        /// <summary>
        /// Element at index [<see cref="Length"/> - 1].
        /// </summary>
        public T Front => this[Length - 1];

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

                return values[(index + accessOffset) % Length];
            }
        }

        private readonly T[] values;
        private int accessOffset;

        /// <summary>
        /// Initializes a new ring buffer, with the values and size of the provided <paramref name="values"/>.
        /// </summary>
        /// <param name="values">Initial values placed into the buffer. Element at [0] is at the back of the queue.</param>
        public RingBuffer(T[] values){
            if (values.Length == 0){
                throw new ArgumentException("Ring buffer size cannot be zero.");
            }

            this.values = values;
            this.Length = values.Length;
        }

        /// <summary>
        /// Initializes a new ring buffer, with a shallow copy of the contents of the provided <paramref name="original"/>.
        /// </summary>
        public RingBuffer(RingBuffer<T> original){
            this.values = (T[])original.values.Clone();
            this.accessOffset = original.accessOffset;
            this.Length = original.Length;
        }

        /// <summary>
        /// Pushes a new value to the front of the queue, removing the value at the back.
        /// </summary>
        public void Push(T value){
            values[accessOffset++ % Length] = value;
        }
    }
}
