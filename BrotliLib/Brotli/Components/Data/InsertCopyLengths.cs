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
    public readonly struct InsertCopyLengths{
        public const int MinInsertLength = 0;
        public const int MaxInsertLength = 22594 + (1 << 24) - 1;

        public const int MinCopyLength = 2;
        public const int MaxCopyLength = 2118 + (1 << 24) - 1;

        public static readonly IntRange InsertLengthRange = new IntRange(MinInsertLength, MaxInsertLength);
        public static readonly IntRange CopyLengthRange = new IntRange(MinCopyLength, MaxCopyLength);

        internal static void CheckBounds(int insertLength, int copyLength){
            if (!InsertLengthRange.Contains(insertLength)){
                throw new ArgumentOutOfRangeException(nameof(insertLength), insertLength, "Insert length must be in the range " + InsertLengthRange + ".");
            }

            if (!CopyLengthRange.Contains(copyLength)){
                throw new ArgumentOutOfRangeException(nameof(copyLength), copyLength, "Copy length must be in the range " + CopyLengthRange + ".");
            }
        }

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

        private static readonly IntRange[] InsertCodeRanges = InsertCodeValueOffsets.Zip(InsertCodeExtraBits, IntRange.FromOffsetBitPair).ToArray();

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

        private static readonly IntRange[] CopyCodeRanges = CopyCodeValueOffsets.Zip(CopyCodeExtraBits, IntRange.FromOffsetBitPair).ToArray();

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
        /// Calculates the distance context ID used in the insert&amp;copy command.
        /// </summary>
        public int DistanceContextID => Math.Min(3, CopyLength - 2);

        /// <summary>
        /// Initializes the lengths with the provided values.
        /// </summary>
        public InsertCopyLengths(int insertLength, int copyLength){
            CheckBounds(insertLength, copyLength);

            this.InsertLength = insertLength;
            this.CopyLength = copyLength;
        }

        /// <summary>
        /// Constructs an <see cref="InsertCopyLengthCode"/> that can encode the stored lengths, and can therefore be used as context in the <see cref="Serializer"/>.
        /// </summary>
        public InsertCopyLengthCode MakeCode(DistanceCodeZeroStrategy dczStrategy = DistanceCodeZeroStrategy.Disable){
            int insertLength = InsertLength;
            int copyLength = CopyLength;

            int insertCode = Array.FindIndex(InsertCodeRanges, range => range.Contains(insertLength));
            int copyCode = Array.FindIndex(CopyCodeRanges, range => range.Contains(copyLength));

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
            return HashCode.Combine(InsertLength, CopyLength);
        }

        public override string ToString(){
            return "InsertLength = " + InsertLength + ", CopyLength = " + CopyLength;
        }

        // Serialization

        public static readonly BitDeserializer<InsertCopyLengths, InsertCopyLengthCode> Deserialize = MarkedBitDeserializer.Wrap<InsertCopyLengths, InsertCopyLengthCode>(
            (reader, context) => {
                int insertCode = context.InsertCode;
                int copyCode = context.CopyCode;

                int insertLength = reader.NextChunk(InsertCodeExtraBits[insertCode], "ILEN", value => InsertCodeValueOffsets[insertCode] + value);
                int copyLength = reader.NextChunk(CopyCodeExtraBits[copyCode], "CLEN", value => CopyCodeValueOffsets[copyCode] + value);

                return new InsertCopyLengths(insertLength, copyLength);
            }
        );

        public static readonly BitSerializer<InsertCopyLengths, InsertCopyLengthCode> Serialize = (writer, obj, context) => {
            int insertCode = context.InsertCode;
            int copyCode = context.CopyCode;
            
            int insertNormalized = obj.InsertLength - InsertCodeValueOffsets[insertCode];
            int copyNormalized = obj.CopyLength - CopyCodeValueOffsets[copyCode];

            writer.WriteChunk(InsertCodeExtraBits[insertCode], insertNormalized);
            writer.WriteChunk(CopyCodeExtraBits[copyCode], copyNormalized);
        };
    }
}
