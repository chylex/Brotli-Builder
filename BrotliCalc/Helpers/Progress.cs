using System;
using System.Collections.Generic;
using System.Threading;

namespace BrotliCalc.Helpers{
    sealed class Progress : IDisposable{
        private readonly object consoleLock = new object();

        private readonly string total;
        private readonly int digits;
        
        private readonly Dictionary<int, Line> threadLines = new Dictionary<int, Line>();
        private readonly Line statusLine = new Line(0);
        private bool started = false;
        private int finished = 0;

        private class Line{
            public int Offset { get; }
            public string Text { get; set; }

            public Line(int offset){
                this.Offset = offset;
                this.Text = string.Empty;
            }
        }

        public Progress(int count){
            this.total = count.ToString();
            this.digits = total.Length;
        }

        public void Print(ConsoleColor color, string message){
            if (Console.IsOutputRedirected){
                var prevColor = Console.ForegroundColor;

                Console.ForegroundColor = color;
                Console.WriteLine(message);
                Console.ForegroundColor = prevColor;
            }
            else{
                lock(consoleLock){
                    var prevColor = Console.ForegroundColor;

                    Console.ForegroundColor = color;
                    WriteAndJumpBack(0, message.PadRight(Console.BufferWidth, ' '));
                    Console.CursorTop += 1;
                    Console.ForegroundColor = prevColor;

                    WriteLine(statusLine);

                    foreach(var line in threadLines.Values){
                        WriteLine(line);
                    }
                }
            }
        }

        public void Start(string message){
            if (Console.IsOutputRedirected){
                return;
            }

            lock(consoleLock){
                if (!started){
                    started = true;
                    statusLine.Text = $"[{"0".PadLeft(digits, ' ')}/{total}]";
                    WriteLine(statusLine);
                }

                WriteThreadAndJumpBack(PadWithEllipsis(message, Console.BufferWidth));
            }
        }

        public void Finish(string message){
            if (Console.IsOutputRedirected){
                Console.WriteLine($"[{(Interlocked.Increment(ref finished)).ToString().PadLeft(digits, ' ')}/{total}] {message}");
            }
            else{
                lock(consoleLock){
                    statusLine.Text = $"[{(++finished).ToString().PadLeft(digits, ' ')}/{total}]";
                    WriteLine(statusLine);
                    WriteThreadAndJumpBack(message);
                }
            }
        }

        public void Dispose(){
            if (finished > 0){
                if (Console.IsOutputRedirected){
                    Console.WriteLine();
                }
                else{
                    for(int line = 0; line < 1 + threadLines.Count; line++){
                        WriteAndJumpBack(line, new string(' ', Console.BufferWidth));
                    }
                }
            }
        }

        // Helpers

        private string PadWithEllipsis(string message, int maxLength){
            if (maxLength < 3){
                return new string('.', maxLength);
            }
            else if (message.Length > maxLength){
                return message.Substring(0, maxLength - 3) + "...";
            }
            else{
                return message.PadRight(maxLength, ' ');
            }
        }

        private void WriteThreadAndJumpBack(string text){
            int id = Thread.CurrentThread.ManagedThreadId;
            var line = threadLines.TryGetValue(id, out var found) ? found : (threadLines[id] = new Line(1 + threadLines.Count));

            line.Text = text;
            WriteLine(line);
        }

        private void WriteLine(Line line){
            WriteAndJumpBack(line.Offset, PadWithEllipsis(line.Text, Console.BufferWidth));
        }

        private void WriteAndJumpBack(int line, string text){
            int prevTop = Console.CursorTop;

            Console.CursorTop = prevTop + line;
            Console.Write(text);
            Console.CursorTop = prevTop;
            Console.CursorLeft = 0;
        }
    }
}
