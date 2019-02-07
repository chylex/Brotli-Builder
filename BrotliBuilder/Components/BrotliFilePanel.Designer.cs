namespace BrotliBuilder.Components {
    partial class BrotliFilePanel {
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
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.labelBitStream = new System.Windows.Forms.Label();
            this.textBoxBitStream = new RicherTextBox();
            this.textBoxOutput = new RicherTextBox();
            this.labelOutput = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.IsSplitterFixed = true;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.labelBitStream);
            this.splitContainer.Panel1.Controls.Add(this.textBoxBitStream);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.textBoxOutput);
            this.splitContainer.Panel2.Controls.Add(this.labelOutput);
            this.splitContainer.Size = new System.Drawing.Size(957, 422);
            this.splitContainer.SplitterDistance = 206;
            this.splitContainer.TabIndex = 1;
            // 
            // labelBitStream
            // 
            this.labelBitStream.AutoSize = true;
            this.labelBitStream.Location = new System.Drawing.Point(9, 2);
            this.labelBitStream.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelBitStream.Name = "labelBitStream";
            this.labelBitStream.Size = new System.Drawing.Size(55, 13);
            this.labelBitStream.TabIndex = 0;
            this.labelBitStream.Text = "Bit Stream";
            // 
            // textBoxBitStream
            // 
            this.textBoxBitStream.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBitStream.DetectUrls = false;
            this.textBoxBitStream.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxBitStream.Location = new System.Drawing.Point(12, 20);
            this.textBoxBitStream.Name = "textBoxBitStream";
            this.textBoxBitStream.ReadOnly = true;
            this.textBoxBitStream.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxBitStream.Size = new System.Drawing.Size(933, 183);
            this.textBoxBitStream.TabIndex = 1;
            this.textBoxBitStream.Text = "";
            // 
            // textBoxOutput
            // 
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.DetectUrls = false;
            this.textBoxOutput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxOutput.Location = new System.Drawing.Point(12, 20);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.Size = new System.Drawing.Size(933, 189);
            this.textBoxOutput.TabIndex = 1;
            this.textBoxOutput.Text = "";
            this.textBoxOutput.WordWrap = false;
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(9, 2);
            this.labelOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(39, 13);
            this.labelOutput.TabIndex = 0;
            this.labelOutput.Text = "Output";
            // 
            // BrotliFilePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "BrotliFilePanel";
            this.Size = new System.Drawing.Size(957, 422);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label labelBitStream;
        private RicherTextBox textBoxBitStream;
        private RicherTextBox textBoxOutput;
        private System.Windows.Forms.Label labelOutput;
    }
}
