using System.Collections.Generic;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Collections.Huffman{
    partial class HuffmanNode<T>{
        /// <summary>
        /// Leaf node that contains a <see cref="value"/> of type <code>T</code>.
        /// </summary>
        public sealed class Leaf : HuffmanNode<T>{
            private readonly T value;
            
            public Leaf(T value){
                this.value = value;
            }

            public override T LookupValue(IBitReader bits){
                return value;
            }

            protected override IEnumerable<KeyValuePair<T, BitStream>> ListValues(BitStream prefix){
                yield return new KeyValuePair<T, BitStream>(value, prefix);
            }

            protected override IEnumerable<KeyValuePair<T, BitPath>> ListValues(BitPath prefix){
                yield return new KeyValuePair<T, BitPath>(value, prefix);
            }
        }
    }
}
