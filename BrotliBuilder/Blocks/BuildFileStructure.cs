using System;
using System.Linq;
using System.Windows.Forms;
using BrotliBuilder.Blocks.Structure;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Contents;
using BrotliLib.Brotli.Components.Header;

namespace BrotliBuilder.Blocks{
    partial class BuildFileStructure : UserControl{
        private readonly IBuildingBlockContext context;
        private readonly BrotliFileStructure brotliFile;

        public BuildFileStructure(IBuildingBlockContext context, BrotliFileStructure brotliFile){
            InitializeComponent();

            this.context = context;
            this.context.Notified += context_Notified;
            this.brotliFile = brotliFile;

            RegenerateElementList(selectIndex: -1, notifyParent: false);
        }

        private void RegenerateElementList(int selectIndex, bool notifyParent){
            listElements.Items.Clear();
            listElements.Items.Add(new StructureWindowSizeItem(brotliFile));

            foreach(MetaBlock metaBlock in brotliFile.MetaBlocks){
                listElements.Items.Add(new StructureMetaBlockItem(metaBlock));
            }

            listElements.SelectedIndex = selectIndex;

            if (notifyParent){
                context.NotifyParent(EventArgs.Empty);
            }
        }

        private void context_Notified(object sender, EventArgs e){
            if (e is BuildWindowSize.WindowSizeNotifyArgs wsna){
                brotliFile.Parameters = new BrotliFileParameters(wsna.NewWindowSize, brotliFile.Parameters.Dictionary);
            }
            else if (listElements.SelectedItem is StructureMetaBlockItem smbi){
                smbi.HandleNotification(e);
            }

            brotliFile.Fixup();
            
            listElements.SelectedValueChanged -= listElements_SelectedValueChanged;
            listElements.Items[listElements.SelectedIndex] = listElements.SelectedItem; // update item text
            listElements.SelectedValueChanged += listElements_SelectedValueChanged;
        }

        private void listElements_SelectedValueChanged(object sender, EventArgs e){
            object selected = listElements.SelectedItem;
            bool isSelectingMetaBlock = false;

            if (selected == null){
                context.SetChildBlock(null);
            }
            else if (selected is StructureWindowSizeItem){
                context.SetChildBlock(ctx => new BuildWindowSize(ctx, brotliFile.Parameters.WindowSize));
            }
            else if (selected is StructureMetaBlockItem smbi){
                context.SetChildBlock(smbi.CreateStructureBlock());
                isSelectingMetaBlock = true;
            }

            buttonDeleteMetaBlock.Enabled = isSelectingMetaBlock;
            buttonMoveMetaBlockUp.Enabled = isSelectingMetaBlock && listElements.SelectedIndex > 1;
            buttonMoveMetaBlockDown.Enabled = isSelectingMetaBlock && listElements.SelectedIndex < listElements.Items.Count - 1;
        }

        // Meta-block item handling

        private void AddMetaBlock(MetaBlock metaBlock){
            object selected = listElements.SelectedItem;
            int insertAt;

            if (selected == null){
                insertAt = brotliFile.MetaBlocks.Count;
            }
            else if (selected is StructureMetaBlockItem item){
                insertAt = brotliFile.MetaBlocks.IndexOf(item.Value) + 1;
            }
            else{
                insertAt = 0;
            }

            brotliFile.MetaBlocks.Insert(insertAt, metaBlock);
            brotliFile.Fixup();
            RegenerateElementList(selectIndex: insertAt + 1, notifyParent: true);
        }

        private void buttonAddMetaBlockCompressed_Click(object sender, EventArgs e){
            // TODO
        }

        private void buttonAddMetaBlockUncompressed_Click(object sender, EventArgs e){
            AddMetaBlock(new MetaBlock.Uncompressed(new byte[0]));
        }

        private void buttonAddMetaBlockEmpty_Click(object sender, EventArgs e){
            if (listElements.SelectedIndex == listElements.Items.Count - 1 && !listElements.Items.OfType<StructureMetaBlockItem>().Any(item => item.Value is MetaBlock.LastEmpty)){
                AddMetaBlock(new MetaBlock.LastEmpty());
            }
            else{
                AddMetaBlock(new MetaBlock.PaddedEmpty(new byte[0]));
            }
        }

