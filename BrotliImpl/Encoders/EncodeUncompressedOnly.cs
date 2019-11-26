using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Collections;

namespace BrotliImpl.Encoders{
    /// <summary>
    /// Encodes bytes into a series of uncompressed meta-blocks.
    /// </summary>
    public class EncodeUncompressedOnly : IBrotliEncoder{
        public (MetaBlock, BrotliEncodeInfo) Encode(BrotliEncodeInfo info){
            var state = info.State;
            var bytes = CollectionHelper.SliceAtMost(info.Bytes, DataLength.MaxUncompressedBytes).ToArray();

            state.OutputBytes(bytes);
            return (new MetaBlock.Uncompressed(bytes), info.WithProcessedBytes(state, bytes.Length));
        }
    }
}
