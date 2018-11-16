using System;

namespace BrotliLib.IO{
    /// <summary>
    /// Provides a generic way of converting an object to or from a <see cref="BitStream"/> representation.
    /// </summary>
    /// <typeparam name="T">Type to convert.</typeparam>
    /// <typeparam name="C">Type used to provide context.</typeparam>
    public interface IBitSerializer<T, C>{
        T FromBits(BitReader reader, C context);
        void ToBits(BitWriter writer, T obj, C context);
    }
    
    /// <summary>
    /// Utility class to implement <see cref="IBitSerializer{T,C}"/> using anonymous functions.
    /// </summary>
    public sealed class BitSerializer<T, C> : IBitSerializer<T, C>{
        private readonly Func<BitReader, C, T> fromBits;
        private readonly Action<BitWriter, T, C> toBits;

        public BitSerializer(Func<BitReader, C, T> fromBits, Action<BitWriter, T, C> toBits){
            this.fromBits = fromBits;
            this.toBits = toBits;
        }

        public T FromBits(BitReader reader, C context) => fromBits(reader, context);
        public void ToBits(BitWriter writer, T obj, C context) => toBits(writer, obj, context);
    }
}
