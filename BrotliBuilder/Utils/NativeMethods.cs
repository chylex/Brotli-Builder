using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BrotliBuilder.Utils{
    static class NativeMethods{
        public const int EM_SETTEXTMODE = 0x459;
        public const int TM_PLAINTEXT = 1;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        public static void SetPlainTextMode(this RichTextBox tb){
            SendMessage(tb.Handle, EM_SETTEXTMODE, (IntPtr)TM_PLAINTEXT, IntPtr.Zero);
        }
    }
}
