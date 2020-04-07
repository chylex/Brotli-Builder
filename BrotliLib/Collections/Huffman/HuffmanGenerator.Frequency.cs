using System;
using System.Linq;

namespace BrotliLib.Collections.Huffman{
    /// <summary>
    /// Provides utilities for generating Huffman trees.
    /// </summary>
    public static partial class HuffmanGenerator<T> where T : IComparable<T>{
        /// <summary>
        /// Pairs a <see cref="HuffmanNode{T}"/> with its frequency, used while constructing a tree.
        /// </summary>
        private readonly struct Freq : IComparable<Freq>{
            public HuffmanNode<T> Node { get; }
            public int Frequency { get; }

            public Freq(HuffmanNode<T> node, int frequency){
                this.Node = node;
                this.Frequency = frequency;
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
        public readonly struct SymbolFreq : IComparable<SymbolFreq>{
            public T Symbol { get; }
            public int Frequency { get; }

            public SymbolFreq(T symbol, int frequency){
                this.Symbol = symbol;
                this.Frequency = frequency;
            }
            
            public int CompareTo(SymbolFreq other){
                return Frequency.CompareTo(other.Frequency);
            }
        }

        /// <summary>
        /// Generates a Huffman tree from an array of symbols with their frequencies. There must be at least 1 symbol in the <paramref name="symbols"/> array.
        /// If the array contains a single node with any frequency, that node will be returned.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="symbols"/> array is empty.</exception>
        public static HuffmanNode<T> FromFrequencies(SymbolFreq[] symbols){
            if (symbols.Length == 1){
                return new HuffmanNode<T>.Leaf(symbols[0].Symbol);
            }
            else if (symbols.Length == 0){
                throw new ArgumentException("Cannot generate a Huffman tree with no symbols.", nameof(symbols));
            }

            var nodes = new PriorityQueue<Freq>();

            foreach(SymbolFreq entry in symbols){
                nodes.Insert(new Freq(new HuffmanNode<T>.Leaf(entry.Symbol), entry.Frequency));
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
        /// Generates a canonical Huffman tree from an array of symbols with their frequencies.
        /// There must be at least 1 symbol in the <paramref name="symbols"/> array. The array will be sorted.
        /// If the array contains a single node with any frequency, that node will be returned.
        /// <para/>
        /// The additional <paramref name="maxDepth"/> parameter will limit the maximum length of paths in the tree.
        /// Depth limiting is implemented using a heuristic described at https://cbloomrants.blogspot.com/2010/07/07-03-10-length-limitted-huffman-codes.html.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="symbols"/> array is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxDepth"/> is smaller than 1 or larger than <see cref="BitPath.MaxLength"/>.</exception>
        public static HuffmanNode<T> FromFrequenciesCanonical(SymbolFreq[] symbols, byte maxDepth){
            if (maxDepth < 1 || maxDepth > BitPath.MaxLength){
                throw new ArgumentOutOfRangeException(nameof(maxDepth), "Depth-limited trees must have a specified maximum depth between 1 and " + BitPath.MaxLength + ".");
            }

            if (symbols.Length > (1 << maxDepth)){
                throw new ArgumentException("Maximum depth " + maxDepth + " can only encode up to " + (1 << maxDepth) + " symbol(s), amount of provided symbols is " + symbols.Length + ".", nameof(maxDepth));
            }
            
            var lengthMap = FromFrequencies(symbols).GenerateValueMap();

            if (lengthMap.All(entry => entry.Value.Length <= maxDepth)){
                return FromBitCountsCanonical(lengthMap.Select(kvp => new Entry(kvp.Key, (byte)kvp.Value.Length)).ToArray());
            }

            Entry MakeLimitedDepthEntry(T symbol){
                return new Entry(symbol, Math.Min((byte)lengthMap[symbol].Length, maxDepth));
            }
            
            var symbolOrder = Array.ConvertAll(symbols, entry => entry.Symbol);
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

            // TODO can sometimes generate incomplete paths, look into https://shodhganga.inflibnet.ac.in/bitstream/10603/187253/9/09_chapter%204.pdf
            return FromBitCountsCanonical(symbolEntries);
        }
    }
}
