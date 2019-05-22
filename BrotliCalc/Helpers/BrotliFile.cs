using System;
using System.IO;
using BrotliLib.Brotli;

namespace BrotliCalc.Helpers{
    class BrotliFile{
        public string Path { get; set; }
        public string Name { get; set; }
        public int? Quality { get; set; }
        public BrotliFileStructure Structure { get; set; }

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
