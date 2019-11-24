using System;
using BrotliLib.Serialization;

namespace BrotliLib.Collections.Huffman{
    /// <summary>
    /// Describes a path in a binary tree. Internally stored as an <see cref="ushort"/>, which limits the maximum path length to 16.
    /// </summary>
    public readonly struct BitPath{
        public const int MaxLength = sizeof(ushort) * 8;

        public bool this[int index]{
            get{
                if (index < 0 || index >= Length){
                    throw new IndexOutOfRangeException();
                }

                return (value & (1 << (Length - 1 - index))) != 0;
            }
        }

        public byte Length { get; }
        private readonly ushort value;

        public BitPath(int value, int length){
            if (length > MaxLength){
                throw new ArgumentOutOfRangeException(nameof(length), length, "Bit path must be at most " + MaxLength + " bits long.");
            }

            this.value = (ushort)value;
            this.Length = (byte)length;
        }

        public BitPath Add(bool isOne){
            return new BitPath((value << 1) | (isOne ? 1 : 0), Length + 1);
        }

        public override bool Equals(object obj){
            return obj is BitPath path &&
                   value == path.value &&
                   Length == path.Length;
        }

        public override int GetHashCode(){
            return HashCode.Combine(value, Length);
        }

        public override string ToString(){
            var stream = new BitStream();

            for(int index = 0; index < Length; index++){
                stream.Add(this[index]);
            }

            return stream.ToString();
        }
    }
}
