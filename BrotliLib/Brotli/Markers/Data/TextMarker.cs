using System.Text;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Data{
    sealed class TextMarker : IMarkerInfo{
        public bool IsBold => false;

        private readonly string text;

        public TextMarker(string text){
            this.text = string.Intern(text);
        }

        public void ToString(StringBuilder build, int length){
            build.Append(text);
        }
        
        public override string ToString(){
            return text;
        }
    }
}
