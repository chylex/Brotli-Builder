using System;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters.Heuristics;
using BrotliLib.Collections;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliCompressionParameters{
        public static BrotliCompressionParameters Default { get; } = new Builder().Build();

        public delegate HuffmanTree<T> GenerateHuffmanTree<T>(FrequencyList<T> frequencies) where T : IComparable<T>;

        public GenerateHuffmanTree<Literal>              GenerateLiteralTree      { get; private set; }
        public GenerateHuffmanTree<InsertCopyLengthCode> GenerateLengthCodeTree   { get; private set; }
        public GenerateHuffmanTree<DistanceCode>         GenerateDistanceCodeTree { get; private set; }

        public PickCodeHeuristics<DistanceCode>.Picker  DistanceCodePicker  { get; private set; }
        public PickCodeHeuristics<BlockTypeCode>.Picker BlockTypeCodePicker { get; private set; }
        
        #pragma warning disable CS8618
        private BrotliCompressionParameters(){}
        #pragma warning restore CS8618

        public sealed class Builder{
            public GenerateHuffmanTree<Literal>              GenerateLiteralTree      { get; set; } = HuffmanTree<Literal>.FromSymbols;
            public GenerateHuffmanTree<InsertCopyLengthCode> GenerateLengthCodeTree   { get; set; } = HuffmanTree<InsertCopyLengthCode>.FromSymbols;
            public GenerateHuffmanTree<DistanceCode>         GenerateDistanceCodeTree { get; set; } = HuffmanTree<DistanceCode>.FromSymbols;

            public PickCodeHeuristics<DistanceCode>.Picker  DistanceCodePicker  { get; set; } = PickCodeHeuristics<DistanceCode>.PickFirstOption; // TODO
            public PickCodeHeuristics<BlockTypeCode>.Picker BlockTypeCodePicker { get; set; } = PickCodeHeuristics<BlockTypeCode>.PickFirstOption; // TODO

            public Builder(){}

            public Builder(BrotliCompressionParameters original){
                GenerateLiteralTree = original.GenerateLiteralTree;
                GenerateLengthCodeTree = original.GenerateLengthCodeTree;
                GenerateDistanceCodeTree = original.GenerateDistanceCodeTree;

                DistanceCodePicker = original.DistanceCodePicker;
                BlockTypeCodePicker = original.BlockTypeCodePicker;
            }

            public BrotliCompressionParameters Build(){
                return new BrotliCompressionParameters{
                    GenerateLiteralTree = GenerateLiteralTree,
                    GenerateLengthCodeTree = GenerateLengthCodeTree,
                    GenerateDistanceCodeTree = GenerateDistanceCodeTree,

                    DistanceCodePicker = DistanceCodePicker,
                    BlockTypeCodePicker = BlockTypeCodePicker
                };
            }
        }
    }
}
