﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using BrotliBuilder.Blocks;
using BrotliBuilder.Dialogs;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;

namespace BrotliBuilder{
    partial class FormMain : Form{
        private const int LimitOutputLength = 16384;

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
                    owner.RegenerateBrotliStream(markAsDirty: true);
                }
                else{
                    parent.NotifyParent(args);
                }
            }
        }

        #endregion

        private BrotliFileStructure brotliFile = BrotliFileStructure.NewEmpty();
        private string lastFileName = "compressed";
        private bool isDirty = false;
        
        public FormMain(){
            InitializeComponent();
            
            menuItemLimitOutput_Click(this, EventArgs.Empty);
            splitContainerOuter.Panel2Collapsed = true;
            OnNewBrotliFile();
        }

        #region File state handling

        private void LoadExistingBrotliFile(byte[] bytes){
            splitContainerOuter.Panel2Collapsed = false;
            statusBarPanelTimeBits.Text = "Decompressing...";
            statusBarPanelTimeOutput.Text = "Decompressing...";

            flowPanelBlocks.Controls.Clear();
            brotliFilePanelGenerated.InvalidatePanel();

            brotliFilePanelOriginal.LoadBrotliFile(bytes, file => {
                brotliFile = file;
                OnNewBrotliFile();
            });
        }

        private void OnNewBrotliFile(){
            flowPanelBlocks.Controls.Clear();
            flowPanelBlocks.Controls.Add(new BuildFileStructure(new BuildingBlockContext(this, flowPanelBlocks), brotliFile));

            RegenerateBrotliStream(markAsDirty: false);
            isDirty = false;
        }
        
        private void timerRegenerationDelay_Tick(object sender, EventArgs e){
            timerRegenerationDelay.Stop();
            
            statusBarPanelTimeBits.Text = "Generating...";
            statusBarPanelTimeOutput.Text = "Generating...";

            brotliFilePanelGenerated.LoadBrotliFile(
                brotliFile,

                onSerializedStopwatch =>
                    statusBarPanelTimeBits.Text = onSerializedStopwatch == null ?
                        "Error generating bit stream." :
                        "Generated bit stream in " + onSerializedStopwatch.ElapsedMilliseconds + " ms.",

                onDecompressedStopwatch =>
                    statusBarPanelTimeOutput.Text = onDecompressedStopwatch == null ?
                        "Error generating output." :
                        "Generated output in " + onDecompressedStopwatch.ElapsedMilliseconds + " ms."
            );
        }

        private void RegenerateBrotliStream(bool markAsDirty){
            isDirty = isDirty || markAsDirty;
            timerRegenerationDelay.Stop();
            timerRegenerationDelay.Start();
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
                menuItemSave_Click(null, EventArgs.Empty); // sets isDirty to false on success

                if (isDirty){
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Form events

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e){
            e.Cancel = PromptUnsavedChanges("Would you like to save changes before exiting?");
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

                    try{
                        LoadExistingBrotliFile(File.ReadAllBytes(lastFileName));
                    }catch(Exception ex){
                        Debug.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void menuItemSave_Click(object sender, EventArgs e){
            using(SaveFileDialog dialog = new SaveFileDialog{
                Title = "Save Compressed File",
                Filter = "Brotli (*.br)|*.br|All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName),
                DefaultExt = "br"
            }){
                if (dialog.ShowDialog() == DialogResult.OK){
                    lastFileName = dialog.FileName;

                    File.WriteAllBytes(lastFileName, brotliFile.Serialize().ToByteArray());
                    isDirty = false;
                }
            }
        }

        private void menuItemExit_Click(object sender, EventArgs e){
            Close();
        }

        #endregion

        #region Menu events (View)

        private void menuItemWrapOutput_Click(object sender, EventArgs e){
            bool enable = !menuItemWrapOutput.Checked;
            menuItemWrapOutput.Checked = enable;

            brotliFilePanelGenerated.WordWrapOutput = brotliFilePanelOriginal.WordWrapOutput = enable;
        }

        private void menuItemLimitOutput_Click(object sender, EventArgs e){
            bool enable = !menuItemLimitOutput.Checked;
            menuItemLimitOutput.Checked = enable;

            brotliFilePanelGenerated.MaxLength = brotliFilePanelOriginal.MaxLength = enable ? LimitOutputLength : -1;
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

        private void menuItemEncodeUncompressedMBs_Click(object sender, EventArgs e){
            OpenFileWithEncoder(WindowSize.Default, new EncodeUncompressedOnly());
        }

        private void OpenFileWithEncoder(WindowSize windowSize, IBrotliEncoder encoder){
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

                    byte[] bytes;

                    try{
                        bytes = File.ReadAllBytes(lastFileName);
                    }catch(Exception ex){
                        Debug.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "File Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    splitContainerOuter.Panel2Collapsed = true;
                    
                    try{
                        brotliFile = BrotliFileStructure.FromEncoder(windowSize, encoder, bytes);
                        OnNewBrotliFile();
                    }catch(Exception ex){
                        Debug.WriteLine(ex.ToString());
                        MessageBox.Show(ex.Message, "File Encode Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        #endregion
        
    }
}
