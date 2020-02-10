using System;
using System.Windows.Forms;
using BrotliBuilder.Utils;

namespace BrotliBuilder{
    partial class FormMain{
        private void InitializeMenuView(){
            var items = menuItemView.MenuItems;

            void AddCheckBox(string caption, bool initialState, Action<bool> onClick){
                items.Add(caption, (obj, e) => onClick(((MenuItem)obj!).Toggle())).Checked = initialState;
            }
            
            AddCheckBox("File Structure Panel", true,  state => splitContainerRight.Panel1Collapsed = !state);
            AddCheckBox("Marker Info Panel",    true,  ShowMarkerInfoPanel);
            AddCheckBox("Wrap Output",          false, state => brotliFilePanelGenerated.WordWrapOutput = brotliFilePanelOriginal.WordWrapOutput = state);
            AddCheckBox("Wrap Marker Info",     false, state => brotliMarkerInfoPanel.WordWrap = state);
        }
    }
}
