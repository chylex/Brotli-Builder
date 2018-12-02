using System.Reflection;
using System.Windows.Forms;

namespace BrotliBuilder.Utils{
    static class ControlExtensions{
        public static void EnableDoubleBuffering(this DataGridView dataGridView){
            // Visual Studio is garbage and the designer crashes if DataGridView is extended, so reflection it is
            dataGridView.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dataGridView, true, null);
        }
    }
}
