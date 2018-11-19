using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrotliBuilder.Blocks;
using BrotliBuilder.Utils;
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

        private readonly AsyncWorker<string> workerStream;
        private readonly AsyncWorker<string> workerOutput;

        public FormMain(){
            InitializeComponent();

            textBoxBitStream.SetPlainTextMode();
            textBoxDecompressedOutput.SetPlainTextMode();

            workerStream = new AsyncWorker<string>("GenBits");
            workerOutput = new AsyncWorker<string>("GenOutput");

            SetupWorker(
                workerStream,
                textBoxBitStream,
                statusBarPanelTimeBits,
                ms => "Generated bit stream in " + ms + " ms.",
                () => "Error generating bit stream."
            );

            SetupWorker(
                workerOutput,
                textBoxDecompressedOutput,
                statusBarPanelTimeOutput,
                ms => "Generated output in " + ms + " ms.",
                () => "Error generating output."
            );

            OnNewBrotliFile();
        }

        #region File state handling

        private void OnNewBrotliFile(){
            flowPanelBlocks.Controls.Clear();
            flowPanelBlocks.Controls.Add(new BuildFileStructure(new BuildingBlockContext(this, flowPanelBlocks), brotliFile));

            RegenerateBrotliStream(markAsDirty: false);
            isDirty = false;
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

        #region Output generation

        private void SetupWorker(AsyncWorker<string> worker, RichTextBox tb, StatusBarPanel status, Func<long, string> funcStatusTextSuccess, Func<string> funcStatusTextFailure){
            worker.WorkFinished += (sender, args) => {
                string result = args.Result;

                if (menuItemLimitOutput.Checked && result.Length > LimitOutputLength){
                    result = result.Substring(0, LimitOutputLength) + "(...)";
                }

                tb.ForeColor = SystemColors.WindowText;
                tb.Text = result;
                status.Text = funcStatusTextSuccess(args.Stopwatch.ElapsedMilliseconds);
            };

            worker.WorkCrashed += (sender, args) => {
                tb.ForeColor = Color.Red;
                tb.Text = Regex.Replace(args.Exception.ToString(), " in (.*):", " : ");
                status.Text = funcStatusTextFailure();
            };
        }

        private void timerRegenerationDelay_Tick(object sender, EventArgs e){
            timerRegenerationDelay.Stop();

            textBoxBitStream.ForeColor = SystemColors.GrayText;
            textBoxDecompressedOutput.ForeColor = SystemColors.GrayText;

            statusBarPanelTimeBits.Text = "Generating...";
            statusBarPanelTimeOutput.Text = "Generating...";

            workerStream.Start(() => brotliFile.Serialize().ToString());
            workerOutput.Start(() => brotliFile.GetDecompressionState().OutputAsUTF8);
        }

        private void RegenerateBrotliStream(bool markAsDirty){
            isDirty = isDirty || markAsDirty;
            timerRegenerationDelay.Stop();
            timerRegenerationDelay.Start();
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
                        brotliFile = BrotliFileStructure.FromBytes(File.ReadAllBytes(lastFileName));
                        OnNewBrotliFile();
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

        private void menuItemLimitOutput_Click(object sender, EventArgs e){
            menuItemLimitOutput.Checked = !menuItemLimitOutput.Checked;
            RegenerateBrotliStream(markAsDirty: false);
        }

        #endregion

        #region Menu events (Tools)

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
