using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Parameters;

namespace BrotliLib.Brotli.Encode{
    public interface IBrotliTransformer{
        IList<(MetaBlock MetaBlock, BrotliGlobalState NextState)> Transform(MetaBlock original, BrotliGlobalState state, BrotliCompressionParameters parameters);
    }
}
