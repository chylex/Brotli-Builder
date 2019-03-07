using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrotliLib.Markers;
using FastColoredTextBoxNS;

namespace BrotliBuilder.Dialogs{
    public partial class FormBitStreamContext : Form{
        private static readonly TextStyle StyleNormalBlack = new TextStyle(new SolidBrush(Color.Black), null, FontStyle.Regular);
        private static readonly TextStyle StyleNormalGray = new TextStyle(new SolidBrush(Color.DarkGray), null, FontStyle.Regular);

        private static readonly TextStyle StyleBoldBlack = new TextStyle(new SolidBrush(Color.Black), null, FontStyle.Bold);
        private static readonly TextStyle StyleBoldGray = new TextStyle(new SolidBrush(Color.DarkGray), null, FontStyle.Bold);

        private static readonly TextStyle[,] Styles = {
            { null /*default*/, StyleBoldGray },
            { StyleNormalBlack, StyleBoldBlack }
        };

        public static FormBitStreamContext GetOrSpawn(IWin32Window owner){
            var mainForm = Application.OpenForms.OfType<FormMain>().FirstOrDefault();
            var dialogForm = Application.OpenForms.OfType<FormBitStreamContext>().FirstOrDefault();

            if (dialogForm == null){
                dialogForm = new FormBitStreamContext();
                dialogForm.Show(owner);
            }

            if (mainForm != null){
                Point loc = mainForm.Location;
                loc.Offset(-dialogForm.Width, 0);
                
                dialogForm.Location = loc;
                dialogForm.Height = mainForm.Height;
            }

            return dialogForm;
        }

        private IList<MarkerNode> prevMarkerNodes = null;
        private MarkerNode prevCaretNode = null;

        public FormBitStreamContext(){
            InitializeComponent();
            textBoxContext.DefaultStyle = StyleNormalGray;
        }

        public void Display(IList<MarkerNode> markerNodes, HashSet<MarkerNode> highlightedNodes, MarkerNode caretNode){
            if (ReferenceEquals(prevCaretNode, caretNode)){
                return;
            }

            prevCaretNode = caretNode;

            textBoxContext.Selection.BeginUpdate();
            textBoxContext.ClearStyle(StyleIndex.All);
            
            if (!ReferenceEquals(prevMarkerNodes, markerNodes)){
                prevMarkerNodes = markerNodes;

                StringBuilder build = new StringBuilder(512);

                foreach(MarkerNode node in markerNodes){
                    build.Append('\t', node.Depth);
                    build.Append(node.Marker.Info);
                    build.Append('\n');
                }

                textBoxContext.Text = build.ToString();
            }

            for(int line = 0; line < markerNodes.Count; line++){
                MarkerNode node = markerNodes[line];
                IMarkerInfo info = node.Marker.Info;

                int indexColor = highlightedNodes.Contains(node) ? 1 : 0;
                int indexBold = info.IsBold ? 1 : 0;

                TextStyle style = Styles[indexColor, indexBold];

                if (style != null){
                    textBoxContext.GetLine(line).SetStyle(style);
                }
            }
            
            int caretLine = markerNodes.IndexOf(caretNode);
            textBoxContext.Navigate(caretLine);
            
            textBoxContext.Selection.EndUpdate();
        }
    }
}
