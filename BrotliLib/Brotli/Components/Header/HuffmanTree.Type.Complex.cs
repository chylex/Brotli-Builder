using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Collections.Huffman;
using BrotliLib.Markers.Serialization;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Writer;
using ComplexLengthNode = BrotliLib.Collections.Huffman.HuffmanNode<byte>;

namespace BrotliLib.Brotli.Components.Header{
    partial class HuffmanTree<T>{
        /// <summary>
        /// https://tools.ietf.org/html/rfc7932#section-3.5
        /// </summary>
        private static class Complex{
            private const int SymbolBitSpace = 1 << DefaultMaxDepth;
            private const byte NoForcedCode = byte.MaxValue;

            public static readonly BitDeserializer<HuffmanTree<T>, Context> Deserialize = MarkedBitDeserializer.Wrap<HuffmanTree<T>, Context>(
                (reader, context) => {
                    Func<int, T> getSymbol = context.BitsToSymbol;

                    var defaultRepeatCode = new HuffmanGenerator<T>.Entry(getSymbol(0), ComplexLengthCode.DefaultRepeatCode);
                    var lengthCodes = ComplexLengthCode.Read(reader, context.SkippedComplexCodeLengths);
                    byte nextForcedCode = NoForcedCode;

                    int symbolIndex = 0;
                    int symbolCount = context.AlphabetSize.SymbolCount;
                    var symbolEntries = new List<HuffmanGenerator<T>.Entry>(symbolCount);

                    int bitSpaceRemaining = SymbolBitSpace;

                    reader.MarkStart();

                    void AddMarkedSymbol(HuffmanGenerator<T>.Entry entry){
                        symbolEntries.Add(entry);
                        reader.MarkStart();
                        reader.MarkEndValue("entry", entry);
                    }
                    
                    while(bitSpaceRemaining > 0 && symbolIndex < symbolCount){
                        byte nextCode;

                        if (nextForcedCode == NoForcedCode){
                            nextCode = reader.ReadValue(lengthCodes, "code", code => code.Code);
                        }
                        else{
                            nextCode = nextForcedCode;
                            nextForcedCode = NoForcedCode;
                        }
                        
                        if (nextCode == ComplexLengthCode.Skip){
                            reader.MarkStart();

                            int NextSkipData() => 3 + reader.NextChunk(3, "skip data");
                            int skipCount = NextSkipData();

                            while((nextForcedCode = reader.ReadValue(lengthCodes, "code", code => code.Code)) == ComplexLengthCode.Skip){
                                skipCount = 8 * (skipCount - 2) + NextSkipData();
                            }

                            reader.MarkEndValue("skip count", skipCount);

                            symbolIndex += skipCount;
                        }
                        else if (nextCode == ComplexLengthCode.Repeat){
                            byte repeatedCode = symbolEntries.DefaultIfEmpty(defaultRepeatCode).Select(entry => entry.Bits).Last(value => value > 0);
                            int sumPerRepeat = SymbolBitSpace >> repeatedCode;
                            
                            reader.MarkStart();
                            
                            int NextRepeatData() => 3 + reader.NextChunk(2, "repeat data");
                            int repeatCount = NextRepeatData();

                            while(bitSpaceRemaining - sumPerRepeat * repeatCount > 0 && (nextForcedCode = reader.ReadValue(lengthCodes, "code", code => code.Code)) == ComplexLengthCode.Repeat){
                                repeatCount = 4 * (repeatCount - 2) + NextRepeatData();
                            }

                            reader.MarkEndValue("repeat count", repeatCount);

                            bitSpaceRemaining -= sumPerRepeat * repeatCount;
                        
                            while(--repeatCount >= 0){
                                AddMarkedSymbol(new HuffmanGenerator<T>.Entry(getSymbol(symbolIndex++), repeatedCode));
                            }
                        }
                        else if (nextCode == 0){
                            ++symbolIndex;
                        }
                        else if (nextCode <= ComplexLengthCode.MaxLength){
                            AddMarkedSymbol(new HuffmanGenerator<T>.Entry(getSymbol(symbolIndex++), nextCode));
                            bitSpaceRemaining -= SymbolBitSpace >> nextCode;
                        }
                    }

                    reader.MarkEndTitle("Symbols");

                    return new HuffmanTree<T>(HuffmanGenerator<T>.FromBitCountsCanonical(symbolEntries));
                }
            );

