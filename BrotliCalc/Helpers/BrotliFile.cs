using System;
using System.Diagnostics;
using System.IO;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Streaming;
using BrotliLib.Markers;

namespace BrotliCalc.Helpers{
    abstract class BrotliFile{
        private static int? TryGetFileSize(string path){
            try{
                return (int)new FileInfo(path).Length;
            }catch(Exception ex){
                Debug.WriteLine(ex);
                return null;
            }
        }

        public string Path { get; }
        public string Name { get; }

        public abstract string FullName { get; }

        public byte[] Contents => contentsLazy.Value;
        public int? SizeBytes => sizeBytesLazy.Value;

        private readonly Lazy<byte[]> contentsLazy;
        private readonly Lazy<int?> sizeBytesLazy;

        protected BrotliFile(string path, string name){
            this.Path = path;
            this.Name = name;

            this.contentsLazy = new Lazy<byte[]>(() => File.ReadAllBytes(Path), isThreadSafe: true);
            this.sizeBytesLazy = new Lazy<int?>(() => TryGetFileSize(Path), isThreadSafe: true);
        }

        public override string ToString(){
            return FullName;
        }

        // Types

        internal class Uncompressed : BrotliFile{
            public override string FullName => Name;

            public Uncompressed(string path, string name) : base(path, name){}

            public BrotliFileStructure Encode(IBrotliEncoder encoder){
                return BrotliFileStructure.FromEncoder(Parameters.File, Parameters.Compression, Contents, encoder);
            }

            public IBrotliFileStream Encoding(IBrotliEncoder encoder){
                return new BrotliFileStreamEncoder(Parameters.File, Parameters.Compression, Contents, encoder);
            }
        }

        internal class Compressed : BrotliFile{
            public string Identifier { get; }
            public override string FullName => $"{Name}.{Identifier}{Brotli.CompressedFileExtension}";

            public BrotliFileStructure Structure => structureLazy.Value;
            public BrotliFileReader Reader => BrotliFileReader.FromBytes(Contents, MarkerLevel.None, Parameters.File.Dictionary);

            private readonly Lazy<BrotliFileStructure> structureLazy;

            public Compressed(string path, string name, string identifier) : base(path, name){
                this.Identifier = identifier;
                this.structureLazy = new Lazy<BrotliFileStructure>(() => BrotliFileStructure.FromBytes(Contents, MarkerLevel.None, Parameters.File.Dictionary).Structure, isThreadSafe: true);
            }

            public BrotliFileStructure Transform(IBrotliTransformer transformer){
                return Structure.Transform(transformer, Parameters.Compression);
            }

            public IBrotliFileStream Transforming(IBrotliTransformer transformer){
                return new BrotliFileStreamTransformer(Reader, Parameters.Compression, transformer);
            }

            public MarkerRoot GenerateMarkers(MarkerLevel markerLevel){
                var reader = BrotliFileReader.FromBytes(Contents, markerLevel, Parameters.File.Dictionary);
                while(reader.NextMetaBlock() != null){}
                return reader.MarkerRoot;
            }
        }
    }
}
