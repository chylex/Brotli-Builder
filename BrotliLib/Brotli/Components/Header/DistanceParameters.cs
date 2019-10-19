using System;
using BrotliLib.Brotli.Markers;
using BrotliLib.IO;
using BrotliLib.Numbers;

namespace BrotliLib.Brotli.Components.Header{
    /// <summary>
    /// Describes two parameters that modify the calculation of distances.
    /// https://tools.ietf.org/html/rfc7932#section-4
    /// </summary>
    public readonly struct DistanceParameters{
        public const int MaxPostfixBitCount = 3;
        public const int MaxDirectCodeBits = 15;

        public static readonly int MaxDirectCodeCount = new DistanceParameters(MaxPostfixBitCount, MaxDirectCodeBits).DirectCodeCount;

        public static readonly DistanceParameters NoDirectCodes = new DistanceParameters(0, 0);

        // Data

        public int DirectCodeCount => DirectCodeBits << PostfixBitCount;
        public AlphabetSize AlphabetSize => new AlphabetSize(16 + DirectCodeCount + (48 << PostfixBitCount));

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

        // Object

        public override bool Equals(object obj){
            return obj is DistanceParameters parameters &&
                   PostfixBitCount == parameters.PostfixBitCount &&
                   DirectCodeBits == parameters.DirectCodeBits;
        }

        public override int GetHashCode(){
            return HashCode.Combine(PostfixBitCount, DirectCodeBits);
        }

        public override string ToString(){
            return "PostfixBitCount = " + PostfixBitCount + ", DirectCodeBits = " + DirectCodeBits + " (DirectCodeCount = " + DirectCodeCount + ", AlphabetSize = { " + AlphabetSize + " })";
        }

        // Serialization

        public static readonly BitDeserializer<DistanceParameters, NoContext> Deserialize = MarkedBitDeserializer.Title<DistanceParameters, NoContext>(
            "Distance Parameters",

            (reader, context) => {
                int postfixBitCount = reader.NextChunk(2, "NPOSTFIX");
                int directCodeBits = reader.NextChunk(4, "NDIRECT >> 4");

                return new DistanceParameters((byte)postfixBitCount, (byte)directCodeBits);
            }
        );

        public static readonly BitSerializer<DistanceParameters, NoContext> Serialize = (writer, obj, context) => {
            writer.WriteChunk(2, obj.PostfixBitCount);
            writer.WriteChunk(4, obj.DirectCodeBits);
        };
    }
}
