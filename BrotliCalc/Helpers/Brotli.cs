using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BrotliLib.Brotli.Components;
using BrotliLib.Numbers;

namespace BrotliCalc.Helpers{
    static class Brotli{
        private const int CompressionFileLimit = 2000;
        private const string CompressedFileExtension = ".br";
        public const char DirectorySeparator = '/';
        
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
            if (!File.GetAttributes(path).HasFlag(FileAttributes.Directory)){
                var name = Path.GetFileName(path) ?? path;
                var file = new BrotliFile.Uncompressed(path, name);

                return new BrotliFileGroup[]{ new BrotliFileGroup(file, Array.Empty<BrotliFile.Compressed>()) };
            }

            int fullPathLength = Path.GetFullPath(path).Length;

            string GetRelativePath(string file){
                return file.Substring(fullPathLength).Replace(Path.DirectorySeparatorChar, DirectorySeparator).Replace(Path.AltDirectorySeparatorChar, DirectorySeparator).TrimStart(DirectorySeparator);
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

            BrotliFileGroup[] groups;
            
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)){
                groups = ListPath(path).ToArray();
            }
            else{
                groups = new BrotliFileGroup[]{
                    new BrotliFileGroup(new BrotliFile.Uncompressed(path, Path.GetFileName(path)), new BrotliFile.Compressed[0])
                };
            }

            if (groups.Length > CompressionFileLimit){
                throw new InvalidOperationException($"Too many files to process ({groups.Length} > {CompressionFileLimit}), cancelling command for safety.");
            }

            string argPrefix = $"-w {windowSize.Bits} -q {quality} -S .{quality}{CompressedFileExtension} -f ";

            try{
                Parallel.ForEach(Partitioner.Create(groups, EnumerablePartitionerOptions.NoBuffering), (group, state) => {
                    var file = group.Uncompressed;

                    using Process process = Process.Start("brotli", argPrefix + '"' + file.Path + '"');
                    process.WaitForExit();

                    Console.WriteLine($"Finished {file.Name} (quality {quality}).");
                });
            }catch(AggregateException e){
                if (e.InnerException is Win32Exception we){
                    throw new InvalidOperationException("Brotli executable must be named 'brotli' and placed into the working directory or environment path.", we);
                }
                else{
                    throw;
                }
            }

            return groups.Length;
        }

        public static IEnumerable<(BrotliFileGroup, BrotliFile.Uncompressed)> SelectUncompressedFiles(this IEnumerable<BrotliFileGroup> me){
            return me.Select(group => (group, group.Uncompressed));
        }

        public static IEnumerable<(BrotliFileGroup, BrotliFile.Compressed)> SelectCompressedFiles(this IEnumerable<BrotliFileGroup> me){
            return me.SelectMany(group => group.Compressed.Select(file => (group, file)));
        }
    }
}
