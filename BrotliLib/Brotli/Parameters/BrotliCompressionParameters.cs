namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliCompressionParameters{
        public static BrotliCompressionParameters Default { get; } = new Builder().Build();
        
        #pragma warning disable CS8618
        private BrotliCompressionParameters(){}
        #pragma warning restore CS8618

        public sealed class Builder{
            public Builder(){}

            public Builder(BrotliCompressionParameters original){
            }

            public BrotliCompressionParameters Build(){
                return new BrotliCompressionParameters{
                };
            }
        }
    }
}
