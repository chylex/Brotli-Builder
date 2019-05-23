﻿using System;
using System.Linq;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.IO;
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

        private static readonly Range[] BlockLengthRanges = BlockLengthOffsets.Zip(BlockLengthExtraBits, Range.FromOffsetBitPair).ToArray();

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

        private int ReadValue(BitReader reader){
            return BlockLengthOffsets[Code] + reader.NextChunk(BlockLengthExtraBits[Code]);
        }

        private void WriteValue(BitWriter writer, int value){
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
            unchecked{
                return -434485196 + Code.GetHashCode();
            }
        }

        public override string ToString(){
            return "Code = " + Code;
        }

        // Serialization

        internal static readonly IBitSerializer<int, BlockLengthCode> Serializer = new BitSerializer<int, BlockLengthCode>(
            fromBits: (reader, context) => context.ReadValue(reader),
            toBits: (writer, obj, context) => context.WriteValue(writer, obj)
        ); 
    }
}
