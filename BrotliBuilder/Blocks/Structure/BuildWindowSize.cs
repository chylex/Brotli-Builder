using System;
using System.Linq;
using System.Windows.Forms;
using BrotliLib.Brotli.Components;

namespace BrotliBuilder.Blocks.Structure{
    partial class BuildWindowSize : UserControl{
        private readonly IBuildingBlockContext context;

        public BuildWindowSize(IBuildingBlockContext context, WindowSize windowSize){
            InitializeComponent();

            this.context = context;

            var items = WindowSize.ValidValues.Select(value => new WindowSizeItem(value)).ToArray();
            this.listElements.Items.AddRange(items);
            this.listElements.SelectedItem = items.FirstOrDefault(item => windowSize.Equals(item.Value));
            this.listElements.SelectedValueChanged += listElements_SelectedValueChanged;
        }

        private void listElements_SelectedValueChanged(object sender, EventArgs e){
            WindowSize newWindowSize = ((WindowSizeItem)listElements.SelectedItem).Value;
            context.NotifyParent(new WindowSizeNotifyArgs(newWindowSize));
        }
        
        private class WindowSizeItem{
            public WindowSize Value { get; }

            public WindowSizeItem(WindowSize value){
                this.Value = value;
            }

            public override string ToString(){
                return $"{Value.Bits} bits / {Value.Bytes} bytes";
            }
        }

        public class WindowSizeNotifyArgs : EventArgs{
            public WindowSize NewWindowSize { get; }

            public WindowSizeNotifyArgs(WindowSize newWindowSize){
                this.NewWindowSize = newWindowSize;
            }
        }
    }
}
