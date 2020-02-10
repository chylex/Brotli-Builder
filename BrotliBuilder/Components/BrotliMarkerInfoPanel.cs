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

        public int TabSize{
            set{
                textBoxContext.TabLength = value;
                RefreshMarkers();
            }
        }

        public Orientation Orientation{
            set{
                if (value == Orientation.Horizontal){
                    textBoxContext.Margin = new Padding(12, 5, 12, 0);
                    textBoxContext.Width -= 12;
                    textBoxContext.Height += 8;
                }
                else{
                    textBoxContext.Margin = new Padding(12, 5, 0, 8);
                    textBoxContext.Width += 12;
                    textBoxContext.Height -= 8;
                }
            }
        }

        private readonly string originalTitle;

        public BrotliMarkerInfoPanel(){
            InitializeComponent();
            originalTitle = labelMarkerInfo.Text;
            textBoxContext.DefaultStyle = StyleNormalGray;
        }

        private LastCall? prev = null;

        private class LastCall{
            public string? Title { get; }
            public MarkerRoot MarkerRoot { get; }
            public IList<MarkerNode> MarkerSequence { get; }
            public HashSet<MarkerNode> HighlightedNodes { get; }
            public MarkerNode? CaretNode { get; }

            public LastCall(string? title, MarkerRoot markerRoot, IList<MarkerNode> markerSequence, HashSet<MarkerNode> highlightedNodes, MarkerNode? caretNode){
                Title = title;
                MarkerRoot = markerRoot;
                MarkerSequence = markerSequence;
                HighlightedNodes = highlightedNodes;
                CaretNode = caretNode;
            }
        }

        public void UpdateMarkers(string? title, MarkerRoot? markerRoot, IList<MarkerNode> markerSequence, HashSet<MarkerNode>? highlightedNodes, MarkerNode? caretNode){
            if (ReferenceEquals(prev?.CaretNode, caretNode)){
                return;
            }

            labelMarkerInfo.Text = title == null ? originalTitle : $"{originalTitle} ({title})";
            textBoxContext.Selection.BeginUpdate();
            textBoxContext.ClearStyle(StyleIndex.All);
            
            if (!ReferenceEquals(prev?.MarkerSequence, markerSequence)){
                textBoxContext.Text = markerRoot?.BuildText(includeBitCounts: true) ?? string.Empty;
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
            prev = markerRoot == null ? null : new LastCall(title, markerRoot, markerSequence, highlightedNodes!, caretNode);
        }

        public void ResetMarkers(){
            UpdateMarkers(null, null, Array.Empty<MarkerNode>(), null, null);
        }

        public void RefreshMarkers(){
            LastCall? restore = prev;

            ResetMarkers();

            if (restore != null){
                UpdateMarkers(restore.Title, restore.MarkerRoot, restore.MarkerSequence, restore.HighlightedNodes, restore.CaretNode);
            }
        }
    }
}
