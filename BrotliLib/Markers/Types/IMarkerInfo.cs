using System.Text;

namespace BrotliLib.Markers.Types{
    public interface IMarkerInfo{
        bool IsBold { get; }
        void ToString(StringBuilder build, int length);
    }
}
