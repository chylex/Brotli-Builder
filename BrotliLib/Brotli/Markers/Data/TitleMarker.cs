using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Data{
    class TitleMarker : IMarkerInfo{
        public bool IsBold => true;

        private readonly string title;

        public TitleMarker(string title){
            this.title = "[" + title + "]";
        }
        
        public override string ToString(){
            return title;
        }
    }
}
