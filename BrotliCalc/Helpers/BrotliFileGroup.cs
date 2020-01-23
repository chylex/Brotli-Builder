using System;
using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Output;
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

        public BitStream SerializeAndValidate(BrotliFileStructure bfs){
            var serialized = bfs.Serialize(Parameters.Serialization);

            var reader = BrotliFileReader.FromBytes(serialized, MarkerLevel.None, Parameters.File.Dictionary);
            var output = new BrotliOutputStored();
            var state = new BrotliGlobalState(reader.Parameters, output);

            MetaBlock? metaBlock;

            while((metaBlock = reader.NextMetaBlock()) != null){
                metaBlock.Decompress(state);
            }

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
