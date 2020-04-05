using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Parameters.Heuristics;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;
using static BrotliLib.Brotli.Utils.DistanceCodeZeroStrategy;

namespace BrotliLib.Brotli.Encode.Build{
    public sealed class CompressedMetaBlockBuilder{
        public CategoryMap<BlockSwitchBuilder> BlockTypes { get; } = BlockTypeInfo.Empty.Select(info => new BlockSwitchBuilder(info));
        public DistanceParameters DistanceParameters { get; set; } = DistanceParameters.Zero;

        public LiteralContextMode[] LiteralContextModes { get; set; } = { LiteralContextMode.LSB6 };
        public ContextMap LiteralCtxMap { get; set; } = ContextMapBuilder.Literals.Simple;
        public ContextMap DistanceCtxMap { get; set; } = ContextMapBuilder.Distances.Simple;

        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands => commands;

        public int OutputSize => intermediateState.OutputSize - initialState.OutputSize;
        public int LastDistance => intermediateState.DistanceBuffer.Front;

        private InsertCopyCommand? FinalInsertCommand{
            get{
                int count = commands.Count;

                if (count == 0){
                    return null;
                }

                var lastCommand = commands[count - 1];
                return lastCommand.CopyDistance == DistanceInfo.EndsAfterLiterals ? lastCommand : null;
            }
        }

        // Fields

        private readonly List<InsertCopyCommand> commands = new List<InsertCopyCommand>();
        private int totalLiterals;
        private int totalExplicitDistances;
        
        private readonly BrotliGlobalState initialState;
        private readonly BrotliGlobalState intermediateState;

        // Construction

        public CompressedMetaBlockBuilder(BrotliGlobalState state){
            this.initialState = state.Clone();
            this.intermediateState = state.Clone();
        }

        public CompressedMetaBlockBuilder(BrotliFileParameters parameters) : this(new BrotliGlobalState(parameters)){}

        public CompressedMetaBlockBuilder(MetaBlock.Compressed metaBlock, BrotliGlobalState state) : this(state){
            var header = metaBlock.Header;
            var data = metaBlock.Data;
            
            this.BlockTypes = header.BlockTypes.Select(info => new BlockSwitchBuilder(info, data.BlockSwitchCommands[info.Category]));
            this.DistanceParameters = header.DistanceParameters;
            this.LiteralContextModes = header.LiteralCtxModes.ToArray();
            this.LiteralCtxMap = header.LiteralCtxMap;
            this.DistanceCtxMap = header.DistanceCtxMap;

            foreach(InsertCopyCommand command in data.InsertCopyCommands){
                AddInsertCopyCommand(command);
            }
        }

        // General

        public int GetTotalBlockLength(Category category){
            return category switch{
                Category.Literal    => totalLiterals,
                Category.InsertCopy => commands.Count,
                Category.Distance   => totalExplicitDistances,
                _ => throw new InvalidOperationException("Invalid category: " + category)
            };
        }

        public CompressedMetaBlockBuilder UseSameLiteralContextMode(LiteralContextMode mode){
            LiteralContextModes = Enumerable.Repeat(mode, BlockTypes[Category.Literal].TypeCount).ToArray();
            return this;
        }

        // Commands

        public CompressedMetaBlockBuilder AddInsertCopyCommand(InsertCopyCommand command){
            var literals = command.Literals;
            var distance = command.CopyDistance;

            var finalInsertCommand = FinalInsertCommand;

            if (finalInsertCommand != null){
                commands[^1] = new InsertCopyCommand(finalInsertCommand.Literals.Concat(literals).ToList(), command.CopyLength, distance); // silently merge with previous command
            }
            else{
                commands.Add(command);
            }

            intermediateState.OutputLiterals(literals);

            if (distance != DistanceInfo.EndsAfterLiterals){
                intermediateState.OutputCopy(command.CopyLength, distance);
            }

            totalLiterals += literals.Count;

            if (distance.HasExplicitDistanceCode()){
                totalExplicitDistances++;
            }

            return this;
        }

