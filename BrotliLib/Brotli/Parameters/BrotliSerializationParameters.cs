using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Parameters.Heuristics;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliSerializationParameters{
        public static BrotliSerializationParameters Default { get; } = new Builder().Build();

        public delegate bool DecideComplexTreeFeature(IReadOnlyList<byte> symbolBits);

        public ContextMapHeuristics.DecideFeature  ContextMapMTF          { get; private set; }
        public ContextMapHeuristics.DecideRuns     ContextMapRLE          { get; private set; }
        public HuffmanTreeHeuristics.Generate<int> GenerateContextMapTree { get; private set; }

        public DecideComplexTreeFeature UseComplexTreeSkipCode   { get; private set; }
        public DecideComplexTreeFeature UseComplexTreeRepeatCode { get; private set; }

        #pragma warning disable CS8618
        private BrotliSerializationParameters(){}
        #pragma warning restore CS8618

        public sealed class Builder{
            public ContextMapHeuristics.DecideFeature  ContextMapMTF          { get; set; } = ContextMapHeuristics.MTF.Enable;
            public ContextMapHeuristics.DecideRuns     ContextMapRLE          { get; set; } = ContextMapHeuristics.RLE.KeepAll;
            public HuffmanTreeHeuristics.Generate<int> GenerateContextMapTree { get; set; } = HuffmanTree<int>.FromSymbols;

            public DecideComplexTreeFeature UseComplexTreeSkipCode   { get; set; } = _ => true;
            public DecideComplexTreeFeature UseComplexTreeRepeatCode { get; set; } = _ => true;

            public Builder(){}

            public Builder(BrotliSerializationParameters original){
                ContextMapMTF = original.ContextMapMTF;
                ContextMapRLE = original.ContextMapRLE;
                GenerateContextMapTree = original.GenerateContextMapTree;

                UseComplexTreeSkipCode = original.UseComplexTreeSkipCode;
                UseComplexTreeRepeatCode = original.UseComplexTreeRepeatCode;
            }

            public BrotliSerializationParameters Build(){
                return new BrotliSerializationParameters{
                    ContextMapMTF = ContextMapMTF,
                    ContextMapRLE = ContextMapRLE,
                    GenerateContextMapTree = GenerateContextMapTree,

                    UseComplexTreeSkipCode = UseComplexTreeSkipCode,
                    UseComplexTreeRepeatCode = UseComplexTreeRepeatCode
                };
            }
        }
    }
}
