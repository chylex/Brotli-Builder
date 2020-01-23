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
        public int OutputSize => intermediateState.OutputSize - initialState.OutputSize;

        public CategoryMap<BlockSwitchBuilder> BlockTypes { get; } = BlockTypeInfo.Empty.Select(info => new BlockSwitchBuilder(info));
        public DistanceParameters DistanceParameters { get; set; } = DistanceParameters.Zero;

        public LiteralContextMode[] LiteralContextModes { get; set; } = { LiteralContextMode.LSB6 };
        public ContextMap LiteralCtxMap { get; set; } = ContextMap.Literals.Simple;
        public ContextMap DistanceCtxMap { get; set; } = ContextMap.Distances.Simple;

        public IReadOnlyList<InsertCopyCommand> InsertCopyCommands => icCommands;

        private readonly List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
        
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
            
            this.BlockTypes = header.BlockTypes.Select(info => new BlockSwitchBuilder(info));
            this.DistanceParameters = header.DistanceParameters;
            this.LiteralContextModes = header.LiteralCtxModes.ToArray();
            this.LiteralCtxMap = header.LiteralCtxMap;
            this.DistanceCtxMap = header.DistanceCtxMap;

            foreach(InsertCopyCommand command in data.InsertCopyCommands){
                AddInsertCopy(command);
            }

            foreach(Category category in Categories.LID){
                var bsBuilder = BlockTypes[category];

                foreach(BlockSwitchCommand command in data.BlockSwitchCommands[category]){
                    bsBuilder.AddBlockSwitch(command);
                }
            }
        }

        // Commands

        public CompressedMetaBlockBuilder AddInsertCopy(InsertCopyCommand command){
            icCommands.Add(command);

            intermediateState.OutputLiterals(command.Literals);

            if (command.CopyDistance != DistanceInfo.EndsAfterLiterals){
                intermediateState.OutputCopy(command.CopyLength, command.CopyDistance);
            }

            return this;
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, DictionaryIndexEntry dictionaryEntry){
            return AddInsertCopy(new InsertCopyCommand(literals, dictionaryEntry.CopyLength, 1 + dictionaryEntry.Packed + Math.Min(intermediateState.Parameters.WindowSize.Bytes, intermediateState.OutputSize + literals.Count)));
        }

        // Building

        public (MetaBlock MetaBlock, BrotliEncodeInfo NextInfo) Build(BrotliEncodeInfo info){
            var (metaBlock, nextState) = Build(info.CompressionParameters);
            return (metaBlock, info.WithProcessedBytes(nextState, OutputSize));
        }

        public (MetaBlock MetaBlock, BrotliGlobalState NextState) Build(BrotliCompressionParameters parameters){
            var state = initialState.Clone();

            // Setup
            
            var bsCommands = BlockTypes.Select<IList<BlockSwitchCommand>>(builder => new List<BlockSwitchCommand>(builder.Commands));

            var blockTypeInfo = BlockTypes.Select(builder => builder.Build());
            var blockTrackers = blockTypeInfo.Select(info => new BlockSwitchTracker.Writing(info, new Queue<BlockSwitchCommand>(bsCommands[info.Category])));

            var literalFreq = NewFreqArray<Literal>(LiteralCtxMap.TreeCount);
            var icLengthCodeFreq = NewFreqArray<InsertCopyLengthCode>(blockTypeInfo[Category.InsertCopy].TypeCount);
            var distanceCodeFreq = NewFreqArray<DistanceCode>(DistanceCtxMap.TreeCount);
            
            // Command processing

            var icCommandCount = icCommands.Count;
            var icCommandsFinal = new InsertCopyCommand[icCommandCount];

            for(int icCommandIndex = 0; icCommandIndex < icCommandCount; icCommandIndex++){
                var icCommand = icCommands[icCommandIndex];
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
                    icLengthCode = icLengthValues.MakeCode(DistanceCodeZeroStrategy.PreferEnabled); // TODO good strategy?
                }
                else{
                    DistanceCode? distanceCode = null;
                    var distanceCodes = icCommand.CopyDistance.MakeCode(DistanceParameters, state);
                    
                    if (distanceCodes != null){
                        int blockID = blockTrackers[Category.Distance].SimulateCommand();
                        int contextID = icLengthValues.DistanceContextID;
                        int treeID = DistanceCtxMap.DetermineTreeID(blockID, contextID);

                        var codeList = distanceCodeFreq[treeID];
                        codeList.Add(distanceCode = distanceCodes.FirstOrDefault(codeList.Contains) ?? distanceCodes[0]); // new CompressedMetaBlockBuildParams().PickDistanceCode(icCommand.CopyDistance, distanceCodes, codeList));
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
                icCommandsFinal[icCommandIndex] = icCommand;
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
                blockTypeInfo,
                DistanceParameters,
                LiteralContextModes,
                LiteralCtxMap,
                DistanceCtxMap,
                ConstructHuffmanTrees(literalFreq),
                ConstructHuffmanTrees(icLengthCodeFreq),
                ConstructHuffmanTrees(distanceCodeFreq)
            );

            var data = new CompressedData(icCommandsFinal, bsCommands);
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

        private static HuffmanTree<T>[] ConstructHuffmanTrees<T>(FrequencyList<T>[] source) where T : IComparable<T>{
            return source.Select(list => HuffmanTree<T>.FromSymbols(list)).ToArray();
        }
    }
}
