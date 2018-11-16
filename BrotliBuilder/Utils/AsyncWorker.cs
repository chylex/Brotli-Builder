using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BrotliBuilder.Utils{
    sealed class AsyncWorker<T>{
        public class FinishedArgs : EventArgs{
            public T Result { get; }
            public Stopwatch Stopwatch { get; }

            public FinishedArgs(T result, Stopwatch stopwatch){
                this.Result = result;
                this.Stopwatch = stopwatch;
            }
        }

        public class CrashedArgs : EventArgs{
            public Exception Exception { get; }

            public CrashedArgs(Exception exception){
                this.Exception = exception;
            }
        }

        public event EventHandler<FinishedArgs> WorkFinished;
        public event EventHandler<CrashedArgs> WorkCrashed;
        public event EventHandler<EventArgs> WorkAborted;

        public bool IsBusy => currentThread?.IsAlive == true;

        private readonly TaskFactory taskFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        private readonly string threadName;
        private Thread currentThread;

        public AsyncWorker(string threadName){
            this.threadName = threadName;
        }
        
        public void Start(Func<T> action){
            Abort();

            currentThread = new Thread(Work){
                Name = threadName,
                IsBackground = true
            };

            currentThread.Start(action);
        }

        public void Abort(){
            Thread thread = currentThread;

            if (thread != null && thread.IsAlive){
                thread.Abort();
                currentThread = null;
            }
        }

        private void Work(object data){
            Func<T> action = (Func<T>)data;

            try{
                Stopwatch stopwatch = Stopwatch.StartNew();
                T result = action.Invoke();
                stopwatch.Stop();

                taskFactory.StartNew(() => WorkFinished?.Invoke(this, new FinishedArgs(result, stopwatch)));
            }catch(ThreadAbortException){
                taskFactory.StartNew(() => WorkAborted?.Invoke(this, EventArgs.Empty));
            }catch(Exception e){
                taskFactory.StartNew(() => WorkCrashed?.Invoke(this, new CrashedArgs(e)));
            }
        }
    }
}
