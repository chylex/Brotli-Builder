using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;

namespace BrotliCalc.Helpers{
    static class Brotli{
        private const int CompressionFileLimit = 2000;
        private const string CompressedFileExtension = ".br";

        public static IEnumerable<(string name, BrotliFileStructure bfs)> DecompressPath(string path, string pattern = "*" + CompressedFileExtension){
            int fullPathLength = Path.GetFullPath(path).Length;

            (string, BrotliFileStructure) ReadFile(string file){
                Debug.WriteLine($"Decompressing file {file}...");
                return (file.Substring(fullPathLength), BrotliFileStructure.FromBytes(File.ReadAllBytes(file)));
            }

            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory)){
                return Directory.EnumerateFiles(path, pattern, SearchOption.AllDirectories).Select(ReadFile);
            }
            else{
                return new []{ ReadFile(path) };
            }
        }

        public static int CompressPath(string path, int quality, WindowSize windowSize){
            if (!File.Exists("dict")){
                throw new FileNotFoundException("Dictionary file must be named 'dict' and placed into the working directory.", "dict");
            }

            if (quality < 0 || quality > 11){
                throw new ArgumentOutOfRangeException(nameof(quality), "Compression quality must be in range [0; 11].");
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
