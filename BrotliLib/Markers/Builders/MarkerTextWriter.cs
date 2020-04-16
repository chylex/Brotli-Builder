using System.Collections.Generic;
using System.IO;
using System.Text;
using BrotliLib.Markers.Types;

namespace BrotliLib.Markers.Builders{
    public class MarkerTextWriter : IMarkerBuilder{
        private readonly TextWriter writer;
        private readonly bool includeBitCounts;

        private readonly StringBuilder tmpBuilder;

        private readonly Stack<MarkerNode> nodes = new Stack<MarkerNode>();
        private readonly Stack<int> starts = new Stack<int>();

        public MarkerTextWriter(TextWriter writer, bool includeBitCounts){
            this.writer = writer;
            this.includeBitCounts = includeBitCounts;

            this.tmpBuilder = new StringBuilder();
        }

        public void MarkStart(int index){
            MarkerNode added = new MarkerNode{ Depth = nodes.Count };

            if (nodes.Count > 0){
                nodes.Peek().AddChildOrSibling(added);
            }

            starts.Push(index);
            nodes.Push(added);
        }

        public void MarkEnd(int index, IMarkerInfo info){
            var node = nodes.Pop();
            node.Marker = new Marker(starts.Pop(), index, info);

            if (nodes.Count == 0){
                foreach(MarkerNode child in node){
                    child.WriteSelf(tmpBuilder, includeBitCounts);
                    writer.WriteLine(tmpBuilder.ToString()); // TODO there is an overload for StringBuilder, which is officially documented for .NET Core 3.0, but the compiler finds sweet FA
                    tmpBuilder.Clear();
                }
            }
        }
    }
}
