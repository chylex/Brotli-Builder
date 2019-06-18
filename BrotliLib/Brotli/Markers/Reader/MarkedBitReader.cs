using System.Collections.Generic;
using BrotliLib.Brotli.Markers.Data;
using BrotliLib.IO.Reader;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Reader{
    class MarkedBitReader : IMarkedBitReader{
        public MarkerRoot MarkerRoot { get; } = new MarkerRoot();

        private readonly IBitReader wrapped;
        
        private readonly Stack<MarkerNode> nodes = new Stack<MarkerNode>();
        private readonly Stack<int> starts = new Stack<int>();
        
        public MarkedBitReader(IBitReader wrapped){
            this.wrapped = wrapped;
        }

        // Markers

        public void MarkStart(){
            MarkerNode added = new MarkerNode{ Depth = nodes.Count };

            if (nodes.Count == 0){
                MarkerRoot.AddChild(added);
            }
            else{
                nodes.Peek().AddChildOrSibling(added);
            }

            starts.Push(Index);
            nodes.Push(added);
        }

        public void MarkEnd(IMarkerInfo info){
            int start = starts.Pop();
            int end = Index;
            nodes.Pop().Marker = new Marker(start, end, info);
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
