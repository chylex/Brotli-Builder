namespace BrotliBuilder.Blocks.Structure {
    partial class BuildCompressedMetaBlock {
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
            this.checkBoxInsertCopy = new System.Windows.Forms.CheckBox();
            this.checkBoxBlockSwitchL = new System.Windows.Forms.CheckBox();
            this.checkBoxBlockSwitchI = new System.Windows.Forms.CheckBox();
            this.checkBoxBlockSwitchD = new System.Windows.Forms.CheckBox();
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
            this.listElements.Location = new System.Drawing.Point(3, 25);
            this.listElements.Name = "listElements";
            this.listElements.Size = new System.Drawing.Size(594, 372);
            this.listElements.TabIndex = 4;
            this.listElements.SelectedValueChanged += listElements_SelectedValueChanged;
            // 
            // checkBoxInsertCopy
            // 
            this.checkBoxInsertCopy.AutoSize = false;
            this.checkBoxInsertCopy.Checked = true;
            this.checkBoxInsertCopy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxInsertCopy.Location = new System.Drawing.Point(3, 3);
            this.checkBoxInsertCopy.Name = "checkBoxInsertCopy";
            this.checkBoxInsertCopy.Size = new System.Drawing.Size(143, 19);
            this.checkBoxInsertCopy.TabIndex = 0;
            this.checkBoxInsertCopy.Text = "Insert&&Copy";
            this.checkBoxInsertCopy.UseVisualStyleBackColor = true;
            this.checkBoxInsertCopy.CheckedChanged += checkBox_CheckedChanged;
            // 
            // checkBoxBlockSwitchL
            // 
            this.checkBoxBlockSwitchL.AutoSize = false;
            this.checkBoxBlockSwitchL.Checked = true;
            this.checkBoxBlockSwitchL.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBlockSwitchL.Location = new System.Drawing.Point(152, 3);
            this.checkBoxBlockSwitchL.Name = "checkBoxBlockSwitchL";
            this.checkBoxBlockSwitchL.Size = new System.Drawing.Size(143, 19);
            this.checkBoxBlockSwitchL.TabIndex = 1;
            this.checkBoxBlockSwitchL.Text = "Block-Switch [L]";
            this.checkBoxBlockSwitchL.UseVisualStyleBackColor = true;
            this.checkBoxBlockSwitchL.CheckedChanged += checkBox_CheckedChanged;
            // 
            // checkBoxBlockSwitchI
            // 
            this.checkBoxBlockSwitchI.AutoSize = false;
            this.checkBoxBlockSwitchI.Checked = true;
            this.checkBoxBlockSwitchI.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBlockSwitchI.Location = new System.Drawing.Point(301, 3);
            this.checkBoxBlockSwitchI.Name = "checkBoxBlockSwitchI";
            this.checkBoxBlockSwitchI.Size = new System.Drawing.Size(143, 19);
            this.checkBoxBlockSwitchI.TabIndex = 2;
            this.checkBoxBlockSwitchI.Text = "Block-Switch [I]";
            this.checkBoxBlockSwitchI.UseVisualStyleBackColor = true;
            this.checkBoxBlockSwitchI.CheckedChanged += checkBox_CheckedChanged;
            // 
            // checkBoxBlockSwitchD
            // 
            this.checkBoxBlockSwitchD.AutoSize = false;
            this.checkBoxBlockSwitchD.Checked = true;
            this.checkBoxBlockSwitchD.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxBlockSwitchD.Location = new System.Drawing.Point(450, 3);
            this.checkBoxBlockSwitchD.Name = "checkBoxBlockSwitchD";
            this.checkBoxBlockSwitchD.Size = new System.Drawing.Size(144, 19);
            this.checkBoxBlockSwitchD.TabIndex = 3;
            this.checkBoxBlockSwitchD.Text = "Block-Switch [D]";
            this.checkBoxBlockSwitchD.UseVisualStyleBackColor = true;
            this.checkBoxBlockSwitchD.CheckedChanged += checkBox_CheckedChanged;
            // 
            // BuildWindowSize
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listElements);
            this.Controls.Add(this.checkBoxInsertCopy);
            this.Controls.Add(this.checkBoxBlockSwitchL);
            this.Controls.Add(this.checkBoxBlockSwitchI);
            this.Controls.Add(this.checkBoxBlockSwitchD);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "BuildCompressedMetaBlock";
            this.Size = new System.Drawing.Size(600, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listElements;
        private System.Windows.Forms.CheckBox checkBoxInsertCopy;
        private System.Windows.Forms.CheckBox checkBoxBlockSwitchL;
        private System.Windows.Forms.CheckBox checkBoxBlockSwitchI;
        private System.Windows.Forms.CheckBox checkBoxBlockSwitchD;
    }
}
