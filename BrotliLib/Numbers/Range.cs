using System;

namespace BrotliLib.Numbers{
    public readonly struct Range{
        /// <summary>
        /// Range containing all signed 32-bit integers.
        /// </summary>
        public static Range Any => default;

        /// <summary>
        /// Returns a range containing all values which can be encoded with the specified amount of <paramref name="bits"/>, with an <paramref name="offset"/> applied to both ends of the range.
        /// </summary>
        public static Range FromOffsetBitPair(int offset, int bits){
            return new Range(offset, offset + (1 << bits) - 1);
        }

        /// <summary>
        /// Returns a range between <paramref name="minimum"/> and <see cref="int.MaxValue"/>.
        /// </summary>
        public static Range AtLeast(int minimum){
            return new Range(minimum, int.MaxValue);
        }
        
        /// <summary>
        /// Returns a range between <see cref="int.MinValue"/> and <paramref name="maximum"/>.
        /// </summary>
        public static Range AtMost(int maximum){
            return new Range(int.MinValue, maximum);
        }
        
        /// <summary>
        /// Returns a range only containing the specified <paramref name="value"/>.
        /// </summary>
        public static Range Only(int value){
            return new Range(value, value);
        }

        // Data

        /// <summary>
        /// When not initialized (using <code>default</code> or <code>new Range()</code>), the range will contain all 32-bit integers.
        /// </summary>
        private bool Initialized { get; }

        /// <summary>
        /// Lower bound (inclusive).
        /// </summary>
        public int First { get; }

        /// <summary>
        /// Upper bound (inclusive).
        /// </summary>
        public int Last { get; }

        /// <summary>
        /// Initializes the range with the provided lower and upper bound (both inclusive).
        /// </summary>
        public Range(int first, int last){
            if (last < first){
                throw new ArgumentOutOfRangeException(nameof(last), "last must be >= first");
            }

            this.Initialized = true;
            this.First = first;
            this.Last = last;
        }
        
        public bool Contains(int value){
            return !Initialized || (value >= First && value <= Last);
        }

        // Object

        public override bool Equals(object obj){
            return obj is Range range &&
                   Initialized == range.Initialized &&
                   First == range.First &&
                   Last == range.Last;
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -1176758481;
                hashCode = hashCode * -1521134295 + Initialized.GetHashCode();
                hashCode = hashCode * -1521134295 + First.GetHashCode();
                hashCode = hashCode * -1521134295 + Last.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString(){
            return Initialized ? "First = " + First + ", Last = " + Last : "Any";
        }
    }
}
