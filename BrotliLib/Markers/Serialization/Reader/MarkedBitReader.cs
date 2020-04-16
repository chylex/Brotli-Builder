using BrotliLib.Markers.Builders;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Markers.Serialization.Reader{
    class MarkedBitReader : IMarkedBitReader{
        public MarkerLevel MarkerLevel { get; }

        private readonly IBitReader wrapped;
        private readonly IMarkerBuilder builder;
        
        public MarkedBitReader(IBitReader wrapped, MarkerLevel level, IMarkerBuilder builder){
            this.wrapped = wrapped;
            this.builder = builder;
            this.MarkerLevel = level;
        }

        // Markers

        public void MarkStart(){
            builder.MarkStart(Index);
        }

        public void MarkEnd(IMarkerInfo info){
            builder.MarkEnd(Index, info);
        }

        public void MarkEndTitle(string title){
            MarkEnd(new TitleMarker(title));
        }

        public void MarkEndValue(string name, object value){
            MarkEnd(new ValueMarker(name, value));
        }

        // Wrapper
        
        public int Index => wrapped.Index;

        public bool NextBit(){
            return wrapped.NextBit();
        }

        public int NextChunk(int bits){
            return wrapped.NextChunk(bits);
        }

        public void AlignToByteBoundary(){
            MarkStart();
            wrapped.AlignToByteBoundary();
            MarkEnd(new TextMarker("byte boundary alignment"));
        }

        public byte NextAlignedByte(){
            return wrapped.NextAlignedByte();
        }
    }
}
