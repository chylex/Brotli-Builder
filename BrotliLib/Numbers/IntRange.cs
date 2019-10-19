using System;

namespace BrotliLib.Numbers{
    /// <summary>
    /// Represents a range of integers. When not initialized (using <code>default</code> or <code>new IntRange()</code>), the range will contain all 32-bit integers.
    /// </summary>
    public readonly struct IntRange{
        /// <summary>
        /// Range containing all signed 32-bit integers.
        /// </summary>
        public static IntRange Any => default;

        /// <summary>
        /// Returns a range containing all values which can be encoded with the specified amount of <paramref name="bits"/>, with an <paramref name="offset"/> applied to both ends of the range.
        /// </summary>
        public static IntRange FromOffsetBitPair(int offset, int bits){
            return new IntRange(offset, offset + (1 << bits) - 1);
        }

        /// <summary>
        /// Returns a range between <paramref name="minimum"/> and <see cref="int.MaxValue"/>.
        /// </summary>
        public static IntRange AtLeast(int minimum){
            return new IntRange(minimum, int.MaxValue);
        }
        
        /// <summary>
        /// Returns a range between <see cref="int.MinValue"/> and <paramref name="maximum"/>.
        /// </summary>
        public static IntRange AtMost(int maximum){
            return new IntRange(int.MinValue, maximum);
        }
        
        /// <summary>
        /// Returns a range only containing the specified <paramref name="value"/>.
        /// </summary>
        public static IntRange Only(int value){
            return new IntRange(value, value);
        }

        // Data

        private readonly bool initialized;
        private readonly int first;
        private readonly int last;
        
        /// <summary>
        /// Lower bound (inclusive).
        /// </summary>
        public int First => initialized ? first : int.MinValue;

        /// <summary>
        /// Upper bound (inclusive).
        /// </summary>
        public int Last => initialized ? last : int.MaxValue;

        /// <summary>
        /// Initializes the range with the provided lower and upper bound (both inclusive).
        /// </summary>
        public IntRange(int first, int last){
            if (last < first){
                throw new ArgumentOutOfRangeException(nameof(last), "last must be >= first");
            }

            this.initialized = true;
            this.first = first;
            this.last = last;
        }
        
        public bool Contains(int value){
            return !initialized || (value >= first && value <= last);
        }

        // Object

        public override bool Equals(object obj){
            return obj is IntRange range &&
                   initialized == range.initialized &&
                   first == range.first &&
                   last == range.last;
        }

        public override int GetHashCode(){
            return HashCode.Combine(initialized, first, last);
        }

        public override string ToString(){
            return initialized ? "[" + first + "; " + last + "]" : "[Any]";
        }
    }
}
