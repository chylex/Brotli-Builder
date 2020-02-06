using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BrotliLib.Markers{
    public sealed class MarkerRoot : IEnumerable<MarkerNode>{
        internal const int OmitBitCounts = -1;

        public int TotalBits => children.Count == 0 ? 0 : children[^1].Marker.IndexEnd;

        private readonly List<MarkerNode> children = new List<MarkerNode>();

        internal void AddChild(MarkerNode added){
            children.Add(added);
        }

        public string BuildText(bool includeBitCounts){
            using var writer = new StringWriter{
                NewLine = "\n"
            };

            WriteText(writer, includeBitCounts);
            return writer.ToString();
        }

        public void WriteText(TextWriter writer, bool includeBitCounts){
            var build = new StringBuilder(256);

            foreach(MarkerNode node in this){
                build.Clear();
                build.Append('\t', node.Depth);

                var marker = node.Marker;
                int startIndex = build.Length;
                marker.Info.ToString(build, includeBitCounts ? marker.Length : OmitBitCounts);

                build.Replace("\r", "\\r", startIndex, build.Length - startIndex);
                build.Replace("\n", "\\n", startIndex, build.Length - startIndex);
                writer.WriteLine(build.ToString());
                // TODO there is an overload for StringBuilder, which is officially documented for .NET Core 3.0, but the compiler finds sweet FA
            }
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
