using BrotliCalc.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using BrotliLib.Numbers;

namespace BrotliCalc{
    public static class Program{
        private static readonly List<ICommand> Commands = new List<ICommand>{
            new CmdCompress(),
            new CmdTestReserializeRebuild(),
            new CmdGenDictionaryIndex()
        };

        private static void Main(){
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Available commands:");
            Console.WriteLine();
            
            foreach(ICommand command in Commands){
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($" {command.ShortName} / {command.FullName} ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(command.ArgumentDesc);
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine();
            Console.WriteLine("Type r/repeat to repeat the last command.");
            Console.WriteLine("Type q/quit to close the program.");
            Console.WriteLine();

            string[] lastInput = null;

            while(true){
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write("> ");

                string[] input = Console.ReadLine().Split(new []{ ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                
                if (input.Length == 0){
                    continue;
                }

                Console.ResetColor();
                Console.WriteLine();

                string trigger = input[0];

                if ((trigger == "r" || trigger == "repeat") && lastInput != null){
                    input = lastInput;
                    trigger = input[0];
                }
                else if (trigger == "q" || trigger == "quit"){
                    break;
                }
                
                var command = Commands.FirstOrDefault(cmd1 => trigger == cmd1.FullName) ?? Commands.FirstOrDefault(cmd2 => trigger == cmd2.ShortName);

                if (command == null){
                    Console.WriteLine("Command not found.");
                    Console.WriteLine();
                    continue;
                }

                Range range = command.ArgumentCount;
                string[] args = ParseCommandArguments(input.ElementAtOrDefault(1) ?? string.Empty);

                if (!range.Contains(args.Length)){
                    if (range.First == range.Last){
                        Console.WriteLine($"Command requires exactly {range.First} argument(s).");
                    }
                    else{
                        Console.WriteLine($"Command requires between {range.First} and {range.Last} argument(s).");
                    }
                    
                    Console.WriteLine();
                    continue;
                }

                lastInput = input;

                try{
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(command.Process(args));
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
