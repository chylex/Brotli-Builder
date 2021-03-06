﻿using BrotliCalc.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using BrotliCalc.Commands.Base;
using BrotliLib.Numbers;

namespace BrotliCalc{
    public static class Program{
        private static readonly List<ICommand?> CommandsAndSeparators = new List<ICommand?>{
            new CmdCompress(),
            new CmdCompressStats(),
            null,
            new CmdReserialize(),
            new CmdRebuild(),
            new CmdEncode(),
            new CmdTransform(),
            null,
            new CmdTestReserialize(),
            new CmdTestRebuild(),
            new CmdTestEncoder(),
            new CmdTestTransformer(),
            null,
            new CmdGenerateMarkers(),
            new CmdExtractWindowSize(),
            new CmdExtractHeaderMeta(),
            new CmdExtractContextMaps(),
            new CmdExtractInsertCopyStats(),
            new CmdExtractBlockTypes(),
            null,
            new CmdValidateCompression(),
            null,
            new CmdCopyUncompressed(),
            new CmdBenchReserializeRebuild(),
            new CmdTimeDictionaryIndex()
        };

        private static void Main(string[] args){
            Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            try{
                Arguments.Read(args);
            }catch(Exception e){
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error parsing the arguments:");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(e.Message);
                Debug.Print(e.ToString());
                return;
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Available commands:");
            Console.WriteLine();
            
            foreach(ICommand? command in CommandsAndSeparators){
                if (command == null){
                    Console.WriteLine();
                }
                else{
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" {command.ShortName} / {command.FullName} ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(command.ArgumentDesc);
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.WriteLine("Type q/quit to close the program.");
            Console.WriteLine();

            while(true){
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("> ");

                string[] input = Console.ReadLine()?.Split(new []{ ' ' }, 2, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
                
                if (input.Length == 0){
                    continue;
                }

                Console.ResetColor();
                Console.WriteLine();

                string trigger = input[0];

                if (trigger == "q" || trigger == "quit"){
                    break;
                }
                
                var command = CommandsAndSeparators.FirstOrDefault(cmd => cmd != null && (trigger == cmd.ShortName || trigger == cmd.FullName));

                if (command == null){
                    Console.WriteLine("Command not found.");
                    Console.WriteLine();
                    continue;
                }

                IntRange range = command.ArgumentCount;
                string[] arguments = ParseCommandArguments(input.ElementAtOrDefault(1) ?? string.Empty);

                if (!range.Contains(arguments.Length)){
                    if (range.First == range.Last){
                        Console.WriteLine($"Command requires exactly {range.First} argument(s).");
                    }
                    else{
                        Console.WriteLine($"Command requires between {range.First} and {range.Last} argument(s).");
                    }
                    
                    Console.WriteLine();
                    continue;
                }

                try{
                    Console.ForegroundColor = ConsoleColor.White;

                    var stopwatch = Stopwatch.StartNew();
                    var result = command.Process(arguments);
                    stopwatch.Stop();

                    Console.WriteLine(result);
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine($"Command finished in {stopwatch.ElapsedMilliseconds} ms.");
                }catch(Exception e){
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error processing the command:");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(e.Message);
                    Debug.Print(e.ToString());
                }
                
                Console.WriteLine();
            }
        }

        private static string[] ParseCommandArguments(string input){
            var chars = input.ToCharArray();

            bool inSingleQuote = false;
            bool inDoubleQuote = false;

            for(int index = 0; index < chars.Length; index++){
                var ch = chars[index];

                if (ch == '"' && !inSingleQuote){
                    inDoubleQuote = !inDoubleQuote;
                    chars[index] = '\n';
                }
                else if (ch == '\'' && !inDoubleQuote){
                    inSingleQuote = !inSingleQuote;
                    chars[index] = '\n';
                }
                else if (ch == ' ' && !inSingleQuote && !inDoubleQuote){
                    chars[index] = '\n';
                }
            }

            return new string(chars).Split(new []{ '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
