using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BrotliCalc.Helpers{
    static class Brotli{
        public const string CompressedFileExtension = ".br";
        public const char DirectorySeparator = '/';

        public static FileOrdering FileOrder { get; set; } = FileOrdering.System;

        public enum FileOrdering{
            System, Quality
        }
        
        private static readonly Regex RegexCompressionIdentifier = new Regex(@"\.([^.]+)\.br$");

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

        public static IEnumerable<(BrotliFileGroup, BrotliFile.Uncompressed)> SelectUncompressedFiles(this IEnumerable<BrotliFileGroup> me){
            return me.Select(group => (group, group.Uncompressed));
        }

        public static IEnumerable<(BrotliFileGroup, BrotliFile.Compressed)> SelectCompressedFiles(this IEnumerable<BrotliFileGroup> me){
            var files = me.SelectMany(group => group.Compressed.Select(file => (group, file)));
            
            return FileOrder switch{
                FileOrdering.Quality => files.OrderBy(item => {
                    var identifier = item.file.Identifier;
                    return int.TryParse(identifier, out int _) ? identifier.PadLeft(item.group.Compressed.Max(file => file.Identifier.Length), '0') : identifier;
                }),

                _ => files
            };
        }
    }
}
