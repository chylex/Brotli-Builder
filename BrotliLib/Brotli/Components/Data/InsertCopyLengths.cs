using System;
using System.Linq;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.IO;
using BrotliLib.Numbers;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Represents the exact insert and copy length of a single insert&amp;copy command.
    /// https://tools.ietf.org/html/rfc7932#section-5
    /// </summary>
    public sealed class InsertCopyLengths{
        public static int MinimumCopyLength => CopyCodeValueOffsets[0];

        // Insert code tables

        private static readonly int[] InsertCodeExtraBits = {
            0, 0, 0, 0,  0,  0,  1,  1,
            2, 2, 3, 3,  4,  4,  5,  5,
            6, 7, 8, 9, 10, 12, 14, 24,
        };

        private static readonly int[] InsertCodeValueOffsets = {
              0,   1,   2,   3,    4,    5,    6,     8,
             10,  14,  18,  26,   34,   50,   66,    98,
            130, 194, 322, 578, 1090, 2114, 6210, 22594,
        };

        private static readonly Range[] InsertCodeRanges = InsertCodeValueOffsets.Zip(InsertCodeExtraBits, Range.FromOffsetBitPair).ToArray();

        // Copy code tables

        private static readonly int[] CopyCodeExtraBits = {
            0, 0, 0, 0, 0, 0,  0,  0,
            1, 1, 2, 2, 3, 3,  4,  4,
            5, 5, 6, 7, 8, 9, 10, 24,
        };

        private static readonly int[] CopyCodeValueOffsets = {
             2,   3,   4,   5,   6,   7,    8,    9,
            10,  12,  14,  18,  22,  30,   38,   54,
            70, 102, 134, 198, 326, 582, 1094, 2118,
        };

        private static readonly Range[] CopyCodeRanges = CopyCodeValueOffsets.Zip(CopyCodeExtraBits, Range.FromOffsetBitPair).ToArray();

        // Data

        /// <summary>
        /// Amount of literals (bytes) that follow immediately after the <see cref="InsertCopyLengthCode"/> in the stream.
        /// </summary>
        public int InsertLength { get; }

        /// <summary>
        /// Either the amount of bytes to copy from a previous point in the stream, or the length of a word in the static dictionary.
        /// </summary>
        public int CopyLength { get; }

        /// <summary>
        /// Initializes the lengths with the provided values.
        /// </summary>
        public InsertCopyLengths(int insertLength, int copyLength){
            this.InsertLength = insertLength;
            this.CopyLength = copyLength;
        }

        /// <summary>
        /// Constructs an <see cref="InsertCopyLengthCode"/> that can encode the stored lengths, and can therefore be used as context in the <see cref="Serializer"/>.
        /// </summary>
        public InsertCopyLengthCode MakeCode(DistanceCodeZeroStrategy dczStrategy){
            int insertCode = Array.FindIndex(InsertCodeRanges, range => range.Contains(InsertLength));
            int copyCode = Array.FindIndex(CopyCodeRanges, range => range.Contains(CopyLength));

            return new InsertCopyLengthCode(insertCode, copyCode, dczStrategy);
        }

        /// <summary>
        /// Returns true if the provided <paramref name="code"/> can encode the stored lengths, and can therefore be used as context in the <see cref="Serializer"/>.
        /// </summary>
        public bool CanEncodeUsing(InsertCopyLengthCode code){
            return (
                InsertCodeRanges[code.InsertCode].Contains(InsertLength) &&
                CopyCodeRanges[code.CopyCode].Contains(CopyLength)
            );
        }

        // Object

        public override bool Equals(object obj){
            return obj is InsertCopyLengths lengths &&
                   InsertLength == lengths.InsertLength &&
                   CopyLength == lengths.CopyLength;
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -1099318212;
                hashCode = hashCode * -1521134295 + InsertLength.GetHashCode();
                hashCode = hashCode * -1521134295 + CopyLength.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString(){
            return "InsertLength = " + InsertLength + ", CopyLength = " + CopyLength;
        }

        // Serialization

        public static readonly IBitSerializer<InsertCopyLengths, InsertCopyLengthCode> Serializer = new MarkedBitSerializer<InsertCopyLengths, InsertCopyLengthCode>(
            fromBits: (reader, context) => {
                int insertCode = context.InsertCode;
                int copyCode = context.CopyCode;

                int insertLength = reader.NextChunk(InsertCodeExtraBits[insertCode], "ILEN", value => InsertCodeValueOffsets[insertCode] + value);
                int copyLength = reader.NextChunk(CopyCodeExtraBits[copyCode], "CLEN", value => CopyCodeValueOffsets[copyCode] + value);

                return new InsertCopyLengths(insertLength, copyLength);
            },

            toBits: (writer, obj, context) => {
                int insertCode = context.InsertCode;
                int copyCode = context.CopyCode;
                
                int insertNormalized = obj.InsertLength - InsertCodeValueOffsets[insertCode];
                int copyNormalized = obj.CopyLength - CopyCodeValueOffsets[copyCode];

                writer.WriteChunk(InsertCodeExtraBits[insertCode], insertNormalized);
                writer.WriteChunk(CopyCodeExtraBits[copyCode], copyNormalized);
            }
        );
    }
}
