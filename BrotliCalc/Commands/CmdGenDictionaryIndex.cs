using BrotliLib.Brotli;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.Dictionary.Source;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdGenDictionaryIndex : ICommand{
        public string FullName => "gen-dictionary-index";
        public string ShortName => "gdi";

        public string ArgumentDesc => "[input-file] <output-file>";
        public Range ArgumentCount => new Range(1, 2);

        public string Process(string[] args){
            int argc = args.Length;

            string inputFile = args.ElementAtOrDefault(argc - 2);
            string outputFile = args[argc - 1];

            BrotliDictionary dictionary = inputFile == null ? BrotliDefaultDictionary.Embedded
                                                            : new BrotliDefaultDictionary(new StreamSource(new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)));

            Debug.WriteLine("Generating index lookup trie...");

            var trie = dictionary.GenerateIndex();

            Debug.WriteLine("Serializing trie to file...");

            using(var stream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read)){
                BrotliDictionaryIndex.Write(stream, trie);
            }

            Debug.WriteLine("Cleaning up...");

            if (dictionary != BrotliDefaultDictionary.Embedded){
                dictionary.Dispose();
            }

            return "Dictionary index lookup generated.";
        }
    }
}
