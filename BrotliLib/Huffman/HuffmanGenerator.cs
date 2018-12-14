using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.IO;

namespace BrotliLib.Huffman{
    /// <summary>
    /// Provides utilities for generating Huffman trees.
    /// </summary>
    public static class HuffmanGenerator<T> where T : IComparable<T>{

        #region Bit paths

        /// <summary>
        /// Pairs a symbol with the length of its path in a tree.
        /// </summary>
        public sealed class Entry : IComparable<Entry>{
            public T Symbol { get; }
            public byte Bits { get; }

            public Entry(T symbol, byte bits){
                Symbol = symbol;
                Bits = bits;
            }

            public int CompareTo(Entry other){
                return Bits == other.Bits ? Symbol.CompareTo(other.Symbol) : Bits.CompareTo(other.Bits);
            }

            public override string ToString(){
                return "{ Symbol = " + Symbol + ", Bits = " + Bits + " }";
            }
        }

        public static Entry MakeEntry(T symbol, byte bits){
            return new Entry(symbol, bits);
        }

        /// <summary>
        /// Generates a canonical Huffman tree from a mapping of symbols to the lengths of their paths in the tree. Symbols of same length will be ordered by their <see cref="IComparable{T}"/> implementation.
        /// If the list contains a single symbol with any length, a <see cref="HuffmanNode{T}.Leaf"/> node will be returned, that always returns the symbol without advancing the enumerator.
        /// </summary>
        /// <param name="entries">Alphabet with each symbol mapped to its path length.</param>
        /// <exception cref="ArgumentException">Thrown when the list of entries is empty or it contains 2+ symbols and at least one of them has a path length of 0, and when the described paths generate unreachable symbols.</exception>
        public static HuffmanNode<T> FromBitCountsCanonical(IList<Entry> entries){
            if (entries.Count == 1){
                return new HuffmanNode<T>.Leaf(entries[0].Symbol);
            }
            else if (entries.Count == 0){
                throw new ArgumentException("Cannot generate a Huffman tree with no symbols.");
            }
            else if (entries.Any(entry => entry.Bits == 0)){
                throw new ArgumentException("Cannot generate a Huffman tree that contains multiple symbols, some of which have a length of 0.");
            }

            int maxBits = entries.Max(entry => entry.Bits);
            var bitCounts = entries.GroupBy(entry => entry.Bits).ToDictionary(group => group.Key, group => group.Count());

            int code = 0;
            int[] nextCode = new int[maxBits];

            for(byte bits = 1; bits <= maxBits; bits++){
                code = (code + (bitCounts.TryGetValue((byte)(bits - 1), out int count) ? count : 0)) << 1;
                nextCode[bits - 1] = code;
            }

            var symbolPaths = new Dictionary<BitStream, T>(bitCounts.Count);

            foreach(Entry entry in entries.OrderBy(entry => entry)){
                int pathCode = nextCode[entry.Bits - 1]++;
                symbolPaths[new BitStream(Convert.ToString(pathCode, 2).PadLeft(entry.Bits, '0'))] = entry.Symbol;
            }

            return FromSymbolPaths(symbolPaths);
        }

        /// <summary>
        /// Converts a <see cref="Dictionary{TKey, TValue}"/>, which maps <see cref="BitStream"/> paths to their assigned symbols, into a <see cref="HuffmanNode{T}"/>.
        /// </summary>
        /// <param name="paths">Mapping of bit paths to the symbols. All possible paths must terminate in a symbol.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="paths"/> dictionary is missing a possible path, or a path is unreachable.</exception>
        public static HuffmanNode<T> FromSymbolPaths(Dictionary<BitStream, T> paths){
            int longestBranch = paths.Select(path => path.Key.Length).DefaultIfEmpty(0).Max();
            int totalLeaves = 0;

            HuffmanNode<T> GenerateNodeBranch(BitStream prefix, bool nextBit){
                if (prefix.Length >= longestBranch){
                    throw new ArgumentException("Incomplete symbol paths, leaking branch at: " + prefix, nameof(paths));
                }

                BitStream branch = prefix.Clone();
                branch.Add(nextBit);
                
                if (paths.TryGetValue(branch, out T symbol)){
                    ++totalLeaves;
                    return new HuffmanNode<T>.Leaf(symbol);
                }
                else{
                    return GenerateNode(branch);
                }
            }

            HuffmanNode<T> GenerateNode(BitStream stream){
                return new HuffmanNode<T>.Path(GenerateNodeBranch(stream, false), GenerateNodeBranch(stream, true));
            }

            HuffmanNode<T> root = GenerateNode(new BitStream());

            if (totalLeaves != paths.Count){
                throw new ArgumentException("Impossible symbol paths, " + (paths.Count - totalLeaves) + " symbol(s) could not be reached.", nameof(paths));
            }

            return root;
        }
    }
}
