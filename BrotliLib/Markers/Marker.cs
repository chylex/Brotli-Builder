using System;
using BrotliLib.Markers.Types;

namespace BrotliLib.Markers{
    public class Marker : IComparable<Marker>{
        public int IndexStart { get; }
        public int IndexEnd { get; }
        public int Length => IndexEnd - IndexStart;
        
        public IMarkerInfo Info { get; }

        public Marker(int indexStart, int indexEnd, IMarkerInfo info){
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
