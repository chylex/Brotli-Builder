using System.Windows.Forms;

namespace BrotliBuilder.Components{
    public sealed class RicherTextBox : RichTextBox{
        public int MaxDisplayedLength{
            get{
                return maxDisplayedLength;
            }

            set{
                if (maxDisplayedLength != value){
                    maxDisplayedLength = value;
                    UpdateDisplayedText();
                }
            }
        }

        public new string Text{
            get{
                return fullText;
            }

            set{
                fullText = value;
                UpdateDisplayedText();
            }
        }

        private int maxDisplayedLength = -1;
        private string fullText = string.Empty;
        
        private void UpdateDisplayedText(){
            if (maxDisplayedLength == -1 || fullText.Length <= maxDisplayedLength){
                base.Text = fullText;
            }
            else{
                base.Text = fullText.Substring(0, maxDisplayedLength) + "(...)";
            }
        }
    }
}
