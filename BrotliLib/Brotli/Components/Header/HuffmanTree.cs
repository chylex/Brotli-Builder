using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Huffman;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Represents a Huffman tree with symbols from various alphabets in the Brotli format. Provides helper methods for iteration and lookups.
    /// https://tools.ietf.org/html/rfc7932#section-3.2
    /// </summary>
    public sealed partial class HuffmanTree<T> : IEnumerable<KeyValuePair<T, BitStream>> where T : IComparable<T>{
        public const int DefaultMaxDepth = 15;

        /// <summary>
        /// Generates a canonical depth-limited Huffman tree using the frequencies of provided <paramref name="symbols"/>.
        /// </summary>
        public static HuffmanTree<T> FromSymbols(IReadOnlyList<T> symbols, byte maxDepth = DefaultMaxDepth){
            if (symbols.Count == 0){
                throw new ArgumentOutOfRangeException(nameof(symbols), "Cannot generate a tree with no symbols.");
            }
            else if (symbols.Count == 1){
                return new HuffmanTree<T>(new HuffmanNode<T>.Leaf(symbols[0]));
            }
            else{
                var frequencies = symbols.GroupBy(symbol => symbol).Select(group => new HuffmanGenerator<T>.SymbolFreq(group.Key, group.Count())).ToArray();
                var root = HuffmanGenerator<T>.FromFrequenciesCanonical(frequencies, maxDepth);

                return new HuffmanTree<T>(root);
            }
        }

        // Data

        /// <summary>
        /// Root node of the tree.
        /// </summary>
        public HuffmanNode<T> Root { get; }

        /// <summary>
        /// Length of the longest path in the tree.
        /// </summary>
        public int MaxDepth => reverseLookup.Values.Max(stream => stream.Length);

        private readonly Dictionary<T, BitStream> reverseLookup;
        
        public HuffmanTree(HuffmanNode<T> root){
            this.reverseLookup = root.GenerateValueMap();
            this.Root = root;
        }

        public BitStream FindPath(T element){
            return reverseLookup[element];
        }

        public BitStream FindPathOrNull(T element){
            return reverseLookup.TryGetValue(element, out BitStream path) ? path : null;
        }

        public KeyValuePair<T, BitStream> FindEntry(Predicate<T> predicate){
            return reverseLookup.First(kvp => predicate(kvp.Key));
        }

        public IEnumerator<KeyValuePair<T, BitStream>> GetEnumerator(){
            return reverseLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        // Object

        public override bool Equals(object obj){
            return obj is HuffmanTree<T> other && reverseLookup.SequenceEqual(other.reverseLookup);
        }

        public override int GetHashCode(){
            int hash = reverseLookup.Count * 17;

            foreach(KeyValuePair<T, BitStream> kvp in this){
                hash = unchecked((hash * 31) + kvp.Key.GetHashCode());
                hash = unchecked((hash * 31) + kvp.Value.GetHashCode());
            }

            return hash;
        }

        // Context

        public class Context{
            public AlphabetSize AlphabetSize { get; }

            public Func<int, T> BitsToSymbol { get; }
            public Func<T, int> SymbolToBits { get; }

            internal int SkippedComplexCodeLengths { get; set; }
            
            public Context(AlphabetSize alphabetSize, Func<int, T> bitsToSymbol, Func<T, int> symbolToBits){
                this.AlphabetSize = alphabetSize;
                this.BitsToSymbol = bitsToSymbol;
                this.SymbolToBits = symbolToBits;
            }
        }

        // Serialization

        public static readonly IBitSerializer<HuffmanTree<T>, Context> Serializer = new MarkedBitSerializer<HuffmanTree<T>, Context>(
            fromBits: (reader, context) => {
                reader.MarkStart();

                int type = reader.NextChunk(2, "HSKIP");
                HuffmanTree<T> tree;

                if (type == 1){
                    tree = SimpleCodeSerializer.FromBits(reader, context);
                    reader.MarkEnd(new TitleMarker("Simple Huffman Tree"));
                }
                else{
                    context.SkippedComplexCodeLengths = type;
                    tree = ComplexCodeSerializer.FromBits(reader, context);
                    reader.MarkEnd(new TitleMarker("Complex Huffman Tree"));
                }

                return tree;
            },

            toBits: (writer, obj, context) => {
                if (obj.Root.SymbolCount <= 4){
                    writer.WriteChunk(2, 0b01);
                    SimpleCodeSerializer.ToBits(writer, obj, context);
                }
                else{
                    // type identifier is written by the serializer
                    ComplexCodeSerializer.ToBits(writer, obj, context);
                }
            }
        );
    }
}
