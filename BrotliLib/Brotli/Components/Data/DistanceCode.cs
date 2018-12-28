using System;
using BrotliLib.Brotli.Components.Header;
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

        // Distance buffer reference tables

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

        // Data

        public int Code { get; }
        
        protected DistanceCode(int code){
            this.Code = code;
        }

        public DistanceContext MakeContext(BrotliGlobalState state){
            return new DistanceContext(this, state);
        }

        public abstract bool CanEncodeValue(BrotliGlobalState state, int value);
        
        protected abstract int ReadValue(BrotliGlobalState state, BitReader reader);
        protected abstract void WriteValue(BrotliGlobalState state, int value, BitWriter writer);

        public int CompareTo(DistanceCode other){
            return Code.CompareTo(other.Code);
        }

        public override int GetHashCode(){
            return Code;
        }

        public override bool Equals(object obj){
            return obj is DistanceCode other && other.Code == Code;
        }

        public override string ToString(){
            return "{ Code = " + Code + " }";
        }

        // Types

        private static DistanceCode Create(DistanceParameters parameters, int code){
            if (code < BufferIndexes.Length){
                return new LastDistance(code);
            }
            
            int normalized = code - BufferIndexes.Length;

            if (normalized < parameters.DirectCodeCount){
                return new DirectDistance(code);
            }
            else{
                return new ComplexDistance(parameters, code);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents a distance code which uses the global ring buffer of last distances.
        /// </summary>
        private class LastDistance : DistanceCode{
            private readonly byte index;
            private readonly sbyte offset;

            public LastDistance(int code) : base(code){
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
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents a direct distance code, which is converted to a distance value between 1 and <see cref="DistanceParameters.DirectCodeCount"/> (both inclusive).
        /// </summary>
        private class DirectDistance : DistanceCode{
            private readonly int encodedValue;

            public DirectDistance(int code) : base(code){
                this.encodedValue = 1 + code - BufferIndexes.Length;
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
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents a distance code which uses additional bits from the stream to calculate the distance value.
        /// </summary>
        private class ComplexDistance : DistanceCode{
            private readonly int postfixBitCount;
            private readonly int extraBitCount;
            private readonly int topOffset;
            private readonly int bottomOffset;

            public ComplexDistance(DistanceParameters parameters, int code) : base(code){
                int directCodeCount = parameters.DirectCodeCount;
                this.postfixBitCount = parameters.PostfixBitCount;

                int normalized = code - directCodeCount - BufferIndexes.Length;
                
                int hcode = normalized >> postfixBitCount;
                int lcode = normalized & (1 << postfixBitCount) - 1;
                
                this.extraBitCount = 1 + (normalized >> (postfixBitCount + 1));
                this.topOffset = ((2 + (hcode & 1)) << extraBitCount) - 4;
                this.bottomOffset = 1 + lcode + directCodeCount;
            }

            public override bool CanEncodeValue(BrotliGlobalState state, int value){
                int extraBitValue = FindExtraBitValue(value);
                return extraBitValue >= 0 && extraBitValue < (1 << extraBitCount);
            }

            protected override int ReadValue(BrotliGlobalState state, BitReader reader){
                int extraBitValue = reader.NextChunk(extraBitCount);
                int topValue = extraBitValue + topOffset;

                return (topValue << postfixBitCount) + bottomOffset;
            }

            protected override void WriteValue(BrotliGlobalState state, int value, BitWriter writer){
                writer.WriteChunk(extraBitCount, FindExtraBitValue(value));
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
        }
        
        // Context

        public class DistanceContext{
            public DistanceCode Code { get; }
            public BrotliGlobalState State { get; }

            public DistanceContext(DistanceCode code, BrotliGlobalState state){
                this.Code = code;
                this.State = state;
            }
        }

        // Serialization

        internal static readonly IBitSerializer<int, DistanceContext> Serializer = new BitSerializer<int, DistanceContext>(
            fromBits: (reader, context) => context.Code.ReadValue(context.State, reader),
            toBits: (writer, obj, context) => context.Code.WriteValue(context.State, obj, writer)
        );
    }
}
