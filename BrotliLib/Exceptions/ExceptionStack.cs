using System.Collections.Generic;

namespace BrotliLib.Exceptions{
    sealed class ExceptionStack{
        private readonly List<IExceptionContext> stack = new List<IExceptionContext>();

        public ExceptionStack Clone(){
            var clone = new ExceptionStack();
            clone.stack.AddRange(stack);
            return clone;
        }

        public void PushContext(IExceptionContext context){
            stack.Add(context);
        }

        public void PopContext(){
            stack.RemoveAt(stack.Count - 1);
        }
    }
}
