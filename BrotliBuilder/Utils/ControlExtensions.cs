using System.Reflection;
using System.Windows.Forms;

namespace BrotliBuilder.Utils{
    static class ControlExtensions{
        public static void EnableDoubleBuffering(this DataGridView dataGridView){
            // Visual Studio is garbage and the designer crashes if DataGridView is extended, so reflection it is
            dataGridView.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dataGridView, true, null);
        }

        public static void SetValueInstantly(this ProgressBar progressBar, int value){
            // update progres bar instantly
            if (value == progressBar.Maximum){
                progressBar.Maximum++;
                progressBar.Value = value + 1;
                progressBar.Maximum--;
            }
            else{
                progressBar.Value = value + 1;
            }

            progressBar.Value = value;
        }

        public static void SetChildHeight(this FlowLayoutPanel flowLayoutPanel, int height){
            flowLayoutPanel.SuspendLayout();
            
            foreach(Control child in flowLayoutPanel.Controls){
                child.Height = height;
            }
            
            flowLayoutPanel.ResumeLayout(true);
        }

        public static bool Toggle(this MenuItem menuItem){
            bool enable = !menuItem.Checked;
            menuItem.Checked = enable;
            return enable;
        }
    }
}