        private void MoveMetaBlock(bool up){
            if (listElements.SelectedItem is StructureMetaBlockItem item){
                int currentIndex = brotliFile.MetaBlocks.IndexOf(item.Value);
                int newIndex = currentIndex + (up ? -1 : 1);

                if (newIndex >= 0 && newIndex < brotliFile.MetaBlocks.Count){
                    brotliFile.MetaBlocks.RemoveAt(currentIndex);
                    brotliFile.MetaBlocks.Insert(newIndex, item.Value);
                    brotliFile.Fixup();
                    RegenerateElementList(selectIndex: newIndex + 1, notifyParent: true);
                }
            }
        }

        private void buttonMoveMetaBlockUp_Click(object sender, EventArgs e){
            MoveMetaBlock(true);
        }

        private void buttonMoveMetaBlockDown_Click(object sender, EventArgs e){
            MoveMetaBlock(false);
        }

        private void buttonDeleteMetaBlock_Click(object sender, EventArgs e){
            if (listElements.SelectedItem is StructureMetaBlockItem item &&
                MessageBox.Show($"Are you sure you want to permanently delete this meta-block?{Environment.NewLine}{item}", "Delete Meta-Block", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes
            ){
                brotliFile.MetaBlocks.Remove(item.Value);
                brotliFile.Fixup();
                RegenerateElementList(selectIndex: Math.Min(listElements.SelectedIndex, listElements.Items.Count - 2), notifyParent: true);
            }
        }

        // List item classes
        
        private class StructureWindowSizeItem{
            private readonly BrotliFileStructure brotliFile;

            public StructureWindowSizeItem(BrotliFileStructure brotliFile){
                this.brotliFile = brotliFile;
            }

            public override string ToString(){
                return $"Window Size ({brotliFile.Parameters.WindowSize.Bits} bits)";
            }
        }

        private class StructureMetaBlockItem{
            public MetaBlock Value { get; }

            public StructureMetaBlockItem(MetaBlock value){
                this.Value = value;
            }

            public Func<IBuildingBlockContext, UserControl> CreateStructureBlock(){
                switch(Value){
                    case MetaBlock.PaddedEmpty pe:
                        return ctx => new BuildEmptyMetaBlock(ctx, pe.Contents);

                    case MetaBlock.Uncompressed u:
                        return ctx => new BuildUncompressedMetaBlock(ctx, u.Contents);

                    case MetaBlock.Compressed c:
                        // TODO

                    default:
                        return null;
                }
            }

            public void HandleNotification(EventArgs args){
                switch(args){
                    case BuildEmptyMetaBlock.HiddenBytesNotifyArgs hbna:
                        if (Value is MetaBlock.PaddedEmpty pe){
                            pe.Contents = new PaddedEmptyMetaBlockContents(hbna.Bytes);
                        }

                        break;

                    case BuildUncompressedMetaBlock.UncompressedBytesNotifyArgs ubna:
                        if (Value is MetaBlock.Uncompressed u){
                            u.Contents = new UncompressedMetaBlockContents(ubna.Bytes);
                            u.DataLength = new DataLength(ubna.Bytes.Length);
                        }

                        break;

                    // TODO
                }
            }

            public override string ToString(){
                string detail;

                switch(Value){
                    case MetaBlock.LastEmpty _:
                        detail = "Empty, Last";
                        break;

                    case MetaBlock.PaddedEmpty pe:
                        int length = pe.Contents.HiddenData.Length;
                        detail = length == 0 ? "Empty, Padded" : $"Empty, Skip {length} B";
                        break;

                    case MetaBlock.Uncompressed u:
                        detail = $"Uncompressed, {u.DataLength.UncompressedBytes} B";
                        break;

                    case MetaBlock.Compressed c:
                        detail = $"Compressed, {c.DataLength.UncompressedBytes} B";
                        break;

                    default:
                        detail = "Unknown";
                        break;
                }

                return $"Meta-Block ({detail})";
            }
        }
    }
}
