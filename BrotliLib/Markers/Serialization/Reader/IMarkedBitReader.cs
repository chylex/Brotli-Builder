using System;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Markers.Serialization.Reader{
    interface IMarkedBitReader : IBitReader{
        MarkerRoot MarkerRoot { get; }

        void MarkStart();
        void MarkEnd(Func<IMarkerInfo> info);

        T MarkCall<T>(Func<T> supplier, Func<T, IMarkerInfo> marker);
    }
}
