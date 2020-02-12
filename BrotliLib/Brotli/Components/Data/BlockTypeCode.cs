using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Numbers;
using BlockTypeCodeTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.BlockTypeCode>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> symbol used to determine the block type in a block-switch command.
    /// https://tools.ietf.org/html/rfc7932#section-6
    /// </summary>
    public sealed class BlockTypeCode : IComparable<BlockTypeCode>{
        public static BlockTypeCodeTree.Context GetTreeContext(int count){
            return new BlockTypeCodeTree.Context(new AlphabetSize(count + 2), value => new BlockTypeCode(value), symbol => symbol.Code);
        }

        // Data

        public int Code { get; }

        public BlockTypeCode(int code){
            this.Code = code;
        }

        public int CompareTo(BlockTypeCode other){
            return Code.CompareTo(other.Code);
        }

        // Object

        public override bool Equals(object obj){
            return obj is BlockTypeCode code &&
                   Code == code.Code;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Code);
        }

        public override string ToString(){
            return "Code = " + Code;
        }
    }
}
