using System;
using System.Text;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli.Components.Contents;

namespace BrotliBuilder.Blocks.Structure{
    partial class BuildUncompressedMetaBlock : UserControl{
        private readonly IBuildingBlockContext context;

        public BuildUncompressedMetaBlock(IBuildingBlockContext context, UncompressedMetaBlockContents contents){
            InitializeComponent();

            this.context = context;
            this.textBoxUncompressedText.SetPlainTextMode();
            this.textBoxUncompressedText.Text = Encoding.UTF8.GetString(contents.UncompressedData);
            this.textBoxUncompressedText.TextChanged += textBoxUncompressedText_TextChanged;
        }

        private void textBoxUncompressedText_TextChanged(object sender, EventArgs e){
            context.NotifyParent(new UncompressedBytesNotifyArgs(Encoding.UTF8.GetBytes(textBoxUncompressedText.Text)));
        }

        public class UncompressedBytesNotifyArgs : EventArgs{
            public byte[] Bytes { get; }

            public UncompressedBytesNotifyArgs(byte[] bytes){
                this.Bytes = bytes;
            }
        }
    }
}
