using BrotliLib.Numbers;

namespace BrotliCalc{
    interface ICommand{
        string FullName { get; }
        string ShortName { get; }

        string ArgumentDesc { get; }
        IntRange ArgumentCount { get; }

        string Process(string[] args);
    }
}
