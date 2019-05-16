using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.State;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Data{
    public abstract partial class DistanceCode{
        /// <inheritdoc />
        /// <summary>
        /// Represents a direct distance code, which is converted to a distance value between 1 and <see cref="DistanceParameters.DirectCodeCount"/> (both inclusive).
        /// </summary>
        private sealed class Direct : DistanceCode{
            private readonly int encodedValue;

            public Direct(int code) : base(code){
                this.encodedValue = code - DirectCodeOffset;

                if (this.encodedValue < 1 || this.encodedValue > DistanceParameters.MaxDirectCodeCount){
                    throw new ArgumentOutOfRangeException(nameof(code), "Direct distance codes (normalized) must be within range [1; " + DistanceParameters.MaxDirectCodeCount + "].");
                }
            }

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                return value == encodedValue;
            }

            protected override int ReadValue(BrotliGlobalState state, BitReader reader){
                return encodedValue;
            }

            protected override void WriteValue(BrotliGlobalState state, int value, BitWriter writer){
                // no extra bits
            }

            public override string ToString(){
                return base.ToString() + " | Value = " + encodedValue;
            }
        }
    }
}
