using BrotliLib.Markers.Types;

namespace BrotliLib.Markers.Builders{
    public interface IMarkerBuilder{
        void MarkStart(int index);
        void MarkEnd(int index, IMarkerInfo info);
    }
}
