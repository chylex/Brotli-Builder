using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliSerializationParameters{
        public static BrotliSerializationParameters Default => new BrotliSerializationParameters();

        public delegate bool DecideContextMapFeature(ContextMap contextMap);

        public DecideContextMapFeature UseContextMapIMTF { get; set; } = _ => true;
        public DecideContextMapFeature UseContextMapRLE  { get; set; } = _ => true;
    }
}
