using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Utils;

namespace BrotliBuilder.Blocks.Structure{
    partial class BuildCompressedMetaBlock : UserControl{
        private const int ListBatchSize = 2500;

        private readonly List<object> allItems = new List<object>();
        private int listCursor = 0;

        public BuildCompressedMetaBlock(MetaBlock.Compressed metaBlock){
            InitializeComponent();

            var blocks = new CategoryMap<BlockSwitchTrackerCustom>(category => new BlockSwitchTrackerCustom(metaBlock, category, allItems));
            var data = metaBlock.Data;

            foreach(var command in data.InsertCopyCommands){
                blocks[Category.InsertCopy].Advance();

                for(int literal = 0; literal < command.Literals.Count; literal++){
                    blocks[Category.Literal].Advance();
                }
                
                if (command.CopyDistance.HasExplicitDistanceCode()){
                    blocks[Category.Distance].Advance();
                }

                allItems.Add(new InsertCopyCommandItem(command, allItems.Count + 1));
            }

            SuspendLayout();

            checkBoxInsertCopy.Text = $"Insert&&Copy ({data.InsertCopyCommands.Count})";
            checkBoxBlockSwitchL.Text = $"Block-Switch/L ({data.BlockSwitchCommands[Category.Literal].Count})";
            checkBoxBlockSwitchI.Text = $"Block-Switch/I ({data.BlockSwitchCommands[Category.InsertCopy].Count})";
            checkBoxBlockSwitchD.Text = $"Block-Switch/D ({data.BlockSwitchCommands[Category.Distance].Count})";

            RefreshItems();
            ResumeLayout(true);
        }

        private sealed class BlockSwitchTrackerCustom : BlockSwitchTracker{
            private readonly Category category;
            private readonly List<object> list;
            private readonly Queue<BlockSwitchCommand> queue;

            public BlockSwitchTrackerCustom(MetaBlock.Compressed metaBlock, Category category, List<object> list) : base(metaBlock.Header.BlockTypes[category]){
                this.category = category;
                this.list = list;
                this.queue = new Queue<BlockSwitchCommand>(metaBlock.Data.BlockSwitchCommands[category]);
            }

            protected override BlockSwitchCommand GetNextCommand(){
                var nextCommand = queue.Dequeue();
                list.Add(new BlockSwitchCommandItem(category, nextCommand, list.Count + 1));
                return nextCommand;
            }
        }

        private void RefreshItems(){
            var blockSwitchChecks = new CategoryMap<bool>(category => category switch{
                Category.Literal    => checkBoxBlockSwitchL.Checked,
                Category.InsertCopy => checkBoxBlockSwitchI.Checked,
                Category.Distance   => checkBoxBlockSwitchD.Checked,
                _ => throw new InvalidOperationException()
            });

            var counter = -listCursor * ListBatchSize;
            var keptItems = new List<object>();

            if (listCursor > 0){
                keptItems.Add(new MoveListViewCursor(0, MoveListViewCursor.Dir.Top, "(jump to first item)"));
                keptItems.Add(new MoveListViewCursor(listCursor - 1, MoveListViewCursor.Dir.Bottom, $"(see previous {ListBatchSize} items)"));
            }

            bool Keep(object item){
                ++counter;

                if (counter <= 0){
                    return true;
                }
                else if (counter > ListBatchSize){
                    int nextCursor = listCursor + 1;
                    int lastCursor = ((allItems.Count + ListBatchSize - 1) / ListBatchSize) - 1;

                    keptItems.Add(new MoveListViewCursor(nextCursor, MoveListViewCursor.Dir.Top, $"(see next {ListBatchSize} items)"));
                    keptItems.Add(new MoveListViewCursor(lastCursor, MoveListViewCursor.Dir.Bottom, "(jump to last item)"));
                    return false;
                }
                else{
                    keptItems.Add(item);
                    return true;
                }
            }

            foreach(var item in allItems){
                if (item is InsertCopyCommandItem){
                    if (checkBoxInsertCopy.Checked && !Keep(item)){
                        break;
                    }
                }
                else if (item is BlockSwitchCommandItem bsci){
                    if (blockSwitchChecks[bsci.Category] && !Keep(item)){
                        break;
                    }
                }
            }
            
            listElements.Items.Clear();
            listElements.Items.AddRange(keptItems.ToArray());
        }

        private void listElements_SelectedValueChanged(object sender, EventArgs e){
            if (listElements.SelectedItem is MoveListViewCursor mlvc){
                listElements.ClearSelected();
                listCursor = mlvc.NewCursorPosition;
                RefreshItems();

                var items = listElements.Items.Count;

                if (items == 0){
                    return;
                }

                if (mlvc.HighlightDirection == MoveListViewCursor.Dir.Bottom){
                    var end = items - 1;
                    listElements.TopIndex = end;
                    listElements.SelectedIndex = end - Enumerable.Range(0, items).FirstOrDefault(index => !(listElements.Items[end - index] is MoveListViewCursor));
                }
                else{
                    listElements.SelectedIndex = Enumerable.Range(0, items).FirstOrDefault(index => !(listElements.Items[index] is MoveListViewCursor));
                }
            }
        }

        private void checkBox_CheckedChanged(object sender, EventArgs e){
            listCursor = 0;
            RefreshItems();
        }

        // List items
        
        private class InsertCopyCommandItem{
            private InsertCopyCommand Cmd { get; }
            private int Ordinal { get; }
            
            public InsertCopyCommandItem(InsertCopyCommand cmd, int ordinal){
                this.Cmd = cmd;
                this.Ordinal = ordinal;
            }

            public override string ToString(){
                return $"{Ordinal}. Insert&Copy : insert length = {Cmd.Literals.Count}, copy length = {Cmd.CopyLength}, distance = {Cmd.CopyDistance}";
            }
        }
        
        private class BlockSwitchCommandItem{
            public Category Category { get; }
            private BlockSwitchCommand Cmd { get; }
            private int Ordinal { get; }
            
            public BlockSwitchCommandItem(Category category, BlockSwitchCommand cmd, int ordinal){
                this.Category = category;
                this.Cmd = cmd;
                this.Ordinal = ordinal;
            }

            public override string ToString(){
                return $"{Ordinal}. Block-Switch/{Category.Id()} : block type = {Cmd.Type}, block length = {Cmd.Length}";
            }
        }

        private class MoveListViewCursor{
            public enum Dir{
                Top, Bottom
            }

            public int NewCursorPosition { get; }
            public Dir HighlightDirection { get; }
            private string Title { get; }

            public MoveListViewCursor(int newCursorPosition, Dir highlightDirection, string title){
                this.NewCursorPosition = newCursorPosition;
                this.HighlightDirection = highlightDirection;
                this.Title = title;
            }

            public override string ToString(){
                return Title;
            }
        }
    }
}
