using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using BrotliBuilder.Blocks;
using BrotliBuilder.Components;
using BrotliBuilder.Dialogs;
using BrotliBuilder.State;
using BrotliBuilder.Utils;
using BrotliImpl.Encoders;
using BrotliImpl.Transformers;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;

namespace BrotliBuilder{
    partial class FormMain : Form{

        #region Building block context

        private class BuildingBlockContext : IBuildingBlockContext{
            public event EventHandler<EventArgs> Notified;

            private readonly FormMain owner;
            private readonly Panel container;
            private readonly IBuildingBlockContext parent;
            private readonly int depth;

            public BuildingBlockContext(FormMain owner, Panel container, IBuildingBlockContext parent = null, int depth = 0){
                this.owner = owner;
                this.container = container;
                this.parent = parent;
                this.depth = depth;
            }

            public void SetChildBlock(Func<IBuildingBlockContext, UserControl> blockFactory){
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

        #endregion

        private string lastFileName = "compressed";
        private bool isDirty = false;

        private BrotliFileStructure lastGeneratedFile;
        private bool skipNextBlockRegeneration = false;

        private readonly BrotliFileController fileGenerated;
        private readonly BrotliFileController fileOriginal;
        
        public FormMain(){
            InitializeComponent();

            this.fileGenerated = new BrotliFileController(brotliFilePanelGenerated.Title);
            this.fileGenerated.StateChanged += FileGenerated_StateChanged;

            this.fileOriginal = new BrotliFileController(brotliFilePanelOriginal.Title);
            this.fileOriginal.StateChanged += FileOriginal_StateChanged;
            
            fileGenerated.ResetToEmpty();
            fileOriginal.ResetToNothing();
        }

        #region File state handling

        private void ResetStatusBars(string text){
            statusBarPanelTimeStructure.Text = text;
            statusBarPanelTimeBits.Text = text;
            statusBarPanelTimeOutput.Text = text;
        }

        private void UpdateStatusBar(StatusBarPanel bar, string type, Stopwatch sw){
            if (sw != null){
                bar.Text = "Generated " + type + " in " + sw.ElapsedMilliseconds + " ms.";
            }
            else{
                bar.Text = "Loaded " + type + ".";
            }
        }

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

        private void FileGenerated_StateChanged(object sender, StateChangedEventArgs e){
            BrotliFilePanel filePanel = brotliFilePanelGenerated;

            switch(e.To){
                case BrotliFileState.NoFile _:
                    menuItemCompareMarkers.Enabled = false;
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

                case BrotliFileState.Loaded loaded:
                    filePanel.UpdateOutput(loaded);
                    UpdateStatusBar(statusBarPanelTimeOutput, "output", loaded.Stopwatch);

                    lastGeneratedFile = loaded.File;

                    if (brotliFilePanelOriginal.MarkerSequence != null && brotliFilePanelGenerated.MarkerSequence != null){
                        menuItemCompareMarkers.Enabled = true;
                    }

                    break;

                case BrotliFileState.Error error:
                    HandleFileError(error, filePanel);
                    break;
            }
        }

        private void FileOriginal_StateChanged(object sender, StateChangedEventArgs e){
            BrotliFilePanel filePanel = brotliFilePanelOriginal;

            switch(e.To){
                case BrotliFileState.NoFile _:
                    filePanel.ResetPanel();
                    splitContainerRightBottom.Panel2Collapsed = true;
                    menuItemCompareMarkers.Enabled = false;
                    return;

                case BrotliFileState.Starting _:
                    flowPanelBlocks.Controls.Clear();
                    fileGenerated.ResetToWaiting();
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

                case BrotliFileState.Loaded loaded:
                    filePanel.UpdateOutput(loaded);
                    fileGenerated.LoadStructure(loaded.File);
                    break;

                case BrotliFileState.Error error:
                    HandleFileError(error, filePanel);
                    break;
            }

            splitContainerRightBottom.Panel2Collapsed = false; // skipped for State.NoFile
        }

        #endregion

        #region Building blocks

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
        
        private void timerRegenerationDelay_Tick(object sender, EventArgs e){
            timerRegenerationDelay.Stop();

            if (lastGeneratedFile != null){
                fileGenerated.LoadStructure(lastGeneratedFile);
            }
        }

        #endregion

        #region Form events

        private bool PromptUnsavedChanges(string message){
            if (!isDirty){
                return false;
            }

            DialogResult result = MessageBox.Show(message, "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (result == DialogResult.Cancel){
                return true;
            }
            else if (result == DialogResult.Yes){
                menuItemSave_Click(null, EventArgs.Empty); // sets isDirty to false on success

                if (isDirty){
                    return true;
                }
            }

            return false;
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e){
            e.Cancel = PromptUnsavedChanges("Would you like to save changes before exiting?");
        }

        #endregion

        #region Control events

        private void flowPanelBlocks_ControlAdded(object sender, ControlEventArgs e){
            e.Control.Height = flowPanelBlocks.ClientSize.Height - 8;
        }

        private void flowPanelBlocks_SizeChanged(object sender, EventArgs e){
            flowPanelBlocks.SetChildHeight(flowPanelBlocks.ClientSize.Height - 8);
        }
        
        private void brotliFilePanel_MarkersUpdated(object sender, MarkedTextBox.MarkerUpdateEventArgs e){
            brotliMarkerInfoPanel.UpdateMarkers(e.MarkerSequence, e.HighlightedNodes, e.CaretNode);
        }

        #endregion

        #region Menu events (File)

        private void menuItemOpen_Click(object sender, EventArgs e){
            if (PromptUnsavedChanges("Would you like to save changes before opening a new file?")){
                return;
            }

            using(OpenFileDialog dialog = new OpenFileDialog{
                Title = "Open Compressed File",
                Filter = "Brotli (*.br)|*.br|All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName),
                DefaultExt = "br"
            }){
                if (dialog.ShowDialog() == DialogResult.OK){
                    lastFileName = dialog.FileName;
                    isDirty = false;
                    skipNextBlockRegeneration = false;

                    fileOriginal.LoadFile(lastFileName);
                }
            }
        }

        private void menuItemSave_Click(object sender, EventArgs e){
            BrotliFileStructure currentFile = fileGenerated.CurrentFile;

            if (currentFile == null){
                MessageBox.Show("No structure loaded.", "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using(SaveFileDialog dialog = new SaveFileDialog{
                Title = "Save Compressed File",
                Filter = "Brotli (*.br)|*.br|All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName),
                DefaultExt = "br"
            }){
                if (dialog.ShowDialog() == DialogResult.OK){
                    lastFileName = dialog.FileName;
                    isDirty = false;

                    File.WriteAllBytes(lastFileName, currentFile.Serialize().ToByteArray());
                }
            }
        }

        private void menuItemExit_Click(object sender, EventArgs e){
            Close();
        }

        #endregion

        #region Menu events (View)

        private void menuItemFileStructure_Click(object sender, EventArgs e){
            splitContainerRight.Panel1Collapsed = !menuItemFileStructure.Toggle();
        }

        private void menuItemMarkerInfo_Click(object sender, EventArgs e){
            bool enable = menuItemMarkerInfo.Toggle();

            splitContainerMain.Panel1Collapsed = !enable;
            fileGenerated.EnableBitMarkers = enable;
            fileOriginal.EnableBitMarkers = enable;

            if (!enable){
                brotliMarkerInfoPanel.ResetMarkers();
            }
        }

        private void menuItemWrapOutput_Click(object sender, EventArgs e){
            brotliFilePanelGenerated.WordWrapOutput = brotliFilePanelOriginal.WordWrapOutput = menuItemWrapOutput.Toggle();
        }

        private void menuItemWrapMarkerInfo_Click(object sender, EventArgs e){
            brotliMarkerInfoPanel.WordWrap = menuItemWrapMarkerInfo.Toggle();
        }

        #endregion
        
        #region Menu events (Tools)

        private void menuItemStaticDictionary_Click(object sender, EventArgs e){
            try{
                using(FormStaticDictionary form = new FormStaticDictionary(BrotliDefaultDictionary.Embedded)){
                    form.ShowDialog();
                }
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message, "Static Dictionary Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void menuItemCompareMarkers_Click(object sender, EventArgs e){
            if (brotliFilePanelOriginal.MarkerSequence == null || brotliFilePanelGenerated.MarkerSequence == null){
                MessageBox.Show("No original file opened.", "Compare Markers Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string originalText = BrotliMarkerInfoPanel.GenerateText(brotliFilePanelOriginal.MarkerSequence);
            string generatedText = BrotliMarkerInfoPanel.GenerateText(brotliFilePanelGenerated.MarkerSequence);

            try{
                WinMerge.CompareText(originalText, generatedText);
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message, "Compare Markers Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Menu events (Encode)

        private void menuItemEncodeUncompressedMBs_Click(object sender, EventArgs e){
            OpenFileWithEncoder(new BrotliFileParameters(), new EncodeUncompressedOnly());
        }
        
        private void menuItemEncodeLiterals_Click(object sender, EventArgs e){
            OpenFileWithEncoder(new BrotliFileParameters(), new EncodeLiterals());
        }

        private void menuItemEncodeGreedySearchOnlyCopies_Click(object sender, EventArgs e){
            OpenFileWithEncoder(new BrotliFileParameters(), new EncodeGreedySearch.OnlyBackReferences(minLength: 4));
        }

        private void menuItemEncodeGreedySearchOnlyDictionary_Click(object sender, EventArgs e){
            OpenFileWithEncoder(new BrotliFileParameters(), new EncodeGreedySearch.OnlyDictionary());
        }

        private void MenuItemEncodeGreedySearchMixed_Click(object sender, EventArgs e){
            OpenFileWithEncoder(new BrotliFileParameters(), new EncodeGreedySearch.Mixed(minCopyLength: 4));
        }

        private void OpenFileWithEncoder(BrotliFileParameters parameters, IBrotliEncoder encoder){
            if (PromptUnsavedChanges("Would you like to save changes before opening a new file?")){
                return;
            }

            using(OpenFileDialog dialog = new OpenFileDialog{
                Title = "Open File to Encode",
                Filter = "All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName)
            }){
                if (dialog.ShowDialog() == DialogResult.OK){
                    lastFileName = dialog.FileName;
                    skipNextBlockRegeneration = false;

                    fileOriginal.ResetToNothing();
                    fileGenerated.EncodeFile(lastFileName, parameters, encoder);
                }
            }
        }

        #endregion

        #region Menu events (Transform)

        private void menuItemTransformRebuild_Click(object sender, EventArgs e){
            TransformCurrentFile(new TransformRebuild());
        }

        private void menuItemTransformTestDistanceParams_Click(object sender, EventArgs e){
            TransformCurrentFile(new TransformTestDistanceParameters());
        }

        private void menuItemTransformSplitInsertCopyLengths_Click(object sender, EventArgs e){
            TransformCurrentFile(new TransformSplitInsertCopyLengths());
        }
        
        private void TransformCurrentFile(IBrotliTransformer transformer){
            skipNextBlockRegeneration = false;

            if (!fileGenerated.Transform(transformer)){
                MessageBox.Show("No structure loaded.", "Transform Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

    }
}
