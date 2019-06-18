using System.Collections.Generic;
using BrotliLib.IO;
using BrotliLib.IO.Reader;

namespace BrotliLib.Huffman{
    partial class HuffmanNode<T>{
        /// <summary>
        /// Leaf node that contains a <see cref="value"/> of type <code>T</code>.
        /// </summary>
        public sealed class Leaf : HuffmanNode<T>{
            public override int SymbolCount => 1;

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
        }
    }
}
