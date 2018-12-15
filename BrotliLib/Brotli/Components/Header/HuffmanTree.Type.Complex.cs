using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Huffman;
using BrotliLib.IO;
using ComplexLengthNode = BrotliLib.Huffman.HuffmanNode<byte>;

namespace BrotliLib.Brotli.Components.Header{
    partial class HuffmanTree<T>{
        private const int SymbolBitSpace = 1 << 15;

        /// <summary>
        /// https://tools.ietf.org/html/rfc7932#section-3.5
        /// </summary>
        private static readonly IBitSerializer<HuffmanTree<T>, Context> ComplexCodeSerializer = new BitSerializer<HuffmanTree<T>, Context>(
            fromBits: (reader, context) => {
                Func<int, T> getSymbol = context.BitsToSymbol;

                var defaultRepeatCode = new HuffmanGenerator<T>.Entry(getSymbol(0), ComplexLengthCode.DefaultRepeatCode);
                var lengthCodes = ComplexLengthCode.Read(reader, context.SkippedComplexCodeLengths);
                byte nextForcedCode = byte.MaxValue;

                int symbolIndex = 0;
                int symbolCount = context.AlphabetSize.SymbolCount;
                var symbolEntries = new List<HuffmanGenerator<T>.Entry>(symbolCount);

                int bitSpaceRemaining = SymbolBitSpace;

                while(bitSpaceRemaining > 0 && symbolIndex < symbolCount){
                    byte nextCode;

                    if (nextForcedCode == byte.MaxValue){
                        nextCode = lengthCodes.LookupValue(reader).Code;
                    }
                    else{
                        nextCode = nextForcedCode;
                        nextForcedCode = byte.MaxValue;
                    }

                    if (nextCode == ComplexLengthCode.Skip){
                        int skipCount = 3 + reader.NextChunk(3);

                        while((nextForcedCode = lengthCodes.LookupValue(reader).Code) == ComplexLengthCode.Skip){
                            skipCount = 8 * (skipCount - 2) + 3 + reader.NextChunk(3);
                        }

                        symbolIndex += skipCount;
                    }
                    else if (nextCode == ComplexLengthCode.Repeat){
                        byte repeatedCode = symbolEntries.DefaultIfEmpty(defaultRepeatCode).Select(entry => entry.Bits).Last(value => value > 0);
                        int sumPerRepeat = SymbolBitSpace >> repeatedCode;

                        int repeatCount = 3 + reader.NextChunk(2);

                        while(bitSpaceRemaining - sumPerRepeat * repeatCount > 0 && (nextForcedCode = lengthCodes.LookupValue(reader).Code) == ComplexLengthCode.Repeat){
                            repeatCount = 4 * (repeatCount - 2) + 3 + reader.NextChunk(2);
                        }

                        bitSpaceRemaining -= sumPerRepeat * repeatCount;
                    
                        while(--repeatCount >= 0){
                            symbolEntries.Add(new HuffmanGenerator<T>.Entry(getSymbol(symbolIndex++), repeatedCode));
                        }
                    }
                    else if (nextCode == 0){
                        ++symbolIndex;
                    }
                    else if (nextCode <= ComplexLengthCode.MaxLength){
                        symbolEntries.Add(new HuffmanGenerator<T>.Entry(getSymbol(symbolIndex++), nextCode));
                        bitSpaceRemaining -= SymbolBitSpace >> nextCode;
                    }
                }

                return new HuffmanTree<T>(HuffmanGenerator<T>.FromBitCountsCanonical(symbolEntries));
            },

            toBits: (writer, obj, context) => {
                Func<int, T> getSymbol = context.BitsToSymbol;
                
                int symbolCount = context.AlphabetSize.SymbolCount;
                var symbolEntries = new List<HuffmanGenerator<T>.Entry>();

                for(int symbolIndex = 0, bitSpaceRemaining = SymbolBitSpace; bitSpaceRemaining > 0 && symbolIndex < symbolCount; symbolIndex++){
                    T symbol = getSymbol(symbolIndex);
                    BitStream path = obj.FindPathOrNull(symbol);

                    byte length = (byte)(path?.Length ?? 0);
                    symbolEntries.Add(new HuffmanGenerator<T>.Entry(symbol, length));
                    
                    if (length > 0){
                        bitSpaceRemaining -= SymbolBitSpace >> length;
                    }
                }
                
                var lengthEntries = symbolEntries.GroupBy(kvp => kvp.Bits).Select(HuffmanGenerator<byte>.MakeFreq).ToArray();
                var lengthMap = HuffmanGenerator<byte>.FromFrequenciesCanonical(lengthEntries, ComplexLengthCode.LengthMaxDepth).GenerateValueMap();
                
                ComplexLengthCode.Write(writer, lengthMap);
                
                foreach(var entry in symbolEntries){
                    writer.WriteBits(lengthMap[entry.Bits]);
                }
            }
        );
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

        public override int GetHashCode(){
            return Code;
        }

        public override bool Equals(object obj){
            return obj is ComplexLengthCode other && Code == other.Code;
        }

        public override string ToString(){
            return "{ Code = " + Code + " }";
        }

        // Serialization

        public static void Write(BitWriter writer, IDictionary<byte, BitStream> lengthMap){
            int skippedAmount;

            if (!lengthMap.ContainsKey(Order[0]) && !lengthMap.ContainsKey(Order[1])){
                skippedAmount = lengthMap.ContainsKey(Order[2]) ? 2 : 3;
            }
            else{
                skippedAmount = 0;
            }

            writer.WriteChunk(2, skippedAmount);
            
            for(int index = skippedAmount, bitSpaceRemaining = LengthBitSpace; bitSpaceRemaining > 0 && index < Order.Length; index++){ // TODO use repeat/skip codes
                byte code = Order[index];
                byte length = (byte)(lengthMap.TryGetValue(code, out BitStream stream) ? stream.Length : 0);

                writer.WriteBits(LengthLookup[length]);

                if (length > 0){
                    bitSpaceRemaining -= LengthBitSpace >> length;
                }
            }
        }

        public static HuffmanNode<ComplexLengthCode> Read(BitReader reader, int skippedAmount){
            byte[] bitCounts = new byte[Codes.Length];

            for(int index = skippedAmount, bitSpaceRemaining = LengthBitSpace; bitSpaceRemaining > 0 && index < Order.Length; index++){
                byte bitCount = Lengths.LookupValue(reader);

                if (bitCount != 0){
                    bitCounts[Order[index]] = bitCount;
                    bitSpaceRemaining -= LengthBitSpace >> bitCount;
                }
            }

            var lengthEntries = Codes.Zip(bitCounts, HuffmanGenerator<ComplexLengthCode>.MakeEntry).ToArray();
            var filteredLengthEntries = lengthEntries.Where(entry => entry.Bits > 0).ToArray();

            return HuffmanGenerator<ComplexLengthCode>.FromBitCountsCanonical(filteredLengthEntries.Length == 0 ? lengthEntries : filteredLengthEntries);
        }
    }
}
