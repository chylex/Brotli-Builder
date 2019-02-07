using System;
using System.Threading;
using System.Threading.Tasks;

namespace BrotliBuilder.Utils{
    sealed class AsyncWorker{
        public delegate void Work(Action<Action> sync);

        public string Name { get; set; }

        private readonly TaskFactory taskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        private Thread currentThread;
        
        public void Start(Work action){
            Abort();

            void Worker(){
                action(callback => taskFactory.StartNew(callback));
            }

            currentThread = new Thread(Worker){
                Name = this.Name,
                IsBackground = true
            };

            currentThread.Start();
        }

        public void Abort(){
            Thread thread = currentThread;

            if (thread != null && thread.IsAlive){
                thread.Abort();
                currentThread = null;
            }
        }
    }
}
