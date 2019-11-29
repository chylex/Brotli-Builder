using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Collections;
using BrotliLib.Markers;
using BrotliLib.Serialization;

namespace BrotliCalc.Helpers{
    class BrotliFileGroup{
        public BrotliFile.Uncompressed Uncompressed { get; }
        public IReadOnlyList<BrotliFile.Compressed> Compressed { get; }

        public BrotliFileGroup(BrotliFile.Uncompressed uncompressedFile, IReadOnlyList<BrotliFile.Compressed> compressedFiles){
            this.Uncompressed = uncompressedFile;
            this.Compressed = compressedFiles;
        }

        public BitStream SerializeAndValidate(BrotliFileStructure bfs, BrotliSerializationParameters parameters){
            var serialized = bfs.Serialize(parameters);
            var output = BrotliFileStructure.FromBytes(serialized, MarkerLevel.None).Structure.Decompress();

            if (!CollectionHelper.Equal(output.AsBytes, Uncompressed.Contents)){
                throw new InvalidOperationException("Mismatched output bytes.");
            }

            return serialized;
        }

        public int CountBytesAndValidate(BrotliFileStructure bfs, BrotliSerializationParameters parameters){
            return (7 + SerializeAndValidate(bfs, parameters).Length) / 8;
        }
    }
}
