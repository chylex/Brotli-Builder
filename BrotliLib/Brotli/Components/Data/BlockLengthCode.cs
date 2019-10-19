using System;
using System.Linq;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.IO;
using BrotliLib.IO.Reader;
using BrotliLib.IO.Writer;
using BrotliLib.Numbers;
using BlockLengthCodeTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.BlockLengthCode>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> entry used to calculate the length of a block in a block-switch command.
    /// https://tools.ietf.org/html/rfc7932#section-6
    /// </summary>
    public sealed class BlockLengthCode : IComparable<BlockLengthCode>{
        public const int MinLength = 1;
        public const int MaxLength = 16625 + (1 << 24) - 1;

        public static readonly AlphabetSize AlphabetSize = new AlphabetSize(26);
        public static readonly BlockLengthCodeTree.Context TreeContext = new BlockLengthCodeTree.Context(AlphabetSize, value => new BlockLengthCode(value), symbol => symbol.Code);

        // Block length tables

        private static readonly int[] BlockLengthExtraBits = {
            2, 2, 2,  2,  3,  3,  3,  3, 4,
            4, 4, 4,  5,  5,  5,  5,  6, 6,
            7, 8, 9, 10, 11, 12, 13, 24,
        };

        private static readonly int[] BlockLengthOffsets = {
              1,   5,   9,   13,   17,   25,   33,    41,  49,
             65,  81,  97,  113,  145,  177,  209,   241, 305,
            369, 497, 753, 1265, 2289, 4337, 8433, 16625,
        };

        private static readonly IntRange[] BlockLengthRanges = BlockLengthOffsets.Zip(BlockLengthExtraBits, IntRange.FromOffsetBitPair).ToArray();

        public static BlockLengthCode MakeCode(int length){
            return new BlockLengthCode(Array.FindIndex(BlockLengthRanges, range => range.Contains(length)));
        }

        // Data

        public int Code { get; }

        public BlockLengthCode(int code){
            this.Code = code;
        }

        public bool CanEncodeValue(int value){
            return BlockLengthRanges[Code].Contains(value);
        }

        private int ReadValue(IBitReader reader){
            return BlockLengthOffsets[Code] + reader.NextChunk(BlockLengthExtraBits[Code]);
        }

        private void WriteValue(IBitWriter writer, int value){
            writer.WriteChunk(BlockLengthExtraBits[Code], value - BlockLengthOffsets[Code]);
        }

        public int CompareTo(BlockLengthCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public override bool Equals(object obj){
            return obj is BlockLengthCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Code);
        }

        public override string ToString(){
            return "Code = " + Code;
        }

        // Serialization

        internal static readonly BitDeserializer<int, BlockLengthCode> Deserialize = (reader, context) => context.ReadValue(reader);
        internal static readonly BitSerializer<int, BlockLengthCode> Serialize = (writer, obj, context) => context.WriteValue(writer, obj);
    }
}
