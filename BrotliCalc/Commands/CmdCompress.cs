using System;
using System.ComponentModel;
using System.Linq;
using BrotliCalc.Commands.Base;
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

        public static string CustomExePath { get; set; }

        private static readonly IntRange QualityRange = new IntRange(0, 11);
        private const int AutoWindowSize = 0;
        private const int FileLimit = 2000;

        public string Process(string[] args){
            var path = args[0];
            var qualities = args[1] == "all" ? QualityRange : IntRange.Only(int.Parse(args[1]));
            var wbits = args.Length >= 3 ? new WindowSize(int.Parse(args[2])).Bits : AutoWindowSize;

            if (!qualities.Values.All(QualityRange.Contains)){
                throw new ArgumentException($"Compression quality must be in range {QualityRange}.");
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
                }catch(Exception e) when (HasWin32Exception(e, out var we)){
                    if (CustomExePath == null){
                        throw new InvalidOperationException("Brotli executable must be named 'brotli' and placed into the working directory or environment path.", we);
                    }
                    else{
                        throw new InvalidOperationException("Brotli executable failed to start.", we);
                    }
                }
            }

            return $"Compressed {totalFiles} file(s).";
        }

        private static void Compress(int wbits, int quality, string path){
            using DiagProcess process = DiagProcess.Start(CustomExePath ?? "brotli", $"-w {wbits} -q {quality} -S .{quality}{Brotli.CompressedFileExtension} -f \"{path}\"");
            process.WaitForExit();
        }

        private static bool HasWin32Exception(Exception e, out Win32Exception we){
            we = e switch{
                Win32Exception we2 => we2,
                AggregateException ae => ae.InnerException as Win32Exception,
                _ => null
            };

            return we != null;
        }
    }
}
