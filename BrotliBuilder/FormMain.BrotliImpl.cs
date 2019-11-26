using BrotliImpl.Encoders;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Parameters;

namespace BrotliBuilder{
    partial class FormMain{
        private void InitializeMenuEncoders(){
            var items = menuItemEncodeFile.MenuItems;
            var parameters = BrotliFileParameters.Default;

            items.Add("Into Uncompressed Meta-Blocks",   (_, e) => OpenFileWithEncoder(parameters, new EncodeUncompressedOnly()));
            items.Add("Into Literals",                   (_, e) => OpenFileWithEncoder(parameters, new EncodeLiterals()));
            items.Add("-");
            items.Add("Greedy Search (Only Copies)",     (_, e) => OpenFileWithEncoder(parameters, new EncodeGreedySearch.OnlyBackReferences(minLength: 4)));
            items.Add("Greedy Search (Only Dictionary)", (_, e) => OpenFileWithEncoder(parameters, new EncodeGreedySearch.OnlyDictionary()));
            items.Add("Greedy Search (Only Mixed)",      (_, e) => OpenFileWithEncoder(parameters, new EncodeGreedySearch.Mixed(minCopyLength: 4)));
        }

        private void InitializeMenuTransformers(){
            var items = menuItemTransform.MenuItems;

            items.Add("Rebuild",                   (_, e) => TransformCurrentFile(new TransformRebuild()));
            items.Add("Test Distance Parameters",  (_, e) => TransformCurrentFile(new TransformTestDistanceParameters()));
            items.Add("Split Insert Copy Lengths", (_, e) => TransformCurrentFile(new TransformSplitInsertCopyLengths()));
        }
    }
}
