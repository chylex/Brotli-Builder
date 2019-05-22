using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Numbers;

namespace BrotliCalc.Helpers{
    static class Brotli{
        private const int CompressionFileLimit = 2000;
        private const string CompressedFileExtension = ".br";

        private static readonly Regex RegexFileQuality = new Regex(@"\.(\d{1,2})\.br$");
        private static readonly Range QualityRange = new Range(0, 11);

        private static string GetSortKey(string path){
            return RegexFileQuality.Replace(path, match => match.Success ? ".br." + match.Groups[1].Value.PadLeft(2, '0') : ".br");
        }

        private static int? TryDeduceQuality(string name){
            var match = RegexFileQuality.Match(name);
            return match.Success && int.TryParse(match.Groups[1].Value, out int quality) && QualityRange.Contains(quality) ? quality : (int?)null;
        }

        public static IEnumerable<BrotliFile> DecompressPath(string path, string pattern = "*" + CompressedFileExtension){
            int fullPathLength = Path.GetFullPath(path).Length;

            BrotliFile ReadFile(string file){
                Debug.WriteLine($"Decompressing file {file}...");

                string name = file.Substring(fullPathLength).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                int? quality = TryDeduceQuality(name);

                if (quality != null){
                    name = RegexFileQuality.Replace(name, ".br");
                }

                return new BrotliFile{
                    Path = file,
                    Name = name,
                    Quality = quality,
                    Structure = BrotliFileStructure.FromBytes(File.ReadAllBytes(file))
                };
            }

            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)){
                return Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories).OrderBy(GetSortKey).Select(ReadFile);
            }
            else{
                return new []{ ReadFile(path) };
            }
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
                filePaths = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Where(file => Path.GetExtension(file) != CompressedFileExtension).ToList();
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
                    Debug.WriteLine($"Compressing file {file}...");

                    using(Process process = Process.Start("brotli", argPrefix + '"' + file + '"')){
                        process.Start();
                        process.WaitForExit();
                    }

                    // TODO add multiprocess support
                }
            }catch(Win32Exception e){
                throw new InvalidOperationException("Brotli executable must be named 'brotli' and placed into the working directory or environment path.", e);
            }

            return filePaths.Count;
        }
    }
}
