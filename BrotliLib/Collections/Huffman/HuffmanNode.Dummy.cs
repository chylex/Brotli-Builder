using System.Collections.Generic;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Collections.Huffman{
    partial class HuffmanNode<T>{
        /// <summary>
        /// Dummy node used as a workaround for leaking Huffman tree generation.
        /// </summary>
        public sealed class Dummy : HuffmanNode<T>{
            public override T LookupValue(IBitReader bits){
                return default!;
            }

            protected override IEnumerable<KeyValuePair<T, BitStream>> ListValues(BitStream prefix){
                yield break;
            }

            protected override IEnumerable<KeyValuePair<T, BitPath>> ListValues(BitPath prefix){
                yield break;
            }
        }
    }
}