            public static readonly BitSerializer<HuffmanTree<T>, Context> Serialize = (writer, obj, context) => {
                Func<int, T> getSymbol = context.BitsToSymbol;
                
                int symbolCount = context.AlphabetSize.SymbolCount;
                var symbolEntries = new List<HuffmanGenerator<T>.Entry>();

                Queue<byte> extra = new Queue<byte>();
                
                for(int symbolIndex = 0, bitSpaceRemaining = SymbolBitSpace; bitSpaceRemaining > 0 && symbolIndex < symbolCount; symbolIndex++){
                    T symbol = getSymbol(symbolIndex);
                    BitStream path = obj.FindPathOrNull(symbol);

                    byte length = (byte)(path?.Length ?? 0);
                    
                    if (length > 0){
                        bitSpaceRemaining -= SymbolBitSpace >> length;
                    }
                    else if (symbolIndex == symbolCount - 1){
                        length = 15; // if the tree is incomplete, a zero lengh last symbol would generate an ending length code 0 or 17, which Brotli spec forbids
                                     // if the tree is complete and the final symbol was supposed to be a 0, the writer will run out of bit space before it writes the final symbol
                    }

                    symbolEntries.Add(new HuffmanGenerator<T>.Entry(symbol, length));
                }

                int ProcessRepetitions(int length, int multiplier){
                    Stack<byte> newExtra = new Stack<byte>();

                    int remaining = length - 3;

                    do{
                        newExtra.Push((byte)(remaining % multiplier));
                        remaining /= multiplier;
                    }while(--remaining >= 0);

                    foreach(byte entry in newExtra){
                        extra.Enqueue(entry);
                    }

                    return newExtra.Count;
                }

                int ReplaceSequence(int index, byte code, int removeLength, int insertLength){
                    symbolEntries.RemoveRange(index, removeLength);
                    symbolEntries.InsertRange(index, Enumerable.Repeat(new HuffmanGenerator<T>.Entry(default, code), insertLength));
                    return index + insertLength;
                }

                int ReplaceSequenceWithRepetition(int index, byte code, int length, int multiplier){
                    if (length - 3 == multiplier){
                        // when the amount of repetitions equals the first value that requires a second repetition code to encode, it's more efficient to write it as 1 literal code and 1 repetition code
                        // TODO official compressor (and this) only works for the first value that crosses the boundary... potential point for improvement?
                        --length;
                        ++index;
                    }

                    return ReplaceSequence(index, code, length, ProcessRepetitions(length, multiplier));
                }
                
                for(int entryIndex = 0, lastRepeatStartIndex = 0, lastRepeatCode = ComplexLengthCode.DefaultRepeatCode; entryIndex < symbolEntries.Count + 1; entryIndex++){
                    int nextCode = entryIndex < symbolEntries.Count ? symbolEntries[entryIndex].Bits : -1;

                    if (nextCode != lastRepeatCode){
                        if (lastRepeatCode == 0){
                            --lastRepeatStartIndex;
                        }

                        int skipLength = entryIndex - lastRepeatStartIndex;

                        if (skipLength >= 3){
                            entryIndex = lastRepeatCode == 0 ? ReplaceSequenceWithRepetition(lastRepeatStartIndex, ComplexLengthCode.Skip, skipLength, 8)
                                                             : ReplaceSequenceWithRepetition(lastRepeatStartIndex, ComplexLengthCode.Repeat, skipLength, 4);
                        }
                        
                        lastRepeatCode = nextCode;
                        lastRepeatStartIndex = entryIndex + 1;
                    }
                }
                
                var lengthEntries = symbolEntries.GroupBy(kvp => kvp.Bits).Select(group => new HuffmanGenerator<byte>.SymbolFreq(group.Key, group.Count())).ToArray();
                var lengthMap = HuffmanGenerator<byte>.FromFrequenciesCanonical(lengthEntries, ComplexLengthCode.LengthMaxDepth).GenerateValueMap();
                
                ComplexLengthCode.Write(writer, lengthMap);
                
                foreach(byte code in symbolEntries.Select(entry => entry.Bits)){
                    writer.WriteBits(lengthMap[code]);

                    if (code == ComplexLengthCode.Skip){
                        writer.WriteChunk(3, extra.Dequeue());
                    }
                    else if (code == ComplexLengthCode.Repeat){
                        writer.WriteChunk(2, extra.Dequeue());
                    }
                }
            };
        }
    }

