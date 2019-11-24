using System.Diagnostics;
using System;
using BrotliCalc.Commands.Base;
using BrotliLib.Brotli.Dictionary.Default;
using BrotliLib.Numbers;

namespace BrotliCalc.Commands{
    class CmdTimeDictionaryIndex : ICommand{
        public string FullName => "time-dictionary-index";
        public string ShortName => "tdi";

        public string ArgumentDesc => "";
        public IntRange ArgumentCount => IntRange.Only(0);

        public string Process(string[] args){
            GC.Collect();

            long memStart = GC.GetTotalMemory(false);
            Stopwatch sw = Stopwatch.StartNew();

            var _ = BrotliDefaultDictionary.Embedded.Index;

            sw.Stop();

            long memPreGC = GC.GetTotalMemory(false);
            GC.Collect();
            long memPostGC = GC.GetTotalMemory(false);

            return $"Dictionary index constructed in {sw.ElapsedMilliseconds} ms. Memory delta after construction {(memPreGC - memStart) / 1024} kB, after collection {(memPostGC - memStart) / 1024} kB.";
        }
    }
}
