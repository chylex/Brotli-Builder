using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Serialization.Reader;
using BrotliLib.Serialization.Writer;

namespace BrotliLib.Brotli.Components.Data{
    public abstract partial class DistanceCode{
        /// <inheritdoc />
        /// <summary>
        /// Represents a direct distance code, which is converted to a distance value between 1 and <see cref="DistanceParameters.DirectCodeCount"/> (both inclusive).
        /// </summary>
        public sealed class Direct : DistanceCode{
            private readonly int encodedValue;

            public Direct(int code) : base(code){
                this.encodedValue = code - DirectCodeOffset;

                if (this.encodedValue < 1 || this.encodedValue > DistanceParameters.MaxDirectCodeCount){
                    throw new ArgumentOutOfRangeException(nameof(code), "Direct distance codes (normalized) must be within range [1; " + DistanceParameters.MaxDirectCodeCount + "].");
                }
            }

            public override int ExtraBits => 0;

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                return value == encodedValue;
            }

            protected override int ReadValue(BrotliGlobalState state, IBitReader reader){
                return encodedValue;
            }

            protected override void WriteValue(BrotliGlobalState state, int value, IBitWriter writer){
                // no extra bits
            }

            public override string ToString(){
                return base.ToString() + " | Value = " + encodedValue;
            }
        }
    }
}
