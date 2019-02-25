using System;

namespace BrotliLib.Markers{
    public class Marker : IComparable<Marker>{
        public long IndexStart { get; }
        public long IndexEnd { get; }
        public long Length => IndexEnd - IndexStart;
        
        public IMarkerInfo Info { get; }

        public Marker(long indexStart, long indexEnd, IMarkerInfo info){
            this.IndexStart = indexStart;
            this.IndexEnd = indexEnd;
            this.Info = info;
        }

        public bool HasIndex(int index){
            return index >= IndexStart && index < IndexEnd;
        }

        public int CompareTo(Marker other){
            return IndexStart.CompareTo(other.IndexStart);
        }
    }
}
