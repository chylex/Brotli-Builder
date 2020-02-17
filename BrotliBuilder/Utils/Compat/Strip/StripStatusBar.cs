using System.Drawing;
using System.Windows.Forms;

namespace BrotliBuilder.Utils.Compat.Strip{
    class StripStatusBar : StatusBarBase{
        private readonly StatusStrip statusStrip;

        public StripStatusBar(Form form){
            this.statusStrip = new StatusStrip{
                SizingGrip = false
            };

            form.Controls.Add(statusStrip);
        }

        public override bool Visible{
            get => statusStrip.Visible;
            set => statusStrip.Visible = value;
        }

        public override Panel AddPanel(int width){
            var panel = (ToolStripStatusLabel)statusStrip.Items.Add("");
            panel.AutoSize = false;
            panel.BackColor = SystemColors.Control;
            panel.BorderSides = ToolStripStatusLabelBorderSides.Right;
            panel.Size = new Size(width, 17);
            panel.TextAlign = ContentAlignment.MiddleLeft;
            return new InternalPanel(panel);
        }

        public override void AddPadding(int width){
            var panel = statusStrip.Items.Add("");
            panel.BackColor = SystemColors.Control;
            panel.AutoSize = false;
            panel.Size = new Size(width, 17);
        }

        private class InternalPanel : Panel{
            private readonly ToolStripItem stripItem;

            public InternalPanel(ToolStripItem stripItem){
                this.stripItem = stripItem;
            }

            public override int Width{
                get => stripItem.Width;
                set => stripItem.Width = value;
            }

            public override string Text{
                get => stripItem.Text;
                set => stripItem.Text = value;
            }
        }
    }
}
