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
            this.groupContextMapsGeneral = new System.Windows.Forms.GroupBox();
            this.checkBoxContextMapMTF = new System.Windows.Forms.CheckBox();
            this.buttonReserialize = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupContextMapsRLE = new System.Windows.Forms.GroupBox();
            this.radioContextMapsRleSplit1AB = new System.Windows.Forms.RadioButton();
            this.radioContextMapsRleKeepAll = new System.Windows.Forms.RadioButton();
            this.radioContextMapsRleDisable = new System.Windows.Forms.RadioButton();
            this.panelRight = new System.Windows.Forms.Panel();
            this.groupContextMapsGeneral.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.groupContextMapsRLE.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupContextMapsGeneral
            // 
            this.groupContextMapsGeneral.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupContextMapsGeneral.Controls.Add(this.checkBoxContextMapMTF);
            this.groupContextMapsGeneral.Location = new System.Drawing.Point(3, 3);
            this.groupContextMapsGeneral.Name = "groupContextMapsGeneral";
            this.groupContextMapsGeneral.Size = new System.Drawing.Size(254, 51);
            this.groupContextMapsGeneral.TabIndex = 0;
            this.groupContextMapsGeneral.TabStop = false;
            this.groupContextMapsGeneral.Text = "Context Maps — General";
            // 
            // checkBoxContextMapMTF
            // 
            this.checkBoxContextMapMTF.AutoSize = true;
            this.checkBoxContextMapMTF.Checked = true;
            this.checkBoxContextMapMTF.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxContextMapMTF.Location = new System.Drawing.Point(12, 22);
            this.checkBoxContextMapMTF.Margin = new System.Windows.Forms.Padding(9, 3, 3, 3);
            this.checkBoxContextMapMTF.Name = "checkBoxContextMapMTF";
            this.checkBoxContextMapMTF.Size = new System.Drawing.Size(166, 19);
            this.checkBoxContextMapMTF.TabIndex = 0;
            this.checkBoxContextMapMTF.Text = "Move-To-Front Transform";
            this.checkBoxContextMapMTF.UseVisualStyleBackColor = true;
            // 
            // buttonReserialize
            // 
            this.buttonReserialize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonReserialize.Location = new System.Drawing.Point(439, 177);
            this.buttonReserialize.Name = "buttonReserialize";
            this.buttonReserialize.Size = new System.Drawing.Size(93, 23);
            this.buttonReserialize.TabIndex = 1;
            this.buttonReserialize.Text = "Reserialize";
            this.buttonReserialize.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Controls.Add(this.panelRight, 1, 0);
            this.tableLayoutPanel.Location = new System.Drawing.Point(12, 12);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 1;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(520, 159);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // groupContextMapsRLE
            // 
            this.groupContextMapsRLE.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupContextMapsRLE.Controls.Add(this.radioContextMapsRleSplit1AB);
            this.groupContextMapsRLE.Controls.Add(this.radioContextMapsRleKeepAll);
            this.groupContextMapsRLE.Controls.Add(this.radioContextMapsRleDisable);
            this.groupContextMapsRLE.Location = new System.Drawing.Point(3, 60);
            this.groupContextMapsRLE.Name = "groupContextMapsRLE";
            this.groupContextMapsRLE.Size = new System.Drawing.Size(254, 96);
            this.groupContextMapsRLE.TabIndex = 1;
            this.groupContextMapsRLE.TabStop = false;
            this.groupContextMapsRLE.Text = "Context Maps — Run-Length Encoding";
            // 
            // radioContextMapsRleSplit1AB
            // 
            this.radioContextMapsRleSplit1AB.AutoSize = true;
            this.radioContextMapsRleSplit1AB.Location = new System.Drawing.Point(12, 67);
            this.radioContextMapsRleSplit1AB.Margin = new System.Windows.Forms.Padding(9, 2, 3, 2);
            this.radioContextMapsRleSplit1AB.Name = "radioContextMapsRleSplit1AB";
            this.radioContextMapsRleSplit1AB.Size = new System.Drawing.Size(164, 19);
            this.radioContextMapsRleSplit1AB.TabIndex = 2;
            this.radioContextMapsRleSplit1AB.TabStop = true;
            this.radioContextMapsRleSplit1AB.Text = "Split One Above Boundary";
            this.radioContextMapsRleSplit1AB.UseVisualStyleBackColor = true;
            // 
            // radioContextMapsRleKeepAll
            // 
            this.radioContextMapsRleKeepAll.AutoSize = true;
            this.radioContextMapsRleKeepAll.Location = new System.Drawing.Point(12, 44);
            this.radioContextMapsRleKeepAll.Margin = new System.Windows.Forms.Padding(9, 2, 3, 2);
            this.radioContextMapsRleKeepAll.Name = "radioContextMapsRleKeepAll";
            this.radioContextMapsRleKeepAll.Size = new System.Drawing.Size(68, 19);
            this.radioContextMapsRleKeepAll.TabIndex = 1;
            this.radioContextMapsRleKeepAll.TabStop = true;
            this.radioContextMapsRleKeepAll.Text = "Keep All";
            this.radioContextMapsRleKeepAll.UseVisualStyleBackColor = true;
            // 
            // radioContextMapsRleDisable
            // 
            this.radioContextMapsRleDisable.AutoSize = true;
            this.radioContextMapsRleDisable.Location = new System.Drawing.Point(12, 21);
            this.radioContextMapsRleDisable.Margin = new System.Windows.Forms.Padding(9, 2, 3, 2);
            this.radioContextMapsRleDisable.Name = "radioContextMapsRleDisable";
            this.radioContextMapsRleDisable.Size = new System.Drawing.Size(63, 19);
            this.radioContextMapsRleDisable.TabIndex = 0;
            this.radioContextMapsRleDisable.TabStop = true;
            this.radioContextMapsRleDisable.Text = "Disable";
            this.radioContextMapsRleDisable.UseVisualStyleBackColor = true;
            // 
            // panelRight
            // 
            this.panelRight.Controls.Add(this.groupContextMapsRLE);
            this.panelRight.Controls.Add(this.groupContextMapsGeneral);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(260, 0);
            this.panelRight.Margin = new System.Windows.Forms.Padding(0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(260, 159);
            this.panelRight.TabIndex = 1;
            // 
            // FormSerializationParameters
            // 
            this.ClientSize = new System.Drawing.Size(544, 212);
            this.Controls.Add(this.buttonReserialize);
            this.Controls.Add(this.tableLayoutPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "FormSerializationParameters";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Serialization Parameters";
            this.groupContextMapsGeneral.ResumeLayout(false);
            this.groupContextMapsGeneral.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.groupContextMapsRLE.ResumeLayout(false);
            this.groupContextMapsRLE.PerformLayout();
            this.panelRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox groupContextMapsGeneral;
        private System.Windows.Forms.CheckBox checkBoxContextMapMTF;
        private System.Windows.Forms.Button buttonReserialize;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.GroupBox groupContextMapsRLE;
        private System.Windows.Forms.RadioButton radioContextMapsRleSplit1AB;
        private System.Windows.Forms.RadioButton radioContextMapsRleKeepAll;
        private System.Windows.Forms.RadioButton radioContextMapsRleDisable;
        private System.Windows.Forms.Panel panelRight;
    }
}
