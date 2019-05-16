using System;
using System.Linq;
using System.Text;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.State;
using BrotliLib.IO;
using DistanceTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.DistanceCode>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> entry used to calculate the distance in an insert&amp;copy command.
    /// https://tools.ietf.org/html/rfc7932#section-4
    /// </summary>
    public abstract class DistanceCode : IComparable<DistanceCode>{
        public static DistanceTree.Context GenerateTreeContext(DistanceParameters parameters){
            return new DistanceTree.Context(parameters.AlphabetSize, value => Create(parameters, value), symbol => symbol.Code);
        }

        public static DistanceCode Zero => Last.Codes[0];
        private static readonly int DirectCodeOffset = Last.Codes.Length - 1;

        // Data

        public int Code { get; }
        
        protected DistanceCode(int code){
            this.Code = code;
        }

        internal DistanceContext MakeContext(BrotliGlobalState state){
            return new DistanceContext(this, state);
        }

        public abstract bool CanEncodeValue(BrotliGlobalState state, int value);
        
        protected abstract int ReadValue(BrotliGlobalState state, BitReader reader);
        protected abstract void WriteValue(BrotliGlobalState state, int value, BitWriter writer);

        public int CompareTo(DistanceCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public override bool Equals(object obj){
            return obj is DistanceCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            unchecked{
                return -434485196 + Code.GetHashCode();
            }
        }

        public override string ToString(){
            return "Code = " + Code + " | " + GetType().Name;
        }

        // Types

        internal static DistanceCode ForValue(DistanceParameters parameters, BrotliGlobalState state, int value){
            var lastDistance = LastDistances.FirstOrDefault(code => code.CanEncodeValue(state, value));

            if (lastDistance != null){
                return lastDistance;
            }
            
            if (value <= parameters.DirectCodeCount){
                return new Direct(value + DirectCodeOffset);
            }
            
            // TODO deuglify this later
            return Enumerable.Range(Last.Codes.Length + parameters.DirectCodeCount, 100000 /* TODO random */)
                             .Select(code => new Complex(parameters, code))
                             .First(code => code.CanEncodeValue(state, value));
        }

        private static DistanceCode Create(DistanceParameters parameters, int code){
            if (code < Last.Codes.Length){
                return Last.Codes[code];
            }
            
            int normalized = code - DirectCodeOffset;

            if (normalized <= parameters.DirectCodeCount){
                return new Direct(code);
            }
            else{
                return new Complex(parameters, code);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents a distance code which uses the global ring buffer of last distances.
        /// </summary>
        private sealed class Last : DistanceCode{
            private static readonly byte[] BufferIndexes = {
                3, 2, 1, 0,
                3, 3, 3, 3, 3, 3,
                2, 2, 2, 2, 2, 2
            };
        
            private static readonly sbyte[] BufferValueOffsets = {
                0, 0, 0, 0,
                -1, 1, -2, 2, -3, 3,
                -1, 1, -2, 2, -3, 3
            };

            public static readonly DistanceCode[] Codes = Enumerable.Range(0, BufferIndexes.Length)
                                                                    .Select(code => new Last(code))
                                                                    .ToArray<DistanceCode>();

            private readonly byte index;
            private readonly sbyte offset;

            private Last(int code) : base(code){
                this.index = BufferIndexes[code];
                this.offset = BufferValueOffsets[code];
            }

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                return value == state.DistanceBuffer[index] + offset;
            }

            protected override int ReadValue(BrotliGlobalState state, BitReader reader){
                return state.DistanceBuffer[index] + offset;
            }

            protected override void WriteValue(BrotliGlobalState state, int value, BitWriter writer){
                // no extra bits
            }

            public override string ToString(){
                return base.ToString() + " | Value = buffer[" + index + "] " + (offset < 0 ? "- " + (-offset) : "+ " + offset);
            }
        }

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
        
        // Context

        internal class DistanceContext{
            private readonly DistanceCode code;
            private readonly BrotliGlobalState state;

            public DistanceContext(DistanceCode code, BrotliGlobalState state){
                this.code = code;
                this.state = state;
            }

            internal DistanceInfo Read(BitReader reader){
                if (code.Code == 0){
                    return DistanceInfo.ExplicitCodeZero;
                }
                else{
                    return (DistanceInfo)code.ReadValue(state, reader);
                }
            }

            internal void Write(BitWriter writer, DistanceInfo info){
                if (info != DistanceInfo.ExplicitCodeZero){
                    code.WriteValue(state, info.GetValue(state), writer);
                }
            }
        }

        // Serialization

        internal static readonly IBitSerializer<DistanceInfo, DistanceContext> Serializer = new BitSerializer<DistanceInfo, DistanceContext>(
            fromBits: (reader, context) => context.Read(reader),
            toBits: (writer, obj, context) => context.Write(writer, obj)
        );
    }
}
