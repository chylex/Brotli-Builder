using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters.Heuristics;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliCompressionParameters{
        public static BrotliCompressionParameters Default { get; } = new Builder().Build();

        public HuffmanTreeHeuristics.Generate<Literal>              GenerateLiteralTree      { get; private set; }
        public HuffmanTreeHeuristics.Generate<InsertCopyLengthCode> GenerateLengthCodeTree   { get; private set; }
        public HuffmanTreeHeuristics.Generate<DistanceCode>         GenerateDistanceCodeTree { get; private set; }

        public HuffmanTreeHeuristics.Generate<BlockTypeCode>   GenerateBlockTypeCodeTree   { get; private set; }
        public HuffmanTreeHeuristics.Generate<BlockLengthCode> GenerateBlockLengthCodeTree { get; private set; }

        public PickCodeHeuristics<DistanceCode>.Picker  DistanceCodePicker  { get; private set; }
        public PickCodeHeuristics<BlockTypeCode>.Picker BlockTypeCodePicker { get; private set; }
        
        #pragma warning disable CS8618
        private BrotliCompressionParameters(){}
        #pragma warning restore CS8618

        public sealed class Builder{
            public HuffmanTreeHeuristics.Generate<Literal>              GenerateLiteralTree      { get; set; } = HuffmanTree<Literal>.FromSymbols;
            public HuffmanTreeHeuristics.Generate<InsertCopyLengthCode> GenerateLengthCodeTree   { get; set; } = HuffmanTree<InsertCopyLengthCode>.FromSymbols;
            public HuffmanTreeHeuristics.Generate<DistanceCode>         GenerateDistanceCodeTree { get; set; } = HuffmanTree<DistanceCode>.FromSymbols;

            public HuffmanTreeHeuristics.Generate<BlockTypeCode>   GenerateBlockTypeCodeTree   { get; set; } = HuffmanTree<BlockTypeCode>.FromSymbols;
            public HuffmanTreeHeuristics.Generate<BlockLengthCode> GenerateBlockLengthCodeTree { get; set; } = HuffmanTree<BlockLengthCode>.FromSymbols;

            public PickCodeHeuristics<DistanceCode>.Picker  DistanceCodePicker  { get; set; } = PickCodeHeuristics<DistanceCode>.PickFirstOption; // TODO
            public PickCodeHeuristics<BlockTypeCode>.Picker BlockTypeCodePicker { get; set; } = PickCodeHeuristics<BlockTypeCode>.PickFirstOption; // TODO

            public Builder(){}

            public Builder(BrotliCompressionParameters original){
                GenerateLiteralTree = original.GenerateLiteralTree;
                GenerateLengthCodeTree = original.GenerateLengthCodeTree;
                GenerateDistanceCodeTree = original.GenerateDistanceCodeTree;

                GenerateBlockTypeCodeTree = original.GenerateBlockTypeCodeTree;
                GenerateBlockLengthCodeTree = original.GenerateBlockLengthCodeTree;

                DistanceCodePicker = original.DistanceCodePicker;
                BlockTypeCodePicker = original.BlockTypeCodePicker;
            }

            public BrotliCompressionParameters Build(){
                return new BrotliCompressionParameters{
                    GenerateLiteralTree = GenerateLiteralTree,
                    GenerateLengthCodeTree = GenerateLengthCodeTree,
                    GenerateDistanceCodeTree = GenerateDistanceCodeTree,

                    GenerateBlockTypeCodeTree = GenerateBlockTypeCodeTree,
                    GenerateBlockLengthCodeTree = GenerateBlockLengthCodeTree,

                    DistanceCodePicker = DistanceCodePicker,
                    BlockTypeCodePicker = BlockTypeCodePicker
                };
            }
        }
    }
}
