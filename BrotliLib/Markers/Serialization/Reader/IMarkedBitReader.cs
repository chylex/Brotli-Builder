using BrotliLib.Markers.Types;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Markers.Serialization.Reader{
    interface IMarkedBitReader : IBitReader{
        MarkerRoot MarkerRoot { get; }
        MarkerLevel MarkerLevel { get; }

        void MarkStart();
        void MarkEnd(IMarkerInfo info);
        void MarkEndTitle(string title);
        void MarkEndValue(string name, object value);
    }
}
