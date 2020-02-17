using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrotliBuilder.Blocks;
using BrotliBuilder.Components;
using BrotliBuilder.State;
using BrotliBuilder.Utils;
using BrotliBuilder.Utils.Compat;
using BrotliLib.Brotli;

namespace BrotliBuilder{
    partial class FormMain : Form{
        private string lastFileName = "compressed";
        private bool isDirty = false;

        private byte[]? lastOriginalFileBytes;
        private BrotliFileStructure? lastGeneratedFile;
        private bool skipNextBlockRegeneration = false;
        private bool skipNextOriginalToGeneratedFeed = false;

        private readonly BrotliFileController fileGenerated;
        private readonly BrotliFileController fileOriginal;
        
        public FormMain(){
            InitializeComponent();
            SuspendLayout();

            var mainMenu = DeprecatedControls.CreateMainMenu(this, components);
            var menuItemFile = mainMenu.AddItem("&File");
            var menuItemView = mainMenu.AddItem("&View");
            var menuItemTools = mainMenu.AddItem("&Tools");
            var menuItemEncode = mainMenu.AddItem("&Encode");
            var menuItemTransform = mainMenu.AddItem("Transfor&m");

            InitializeMenuFile(menuItemFile);
            InitializeMenuView(menuItemView);
            InitializeMenuTools(menuItemTools);
            InitializeMenuEncoders(menuItemEncode);
            InitializeMenuTransformers(menuItemTransform);

            ResumeLayout(true);

            this.fileGenerated = new BrotliFileController(brotliFilePanelGenerated.Title);
            this.fileGenerated.StateChanged += FileGenerated_StateChanged;

            this.fileOriginal = new BrotliFileController(brotliFilePanelOriginal.Title);
            this.fileOriginal.StateChanged += FileOriginal_StateChanged;
            
            fileGenerated.ResetToEmpty();
            fileOriginal.ResetToNothing();
        }

        // Status bars

        private void ResetStatusBars(string text){
            statusBarPanelTimeStructure.Text = text;
            statusBarPanelTimeBits.Text = text;
            statusBarPanelTimeOutput.Text = text;
        }

        private void UpdateStatusBar(StatusBarPanel bar, string type, Stopwatch? sw){
            if (sw != null){
                bar.Text = "Generated " + type + " in " + sw.ElapsedMilliseconds + " ms.";
            }
            else{
                bar.Text = "Loaded " + type + ".";
            }
        }

        // File handling

