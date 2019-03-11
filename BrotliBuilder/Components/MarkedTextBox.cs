using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using BrotliBuilder.Dialogs;
using BrotliBuilder.Utils;
using BrotliLib.Markers;
using FastColoredTextBoxNS;

namespace BrotliBuilder.Components{
    class MarkedTextBox : FastColoredTextBox{
        private static readonly TextStyle[] MainStyles = Colors.List
                                                               .Select(color => Colors.Mix(SystemColors.Control, color, 0.725))
                                                               .Select(color => new TextStyle(new SolidBrush(Color.Black), new SolidBrush(color), FontStyle.Regular))
                                                               .ToArray();

        private static readonly TextStyle HighlightStyle = new TextStyle(new SolidBrush(Color.White), new SolidBrush(Color.FromArgb(48, 48, 48)), FontStyle.Regular);

        private readonly StyleIndex[] mainStyleIndex;
        private readonly StyleIndex highlightStyleIndex;
        
        private MarkerNode[] markerSequence = null;
        private MarkerNode[] prevMarkerSequence = null;

        private MarkerNode markerCaret = null;
        private bool updatingMarkers = false;

        public MarkedTextBox(){
            AllowSeveralTextStyleDrawing = true;

            mainStyleIndex = MainStyles.Select(style => (StyleIndex)(1 << AddStyle(style))).ToArray();
            highlightStyleIndex = (StyleIndex)(1 << AddStyle(HighlightStyle));

            SelectionChanged += MarkedFastTextBox_SelectionChanged;
            CustomAction += MarkedFastTextBox_CustomAction;
        }

        public void UpdateMarkers(MarkerNode[] newMarkerSequence){
            markerSequence = newMarkerSequence;
            RefreshMarkers();
            prevMarkerSequence = newMarkerSequence;
        }
        
        public void RemoveMarkers(){
            markerSequence = null;
            ClearStyle(StyleIndex.All);
        }

        private void RefreshMarkers(){
            if (markerSequence == null){
                RemoveMarkers();
                return;
            }

            bool recolorAll = markerSequence != prevMarkerSequence;
            
            updatingMarkers = true;
            Selection.BeginUpdate();
            
            int caret = SelectionStart;
            int maxDepth = -1;
            List<MarkerNode> highlightedMarkers = new List<MarkerNode>();

            ClearStyle(recolorAll ? StyleIndex.All : highlightStyleIndex);

            foreach(MarkerNode node in markerSequence){
                Marker marker = node.Marker;
                int depth = node.Depth;

                if (marker.HasIndex(caret)){
                    highlightedMarkers.Add(node);
                    maxDepth = Math.Max(depth, maxDepth);
                }
                else{
                    maxDepth = Math.Min(depth, maxDepth);
                }
            }
            
            if (recolorAll){
                int colorIndex = 0;
            
                foreach(MarkerNode node in markerSequence){
                    Marker marker = node.Marker;

                    SelectionStart = marker.IndexStart;
                    SelectionLength = marker.Length;

                    Selection.ClearStyle(StyleIndex.All);
                    Selection.SetStyle(mainStyleIndex[colorIndex % mainStyleIndex.Length]);

                    ++colorIndex;
                }
            }

            MarkerNode newMarkerCaret = markerCaret;

            if (highlightedMarkers.Count > 0){
                MarkerNode deepestNode = highlightedMarkers.Last();
                Marker deepestMarker = deepestNode.Marker;
                
                SelectionStart = deepestMarker.IndexStart;
                SelectionLength = deepestMarker.Length;
                
                Selection.SetStyle(highlightStyleIndex);
                
                newMarkerCaret = deepestNode;
            }
            
            SelectionStart = caret;
            SelectionLength = 0;

            Selection.EndUpdate();
            updatingMarkers = false;
            
            markerCaret = newMarkerCaret;
            FormBitStreamContext.GetOrSpawn(this).Display(markerSequence, new HashSet<MarkerNode>(highlightedMarkers), markerCaret);
        }

        private void MarkedFastTextBox_SelectionChanged(object sender, EventArgs e){
            if (!updatingMarkers){
                RefreshMarkers();
            }
        }
        
        private void MarkedFastTextBox_CustomAction(object sender, CustomActionEventArgs e){
            Keys key = e.Action == FCTBAction.CustomAction1 ? Keys.Left : e.Action == FCTBAction.CustomAction2 ? Keys.Right : Keys.Escape;

            if ((key == Keys.Left || key == Keys.Right) && markerCaret != null){
                int caret = SelectionStart;

                int nodeIndex = Array.IndexOf(markerSequence, markerCaret);
                MarkerNode targetNode = null;
                    
                if (key == Keys.Left){
                    while(--nodeIndex >= 0){
                        targetNode = markerSequence[nodeIndex];

                        int newCaret = targetNode.Marker.IndexStart;

                        if (caret != newCaret){
                            caret = newCaret;
                            break;
                        }
                    }
                }
                else if (key == Keys.Right){
                    while(++nodeIndex < markerSequence.Length){
                        targetNode = markerSequence[nodeIndex];

                        int newCaret = targetNode.Marker.IndexStart;

                        if (caret != newCaret){
                            caret = newCaret;
                            break;
                        }
                    }
                }

                if (targetNode != null){
                    Selection.BeginUpdate();
                    SelectionStart = caret;
                    Selection.End = Selection.Start;
                    Selection.EndUpdate();

                    markerCaret = targetNode;
                }
            }
        }
    }
}
