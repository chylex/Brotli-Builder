using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BrotliLib.Markers{
    public sealed class MarkerNode : IEnumerable<MarkerNode>{
        public MarkerNode Parent { get; private set; }
        public MarkerNode PrevSibling { get; private set; }
        public MarkerNode NextSibling { get; private set; }

        public int Depth { get; set; }
        public Marker Marker { get; set; }

        private readonly List<MarkerNode> children = new List<MarkerNode>();

        internal void AddChildOrSibling(MarkerNode added){
            if (Depth == added.Depth){
                MarkerNode myLastSibling = this;
                MarkerNode testSibling;

                while((testSibling = myLastSibling.NextSibling) != null){
                    myLastSibling = testSibling;
                }

                added.Parent = Parent;
                added.PrevSibling = myLastSibling;
                myLastSibling.NextSibling = added;
            }
            else if (Depth < added.Depth){
                MarkerNode myLastChild = children.LastOrDefault();

                added.Parent = this;

                if (myLastChild != null){
                    added.PrevSibling = myLastChild;
                    myLastChild.NextSibling = added;
                }
            }
            else{
                throw new InvalidOperationException();
            }

            added.Parent?.children?.Add(added);
        }

        public MarkerNode FindNodeAt(int index){
            if (Marker.HasIndex(index)){
                return children.FirstOrDefault()?.FindNodeAt(index) ?? this;
            }
            else{
                return NextSibling?.FindNodeAt(index);
            }
        }

        public IEnumerator<MarkerNode> GetEnumerator(){
            yield return this;

            if (children.Count > 0){
                foreach(MarkerNode node in children[0]){
                    yield return node;
                }
            }

            if (NextSibling != null){
                foreach(MarkerNode node in NextSibling){
                    yield return node;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        public override string ToString(){
            return Marker == null ? "Empty" : "Depth = " + Depth + ", Index = " + Marker.IndexStart + "-" + Marker.IndexEnd;
        }
    }
}
