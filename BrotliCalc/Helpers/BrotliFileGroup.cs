using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Collections;
using BrotliLib.Serialization;

namespace BrotliCalc.Helpers{
    class BrotliFileGroup{
        public BrotliFile.Uncompressed Uncompressed { get; }
        public IReadOnlyList<BrotliFile.Compressed> Compressed { get; }

        public BrotliFileGroup(BrotliFile.Uncompressed uncompressedFile, IReadOnlyList<BrotliFile.Compressed> compressedFiles){
            this.Uncompressed = uncompressedFile;
            this.Compressed = compressedFiles;
        }

        public BitStream SerializeAndValidate(BrotliFileStructure bfs){
            var serialized = bfs.Serialize();
            var output = bfs.GetDecompressionState(serialized);

            if (!CollectionHelper.Equal(output.AsBytes, Uncompressed.Contents)){
                throw new InvalidOperationException("Mismatched output bytes.");
            }

            return serialized;
        }

        public int CountBytesAndValidate(BrotliFileStructure bfs){
            return (7 + SerializeAndValidate(bfs).Length) / 8;
        }
    }
}
