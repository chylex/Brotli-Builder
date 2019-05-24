using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli;

namespace BrotliCalc.Helpers{
    class BrotliFileGroup{
        public BrotliFile.Uncompressed Uncompressed { get; }
        public IReadOnlyList<BrotliFile.Compressed> Compressed { get; }

        public BrotliFileGroup(BrotliFile.Uncompressed uncompressedFile, IReadOnlyList<BrotliFile.Compressed> compressedFiles){
            this.Uncompressed = uncompressedFile;
            this.Compressed = compressedFiles;
        }

        public int CountBytesAndValidate(BrotliFileStructure bfs){
            var serialized = bfs.Serialize();
            var output = bfs.GetDecompressionState(serialized, enableMarkers: false);

            if (!output.AsBytes.SequenceEqual(Uncompressed.Contents)){
                throw new InvalidOperationException("Mismatched output bytes.");
            }

            return (7 + serialized.Length) / 8;
        }
    }
}
