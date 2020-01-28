using System.Collections.Generic;
using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Parameters{
    public sealed class BrotliSerializationParameters{
        public static BrotliSerializationParameters Default { get; } = new Builder().Build();

        public delegate bool DecideContextMapFeature(ContextMap contextMap);
        public delegate bool DecideComplexTreeFeature(IReadOnlyList<byte> symbolBits);

        public DecideContextMapFeature UseContextMapIMTF { get; private set; }
        public DecideContextMapFeature UseContextMapRLE  { get; private set; }

        public DecideComplexTreeFeature UseComplexTreeSkipCode   { get; private set; }
        public DecideComplexTreeFeature UseComplexTreeRepeatCode { get; private set; }

        #pragma warning disable CS8618
        private BrotliSerializationParameters(){}
        #pragma warning restore CS8618

        public sealed class Builder{
            public DecideContextMapFeature UseContextMapIMTF { get; set; } = _ => true;
            public DecideContextMapFeature UseContextMapRLE  { get; set; } = _ => true;

            public DecideComplexTreeFeature UseComplexTreeSkipCode   { get; set; } = _ => true;
            public DecideComplexTreeFeature UseComplexTreeRepeatCode { get; set; } = _ => true;

            public Builder(){}

            public Builder(BrotliSerializationParameters original){
                UseContextMapIMTF = original.UseContextMapIMTF;
                UseContextMapRLE = original.UseContextMapRLE;

                UseComplexTreeSkipCode = original.UseComplexTreeSkipCode;
                UseComplexTreeRepeatCode = original.UseComplexTreeRepeatCode;
            }

            public BrotliSerializationParameters Build(){
                return new BrotliSerializationParameters{
                    UseContextMapIMTF = UseContextMapIMTF,
                    UseContextMapRLE = UseContextMapRLE,

                    UseComplexTreeSkipCode = UseComplexTreeSkipCode,
                    UseComplexTreeRepeatCode = UseComplexTreeRepeatCode
                };
            }
        }
    }
}
