using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.State.Output;

namespace BrotliLib.Brotli.Encode{
    public sealed class CompressedMetaBlockBuilder{
        public CategoryMap<BlockTypeInfo> BlockTypes { get; set; } = BlockTypeInfo.Empty; // TODO support block splits
        public DistanceParameters DistanceParameters { get; set; } = DistanceParameters.NoDirectCodes;

        public LiteralContextMode[] LiteralContextModes { get; set; } = { LiteralContextMode.LSB6 };
        public ContextMap LiteralCtxMap { get; set; } = ContextMap.Literals.Simple;
        public ContextMap DistanceCtxMap { get; set; } = ContextMap.Distances.Simple;

        public BrotliGlobalState State { get; } // TODO private?

        private readonly WindowSize windowSize;
        private readonly List<InsertCopyCommand> icCommands = new List<InsertCopyCommand>();
        
        public CompressedMetaBlockBuilder(WindowSize windowSize, BrotliDictionary dictionary){
            this.windowSize = windowSize;
            this.State = new BrotliGlobalState(dictionary, windowSize, new BrotliOutputStored()); // TODO support multiple meta-blocks
        }

        public CompressedMetaBlockBuilder(WindowSize windowSize) : this(windowSize, BrotliDefaultDictionary.Embedded){}

        // Commands

        public CompressedMetaBlockBuilder AddCommand(InsertCopyCommand icCommand){
            icCommands.Add(icCommand);

            foreach(Literal literal in icCommand.Literals){
                State.OutputLiteral(literal);
            }

            if (icCommand.CopyDistance != DistanceInfo.EndsAfterLiterals){
                State.OutputCopy(icCommand.CopyLength, icCommand.CopyDistance);
            }

            return this;
        }

        // Building

        public MetaBlock Build(bool isLast){
            var state = new BrotliGlobalState(BrotliDefaultDictionary.Embedded, windowSize, new BrotliOutputStored()); // TODO support multiple meta-blocks
            
            var icLengthCodes = new List<InsertCopyLengthCode>();
            var literalLists = Enumerable.Range(0, LiteralCtxMap.TreeCount).Select(_ => new List<Literal>()).ToArray();
            var distanceCodeLists = Enumerable.Range(0, DistanceCtxMap.TreeCount).Select(_ => new List<DistanceCode>()).ToArray();

            foreach(InsertCopyCommand icCommand in icCommands){
                foreach(Literal literal in icCommand.Literals){
                    int contextID = state.NextLiteralContextID(LiteralContextModes[0]);
                    int treeID = LiteralCtxMap.DetermineTreeID(0, contextID);

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
                        int contextID = icLengthValues.DistanceContextID;
                        int treeID = DistanceCtxMap.DetermineTreeID(0, contextID);

                        distanceCodeLists[treeID].Add(distanceCode);
                    }
                    
                    icLengthCode = icLengthValues.MakeCode(distanceCode == null || distanceCode.Equals(DistanceCode.Zero) ? DistanceCodeZeroStrategy.PreferEnabled : DistanceCodeZeroStrategy.Disable);
                    state.OutputCopy(icCommand.CopyLength, icLengthCode.UseDistanceCodeZero ? DistanceInfo.ImplicitCodeZero : icCommand.CopyDistance);
                }

                icLengthCodes.Add(icLengthCode);
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
                literalLists.Select(literalList => HuffmanTree<Literal>.FromSymbols(literalList)).ToArray(),
                new []{ HuffmanTree<InsertCopyLengthCode>.FromSymbols(icLengthCodes) },
                distanceCodeLists.Select(distanceList => HuffmanTree<DistanceCode>.FromSymbols(distanceList)).ToArray()
            );

            return new MetaBlock.Compressed(isLast, new DataLength(state.OutputSize)){
                Contents = new CompressedMetaBlockContents(header, icCommands)
            };
        }
    }
}
