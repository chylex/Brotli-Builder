using System;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Encode{
    public sealed class BrotliEncodeInfo{
        public BrotliFileParameters FileParameters { get; }
        public BrotliCompressionParameters CompressionParameters { get; }

        public BrotliGlobalState State => state.Clone();

        public ArraySegment<byte> Bytes { get; }
        public bool IsFinished => Bytes.Count == 0;

        private readonly BrotliGlobalState state;

        private BrotliEncodeInfo(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, BrotliGlobalState state, ArraySegment<byte> bytes){
            this.FileParameters = fileParameters;
            this.CompressionParameters = compressionParameters;

            this.state = state;
            this.Bytes = bytes;
        }

        public BrotliEncodeInfo(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, byte[] bytes) : this(
            fileParameters,
            compressionParameters,
            new BrotliGlobalState(fileParameters),
            new ArraySegment<byte>(bytes)
        ){}

        public CompressedMetaBlockBuilder NewBuilder(){
            return new CompressedMetaBlockBuilder(state);
        }

        public BrotliEncodeInfo WithState(BrotliGlobalState newState){
            return new BrotliEncodeInfo(FileParameters, CompressionParameters, newState, Bytes);
        }

        public BrotliEncodeInfo WithProcessedBytes(BrotliGlobalState newState, int processedBytes){
            return new BrotliEncodeInfo(FileParameters, CompressionParameters, newState, Bytes.Slice(processedBytes));
        }

        public BrotliEncodeInfo WithOutputtedMetaBock(MetaBlock metaBlock){
            var newState = State;
            metaBlock.Decompress(newState);
            return WithProcessedBytes(newState, metaBlock.DataLength.UncompressedBytes);
        }
    }
}
