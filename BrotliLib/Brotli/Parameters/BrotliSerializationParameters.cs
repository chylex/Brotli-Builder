using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliSerializationParameters{
        public static BrotliSerializationParameters Default => new BrotliSerializationParameters();

        public delegate bool DecideContextMapFeature(ContextMap contextMap);
        public delegate bool DecideComplexTreeFeature(IReadOnlyList<byte> symbolBits);

        public DecideContextMapFeature UseContextMapIMTF { get; set; } = _ => true;
        public DecideContextMapFeature UseContextMapRLE  { get; set; } = _ => true;

        public DecideComplexTreeFeature UseComplexTreeSkipCode   { get; set; } = _ => true;
        public DecideComplexTreeFeature UseComplexTreeRepeatCode { get; set; } = _ => true;
    }
}
