using System;
using System.IO;
using System.Windows.Forms;
using BrotliBuilder.Utils.Compat;
using BrotliImpl.Combined;
using BrotliImpl.Encoders;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Parameters;

namespace BrotliBuilder{
    partial class FormMain{
        private void InitializeMenuEncoders(MainMenuBase.Item menu){
            var parameters = BrotliFileParameters.Default;
            var dictionary = parameters.Dictionary;

            menu.Add("Into Uncompressed Meta-Blocks", () => OpenFileWithEncoder(parameters, new EncodeUncompressedOnly()));
            menu.Add("Into Literals",                 () => OpenFileWithEncoder(parameters, new EncodeLiterals()));

            menu.AddSeparator();

            menu.Add("Greedy Search (Only Copies)",     () => OpenFileWithEncoder(parameters, new EncodeGreedySearch.OnlyBackReferences(minLength: 4)));
            menu.Add("Greedy Search (Only Dictionary)", () => OpenFileWithEncoder(parameters, new EncodeGreedySearch.OnlyDictionary(minLength: 4)));
            menu.Add("Greedy Search (Mixed)",           () => OpenFileWithEncoder(parameters, new EncodeGreedySearch.Mixed(minCopyLength: 4, minDictionaryLength: 4)));

            menu.AddSeparator();

            menu.Add("Quality 0", () => OpenFileWithPipeline(dictionary, new CompressQuality0()));
            menu.Add("Quality 2", () => OpenFileWithPipeline(dictionary, new CompressQuality2()));
            menu.Add("Quality 3", () => OpenFileWithPipeline(dictionary, new CompressQuality3()));
        }

        private void InitializeMenuTransformers(MainMenuBase.Item menu){
            menu.Add("Rebuild", () => TransformCurrentFile(new TransformRebuild()));

            menu.AddSeparator();

            menu.Add("Convert to Uncompressed",             () => TransformCurrentFile(new TransformCompressedIntoUncompressed()));
            menu.Add("Reset Distance Parameters",           () => TransformCurrentFile(new TransformResetDistanceParameters()));
            menu.Add("Reset Block Splits & Context Models", () => TransformCurrentFile(new TransformResetBlockSplitsContextModels()));

            menu.AddSeparator();

            menu.Add("Test Distance Parameters",     () => TransformCurrentFile(new TransformTestDistanceParameters()));
            menu.Add("Split Insert Copy Lengths",    () => TransformCurrentFile(new TransformSplitInsertCopyLengths()));
            menu.Add("Official Block Splitter (LQ)", () => TransformCurrentFile(new TransformOfficialBlockSplitterLQ()));
        }

        private void OpenFileWith(string title, Action<string> callback){
            if (PromptUnsavedChanges("Would you like to save changes before opening a new file?")){
                return;
            }

            using OpenFileDialog dialog = new OpenFileDialog{
                Title = title,
                Filter = "All Files (*.*)|*.*",
                FileName = Path.GetFileName(lastFileName)
            };

            if (dialog.ShowDialog() == DialogResult.OK){
                lastFileName = dialog.FileName;
                skipNextBlockRegeneration = false;
                callback(lastFileName);
            }
        }

        private void OpenFileWithEncoder(BrotliFileParameters parameters, IBrotliEncoder encoder){
            OpenFileWith("Open File to Encode", fileName => fileGenerated.EncodeFile(fileName, parameters, encoder));
        }
        
        private void OpenFileWithPipeline(BrotliDictionary dictionary, BrotliEncodePipeline pipeline){
            OpenFileWith("Open File to Encode", fileName => fileGenerated.EncodeFile(fileName, pipeline, dictionary));
        }
        
        private void TransformCurrentFile(IBrotliTransformer transformer){
            skipNextBlockRegeneration = false;

            if (!fileGenerated.Transform(transformer)){
                MessageBox.Show("No structure loaded.", "Transform Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
