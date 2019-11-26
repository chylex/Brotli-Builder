using BrotliImpl.Encoders;
using BrotliImpl.Transformers;
using BrotliLib.Brotli.Parameters;

namespace BrotliBuilder{
    partial class FormMain{
        private void InitializeMenuEncoders(){
            var items = menuItemEncodeFile.MenuItems;

            items.Add("Into Uncompressed Meta-Blocks",   (_, e) => OpenFileWithEncoder(new BrotliFileParameters(), new EncodeUncompressedOnly()));
            items.Add("Into Literals",                   (_, e) => OpenFileWithEncoder(new BrotliFileParameters(), new EncodeLiterals()));
            items.Add("-");
            items.Add("Greedy Search (Only Copies)",     (_, e) => OpenFileWithEncoder(new BrotliFileParameters(), new EncodeGreedySearch.OnlyBackReferences(minLength: 4)));
            items.Add("Greedy Search (Only Dictionary)", (_, e) => OpenFileWithEncoder(new BrotliFileParameters(), new EncodeGreedySearch.OnlyDictionary()));
            items.Add("Greedy Search (Only Mixed)",      (_, e) => OpenFileWithEncoder(new BrotliFileParameters(), new EncodeGreedySearch.Mixed(minCopyLength: 4)));
        }

        private void InitializeMenuTransformers(){
            var items = menuItemTransform.MenuItems;

            items.Add("Rebuild",                   (_, e) => TransformCurrentFile(new TransformRebuild()));
            items.Add("Test Distance Parameters",  (_, e) => TransformCurrentFile(new TransformTestDistanceParameters()));
            items.Add("Split Insert Copy Lengths", (_, e) => TransformCurrentFile(new TransformSplitInsertCopyLengths()));
        }
    }
}
