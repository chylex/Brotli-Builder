using System;

namespace BrotliLib.Numbers{
    public class Range{
        /// <summary>
        /// Returns a range containing all values which can be encoded with the specified amount of <paramref name="bits"/>, with an <paramref name="offset"/> applied to both ends of the range.
        /// </summary>
        public static Range FromOffsetBitPair(int offset, int bits){
            return new Range(offset, offset + (1 << bits) - 1);
        }

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

            this.First = first;
            this.Last = last;
        }

        public bool Contains(int value){
            return value >= First && value <= Last;
        }

        // Object

        public override bool Equals(object obj){
            return obj is Range range &&
                   First == range.First &&
                   Last == range.Last;
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -233066942;
                hashCode = hashCode * -1521134295 + First.GetHashCode();
                hashCode = hashCode * -1521134295 + Last.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString(){
            return "First = " + First + ", Last = " + Last;
        }
    }
}
