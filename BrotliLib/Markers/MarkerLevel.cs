using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;

namespace BrotliLib.Markers{
    public enum MarkerLevel{
        None,
        Simple,
        Verbose
    }

    internal static class MarkerLevels{
        public static IMarkedBitReader CreateBitReader(this MarkerLevel markerLevel, BitStream bits){
            return markerLevel != MarkerLevel.None ? new MarkedBitReader(bits.GetReader(), markerLevel) : (IMarkedBitReader)new MarkedBitReaderDummy(bits.GetReader());
        }
    }
}
