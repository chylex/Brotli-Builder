using System;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Collections;
using BrotliLib.Collections.Huffman;

namespace BrotliImpl.Combined{
    partial class CompressQuality0{
        private static class Huffman{
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (BuildAndStoreLiteralPrefixCode).
            /// </summary>
            private static FrequencyList<Literal> SampleLiterals(in ArraySegment<byte> bytes){
                FrequencyList<Literal> samples = new FrequencyList<Literal>();

                if (bytes.Count < (1 << 15)){
                    foreach(byte b in bytes){
                        samples.Add(new Literal(b));
                    }

                    for(int symbol = 0; symbol < 256; symbol++){
                        var literal = new Literal((byte)symbol);

                        if (samples.Contains(literal)){
                            samples[literal] += 2 * Math.Min(samples[literal], 11);
                        }
                    }
                }
                else{
                    const int sampleRate = 29;

                    for(int index = 0; index < bytes.Count; index += sampleRate){
                        samples.Add(new Literal(bytes[index]));
                    }

                    for(int symbol = 0; symbol < 256; symbol++){
                        var literal = new Literal((byte)symbol);

                        samples[literal] += 1 + 2 * Math.Min(samples[literal], 11);
                    }
                }

                return samples;
            }

            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (BuildAndStoreLiteralPrefixCode).
            /// </summary>
            public static (HuffmanTree<Literal>, int) EstimateLiteralRatio(in ArraySegment<byte> bytes){
                var literalHistogram = SampleLiterals(bytes);
                var literalTree = HuffmanTree<Literal>.FromSymbols(literalHistogram);

                int histogramTotal = literalHistogram.Sum(literal => literalHistogram[literal]);
                int histogramRatio = 0;

                foreach(var symbol in literalHistogram){
                    histogramRatio += literalHistogram[symbol] * literalTree.FindPath(symbol).Length;
                }

                return (literalTree, (histogramRatio * 125) / histogramTotal);
            }

            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (InitCommandPrefixCodes).
            /// </summary>
            public static readonly HuffmanTree<InsertCopyLengthCode> PredefinedLengthCodes = new HuffmanTree<InsertCopyLengthCode>(
                HuffmanGenerator<InsertCopyLengthCode>.FromBitCountsCanonical(new HuffmanGenerator<InsertCopyLengthCode>.Entry[]{
                    LengthCodeWithPathLength(  1,  4),
                    LengthCodeWithPathLength(  2,  4),
                    LengthCodeWithPathLength(  3,  5),
                    LengthCodeWithPathLength(  4,  6),
                    LengthCodeWithPathLength(  5,  6),
                    LengthCodeWithPathLength(  6,  7),
                    LengthCodeWithPathLength(  7,  7),
                    LengthCodeWithPathLength( 64,  7),
                    LengthCodeWithPathLength( 65,  7),
                    LengthCodeWithPathLength( 66,  7),
                    LengthCodeWithPathLength( 67,  8),
                    LengthCodeWithPathLength( 68,  8),
                    LengthCodeWithPathLength( 69,  8),
                    LengthCodeWithPathLength( 70,  8),
                    LengthCodeWithPathLength( 71,  8),
                    LengthCodeWithPathLength(131,  4),
                    LengthCodeWithPathLength(132,  4),
                    LengthCodeWithPathLength(133,  4),
                    LengthCodeWithPathLength(134,  4),
                    LengthCodeWithPathLength(135,  4),
                    LengthCodeWithPathLength(136,  4),
                    LengthCodeWithPathLength(144,  4),
                    LengthCodeWithPathLength(152,  5),
                    LengthCodeWithPathLength(160,  5),
                    LengthCodeWithPathLength(168,  5),
                    LengthCodeWithPathLength(176,  6),
                    LengthCodeWithPathLength(184,  6),
                    LengthCodeWithPathLength(192,  5),
                    LengthCodeWithPathLength(193,  5),
                    LengthCodeWithPathLength(194,  6),
                    LengthCodeWithPathLength(195,  6),
                    LengthCodeWithPathLength(196,  6),
                    LengthCodeWithPathLength(197,  6),
                    LengthCodeWithPathLength(198,  7),
                    LengthCodeWithPathLength(199,  7),
                    LengthCodeWithPathLength(256,  7),
                    LengthCodeWithPathLength(264,  8),
                    LengthCodeWithPathLength(272,  8),
                    LengthCodeWithPathLength(280,  9),
                    LengthCodeWithPathLength(288, 10),
                    LengthCodeWithPathLength(296, 10),
                    LengthCodeWithPathLength(304, 10),
                    LengthCodeWithPathLength(312, 10),
                    LengthCodeWithPathLength(384,  7),
                    LengthCodeWithPathLength(385,  7),
                    LengthCodeWithPathLength(386, 10),
                    LengthCodeWithPathLength(387, 10),
                    LengthCodeWithPathLength(388, 10),
                    LengthCodeWithPathLength(389, 10),
                    LengthCodeWithPathLength(390, 10),
                    LengthCodeWithPathLength(391, 10),
                    LengthCodeWithPathLength(448, 10),
                    LengthCodeWithPathLength(456, 10),
                    LengthCodeWithPathLength(464, 10),
                    LengthCodeWithPathLength(472, 10),
                    LengthCodeWithPathLength(480, 10),
                    LengthCodeWithPathLength(488, 10),
                    LengthCodeWithPathLength(496, 10),
                    LengthCodeWithPathLength(504, 10)
                })
            );
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/encode.c (InitCommandPrefixCodes).
            /// </summary>
            public static readonly HuffmanTree<DistanceCode> PredefinedDistanceCodes = new HuffmanTree<DistanceCode>(
                HuffmanGenerator<DistanceCode>.FromBitCountsCanonical(new HuffmanGenerator<DistanceCode>.Entry[]{
                    DistanceCodeWithPathLength( 0,  5),
                    DistanceCodeWithPathLength(16,  6),
                    DistanceCodeWithPathLength(17,  6),
                    DistanceCodeWithPathLength(18,  6),
                    DistanceCodeWithPathLength(19,  6),
                    DistanceCodeWithPathLength(20,  6),
                    DistanceCodeWithPathLength(21,  6),
                    DistanceCodeWithPathLength(22,  5),
                    DistanceCodeWithPathLength(23,  5),
                    DistanceCodeWithPathLength(24,  5),
                    DistanceCodeWithPathLength(25,  5),
                    DistanceCodeWithPathLength(26,  5),
                    DistanceCodeWithPathLength(27,  5),
                    DistanceCodeWithPathLength(28,  4),
                    DistanceCodeWithPathLength(29,  4),
                    DistanceCodeWithPathLength(30,  4),
                    DistanceCodeWithPathLength(31,  4),
                    DistanceCodeWithPathLength(32,  4),
                    DistanceCodeWithPathLength(33,  4),
                    DistanceCodeWithPathLength(34,  4),
                    DistanceCodeWithPathLength(35,  5),
                    DistanceCodeWithPathLength(36,  5),
                    DistanceCodeWithPathLength(37,  5),
                    DistanceCodeWithPathLength(38,  5),
                    DistanceCodeWithPathLength(39,  5),
                    DistanceCodeWithPathLength(40,  5),
                    DistanceCodeWithPathLength(41,  6),
                    DistanceCodeWithPathLength(42,  6),
                    DistanceCodeWithPathLength(43,  7),
                    DistanceCodeWithPathLength(44,  7),
                    DistanceCodeWithPathLength(45,  7),
                    DistanceCodeWithPathLength(46,  8),
                    DistanceCodeWithPathLength(47, 10),
                    DistanceCodeWithPathLength(48, 12),
                    DistanceCodeWithPathLength(49, 12),
                    DistanceCodeWithPathLength(50, 12),
                    DistanceCodeWithPathLength(51, 12),
                    DistanceCodeWithPathLength(52, 12),
                    DistanceCodeWithPathLength(53, 12),
                    DistanceCodeWithPathLength(54, 12),
                    DistanceCodeWithPathLength(55, 12),
                    DistanceCodeWithPathLength(56, 12),
                    DistanceCodeWithPathLength(57, 12),
                    DistanceCodeWithPathLength(58, 12),
                    DistanceCodeWithPathLength(59, 12)
                })
            );

            // Helpers

            private static HuffmanGenerator<InsertCopyLengthCode>.Entry LengthCodeWithPathLength(int code, byte length){
                return new HuffmanGenerator<InsertCopyLengthCode>.Entry(new InsertCopyLengthCode(code), length);
            }

            private static HuffmanGenerator<DistanceCode>.Entry DistanceCodeWithPathLength(int code, byte length){
                return new HuffmanGenerator<DistanceCode>.Entry(DistanceCode.Create(DistanceParameters.Zero, code), length);
            }
        }
    }
}
