namespace BrotliBuilder.Blocks.Structure {
    partial class BuildUncompressedMetaBlock {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.textBoxUncompressedText = new System.Windows.Forms.RichTextBox();
            this.labelUncompressedText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxUncompressedText
            // 
            this.textBoxUncompressedText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxUncompressedText.DetectUrls = false;
            this.textBoxUncompressedText.Location = new System.Drawing.Point(3, 21);
            this.textBoxUncompressedText.MaxLength = 16777216;
            this.textBoxUncompressedText.Name = "textBoxUncompressedText";
            this.textBoxUncompressedText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxUncompressedText.Size = new System.Drawing.Size(594, 376);
            this.textBoxUncompressedText.TabIndex = 1;
            this.textBoxUncompressedText.Text = "";
            // 
            // labelUncompressedText
            // 
            this.labelUncompressedText.AutoSize = true;
            this.labelUncompressedText.Location = new System.Drawing.Point(0, 3);
            this.labelUncompressedText.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelUncompressedText.Name = "labelUncompressedText";
            this.labelUncompressedText.Size = new System.Drawing.Size(56, 15);
            this.labelUncompressedText.TabIndex = 0;
            this.labelUncompressedText.Text = "Text Data";
            // 
            // BuildUncompressedMetaBlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelUncompressedText);
            this.Controls.Add(this.textBoxUncompressedText);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "BuildUncompressedMetaBlock";
            this.Size = new System.Drawing.Size(600, 400);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox textBoxUncompressedText;
        private System.Windows.Forms.Label labelUncompressedText;
    }
}
