using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BrotliCalc.Helpers;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands.Base{
    abstract class CmdAbstractFileMapper : ICommand{
        public abstract string FullName { get; }
        public abstract string ShortName { get; }

        public string ArgumentDesc => (ExtraArgumentCount > 0 ? ExtraArgumentDesc + " " : "") + "<source-path> [output-folder]";
        public IntRange ArgumentCount => new IntRange(ExtraArgumentCount + 1, ExtraArgumentCount + 2);

        protected virtual int ExtraArgumentCount => 0;
        protected virtual string ExtraArgumentDesc => "";

        protected virtual string WorkDesc => "Processed";
        protected virtual string AppendFileName => "";

        // Process

        public abstract string Process(string[] args);

        internal abstract class Base<T> : CmdAbstractFileMapper where T : BrotliFile{
            protected abstract IEnumerable<(BrotliFileGroup, T)> SelectFiles(IEnumerable<BrotliFileGroup> groups);

            public override string Process(string[] args){
                var source = args[ExtraArgumentCount + 0];
                var output = args.Length >= ExtraArgumentCount + 2 ? args[ExtraArgumentCount + 1] : File.Exists(source) ? Path.GetDirectoryName(source) : source;

                if (!Directory.Exists(output)){
                    throw new ArgumentException("Output folder does not exist.");
                }
                
                Setup(args);

                var items = SelectFiles(Brotli.ListPath(source)).ToArray();
                int errors = 0;

                using(var progress = new Progress(items.Length)){
                    items.Parallelize().ForAll(item => {
                        var (group, file) = item;
                        string markerFile = Path.Combine(output, file.FullName + AppendFileName);

                        try{
                            Directory.CreateDirectory(Path.GetDirectoryName(markerFile));
                            File.WriteAllBytes(markerFile, MapFile(group, file));

                            progress.Post($"Finished {file}");
                        }catch(Exception e){
                            Interlocked.Increment(ref errors);

                            progress.Print($"Error processing {file}: {e.Message}");
                            Debug.WriteLine(e.ToString());
                        }
                    });
                }

                return $"{WorkDesc} {items.Length} file(s) with {errors} error(s).";
            }
            
            protected virtual void Setup(string[] args){}

            protected abstract byte[] MapFile(BrotliFileGroup group, T file);
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
