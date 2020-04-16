using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Streaming;
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

        private BitStream Validate(BitStream bits){
            IBrotliFileStream reader = BrotliFileReader.FromBytes(bits, Parameters.File.Dictionary);

            var output = new BrotliOutputStored();
            var state = new BrotliGlobalState(reader.Parameters, output);

            reader.ForEachRemainingMetaBlock(metaBlock => metaBlock.Decompress(state));

            if (!CollectionHelper.Equal(output.AsBytes, Uncompressed.Contents)){
                throw new InvalidOperationException("Mismatched output bytes.");
            }

            return bits;
        }

        public BitStream SerializeAndValidate(IBrotliFileStream reader){
            var writer = new BrotliFileWriter(reader.Parameters, Parameters.Serialization);
            reader.ForEachRemainingMetaBlock(writer.WriteMetaBlock);
            return Validate(writer.Close());
        }

        public BitStream SerializeAndValidate(BrotliFileStructure bfs){
            return Validate(bfs.Serialize(Parameters.Serialization));
        }

        public int CountBytesAndValidate(IBrotliFileStream reader){
            return (7 + SerializeAndValidate(reader).Length) / 8;
        }

        public int CountBytesAndValidate(BrotliFileStructure bfs){
            return (7 + SerializeAndValidate(bfs).Length) / 8;
        }
    }
}
