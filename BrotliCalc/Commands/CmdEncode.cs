using System;
using System.Collections.Generic;
using System.IO;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Encoders;
using BrotliLib.Brotli.Encode;

namespace BrotliCalc.Commands{
    class CmdEncode : CmdAbstractFileMapper.Uncompressed{
        private static readonly Dictionary<string, IBrotliEncoder> Encoders = new Dictionary<string, IBrotliEncoder>{
            { "literals",      new EncodeLiterals() },
            { "greedy-copies", new EncodeGreedySearch.OnlyBackReferences(minLength: 4) },
            { "greedy-dict",   new EncodeGreedySearch.OnlyDictionary() },
            { "greedy-mixed",  new EncodeGreedySearch.Mixed(minCopyLength: 4) },
        };

        public static string EncoderArgumentDesc { get; }  = "<{" + string.Join('|', Encoders.Keys) + "}>";

        public static IBrotliEncoder GetEncoder(string arg){
            return Encoders.TryGetValue(arg, out var encoder) ? encoder : throw new ArgumentException($"Unknown encoder: {arg}");
        }

        public override string FullName => "encode";
        public override string ShortName => "e";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => EncoderArgumentDesc;

        protected override string WorkDesc => "Encoded";
        protected override string AppendFileName => $".{arg}.br";

        private IBrotliEncoder? encoder;
        private string? arg;

        protected override void Setup(string[] args){
            encoder = GetEncoder(arg = args[0]);
        }

        protected override void MapFile(BrotliFileGroup group, BrotliFile.Uncompressed file, FileStream output){
            output.Write(group.SerializeAndValidate(file.Encoding(encoder!)).ToByteArray());
        }
    }
}