        public CompressedMetaBlockBuilder AddInsert(IList<Literal> literals){
            return AddInsertCopyCommand(new InsertCopyCommand(literals));
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, int copyLength, DistanceInfo copyDistance){
            return AddInsertCopyCommand(new InsertCopyCommand(literals, copyLength, copyDistance));
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, int copyLength, int copyDistance, DistanceCodeZeroStrategy dczStrategy = PreferImplicit){
            if (copyDistance == LastDistance){
                var literalsToMerge = FinalInsertCommand?.Literals.Count ?? 0;
                return AddInsertCopyCommand(new InsertCopyCommand(literals, copyLength, dczStrategy.Decide(literals.Count + literalsToMerge, copyLength, copyDistance)));
            }
            else{
                return AddInsertCopyCommand(new InsertCopyCommand(literals, copyLength, copyDistance));
            }
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, DictionaryIndexEntry dictionaryEntry, DistanceCodeZeroStrategy dczStrategy = PreferImplicit){
            var startDistance = 1 + Math.Min(intermediateState.Parameters.WindowSize.Bytes, intermediateState.OutputSize + literals.Count);
            var entryDistance = dictionaryEntry.Packed + startDistance;

            return AddInsertCopy(literals, dictionaryEntry.CopyLength, entryDistance, dczStrategy);
        }
        
        public CompressedMetaBlockBuilder AddCopy(int copyLength, DistanceInfo copyDistance){
            return AddInsertCopy(Array.Empty<Literal>(), copyLength, copyDistance);
        }

        public CompressedMetaBlockBuilder AddCopy(int copyLength, int copyDistance, DistanceCodeZeroStrategy dczStrategy = PreferImplicit){
            return AddInsertCopy(Array.Empty<Literal>(), copyLength, copyDistance, dczStrategy);
        }

        public CompressedMetaBlockBuilder AddCopy(DictionaryIndexEntry dictionaryEntry, DistanceCodeZeroStrategy dczStrategy = PreferImplicit){
            return AddInsertCopy(Array.Empty<Literal>(), dictionaryEntry, dczStrategy);
        }

        // Building

        public (MetaBlock.Compressed MetaBlock, BrotliEncodeInfo NextInfo) Build(BrotliEncodeInfo info, BrotliCompressionParameters? parameters = null){
            var (metaBlock, nextState) = Build(parameters ?? info.CompressionParameters);
            return (metaBlock, info.WithProcessedBytes(nextState, OutputSize));
        }

