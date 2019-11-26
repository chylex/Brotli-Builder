using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrotliBuilder.Utils{
    sealed class AsyncWorker{
        public string Name { get; }

        private readonly TaskFactory taskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        private Thread? currentThread;

        public AsyncWorker(string name){
            this.Name = name;
        }

        public void Start(Action action){
            Abort();

            currentThread = new Thread(new ThreadStart(action)){
                Name = this.Name,
                IsBackground = true
            };

            currentThread.Start();
        }

        public void Sync(Action action){
            taskFactory.StartNew(action);
        }

        public void Abort(){
            Thread? thread = currentThread;

            if (thread != null && thread.IsAlive){
                thread.Abort();
                currentThread = null;
            }
        }
    }
}
