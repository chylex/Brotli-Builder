using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.State;
using BrotliLib.Brotli.State.Output;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Encode{
    public sealed class CompressedMetaBlockBuilder{
        public CategoryMap<BlockTypeInfo> BlockTypes { get; set; } = BlockTypeInfo.Empty;
        public DistanceParameters DistanceParameters { get; set; } = DistanceParameters.NoDirectCodes;

        public LiteralContextMode[] LiteralContextModes { get; set; } = { LiteralContextMode.LSB6 };
        public ContextMap LiteralCtxMap { get; set; } = ContextMap.Literals.Simple;
        public ContextMap DistanceCtxMap { get; set; } = ContextMap.Distances.Simple;

        public BrotliGlobalState State { get; } // TODO private?

        private readonly WindowSize windowSize;
        private readonly List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
        private readonly CategoryMap<List<BlockSwitchCommand>> bsCommands = new CategoryMap<List<BlockSwitchCommand>>(_ => new List<BlockSwitchCommand>());
        
        public CompressedMetaBlockBuilder(WindowSize windowSize, BrotliDictionary dictionary){
            this.windowSize = windowSize;
            this.State = new BrotliGlobalState(dictionary, windowSize, new BrotliOutputStored()); // TODO support multiple meta-blocks
        }

        public CompressedMetaBlockBuilder(WindowSize windowSize) : this(windowSize, BrotliDefaultDictionary.Embedded){}

        // Commands

        public CompressedMetaBlockBuilder AddInsertCopy(InsertCopyCommand command){
            icCommands.Add(command);

            foreach(Literal literal in command.Literals){
                State.OutputLiteral(literal);
            }

            if (command.CopyDistance != DistanceInfo.EndsAfterLiterals){
                State.OutputCopy(command.CopyLength, command.CopyDistance);
            }

            return this;
        }

        public CompressedMetaBlockBuilder AddBlockSwitch(Category category, BlockSwitchCommand command){
            bsCommands[category].Add(command);
            return this;
        }

        // Building

        public MetaBlock Build(bool isLast){
            var state = new BrotliGlobalState(BrotliDefaultDictionary.Embedded, windowSize, new BrotliOutputStored()); // TODO support multiple meta-blocks

            ///// TODO reorganize

            var blockTrackers = BlockTypes.Select((_, info) => new BlockTypeTracker(info));
            var blockSwitchQueues = bsCommands.Select((_, list) => new Queue<BlockSwitchCommand>(list));
            var nullWriter = new BitStream().GetWriter();

            int NextBlockID(Category category){
                BlockTypeTracker tracker = blockTrackers[category];
                tracker.WriteCommand(nullWriter, blockSwitchQueues[category]);
                return tracker.CurrentID;
            }

            /////
            
            var literalLists = NewListArray<Literal>(LiteralCtxMap.TreeCount);
            var icLengthCodeLists = NewListArray<InsertCopyLengthCode>(BlockTypes[Category.InsertCopy].Count);
            var distanceCodeLists = NewListArray<DistanceCode>(DistanceCtxMap.TreeCount);

            foreach(InsertCopyCommand icCommand in icCommands){
                int icBlockID = NextBlockID(Category.InsertCopy);

                foreach(Literal literal in icCommand.Literals){
                    int blockID = NextBlockID(Category.Literal);
                    int contextID = state.NextLiteralContextID(LiteralContextModes[blockID]);
                    int treeID = LiteralCtxMap.DetermineTreeID(blockID, contextID);

                    literalLists[treeID].Add(literal);
                    state.OutputLiteral(literal);
                }
                
                InsertCopyLengths icLengthValues = icCommand.Lengths;
                InsertCopyLengthCode icLengthCode;

                if (icCommand.CopyDistance == DistanceInfo.EndsAfterLiterals){
                    icLengthCode = icLengthValues.MakeCode(DistanceCodeZeroStrategy.PreferEnabled); // TODO good strategy?
                }
                else{
                    var distanceCode = icCommand.CopyDistance.MakeCode(DistanceParameters, state);
                    
                    if (distanceCode != null){
                        int blockID = NextBlockID(Category.Distance);
                        int contextID = icLengthValues.DistanceContextID;
                        int treeID = DistanceCtxMap.DetermineTreeID(blockID, contextID);

                        distanceCodeLists[treeID].Add(distanceCode);
                    }
                    
                    icLengthCode = icLengthValues.MakeCode(distanceCode == null || distanceCode.Equals(DistanceCode.Zero) ? DistanceCodeZeroStrategy.PreferEnabled : DistanceCodeZeroStrategy.Disable);
                    state.OutputCopy(icCommand.CopyLength, icLengthCode.UseDistanceCodeZero ? DistanceInfo.ImplicitCodeZero : icCommand.CopyDistance);
                }

                icLengthCodeLists[icBlockID].Add(icLengthCode);
            }

            foreach(var literalList in literalLists){
                if (literalList.Count == 0){
                    literalList.Add(new Literal(0));
                }
            }

            foreach(var distanceCodeList in distanceCodeLists){
                if (distanceCodeList.Count == 0){
                    distanceCodeList.Add(DistanceCode.Zero);
                }
            }

            var header = new MetaBlockCompressionHeader(
                BlockTypes,
                DistanceParameters,
                LiteralContextModes,
                LiteralCtxMap,
                DistanceCtxMap,
                ConstructHuffmanTrees(literalLists),
                ConstructHuffmanTrees(icLengthCodeLists),
                ConstructHuffmanTrees(distanceCodeLists)
            );

            var metaBlock = new MetaBlock.Compressed(isLast: false, new DataLength(state.OutputSize)){
                Contents = new CompressedMetaBlockContents(header, icCommands, bsCommands.Select<IReadOnlyList<BlockSwitchCommand>>((_, list) => list.AsReadOnly()))
            };
        }

        // Helpers

        private static List<T>[] NewListArray<T>(int arraySize){
            return Enumerable.Range(0, arraySize).Select(_ => new List<T>()).ToArray();
        }

        private static HuffmanTree<T>[] ConstructHuffmanTrees<T>(List<T>[] source) where T : IComparable<T>{
            return source.Select(list => HuffmanTree<T>.FromSymbols(list)).ToArray();
        }
    }
}
