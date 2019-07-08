using System.Text;

namespace BrotliLib.Markers{
    public interface IMarkerInfo{
        bool IsBold { get; }
        void ToString(StringBuilder build);
    }
}
