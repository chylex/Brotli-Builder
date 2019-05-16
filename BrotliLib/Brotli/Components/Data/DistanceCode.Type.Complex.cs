using System;
using System.Text;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.State;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Data{
    public abstract partial class DistanceCode{
        /// <inheritdoc />
        /// <summary>
        /// Represents a distance code which uses additional bits from the stream to calculate the distance value.
        /// </summary>
        private sealed class Complex : DistanceCode{
            private readonly byte postfixBitCount;
            private readonly byte postfixBitValue;

            private readonly int extraBitCount;
            private readonly int topOffset;
            private readonly int bottomOffset;

            private int PostfixBitMask => (1 << postfixBitCount) - 1;

            /// <summary>
            /// Since the Brotli format documentation only specifies a bunch of magic formulas, here's an attempt at an explanation.
            /// Note that the distance code we're talking about is after subtracting <see cref="DistanceCode.Last.Codes"/> and <see cref="DistanceParameters.DirectCodeCount"/>.
            /// <para/>
            /// Distance code is constructed from the following groups of bits: MSB [extracount][x][postfix] LSB
            /// Distance value is constructed from the following groups of bits: MSB [1x][extravalue][postfix] LSB + offset
            /// <para/>
            /// <list type="bullet">
            /// <item><description>[postfix] is the bottom <see cref="DistanceParameters.PostfixBitCount"/> bits of the distance code,</description></item>
            /// <item><description>[extracount] is one less than the amount of extra bits that need to be read from the stream,</description></item>
            /// <item><description>[extravalue] is the value of those extra bits,</description></item>
            /// <item><description>[x] is a single bit which is just shoved near the beginning of the distance value</description></item>
            /// </list>
            /// <para/>
            /// Finally, the value is offset by (1 + direct code count), as all values below that can be represented using <see cref="DistanceCode.Direct"/> instead.
            /// </summary>
            public Complex(DistanceParameters parameters, int code) : base(code){
                int directCodeCount = parameters.DirectCodeCount;
                this.postfixBitCount = parameters.PostfixBitCount;

                int normalized = code - directCodeCount - Last.Codes.Length;

                if (normalized < 0){
                    throw new ArgumentOutOfRangeException(nameof(code), "Complex distance codes (normalized) must be at least 0.");
                }

                int hcode = normalized >> postfixBitCount;
                int lcode = normalized & PostfixBitMask;

                this.postfixBitValue = (byte)lcode;
                this.extraBitCount = 1 + (normalized >> (postfixBitCount + 1));

                this.topOffset = ((2 + (hcode & 1)) << extraBitCount) - 4;
                this.bottomOffset = 1 + lcode + directCodeCount;
            }

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                if (((value - 1) & PostfixBitMask) != postfixBitValue){
                    return false;
                }

                int extraBitValue = FindExtraBitValue(value);
                return extraBitValue >= 0 && extraBitValue < (1 << extraBitCount);
            }

            protected override int ReadValue(BrotliGlobalState state, BitReader reader){
                return CalculateValue(reader.NextChunk(extraBitCount));
            }

            protected override void WriteValue(BrotliGlobalState state, int value, BitWriter writer){
                writer.WriteChunk(extraBitCount, FindExtraBitValue(value));
            }

            private int CalculateValue(int extraBitValue){
                int topValue = extraBitValue + topOffset;
                return (topValue << postfixBitCount) + bottomOffset;
            }

            private int FindExtraBitValue(int value){
                int withoutBottomOffset = value - bottomOffset;

                if (withoutBottomOffset < 0){
                    return -1;
                }
                else{
                    return (withoutBottomOffset >> postfixBitCount) - topOffset;
                }
            }

            public override string ToString(){
                StringBuilder build = new StringBuilder();
                build.Append(base.ToString());
                build.Append(" | Value in { ");

                int lastExtraBitValue = (1 << extraBitCount) - 1;
                int printBeginningUpTo = Math.Min(3, lastExtraBitValue);

                for(int extraBitValue = 0; extraBitValue < printBeginningUpTo; extraBitValue++){
                    build.Append(CalculateValue(extraBitValue)).Append("; ");
                }

                if (printBeginningUpTo != lastExtraBitValue){
                    build.Append("...; ");
                }

                build.Append(CalculateValue(lastExtraBitValue));
                build.Append(" }");
                return build.ToString();
            }
        }
    }
}
