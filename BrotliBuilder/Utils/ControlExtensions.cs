using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace BrotliBuilder.Utils{
    static class ControlExtensions{
        public static void EnableDoubleBuffering(this DataGridView dataGridView){
            // Visual Studio is garbage and the designer crashes if DataGridView is extended, so reflection it is
            dataGridView.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(dataGridView, true, null);
        }

        public static void SetValueInstantly(this ProgressBar progressBar, int value){
            // update progres bar instantly
            if (value == progressBar.Maximum){
                progressBar.Maximum++;
                progressBar.Value = value + 1;
                progressBar.Maximum--;
            }
            else{
                progressBar.Value = value + 1;
            }

            progressBar.Value = value;
        }

        public static void ScrollToCaretCentered(this RichTextBox tb){
            if (tb.Lines.Length < 2){
                return;
            }

            Point pos1 = tb.GetPositionFromCharIndex(tb.GetFirstCharIndexFromLine(0));
            Point pos2 = tb.GetPositionFromCharIndex(tb.GetFirstCharIndexFromLine(1));
            int lineHeight = pos2.Y - pos1.Y;

            int prevSelectionStart = tb.SelectionStart;
            int prevSelectionLength = tb.SelectionLength;

            int centerLine = tb.GetLineFromCharIndex(prevSelectionStart);
            int topLine = Math.Max(0, centerLine - (tb.Height / lineHeight) / 2);
            
            tb.Select(tb.GetFirstCharIndexFromLine(topLine), 0);
            tb.ScrollToCaret();

            tb.Select(prevSelectionStart, prevSelectionLength);
        }
    }
}
