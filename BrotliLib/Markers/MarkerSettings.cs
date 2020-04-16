using System;
using BrotliLib.Markers.Builders;
using BrotliLib.Markers.Serialization.Reader;
using BrotliLib.Serialization;

namespace BrotliLib.Markers{
    public readonly struct MarkerSettings{
        public static MarkerSettings NoMarkers { get; } = new MarkerSettings(MarkerLevel.None, null);

        public MarkerLevel Level { get; }
        public IMarkerBuilder? Builder { get; }

        public MarkerSettings(MarkerLevel level, IMarkerBuilder? builder){
            if (level != MarkerLevel.None && builder == null){
                throw new ArgumentNullException(nameof(builder), "Marker builder must not be null if marking is enabled.");
            }

            this.Level = level;
            this.Builder = builder;
        }

        internal IMarkedBitReader CreateBitReader(BitStream bits){
            return Level != MarkerLevel.None ? new MarkedBitReader(bits.GetReader(), Level, Builder!) : (IMarkedBitReader)new MarkedBitReaderDummy(bits.GetReader());
        }
    }
}
