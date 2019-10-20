using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.State;
using BrotliLib.IO.Reader;
using BrotliLib.IO.Writer;
using BrotliLib.Numbers;

namespace BrotliLib.Brotli.Components.Data{
    public abstract partial class DistanceCode{
        /// <inheritdoc />
        /// <summary>
        /// Represents a distance code which uses additional bits from the stream to calculate the distance value.
        /// <para/>
        /// To understand what each code represents, it helps to see which distances a code can represent with various <see cref="DistanceParameters.PostfixBitCount"/> values.
        /// In the following examples, assume <see cref="DistanceParameters.DirectCodeCount"/> is 0. The first code is 16, as the first 15 codes are always <see cref="DistanceCode.Last"/>.
        /// <para/>
        /// <list type="bullet">
        /// <item><description>PostfixBitCount = 0 | Code = 16 | Value in { 1; 2 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 17 | Value in { 3; 4 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 18 | Value in { 5; 6; 7; 8 } | 4 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 19 | Value in { 9; 10; 11; 12 } | 4 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 20 | Value in { 13; 14; 15; ...; 20 } | 8 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 21 | Value in { 21; 22; 23; ...; 28 } | 8 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 22 | Value in { 29; 30; 31; ...; 44 } | 16 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 23 | Value in { 45; 46; 47; ...; 60 } | 16 values</description></item>
        /// <item><description>PostfixBitCount = 0 | Code = 24 | Value in { 61; 62; 63; ...; 92 } | 32 values</description></item>
        /// </list>
        /// <para/>
        /// <list type="bullet">
        /// <item><description>PostfixBitCount = 1 | Code = 16 | Value in { 1; 3 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 17 | Value in { 2; 4 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 18 | Value in { 5; 7 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 19 | Value in { 6; 8 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 20 | Value in { 9; 11; 13; 15 } | 4 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 21 | Value in { 10; 12; 14; 16 } | 4 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 22 | Value in { 17; 19; 21; 23 } | 4 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 23 | Value in { 18; 20; 22; 24 } | 4 values</description></item>
        /// <item><description>PostfixBitCount = 1 | Code = 24 | Value in { 25; 27; 29; ...; 39 } | 8 values</description></item>
        /// </list>
        /// <para/>
        /// <list type="bullet">
        /// <item><description>PostfixBitCount = 2 | Code = 16 | Value in { 1; 5 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 17 | Value in { 2; 6 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 18 | Value in { 3; 7 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 19 | Value in { 4; 8 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 20 | Value in { 9; 13 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 21 | Value in { 10; 14 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 22 | Value in { 11; 15 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 23 | Value in { 12; 16 } | 2 values</description></item>
        /// <item><description>PostfixBitCount = 2 | Code = 24 | Value in { 17; 21; 25; 29 } | 4 values</description></item>
        /// </list>
        /// </summary>
        public sealed class Complex : DistanceCode{
            /// <summary>
            /// Calculates distance code that can represent the specified <paramref name="value"/>, using set <paramref name="parameters"/>.
            /// To understand what each variable defined in the calculation means, first understand the examples in documentation for <see cref="DistanceCode.Complex"/>.
            /// <para/>
            /// 
            /// The <c>dist</c> value is constructed from a one bit followed by <c>(2 + PostfixBitCount)</c> zero bits, then incremented by the distance value minus 1.
            /// Whenever the distance value becomes large enough to add a new bit (for ex. 111 -> 1000), the <c>bucket</c> value increases.
            /// <para/>
            ///
            /// The <c>bucket</c> value splits the entire range of distance codes into sections. When the value increases, it doubles the amount of distance values a code within that section can represent.
            /// The first code has a <c>bucket</c> value equal to <c>(1 + PostfixBitCount)</c>, the value is then incremented by 1 every <c>(2 ^ (1 + PostfixBitCount))</c> codes.
            /// <para/>
            /// - For <see cref="DistanceParameters.PostfixBitCount"/> = 2, the value starts at 3 and is incremented by 1 every <c>(2 ^ (1 + 2)) = 8</c> codes (for example when going from code 23 to code 24).
            /// <para/>
            /// 
            /// The <c>prefix</c> value is a single bit that splits each bucket into 2 subsections.
            /// <para/>
            /// - For <see cref="DistanceParameters.PostfixBitCount"/> = 2, the codes 16 to 19 have <c>prefix</c> = 0, and the codes 20 to 23 have <c>prefix</c> = 1.
            /// <para/>
            /// 
            /// The <c>postfix</c> value is the bottom <c>PostfixBitCount</c> bits of <c>dist</c>, which offset the distance values.
            /// <para/>
            /// - For <see cref="DistanceParameters.PostfixBitCount"/> = 2, the codes 16 to 19 have <c>postfix</c> go from 0 to 3, then it wraps around and goes from 0 to 3 again for codes 20 to 23.
            /// <para/>
            ///
            /// Finally, everything is combined into the final code as: MSB [extracount-1][prefix][postfix] LSB.
            /// <para/>
            /// 
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/prefix.h (PrefixEncodeCopyDistance)
            /// </summary>
            public static Complex ForValue(in DistanceParameters parameters, int value){
                int directCodeCount = parameters.DirectCodeCount;
                int normalized = value - directCodeCount;

                int postfixBitCount = parameters.PostfixBitCount;
                int postfixBitMask = (1 << postfixBitCount) - 1;

                int dist = (1 << (postfixBitCount + 2)) + (normalized - 1);
                int bucket = Log2.Floor(dist) - 1;

                int prefix = (dist >> bucket) & 1;
                int postfix = dist & postfixBitMask;

                int extraBitCount = bucket - postfixBitCount;
                int baseCode = ((((extraBitCount - 1) << 1) + prefix) << postfixBitCount) + postfix;
                
                return new Complex(parameters, baseCode + directCodeCount + Last.CodeCount);
            }

