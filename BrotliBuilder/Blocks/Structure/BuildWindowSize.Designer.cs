namespace BrotliBuilder.Blocks.Structure {
    partial class BuildWindowSize {
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
            this.listElements = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // listElements
            // 
            this.listElements.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listElements.FormattingEnabled = true;
            this.listElements.IntegralHeight = false;
            this.listElements.ItemHeight = 15;
            this.listElements.Location = new System.Drawing.Point(3, 3);
            this.listElements.Name = "listElements";
            this.listElements.Size = new System.Drawing.Size(194, 394);
            this.listElements.TabIndex = 0;
            // 
            // BuildWindowSize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listElements);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "BuildWindowSize";
            this.Size = new System.Drawing.Size(200, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listElements;
    }
}
