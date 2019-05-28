namespace BrotliBuilder.Components {
    partial class BrotliMarkerInfoPanel {
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BrotliMarkerInfoPanel));
            this.textBoxContext = new FastColoredTextBoxNS.FastColoredTextBox();
            this.labelMarkerInfo = new System.Windows.Forms.Label();
            this.panel = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.textBoxContext)).BeginInit();
            this.panel.SuspendLayout();
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
            this.textBoxContext.Location = new System.Drawing.Point(12, 20);
            this.textBoxContext.Margin = new System.Windows.Forms.Padding(12, 5, 0, 8);
            this.textBoxContext.Name = "textBoxContext";
            this.textBoxContext.Paddings = new System.Windows.Forms.Padding(0);
            this.textBoxContext.ReadOnly = true;
            this.textBoxContext.SelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(255)))));
            this.textBoxContext.ServiceColors = ((FastColoredTextBoxNS.ServiceColors)(resources.GetObject("textBoxContext.ServiceColors")));
            this.textBoxContext.ShowLineNumbers = false;
            this.textBoxContext.Size = new System.Drawing.Size(588, 372);
            this.textBoxContext.TabIndex = 1;
            this.textBoxContext.TabLength = 3;
            this.textBoxContext.WordWrapIndent = 1;
            this.textBoxContext.Zoom = 100;
            // 
            // labelMarkerInfo
            // 
            this.labelMarkerInfo.AutoSize = true;
            this.labelMarkerInfo.Location = new System.Drawing.Point(9, 2);
            this.labelMarkerInfo.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelMarkerInfo.Name = "labelMarkerInfo";
            this.labelMarkerInfo.Size = new System.Drawing.Size(61, 13);
            this.labelMarkerInfo.TabIndex = 2;
            this.labelMarkerInfo.Text = "Marker Info";
            // 
            // panel
            // 
            this.panel.Controls.Add(this.labelMarkerInfo);
            this.panel.Controls.Add(this.textBoxContext);
            this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel.Location = new System.Drawing.Point(0, 0);
            this.panel.Name = "panel";
            this.panel.Size = new System.Drawing.Size(600, 400);
            this.panel.TabIndex = 3;
            // 
            // BrotliMarkerInfoPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel);
            this.Name = "BrotliMarkerInfoPanel";
            this.Size = new System.Drawing.Size(600, 400);
            ((System.ComponentModel.ISupportInitialize)(this.textBoxContext)).EndInit();
            this.panel.ResumeLayout(false);
            this.panel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private FastColoredTextBoxNS.FastColoredTextBox textBoxContext;
        private System.Windows.Forms.Label labelMarkerInfo;
        private System.Windows.Forms.Panel panel;
    }
}
