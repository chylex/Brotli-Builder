using System;

namespace BrotliLib.Brotli.Components.Utils{
    /// <summary>
    /// Defines properties of a symbol alphabet.
    /// </summary>
    public readonly struct AlphabetSize{
        /// <summary>
        /// Returns the minimum amount of bits required to represent every symbol in the alphabet.
        /// </summary>
        public byte BitsPerSymbol{
            get{
                int size = SymbolCount - 1;
                byte bitsPerSymbol = 0;
            
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

        // Object

        public override bool Equals(object obj){
            var size = obj as AlphabetSize?;
            return SymbolCount == size?.SymbolCount;
        }

        public override int GetHashCode(){
            unchecked{
                return -1048165080 + SymbolCount.GetHashCode();
            }
        }

        public override string ToString(){
            return "SymbolCount = " + SymbolCount + " (BitsPerSymbol = " + BitsPerSymbol + ")";
        }
    }
}
