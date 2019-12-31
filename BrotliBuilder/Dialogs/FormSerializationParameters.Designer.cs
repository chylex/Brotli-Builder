namespace BrotliBuilder.Dialogs {
    partial class FormSerializationParameters {
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
            this.checkBoxComplexTreeSkipCode = new System.Windows.Forms.CheckBox();
            this.checkBoxComplexTreeRepeatCode = new System.Windows.Forms.CheckBox();
            this.groupHuffmanTrees = new System.Windows.Forms.GroupBox();
            this.groupContextMaps = new System.Windows.Forms.GroupBox();
            this.checkBoxContextMapIMTF = new System.Windows.Forms.CheckBox();
            this.checkBoxContextMapRLE = new System.Windows.Forms.CheckBox();
            this.buttonReserialize = new System.Windows.Forms.Button();
            // 
            // checkBoxComplexTreeSkipCode
            // 
            this.checkBoxComplexTreeSkipCode.AutoSize = true;
            this.checkBoxComplexTreeSkipCode.Checked = true;
            this.checkBoxComplexTreeSkipCode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxComplexTreeSkipCode.Location = new System.Drawing.Point(12, 22);
            this.checkBoxComplexTreeSkipCode.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
            this.checkBoxComplexTreeSkipCode.Name = "checkBoxComplexTreeSkipCode";
            this.checkBoxComplexTreeSkipCode.Size = new System.Drawing.Size(176, 19);
            this.checkBoxComplexTreeSkipCode.TabIndex = 0;
            this.checkBoxComplexTreeSkipCode.Text = "Use Complex Tree Skip Code";
            this.checkBoxComplexTreeSkipCode.UseVisualStyleBackColor = true;
            // 
            // checkBoxComplexTreeRepeatCode
            // 
            this.checkBoxComplexTreeRepeatCode.AutoSize = true;
            this.checkBoxComplexTreeRepeatCode.Checked = true;
            this.checkBoxComplexTreeRepeatCode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxComplexTreeRepeatCode.Location = new System.Drawing.Point(12, 47);
            this.checkBoxComplexTreeRepeatCode.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
            this.checkBoxComplexTreeRepeatCode.Name = "checkBoxComplexTreeRepeatCode";
            this.checkBoxComplexTreeRepeatCode.Size = new System.Drawing.Size(190, 19);
            this.checkBoxComplexTreeRepeatCode.TabIndex = 1;
            this.checkBoxComplexTreeRepeatCode.Text = "Use Complex Tree Repeat Code";
            this.checkBoxComplexTreeRepeatCode.UseVisualStyleBackColor = true;
            // 
            // groupHuffmanTrees
            // 
            this.groupHuffmanTrees.Controls.Add(this.checkBoxComplexTreeSkipCode);
            this.groupHuffmanTrees.Controls.Add(this.checkBoxComplexTreeRepeatCode);
            this.groupHuffmanTrees.Location = new System.Drawing.Point(12, 12);
            this.groupHuffmanTrees.Name = "groupHuffmanTrees";
            this.groupHuffmanTrees.Size = new System.Drawing.Size(219, 75);
            this.groupHuffmanTrees.TabIndex = 0;
            this.groupHuffmanTrees.TabStop = false;
            this.groupHuffmanTrees.Text = "Huffman Trees";
            // 
            // groupContextMaps
            // 
            this.groupContextMaps.Controls.Add(this.checkBoxContextMapIMTF);
            this.groupContextMaps.Controls.Add(this.checkBoxContextMapRLE);
            this.groupContextMaps.Location = new System.Drawing.Point(237, 12);
            this.groupContextMaps.Name = "groupContextMaps";
            this.groupContextMaps.Size = new System.Drawing.Size(247, 75);
            this.groupContextMaps.TabIndex = 1;
            this.groupContextMaps.TabStop = false;
            this.groupContextMaps.Text = "Context Maps";
            // 
            // checkBoxContextMapIMTF
            // 
            this.checkBoxContextMapIMTF.AutoSize = true;
            this.checkBoxContextMapIMTF.Checked = true;
            this.checkBoxContextMapIMTF.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxContextMapIMTF.Location = new System.Drawing.Point(12, 22);
            this.checkBoxContextMapIMTF.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
            this.checkBoxContextMapIMTF.Name = "checkBoxContextMapIMTF";
            this.checkBoxContextMapIMTF.Size = new System.Drawing.Size(226, 19);
            this.checkBoxContextMapIMTF.TabIndex = 0;
            this.checkBoxContextMapIMTF.Text = "Use Inverse Move-To-Front Transform";
            this.checkBoxContextMapIMTF.UseVisualStyleBackColor = true;
            // 
            // checkBoxContextMapRLE
            // 
            this.checkBoxContextMapRLE.AutoSize = true;
            this.checkBoxContextMapRLE.Checked = true;
            this.checkBoxContextMapRLE.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxContextMapRLE.Location = new System.Drawing.Point(12, 47);
            this.checkBoxContextMapRLE.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
            this.checkBoxContextMapRLE.Name = "checkBoxContextMapRLE";
            this.checkBoxContextMapRLE.Size = new System.Drawing.Size(164, 19);
            this.checkBoxContextMapRLE.TabIndex = 1;
            this.checkBoxContextMapRLE.Text = "Use Run-Length Encoding";
            this.checkBoxContextMapRLE.UseVisualStyleBackColor = true;
            // 
            // buttonReserialize
            // 
            this.buttonReserialize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReserialize.Location = new System.Drawing.Point(391, 93);
            this.buttonReserialize.Name = "buttonReserialize";
            this.buttonReserialize.Size = new System.Drawing.Size(93, 23);
            this.buttonReserialize.TabIndex = 2;
            this.buttonReserialize.Text = "Reserialize";
            this.buttonReserialize.UseVisualStyleBackColor = true;
            this.buttonReserialize.Click += new System.EventHandler(this.buttonReserialize_Click);
            // 
            // FormParameters
            // 
            this.ClientSize = new System.Drawing.Size(496, 128);
            this.Controls.Add(this.buttonReserialize);
            this.Controls.Add(this.groupContextMaps);
            this.Controls.Add(this.groupHuffmanTrees);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormSerializationParameters";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Serialization Parameters";

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxComplexTreeSkipCode;
        private System.Windows.Forms.CheckBox checkBoxComplexTreeRepeatCode;
        private System.Windows.Forms.GroupBox groupHuffmanTrees;
        private System.Windows.Forms.GroupBox groupContextMaps;
        private System.Windows.Forms.CheckBox checkBoxContextMapIMTF;
        private System.Windows.Forms.CheckBox checkBoxContextMapRLE;
        private System.Windows.Forms.Button buttonReserialize;
    }
}
