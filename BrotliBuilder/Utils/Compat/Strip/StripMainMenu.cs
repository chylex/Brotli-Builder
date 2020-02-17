using System;
using System.Drawing;
using System.Windows.Forms;

namespace BrotliBuilder.Utils.Compat.Strip{
    class StripMainMenu : MainMenuBase{
        private readonly MenuStrip menuStrip;

        public StripMainMenu(Form form){
            this.menuStrip = form.MainMenuStrip = new MenuStrip();
            this.menuStrip.Size = new Size(form.Width, 24);
            this.menuStrip.RenderMode = ToolStripRenderMode.Professional;

            form.Controls.Add(menuStrip);
        }

        public override Item AddItem(string caption){
            return new InternalItem(menuStrip.Items.Add(caption));
        }

        private class InternalItem : Item{
            private ToolStripItemCollection Items => ((ToolStripMenuItem)menuItem).DropDownItems;

            private readonly ToolStripItem menuItem;

            public InternalItem(ToolStripItem menuItem){
                this.menuItem = menuItem;
            }

            public override bool Enabled{
                get => menuItem.Enabled;
                set => menuItem.Enabled = value;
            }

            public override Item Add(string caption, Action onClick, Shortcut shortcut = Shortcut.None, bool isEnabled = true){
                var item = (ToolStripMenuItem)Items.Add(caption);
                item.ShortcutKeys = (Keys)shortcut;
                item.Enabled = isEnabled;
                item.Click += (_, e) => onClick();
                return new InternalItem(item);
            }

            public override Item AddCheckBox(string caption, bool initialState, Action<bool> onToggle){
                var item = (ToolStripMenuItem)Items.Add(caption);
                item.Checked = initialState;
                item.CheckOnClick = true;
                item.CheckedChanged += (_, e) => onToggle(item.Checked);
                return new InternalItem(item);
            }

            public override void AddRadioOptions(string caption, string[] options, string initialState, Action<string> onChange){
                var parent = (ToolStripMenuItem)Items.Add(caption);
                var parentItems = parent.DropDownItems;

                foreach(var option in options){
                    ((ToolStripMenuItem)parentItems.Add(option)).Click += (obj, e) => {
                        foreach(var other in parentItems){
                            ((ToolStripMenuItem)other!).Checked = ReferenceEquals(obj, other);
                        }

                        onChange(option);
                    };
                }

                ((ToolStripMenuItem)parent.DropDownItems[Array.IndexOf(options, initialState)]).Checked = true;
            }

            public override void AddSeparator(){
                Items.Add(new ToolStripSeparator());
            }
        }
    }
}
