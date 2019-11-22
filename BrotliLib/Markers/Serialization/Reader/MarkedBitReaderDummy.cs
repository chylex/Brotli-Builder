using System;
using BrotliLib.Markers.Types;
using BrotliLib.Serialization.Reader;

namespace BrotliLib.Markers.Serialization.Reader{
    class MarkedBitReaderDummy : IMarkedBitReader{
        public MarkerRoot MarkerRoot { get; } = new MarkerRoot();

        private readonly IBitReader wrapped;
        
        public MarkedBitReaderDummy(IBitReader wrapped){
            this.wrapped = wrapped;
        }

        public void MarkStart(){}
        public void MarkEnd(Func<IMarkerInfo> info){}
        public T MarkCall<T>(Func<T> supplier, Func<T, IMarkerInfo> marker) => supplier();

        public int Index => wrapped.Index;

        public bool NextBit() => wrapped.NextBit();
        public int NextChunk(int bits) => wrapped.NextChunk(bits);
        public void AlignToByteBoundary() => wrapped.AlignToByteBoundary();
        public byte NextAlignedByte() => wrapped.NextAlignedByte();
    }
}
