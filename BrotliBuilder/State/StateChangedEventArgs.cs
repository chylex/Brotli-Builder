using System;

namespace BrotliBuilder.State{
    class StateChangedEventArgs : EventArgs{
        public BrotliFileState From { get; }
        public BrotliFileState To { get; }

        public StateChangedEventArgs(BrotliFileState from, BrotliFileState to){
            this.From = from;
            this.To = to;
        }
    }
}
