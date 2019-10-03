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
            unchecked{
                var hashCode = -1193651551;
                hashCode = hashCode * -1521134295 + Packed.GetHashCode();
                hashCode = hashCode * -1521134295 + CopyLength.GetHashCode();
                hashCode = hashCode * -1521134295 + OutputLength.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString(){
            return "Packed = " + Packed + ", CopyLength = " + CopyLength + ", OutputLength = " + OutputLength;
        }
    }
}
