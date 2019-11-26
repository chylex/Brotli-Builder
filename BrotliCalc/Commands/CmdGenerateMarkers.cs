using System;
using System.Text;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Markers;
using BrotliLib.Serialization;

namespace BrotliCalc.Commands{
    class CmdGenerateMarkers : CmdAbstractFileMapper.Compressed{
        public override string FullName => "generate-markers";
        public override string ShortName => "gm";

        protected override int ExtraArgumentCount => 1;
        protected override string ExtraArgumentDesc => "<s|simple|v|verbose>";

        protected override string WorkDesc => "Extracted markers from";
        protected override string AppendFileName => ".txt";

        private MarkerLevel markerLevel;

        protected override void Setup(string[] args){
            markerLevel = args[0] switch{
                var x when x == "s" || x == "simple"  => MarkerLevel.Simple,
                var x when x == "v" || x == "verbose" => MarkerLevel.Verbose,
                _ => throw new ArgumentException("Invalid marker type: " + args[0])
            };
        }

        protected override byte[] MapFile(BrotliFileGroup group, BrotliFile.Compressed file){
            var state = file.Structure.GetDecompressionState(new BitStream(file.Contents), markerLevel);
            var text = state.MarkerRoot.BuildText();

            return Encoding.UTF8.GetBytes(text);
        }
    }
}
