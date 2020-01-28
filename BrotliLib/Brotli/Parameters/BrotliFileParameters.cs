using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Default;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliFileParameters{
        public static BrotliFileParameters Default { get; } = new Builder().Build();

        public WindowSize WindowSize       { get; private set; }
        public BrotliDictionary Dictionary { get; private set; }

        #pragma warning disable CS8618
        private BrotliFileParameters(){}
        #pragma warning restore CS8618

        public sealed class Builder{
            public WindowSize WindowSize       { get; set; } = WindowSize.Default;
            public BrotliDictionary Dictionary { get; set; } = BrotliDefaultDictionary.Embedded;

            public Builder(){}

            public Builder(BrotliFileParameters original){
                WindowSize = original.WindowSize;
                Dictionary = original.Dictionary;
            }

            public BrotliFileParameters Build(){
                return new BrotliFileParameters{
                    WindowSize = WindowSize,
                    Dictionary = Dictionary
                };
            }
        }
    }
}
