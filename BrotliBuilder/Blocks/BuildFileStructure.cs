using System;
using System.Linq;
using System.Windows.Forms;
using BrotliBuilder.Blocks.Structure;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;

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
                brotliFile.WindowSize = wsna.NewWindowSize;
            }
            else if (listElements.SelectedItem is StructureMetaBlockItem smbi){
                smbi.HandleNotification(e);
            }
            
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
                context.SetChildBlock(ctx => new BuildWindowSize(ctx, brotliFile.WindowSize));
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
            RegenerateElementList(selectIndex: insertAt + 1, notifyParent: true);
        }

        private void buttonAddMetaBlockCompressed_Click(object sender, EventArgs e){
            // TODO
        }

        private void buttonAddMetaBlockUncompressed_Click(object sender, EventArgs e){
            // TODO
        }

        private void buttonAddMetaBlockEmpty_Click(object sender, EventArgs e){
            if (listElements.SelectedIndex == listElements.Items.Count - 1 && !listElements.Items.OfType<StructureMetaBlockItem>().Any(item => item.Value is MetaBlock.LastEmpty)){
                AddMetaBlock(new MetaBlock.LastEmpty());
            }
            else{
                // TODO
            }
        }

        private void MoveMetaBlock(bool up){
            if (listElements.SelectedItem is StructureMetaBlockItem item){
                int currentIndex = brotliFile.MetaBlocks.IndexOf(item.Value);
                int newIndex = currentIndex + (up ? -1 : 1);

                if (newIndex >= 0 && newIndex < brotliFile.MetaBlocks.Count){
                    brotliFile.MetaBlocks.RemoveAt(currentIndex);
                    brotliFile.MetaBlocks.Insert(newIndex, item.Value);
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
            if (listElements.SelectedItem is StructureMetaBlockItem item){
                brotliFile.MetaBlocks.Remove(item.Value);
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
                return $"Window Size ({brotliFile.WindowSize.Bits} bits)";
            }
        }

        private class StructureMetaBlockItem{
            public MetaBlock Value { get; }

            public StructureMetaBlockItem(MetaBlock value){
                this.Value = value;
            }

            public Func<IBuildingBlockContext, UserControl> CreateStructureBlock(){
                switch(Value){
                    // TODO

                    default:
                        return null;
                }
            }

            public void HandleNotification(EventArgs args){
                switch(args){
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
