using System;
using System.Collections.Generic;
using System.IO;
using BrotliCalc.Helpers;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands.Base{
    abstract class CmdAbstractFileTable : ICommand{
        public abstract string FullName { get; }
        public abstract string ShortName { get; }

        public string ArgumentDesc => (ExtraArgumentCount > 0 ? ExtraArgumentDesc + " " : "") + "<source-path> [output-file]";
        public IntRange ArgumentCount => new IntRange(ExtraArgumentCount + 1, ExtraArgumentCount + 2);

        protected virtual int ExtraArgumentCount => 0;
        protected virtual string ExtraArgumentDesc => "";
        
        protected virtual string WorkDesc => "Processed";
        protected abstract string[] Columns { get; }

        protected string GetTemporaryOutputFile(){
            var folder = Path.Combine(Path.GetTempPath(), "BrotliCalc");
            Directory.CreateDirectory(folder);

            return Path.Combine(folder, $"{FullName}-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        }

        // Process

        public abstract string Process(string[] args);

        internal abstract class Base<T> : CmdAbstractFileTable where T : BrotliFile{
            protected abstract IEnumerable<(BrotliFileGroup, T)> SelectFiles(IEnumerable<BrotliFileGroup> groups);

            public sealed override string Process(string[] args){
                Setup(args);

                var items = SelectFiles(Brotli.ListPath(args[ExtraArgumentCount + 0]));
                var output = args.Length >= ExtraArgumentCount + 2 ? args[ExtraArgumentCount + 1] : GetTemporaryOutputFile();

                using var table = new Table.CSV(output, Columns);

                var result = new FileWorker<T>{
                    Work = GenerateRows,
                    Error = OnError
                }.Start(table, items);

                Finalize(table);

                return $"{WorkDesc} {result.TotalProcessed} file(s) with {result.TotalErrors} error(s).";
            }

            protected virtual void Setup(string[] args){}
            protected virtual void Finalize(Table.CSV table){}

            protected abstract IEnumerable<object[]> GenerateRows(BrotliFileGroup group, T file);

            protected virtual IEnumerable<object[]> OnError(BrotliFileGroup group, T file, Exception ex){
                var row = new object[Columns.Length];
                row[0] = file.Name;
                yield return row;
            }
        }

        // Types

        internal abstract class Uncompressed : Base<BrotliFile.Uncompressed>{
            protected sealed override IEnumerable<(BrotliFileGroup, BrotliFile.Uncompressed)> SelectFiles(IEnumerable<BrotliFileGroup> groups){
                return groups.SelectUncompressedFiles();
            }
        }

        internal abstract class Compressed : Base<BrotliFile.Compressed>{
            protected sealed override IEnumerable<(BrotliFileGroup, BrotliFile.Compressed)> SelectFiles(IEnumerable<BrotliFileGroup> groups){
                return groups.SelectCompressedFiles();
            }
        }
    }
}
