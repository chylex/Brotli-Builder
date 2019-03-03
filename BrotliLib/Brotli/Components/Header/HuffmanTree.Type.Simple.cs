using System;
using System.Linq;
using BrotliLib.Brotli.Markers;
using BrotliLib.Huffman;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    partial class HuffmanTree<T>{
        /// <summary>
        /// https://tools.ietf.org/html/rfc7932#section-3.4
        /// </summary>
        private static readonly IBitSerializer<HuffmanTree<T>, Context> SimpleCodeSerializer = new MarkedBitSerializer<HuffmanTree<T>, Context>(
            fromBits: (reader, context) => {
                byte bitsPerSymbol = context.AlphabetSize.BitsPerSymbol;
                int symbolCount = reader.NextChunk(2, "NSYM", value => 1 + value);

                T[] symbols = reader.ReadValueArray(symbolCount, "symbol", () => context.BitsToSymbol(reader.NextChunk(bitsPerSymbol)));
                byte[] lengths = DetermineSimpleCodeLengths(reader, symbolCount);

                var symbolEntries = symbols.Zip(lengths, HuffmanGenerator<T>.MakeEntry).ToArray();
                return new HuffmanTree<T>(HuffmanGenerator<T>.FromBitCountsCanonical(symbolEntries));
            },

            toBits: (writer, obj, context) => {
                byte bitsPerSymbol = context.AlphabetSize.BitsPerSymbol;

                writer.WriteChunk(2, obj.Root.SymbolCount - 1);

                foreach(T symbol in obj.OrderBy(kvp => kvp.Value.Length).Select(kvp => kvp.Key)){
                    writer.WriteChunk(bitsPerSymbol, context.SymbolToBits(symbol));
                }

                if (obj.Root.SymbolCount == 4){
                    writer.WriteBit(obj.MaxDepth > 2);
                }
            }
        );

        /// <summary>
        /// Returns lengths of paths that are needed to encode exactly <paramref name="symbolCount"/> symbols.
        /// If <paramref name="symbolCount"/> equals 4, another bit is consumed from the <paramref name="reader"/> to determine the correct lengths.
        /// </summary>
        private static byte[] DetermineSimpleCodeLengths(MarkedBitReader reader, int symbolCount){
            switch(symbolCount){
                case 1: return new byte[]{ 0 };
                case 2: return new byte[]{ 1, 1 };
                case 3: return new byte[]{ 1, 2, 2 };
                case 4: return reader.NextBit("tree-select") ? new byte[]{ 1, 2, 3, 3 } : new byte[]{ 2, 2, 2, 2 };
                default: throw new ArgumentOutOfRangeException(nameof(symbolCount), "The amount of symbols in a simple code must be in the range [1; 4].");
            }
        }
    }
}
