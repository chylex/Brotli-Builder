using System;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Describes two parameters that modify the calculation of distances.
    /// https://tools.ietf.org/html/rfc7932#section-4
    /// </summary>
    public sealed class DistanceParameters{
        private const int MaxPostfixBitCount = 3;
        private const int MaxDirectCodeBits = 15;

        // Data

        public int DirectCodeCount => DirectCodeBits << PostfixBitCount;

        public byte PostfixBitCount { get; }
        public byte DirectCodeBits { get; }

        public DistanceParameters(byte postfixBitCount, byte directCodeBits){
            if (postfixBitCount > MaxPostfixBitCount){
                throw new ArgumentOutOfRangeException(nameof(postfixBitCount), "Postfix bit count must be at most " + MaxPostfixBitCount + ".");
            }

            if (directCodeBits > MaxDirectCodeBits){
                throw new ArgumentOutOfRangeException(nameof(directCodeBits), "Direct code bits must be at most " + MaxDirectCodeBits + ".");
            }

            this.PostfixBitCount = postfixBitCount;
            this.DirectCodeBits = directCodeBits;
        }

        public override int GetHashCode(){
            return (PostfixBitCount << 8) + DirectCodeBits;
        }

        public override bool Equals(object obj){
            return obj is DistanceParameters other && other.PostfixBitCount == PostfixBitCount && other.DirectCodeCount == DirectCodeCount;
        }

        // Serialization

        public static readonly IBitSerializer<DistanceParameters, object> Serializer = new BitSerializer<DistanceParameters, object>(
            fromBits: (reader, context) => {
                int postfixBitCount = reader.NextChunk(2);
                int directCodeBits = reader.NextChunk(4);

                return new DistanceParameters((byte)postfixBitCount, (byte)directCodeBits);
            },

            toBits: (writer, obj, context) => {
                writer.WriteChunk(2, obj.PostfixBitCount);
                writer.WriteChunk(4, obj.DirectCodeBits);
            }
        );
    }
}
