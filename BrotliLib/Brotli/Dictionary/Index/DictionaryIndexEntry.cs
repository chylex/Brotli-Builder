namespace BrotliLib.Brotli.Dictionary.Index{
    public readonly struct DictionaryIndexEntry{
        public int Length { get; }
        public int Packed { get; }

        public DictionaryIndexEntry(int length, int packed){
            this.Length = length;
            this.Packed = packed;
        }

        public void Deconstruct(out int length, out int packed){
            length = this.Length;
            packed = this.Packed;
        }

        public override bool Equals(object obj){
            return obj is DictionaryIndexEntry entry &&
                   Length == entry.Length &&
                   Packed == entry.Packed;
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -295785090;
                hashCode = hashCode * -1521134295 + Length.GetHashCode();
                hashCode = hashCode * -1521134295 + Packed.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString(){
            return "Length = " + Length + ", Packed = " + Packed;
        }
    }
}
