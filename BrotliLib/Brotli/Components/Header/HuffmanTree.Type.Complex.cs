using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Collections.Huffman;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

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

                    var defaultRepeatCode = new HuffmanGenerator<T>.Entry(getSymbol(0), HuffmanTreeLengthCode.InitialRepeatedCode);
                    var lengthCodes = HuffmanTreeLengthCode.Read(reader, context.SkippedComplexCodeLengths);
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
                        
                        if (nextCode == HuffmanTreeLengthCode.Skip){
                            reader.MarkStart();

                            int NextSkipData() => 3 + reader.NextChunk(3, "skip data");
                            int skipCount = NextSkipData();

                            while((nextForcedCode = reader.ReadValue(lengthCodes, "code", code => code.Code)) == HuffmanTreeLengthCode.Skip){
                                skipCount = 8 * (skipCount - 2) + NextSkipData();
                            }

                            reader.MarkEndValue("skip count", skipCount);

                            symbolIndex += skipCount;
                        }
                        else if (nextCode == HuffmanTreeLengthCode.Repeat){
                            byte repeatedCode = symbolEntries.DefaultIfEmpty(defaultRepeatCode).Select(entry => entry.Bits).Last(value => value > 0);
                            int sumPerRepeat = SymbolBitSpace >> repeatedCode;
                            
                            reader.MarkStart();
                            
                            int NextRepeatData() => 3 + reader.NextChunk(2, "repeat data");
                            int repeatCount = NextRepeatData();

                            while(bitSpaceRemaining - sumPerRepeat * repeatCount > 0 && (nextForcedCode = reader.ReadValue(lengthCodes, "code", code => code.Code)) == HuffmanTreeLengthCode.Repeat){
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
                        else if (nextCode <= HuffmanTreeLengthCode.MaxLength){
                            AddMarkedSymbol(new HuffmanGenerator<T>.Entry(getSymbol(symbolIndex++), nextCode));
                            bitSpaceRemaining -= SymbolBitSpace >> nextCode;
                        }
                    }

                    reader.MarkEndTitle("Symbols");

                    return new HuffmanTree<T>(HuffmanGenerator<T>.FromBitCountsCanonical(symbolEntries));
                }
            );

            public static readonly BitSerializer<HuffmanTree<T>, Context, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
                Func<int, T> getSymbol = context.BitsToSymbol;
                
                int symbolCount = context.AlphabetSize.SymbolCount;
                var symbolLengths = new List<byte>();
                
                for(int symbolIndex = 0, bitSpaceRemaining = SymbolBitSpace; bitSpaceRemaining > 0 && symbolIndex < symbolCount; symbolIndex++){
                    BitPath? path = obj.FindPathOrNull(getSymbol(symbolIndex));
                    byte length = path?.Length ?? 0;
                    
                    if (length > 0){
                        bitSpaceRemaining -= SymbolBitSpace >> length;
                    }
                    else if (symbolIndex == symbolCount - 1){
                        length = 15; // if the tree is incomplete, a zero length last symbol would generate an ending length code 0 or 17, which Brotli spec forbids
                                     // if the tree is complete and the final symbol was supposed to be a 0, the writer will run out of bit space before it writes the final symbol
                    }

                    symbolLengths.Add(length);
                }

                var runDecider = new HuffmanTreeLengthCode.RunDecider(symbolLengths, context.AlphabetSize);
                var (lengthCodes, extra) = parameters.HuffmanTreeRLE(runDecider).GenerateCodesAndExtraBits();
                
                var lengthEntries = lengthCodes.GroupBy(length => length).Select(group => new HuffmanGenerator<byte>.SymbolFreq(group.Key, group.Count())).ToArray();
                var lengthMap = HuffmanGenerator<byte>.FromFrequenciesCanonical(lengthEntries, HuffmanTreeLengthCode.LengthMaxDepth).GenerateValueMapOptimized();
                
                HuffmanTreeLengthCode.Write(writer, lengthMap);
                
                foreach(byte code in lengthCodes){
                    writer.WriteBits(lengthMap[code]);

                    if (code == HuffmanTreeLengthCode.Skip){
                        writer.WriteChunk(HuffmanTreeLengthCode.SkipCodeExtraBits, extra.Dequeue());
                    }
                    else if (code == HuffmanTreeLengthCode.Repeat){
                        writer.WriteChunk(HuffmanTreeLengthCode.RepeatCodeExtraBits, extra.Dequeue());
                    }
                }
            };
        }
    }
}
