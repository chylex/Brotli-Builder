using System;
using System.Linq;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Numbers;
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

        private static readonly (IntRange i, IntRange c)[] PairedCellOffsets = InsertCellOffsets.Zip(CopyCellOffsets, (i, c) => (new IntRange(i, i + 7), new IntRange(c, c + 7))).ToArray();

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
                throw new ArgumentOutOfRangeException(nameof(compactedCode), "Compacted insert&copy length code must be in the range [0; " + AlphabetSize.SymbolCount + ").");
            }
            
            int cell = compactedCode / 64;

            this.CompactedCode = compactedCode;
            this.InsertCode = InsertCellOffsets[cell] + ((compactedCode >> 3) & 0b111);
            this.CopyCode = CopyCellOffsets[cell] + (compactedCode & 0b111);
            this.UseDistanceCodeZero = cell < 2;
        }

        /// <summary>
        /// Initializes the code with the concrete insert and copy codes, and the flag which determines whether to use an implied distance code zero.
        /// </summary>
        public InsertCopyLengthCode(int insertCode, int copyCode, DistanceCodeZeroStrategy dczStrategy){
            if (insertCode < 0 || insertCode > 23){
                throw new ArgumentOutOfRangeException(nameof(insertCode), "Insert code must be in the range [0; 23].");
            }

            if (copyCode < 0 || copyCode > 23){
                throw new ArgumentOutOfRangeException(nameof(copyCode), "Copy code must be in the range [0; 23].");
            }

            bool useDistanceCodeZero = dczStrategy.Determine(insertCode, copyCode);

            int startCellIndex = useDistanceCodeZero ? 0 : 2;
            int cell = Array.FindIndex(PairedCellOffsets, startCellIndex, pair => pair.i.Contains(insertCode) && pair.c.Contains(copyCode));

            this.CompactedCode = (64 * cell) + ((insertCode & 0b111) << 3) | (copyCode & 0b111);
            this.InsertCode = insertCode;
            this.CopyCode = copyCode;
            this.UseDistanceCodeZero = useDistanceCodeZero;
        }

        public int CompareTo(InsertCopyLengthCode other){
            return CompactedCode.CompareTo(other.CompactedCode);
        }

        // Object

        public override bool Equals(object obj){
            return obj is InsertCopyLengthCode code &&
                   CompactedCode == code.CompactedCode;
        }

        public override int GetHashCode(){
            return HashCode.Combine(CompactedCode);
        }

        public override string ToString(){
            return "Code = " + CompactedCode + " (InsertCode = " + InsertCode + ", CopyCode = " + CopyCode + ", DistanceCodeZero = " + UseDistanceCodeZero + ")";
        }
    }
}
