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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrotliFilePanel));
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.textBoxBitStream = new BrotliBuilder.Components.MarkedTextBox();
            this.labelBitStream = new System.Windows.Forms.Label();
            this.textBoxOutput = new FastColoredTextBoxNS.FastColoredTextBox();
            this.labelOutput = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxBitStream)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxOutput)).BeginInit();
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
            this.splitContainer.Panel1.Controls.Add(this.textBoxBitStream);
            this.splitContainer.Panel1.Controls.Add(this.labelBitStream);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.textBoxOutput);
            this.splitContainer.Panel2.Controls.Add(this.labelOutput);
            this.splitContainer.Size = new System.Drawing.Size(957, 422);
            this.splitContainer.SplitterDistance = 206;
            this.splitContainer.TabIndex = 0;
            // 
            // textBoxBitStream
            // 
            this.textBoxBitStream.AllowDrop = false;
            this.textBoxBitStream.AllowMacroRecording = false;
            this.textBoxBitStream.AllowSeveralTextStyleDrawing = true;
            this.textBoxBitStream.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBitStream.AutoCompleteBracketsList = new char[0];
            this.textBoxBitStream.AutoIndent = false;
            this.textBoxBitStream.AutoIndentChars = false;
            this.textBoxBitStream.AutoIndentCharsPatterns = "";
            this.textBoxBitStream.AutoIndentExistingLines = false;
            this.textBoxBitStream.AutoScrollMinSize = new System.Drawing.Size(0, 15);
            this.textBoxBitStream.BackBrush = null;
            this.textBoxBitStream.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxBitStream.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.textBoxBitStream.CharHeight = 15;
            this.textBoxBitStream.CharWidth = 7;
            this.textBoxBitStream.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxBitStream.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBoxBitStream.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxBitStream.Hotkeys = resources.GetString("textBoxBitStream.Hotkeys");
            this.textBoxBitStream.IsReplaceMode = false;
            this.textBoxBitStream.Location = new System.Drawing.Point(12, 20);
            this.textBoxBitStream.Name = "textBoxBitStream";
            this.textBoxBitStream.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxBitStream.ReadOnly = true;
            this.textBoxBitStream.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textBoxBitStream.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxBitStream.ServiceColors")));
            this.textBoxBitStream.ShowLineNumbers = false;
            this.textBoxBitStream.Size = new System.Drawing.Size(933, 183);
            this.textBoxBitStream.TabIndex = 1;
            this.textBoxBitStream.WordWrap = true;
            this.textBoxBitStream.WordWrapMode = FastColoredTextBoxNS.WordWrapMode.CharWrapControlWidth;
            this.textBoxBitStream.Zoom = 100;
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
            // textBoxOutput
            // 
            this.textBoxOutput.AllowDrop = false;
            this.textBoxOutput.AllowMacroRecording = false;
            this.textBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutput.AutoCompleteBracketsList = new char[0];
            this.textBoxOutput.AutoIndent = false;
            this.textBoxOutput.AutoIndentChars = false;
            this.textBoxOutput.AutoIndentCharsPatterns = "";
            this.textBoxOutput.AutoIndentExistingLines = false;
            this.textBoxOutput.AutoScrollMinSize = new System.Drawing.Size(25, 15);
            this.textBoxOutput.BackBrush = null;
            this.textBoxOutput.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxOutput.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.textBoxOutput.CharHeight = 15;
            this.textBoxOutput.CharWidth = 7;
            this.textBoxOutput.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxOutput.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBoxOutput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxOutput.Hotkeys = resources.GetString("textBoxOutput.Hotkeys");
            this.textBoxOutput.IsReplaceMode = false;
            this.textBoxOutput.LineNumberColor = System.Drawing.Color.Gray;
            this.textBoxOutput.Location = new System.Drawing.Point(12, 20);
            this.textBoxOutput.Name = "textBoxOutput";
            this.textBoxOutput.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxOutput.ReadOnly = true;
            this.textBoxOutput.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textBoxOutput.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxOutput.ServiceColors")));
            this.textBoxOutput.Size = new System.Drawing.Size(933, 183);
            this.textBoxOutput.TabIndex = 1;
            this.textBoxOutput.WordWrapMode = FastColoredTextBoxNS.WordWrapMode.CharWrapControlWidth;
            this.textBoxOutput.Zoom = 100;
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
            ((System.ComponentModel.ISupportInitialize)(this.textBoxBitStream)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxOutput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Label labelBitStream;
        private System.Windows.Forms.Label labelOutput;
        private BrotliBuilder.Components.MarkedTextBox textBoxBitStream;
        private FastColoredTextBoxNS.FastColoredTextBox textBoxOutput;
    }
}
