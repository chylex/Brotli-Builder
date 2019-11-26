using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Default;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliFileParameters{
        public static BrotliFileParameters Default => new BrotliFileParameters();

        public WindowSize WindowSize { get; set; } = WindowSize.Default;
        public BrotliDictionary Dictionary { get; set; } = BrotliDefaultDictionary.Embedded;
    }
}
