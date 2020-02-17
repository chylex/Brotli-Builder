using System.Windows.Forms;

namespace BrotliBuilder.Utils.Compat.Legacy{
    class LegacyStatusBar : StatusBarBase{
        private readonly StatusBar statusBar;

        public LegacyStatusBar(Form form){
            this.statusBar = new StatusBar{
                ShowPanels = true,
                SizingGrip = false
            };

            form.Controls.Add(statusBar);
        }

        public override bool Visible{
            get => statusBar.Visible;
            set => statusBar.Visible = value;
        }

        public override Panel AddPanel(int width){
            var panel = new StatusBarPanel();
            panel.BeginInit();
            panel.Width = width;
            panel.EndInit();
            statusBar.Panels.Add(panel);
            return new InternalPanel(panel);
        }

        public override void AddPadding(int width){
            var panel = new StatusBarPanel();
            panel.BeginInit();
            panel.MinWidth = width;
            panel.Width = width;
            panel.BorderStyle = StatusBarPanelBorderStyle.None;
            panel.EndInit();
            statusBar.Panels.Add(panel);
        }

        private class InternalPanel : Panel{
            private readonly StatusBarPanel panel;

            public InternalPanel(StatusBarPanel panel){
                this.panel = panel;
            }

            public override int Width{
                get => panel.Width;
                set => panel.Width = value;
            }

            public override string Text{
                get => panel.Text;
                set => panel.Text = value;
            }
        }
    }
}
