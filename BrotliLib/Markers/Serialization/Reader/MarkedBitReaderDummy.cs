using BrotliLib.Markers.Types;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Markers.Serialization.Reader{
    class MarkedBitReaderDummy : IMarkedBitReader{
        public MarkerRoot MarkerRoot { get; } = new MarkerRoot();
        public MarkerLevel MarkerLevel => MarkerLevel.None;

        private readonly IBitReader wrapped;
        
        public MarkedBitReaderDummy(IBitReader wrapped){
            this.wrapped = wrapped;
        }

        public void MarkStart(){}
        public void MarkEnd(IMarkerInfo info){}
        public void MarkEndTitle(string title){}
        public void MarkEndValue(string name, object value){}

        public int Index => wrapped.Index;

        public bool NextBit() => wrapped.NextBit();
        public int NextChunk(int bits) => wrapped.NextChunk(bits);
        public void AlignToByteBoundary() => wrapped.AlignToByteBoundary();
        public byte NextAlignedByte() => wrapped.NextAlignedByte();
    }
}
