﻿using System.Collections.Generic;
using BrotliLib.IO;
using BrotliLib.IO.Reader;

namespace BrotliLib.Huffman{
    partial class HuffmanNode<T>{
        /// <summary>
        /// Dummy node used as a workaround for leaking Huffman tree generation.
        /// </summary>
        public sealed class Dummy : HuffmanNode<T>{
            public override int SymbolCount => 0;

            public override T LookupValue(IBitReader bits){
                return default;
            }

            protected override IEnumerable<KeyValuePair<T, BitStream>> ListValues(BitStream prefix){
                yield break;
            }
        }
    }
}
