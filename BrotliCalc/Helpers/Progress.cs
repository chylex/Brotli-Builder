using System;
using System.Threading;

namespace BrotliCalc.Helpers{
    sealed class Progress : IDisposable{
        private readonly object consoleLock = new object();

        private readonly string total;
        private readonly int digits;

        private int current = 0;

        public Progress(int count){
            this.total = count.ToString();
            this.digits = total.Length;
        }

        public void Print(string message){
            if (Console.IsOutputRedirected){
                Console.WriteLine(message);
            }
            else{
                lock(consoleLock){
                    Console.WriteLine(message);
                }
            }
        }

        public void Post(string message){
            string text = $"[{(Interlocked.Increment(ref current)).ToString().PadLeft(digits, ' ')}/{total}] {message}";

            if (Console.IsOutputRedirected){
                Console.WriteLine(text);
            }
            else{
                lock(consoleLock){
                    int width = Console.BufferWidth;

                    if (width < 3){
                        text = new string('.', width);
                    }
                    else if (text.Length > width){
                        text = text.Substring(0, width - 3) + "...";
                    }
                    else{
                        text = text.PadRight(width, ' ');
                    }

                    WriteAndJumpBack(text);
                }
            }
        }

        public void Dispose(){
            if (current > 0){
                if (Console.IsOutputRedirected){
                    Console.WriteLine();
                }
                else{
                    WriteAndJumpBack(new string(' ', Console.BufferWidth));
                }
            }
        }

        private void WriteAndJumpBack(string text){
            int prevTop = Console.CursorTop;

            Console.Write(text);
            Console.CursorTop = prevTop;
            Console.CursorLeft = 0;
        }
    }
}
