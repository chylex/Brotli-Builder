namespace BrotliBuilder.Dialogs {
    partial class FormStaticDictionary {
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.dataGridViewWords = new System.Windows.Forms.DataGridView();
            this.columnLength = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnIndex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnTransform = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.columnText = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.progressBarLoading = new System.Windows.Forms.ProgressBar();
            this.backgroundWorkerLoading = new System.ComponentModel.BackgroundWorker();
            this.textBoxFilter = new System.Windows.Forms.TextBox();
            this.labelFilter = new System.Windows.Forms.Label();
            this.labelWordCount = new System.Windows.Forms.Label();
            this.labelWordCountValue = new System.Windows.Forms.Label();
            this.checkBoxShowTransforms = new System.Windows.Forms.CheckBox();
            this.labelDuplicateCount = new System.Windows.Forms.Label();
            this.labelDuplicateCountValue = new System.Windows.Forms.Label();
            this.timerFilterUpdate = new System.Windows.Forms.Timer(this.components);
            this.panelFilters = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWords)).BeginInit();
            this.panelFilters.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridViewWords
            // 
            this.dataGridViewWords.AllowUserToAddRows = false;
            this.dataGridViewWords.AllowUserToDeleteRows = false;
            this.dataGridViewWords.AllowUserToResizeColumns = false;
            this.dataGridViewWords.AllowUserToResizeRows = false;
            this.dataGridViewWords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridViewWords.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewWords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridViewWords.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewWords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.columnLength,
            this.columnIndex,
            this.columnTransform,
            this.columnText});
            this.dataGridViewWords.Enabled = false;
            this.dataGridViewWords.Location = new System.Drawing.Point(0, 65);
            this.dataGridViewWords.Margin = new System.Windows.Forms.Padding(3, 12, 3, 3);
            this.dataGridViewWords.Name = "dataGridViewWords";
            this.dataGridViewWords.ReadOnly = true;
            this.dataGridViewWords.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewWords.Size = new System.Drawing.Size(784, 496);
            this.dataGridViewWords.TabIndex = 6;
            // 
            // columnLength
            // 
            this.columnLength.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnLength.DefaultCellStyle = dataGridViewCellStyle1;
            this.columnLength.HeaderText = "Length";
            this.columnLength.Name = "columnLength";
            this.columnLength.ReadOnly = true;
            this.columnLength.Width = 69;
            // 
            // columnIndex
            // 
            this.columnIndex.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnIndex.DefaultCellStyle = dataGridViewCellStyle2;
            this.columnIndex.HeaderText = "Index";
            this.columnIndex.Name = "columnIndex";
            this.columnIndex.ReadOnly = true;
            this.columnIndex.Width = 60;
            // 
            // columnTransform
            // 
            this.columnTransform.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.columnTransform.DefaultCellStyle = dataGridViewCellStyle3;
            this.columnTransform.HeaderText = "Transform";
            this.columnTransform.Name = "columnTransform";
            this.columnTransform.ReadOnly = true;
            this.columnTransform.Width = 86;
            // 
            // columnText
            // 
            this.columnText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.columnText.HeaderText = "Text";
            this.columnText.Name = "columnText";
            this.columnText.ReadOnly = true;
            // 
            // progressBarLoading
            // 
            this.progressBarLoading.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarLoading.Location = new System.Drawing.Point(73, 313);
            this.progressBarLoading.Margin = new System.Windows.Forms.Padding(64, 3, 64, 3);
            this.progressBarLoading.Name = "progressBarLoading";
            this.progressBarLoading.Size = new System.Drawing.Size(638, 23);
            this.progressBarLoading.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarLoading.TabIndex = 0;
            // 
            // backgroundWorkerLoading
            // 
            this.backgroundWorkerLoading.WorkerReportsProgress = true;
            this.backgroundWorkerLoading.WorkerSupportsCancellation = true;
            this.backgroundWorkerLoading.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerLoading_DoWork);
            this.backgroundWorkerLoading.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorkerLoading_ProgressChanged);
            this.backgroundWorkerLoading.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorkerLoading_RunWorkerCompleted);
            // 
            // textBoxFilter
            // 
            this.textBoxFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxFilter.Enabled = false;
            this.textBoxFilter.Location = new System.Drawing.Point(15, 27);
            this.textBoxFilter.Margin = new System.Windows.Forms.Padding(3, 3, 12, 3);
            this.textBoxFilter.Name = "textBoxFilter";
            this.textBoxFilter.Size = new System.Drawing.Size(282, 23);
            this.textBoxFilter.TabIndex = 1;
            this.textBoxFilter.TextChanged += new System.EventHandler(this.textBoxFilter_TextChanged);
            // 
            // labelFilter
            // 
            this.labelFilter.AutoSize = true;
            this.labelFilter.Location = new System.Drawing.Point(12, 9);
            this.labelFilter.Name = "labelFilter";
            this.labelFilter.Size = new System.Drawing.Size(33, 15);
            this.labelFilter.TabIndex = 0;
            this.labelFilter.Text = "Filter";
            // 
            // labelWordCount
            // 
            this.labelWordCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWordCount.AutoSize = true;
            this.labelWordCount.Location = new System.Drawing.Point(629, 12);
            this.labelWordCount.Margin = new System.Windows.Forms.Padding(3);
            this.labelWordCount.Name = "labelWordCount";
            this.labelWordCount.Size = new System.Drawing.Size(80, 15);
            this.labelWordCount.TabIndex = 2;
            this.labelWordCount.Text = "Shown Words";
            // 
            // labelWordCountValue
            // 
            this.labelWordCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelWordCountValue.AutoEllipsis = true;
            this.labelWordCountValue.Location = new System.Drawing.Point(712, 12);
            this.labelWordCountValue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.labelWordCountValue.Name = "labelWordCountValue";
            this.labelWordCountValue.Size = new System.Drawing.Size(60, 15);
            this.labelWordCountValue.TabIndex = 3;
            this.labelWordCountValue.Text = "0";
            this.labelWordCountValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // checkBoxShowTransforms
            // 
            this.checkBoxShowTransforms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBoxShowTransforms.AutoSize = true;
            this.checkBoxShowTransforms.Checked = true;
            this.checkBoxShowTransforms.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowTransforms.Enabled = false;
            this.checkBoxShowTransforms.Location = new System.Drawing.Point(312, 29);
            this.checkBoxShowTransforms.Name = "checkBoxShowTransforms";
            this.checkBoxShowTransforms.Size = new System.Drawing.Size(85, 19);
            this.checkBoxShowTransforms.TabIndex = 2;
            this.checkBoxShowTransforms.Text = "Transforms";
            this.checkBoxShowTransforms.UseVisualStyleBackColor = true;
            this.checkBoxShowTransforms.CheckedChanged += new System.EventHandler(this.checkBoxShowTransforms_CheckedChanged);
            // 
            // labelDuplicateCount
            // 
            this.labelDuplicateCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDuplicateCount.AutoSize = true;
            this.labelDuplicateCount.Location = new System.Drawing.Point(608, 33);
            this.labelDuplicateCount.Margin = new System.Windows.Forms.Padding(3);
            this.labelDuplicateCount.Name = "labelDuplicateCount";
            this.labelDuplicateCount.Size = new System.Drawing.Size(101, 15);
            this.labelDuplicateCount.TabIndex = 4;
            this.labelDuplicateCount.Text = "Shown Duplicates";
            // 
            // labelDuplicateCountValue
            // 
            this.labelDuplicateCountValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDuplicateCountValue.AutoEllipsis = true;
            this.labelDuplicateCountValue.Location = new System.Drawing.Point(712, 33);
            this.labelDuplicateCountValue.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.labelDuplicateCountValue.Name = "labelDuplicateCountValue";
            this.labelDuplicateCountValue.Size = new System.Drawing.Size(60, 15);
            this.labelDuplicateCountValue.TabIndex = 5;
            this.labelDuplicateCountValue.Text = "(...)";
            this.labelDuplicateCountValue.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // timerFilterUpdate
            // 
            this.timerFilterUpdate.Interval = 750;
            this.timerFilterUpdate.Tick += new System.EventHandler(this.timerFilterUpdate_Tick);
            // 
            // panelFilters
            // 
            this.panelFilters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelFilters.Controls.Add(this.textBoxFilter);
            this.panelFilters.Controls.Add(this.labelFilter);
            this.panelFilters.Controls.Add(this.checkBoxShowTransforms);
            this.panelFilters.Location = new System.Drawing.Point(0, 0);
            this.panelFilters.Margin = new System.Windows.Forms.Padding(0);
            this.panelFilters.MinimumSize = new System.Drawing.Size(180, 0);
            this.panelFilters.Name = "panelFilters";
            this.panelFilters.Padding = new System.Windows.Forms.Padding(12, 9, 0, 0);
            this.panelFilters.Size = new System.Drawing.Size(400, 60);
            this.panelFilters.TabIndex = 1;
            // 
            // FormStaticDictionary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.Controls.Add(this.panelFilters);
            this.Controls.Add(this.labelDuplicateCountValue);
            this.Controls.Add(this.labelDuplicateCount);
            this.Controls.Add(this.labelWordCountValue);
            this.Controls.Add(this.labelWordCount);
            this.Controls.Add(this.progressBarLoading);
            this.Controls.Add(this.dataGridViewWords);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(380, 240);
            this.Name = "FormStaticDictionary";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Static Dictionary";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormStaticDictionary_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewWords)).EndInit();
            this.panelFilters.ResumeLayout(false);
            this.panelFilters.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewWords;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnLength;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnIndex;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnTransform;
        private System.Windows.Forms.DataGridViewTextBoxColumn columnText;
        private System.Windows.Forms.ProgressBar progressBarLoading;
        private System.ComponentModel.BackgroundWorker backgroundWorkerLoading;
        private System.Windows.Forms.TextBox textBoxFilter;
        private System.Windows.Forms.Label labelFilter;
        private System.Windows.Forms.Label labelWordCount;
        private System.Windows.Forms.Label labelWordCountValue;
        private System.Windows.Forms.CheckBox checkBoxShowTransforms;
        private System.Windows.Forms.Label labelDuplicateCount;
        private System.Windows.Forms.Label labelDuplicateCountValue;
        private System.Windows.Forms.Timer timerFilterUpdate;
        private System.Windows.Forms.Panel panelFilters;
    }
}