        public (MetaBlock.Compressed MetaBlock, BrotliGlobalState NextState) Build(BrotliCompressionParameters parameters){
            var state = initialState.Clone();

            // Setup
            
            var blockTypes = BlockTypes.Select(builder => builder.Build(GetTotalBlockLength(builder.Category), parameters));
            var blockTrackers = blockTypes.Select(built => new BlockSwitchTracker.Writing(built.Info, new Queue<BlockSwitchCommand>(built.Commands)));

            var literalFreq = FrequencyList<Literal>.Array(LiteralCtxMap.TreeCount);
            var icLengthCodeFreq = FrequencyList<InsertCopyLengthCode>.Array(blockTypes[Category.InsertCopy].Info.TypeCount);
            var distanceCodeFreq = FrequencyList<DistanceCode>.Array(DistanceCtxMap.TreeCount);

            var icCommandCount = commands.Count;
            var icCommandsFinal = new InsertCopyCommand[icCommandCount];

            // Early validation

            if (icCommandCount == 0){
                throw new InvalidOperationException("Cannot build a compressed meta-block with no insert&copy commands.");
            }

            if (LiteralContextModes.Length != blockTypes[Category.Literal].Info.TypeCount){
                throw new InvalidOperationException("Literal context mode array size must match the amount of literal block types (" + LiteralContextModes.Length + " != " + blockTypes[Category.Literal].Info.TypeCount + ").");
            }

            if (LiteralCtxMap.BlockTypes != blockTypes[Category.Literal].Info.TypeCount){
                throw new InvalidOperationException("Literal context map size is incorrect for the amount of literal block types.");
            }
            
            if (DistanceCtxMap.BlockTypes != blockTypes[Category.Distance].Info.TypeCount){
                throw new InvalidOperationException("Distance context map size is incorrect for the amount of distance block types.");
            }
            
            // Command processing

            for(int icIndex = 0; icIndex < icCommandCount; icIndex++){
                var icCommand = commands[icIndex];
                int icBlockID = blockTrackers[Category.InsertCopy].SimulateCommand();
                var icFreq = icLengthCodeFreq[icBlockID];

                for(int literalIndex = 0; literalIndex < icCommand.Literals.Count; literalIndex++){
                    var literal = icCommand.Literals[literalIndex];

                    int blockID = blockTrackers[Category.Literal].SimulateCommand();
                    int contextID = state.NextLiteralContextID(LiteralContextModes[blockID]);
                    int treeID = LiteralCtxMap.DetermineTreeID(blockID, contextID);

                    literalFreq[treeID].Add(literal);
                    state.OutputLiteral(literal);
                }
                
                InsertCopyLengths icLengthValues = icCommand.Lengths;
                InsertCopyLengthCode icLengthCode;

                if (icCommand.CopyDistance == DistanceInfo.EndsAfterLiterals){
                    if (icIndex != icCommandCount - 1){
                        throw new InvalidOperationException("Insert&copy command that ends after literals must be the last.");
                    }

                    icLengthCode = icLengthValues.MakeCode(ImplicitDistanceCodeZero.PreferEnabled);

                    if (icLengthCode.UseDistanceCodeZero){
                        var alternativeCode = icLengthValues.MakeCode(ImplicitDistanceCodeZero.Disable);

                        if (icFreq[alternativeCode] > icFreq[icLengthCode]){
                            icLengthCode = alternativeCode; // if the first code uses DCZ, try a non-DCZ code and pick whichever one was used more often
                        }
                    }
                }
                else{
                    var distanceCodes = icCommand.CopyDistance.MakeCode(DistanceParameters, state);
                    
                    if (distanceCodes == null){
                        icLengthCode = icLengthValues.MakeCode(ImplicitDistanceCodeZero.ForceEnabled);
                    }
                    else{
                        int blockID = blockTrackers[Category.Distance].SimulateCommand();
                        int contextID = icLengthValues.DistanceContextID;
                        int treeID = DistanceCtxMap.DetermineTreeID(blockID, contextID);

                        var distanceFreq = distanceCodeFreq[treeID];
                        DistanceCode distanceCode;

                        if (icCommand.CopyDistance == DistanceInfo.ExplicitCodeZero){
                            distanceCode = DistanceCode.Zero;
                        }
                        else{
                            distanceCode = distanceCodes.Count > 1 ? parameters.DistanceCodePicker(distanceCodes, distanceFreq) : distanceCodes[0];

                            if (distanceCode.Equals(DistanceCode.Zero)){
                                throw new InvalidOperationException("Cannot pick distance code zero for an insert&copy command that does not explicitly request it.");
                            }
                        }

                        distanceFreq.Add(distanceCode);
                        icLengthCode = icLengthValues.MakeCode(ImplicitDistanceCodeZero.Disable);
                    }

                    state.OutputCopy(icCommand.CopyLength, icCommand.CopyDistance);
                }

                icFreq.Add(icLengthCode);
                icCommandsFinal[icIndex] = icCommand;
            }

            // Finalize

            foreach(var literalList in literalFreq){
                if (literalList.Count == 0){
                    literalList.Add(new Literal(0));
                }
            }

            foreach(var distanceCodeList in distanceCodeFreq){
                if (distanceCodeList.Count == 0){
                    distanceCodeList.Add(DistanceCode.Zero);
                }
            }

            var header = new CompressedHeader(
                blockTypes.Select(built => built.Info),
                DistanceParameters,
                LiteralContextModes,
                LiteralCtxMap,
                DistanceCtxMap,
                ConstructHuffmanTrees(literalFreq, parameters.GenerateLiteralTree),
                ConstructHuffmanTrees(icLengthCodeFreq, parameters.GenerateLengthCodeTree),
                ConstructHuffmanTrees(distanceCodeFreq, parameters.GenerateDistanceCodeTree)
            );

            var data = new CompressedData(icCommandsFinal, blockTypes.Select(built => built.Commands));
            var dataLength = new DataLength(OutputSize);

            return (new MetaBlock.Compressed(dataLength, header, data), state);
        }

        // Helpers

        private static HuffmanTree<T>[] ConstructHuffmanTrees<T>(FrequencyList<T>[] source, HuffmanTreeHeuristics.Generate<T> generator) where T : IComparable<T>{
            HuffmanTree<T>[] array = new HuffmanTree<T>[source.Length];

            for(int index = 0; index < array.Length; index++){
                array[index] = generator(source[index]);
            }

            return array;
        }
    }
}
