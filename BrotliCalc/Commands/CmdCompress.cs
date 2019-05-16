using System.Linq;
using BrotliCalc.Helpers;
using BrotliLib.Brotli.Components;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdCompress : ICommand{
        public string FullName => "compress";
        public string ShortName => "c";

        public string ArgumentDesc => "<source-path> <quality|all> [window-size]";
        public Range ArgumentCount => new Range(2, 3);

        public string Process(string[] args){
            var qualities = args[1] == "all" ? Enumerable.Range(0, 12) : Enumerable.Range(int.Parse(args[1]), 1);
            var windowSize = args.Length >= 3 ? new WindowSize(int.Parse(args[2])) : WindowSize.Default;
            
            return $"Compressed files: {qualities.Sum(quality => Brotli.CompressPath(args[0], quality, windowSize))}";
        }
    }
}
