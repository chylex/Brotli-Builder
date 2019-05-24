using System.Collections.Generic;

namespace BrotliCalc.Helpers{
    class BrotliFileGroup{
        public BrotliFile.Uncompressed Uncompressed { get; }
        public IReadOnlyList<BrotliFile.Compressed> Compressed { get; }

        public BrotliFileGroup(BrotliFile.Uncompressed uncompressedFile, IReadOnlyList<BrotliFile.Compressed> compressedFiles){
            this.Uncompressed = uncompressedFile;
            this.Compressed = compressedFiles;
        }
    }
}
