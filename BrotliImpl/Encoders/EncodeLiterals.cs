using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Collections;

namespace BrotliImpl.Encoders{
    /// <summary>
    /// Encodes bytes into a series of compressed meta-blocks, where each contains a single insert&amp;copy command with each byte stored as a literal.
    /// </summary>
    public class EncodeLiterals : IBrotliEncoder{
        public (MetaBlock, BrotliEncodeInfo) Encode(BrotliEncodeInfo info){
            var bytes = CollectionHelper.SliceAtMost(info.Bytes, DataLength.MaxUncompressedBytes).ToArray();

            return info.NewBuilder()
                       .AddInsertFinal(Literal.FromBytes(bytes))
                       .Build(info);
        }
    }
}
