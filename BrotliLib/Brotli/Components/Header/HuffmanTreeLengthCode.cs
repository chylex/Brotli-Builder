using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Collections.Huffman;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Numbers;
using BrotliLib.Serialization.Writer;
using LengthNode = BrotliLib.Collections.Huffman.HuffmanNode<byte>;

namespace BrotliLib.Brotli.Components.Header{
    public sealed class HuffmanTreeLengthCode : IComparable<HuffmanTreeLengthCode>{
        public const byte MaxLength = 15;
        public const byte Repeat = 16;
        public const byte Skip = 17;

        public const byte InitialRepeatedCode = 8;

        public const byte RepeatCodeExtraBits = 2;
        public const byte SkipCodeExtraBits = 3;
        
        /// <summary>
        /// Order of the complex length codes as they appear in the bit stream.
        /// </summary>
        private static readonly byte[] Order = {
            1, 2, 3, 4, 0, 5, Skip, 6, Repeat, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };

        /// <summary>
        /// List of all complex length codes ordered by their integer value.
        /// </summary>
        private static readonly HuffmanTreeLengthCode[] Codes = Enumerable.Range(0, Order.Length).Select(code => new HuffmanTreeLengthCode(code)).ToArray();

        /// <summary>
        /// Huffman tree used to encode lengths of the complex length codes.
        /// </summary>
        private static readonly LengthNode LengthTree = new LengthNode.Path(
            new LengthNode.Path(        // x0
                new LengthNode.Leaf(0), // 00
                new LengthNode.Leaf(3)  // 01
            ),
            new LengthNode.Path(                //   x1
                new LengthNode.Leaf(4),         //   01
                new LengthNode.Path(            //  x11
                    new LengthNode.Leaf(2),     //  011
                    new LengthNode.Path(        // x111
                        new LengthNode.Leaf(1), // 0111
                        new LengthNode.Leaf(5)  // 1111
                    )
                )
            )
        );

        private static readonly Dictionary<byte, BitPath> LengthLookup = LengthTree.GenerateValueMapOptimized();
        
        public const byte LengthMaxDepth = 5;
        private const int LengthBitSpace = 1 << LengthMaxDepth;

        public struct Run{
            public const int MinSpecialCodeLength = 3;

            public byte Symbol { get; }
            public int Length { get; }

            public Run(byte symbol, int length){
                this.Symbol = symbol;
                this.Length = length;
            }

            /// <summary>
            /// Does not use RLE for this run.
            /// </summary>
            public int Reject() => 0;

            /// <summary>
            /// Uses RLE for this run.
            /// </summary>
            public int Accept() => Length;

            /// <summary>
            /// Splits the run into two parts.
            /// The first part is <see cref="Length"/><c>-</c><paramref name="newRunLength"/> long and encoded without RLE.
            /// The second part is <paramref name="newRunLength"/> long and encoded with RLE, if possible.
            /// </summary>
            public int Split(int newRunLength){
                if (newRunLength < 1){
                    throw new ArgumentOutOfRangeException(nameof(newRunLength));
                }

                return newRunLength; // if newRunLength > Length, will crash in RunDecider.Resolve
            }
        }

        /// <summary>
        /// Decides substitution of runs representable by the <see cref="Repeat"/> and <see cref="Skip"/> codes.
        /// </summary>
        public sealed class RunDecider{
            /// <summary>
            /// Alphabet size of the encoded Huffman tree.
            /// </summary>
            public AlphabetSize AlphabetSize { get; }

            /// <summary>
            /// Amount of symbols whose lengths will actually be encoded. Does not include unused symbols that appear after the last used symbol.
            /// </summary>
            public int TrimmedSymbolCount { get; }

            private readonly List<byte> symbolLengths;

            public RunDecider(List<byte> symbolLengths, AlphabetSize alphabetSize){
                this.symbolLengths = symbolLengths;
                this.AlphabetSize = alphabetSize;
                this.TrimmedSymbolCount = symbolLengths.Count;
            }

            /// <summary>
            /// Returns the symbol bit length at the specified <paramref name="index"/>.
            /// </summary>
            public byte GetSymbolLength(int index){
                return symbolLengths[index];
            }

            /// <summary>
            /// Applies a resolution function over all runs in the data.
            /// </summary>
            public RunResolution Resolve(Func<Run, int> resolver){
                var resolution = new RunResolution.Builder(symbolLengths);

                for(int entryIndex = 0, lastRepeatStartIndex = 1, lastRepeatedCode = InitialRepeatedCode; entryIndex < resolution.CodeCount + 1; entryIndex++){
                    int nextCode = entryIndex < resolution.CodeCount ? resolution.GetCodeAt(entryIndex) : -1;

                    if (nextCode != lastRepeatedCode){
                        if (lastRepeatedCode == 0 || lastRepeatedCode == resolution.FindLastNonRepetitionNonZeroCode(lastRepeatStartIndex - 2)){
                            --lastRepeatStartIndex;
                        }

                        int runLength = entryIndex - lastRepeatStartIndex;

                        if (runLength >= Run.MinSpecialCodeLength){
                            int retained = resolver(new Run((byte)lastRepeatedCode, runLength));
                            
                            if (retained > runLength){
                                throw new InvalidOperationException("Cannot request encoding a run longer than originally asked for (" + retained + " > " + runLength + ").");
                            }

                            if (retained >= Run.MinSpecialCodeLength){
                                int runStartIndex = lastRepeatStartIndex + runLength - retained;
                                
                                entryIndex = lastRepeatedCode == 0 ? resolution.EncodeRepetition(runStartIndex, Skip, retained, 1 << SkipCodeExtraBits)
                                                                   : resolution.EncodeRepetition(runStartIndex, Repeat, retained, 1 << RepeatCodeExtraBits);
                            }
                        }
                        
                        lastRepeatedCode = nextCode;
                        lastRepeatStartIndex = entryIndex + 1;
                    }
                }

                return resolution.Build();
            }
        }

