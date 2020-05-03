using System;
using System.Collections.Generic;
using System.Linq;
using BrotliCalc.Commands.Base;
using BrotliCalc.Helpers;
using BrotliImpl.Combined;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;

namespace BrotliCalc.Commands{
    class CmdValidateQuality0 : CmdAbstractFileTable.Uncompressed{
        public override string FullName => "validate-quality-0";
        public override string ShortName => "vq0";

        protected override string WorkDesc => "Validated";

        protected override string[] Columns { get; } = {
            "File", "Uncompressed Bytes", "Matches"
        };

        private readonly CompressQuality0 Pipeline = new CompressQuality0();

        protected override IEnumerable<object?[]> GenerateRows(BrotliFileGroup group, BrotliFile.Uncompressed file){
            var quality0 = group.Compressed.FirstOrDefault(f => f.Identifier == "0");

            if (quality0 == null){
                throw new InvalidOperationException("Missing file with compression level 0.");
            }
            
            var originalStructure = quality0.Structure;
            var generatedStructure = Pipeline.Apply(file.Contents, Parameters.File.Dictionary);

            ValidateMatch(originalStructure, generatedStructure);

            return new List<object?[]>{
                new object?[]{ file.Name, file.SizeBytes, 1 }
            };
        }

        protected override IEnumerable<object?[]> OnError(BrotliFileGroup group, BrotliFile.Uncompressed file, Exception ex){
            return new List<object?[]>{
                new object?[]{ file.Name, file.SizeBytes, 0 }
            };
        }

        private void ValidateMatch(BrotliFileStructure original, BrotliFileStructure generated){
            var mbOriginal = original.MetaBlocks;
            var mbGenerated = generated.MetaBlocks;

            if (mbOriginal.Count != mbGenerated.Count){
                throw new Exception($"Meta-block count mismatched ({mbOriginal.Count} != {mbGenerated.Count}).");
            }

            for(int index = 0; index < mbOriginal.Count; index++){
                ValidateMatch(index, mbOriginal[index], mbGenerated[index]);
            }
        }

        private void ValidateMatch(int index, MetaBlock original, MetaBlock generated){
            if (original.GetType() != generated.GetType()){
                throw new Exception($"Meta-block {index} type mismatched (original {original.GetType().Name}, generated {generated.GetType().Name}).");
            }

            switch(original){
                case MetaBlock.Uncompressed uo:
                    if (!CollectionHelper.Equal(uo.UncompressedData, ((MetaBlock.Uncompressed)generated).UncompressedData)){
                        throw new Exception($"Meta-block {index} (uncompressed) data mismatched.");
                    }

                    break;

                case MetaBlock.Compressed co:
                    var cg = (MetaBlock.Compressed)generated;

                    ValidateLists(index, "insert&copy command", co.Data.InsertCopyCommands, cg.Data.InsertCopyCommands);

                    foreach(var category in Categories.LID){
                        ValidateLists(index, "block-switch command", co.Data.BlockSwitchCommands[category], cg.Data.BlockSwitchCommands[category]);
                    }

                    break;
            }
        }

        private void ValidateLists<T>(int index, string title, IReadOnlyList<T> original, IReadOnlyList<T> generated){
            if (original.Count != generated.Count){
                throw new Exception($"Meta-block {index} (compressed) {title} count mismatched ({original.Count} != {generated.Count}).");
            }

            if (!CollectionHelper.Equal(original, generated)){
                throw new Exception($"Meta-block {index} (compressed) {title} data mismatched.");
            }
        }
    }
}
