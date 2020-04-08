using System.Collections;
using System.Text;

namespace BrotliLib.Exceptions.Contexts{
    class TitledListCtx : IExceptionContext{
        private readonly string title;
        private readonly IEnumerable enumerable;

        public TitledListCtx(string title, IEnumerable enumerable){
            this.title = title;
            this.enumerable = enumerable;
        }

        public void Explain(StringBuilder build){
            build.Append(title)
                 .Append(": [\n  ")
                 .AppendJoin(",\n  ", enumerable)
                 .Append("\n]\n");
        }
    }
}
