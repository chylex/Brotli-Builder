using BrotliLib.Markers.Types;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BrotliLib.Markers{
    public sealed class MarkerNode : IEnumerable<MarkerNode>{
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
