using System.Collections.Generic;
using BrotliLib.Markers.Types;

namespace BrotliLib.Markers.Builders{
    public class MarkerRootBuilder : IMarkerBuilder{
        public MarkerRoot Root { get; }

        private readonly Stack<MarkerNode> nodes = new Stack<MarkerNode>();
        private readonly Stack<int> starts = new Stack<int>();

        public MarkerRootBuilder(){
            this.Root = new MarkerRoot();
        }

        public void MarkStart(int index){
            MarkerNode added = new MarkerNode{ Depth = nodes.Count };

            if (nodes.Count == 0){
                Root.AddChild(added);
            }
            else{
                nodes.Peek().AddChildOrSibling(added);
            }

            starts.Push(index);
            nodes.Push(added);
        }

        public void MarkEnd(int index, IMarkerInfo info){
            nodes.Pop().Marker = new Marker(starts.Pop(), index, info);
        }
    }
}
