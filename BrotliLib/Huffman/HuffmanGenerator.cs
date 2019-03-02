using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Collections;
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

            internal double Kraft => Math.Pow(2, -Bits);

            public Entry(T symbol, byte bits){
                Symbol = symbol;
                Bits = bits;
            }

            internal Entry Resize(byte bits){
                return new Entry(Symbol, bits);
            }

            public int CompareTo(Entry other){
                return Bits == other.Bits ? Symbol.CompareTo(other.Symbol) : Bits.CompareTo(other.Bits);
            }

            public override string ToString(){
                return "Symbol = " + Symbol + ", Bits = " + Bits;
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

        #endregion
        
        #region Frequencies

        /// <summary>
        /// Pairs a <see cref="HuffmanNode{T}"/> with its frequency, used while constructing a tree.
        /// </summary>
        public class Freq : IComparable<Freq>{
            public HuffmanNode<T> Node { get; }
            public int Frequency { get; }

            public Freq(HuffmanNode<T> node, int frequency){
                Node = node;
                Frequency = frequency;
            }
            
            public int CompareTo(Freq other){
                return Frequency.CompareTo(other.Frequency);
            }

            public void Deconstruct(out HuffmanNode<T> node, out int frequency){
                node = Node;
                frequency = Frequency;
            }
        }

        /// <summary>
        /// Pairs a symbol with its frequency, used while constructing a tree.
        /// </summary>
        public sealed class SymbolFreq : Freq{
            public T Symbol { get; }

            public SymbolFreq(T symbol, int frequency) : base(new HuffmanNode<T>.Leaf(symbol), frequency){
                this.Symbol = symbol;
            }
        }

        public static SymbolFreq MakeFreq<U>(IGrouping<T, U> grouping){
            return new SymbolFreq(grouping.Key, grouping.Count());
        }

        /// <summary>
        /// Generates a Huffman tree from a list of symbols with their frequencies. There must be at least 1 symbol in the <paramref name="symbols"/> list.
        /// If the list contains a single node with any frequency, that node will be returned.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="symbols"/> list is empty.</exception>
        public static HuffmanNode<T> FromFrequencies(IList<SymbolFreq> symbols){
            if (symbols.Count == 1){
                return symbols[0].Node;
            }
            else if (symbols.Count == 0){
                throw new ArgumentException("Cannot generate a Huffman tree with no symbols.", nameof(symbols));
            }

            var nodes = new PriorityQueue<Freq>();

            foreach(SymbolFreq entry in symbols){
                nodes.Insert(entry);
            }
            
            while(nodes.Count > 1){
                var (node1, freq1) = nodes.ExtractMin();
                var (node2, freq2) = nodes.ExtractMin();

                var newPath = freq1 <= freq2 ? new HuffmanNode<T>.Path(node1, node2) : new HuffmanNode<T>.Path(node2, node1);
                nodes.Insert(new Freq(newPath, freq1 + freq2));
            }

            return nodes.ExtractMin().Node;
        }

        /// <summary>
        /// Generates a canonical Huffman tree from a list of symbols with their frequencies. There must be at least 1 symbol in the <paramref name="symbols"/> list.
        /// If the list contains a single node with any frequency, that node will be returned.
        /// <para/>
        /// The additional <paramref name="maxDepth"/> parameter will limit the maximum length of paths in the tree.
        /// Depth limiting is implemented using a heuristic described at https://cbloomrants.blogspot.com/2010/07/07-03-10-length-limitted-huffman-codes.html.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="symbols"/> list is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxDepth"/> is smaller than 1 or larger than 31.</exception>
        public static HuffmanNode<T> FromFrequenciesCanonical(IList<SymbolFreq> symbols, byte maxDepth){
            if (maxDepth < 1 || maxDepth > 31){
                throw new ArgumentOutOfRangeException(nameof(maxDepth), "Depth-limited trees must have a specified maximum depth between 1 and 31.");
            }

            if (symbols.Count > (1 << maxDepth)){
                throw new ArgumentException("Maximum depth " + maxDepth + " can only encode up to " + (1 << maxDepth) + " symbol(s), amount of provided symbols is " + symbols.Count + ".", nameof(maxDepth));
            }
            
            var lengthMap = FromFrequencies(symbols).GenerateValueMap();

            if (lengthMap.All(entry => entry.Value.Length <= maxDepth)){
                return FromBitCountsCanonical(lengthMap.Select(kvp => new Entry(kvp.Key, (byte)kvp.Value.Length)).ToArray());
            }

            Entry MakeLimitedDepthEntry(T symbol){
                return new Entry(symbol, Math.Min((byte)lengthMap[symbol].Length, maxDepth));
            }
            
            var symbolOrder = symbols.Select(entry => entry.Symbol).ToArray();
            
            var symbolEntries = symbols.OrderBy(entry => entry.Frequency)
                                       .ThenBy(entry => Array.IndexOf(symbolOrder, entry.Symbol))
                                       .Select(entry => MakeLimitedDepthEntry(entry.Symbol))
                                       .ToArray();
            
            double bitSpace = symbolEntries.Sum(entry => entry.Kraft);

            for(int index = 0; index < symbolEntries.Length && bitSpace > 1; index++){
                var entry = symbolEntries[index];

                if (entry.Bits < maxDepth){
                    entry = entry.Resize((byte)(entry.Bits + 1));

                    symbolEntries[index] = entry;
                    bitSpace -= entry.Kraft;
                }
            }

            for(int index = symbolEntries.Length - 1; index > 0 && bitSpace < 1; index--){
                var entry = symbolEntries[index];

                if (bitSpace + entry.Kraft <= 1){
                    entry = entry.Resize((byte)(entry.Bits - 1));

                    symbolEntries[index] = entry;
                    bitSpace += entry.Kraft;
                }
            }

            return FromBitCountsCanonical(symbolEntries);
        }

        #endregion
    }
}
