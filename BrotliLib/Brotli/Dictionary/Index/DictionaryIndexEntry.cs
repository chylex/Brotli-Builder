using System;

namespace BrotliLib.Brotli.Dictionary.Index{
    public readonly struct DictionaryIndexEntry{
        public int Packed { get; }
        public byte CopyLength { get; }
        public byte OutputLength { get; }

        public DictionaryIndexEntry(int packed, int copyLength, int outputLength){
            this.Packed = packed;
            this.CopyLength = (byte)copyLength;
            this.OutputLength = (byte)outputLength;
        }

        public override bool Equals(object obj){
            return obj is DictionaryIndexEntry entry &&
                   Packed == entry.Packed &&
                   CopyLength == entry.CopyLength &&
                   OutputLength == entry.OutputLength;
        }

        public override int GetHashCode(){
            return HashCode.Combine(Packed, CopyLength, OutputLength);
        }

        public override string ToString(){
            return "Packed = " + Packed + ", CopyLength = " + CopyLength + ", OutputLength = " + OutputLength;
        }
    }
}
