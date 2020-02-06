using System.IO;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;

namespace BrotliCalc.Commands{
    class CmdCopyUncompressed : CmdAbstractFileMapper.Uncompressed{
        public override string FullName => "copy-uncompressed";
        public override string ShortName => "cu";

        protected override string WorkDesc => "Copied";

        protected override void MapFile(BrotliFileGroup group, BrotliFile.Uncompressed file, FileStream output){
            output.Write(file.Contents);
        }
    }
}
