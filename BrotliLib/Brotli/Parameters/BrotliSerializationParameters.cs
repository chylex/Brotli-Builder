using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliSerializationParameters{
        public static BrotliSerializationParameters Default { get; } = new BrotliSerializationParameters();

        public delegate bool DecideContextMapFeature(ContextMap contextMap);

        public DecideContextMapFeature UseContextMapIMTF = _ => true;
        public DecideContextMapFeature UseContextMapRLE = _ => true;
    }
}
