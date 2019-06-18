using BrotliLib.IO.Reader;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Reader{
    interface IMarkedBitReader : IBitReader{
        MarkerRoot MarkerRoot { get; }

        void MarkStart();
        void MarkEnd(IMarkerInfo info);
    }
}
