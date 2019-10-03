using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.State;
using BrotliLib.Brotli.State.Output;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Encode{
    public sealed class CompressedMetaBlockBuilder{
        public int OutputSize => intermediateState.OutputSize - initialState.OutputSize;

        public CategoryMap<BlockSwitchBuilder> BlockTypes { get; } = BlockTypeInfo.Empty.Select(info => new BlockSwitchBuilder(info));
        public DistanceParameters DistanceParameters { get; set; } = DistanceParameters.NoDirectCodes;

        public LiteralContextMode[] LiteralContextModes { get; set; } = { LiteralContextMode.LSB6 };
        public ContextMap LiteralCtxMap { get; set; } = ContextMap.Literals.Simple;
        public ContextMap DistanceCtxMap { get; set; } = ContextMap.Distances.Simple;

        public IEnumerable<InsertCopyCommand> InsertCopyCommands => icCommands;

        private readonly IList<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
        
        private readonly BrotliGlobalState initialState;
        private readonly BrotliGlobalState intermediateState;

        // Construction

        private CompressedMetaBlockBuilder(BrotliGlobalState state){
            this.initialState = state.Clone();
            this.intermediateState = state.Clone();
        }

        public CompressedMetaBlockBuilder(BrotliFileParameters parameters) : this(new BrotliGlobalState(parameters, new BrotliOutputWindowed(parameters.WindowSize))){}

        public CompressedMetaBlockBuilder(MetaBlock.Compressed metaBlock, BrotliGlobalState state) : this(state){
            var contents = metaBlock.Contents;
            var header = contents.Header;
            
            this.BlockTypes = header.BlockTypes.Select(info => new BlockSwitchBuilder(info));
            this.DistanceParameters = header.DistanceParameters;
            this.LiteralContextModes = header.LiteralCtxModes.ToArray();
            this.LiteralCtxMap = header.LiteralCtxMap;
            this.DistanceCtxMap = header.DistanceCtxMap;

            foreach(InsertCopyCommand command in contents.InsertCopyCommands){
                AddInsertCopy(command);
            }

            foreach(Category category in Categories.LID){
                var bsBuilder = BlockTypes[category];

                foreach(BlockSwitchCommand command in contents.BlockSwitchCommands[category]){
                    bsBuilder.AddBlockSwitch(command);
                }
            }
        }

        // Commands

        public CompressedMetaBlockBuilder AddInsertCopy(InsertCopyCommand command){
            icCommands.Add(command);

            foreach(Literal literal in command.Literals){
                intermediateState.OutputLiteral(literal);
            }

            if (command.CopyDistance != DistanceInfo.EndsAfterLiterals){
                intermediateState.OutputCopy(command.CopyLength, command.CopyDistance);
            }

            return this;
        }

        public CompressedMetaBlockBuilder AddInsertCopy(IList<Literal> literals, DictionaryIndexEntry dictionaryEntry){
            return AddInsertCopy(new InsertCopyCommand(literals, dictionaryEntry.CopyLength, 1 + intermediateState.MaxDistance + literals.Count + dictionaryEntry.Packed));
        }

        // Building

        public (MetaBlock MetaBlock, Func<CompressedMetaBlockBuilder> Next) Build(){
            var state = initialState.Clone();

            // Block type setup
            
            var blockTypeInfo = BlockTypes.Select(builder => builder.Build());
            var bsCommands = BlockTypes.Select<IList<BlockSwitchCommand>>(builder => new List<BlockSwitchCommand>(builder.Commands));

            var blockTrackers = blockTypeInfo.Select(info => new BlockSwitchTracker(info));
            var blockSwitchQueues = bsCommands.Select(commands => new Queue<BlockSwitchCommand>(commands));

            int NextBlockID(Category category){
                return blockTrackers[category].SimulateCommand(blockSwitchQueues);
            }

            // Command processing
            
            var literalFreq = NewFreqArray<Literal>(LiteralCtxMap.TreeCount);
            var icLengthCodeFreq = NewFreqArray<InsertCopyLengthCode>(blockTypeInfo[Category.InsertCopy].Count);
            var distanceCodeFreq = NewFreqArray<DistanceCode>(DistanceCtxMap.TreeCount);

            var icCommandCount = icCommands.Count;
            var icCommandsFinal = new List<InsertCopyCommand>(icCommandCount);

            for(int index = 0; index < icCommandCount; index++){
                var icCommand = icCommands[index];
                int icBlockID = NextBlockID(Category.InsertCopy);

                foreach(Literal literal in icCommand.Literals){
                    int blockID = NextBlockID(Category.Literal);
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
                    var distanceCodes = icCommand.CopyDistance.MakeCode(DistanceParameters, state);
                    DistanceCode distanceCode = null;
                    
                    if (distanceCodes != null){
                        int blockID = NextBlockID(Category.Distance);
                        int contextID = icLengthValues.DistanceContextID;
                        int treeID = DistanceCtxMap.DetermineTreeID(blockID, contextID);

                        var codeList = distanceCodeFreq[treeID];
                        codeList.Add(distanceCode = distanceCodes.FirstOrDefault(codeList.Contains) ?? distanceCodes[0]); // TODO figure out a better strategy for picking the code
                    }
                    
                    icLengthCode = icLengthValues.MakeCode(distanceCode == null || distanceCode.Equals(DistanceCode.Zero) ? DistanceCodeZeroStrategy.PreferEnabled : DistanceCodeZeroStrategy.Disable);

                    if (icLengthCode.UseDistanceCodeZero){
                        icCommand = icCommand.WithImplicitDistanceCodeZero();
                    }

                    state.OutputCopy(icCommand.CopyLength, icCommand.CopyDistance);
                }

                icLengthCodeFreq[icBlockID].Add(icLengthCode);
                icCommandsFinal.Add(icCommand);
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

            var header = new MetaBlockCompressionHeader(
                blockTypeInfo,
                DistanceParameters,
                LiteralContextModes,
                LiteralCtxMap,
                DistanceCtxMap,
                ConstructHuffmanTrees(literalFreq),
                ConstructHuffmanTrees(icLengthCodeFreq),
                ConstructHuffmanTrees(distanceCodeFreq)
            );

            var metaBlock = new MetaBlock.Compressed(isLast: false, new DataLength(OutputSize)){
                Contents = new CompressedMetaBlockContents(header, icCommandsFinal, bsCommands)
            };

            return (metaBlock, () => new CompressedMetaBlockBuilder(state));
        }

        // Helpers

        private static FrequencyList<T>[] NewFreqArray<T>(int arraySize) where T : IComparable<T>{
            return Enumerable.Range(0, arraySize).Select(_ => new FrequencyList<T>()).ToArray();
        }

        private static HuffmanTree<T>[] ConstructHuffmanTrees<T>(FrequencyList<T>[] source) where T : IComparable<T>{
            return source.Select(list => HuffmanTree<T>.FromSymbols(list)).ToArray();
        }
    }
}
