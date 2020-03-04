﻿using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Encode.Build{
    public sealed class CompressedMetaBlockBuilder{
        public CategoryMap<BlockSwitchBuilder> BlockTypes { get; } = BlockTypeInfo.Empty.Select(info => new BlockSwitchBuilder(info));
        public DistanceParameters DistanceParameters { get; set; } = DistanceParameters.Zero;

        public LiteralContextMode[] LiteralContextModes { get; set; } = { LiteralContextMode.LSB6 };
        public ContextMap LiteralCtxMap { get; set; } = ContextMapBuilder.Literals.Simple;
        public ContextMap DistanceCtxMap { get; set; } = ContextMapBuilder.Distances.Simple;

        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands => icCommands;

        public int OutputSize => intermediateState.OutputSize - initialState.OutputSize;
        public int LastDistance => intermediateState.DistanceBuffer.Front;

        // Fields

        private readonly List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
        private int totalLiterals;
        private int totalExplicitDistances;
        private DistanceCodeZeroStrategy? finalInsertDistanceCodeZeroStrategy;
        
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
                AddInsertCopy(command);
            }
        }

        // Data

        public int GetTotalBlockLength(Category category){
            return category switch{
                Category.Literal    => totalLiterals,
                Category.InsertCopy => icCommands.Count,
                Category.Distance   => totalExplicitDistances,
                _ => throw new InvalidOperationException("Invalid category: " + category)
            };
        }

        // Commands

        private CompressedMetaBlockBuilder AddInsertCopy(InsertCopyCommand command){
            icCommands.Add(command);

            var literals = command.Literals;
            var distance = command.CopyDistance;

            intermediateState.OutputLiterals(literals);

            if (distance != DistanceInfo.EndsAfterLiterals){
                intermediateState.OutputCopy(command.CopyLength, distance);

                if (distance != DistanceInfo.ImplicitCodeZero){
                    totalExplicitDistances++;
                }
            }

            totalLiterals += literals.Count;
            return this;
        }

        public CompressedMetaBlockBuilder AddInsertFinal(IList<Literal> literals, DistanceCodeZeroStrategy dczStrategy = DistanceCodeZeroStrategy.PreferEnabled){
            finalInsertDistanceCodeZeroStrategy = dczStrategy;
            return AddInsertCopy(new InsertCopyCommand(literals));
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, int copyLength, DistanceInfo copyDistance){
            return AddInsertCopy(new InsertCopyCommand(literals, copyLength, copyDistance));
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, int copyLength, int copyDistance, DistanceCodeZeroStrategy dczStrategy = DistanceCodeZeroStrategy.PreferEnabled){
            if (copyDistance == LastDistance){
                switch(dczStrategy){
                    case DistanceCodeZeroStrategy.ForceEnabled:
                        return AddInsertCopy(literals, copyLength, DistanceInfo.ImplicitCodeZero);

                    case DistanceCodeZeroStrategy.PreferEnabled:
                        return AddInsertCopy(literals, copyLength, InsertCopyLengths.CanUseImplicitDCZ(literals.Count, copyLength) ? DistanceInfo.ImplicitCodeZero : DistanceInfo.ExplicitCodeZero);

                    case DistanceCodeZeroStrategy.Disable:
                        // Returns the copy distance verbatim, however note that this still allows the
                        // distance picker to pick an explicit code zero during the building process.
                        break;
                }
            }

            return AddInsertCopy(new InsertCopyCommand(literals, copyLength, copyDistance));
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, DictionaryIndexEntry dictionaryEntry){
            var startDistance = 1 + Math.Min(intermediateState.Parameters.WindowSize.Bytes, intermediateState.OutputSize + literals.Count);
            var entryDistance = dictionaryEntry.Packed + startDistance;

            return AddInsertCopy(literals, dictionaryEntry.CopyLength, entryDistance, DistanceCodeZeroStrategy.Disable);
        }

        public CompressedMetaBlockBuilder AddCopy(int copyLength, int copyDistance, DistanceCodeZeroStrategy dczStrategy = DistanceCodeZeroStrategy.PreferEnabled){
            return AddInsertCopy(Array.Empty<Literal>(), copyLength, copyDistance, dczStrategy);
        }
        
        public CompressedMetaBlockBuilder AddCopy(int copyLength, DistanceInfo copyDistance){
            return AddInsertCopy(Array.Empty<Literal>(), copyLength, copyDistance);
        }

        public CompressedMetaBlockBuilder AddCopy(DictionaryIndexEntry dictionaryEntry){
            return AddInsertCopy(Array.Empty<Literal>(), dictionaryEntry);
        }

        // Building

        public (MetaBlock MetaBlock, BrotliEncodeInfo NextInfo) Build(BrotliEncodeInfo info, BrotliCompressionParameters? parameters = null){
            var (metaBlock, nextState) = Build(parameters ?? info.CompressionParameters);
            return (metaBlock, info.WithProcessedBytes(nextState, OutputSize));
        }

        public (MetaBlock MetaBlock, BrotliGlobalState NextState) Build(BrotliCompressionParameters parameters){
            var state = initialState.Clone();

            // Setup
            
            var blockTypes = BlockTypes.Select(builder => builder.Build(GetTotalBlockLength(builder.Category), parameters));
            var blockTrackers = blockTypes.Select(built => new BlockSwitchTracker.Writing(built.Info, new Queue<BlockSwitchCommand>(built.Commands)));

            var literalFreq = NewFreqArray<Literal>(LiteralCtxMap.TreeCount);
            var icLengthCodeFreq = NewFreqArray<InsertCopyLengthCode>(blockTypes[Category.InsertCopy].Info.TypeCount);
            var distanceCodeFreq = NewFreqArray<DistanceCode>(DistanceCtxMap.TreeCount);

            var icCommandCount = icCommands.Count;
            var icCommandsFinal = new InsertCopyCommand[icCommandCount];

            // Early validation

            if (icCommands.Count == 0){
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
                var icCommand = icCommands[icIndex];
                int icBlockID = blockTrackers[Category.InsertCopy].SimulateCommand();

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

                    icLengthCode = icLengthValues.MakeCode(finalInsertDistanceCodeZeroStrategy ?? throw new InvalidOperationException());
                }
                else{
                    DistanceCode? distanceCode = null;
                    var distanceCodes = icCommand.CopyDistance.MakeCode(DistanceParameters, state);
                    
                    if (distanceCodes != null){
                        int blockID = blockTrackers[Category.Distance].SimulateCommand();
                        int contextID = icLengthValues.DistanceContextID;
                        int treeID = DistanceCtxMap.DetermineTreeID(blockID, contextID);

                        var codeList = distanceCodeFreq[treeID];
                        codeList.Add(distanceCode = parameters.DistanceCodePicker(distanceCodes, codeList));
                    }

                    bool isImplicitCodeZero = distanceCode == null;
                    bool isDistanceCodeZero = isImplicitCodeZero || distanceCode!.Equals(DistanceCode.Zero);

                    icLengthCode = icLengthValues.MakeCode(isImplicitCodeZero ? DistanceCodeZeroStrategy.ForceEnabled : DistanceCodeZeroStrategy.Disable);
                    // TODO not allowed to use implicit code unless defined in the command, as implicit code doesn't advance the distance block tracker and would require lengths to be recalculated

                    if (icLengthCode.UseDistanceCodeZero){
                        icCommand = icCommand.WithDistance(DistanceInfo.ImplicitCodeZero);
                    }
                    else if (isDistanceCodeZero){
                        icCommand = icCommand.WithDistance(DistanceInfo.ExplicitCodeZero);
                    }

                    state.OutputCopy(icCommand.CopyLength, icCommand.CopyDistance);
                }

                icLengthCodeFreq[icBlockID].Add(icLengthCode);
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

            return (new MetaBlock.Compressed(isLast: false, dataLength, header, data), state);
        }

        // Helpers

        private static FrequencyList<T>[] NewFreqArray<T>(int arraySize) where T : IComparable<T>{
            FrequencyList<T>[] array = new FrequencyList<T>[arraySize];

            for(int index = 0; index < arraySize; index++){
                array[index] = new FrequencyList<T>();
            }

            return array;
        }

        private static HuffmanTree<T>[] ConstructHuffmanTrees<T>(FrequencyList<T>[] source, BrotliCompressionParameters.GenerateHuffmanTree<T> generator) where T : IComparable<T>{
            HuffmanTree<T>[] array = new HuffmanTree<T>[source.Length];

            for(int index = 0; index < array.Length; index++){
                array[index] = generator(source[index]);
            }

            return array;
        }
    }
}
