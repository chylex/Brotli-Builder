using System.Collections.Generic;

namespace BrotliLib.Brotli.Output{
    internal sealed class BrotliOutputMultiple : IBrotliOutput{
        public int OutputSize => wrapped.OutputSize;

        private readonly IBrotliOutput wrapped;
        private readonly List<IBrotliOutput> callbacks = new List<IBrotliOutput>();

        public BrotliOutputMultiple(IBrotliOutput wrapped){
            this.wrapped = wrapped;
        }

        public void AddCallback(IBrotliOutput dlg){
            if (!callbacks.Contains(dlg)){
                callbacks.Add(dlg);
            }
        }

        public void RemoveCallback(IBrotliOutput dlg){
            callbacks.Remove(dlg);
        }

        public void Write(byte value){
            wrapped.Write(value);
            
            foreach(var callback in callbacks){
                callback.Write(value);
            }
        }

        public void Write(byte[] bytes){
            wrapped.Write(bytes);
            
            foreach(var callback in callbacks){
                callback.Write(bytes);
            }
        }

        public byte GetByte(int distance){
            return wrapped.GetByte(distance);
        }

        public IBrotliOutput Clone(){
            var cloned = new BrotliOutputMultiple(wrapped.Clone());

            foreach(var callback in callbacks){
                cloned.callbacks.Add(callback.Clone());
            }

            return wrapped.Clone();
        }
    }
}
