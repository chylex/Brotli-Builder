using System.ComponentModel;

namespace BrotliBuilder.Utils.Compat{
    abstract class StatusBarBase : Component{
        public abstract bool Visible { get; set; }

        public abstract Panel AddPanel(int width);
        public abstract void AddPadding(int width);

        public abstract class Panel{
            public abstract int Width { get; set; }
            public abstract string Text { get; set; }
        }
    }
}
