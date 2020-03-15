using System.IO;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Transformers;

namespace BrotliCalc.Commands{
    class CmdRebuild : CmdAbstractFileMapper.Compressed{
        public override string FullName => "rebuild";
        public override string ShortName => "rb";

        protected override string WorkDesc => "Rebuilt";

        protected override void MapFile(BrotliFileGroup group, BrotliFile.Compressed file, FileStream output){
            output.Write(group.SerializeAndValidate(file.Transforming(new TransformRebuild())).ToByteArray());
        }
    }
}
