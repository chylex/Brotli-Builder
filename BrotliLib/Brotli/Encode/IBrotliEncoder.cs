using BrotliLib.Brotli.Components;

namespace BrotliLib.Brotli.Encode{
    /// <summary>
    /// Allows converting bytes into a series of <see cref="MetaBlock"/> objects.
    /// </summary>
    public interface IBrotliEncoder{
        (MetaBlock MetaBlock, BrotliEncodeInfo Next) Encode(BrotliEncodeInfo info);
    }
}
