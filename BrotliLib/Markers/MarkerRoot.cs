using System.Collections;
using System.Collections.Generic;

namespace BrotliLib.Markers{
    public sealed class MarkerRoot : IEnumerable<MarkerNode>{
        private readonly List<MarkerNode> children = new List<MarkerNode>();

        internal void AddChild(MarkerNode added){
            children.Add(added);
        }

        public IEnumerator<MarkerNode> GetEnumerator(){
            foreach(MarkerNode child in children){
                foreach(MarkerNode node in child){
                    yield return node;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }

        public override string ToString(){
            return "Root";
        }
    }
}
