using System;
using System.Linq;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.Markers.Reader;
using BrotliLib.Huffman;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    partial class HuffmanTree<T>{

        // https://tools.ietf.org/html/rfc7932#section-3.4

        private static readonly BitDeserializer<HuffmanTree<T>, Context> SimpleCodeDeserialize = MarkedBitDeserializer.Wrap<HuffmanTree<T>, Context>(
            (reader, context) => {
                byte bitsPerSymbol = context.AlphabetSize.BitsPerSymbol;
                int symbolCount = reader.NextChunk(2, "NSYM", value => 1 + value);

                T[] symbols = reader.ReadValueArray(symbolCount, "symbol", () => context.BitsToSymbol(reader.NextChunk(bitsPerSymbol)));
                byte[] lengths = DetermineSimpleCodeLengths(reader, symbolCount);

                var symbolEntries = symbols.Zip(lengths, HuffmanGenerator<T>.MakeEntry).ToArray();
                return new HuffmanTree<T>(HuffmanGenerator<T>.FromBitCountsCanonical(symbolEntries));
            }
        );

        private static readonly BitSerializer<HuffmanTree<T>, Context> SimpleCodeSerialize = (writer, obj, context) => {
            byte bitsPerSymbol = context.AlphabetSize.BitsPerSymbol;

            writer.WriteChunk(2, obj.Root.SymbolCount - 1);

            foreach(T symbol in obj.OrderBy(kvp => kvp.Value.Length).Select(kvp => kvp.Key)){
                writer.WriteChunk(bitsPerSymbol, context.SymbolToBits(symbol));
            }

            if (obj.Root.SymbolCount == 4){
                writer.WriteBit(obj.MaxDepth > 2);
            }
        };

        /// <summary>
        /// Returns lengths of paths that are needed to encode exactly <paramref name="symbolCount"/> symbols.
        /// If <paramref name="symbolCount"/> equals 4, another bit is consumed from the <paramref name="reader"/> to determine the correct lengths.
        /// </summary>
        private static byte[] DetermineSimpleCodeLengths(IMarkedBitReader reader, int symbolCount){
            return symbolCount switch{
                1 => new byte[] { 0 },
                2 => new byte[] { 1, 1 },
                3 => new byte[] { 1, 2, 2 },
                4 => reader.NextBit("tree-select") ? new byte[] { 1, 2, 3, 3 } : new byte[] { 2, 2, 2, 2 },
                _ => throw new ArgumentOutOfRangeException(nameof(symbolCount), "The amount of symbols in a simple code must be in the range [1; 4]."),
            };
        }
    }
}
