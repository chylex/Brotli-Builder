namespace BrotliBuilder.Blocks.Structure {
    partial class BuildEmptyMetaBlock {
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
            this.textBoxHiddenText = new System.Windows.Forms.RichTextBox();
            this.labelHiddenText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxHiddenText
            // 
            this.textBoxHiddenText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxHiddenText.DetectUrls = false;
            this.textBoxHiddenText.Location = new System.Drawing.Point(3, 21);
            this.textBoxHiddenText.MaxLength = 16777216;
            this.textBoxHiddenText.Name = "textBoxHiddenText";
            this.textBoxHiddenText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxHiddenText.Size = new System.Drawing.Size(594, 376);
            this.textBoxHiddenText.TabIndex = 1;
            this.textBoxHiddenText.Text = "";
            // 
            // labelHiddenText
            // 
            this.labelHiddenText.AutoSize = true;
            this.labelHiddenText.Location = new System.Drawing.Point(0, 3);
            this.labelHiddenText.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.labelHiddenText.Name = "labelHiddenText";
            this.labelHiddenText.Size = new System.Drawing.Size(98, 15);
            this.labelHiddenText.TabIndex = 0;
            this.labelHiddenText.Text = "Hidden Text Data";
            // 
            // BuildEmptyMetaBlock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelHiddenText);
            this.Controls.Add(this.textBoxHiddenText);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "BuildEmptyMetaBlock";
            this.Size = new System.Drawing.Size(600, 400);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox textBoxHiddenText;
        private System.Windows.Forms.Label labelHiddenText;
    }
}