            private readonly byte postfixBitCount;
            private readonly byte postfixBitMask;
            private readonly byte postfixBitValue;

            private readonly int extraBitCount;
            private readonly int topOffset;
            private readonly int bottomOffset;

            /// <summary>
            /// Since the Brotli format documentation only specifies a bunch of magic formulas, here's an attempt at an explanation.
            /// Note that the distance code we're talking about is after subtracting <see cref="DistanceCode.Last.CodeCount"/> and <see cref="DistanceParameters.DirectCodeCount"/>.
            /// <para/>
            /// Distance code is constructed from the following groups of bits: MSB [extracount-1][prefix][postfix] LSB
            /// Distance value is constructed from the following groups of bits: MSB 1[prefix][extravalue][postfix] LSB + offset
            /// <para/>
            /// <list type="bullet">
            /// <item><description>[postfix] is the bottom <see cref="DistanceParameters.PostfixBitCount"/> bits of the distance code,</description></item>
            /// <item><description>[extracount-1] is one less than the amount of extra bits that need to be read from the stream,</description></item>
            /// <item><description>[extravalue] is the value of those extra bits,</description></item>
            /// <item><description>[prefix] is a single bit which is just shoved near the beginning of the distance value</description></item>
            /// </list>
            /// <para/>
            /// Finally, the value is offset by (1 + direct code count), as all values below that are represented using <see cref="DistanceCode.Direct"/> instead.
            /// </summary>
            internal Complex(in DistanceParameters parameters, int code) : base(code){
                int directCodeCount = parameters.DirectCodeCount;
                int normalized = code - directCodeCount - Last.CodeCount;

                if (normalized < 0){
                    throw new ArgumentOutOfRangeException(nameof(code), "Complex distance codes (normalized) must be at least 0.");
                }

                this.postfixBitCount = parameters.PostfixBitCount;
                this.postfixBitMask = (byte)((1 << postfixBitCount) - 1);

                int hcode = normalized >> postfixBitCount;
                int lcode = normalized & postfixBitMask;

                this.postfixBitValue = (byte)lcode;
                this.extraBitCount = 1 + (hcode >> 1);

                this.topOffset = ((2 + (hcode & 1)) << extraBitCount) - 4;
                this.bottomOffset = 1 + lcode + directCodeCount;
            }

            [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
            public override int ExtraBits => extraBitCount;

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                if (((value - 1) & postfixBitMask) != postfixBitValue){
                    return false;
                }

                int extraBitValue = FindExtraBitValue(value);
                return extraBitValue >= 0 && extraBitValue < (1 << extraBitCount);
            }

            protected override int ReadValue(BrotliGlobalState state, IBitReader reader){
                return CalculateValue(reader.NextChunk(extraBitCount));
            }

            protected override void WriteValue(BrotliGlobalState state, int value, IBitWriter writer){
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
