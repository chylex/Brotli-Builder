using System;
using BrotliLib.Brotli.Markers.Reader;
using BrotliLib.IO;
using BrotliLib.IO.Reader;

namespace BrotliLib.Brotli.Markers{
    /// <summary>
    /// Extends <see cref="BitDeserializer{T, C}"/> with ability to use an <see cref="IMarkedBitReader"/> for deserialization.
    /// </summary>
    static class MarkedBitDeserializer{
        public static BitDeserializer<T, C> Wrap<T, C>(Func<IMarkedBitReader, C, T> deserialize){
            return (reader, context) => {
                var markedReader = Cast(reader);
                return deserialize(markedReader, context);
            };
        }

        public static BitDeserializer<T, C> Title<T, C>(string title, Func<IMarkedBitReader, C, T> deserialize){
            return (reader, context) => {
                var markedReader = Cast(reader);
                return markedReader.MarkTitle(title, () => deserialize(markedReader, context));
            };
        }

        public static BitDeserializer<T, C> Title<T, C>(Func<C, string> title, Func<IMarkedBitReader, C, T> deserialize){
            return (reader, context) => {
                var markedReader = Cast(reader);
                return markedReader.MarkTitle(title(context), () => deserialize(markedReader, context));
            };
        }

        private static IMarkedBitReader Cast(IBitReader reader){
            return reader as IMarkedBitReader ?? new MarkedBitReaderDummy(reader);
        }
    }
}
