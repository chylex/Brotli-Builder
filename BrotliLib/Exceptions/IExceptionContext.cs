using System.Text;

namespace BrotliLib.Exceptions{
    interface IExceptionContext{
        void Explain(StringBuilder build);
    }
}
