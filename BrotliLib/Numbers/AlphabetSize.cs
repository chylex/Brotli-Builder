using System;

namespace BrotliLib.Numbers{
    /// <summary>
    /// Defines properties of a symbol alphabet.
    /// </summary>
    public readonly struct AlphabetSize{
        /// <summary>
        /// Returns the minimum amount of bits required to represent every symbol in the alphabet.
        /// </summary>
        public int BitsPerSymbol{
            get{
                if (SymbolCount <= 1){
                    return 0;
                }

                return Log2.Floor(SymbolCount - 1) + 1;
            }
        }

        /// <summary>
        /// Amount of symbols in the alphabet.
        /// </summary>
        public int SymbolCount { get; }
        
        public AlphabetSize(int symbolCount){
            if (symbolCount < 0){
                throw new ArgumentOutOfRangeException(nameof(symbolCount), "Alphabet cannot have a negative amount of symbols.");
            }

            this.SymbolCount = symbolCount;
        }

        // Object

        public override bool Equals(object obj){
            var size = obj as AlphabetSize?;
            return SymbolCount == size?.SymbolCount;
        }

        public override int GetHashCode(){
            return HashCode.Combine(SymbolCount);
        }

        public override string ToString(){
            return "SymbolCount = " + SymbolCount + " (BitsPerSymbol = " + BitsPerSymbol + ")";
        }
    }
}
