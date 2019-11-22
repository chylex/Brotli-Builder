using System.Text;

namespace BrotliLib.Markers.Types{
    sealed class TitleMarker : IMarkerInfo{
        public bool IsBold => true;

        private readonly string title;

        public TitleMarker(string title){
            this.title = string.Intern(title);
        }

        public void ToString(StringBuilder build, int length){
            build.Append('[').Append(title).Append("] · ").Append(length).Append(length == 1 ? " bit" : " bits");
        }
        
        public override string ToString(){
            return title;
        }
    }
}
