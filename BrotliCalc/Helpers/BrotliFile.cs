using System;
using System.Diagnostics;
using System.IO;
using BrotliLib.Brotli;

namespace BrotliCalc.Helpers{
    abstract class BrotliFile{
        public string Path { get; }
        public string Name { get; }

        public byte[] Contents => contentsLazy.Value;
        public int? SizeBytes => sizeBytesLazy.Value;

        private readonly Lazy<byte[]> contentsLazy;
        private readonly Lazy<int?> sizeBytesLazy;

        protected BrotliFile(string path, string name){
            this.Path = path;
            this.Name = name;

            this.contentsLazy = new Lazy<byte[]>(() => {
                try{
                    return File.ReadAllBytes(Path);
                }catch(Exception ex){
                    Debug.WriteLine(ex);
                    return null;
                }
            }, isThreadSafe: true);

            this.sizeBytesLazy = new Lazy<int?>(() => {
                try{
                    return (int)new FileInfo(Path).Length;
                }catch(Exception ex){
                    Debug.WriteLine(ex);
                    return null;
                }
            }, isThreadSafe: false);
        }

        // Types

        internal class Uncompressed : BrotliFile{
            public Uncompressed(string path, string name) : base(path, name){}
        }

        internal class Compressed : BrotliFile{
            public string Identifier { get; }

            public BrotliFileStructure Structure{
                get{
                    Debug.WriteLine($"Decompressing file {Name}.{Identifier}...");
                    return BrotliFileStructure.FromBytes(Contents);
                }
            }

            public Compressed(string path, string name, string identifier) : base(path, name){
                this.Identifier = identifier;
            }
        }
    }
}
