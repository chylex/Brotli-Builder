using System.Windows.Forms;
using BrotliBuilder.Utils.Compat;
using BrotliLib.Markers;

namespace BrotliBuilder{
    partial class FormMain{
        private void InitializeMenuView(MainMenuBase.Item menu){
            menu.AddCheckBox("File Structure Panel", true, ShowFileStructurePanel);
            menu.AddCheckBox("Status Bar",           true, state => statusBar.Visible = state);

            menu.AddSeparator();

            menu.AddCheckBox("Marker Info Panel", true, ShowMarkerInfoPanel);
            menu.AddRadioOptions("Marker Info Placement", new string[]{ "Left", "Top" }, "Left", state => SetMarkerInfoPanelOrientation(state == "Top" ? Orientation.Horizontal : Orientation.Vertical));
            menu.AddRadioOptions("Marker Info Tab Size", new string[]{ "1", "2", "3", "4" }, "3", state => brotliMarkerInfoPanel.TabSize = int.TryParse(state, out int tabSize) ? tabSize : 3);
            
            menu.AddSeparator();

            menu.AddCheckBox("Wrap Output",      false, state => brotliFilePanelGenerated.WordWrapOutput = brotliFilePanelOriginal.WordWrapOutput = state);
            menu.AddCheckBox("Wrap Marker Info", false, state => brotliMarkerInfoPanel.WordWrap = state);
        }

        private void ShowFileStructurePanel(bool show){
            splitContainerRight.Panel1Collapsed = !show;
            splitContainerRight.Panel1MinSize = show ? 175 : 0;
        }

        private void ShowMarkerInfoPanel(bool show){
            splitContainerMain.Panel1Collapsed = !show;
            fileGenerated.BitMarkerLevel = show ? MarkerLevel.Verbose : MarkerLevel.None;
            fileOriginal.BitMarkerLevel = show ? MarkerLevel.Verbose : MarkerLevel.None;

            if (!show){
                brotliMarkerInfoPanel.ResetMarkers();
            }
        }

        private void SetMarkerInfoPanelOrientation(Orientation orientation){
            SuspendLayout();
            brotliMarkerInfoPanel.Orientation = orientation;
            splitContainerMain.Orientation = orientation;

            if (orientation == Orientation.Horizontal){
                splitContainerMain.SplitterDistance /= 2;
                splitContainerMain.Panel2MinSize = 125;
            }
            else{
                splitContainerMain.SplitterDistance *= 2;
                splitContainerMain.Panel2MinSize = 275;
            }

            ResumeLayout(true);
        }
    }
}
