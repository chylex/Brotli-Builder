using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;

namespace BrotliCalc.Commands{
    class CmdReserialize : CmdAbstractFileMapper.Compressed{
        public override string FullName => "reserialize";
        public override string ShortName => "r";

        protected override string WorkDesc => "Reserialized";

        protected override byte[] MapFile(BrotliFileGroup group, BrotliFile.Compressed file){
            return group.SerializeAndValidate(file.Structure, Parameters.Serialization).ToByteArray();
        }
    }
}