    internal sealed class ComplexLengthCode : IComparable<ComplexLengthCode>{
        public const byte MaxLength = 15;
        public const byte Repeat = 16;
        public const byte Skip = 17;

        public const byte DefaultRepeatCode = 8;
        
        /// <summary>
        /// Order of the complex length codes as they appear in the bit stream.
        /// </summary>
        private static readonly byte[] Order = {
            1, 2, 3, 4, 0, 5, Skip, 6, Repeat, 7, 8, 9, 10, 11, 12, 13, 14, 15
        };

        /// <summary>
        /// List of all complex length codes ordered by their integer value.
        /// </summary>
        private static readonly ComplexLengthCode[] Codes = Enumerable.Range(0, Order.Length).Select(code => new ComplexLengthCode(code)).ToArray();

        /// <summary>
        /// Huffman tree used to encode lengths of the complex length codes.
        /// </summary>
        private static readonly ComplexLengthNode Lengths = new ComplexLengthNode.Path(
            new ComplexLengthNode.Path(        // x0
                new ComplexLengthNode.Leaf(0), // 00
                new ComplexLengthNode.Leaf(3)  // 01
            ),
            new ComplexLengthNode.Path(                //   x1
                new ComplexLengthNode.Leaf(4),         //   01
                new ComplexLengthNode.Path(            //  x11
                    new ComplexLengthNode.Leaf(2),     //  011
                    new ComplexLengthNode.Path(        // x111
                        new ComplexLengthNode.Leaf(1), // 0111
                        new ComplexLengthNode.Leaf(5)  // 1111
                    )
                )
            )
        );

        private static readonly Dictionary<byte, BitStream> LengthLookup = Lengths.GenerateValueMap();
        
        public const byte LengthMaxDepth = 5;
        private const int LengthBitSpace = 1 << LengthMaxDepth;
        
        // Data

        public byte Code { get; }

        private ComplexLengthCode(int code){
            this.Code = (byte)code;
        }

        public int CompareTo(ComplexLengthCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public override bool Equals(object obj){
            return obj is ComplexLengthCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Code);
        }

        public override string ToString(){
            return Code switch{
                Repeat => "Repeat",
                Skip => "Skip",
                _ => "Length = " + Code,
            };
        }

        // Serialization

        public static void Write(IBitWriter writer, IDictionary<byte, BitStream> lengthMap){
            int skippedAmount;

            if (!lengthMap.ContainsKey(Order[0]) && !lengthMap.ContainsKey(Order[1])){
                skippedAmount = lengthMap.ContainsKey(Order[2]) ? 2 : 3;
            }
            else{
                skippedAmount = 0;
            }

            writer.WriteChunk(2, skippedAmount);
            
            for(int index = skippedAmount, bitSpaceRemaining = LengthBitSpace; bitSpaceRemaining > 0 && index < Order.Length; index++){
                byte code = Order[index];
                byte length = (byte)(lengthMap.TryGetValue(code, out BitStream stream) ? stream.Length : 0);

                writer.WriteBits(LengthLookup[length]);

                if (length > 0){
                    bitSpaceRemaining -= LengthBitSpace >> length;
                }
            }
        }

        public static HuffmanNode<ComplexLengthCode> Read(IMarkedBitReader reader, int skippedAmount) => reader.MarkTitle("Bit Lengths", () => {
            byte[] bitCounts = new byte[Codes.Length];

            for(int index = skippedAmount, bitSpaceRemaining = LengthBitSpace; bitSpaceRemaining > 0 && index < Order.Length; index++){
                byte bitCount = reader.ReadValue(Lengths, "bit length for code " + Order[index]);

                if (bitCount != 0){
                    bitCounts[Order[index]] = bitCount;
                    bitSpaceRemaining -= LengthBitSpace >> bitCount;
                }
            }

            var lengthEntries = Codes.Zip(bitCounts, HuffmanGenerator<ComplexLengthCode>.MakeEntry).ToArray();
            var filteredLengthEntries = lengthEntries.Where(entry => entry.Bits > 0).ToArray();

            return HuffmanGenerator<ComplexLengthCode>.FromBitCountsCanonical(filteredLengthEntries.Length == 0 ? lengthEntries : filteredLengthEntries);
        });
    }
}
