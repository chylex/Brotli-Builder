using System;
using System.Windows.Forms;

namespace BrotliBuilder.Blocks{
    interface IBuildingBlockContext{
        event EventHandler<EventArgs> Notified;

        void SetChildBlock(Func<IBuildingBlockContext, UserControl>? blockFactory);
        void NotifyParent(EventArgs args);
    }
}
