using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BrotliCalc.Commands.Base;
using BrotliLib.Brotli.Dictionary.Default;
using BrotliLib.Collections;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    partial class CmdGenerateOfficialDictionaryHashLookup : ICommand{
        public string FullName => "generate-official-dictionary-hash-lookup";
        public string ShortName => "godhl";

        public string ArgumentDesc => "<output-file>";
        public IntRange ArgumentCount => IntRange.Only(1);

        public string Process(string[] args){
            var dict = BrotliDefaultDictionary.Embedded;
            var format = dict.Format;

            var words = new ushort[32768];
            var lengths = new byte[32768];

            Array.Copy(DefaultWords, 0, words, 0, words.Length);
            Array.Copy(DefaultLengths, 0, lengths, 0, lengths.Length);
            
            var a = Enumerable.Range(0, lengths.Length / 2).Select(i => lengths[i * 2]).GroupBy(i => i).Select(g => (g.Key, g.Count())).ToArray();
            var b = Enumerable.Range(0, lengths.Length / 2).Select(i => lengths[i * 2 + 1]).GroupBy(i => i).Select(g => (g.Key, g.Count())).ToArray();

            void Update(uint hash, int length, ushort word, int transform){
                if (lengths[hash] == 0){
                    words[hash] = word;
                    lengths[hash] = (byte)length;
                }
            }
            /*
            for(int length = 8; length <= format.WordLengths.Max(); length++){
                int count = format.WordCount(length);

                for(ushort index = 0; index < count; index++){
                    var word = dict.ReadRaw(length, index);
                    var hash = Hash14(word) << 1;

                    if (length > lengths[hash]){
                        Update(hash, length, index, 0);
                    }
                }
            }

            for(int length = 4; length <= format.WordLengths.Max(); length++){
                int count = format.WordCount(length);

                for(ushort index = 0; index < count; index++){
                    var word = dict.ReadRaw(length, index);
                    var hash = Hash14(word) << 1;

                    if (!(words[hash] == index && length == lengths[hash])){
                        Update(hash + 1, length, index, 0);
                    }
                }
            }*/

            for(int length = 4; length <= format.WordLengths.Max(); length++){
                int count = format.WordCount(length);

                for(ushort index = 0; index < count; index++){
                    var word = dict.ReadTransformed(length, index, 9);
                    var hash = Hash14(word) << 1;

                    if (index != words[hash] && length > lengths[hash + 1]){
                        Update(hash + 1, length, index, 9);
                    }
                }
            }

            File.WriteAllLines(args[0], GenerateFile(words, lengths));
            return "Done.";
        }

        private static IEnumerable<string> GenerateFile(ushort[] words, byte[] lengths){
            var lines = new List<string>();

            static void GenerateSection<T>(List<string> lines, T[] values) where T : notnull{
                var line = new StringBuilder();

                foreach(var value in values){
                    var str = value.ToString()!;

                    if (line.Length + str.Length + 1 > 80){
                        lines.Add(line.ToString());
                        line.Clear();
                    }

                    line.Append(str).Append(',');
                }

                if (line.Length > 0){
                    lines.Add(line.ToString());
                }

                lines[^1] = lines[^1][..^1] + "};";
            }

            lines.Add("BROTLI_INTERNAL const uint16_t kStaticDictionaryHashWords[32768] = {");
            GenerateSection(lines, words);
            lines.Add("BROTLI_INTERNAL const uint8_t kStaticDictionaryHashLengths[32768] = {");
            GenerateSection(lines, lengths);

            return lines;
        }

        private static uint Hash14(ArraySegment<byte> data){
            uint n = 0;

            for(int offset = 0; offset < sizeof(uint); offset++){
                n |= (uint)data[offset] << (8 * offset);
            }

            return unchecked(n * 0x1E35A7BD) >> (32 - 14);
        }
    }
}
