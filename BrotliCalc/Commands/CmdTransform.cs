using System;
using System.Collections.Generic;
using System.IO;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Encode;

namespace BrotliCalc.Commands{
    class CmdTransform : CmdAbstractFileMapper.Compressed{
        private static readonly Dictionary<string, IBrotliTransformer> Transformers = new Dictionary<string, IBrotliTransformer>{
            { "touncompressed",     new TransformCompressedIntoUncompressed() },
            { "distanceparams",     new TransformTestDistanceParameters() },
            { "avoidlastdistcodes", new TransformAvoidLastDistanceCodes() },
            { "spliticlengths",     new TransformSplitInsertCopyLengths() },
            { "officiallqsplitter", new TransformOfficialBlockSplitterLQ() },
        };

        public static string TransformerArgumentDesc { get; }  = "<{" + string.Join('|', Transformers.Keys) + "}>";

        public static IBrotliTransformer GetTransformer(string arg){
            return Transformers.TryGetValue(arg, out var transformer) ? transformer : throw new ArgumentException($"Unknown transformer: {arg}");
        }

        public override string FullName => "transform";
        public override string ShortName => "t";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => TransformerArgumentDesc;

        protected override string WorkDesc => "Transformed";

        private IBrotliTransformer? transformer;

        protected override void Setup(string[] args){
            transformer = GetTransformer(args[0]);
        }

        protected override void MapFile(BrotliFileGroup group, BrotliFile.Compressed file, FileStream output){
            output.Write(group.SerializeAndValidate(file.Transforming(transformer!)).ToByteArray());
        }
    }
}
