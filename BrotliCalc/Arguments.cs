using System;
using System.IO;
using BrotliCalc.Commands;
using BrotliCalc.Helpers;
using BrotliLib.Numbers;

namespace BrotliCalc{
    static class Arguments{
        public static void Read(string[] args){
            for(int index = 0; index < args.Length;){
                string key = args[index];
                string? value = index + 1 < args.Length ? args[index + 1] : null;

                index += ProcessArgument(key, value);
            }
        }

        private static int ProcessArgument(string key, string? value){
            switch(key){
                case "-help":
                    static void Print(string arg, string description){
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(" ");
                        Console.WriteLine(arg);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.Write("  ");
                        Console.WriteLine(description);
                    }

                    Console.WriteLine("BrotliCalc arguments");
                    Console.WriteLine("--------------------");

                    Print("-brotliexe <path>", "Path to brotli executable used for compression.");
                    Print("-maxdop <n>", "Maximum degree of parallelism (threads/processes) to use for parallel tasks.");
                    Print("-fileorder {system|quality}", "Whether to process files in system order, or order by compression quality.");

                    Environment.Exit(0);
                    return 1;

                case "-brotliexe":
                    CmdCompress.CustomExePath = ParseFile(key, value);
                    return 2;

                case "-maxdop":
                    Linq.MaxThreads = ParseInt(key, value, IntRange.AtLeast(1));
                    return 2;

                case "-fileorder":
                    Brotli.FileOrder = value switch{
                        "system"  => Brotli.FileOrdering.System,
                        "quality" => Brotli.FileOrdering.Quality,
                        _ => throw new ArgumentException($"Unknown file ordering {value}")
                    };

                    return 2;

                default:
                    throw new ArgumentException($"Unknown argument {key}");
            }
        }

        // Parsing

        private static int ParseInt(string key, string? value, IntRange range = default /* IntRange.Any */){
            if (value == null){
                throw new ArgumentException($"Missing value for argument {key}");
            }
            else if (!int.TryParse(value, out int result)){
                throw new FormatException($"Value of argument {key} must be a number");
            }
            else if (!range.Contains(result)){
                throw new ArgumentException($"Value of argument {key} must be in the range {range}");
            }
            else{
                return result;
            }
        }

        private static string ParseFolder(string key, string? value){
            if (value == null || !Directory.Exists(value)){
                throw new ArgumentException($"Folder specified by argument {key} does not exist: {value ?? "<missing>"}");
            }
            else{
                return value;
            }
        }

        private static string ParseFile(string key, string? value){
            if (value == null || !File.Exists(value)){
                throw new ArgumentException($"File specified by argument {key} does not exist: {value ?? "<missing>"}");
            }
            else{
                return value;
            }
        }
    }
}
