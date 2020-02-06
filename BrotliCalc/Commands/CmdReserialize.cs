using System.IO;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;

namespace BrotliCalc.Commands{
    class CmdReserialize : CmdAbstractFileMapper.Compressed{
        public override string FullName => "reserialize";
        public override string ShortName => "r";

        protected override string WorkDesc => "Reserialized";

        protected override void MapFile(BrotliFileGroup group, BrotliFile.Compressed file, FileStream output){
            output.Write(group.SerializeAndValidate(file.Structure).ToByteArray());
        }
    }
}
