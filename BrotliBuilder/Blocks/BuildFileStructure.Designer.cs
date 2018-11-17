namespace BrotliBuilder.Blocks {
    partial class BuildFileStructure {
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
            this.buttonAddMetaBlockUncompressed = new System.Windows.Forms.Button();
            this.buttonAddMetaBlockCompressed = new System.Windows.Forms.Button();
            this.buttonDeleteMetaBlock = new System.Windows.Forms.Button();
            this.buttonAddMetaBlockEmpty = new System.Windows.Forms.Button();
            this.buttonMoveMetaBlockUp = new System.Windows.Forms.Button();
            this.buttonMoveMetaBlockDown = new System.Windows.Forms.Button();
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
            this.listElements.Size = new System.Drawing.Size(234, 307);
            this.listElements.TabIndex = 0;
            this.listElements.SelectedValueChanged += new System.EventHandler(this.listElements_SelectedValueChanged);
            // 
            // buttonAddMetaBlockUncompressed
            // 
            this.buttonAddMetaBlockUncompressed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddMetaBlockUncompressed.Location = new System.Drawing.Point(3, 345);
            this.buttonAddMetaBlockUncompressed.Name = "buttonAddMetaBlockUncompressed";
            this.buttonAddMetaBlockUncompressed.Size = new System.Drawing.Size(199, 23);
            this.buttonAddMetaBlockUncompressed.TabIndex = 2;
            this.buttonAddMetaBlockUncompressed.Text = "Add Meta-Block (Uncompressed)";
            this.buttonAddMetaBlockUncompressed.UseVisualStyleBackColor = true;
            this.buttonAddMetaBlockUncompressed.Click += new System.EventHandler(this.buttonAddMetaBlockUncompressed_Click);
            // 
            // buttonAddMetaBlockCompressed
            // 
            this.buttonAddMetaBlockCompressed.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddMetaBlockCompressed.Location = new System.Drawing.Point(3, 316);
            this.buttonAddMetaBlockCompressed.Name = "buttonAddMetaBlockCompressed";
            this.buttonAddMetaBlockCompressed.Size = new System.Drawing.Size(199, 23);
            this.buttonAddMetaBlockCompressed.TabIndex = 1;
            this.buttonAddMetaBlockCompressed.Text = "Add Meta-Block (Compressed)";
            this.buttonAddMetaBlockCompressed.UseVisualStyleBackColor = true;
            this.buttonAddMetaBlockCompressed.Click += new System.EventHandler(this.buttonAddMetaBlockCompressed_Click);
            // 
            // buttonDeleteMetaBlock
            // 
            this.buttonDeleteMetaBlock.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDeleteMetaBlock.Enabled = false;
            this.buttonDeleteMetaBlock.Location = new System.Drawing.Point(208, 374);
            this.buttonDeleteMetaBlock.Name = "buttonDeleteMetaBlock";
            this.buttonDeleteMetaBlock.Size = new System.Drawing.Size(29, 23);
            this.buttonDeleteMetaBlock.TabIndex = 6;
            this.buttonDeleteMetaBlock.Text = "✖";
            this.buttonDeleteMetaBlock.UseVisualStyleBackColor = true;
            this.buttonDeleteMetaBlock.Click += new System.EventHandler(this.buttonDeleteMetaBlock_Click);
            // 
            // buttonAddMetaBlockEmpty
            // 
            this.buttonAddMetaBlockEmpty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAddMetaBlockEmpty.Location = new System.Drawing.Point(3, 374);
            this.buttonAddMetaBlockEmpty.Name = "buttonAddMetaBlockEmpty";
            this.buttonAddMetaBlockEmpty.Size = new System.Drawing.Size(199, 23);
            this.buttonAddMetaBlockEmpty.TabIndex = 3;
            this.buttonAddMetaBlockEmpty.Text = "Add Meta-Block (Empty)";
            this.buttonAddMetaBlockEmpty.UseVisualStyleBackColor = true;
            this.buttonAddMetaBlockEmpty.Click += new System.EventHandler(this.buttonAddMetaBlockEmpty_Click);
            // 
            // buttonMoveMetaBlockUp
            // 
            this.buttonMoveMetaBlockUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveMetaBlockUp.Enabled = false;
            this.buttonMoveMetaBlockUp.Location = new System.Drawing.Point(208, 316);
            this.buttonMoveMetaBlockUp.Name = "buttonMoveMetaBlockUp";
            this.buttonMoveMetaBlockUp.Size = new System.Drawing.Size(29, 23);
            this.buttonMoveMetaBlockUp.TabIndex = 4;
            this.buttonMoveMetaBlockUp.Text = "▲";
            this.buttonMoveMetaBlockUp.UseVisualStyleBackColor = true;
            this.buttonMoveMetaBlockUp.Click += new System.EventHandler(this.buttonMoveMetaBlockUp_Click);
            // 
            // buttonMoveMetaBlockDown
            // 
            this.buttonMoveMetaBlockDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonMoveMetaBlockDown.Enabled = false;
            this.buttonMoveMetaBlockDown.Location = new System.Drawing.Point(208, 345);
            this.buttonMoveMetaBlockDown.Name = "buttonMoveMetaBlockDown";
            this.buttonMoveMetaBlockDown.Size = new System.Drawing.Size(29, 23);
            this.buttonMoveMetaBlockDown.TabIndex = 5;
            this.buttonMoveMetaBlockDown.Text = "▼";
            this.buttonMoveMetaBlockDown.UseVisualStyleBackColor = true;
            this.buttonMoveMetaBlockDown.Click += new System.EventHandler(this.buttonMoveMetaBlockDown_Click);
            // 
            // BuildFileStructure
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonMoveMetaBlockDown);
            this.Controls.Add(this.buttonMoveMetaBlockUp);
            this.Controls.Add(this.buttonAddMetaBlockEmpty);
            this.Controls.Add(this.buttonDeleteMetaBlock);
            this.Controls.Add(this.buttonAddMetaBlockCompressed);
            this.Controls.Add(this.buttonAddMetaBlockUncompressed);
            this.Controls.Add(this.listElements);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "BuildFileStructure";
            this.Size = new System.Drawing.Size(240, 400);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox listElements;
        private System.Windows.Forms.Button buttonAddMetaBlockUncompressed;
        private System.Windows.Forms.Button buttonAddMetaBlockCompressed;
        private System.Windows.Forms.Button buttonDeleteMetaBlock;
        private System.Windows.Forms.Button buttonAddMetaBlockEmpty;
        private System.Windows.Forms.Button buttonMoveMetaBlockUp;
        private System.Windows.Forms.Button buttonMoveMetaBlockDown;
    }
}
