using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrotliBuilder.Utils{
    sealed class AsyncWorker{
        public string Name { get; }

        private readonly TaskFactory taskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        private Thread? currentThread;
        private CancellationTokenSource? currentToken;

        public AsyncWorker(string name){
            this.Name = name;
        }

        public void Start(Action<object> action){
            Abort();

            currentToken = new CancellationTokenSource();

            currentThread = new Thread(new ParameterizedThreadStart(action)){
                Name = this.Name,
                IsBackground = true
            };

            currentThread.Start(currentToken.Token);
        }

        public void Sync(Action action){
            taskFactory.StartNew(action);
        }

        public void Abort(){
            Thread? thread = currentThread;

            if (thread != null && thread.IsAlive){
                currentToken?.Cancel();

                try{
                    thread.Abort();
                }catch(PlatformNotSupportedException){
                    thread.Priority = ThreadPriority.Lowest; // cannot kill, so it will just stay in the background until it reaches a cancellation point...
                }
                
                currentThread = null;
                currentToken = null;
            }
        }
    }
}