        public sealed class RunResolution{
            private readonly List<byte> codes;
            private readonly Queue<byte> extra;

            public RunResolution(List<byte> codes, Queue<byte> extra){
                this.codes = codes;
                this.extra = extra;
            }

            public (List<byte>, Queue<byte>) GenerateCodesAndExtraBits(){
                return (new List<byte>(codes), new Queue<byte>(extra));
            }

            public sealed class Builder{
                public int CodeCount => codes!.Count;

                private List<byte>? codes;
                private Queue<byte>? extra;

                public Builder(List<byte> symbolLengths){
                    this.codes = new List<byte>(symbolLengths);
                    this.extra = new Queue<byte>();
                }

                public int GetCodeAt(int index){
                    return codes![index];
                }

                public byte FindLastNonRepetitionNonZeroCode(int startIndex){
                    for(int index = startIndex; index >= 0; index--){
                        byte bits = codes![index];

                        if (bits != 0 && bits != Skip && bits != Repeat){
                            return bits;
                        }
                    }

                    return 0;
                }

                private int DetermineExtraBits(int length, int multiplier){
                    var newExtra = new Stack<byte>();
                    int remaining = length - Run.MinSpecialCodeLength;

                    do{
                        remaining = Math.DivRem(remaining, multiplier, out int remainder);
                        newExtra.Push((byte)remainder);
                    }while(--remaining >= 0);

                    foreach(byte entry in newExtra){
                        extra!.Enqueue(entry);
                    }

                    return newExtra.Count;
                }

                public int EncodeRepetition(int index, byte code, int length, int multiplier){
                    int repetitions = DetermineExtraBits(length, multiplier);
                    
                    codes!.RemoveRange(index, length);
                    codes!.InsertRange(index, Enumerable.Repeat(code, repetitions));

                    return index + repetitions;
                }

                public RunResolution Build(){
                    if (codes == null || extra == null){
                        throw new InvalidOperationException("The builder has already been built.");
                    }

                    var built = new RunResolution(codes, extra);
                    codes = null;
                    extra = null;
                    return built;
                }
            }
        }
        
        // Data

        public byte Code { get; }

        private HuffmanTreeLengthCode(int code){
            this.Code = (byte)code;
        }

        public int CompareTo(HuffmanTreeLengthCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public override bool Equals(object obj){
            return obj is HuffmanTreeLengthCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Code);
        }

        public override string ToString(){
            return Code switch{
                Repeat => "Repeat",
                Skip   => "Skip",
                _      => "Length = " + Code
            };
        }

        // Serialization

        internal static HuffmanNode<HuffmanTreeLengthCode> Read(IMarkedBitReader reader, int skippedAmount) => reader.MarkTitle("Bit Lengths", () => {
            byte[] bitCounts = new byte[Codes.Length];

            for(int index = skippedAmount, bitSpaceRemaining = LengthBitSpace; bitSpaceRemaining > 0 && index < Order.Length; index++){
                byte bitCount = reader.ReadValue(LengthTree, "bit length for code " + Order[index]);

                if (bitCount != 0){
                    bitCounts[Order[index]] = bitCount;
                    bitSpaceRemaining -= LengthBitSpace >> bitCount;
                }
            }

            var lengthEntries = Codes.Zip(bitCounts, HuffmanGenerator<HuffmanTreeLengthCode>.MakeEntry).ToArray();
            var filteredLengthEntries = lengthEntries.Where(entry => entry.Bits > 0).ToArray();

            return HuffmanGenerator<HuffmanTreeLengthCode>.FromBitCountsCanonical(filteredLengthEntries.Length == 0 ? lengthEntries : filteredLengthEntries);
        });

        internal static void Write(IBitWriter writer, IReadOnlyDictionary<byte, BitPath> lengthMap){
            int skippedAmount;

            if (!lengthMap.ContainsKey(Order[0]) && !lengthMap.ContainsKey(Order[1])){
                skippedAmount = lengthMap.ContainsKey(Order[2]) ? 2 : 3;
            }
            else{
                skippedAmount = 0;
            }

            writer.WriteChunk(2, skippedAmount);

            if (lengthMap.Count == 1){
                // if lengthMap has only 1 element, its path length is zero, which would omit the element completely
                // instead, a length of 3 is chosen because its path is encoded using only 2 bits
                lengthMap = new Dictionary<byte, BitPath>{
                    { lengthMap.Keys.First(), new BitPath(0, 3) }
                };
            }
            
            for(int index = skippedAmount, bitSpaceRemaining = LengthBitSpace; bitSpaceRemaining > 0 && index < Order.Length; index++){
                byte code = Order[index];
                byte length = lengthMap.TryGetValue(code, out BitPath path) ? path.Length : (byte)0;

                writer.WriteBits(LengthLookup[length]);

                if (length > 0){
                    bitSpaceRemaining -= LengthBitSpace >> length;
                }
            }
        }
    }
}
