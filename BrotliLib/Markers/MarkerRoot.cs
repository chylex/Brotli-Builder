using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BrotliLib.Markers{
    public sealed class MarkerRoot : IEnumerable<MarkerNode>{
        internal const int OmitBitCounts = -1;

        private readonly List<MarkerNode> children = new List<MarkerNode>();

        internal void AddChild(MarkerNode added){
            children.Add(added);
        }

        public string BuildText(bool includeBitCounts){
            var build = new StringBuilder(512);

            foreach(MarkerNode node in this){
                build.Append('\t', node.Depth);

                var marker = node.Marker;
                int startIndex = build.Length;
                marker.Info.ToString(build, includeBitCounts ? marker.Length : OmitBitCounts);

                build.Replace("\r", "\\r", startIndex, build.Length - startIndex);
                build.Replace("\n", "\\n", startIndex, build.Length - startIndex);

                build.Append('\n');
            }

            return build.ToString();
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
