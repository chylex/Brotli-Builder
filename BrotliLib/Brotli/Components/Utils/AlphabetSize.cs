using System;

namespace BrotliLib.Brotli.Components.Utils{
    /// <summary>
    /// Defines properties of a symbol alphabet.
    /// </summary>
    public struct AlphabetSize{
        /// <summary>
        /// Returns the minimum amount of bits required to represent every symbol in the alphabet.
        /// </summary>
        public int BitsPerSymbol{
            get{
                int size = SymbolCount - 1;
                int bitsPerSymbol = 0;
            
                while(size > 0){
                    size >>= 1;
                    ++bitsPerSymbol;
                }

                return bitsPerSymbol;
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
    }
}
