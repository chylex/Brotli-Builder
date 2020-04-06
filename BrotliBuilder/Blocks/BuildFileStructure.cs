using System;
using System.Linq;
using System.Windows.Forms;
using BrotliBuilder.Blocks.Structure;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Parameters;

namespace BrotliBuilder.Blocks{
    partial class BuildFileStructure : UserControl{
        private readonly IBuildingBlockContext context;
        private readonly BrotliFileStructure brotliFile;

        public BuildFileStructure(IBuildingBlockContext context, BrotliFileStructure brotliFile){
            InitializeComponent();

            this.context = context;
            this.context.Notified += context_Notified;
            this.brotliFile = brotliFile;

            RegenerateElementList(selectMetaBlock: null, notifyParent: false);
        }

        private void RegenerateElementList(MetaBlock? selectMetaBlock, bool notifyParent){
            listElements.Items.Clear();
            listElements.Items.Add(new StructureWindowSizeItem(brotliFile));

            foreach(MetaBlock metaBlock in brotliFile.MetaBlocks){
                listElements.Items.Add(new StructureMetaBlockItem(brotliFile, metaBlock));
            }

            listElements.SelectedIndex = selectMetaBlock == null ? -1 : brotliFile.MetaBlocks.IndexOf(selectMetaBlock) + 1;

            if (notifyParent){
                context.NotifyParent(EventArgs.Empty);
            }
        }

        private void context_Notified(object? sender, EventArgs e){
            if (e is BuildWindowSize.WindowSizeNotifyArgs wsna){
                brotliFile.Parameters = new BrotliFileParameters.Builder(brotliFile.Parameters){ WindowSize = wsna.NewWindowSize }.Build();
            }
            else if (listElements.SelectedItem is StructureMetaBlockItem smbi){
                smbi.HandleNotification(e);
            }

            brotliFile.Fixup();
            
            if (listElements.SelectedItem == null){
                listElements.SelectedItem = listElements.Items[^1];
            }
            else{
                listElements.SelectedValueChanged -= listElements_SelectedValueChanged;
                listElements.Items[listElements.SelectedIndex] = listElements.SelectedItem; // update item text
                listElements.SelectedValueChanged += listElements_SelectedValueChanged;
            }
        }

        private void listElements_SelectedValueChanged(object? sender, EventArgs e){
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
            RegenerateElementList(selectMetaBlock: metaBlock, notifyParent: true);
        }

        private void buttonAddMetaBlockCompressed_Click(object? sender, EventArgs e){
            // TODO
        }

        private void buttonAddMetaBlockUncompressed_Click(object? sender, EventArgs e){
            AddMetaBlock(new MetaBlock.Uncompressed(Array.Empty<byte>()));
        }

        private void buttonAddMetaBlockEmpty_Click(object? sender, EventArgs e){
            if (listElements.SelectedIndex == listElements.Items.Count - 1 && !listElements.Items.OfType<StructureMetaBlockItem>().Any(item => item.Value is MetaBlock.LastEmpty)){
                AddMetaBlock(new MetaBlock.LastEmpty());
            }
            else{
                AddMetaBlock(new MetaBlock.PaddedEmpty(Array.Empty<byte>()));
            }
        }

        private void MoveMetaBlock(bool up){
            if (listElements.SelectedItem is StructureMetaBlockItem item){
                var metaBlock = item.Value;
                int currentIndex = brotliFile.MetaBlocks.IndexOf(metaBlock);
                int newIndex = currentIndex + (up ? -1 : 1);

                if (newIndex >= 0 && newIndex < brotliFile.MetaBlocks.Count){
                    brotliFile.MetaBlocks.RemoveAt(currentIndex);
                    brotliFile.MetaBlocks.Insert(newIndex, metaBlock);
                    brotliFile.Fixup();
                    RegenerateElementList(selectMetaBlock: metaBlock, notifyParent: true);
                }
            }
        }

        private void buttonMoveMetaBlockUp_Click(object? sender, EventArgs e){
            MoveMetaBlock(true);
        }

        private void buttonMoveMetaBlockDown_Click(object? sender, EventArgs e){
            MoveMetaBlock(false);
        }

        private void buttonDeleteMetaBlock_Click(object? sender, EventArgs e){
            if (listElements.SelectedItem is StructureMetaBlockItem item &&
                MessageBox.Show($"Are you sure you want to permanently delete this meta-block?{Environment.NewLine}{item}", "Delete Meta-Block", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes
            ){
                var metaBlockAbove = listElements.SelectedIndex > 1 ? brotliFile.MetaBlocks[listElements.SelectedIndex - 2] : null;

                brotliFile.MetaBlocks.Remove(item.Value);
                brotliFile.Fixup();

                RegenerateElementList(selectMetaBlock: metaBlockAbove, notifyParent: true);
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
            public MetaBlock Value { get; private set; }

            private readonly BrotliFileStructure brotliFile;

            public StructureMetaBlockItem(BrotliFileStructure brotliFile, MetaBlock value){
                this.brotliFile = brotliFile;
                this.Value = value;
            }

            private void ReplaceMetaBlock(MetaBlock newMetaBlock){
                brotliFile.MetaBlocks[brotliFile.MetaBlocks.IndexOf(Value)] = Value = newMetaBlock;
            }

            public Func<IBuildingBlockContext, UserControl>? CreateStructureBlock(){
                return Value switch{
                    MetaBlock.PaddedEmpty pe => ctx => new BuildEmptyMetaBlock(ctx, pe),
                    MetaBlock.Uncompressed u => ctx => new BuildUncompressedMetaBlock(ctx, u),
                    MetaBlock.Compressed   c => ctx => new BuildCompressedMetaBlock(c),
                    _                        => null
                };
            }

            public void HandleNotification(EventArgs args){
                switch(args){
                    case BuildEmptyMetaBlock.HiddenBytesNotifyArgs hbna:
                        ReplaceMetaBlock(new MetaBlock.PaddedEmpty(hbna.Bytes));
                        break;

                    case BuildUncompressedMetaBlock.UncompressedBytesNotifyArgs ubna:
                        ReplaceMetaBlock(new MetaBlock.Uncompressed(ubna.Bytes));
                        break;
                }
            }

            public override string ToString(){
                static string Number(int n){
                    return n.ToString("N0", Program.Culture);
                }

                var detail = Value switch{
                    MetaBlock.LastEmpty    _ => "Empty, Last",
                    MetaBlock.PaddedEmpty pe => pe.HiddenDataLength == 0 ? "Empty, Padded" : $"Empty, Skip {Number(pe.HiddenDataLength)} B",
                    MetaBlock.Uncompressed u => $"Uncompressed, {Number(u.DataLength.UncompressedBytes)} B",
                    MetaBlock.Compressed   c => $"Compressed, {Number(c.DataLength.UncompressedBytes)} B",
                    _ => "Unknown"
                };

                return $"Meta-Block ({detail})";
            }
        }
    }
}
