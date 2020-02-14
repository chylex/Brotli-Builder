using System.IO;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;

namespace BrotliBuilder{
    partial class FormMain{
        private MenuItem? menuItemCloseOriginal;

        private void InitializeMenuFile(){
            var menu = menuItemFile;

            menu.Add("Open Brotli", OpenBrotli, Shortcut.CtrlO);
            menu.Add("Save Brotli", SaveBrotli, Shortcut.CtrlS);
            menu.Add("Save Output", SaveOutput);
            menu.AddSeparator();
            menuItemCloseOriginal = menu.Add("Close Original", CloseOriginal, isEnabled: false);
            menu.AddSeparator();
            menu.Add("Exit", Close);
        }

        private void OpenBrotli(){
            if (PromptUnsavedChanges("Would you like to save changes before opening a new file?")){
                return;
            }

            using OpenFileDialog dialog = new OpenFileDialog{
                Title = "Open Compressed File",
                Filter = "Brotli (*.br)|*.br|All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName),
                DefaultExt = "br"
            };

            if (dialog.ShowDialog() == DialogResult.OK){
                lastFileName = dialog.FileName;
                isDirty = false;
                skipNextBlockRegeneration = false;
                skipNextOriginalToGeneratedFeed = false;

                fileOriginal.LoadFile(lastFileName);
            }
        }

        private BrotliFileStructure? GetCurrentFileOrShowError(){
            BrotliFileStructure? currentFile = fileGenerated.CurrentFile;

            if (currentFile == null){
                MessageBox.Show("No structure loaded.", "Save File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }

            return currentFile;
        }

        private void SaveBrotli(){
            BrotliFileStructure? currentFile = GetCurrentFileOrShowError();

            if (currentFile == null){
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog{
                Title = "Save Brotli",
                Filter = "Brotli (*.br)|*.br|All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName),
                DefaultExt = "br"
            };

            if (dialog.ShowDialog() == DialogResult.OK){
                lastFileName = dialog.FileName;
                isDirty = false;

                File.WriteAllBytes(lastFileName, currentFile.Serialize(fileGenerated.SerializationParameters).ToByteArray());
            }
        }

        private void SaveOutput(){
            BrotliFileStructure? currentFile = GetCurrentFileOrShowError();

            if (currentFile == null){
                return;
            }

            using SaveFileDialog dialog = new SaveFileDialog{
                Title = "Save Output",
                Filter = "All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == DialogResult.OK){
                File.WriteAllBytes(dialog.FileName, currentFile.Decompress().AsBytes);
            }
        }

        private void CloseOriginal(){
            fileOriginal.ResetToNothing();
        }
    }
}
