using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BrotliLib.Markers;
using BrotliLib.Markers.Types;
using FastColoredTextBoxNS;

namespace BrotliBuilder.Components{
    public partial class BrotliMarkerInfoPanel : UserControl{
        private static readonly TextStyle StyleNormalBlack = new TextStyle(new SolidBrush(Color.Black), null, FontStyle.Regular);
        private static readonly TextStyle StyleNormalGray = new TextStyle(new SolidBrush(Color.FromArgb(140, 140, 150)), null, FontStyle.Regular);

        private static readonly TextStyle StyleBoldBlack = new TextStyle(new SolidBrush(Color.Black), null, FontStyle.Bold);
        private static readonly TextStyle StyleBoldGray = new TextStyle(new SolidBrush(Color.FromArgb(140, 140, 150)), null, FontStyle.Bold);

        private static readonly TextStyle?[,] Styles = {
            { null /*default*/, StyleBoldGray },
            { StyleNormalBlack, StyleBoldBlack }
        };

        public bool WordWrap{
            set => textBoxContext.WordWrap = value;
        }

        public BrotliMarkerInfoPanel(){
            InitializeComponent();
            textBoxContext.DefaultStyle = StyleNormalGray;
        }

        private IList<MarkerNode>? prevMarkerNodes = null;
        private MarkerNode? prevCaretNode = null;

        public void UpdateMarkers(MarkerRoot? markerRoot, IList<MarkerNode> markerSequence, HashSet<MarkerNode>? highlightedNodes, MarkerNode? caretNode){
            if (ReferenceEquals(prevCaretNode, caretNode)){
                return;
            }

            prevCaretNode = caretNode;

            textBoxContext.Selection.BeginUpdate();
            textBoxContext.ClearStyle(StyleIndex.All);
            
            if (!ReferenceEquals(prevMarkerNodes, markerSequence)){
                prevMarkerNodes = markerSequence;
                textBoxContext.Text = markerRoot?.BuildText() ?? string.Empty;
            }

            for(int line = 0; line < markerSequence.Count; line++){
                MarkerNode node = markerSequence[line];
                IMarkerInfo info = node.Marker.Info;

                int indexColor = highlightedNodes!.Contains(node) ? 1 : 0;
                int indexBold = info.IsBold ? 1 : 0;

                TextStyle? style = Styles[indexColor, indexBold];

                if (style != null){
                    textBoxContext.GetLine(line).SetStyle(style);
                }
            }
            
            if (caretNode != null){
                int caretLine = markerSequence.IndexOf(caretNode);
                textBoxContext.Navigate(caretLine);
            }
            
            textBoxContext.Selection.EndUpdate();
        }

        public void ResetMarkers(){
            UpdateMarkers(null, Array.Empty<MarkerNode>(), null, null);
        }
    }
}
