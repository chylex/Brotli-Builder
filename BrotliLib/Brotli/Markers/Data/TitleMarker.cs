using System.Text;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Data{
    sealed class TitleMarker : IMarkerInfo{
        public bool IsBold => true;

        private readonly string title;

        public TitleMarker(string title){
            this.title = string.Intern("[" + title + "]");
        }

        public void ToString(StringBuilder build){
            build.Append(title);
        }
        
        public override string ToString(){
            return title;
        }
    }
}