        private void HandleFileError(BrotliFileState.Error error, BrotliFilePanel panel){
            Exception ex = error.Exception;
            string message = ex.Message;

            switch(error.Type){
                case ErrorType.ReadingFile:
                    MessageBox.Show(message, "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case ErrorType.EncodingBytes:
                    MessageBox.Show(message, "Encoder Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case ErrorType.TransformingStructure:
                    MessageBox.Show(message, "Transformer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;

                case ErrorType.DeserializingFile:
                case ErrorType.SerializingStructure:
                    panel.UpdateBits(ex);
                    break;

                case ErrorType.DecompressingStructure:
                    panel.UpdateOutput(ex);
                    break;
            }
        }

        private void CheckOutputMatches(BrotliFileState.HasOutput hasOutput){
            var prev = hasOutput.PreviousBytes;
            var gen = hasOutput.OutputBytes;

            if (prev == null){
                return;
            }

            bool sameLength = prev.Length == gen.Length;

            if (sameLength && prev.SequenceEqual(gen)){
                return;
            }

            static string BytesText(int n) => n + (n == 1 ? " byte" : " bytes");
            string sizeMessage = sameLength ? "same length" : "previously " + BytesText(prev.Length) + ", now " + BytesText(gen.Length);

            if (MessageBox.Show("Found mismatched output (" + sizeMessage + "). Would you like to compare?", "Output Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes){
                WinMerge.CompareText(Encoding.UTF8.GetString(prev), Encoding.UTF8.GetString(gen));
            }
        }

        private void FileGenerated_StateChanged(object? sender, StateChangedEventArgs e){
            BrotliFilePanel filePanel = brotliFilePanelGenerated;

            switch(e.To){
                case BrotliFileState.NoFile _:
                    menuItemCompareMarkers!.Enabled = false;
                    menuItemCloneGeneratedToOriginal!.Enabled = false;
                    break;

                case BrotliFileState.Starting _:
                    if (!skipNextBlockRegeneration){
                        flowPanelBlocks.Controls.Clear();
                    }

                    filePanel.InvalidatePanel();
                    ResetStatusBars("Generating...");
                    break;

                case BrotliFileState.Waiting _:
                    filePanel.InvalidatePanel();
                    break;

                case BrotliFileState.HasStructure hasStructure:
                    RegenerateBuildingBlocks(hasStructure.File);
                    UpdateStatusBar(statusBarPanelTimeStructure, "structure", hasStructure.Stopwatch);
                    break;

                case BrotliFileState.HasBits hasBits:
                    filePanel.UpdateBits(hasBits);
                    UpdateStatusBar(statusBarPanelTimeBits, "bits", hasBits.Stopwatch);
                    break;

                case BrotliFileState.HasMarkers hasMarkers:
                    filePanel.UpdateMarkers(hasMarkers);
                    break;

                case BrotliFileState.HasOutput hasOutput:
                    filePanel.UpdateOutput(hasOutput);
                    UpdateStatusBar(statusBarPanelTimeOutput, "output", hasOutput.Stopwatch);
                    CheckOutputMatches(hasOutput);
                    break;

                case BrotliFileState.Loaded loaded:
                    filePanel.FinalizeOutput(loaded);

                    lastGeneratedFile = loaded.File;
                    menuItemCloneGeneratedToOriginal!.Enabled = true;

                    if (brotliFilePanelOriginal.MarkerRoot != null && brotliFilePanelGenerated.MarkerRoot != null){
                        menuItemCompareMarkers!.Enabled = true;
                    }

                    break;

                case BrotliFileState.Error error:
                    HandleFileError(error, filePanel);
                    break;
            }
        }

        private void FileOriginal_StateChanged(object? sender, StateChangedEventArgs e){
            BrotliFilePanel filePanel = brotliFilePanelOriginal;

            switch(e.To){
                case BrotliFileState.NoFile _:
                    filePanel.ResetPanel();
                    splitContainerRightBottom.Panel2Collapsed = true;

                    menuItemCloseOriginal!.Enabled = false;
                    menuItemCompareMarkers!.Enabled = false;
                    menuItemCloneOriginalToGenerated!.Enabled = false;

                    lastOriginalFileBytes = null;
                    return;

                case BrotliFileState.Starting _:
                    if (!skipNextOriginalToGeneratedFeed){
                        flowPanelBlocks.Controls.Clear();
                        fileGenerated.ResetToWaiting();
                    }

                    filePanel.InvalidatePanel();
                    ResetStatusBars("Loading...");

                    isDirty = false;
                    skipNextBlockRegeneration = false;
                    break;

                case BrotliFileState.Waiting _:
                    filePanel.InvalidatePanel();
                    break;

                case BrotliFileState.HasBits hasBits:
                    filePanel.UpdateBits(hasBits);
                    break;

                case BrotliFileState.HasMarkers hasMarkers:
                    filePanel.UpdateMarkers(hasMarkers);
                    break;

                case BrotliFileState.HasOutput hasOutput:
                    filePanel.UpdateOutput(hasOutput);
                    CheckOutputMatches(hasOutput);
                    lastOriginalFileBytes = hasOutput.OutputBytes;
                    break;

                case BrotliFileState.Loaded loaded:
                    filePanel.FinalizeOutput(loaded);

                    if (skipNextOriginalToGeneratedFeed){
                        skipNextOriginalToGeneratedFeed = false;
                    }
                    else{
                        fileGenerated.LoadStructure(loaded.File, lastOriginalFileBytes);
                    }
                    
                    menuItemCloseOriginal!.Enabled = true;
                    menuItemCloneOriginalToGenerated!.Enabled = true;
                    break;

                case BrotliFileState.Error error:
                    HandleFileError(error, filePanel);
                    break;
            }

            splitContainerRightBottom.Panel2Collapsed = false; // skipped for State.NoFile
        }

        private bool PromptUnsavedChanges(string message){
            if (!isDirty){
                return false;
            }

            DialogResult result = MessageBox.Show(message, "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel){
                return true;
            }
            else if (result == DialogResult.Yes){
                SaveBrotli(); // sets isDirty to false on success

                if (isDirty){
                    return true;
                }
            }

            return false;
        }

        // Main events

        private void FormMain_FormClosing(object? sender, FormClosingEventArgs e){
            e.Cancel = PromptUnsavedChanges("Would you like to save changes before exiting?");
        }

        private void FormMain_Resize(object? sender, EventArgs e){
            int statusBarPanelWidth = Width < 645 ? 200 - Math.Min(100, (645 - Width) / 3) : 200;

            statusBarPanelTimeStructure.Width = statusBarPanelWidth;
            statusBarPanelTimeBits.Width = statusBarPanelWidth;
            statusBarPanelTimeOutput.Width = statusBarPanelWidth;
        }

        private void flowPanelBlocks_ControlAdded(object? sender, ControlEventArgs e){
            e.Control.Height = flowPanelBlocks.ClientSize.Height - 8;
        }

        private void flowPanelBlocks_SizeChanged(object? sender, EventArgs e){
            flowPanelBlocks.SetChildHeight(flowPanelBlocks.ClientSize.Height - 8);
        }
        
        private void brotliFilePanel_MarkersUpdated(object? sender, MarkedTextBox.MarkerUpdateEventArgs e){
            brotliMarkerInfoPanel.UpdateMarkers(e.Title, e.MarkerRoot, e.MarkerSequence, e.HighlightedNodes, e.CaretNode);
        }

        // Building blocks

        private void RegenerateBuildingBlocks(BrotliFileStructure file){
            if (skipNextBlockRegeneration){
                skipNextBlockRegeneration = false;
                return;
            }

            flowPanelBlocks.Controls.Clear();
            flowPanelBlocks.Controls.Add(new BuildFileStructure(new BuildingBlockContext(this, flowPanelBlocks), file));
        }

        private void RegenerateBrotliStream(){
            isDirty = true;
            skipNextBlockRegeneration = true;
            timerRegenerationDelay.Stop();
            timerRegenerationDelay.Start();
        }
        
        private void timerRegenerationDelay_Tick(object? sender, EventArgs e){
            timerRegenerationDelay.Stop();

            if (lastGeneratedFile != null){
                fileGenerated.LoadStructure(lastGeneratedFile);
            }
        }

        private class BuildingBlockContext : IBuildingBlockContext{
            public event EventHandler<EventArgs>? Notified;

            private readonly FormMain owner;
            private readonly Panel container;
            private readonly IBuildingBlockContext? parent;
            private readonly int depth;

            public BuildingBlockContext(FormMain owner, Panel container, IBuildingBlockContext? parent = null, int depth = 0){
                this.owner = owner;
                this.container = container;
                this.parent = parent;
                this.depth = depth;
            }

            public void SetChildBlock(Func<IBuildingBlockContext, UserControl>? blockFactory){
                int childDepth = depth + 1;
                var controls = container.Controls;
                
                while(childDepth < controls.Count){
                    controls.RemoveAt(childDepth);
                }

                if (blockFactory != null){
                    controls.Add(blockFactory(new BuildingBlockContext(owner, container, this, childDepth)));
                }
            }

            public void NotifyParent(EventArgs args){
                Notified?.Invoke(container.Controls[depth], args);

                if (parent == null){
                    owner.RegenerateBrotliStream();
                }
                else{
                    parent.NotifyParent(args);
                }
            }
        }
    }
}
