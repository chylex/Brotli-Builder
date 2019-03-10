namespace BrotliCalc{
    interface ICommand{
        string FullName { get; }
        string ShortName { get; }
        string Arguments { get; }

        string Process(string[] args);
    }
}
