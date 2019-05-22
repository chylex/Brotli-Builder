using System;
using System.IO;
using BrotliLib.Brotli;
using IOPath = System.IO.Path;

namespace BrotliCalc.Helpers{
    class BrotliFile{
        public string Path { get; set; }
        public string Name { get; set; }
        public int? Quality { get; set; }
        public BrotliFileStructure Structure { get; set; }

        public string EstimatedUncompressedPath{
            get{
                return IOPath.Combine(IOPath.GetDirectoryName(Path), IOPath.GetFileNameWithoutExtension(Name));
            }
        }

        public byte[] CompressedBytes{
            get{
                return File.ReadAllBytes(Path);
            }
        }

        public long? SizeBytes{
            get{
                try{
                    return new FileInfo(Path).Length;
                }catch(Exception){
                    return null;
                }
            }
        }
    }
}
