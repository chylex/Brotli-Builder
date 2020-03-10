using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Parameters.Heuristics{
    public static class HuffmanTreeHeuristics{
        public delegate HuffmanTree<T> Generate<T>(FrequencyList<T> frequencies) where T : IComparable<T>;
    }
}
