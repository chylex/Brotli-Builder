﻿using System;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Markers.Serialization{
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
                markedReader.MarkStart();

                var result = deserialize(markedReader, context);

                markedReader.MarkEndTitle(title);
                return result;
            };
        }

        public static BitDeserializer<T, C> Title<T, C>(Func<C, string> title, Func<IMarkedBitReader, C, T> deserialize){
            return (reader, context) => {
                var markedReader = Cast(reader);
                markedReader.MarkStart();

                var result = deserialize(markedReader, context);

                markedReader.MarkEndTitle(title(context));
                return result;
            };
        }

        private static IMarkedBitReader Cast(IBitReader reader){
            return reader as IMarkedBitReader ?? new MarkedBitReaderDummy(reader);
        }
    }
}
