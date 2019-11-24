using System.Text;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliLib.Serialization;

namespace BrotliCalc.Commands{
    class CmdGenerateMarkers : CmdAbstractFileMapper.Compressed{
        public override string FullName => "generate-markers";
        public override string ShortName => "gm";

        protected override string WorkDesc => "Extracted markers from";
        protected override string AppendFileName => ".txt";

        protected override byte[] MapFile(BrotliFileGroup group, BrotliFile.Compressed file){
            var state = file.Structure.GetDecompressionState(new BitStream(file.Contents), enableMarkers: true);
            var text = state.BitMarkerRoot.BuildText();

            return Encoding.UTF8.GetBytes(text);
        }
    }
}
