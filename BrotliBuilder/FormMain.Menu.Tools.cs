using System;
using System.Diagnostics;
using System.Windows.Forms;
using BrotliBuilder.Dialogs;
using BrotliBuilder.State;
using BrotliBuilder.Utils;
using BrotliBuilder.Utils.Compat;
using BrotliLib.Brotli.Dictionary.Default;
using BrotliLib.Brotli.Parameters;

namespace BrotliBuilder{
    partial class FormMain{
        private MainMenuBase.Item? menuItemCompareMarkers;
        private MainMenuBase.Item? menuItemCloneGeneratedToOriginal;
        private MainMenuBase.Item? menuItemCloneOriginalToGenerated;

        private void InitializeMenuTools(MainMenuBase.Item menu){
            menu.Add("Configure Serialization Parameters", OpenSerializationParametersDialog, Shortcut.CtrlP);
            menu.AddSeparator();
            menuItemCompareMarkers = menu.Add("Compare Markers", CompareMarkers, Shortcut.CtrlM, isEnabled: false);
            menu.AddSeparator();
            menuItemCloneGeneratedToOriginal = menu.Add("Generated >> Original", CloneGeneratedToOriginal, isEnabled: false);
            menuItemCloneOriginalToGenerated = menu.Add("Generated << Original", CloneOriginalToGenerated, isEnabled: false);
            menu.AddSeparator();
            menu.Add("Static Dictionary", OpenStaticDictionaryDialog, Shortcut.CtrlD);

            if (Debugger.IsAttached){
                menu.AddSeparator();
                menu.Add("Debug (Break)", DebugStructure);
            }
        }

        private void OpenSerializationParametersDialog(){
            void SerializationParametersUpdated(object? sender, BrotliSerializationParameters newParameters){
                fileGenerated.SerializationParameters = newParameters;
            }

            void Reserialize(object? sender, EventArgs e){
                if (lastGeneratedFile == null){
                    MessageBox.Show("No previously generated file available.", "Reserialize Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else{
                    fileGenerated.LoadStructure(lastGeneratedFile, lastOriginalFileBytes);
                }
            }

            using FormSerializationParameters form = new FormSerializationParameters(fileGenerated.SerializationParameters);
            form.Updated += SerializationParametersUpdated;
            form.Reserialize += Reserialize;
            form.ShowDialog();
        }

        private void CompareMarkers(){
            if (brotliFilePanelOriginal.MarkerRoot == null || brotliFilePanelGenerated.MarkerRoot == null){
                MessageBox.Show("No original file opened.", "Compare Markers Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string originalText = brotliFilePanelOriginal.MarkerRoot.BuildText(includeBitCounts: true);
            string generatedText = brotliFilePanelGenerated.MarkerRoot.BuildText(includeBitCounts: true);

            try{
                WinMerge.CompareText(originalText, generatedText);
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message, "Compare Markers Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CloneFileBetweenControllers(BrotliFileController from, BrotliFileController to){
            if (from.CurrentFile == null){
                MessageBox.Show("No structure loaded.", "Clone File Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            from.ReplayOver(to);
        }

        private void CloneGeneratedToOriginal(){
            skipNextOriginalToGeneratedFeed = true;
            CloneFileBetweenControllers(fileGenerated, fileOriginal);
        }

        private void CloneOriginalToGenerated(){
            CloneFileBetweenControllers(fileOriginal, fileGenerated);
        }

        private void OpenStaticDictionaryDialog(){
            try{
                using FormStaticDictionary form = new FormStaticDictionary(BrotliDefaultDictionary.Embedded);
                form.ShowDialog();
            }catch(Exception ex){
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.Message, "Static Dictionary Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DebugStructure(){
            var original = fileOriginal.CurrentFile;
            var generated = fileGenerated.CurrentFile;

            Debugger.Break();

            // ensure locals are not optimized away
            Trace.Assert(original == null || original != null);
            Trace.Assert(generated == null || generated != null);
        }
    }
}
