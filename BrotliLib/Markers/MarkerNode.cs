using BrotliLib.Markers.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BrotliLib.Markers{
    public sealed class MarkerNode : IEnumerable<MarkerNode>{
        internal const int OmitBitCounts = -1;

        private static readonly Marker EmptyMarker = new Marker(int.MaxValue, int.MaxValue, new TextMarker("<Invalid>"));

        public int Depth { get; set; }
        public Marker Marker { get; set; } = EmptyMarker;

        private MarkerNode? parent;
        private readonly List<MarkerNode> children = new List<MarkerNode>(2);

        internal void AddChildOrSibling(MarkerNode added){
            if (Depth == added.Depth){
                added.parent = parent;
                parent?.children.Add(added);
            }
            else if (Depth < added.Depth){
                added.parent = this;
                children.Add(added);
            }
            else{
                throw new InvalidOperationException();
            }
        }

        public void WriteSelf(StringBuilder build, bool includeBitCounts){
            build.Append('\t', Depth);

            int startIndex = build.Length;
            Marker.Info.ToString(build, includeBitCounts ? Marker.Length : OmitBitCounts);

            build.Replace("\r", "\\r", startIndex, build.Length - startIndex);
            build.Replace("\n", "\\n", startIndex, build.Length - startIndex);
        }

        public IEnumerator<MarkerNode> GetEnumerator(){
            yield return this;

            if (children.Count > 0){
                foreach(MarkerNode child in children){
                    foreach(MarkerNode node in child){
                        yield return node;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        public override string ToString(){
            return Marker == EmptyMarker ? "Empty" : "Depth = " + Depth + ", Index = " + Marker.IndexStart + "-" + Marker.IndexEnd;
        }
    }
}
