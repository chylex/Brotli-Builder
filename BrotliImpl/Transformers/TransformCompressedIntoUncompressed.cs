using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;

namespace BrotliImpl.Transformers{
    public class TransformCompressedIntoUncompressed : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var uncompressed = new BrotliOutputStored();

            state.AddOutputCallback(uncompressed);
            original.Decompress(state);
            state.RemoveOutputCallback(uncompressed);

            return (new MetaBlock.Uncompressed(uncompressed.AsBytes), state);
        }
    }
}
