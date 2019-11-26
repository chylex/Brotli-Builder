using System;
using System.Text;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli.Components;

namespace BrotliBuilder.Blocks.Structure{
    partial class BuildEmptyMetaBlock : UserControl{
        private readonly IBuildingBlockContext context;

        public BuildEmptyMetaBlock(IBuildingBlockContext context, MetaBlock.PaddedEmpty metaBlock){
            InitializeComponent();

            this.context = context;
            this.textBoxHiddenText.SetPlainTextMode();
            this.textBoxHiddenText.Text = Encoding.UTF8.GetString(metaBlock.HiddenData);
            this.textBoxHiddenText.TextChanged += textBoxHiddenText_TextChanged;
        }

        private void textBoxHiddenText_TextChanged(object? sender, EventArgs e){
            context.NotifyParent(new HiddenBytesNotifyArgs(Encoding.UTF8.GetBytes(textBoxHiddenText.Text)));
        }

        public class HiddenBytesNotifyArgs : EventArgs{
            public byte[] Bytes { get; }

            public HiddenBytesNotifyArgs(byte[] bytes){
                this.Bytes = bytes;
            }
        }
    }
}
