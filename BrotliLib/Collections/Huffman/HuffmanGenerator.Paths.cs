using System;
using System.Collections.Generic;
using System.Linq;

namespace BrotliLib.Collections.Huffman{
    /// <summary>
    /// Provides utilities for generating Huffman trees.
    /// </summary>
    public static partial class HuffmanGenerator<T> where T : IComparable<T>{
        /// <summary>
        /// Pairs a symbol with the length of its path in a tree.
        /// </summary>
        public readonly struct Entry : IComparable<Entry>{
            public T Symbol { get; }
            public byte Bits { get; }

            internal double Kraft => Math.Pow(2, -Bits);

            public Entry(T symbol, byte bits){
                this.Symbol = symbol;
                this.Bits = bits;
            }

            internal Entry Resize(byte bits){
                return new Entry(Symbol, bits);
            }

            public int CompareTo(Entry other){
                return Bits == other.Bits ? Symbol.CompareTo(other.Symbol) : Bits.CompareTo(other.Bits);
            }

            public override string ToString(){
                return "Bits = " + Bits + ", Symbol = { " + Symbol + " }";
            }
        }

        public static Entry MakeEntry(T symbol, byte bits){
            return new Entry(symbol, bits);
        }

        /// <summary>
        /// Generates a canonical Huffman tree from a mapping of symbols to the lengths of their paths in the tree. Symbols of same length will be ordered by their <see cref="IComparable{T}"/> implementation.
        /// If the array contains a single symbol with any length, a <see cref="HuffmanNode{T}.Leaf"/> node will be returned, that always returns the symbol without advancing the enumerator.
        /// </summary>
        /// <param name="entries">Alphabet with each symbol mapped to its path length. The array will be sorted.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="entries"/> array is empty, or it contains 2+ symbols and at least one of them has a path length of 0, or one of its paths exceeds the limit set by <see cref="BitPath.MaxLength"/>, or when the described paths generate unreachable symbols.</exception>
        public static HuffmanNode<T> FromBitCountsCanonical(Entry[] entries){
            if (entries.Length == 1){
                return new HuffmanNode<T>.Leaf(entries[0].Symbol);
            }
            else if (entries.Length == 0){
                throw new ArgumentException("Cannot generate a Huffman tree with no symbols.");
            }
            else if (entries.Any(entry => entry.Bits == 0)){
                throw new ArgumentException("Cannot generate a Huffman tree that contains multiple symbols, some of which have a length of 0.");
            }
            else if (entries.Any(entry => entry.Bits > BitPath.MaxLength)){
                throw new ArgumentException("Cannot generate a Huffman tree that has paths longer than " + BitPath.MaxLength + ".");
            }

            int[] bitCounts = new int[BitPath.MaxLength + 1];
            int[] nextCode = new int[BitPath.MaxLength + 1];

            foreach(Entry entry in entries){
                bitCounts[entry.Bits]++;
            }

            for(int index = 0, code = 0; index < nextCode.Length; index++){
                code = (code + bitCounts[index]) << 1;
                nextCode[index] = code;
            }

            Array.Sort(entries);

            var symbolPaths = new Dictionary<BitPath, T>(entries.Length);

            foreach(Entry entry in entries){
                int pathCode = nextCode[entry.Bits - 1]++;
                symbolPaths[new BitPath(pathCode, entry.Bits)] = entry.Symbol;
            }

            return FromSymbolPaths(symbolPaths);
        }

        /// <summary>
        /// Converts a <see cref="Dictionary{TKey, TValue}"/>, which maps <see cref="BitPath"/> instances to their assigned symbols, into a <see cref="HuffmanNode{T}"/>.
        /// </summary>
        /// <param name="paths">Mapping of bit paths to the symbols. All possible paths must terminate in a symbol.</param>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="paths"/> dictionary is missing a possible path, or a path is unreachable.</exception>
        public static HuffmanNode<T> FromSymbolPaths(Dictionary<BitPath, T> paths){
            int longestBranch = paths.Select(path => (int)path.Key.Length).DefaultIfEmpty(0).Max();
            int totalLeaves = 0;

            HuffmanNode<T> GenerateNodeBranch(in BitPath prefix, bool nextBit){
                BitPath branch = prefix.Add(nextBit);

                if (paths.TryGetValue(branch, out T symbol)){
                    ++totalLeaves;
                    return new HuffmanNode<T>.Leaf(symbol);
                }
                else if (branch.Length >= longestBranch){
                    return new HuffmanNode<T>.Dummy(); // TODO hack to "support" Huffman trees with missing branches
                }
                else{
                    return GenerateNode(branch);
                }
            }

            HuffmanNode<T> GenerateNode(in BitPath stream){
                return new HuffmanNode<T>.Path(GenerateNodeBranch(stream, false), GenerateNodeBranch(stream, true));
            }

            HuffmanNode<T> root = GenerateNode(new BitPath());

            if (totalLeaves != paths.Count){
                throw new ArgumentException("Impossible symbol paths, " + (paths.Count - totalLeaves) + " symbol(s) could not be reached.", nameof(paths));
            }

            return root;
        }
    }
}
