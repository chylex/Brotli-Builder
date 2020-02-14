using System;
using System.Linq;
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

            foreach(Control child in flowLayoutPanel.Controls.Cast<Control>()){
                child.Height = height;
            }
            
            flowLayoutPanel.ResumeLayout(true);
        }

        public static bool Toggle(this MenuItem menuItem){
            bool enable = !menuItem.Checked;
            menuItem.Checked = enable;
            return enable;
        }

        public static MenuItem Add(this MenuItem menuItem, string caption, Action onClick, Shortcut shortcut = Shortcut.None, bool isEnabled = true){
            var item = menuItem.MenuItems.Add(caption, (_, e) => onClick());
            item.Shortcut = shortcut;
            item.Enabled = isEnabled;
            return item;
        }

        public static void AddSeparator(this MenuItem menuItem){
            menuItem.MenuItems.Add("-");
        }

        public static void AddCheckBox(this MenuItem menuItem, string caption, bool initialState, Action<bool> onToggle){
            menuItem.MenuItems.Add(caption, (obj, e) => onToggle(((MenuItem)obj!).Toggle())).Checked = initialState;
        }

        public static void AddRadioOptions(this MenuItem menuItem, string caption, string[] options, string initialState, Action<string> onChange){
            var parent = menuItem.MenuItems.Add(caption);
            var parentItems = parent.MenuItems;

            foreach(var option in options){
                parentItems.Add(option, (obj, e) => {
                    foreach(var other in parentItems){
                        ((MenuItem)other!).Checked = ReferenceEquals(obj, other);
                    }

                    onChange(option);
                }).RadioCheck = true;
            }

            parent.MenuItems[Array.IndexOf(options, initialState)].Checked = true;
        }
    }
}
