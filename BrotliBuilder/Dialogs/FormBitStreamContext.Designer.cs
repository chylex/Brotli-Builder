namespace BrotliBuilder.Dialogs {
    partial class FormBitStreamContext {
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormBitStreamContext));
            this.textBoxContext = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxContext)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxContext
            // 
            this.textBoxContext.AllowDrop = false;
            this.textBoxContext.AllowMacroRecording = false;
            this.textBoxContext.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxContext.AutoCompleteBracketsList = new char[0];
            this.textBoxContext.AutoIndent = false;
            this.textBoxContext.AutoIndentChars = false;
            this.textBoxContext.AutoIndentCharsPatterns = "";
            this.textBoxContext.AutoIndentExistingLines = false;
            this.textBoxContext.AutoScrollMinSize = new System.Drawing.Size(2, 15);
            this.textBoxContext.BackBrush = null;
            this.textBoxContext.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxContext.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.textBoxContext.CharHeight = 15;
            this.textBoxContext.CharWidth = 7;
            this.textBoxContext.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxContext.DisabledColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.textBoxContext.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxContext.Hotkeys = resources.GetString("textBoxContext.Hotkeys");
            this.textBoxContext.IsReplaceMode = false;
            this.textBoxContext.Location = new System.Drawing.Point(12, 12);
            this.textBoxContext.Name = "textBoxContext";
            this.textBoxContext.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxContext.ReadOnly = true;
            this.textBoxContext.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textBoxContext.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxContext.ServiceColors")));
            this.textBoxContext.ShowLineNumbers = false;
            this.textBoxContext.Size = new System.Drawing.Size(280, 258);
            this.textBoxContext.TabIndex = 0;
            this.textBoxContext.TabLength = 3;
            this.textBoxContext.Zoom = 100;
            // 
            // FormBitStreamContext
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 282);
            this.Controls.Add(this.textBoxContext);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(320, 320);
            this.Name = "FormBitStreamContext";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Bit Stream";
            ((System.ComponentModel.ISupportInitialize)(this.textBoxContext)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox textBoxContext;
    }
}