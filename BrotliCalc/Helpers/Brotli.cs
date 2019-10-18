using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BrotliLib.Brotli.Components;
using BrotliLib.Numbers;

namespace BrotliCalc.Helpers{
    static class Brotli{
        private const int CompressionFileLimit = 2000;
        private const string CompressedFileExtension = ".br";
        
        private static readonly Regex RegexCompressionIdentifier = new Regex(@"\.([^.]+)\.br$");
        private static readonly IntRange QualityRange = new IntRange(0, 11);

        private static string GetUncompressedName(string path){
            return Path.GetExtension(path) == CompressedFileExtension ? RegexCompressionIdentifier.Replace(path, "") : path;
        }

        private static bool IsUncompressed(string path){
            return Path.GetExtension(path) != CompressedFileExtension;
        }

        private static string GetSortKey(BrotliFile.Compressed file){
            return file.Identifier.PadLeft(2, '0');
        }

        public static IEnumerable<BrotliFileGroup> ListPath(string path){
            int fullPathLength = Path.GetFullPath(path).Length;

            string GetRelativePath(string file){
                return file.Substring(fullPathLength).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            }

            BrotliFile.Compressed ConstructCompressed(string file){
                var match = RegexCompressionIdentifier.Match(file);
                var identifier = match.Success ? match.Groups[1].Value : "?";

                return new BrotliFile.Compressed(file, GetUncompressedName(GetRelativePath(file)), identifier);
            }

            BrotliFileGroup ProcessGroup(IGrouping<string, string> group){
                var uncompressed = group.FirstOrDefault(IsUncompressed);
                var compressed = group.Except(new string[]{ uncompressed });
                
                return new BrotliFileGroup(
                    new BrotliFile.Uncompressed(uncompressed, GetRelativePath(uncompressed)),
                    compressed.Select(ConstructCompressed).OrderBy(GetSortKey).ToArray()
                );
            }

            return Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).GroupBy(GetUncompressedName).Select(ProcessGroup);
        }

        public static int CompressPath(string path, int quality, WindowSize windowSize){
            if (!File.Exists("dict")){
                throw new FileNotFoundException("Dictionary file must be named 'dict' and placed into the working directory.", "dict");
            }

            if (!QualityRange.Contains(quality)){
                throw new ArgumentOutOfRangeException(nameof(quality), $"Compression quality must be in range {QualityRange}.");
            }

            IList<string> filePaths;
            
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)){
                filePaths = ListPath(path).Select(group => group.Uncompressed.Path).ToArray();
            }
            else{
                filePaths = new []{ path };
            }

            if (filePaths.Count > CompressionFileLimit){
                throw new InvalidOperationException($"Too many files to process ({filePaths.Count} > {CompressionFileLimit}), cancelling command for safety.");
            }

            string argPrefix = $"-w {windowSize.Bits} -q {quality} -S .{quality}{CompressedFileExtension} -f ";

            try{
                foreach(string file in filePaths){
                    Console.WriteLine($"Compressing file {file} (quality {quality})...");

                    using Process process = Process.Start("brotli", argPrefix + '"' + file + '"');
                    process.Start();
                    process.WaitForExit();

                    // TODO add multiprocess support
                }
            }catch(Win32Exception e){
                throw new InvalidOperationException("Brotli executable must be named 'brotli' and placed into the working directory or environment path.", e);
            }

            return filePaths.Count;
        }
    }
}
