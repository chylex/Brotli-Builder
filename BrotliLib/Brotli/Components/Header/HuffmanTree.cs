﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Collections;
using BrotliLib.Collections.Huffman;
using BrotliLib.Markers.Serialization;
using BrotliLib.Numbers;
using BrotliLib.Serialization;

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
        public int MaxDepth => ReverseLookup.Values.Max(stream => stream.Length);

        private Dictionary<T, BitStream> ReverseLookup => reverseLookupCached ??= Root.GenerateValueMap();
        private Dictionary<T, BitStream> reverseLookupCached;
        
        public HuffmanTree(HuffmanNode<T> root){
            this.Root = root;
        }

        public BitStream FindPath(T element){
            return ReverseLookup[element];
        }

        public BitStream FindPathOrNull(T element){
            return ReverseLookup.TryGetValue(element, out BitStream path) ? path : null;
        }

        public KeyValuePair<T, BitStream> FindEntry(Predicate<T> predicate){
            return ReverseLookup.First(kvp => predicate(kvp.Key));
        }

        public IEnumerator<KeyValuePair<T, BitStream>> GetEnumerator(){
            return ReverseLookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        // Object

        public override bool Equals(object obj){
            return obj is HuffmanTree<T> tree &&
                   ReverseLookup.SequenceEqual(tree.ReverseLookup);
        }

        public override int GetHashCode(){
            int hash = ReverseLookup.Count * 17;

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

            internal int SkippedComplexCodeLengths { get; }
            
            private Context(AlphabetSize alphabetSize, Func<int, T> bitsToSymbol, Func<T, int> symbolToBits, int skippedComplexCodeLengths){
                this.AlphabetSize = alphabetSize;
                this.BitsToSymbol = bitsToSymbol;
                this.SymbolToBits = symbolToBits;
                this.SkippedComplexCodeLengths = skippedComplexCodeLengths;
            }

            public Context(AlphabetSize alphabetSize, Func<int, T> bitsToSymbol, Func<T, int> symbolToBits) : this(alphabetSize, bitsToSymbol, symbolToBits, -1){}

            public Context ForComplexDeserialization(int skippedComplexCodeLengths){
                return new Context(AlphabetSize, BitsToSymbol, SymbolToBits, skippedComplexCodeLengths);
            }
        }

        // Serialization

        public static readonly BitDeserializer<HuffmanTree<T>, Context> Deserialize = MarkedBitDeserializer.Wrap<HuffmanTree<T>, Context>(
            (reader, context) => {
                reader.MarkStart();

                int type = reader.NextChunk(2, "HSKIP");
                HuffmanTree<T> tree;

                if (type == 1){
                    tree = Simple.Deserialize(reader, context);
                    reader.MarkEndTitle("Simple Huffman Tree");
                }
                else{
                    tree = Complex.Deserialize(reader, context.ForComplexDeserialization(type));
                    reader.MarkEndTitle("Complex Huffman Tree");
                }

                return tree;
            }
        );

        public static readonly BitSerializer<HuffmanTree<T>, Context> Serialize = (writer, obj, context) => {
            if (obj.Root.SymbolCount <= 4){
                writer.WriteChunk(2, 0b01);
                Simple.Serialize(writer, obj, context);
            }
            else{
                // type identifier is written by the serializer
                Complex.Serialize(writer, obj, context);
            }
        };
    }
}
