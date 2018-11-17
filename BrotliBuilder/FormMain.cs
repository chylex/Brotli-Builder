using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrotliBuilder.Blocks;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;

namespace BrotliBuilder{
    partial class FormMain : Form{
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

        // Instance

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

        // File state handling

        private void OnNewBrotliFile(){
            flowPanelBlocks.Controls.Clear();
            flowPanelBlocks.Controls.Add(new BuildFileStructure(new BuildingBlockContext(this, flowPanelBlocks), brotliFile));

            RegenerateBrotliStream();
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

        // Output generation

        private void SetupWorker(AsyncWorker<string> worker, RichTextBox tb, StatusBarPanel status, Func<long, string> funcStatusTextSuccess, Func<string> funcStatusTextFailure){
            worker.WorkFinished += (sender, args) => {
                tb.ForeColor = SystemColors.WindowText;
                tb.Text = args.Result;
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

            statusBarPanelTimeBits.Text = "Generating...";
            statusBarPanelTimeOutput.Text = "Generating...";

            workerStream.Start(() => brotliFile.Serialize().ToString());
            workerOutput.Start(() => brotliFile.GetDecompressionState().OutputAsUTF8);
        }

        private void RegenerateBrotliStream(){
            isDirty = true;
            timerRegenerationDelay.Stop();
            timerRegenerationDelay.Start();
        }

        // Form events

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e){
            e.Cancel = PromptUnsavedChanges("Would you like to save changes before exiting?");
        }

        // Menu events

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
    }
}
