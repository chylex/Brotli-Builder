using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Default;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliFileParameters{
        public WindowSize WindowSize { get; }
        public BrotliDictionary Dictionary { get; }

        public BrotliFileParameters(WindowSize windowSize, BrotliDictionary dictionary){
            this.WindowSize = windowSize;
            this.Dictionary = dictionary;
        }

        public BrotliFileParameters(WindowSize windowSize) : this(windowSize, BrotliDefaultDictionary.Embedded){}
        public BrotliFileParameters() : this(WindowSize.Default){}
    }
}
