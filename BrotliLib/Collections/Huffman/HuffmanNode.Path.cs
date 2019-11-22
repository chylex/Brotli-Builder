using System;
using System.Collections.Generic;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Collections.Huffman{
    partial class HuffmanNode<T>{
        /// <summary>
        /// Path node with exactly two child nodes. When traversing through the tree, a 0 bit indicates going <see cref="left"/> while a 1 bit indicates going <see cref="right"/>.
        /// </summary>
        public sealed class Path : HuffmanNode<T>{
            public override int SymbolCount => symbolCount;

            private readonly HuffmanNode<T> left, right;
            private readonly int symbolCount;

            public Path(HuffmanNode<T> left, HuffmanNode<T> right){
                this.left = left;
                this.right = right;
                this.symbolCount = left.SymbolCount + right.SymbolCount;
            }

            public override T LookupValue(IBitReader bits){
                try{
                    return (bits.NextBit() ? right : left).LookupValue(bits);
                }catch(IndexOutOfRangeException){
                    return default;
                }
            }

            protected override IEnumerable<KeyValuePair<T, BitStream>> ListValues(BitStream prefix){
                BitStream NextPrefix(bool next){
                    BitStream cloned = prefix.Clone();
                    cloned.Add(next);
                    return cloned;
                }

                foreach(KeyValuePair<T, BitStream> kvp in left.ListValues(NextPrefix(false))){
                    yield return kvp;
                }
                
                foreach(KeyValuePair<T, BitStream> kvp in right.ListValues(NextPrefix(true))){
                    yield return kvp;
                }
            }
        }
    }
}
