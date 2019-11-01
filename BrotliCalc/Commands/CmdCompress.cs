using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Numbers;
using DiagProcess = System.Diagnostics.Process;

namespace BrotliCalc.Commands{
    class CmdCompress : ICommand{
        public string FullName => "compress";
        public string ShortName => "c";

        public string ArgumentDesc => "<source-path> <quality|all> [window-size]";
        public IntRange ArgumentCount => new IntRange(2, 3);

        private static readonly IntRange QualityRange = new IntRange(0, 11);
        private const int FileLimit = 2000;

        public string Process(string[] args){
            var path = args[0];
            var qualities = args[1] == "all" ? QualityRange : IntRange.Only(int.Parse(args[1]));
            var wbits = (args.Length >= 3 ? new WindowSize(int.Parse(args[2])) : WindowSize.Default).Bits;

            if (!qualities.Values.All(QualityRange.Contains)){
                throw new ArgumentException($"Compression quality must be in range {QualityRange}.");
            }

            if (!File.Exists("dict")){
                throw new FileNotFoundException("Dictionary file must be named 'dict' and placed into the working directory.", "dict");
            }

            var groups = Brotli.ListPath(path).ToArray();
            int totalFiles = groups.Length;

            if (totalFiles > FileLimit){
                throw new InvalidOperationException($"Too many files to process ({totalFiles} > {FileLimit}), cancelling command for safety.");
            }

            var items = qualities.Values.Cartesian(groups).ToArray();

            using(var progress = new Progress(items.Length)){
                try{
                    items.Parallelize().ForAll(item => {
                        var (quality, group) = item;
                        var file = group.Uncompressed;

                        Compress(wbits, quality, file.Path);
                        progress.Post($"Finished {file} (quality {quality})");
                    });
                }catch(Win32Exception e){
                    throw new InvalidOperationException("Brotli executable must be named 'brotli' and placed into the working directory or environment path.", e);
                }
            }

            return $"Compressed {totalFiles} file(s).";
        }

        private static void Compress(int wbits, int quality, string path){
            using DiagProcess process = DiagProcess.Start("brotli", $"-w {wbits} -q {quality} -S .{quality}{Brotli.CompressedFileExtension} -f \"{path}\"");
            process.WaitForExit();
        }
    }
}
