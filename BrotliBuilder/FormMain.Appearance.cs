using System;
using System.Windows.Forms;
using BrotliBuilder.Utils;

namespace BrotliBuilder{
    partial class FormMain{
        private void InitializeMenuView(){
            var items = menuItemView.MenuItems;

            void AddCheckBox(string caption, bool initialState, Action<bool> onToggle){
                items.Add(caption, (obj, e) => onToggle(((MenuItem)obj!).Toggle())).Checked = initialState;
            }

            void AddRadioOptions(string caption, string[] options, string initialState, Action<string> onChange){
                var parent = items.Add(caption);
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
            
            AddCheckBox("File Structure Panel", true, ShowFileStructurePanel);
            AddCheckBox("Status Bar",           true, state => statusBar.Visible = state);

            items.Add("-");

            AddCheckBox("Marker Info Panel", true, ShowMarkerInfoPanel);
            AddRadioOptions("Marker Info Placement", new string[]{ "Left", "Top" }, "Left", state => SetMarkerInfoPanelOrientation(state == "Top" ? Orientation.Horizontal : Orientation.Vertical));
            AddRadioOptions("Marker Info Tab Size", new string[]{ "1", "2", "3", "4" }, "3", state => brotliMarkerInfoPanel.TabSize = int.TryParse(state, out int tabSize) ? tabSize : 3);

            items.Add("-");

            AddCheckBox("Wrap Output",      false, state => brotliFilePanelGenerated.WordWrapOutput = brotliFilePanelOriginal.WordWrapOutput = state);
            AddCheckBox("Wrap Marker Info", false, state => brotliMarkerInfoPanel.WordWrap = state);
        }
    }
}
