using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.Collections;
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
        /// Generates a canonical depth-limited Huffman tree using the provided <paramref name="symbolFrequencies"/>.
        /// </summary>
        public static HuffmanTree<T> FromSymbols(FrequencyList<T> symbolFrequencies, byte maxDepth = DefaultMaxDepth){
            if (symbolFrequencies.Count == 0){
                throw new ArgumentOutOfRangeException(nameof(symbolFrequencies), "Cannot generate a tree with no symbols.");
            }
            else if (symbolFrequencies.Count == 1){
                return new HuffmanTree<T>(new HuffmanNode<T>.Leaf(symbolFrequencies.First()));
            }
            else{
                return new HuffmanTree<T>(HuffmanGenerator<T>.FromFrequenciesCanonical(symbolFrequencies.HuffmanFreq, maxDepth));
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

        public static readonly BitDeserializer<HuffmanTree<T>, Context> Deserialize = MarkedBitDeserializer.Wrap<HuffmanTree<T>, Context>(
            (reader, context) => {
                reader.MarkStart();

                int type = reader.NextChunk(2, "HSKIP");
                HuffmanTree<T> tree;

                if (type == 1){
                    tree = SimpleCodeDeserialize(reader, context);
                    reader.MarkEnd(new TitleMarker("Simple Huffman Tree"));
                }
                else{
                    context.SkippedComplexCodeLengths = type;
                    tree = ComplexCodeDeserialize(reader, context);
                    reader.MarkEnd(new TitleMarker("Complex Huffman Tree"));
                }

                return tree;
            }
        );

        public static readonly BitSerializer<HuffmanTree<T>, Context> Serialize = (writer, obj, context) => {
            if (obj.Root.SymbolCount <= 4){
                writer.WriteChunk(2, 0b01);
                SimpleCodeSerialize(writer, obj, context);
            }
            else{
                // type identifier is written by the serializer
                ComplexCodeSerialize(writer, obj, context);
            }
        };
    }
}
