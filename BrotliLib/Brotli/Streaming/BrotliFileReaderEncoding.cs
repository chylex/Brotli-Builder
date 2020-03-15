using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Streaming{
    public class BrotliFileReaderEncoding : IBrotliFileReader{
        public BrotliFileParameters Parameters => encodeInfo.FileParameters;
        public BrotliGlobalState State => encodeInfo.State;

        private readonly IBrotliEncoder encoder;
        private BrotliEncodeInfo encodeInfo;

        public BrotliFileReaderEncoding(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, byte[] bytes, IBrotliEncoder encoder){
            this.encoder = encoder;
            this.encodeInfo = new BrotliEncodeInfo(fileParameters, compressionParameters, bytes);
        }

        public MetaBlock? NextMetaBlock(){
            if (encodeInfo.IsFinished){
                return null;
            }
            
            var (metaBlock, newEncodeInfo) = encoder.Encode(encodeInfo);

            encodeInfo = newEncodeInfo;
            return metaBlock;
        }
    }
}
