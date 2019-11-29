using System;
using System.Text;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Markers;

namespace BrotliCalc.Commands{
    class CmdGenerateMarkers : CmdAbstractFileMapper.Compressed{
        public override string FullName => "generate-markers";
        public override string ShortName => "gm";

        protected override int ExtraArgumentCount => 2;
        protected override string ExtraArgumentDesc => "<s|simple|v|verbose> <includeBitCounts>";

        protected override string WorkDesc => "Extracted markers from";
        protected override string AppendFileName => ".txt";

        private MarkerLevel markerLevel;
        private bool includeBitCounts;

        protected override void Setup(string[] args){
            markerLevel = args[0] switch{
                var x when x == "s" || x == "simple"  => MarkerLevel.Simple,
                var x when x == "v" || x == "verbose" => MarkerLevel.Verbose,
                _ => throw new ArgumentException("Invalid marker type: " + args[0])
            };

            includeBitCounts = args[1] switch{
                var x when x == "y" || x == "yes" || x == "true" => true,
                var x when x == "n" || x == "no" || x == "false" => false,
                _ => throw new ArgumentException("Invalid includeBitCounts value: " + args[1])
            };
        }

        protected override byte[] MapFile(BrotliFileGroup group, BrotliFile.Compressed file){
            var (_, markerRoot) = file.GetStructureWithMarkers(markerLevel);
            var text = markerRoot.BuildText(includeBitCounts);

            return Encoding.UTF8.GetBytes(text);
        }
    }
}
