using System;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Markers{
    /// <inheritdoc />
    /// <summary>
    /// Extends <see cref="IBitSerializer{T, C}"/> with ability to use a <see cref="MarkedBitReader"/> for deserialization.
    /// </summary>
    sealed class MarkedBitSerializer<T, C> : IBitSerializer<T, C>{
        private readonly Func<MarkedBitReader, C, T> fromBits;
        private readonly Action<BitWriter, T, C> toBits;

        public MarkedBitSerializer(Func<MarkedBitReader, C, T> fromBits, Action<BitWriter, T, C> toBits){
            this.fromBits = fromBits;
            this.toBits = toBits;
        }

        public MarkedBitSerializer(string markerTitle, Func<MarkedBitReader, C, T> fromBits, Action<BitWriter, T, C> toBits) : this(
            fromBits: (reader, context) => reader.MarkTitle(markerTitle, () => fromBits(reader, context)),
            toBits: toBits
        ){}

        public MarkedBitSerializer(Func<C, string> markerTitle, Func<MarkedBitReader, C, T> fromBits, Action<BitWriter, T, C> toBits) : this(
            fromBits: (reader, context) => reader.MarkTitle(markerTitle(context), () => fromBits(reader, context)),
            toBits: toBits
        ){}
        
        public T FromBits(BitReader reader, C context) => fromBits((reader as MarkedBitReader) ?? new MarkedBitReader.Dummy(reader), context);
        public void ToBits(BitWriter writer, T obj, C context) => toBits(writer, obj, context);
    }
}
