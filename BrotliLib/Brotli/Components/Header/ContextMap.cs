using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;
using BrotliLib.Markers.Serialization;
using BrotliLib.Numbers;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Represents a context map, which maps &lt;blockID, contextID&gt; pairs to indices in an array of Huffman trees for either literals or distances.
    /// https://tools.ietf.org/html/rfc7932#section-7.3
    /// </summary>
    public sealed class ContextMap{
        public Category Category { get; }
        public int TreeCount { get; }

        public int ContextsPerBlockType { get; }
        public int BlockTypes => contextMap.Length / ContextsPerBlockType;

        public byte this[int index] => contextMap[index];

        private readonly byte[] contextMap;

        public ContextMap(Category category, int treeCount, byte[] contextMap){
            this.Category = category;
            this.TreeCount = treeCount;
            this.contextMap = CollectionHelper.Clone(contextMap);
            this.ContextsPerBlockType = category.Contexts();
        }

        public byte DetermineTreeID(int blockID, int contextID){
            return contextMap[blockID * ContextsPerBlockType + contextID];
        }

        // Object
        
        public override bool Equals(object obj){
            return obj is ContextMap map &&
                   Category == map.Category &&
                   TreeCount == map.TreeCount &&
                   CollectionHelper.Equal(contextMap, map.contextMap);
        }

        public override int GetHashCode(){
            return HashCode.Combine(Category, TreeCount, CollectionHelper.HashCode(contextMap));
        }

        public override string ToString(){
            return "TreeCount = " + TreeCount + ", Map = { " + string.Join(", ", contextMap) + " }";
        }
        
        // Helpers

        public struct Run{
            public const int MinSpecialCodeLength = 2;

            /// <summary>
            /// Returns the RLE code required to encode the specified <see cref="Length"/>. The codes follow the pattern:
            /// <list type="bullet">
            /// <item><description>code 1 ... values 2-3,</description></item>
            /// <item><description>code 2 ... values 4-7,</description></item>
            /// <item><description>code 3 ... values 8-15,</description></item>
            /// <item><description>code 4 ... values 16-31,</description></item>
            /// <item><description>(up to code 16)</description></item>
            /// </list>
            /// If <see cref="Length"/> is less than <see cref="MinSpecialCodeLength"/>, returns 0.
            /// </summary>
            public byte Code => Log2.Floor(Length);

            /// <summary>
            /// Amount of zero symbols in the run.
            /// </summary>
            public int Length { get; }

            public Run(int length){
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
            /// Uses RLE for a shorter run of length <paramref name="retainedLength"/>.
            /// If the remaining length is at least <see cref="MinSpecialCodeLength"/>, it will become a new run.
            /// If either side of the split cannot use RLE, it will be encoded as plain zeros instead.
            /// </summary>
            public int Retain(int retainedLength){
                if (retainedLength < 1){
                    throw new ArgumentOutOfRangeException(nameof(retainedLength));
                }

                return retainedLength; // if retainedLength > Length, will crash in RunDecider.Resolve
            }
        }

        /// <summary>
        /// Decides substitution of runs in (potentially move-to-front transformed) context map data.
        /// </summary>
        public sealed class RunDecider{
            public int DataLength => data.Length;

            private readonly byte[] data;

            public RunDecider(byte[] data){
                this.data = data;
            }

            /// <summary>
            /// Returns the byte at the specified <paramref name="index"/>.
            /// </summary>
            public byte GetByteAt(int index){
                return data[index];
            }

            /// <summary>
            /// Returns how many zeroes there are in a sequence starting at <paramref name="startIndex"/>.
            /// </summary>
            public int GetRunLength(int startIndex){
                for(int index = startIndex; index < data.Length + 1; index++){
                    if (index == data.Length || data[index] != 0){
                        return index - startIndex;
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }

            /// <summary>
            /// Applies a resolution function over all runs in the data.
            /// </summary>
            public RunResolution Resolve(Func<Run, int> resolver){
                var resolution = new RunResolution.Builder();

                for(int index = 0; index < data.Length;){
                    byte symbol = data[index];

                    if (symbol == 0){
                        int runLength = GetRunLength(index);
                        int retained = runLength < Run.MinSpecialCodeLength ? runLength : resolver(new Run(runLength));

                        if (retained > runLength){
                            throw new InvalidOperationException("Cannot request encoding a run longer than originally asked for (" + retained + " > " + runLength + ").");
                        }

                        if (retained < Run.MinSpecialCodeLength){
                            if (retained == 0){
                                retained = runLength;
                            }

                            for(int i = 0; i < retained; i++){
                                resolution.AddRaw(0);
                            }
                        }
                        else{
                            resolution.AddRun(retained);
                        }

                        index += retained;
                    }
                    else{
                        resolution.AddRaw(symbol);
                        index += 1;
                    }
                }

                return resolution.Build();
            }
        }

        public sealed class RunResolution{
            public int LongestRun { get; }
            public int RunLengthCodeCount => new Run(LongestRun).Code;

            public IEnumerable<int> RunLengths => intermediate.Where(code => code < 0).Select(code => -code);
            
            private readonly List<int> intermediate;

            private RunResolution(List<int> intermediate, int longestRun){
                this.intermediate = intermediate;
                this.LongestRun = longestRun;
            }

            public (List<int>, Queue<int>) GenerateSymbolsAndExtraBits(){
                var symbols = new List<int>();
                var extra = new Queue<int>();

                int runLengthCodeCount = RunLengthCodeCount;
                    
                foreach(int code in intermediate){
                    if (code == 0){
                        symbols.Add(0);
                    }
                    else if (code > 0){
                        symbols.Add(code + runLengthCodeCount);
                    }
                    else{
                        var run = new Run(-code);
                        int runLengthCode = run.Code;

                        symbols.Add(runLengthCode);
                        extra.Enqueue(run.Length - (1 << runLengthCode));
                    }
                }

                return (symbols, extra);
            }

            public sealed class Builder{
                private List<int>? intermediate = new List<int>();
                private int longestRun;

                public void AddRaw(byte value){
                    intermediate?.Add(value);
                }

                public void AddRun(int length){
                    intermediate?.Add(-length);

                    if (length > longestRun){
                        longestRun = length;
                    }
                }

                public RunResolution Build(){
                    if (intermediate == null){
                        throw new InvalidOperationException("The builder has already been built.");
                    }

                    var built = new RunResolution(intermediate, longestRun);
                    intermediate = null;
                    return built;
                }
            }
        }

        // Serialization

        private static HuffmanTree<int>.Context GetCodeTreeContext(int alphabetSize){
            return new HuffmanTree<int>.Context(new AlphabetSize(alphabetSize), bits => bits, symbol => symbol);
        }

        public static readonly BitDeserializer<ContextMap, BlockTypeInfo> Deserialize = MarkedBitDeserializer.Title<ContextMap, BlockTypeInfo>(
            context => "Context Map (" + context.Category + ")",

            (reader, context) => {
                int treeCount = reader.ReadValue(VariableLength11Code.Deserialize, NoContext.Value, "NTREES", value => value.Value);

                var category = context.Category;
                var contextMap = new byte[context.TypeCount * category.Contexts()];

                if (treeCount > 1){
                    int runLengthCodeCount = reader.MarkValue("RLEMAX", () => reader.NextBit() ? 1 + reader.NextChunk(4) : 0);
                    
                    var codeContext = GetCodeTreeContext(treeCount + runLengthCodeCount);
                    var codeLookup = reader.ReadStructure(HuffmanTree<int>.Deserialize, codeContext, "code tree").Root;
                    
                    for(int index = 0; index < contextMap.Length;){
                        reader.MarkStart();

                        int code = codeLookup.LookupValue(reader);

                        if (code > 0){
                            if (code <= runLengthCodeCount){
                                int skip = (1 << code) + reader.NextChunk(code);

                                reader.MarkEndTitle("repeat " + skip + " zeros");
                                index += skip;
                                continue;
                            }

                            contextMap[index] = (byte)(code - runLengthCodeCount);
                        }
                        
                        reader.MarkEndValue("CMAP" + category.Id() + "[" + index + "]", contextMap[index]);
                        index += 1;
                    }

                    if (reader.NextBit("IMTF")){
                        MoveToFront.Decode(contextMap);
                    }
                }

                return new ContextMap(category, treeCount, contextMap);
            }
        );

        public static BitSerializer<ContextMap, NoContext, BrotliSerializationParameters> Serialize = (writer, obj, context, parameters) => {
            VariableLength11Code.Serialize(writer, new VariableLength11Code(obj.TreeCount), NoContext.Value);

            if (obj.TreeCount > 1){
                bool mtf = parameters.ContextMapMTF(obj);
                byte[] contextMap;

                if (mtf){
                    contextMap = CollectionHelper.Clone(obj.contextMap);
                    MoveToFront.Encode(contextMap);
                }
                else{
                    contextMap = obj.contextMap;
                }

                var runs = parameters.ContextMapRLE(new RunDecider(contextMap));
                int runLengthCodeCount = runs.RunLengthCodeCount;

                if (runLengthCodeCount > 0){
                    writer.WriteBit(true);
                    writer.WriteChunk(4, runLengthCodeCount - 1);
                }
                else{
                    writer.WriteBit(false);
                }

                var (symbols, extra) = runs.GenerateSymbolsAndExtraBits();

                var codeContext = GetCodeTreeContext(obj.TreeCount + runLengthCodeCount);
                var codeTree = parameters.GenerateContextMapTree(FrequencyList<int>.FromSequence(symbols));

                HuffmanTree<int>.Serialize(writer, codeTree, codeContext, parameters);

                foreach(int symbol in symbols){
                    writer.WriteBits(codeTree.FindPath(symbol));

                    if (symbol > 0 && symbol <= runLengthCodeCount){
                        writer.WriteChunk(symbol, extra.Dequeue());
                    }
                }

                writer.WriteBit(mtf);
            }
        };
    }
}
