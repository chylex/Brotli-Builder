using System;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using InsertCopyTree = BrotliLib.Brotli.Components.Header.HuffmanTree<BrotliLib.Brotli.Components.Data.InsertCopyLengthCode>;

namespace BrotliLib.Brotli.Components.Data{
    /// <summary>
    /// Describes a <see cref="HuffmanTree{T}"/> entry used to calculate lengths of an insert&amp;copy code.
    /// https://tools.ietf.org/html/rfc7932#section-5
    /// </summary>
    public sealed class InsertCopyLengthCode : IComparable<InsertCopyLengthCode>{
        public static readonly AlphabetSize AlphabetSize = new AlphabetSize(704);
        public static readonly InsertCopyTree.Context TreeContext = new InsertCopyTree.Context(AlphabetSize, value => new InsertCopyLengthCode(value), symbol => symbol.CompactedCode);

        // Cell offsets

        private static readonly int[] InsertCellOffsets = {
            0, 0, 0, 0, 8, 8, 0, 16, 8, 16, 16
        };
        
        private static readonly int[] CopyCellOffsets = {
            0, 8, 0, 8, 0, 8, 16, 0, 16, 8, 16
        };

        // Data

        /// <summary>
        /// Combined <see cref="InsertCode"/> and <see cref="CopyCode"/> values into a single value from the insert&amp;copy alphabet.
        /// </summary>
        public int CompactedCode { get; }

        /// <summary>
        /// Code used to determine the <see cref="InsertCopyLengths.InsertLength"/> value.
        /// </summary>
        public int InsertCode { get; }

        /// <summary>
        /// Code used to determine the <see cref="InsertCopyLengths.CopyLength"/> value.
        /// </summary>
        public int CopyCode { get; }
        
        /// <summary>
        /// Whether to skip reading the <see cref="DistanceCode"/>, and use distance code 0 instead.
        /// </summary>
        public bool UseDistanceCodeZero { get; }

        /// <summary>
        /// Initializes the code with a value from the insert&amp;copy alphabet.
        /// </summary>
        public InsertCopyLengthCode(int compactedCode){
            if (compactedCode < 0 || compactedCode >= AlphabetSize.SymbolCount){
                throw new ArgumentOutOfRangeException(nameof(compactedCode), "Compacted insert&copy length code must be in range [0; " + AlphabetSize.SymbolCount + ").");
            }

            int cell = compactedCode / 64;

            this.CompactedCode = compactedCode;
            this.InsertCode = InsertCellOffsets[cell] + ((compactedCode >> 3) & 0b111);
            this.CopyCode = CopyCellOffsets[cell] + (compactedCode & 0b111);
            this.UseDistanceCodeZero = cell < 2;
        }

        public int CompareTo(InsertCopyLengthCode other){
            return CompactedCode.CompareTo(other.CompactedCode);
        }

        public override int GetHashCode(){
            return CompactedCode;
        }

        public override bool Equals(object obj){
            return obj is InsertCopyLengthCode other && other.CompactedCode == CompactedCode;
        }

        public override string ToString(){
            return "{ Compacted = " + CompactedCode + " }";
        }
    }
}
