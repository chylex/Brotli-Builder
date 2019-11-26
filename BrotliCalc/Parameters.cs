using BrotliLib.Brotli.Parameters;

namespace BrotliCalc{
    static class Parameters{
        public static BrotliFileParameters File { get; } = BrotliFileParameters.Default;
        public static BrotliSerializationParameters Serialization { get; } = BrotliSerializationParameters.Default;
        public static BrotliCompressionParameters Compression { get; } = BrotliCompressionParameters.Default;
    }
}
