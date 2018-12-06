namespace BrotliBuilder {
    partial class FormMain {
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
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItemFile = new System.Windows.Forms.MenuItem();
            this.menuItemOpen = new System.Windows.Forms.MenuItem();
            this.menuItemSave = new System.Windows.Forms.MenuItem();
            this.menuItemExit = new System.Windows.Forms.MenuItem();
            this.menuItemView = new System.Windows.Forms.MenuItem();
            this.menuItemLimitOutput = new System.Windows.Forms.MenuItem();
            this.menuItemTools = new System.Windows.Forms.MenuItem();
            this.menuItemStaticDictionary = new System.Windows.Forms.MenuItem();
            this.menuItemEncodeFile = new System.Windows.Forms.MenuItem();
            this.menuItemEncodeUncompressedMBs = new System.Windows.Forms.MenuItem();
            this.flowPanelBlocks = new System.Windows.Forms.FlowLayoutPanel();
            this.textBoxGenBitStream = new System.Windows.Forms.RichTextBox();
            this.labelGenBitStream = new System.Windows.Forms.Label();
            this.labelGenOutput = new System.Windows.Forms.Label();
            this.textBoxGenOutput = new System.Windows.Forms.RichTextBox();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.statusBarPanelPadding1 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeBits = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelPadding2 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeOutput = new System.Windows.Forms.StatusBarPanel();
            this.timerRegenerationDelay = new System.Windows.Forms.Timer(this.components);
            this.splitContainerLeft = new System.Windows.Forms.SplitContainer();
            this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
            this.splitContainerRight = new System.Windows.Forms.SplitContainer();
            this.labelOrigBitStream = new System.Windows.Forms.Label();
            this.textBoxOrigBitStream = new System.Windows.Forms.RichTextBox();
            this.textBoxOrigOutput = new System.Windows.Forms.RichTextBox();
            this.labelOrigOutput = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).BeginInit();
            this.splitContainerLeft.Panel1.SuspendLayout();
            this.splitContainerLeft.Panel2.SuspendLayout();
            this.splitContainerLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).BeginInit();
            this.splitContainerOuter.Panel1.SuspendLayout();
            this.splitContainerOuter.Panel2.SuspendLayout();
            this.splitContainerOuter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
            this.splitContainerRight.Panel1.SuspendLayout();
            this.splitContainerRight.Panel2.SuspendLayout();
            this.splitContainerRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemFile,
            this.menuItemView,
            this.menuItemTools});
            // 
            // menuItemFile
            // 
            this.menuItemFile.Index = 0;
            this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemOpen,
            this.menuItemSave,
            this.menuItemExit});
            this.menuItemFile.Text = "&File";
            // 
            // menuItemOpen
            // 
            this.menuItemOpen.Index = 0;
            this.menuItemOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuItemOpen.Text = "Open";
            this.menuItemOpen.Click += new System.EventHandler(this.menuItemOpen_Click);
            // 
            // menuItemSave
            // 
            this.menuItemSave.Index = 1;
            this.menuItemSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuItemSave.Text = "Save";
            this.menuItemSave.Click += new System.EventHandler(this.menuItemSave_Click);
            // 
            // menuItemExit
            // 
            this.menuItemExit.Index = 2;
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // menuItemView
            // 
            this.menuItemView.Index = 1;
            this.menuItemView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemLimitOutput});
            this.menuItemView.Text = "&View";
            // 
            // menuItemLimitOutput
            // 
            this.menuItemLimitOutput.Checked = true;
            this.menuItemLimitOutput.Index = 0;
            this.menuItemLimitOutput.Text = "Limit Output Length";
            this.menuItemLimitOutput.Click += new System.EventHandler(this.menuItemLimitOutput_Click);
            // 
            // menuItemTools
            // 
            this.menuItemTools.Index = 2;
            this.menuItemTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemStaticDictionary,
            this.menuItemEncodeFile});
            this.menuItemTools.Text = "&Tools";
            // 
            // menuItemStaticDictionary
            // 
            this.menuItemStaticDictionary.Index = 0;
            this.menuItemStaticDictionary.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
            this.menuItemStaticDictionary.Text = "Static Dictionary";
            this.menuItemStaticDictionary.Click += new System.EventHandler(this.menuItemStaticDictionary_Click);
            // 
            // menuItemEncodeFile
            // 
            this.menuItemEncodeFile.Index = 1;
            this.menuItemEncodeFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemEncodeUncompressedMBs});
            this.menuItemEncodeFile.Text = "&Encode File...";
            // 
            // menuItemEncodeUncompressedMBs
            // 
            this.menuItemEncodeUncompressedMBs.Index = 0;
            this.menuItemEncodeUncompressedMBs.Shortcut = System.Windows.Forms.Shortcut.Ctrl0;
            this.menuItemEncodeUncompressedMBs.Text = "Into Uncompressed Meta-Blocks";
            this.menuItemEncodeUncompressedMBs.Click += new System.EventHandler(this.menuItemEncodeUncompressedMBs_Click);
            // 
            // flowPanelBlocks
            // 
            this.flowPanelBlocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowPanelBlocks.AutoScroll = true;
            this.flowPanelBlocks.BackColor = System.Drawing.SystemColors.Control;
            this.flowPanelBlocks.Location = new System.Drawing.Point(0, 0);
            this.flowPanelBlocks.Margin = new System.Windows.Forms.Padding(0, 0, 0, 12);
            this.flowPanelBlocks.Name = "flowPanelBlocks";
            this.flowPanelBlocks.Size = new System.Drawing.Size(1008, 424);
            this.flowPanelBlocks.TabIndex = 0;
            this.flowPanelBlocks.WrapContents = false;
            // 
            // textBoxGenBitStream
            // 
            this.textBoxGenBitStream.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGenBitStream.DetectUrls = false;
            this.textBoxGenBitStream.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxGenBitStream.Location = new System.Drawing.Point(12, 20);
            this.textBoxGenBitStream.Name = "textBoxGenBitStream";
            this.textBoxGenBitStream.ReadOnly = true;
            this.textBoxGenBitStream.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxGenBitStream.Size = new System.Drawing.Size(478, 101);
            this.textBoxGenBitStream.TabIndex = 1;
            this.textBoxGenBitStream.Text = "";
            // 
            // labelGenBitStream
            // 
            this.labelGenBitStream.AutoSize = true;
            this.labelGenBitStream.Location = new System.Drawing.Point(9, 2);
            this.labelGenBitStream.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelGenBitStream.Name = "labelGenBitStream";
            this.labelGenBitStream.Size = new System.Drawing.Size(118, 15);
            this.labelGenBitStream.TabIndex = 0;
            this.labelGenBitStream.Text = "Generated Bit Stream";
            // 
            // labelGenOutput
            // 
            this.labelGenOutput.AutoSize = true;
            this.labelGenOutput.Location = new System.Drawing.Point(9, 2);
            this.labelGenOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelGenOutput.Name = "labelGenOutput";
            this.labelGenOutput.Size = new System.Drawing.Size(102, 15);
            this.labelGenOutput.TabIndex = 0;
            this.labelGenOutput.Text = "Generated Output";
            // 
            // textBoxGenOutput
            // 
            this.textBoxGenOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxGenOutput.DetectUrls = false;
            this.textBoxGenOutput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxGenOutput.Location = new System.Drawing.Point(12, 20);
            this.textBoxGenOutput.Name = "textBoxGenOutput";
            this.textBoxGenOutput.ReadOnly = true;
            this.textBoxGenOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxGenOutput.Size = new System.Drawing.Size(478, 101);
            this.textBoxGenOutput.TabIndex = 1;
            this.textBoxGenOutput.Text = "";
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 687);
            this.statusBar.Name = "statusBar";
            this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanelPadding1,
            this.statusBarPanelTimeBits,
            this.statusBarPanelPadding2,
            this.statusBarPanelTimeOutput});
            this.statusBar.ShowPanels = true;
            this.statusBar.Size = new System.Drawing.Size(1008, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 2;
            // 
            // statusBarPanelPadding1
            // 
            this.statusBarPanelPadding1.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.statusBarPanelPadding1.MinWidth = 12;
            this.statusBarPanelPadding1.Name = "statusBarPanelPadding1";
            this.statusBarPanelPadding1.Width = 12;
            // 
            // statusBarPanelTimeBits
            // 
            this.statusBarPanelTimeBits.Name = "statusBarPanelTimeBits";
            this.statusBarPanelTimeBits.Width = 200;
            // 
            // statusBarPanelPadding2
            // 
            this.statusBarPanelPadding2.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.statusBarPanelPadding2.MinWidth = 9;
            this.statusBarPanelPadding2.Name = "statusBarPanelPadding2";
            this.statusBarPanelPadding2.Width = 9;
            // 
            // statusBarPanelTimeOutput
            // 
            this.statusBarPanelTimeOutput.Name = "statusBarPanelTimeOutput";
            this.statusBarPanelTimeOutput.Width = 200;
            // 
            // timerRegenerationDelay
            // 
            this.timerRegenerationDelay.Interval = 300;
            this.timerRegenerationDelay.Tick += new System.EventHandler(this.timerRegenerationDelay_Tick);
            // 
            // splitContainerLeft
            // 
            this.splitContainerLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerLeft.IsSplitterFixed = true;
            this.splitContainerLeft.Location = new System.Drawing.Point(0, 0);
            this.splitContainerLeft.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerLeft.Name = "splitContainerLeft";
            this.splitContainerLeft.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerLeft.Panel1
            // 
            this.splitContainerLeft.Panel1.Controls.Add(this.labelGenBitStream);
            this.splitContainerLeft.Panel1.Controls.Add(this.textBoxGenBitStream);
            // 
            // splitContainerLeft.Panel2
            // 
            this.splitContainerLeft.Panel2.Controls.Add(this.textBoxGenOutput);
            this.splitContainerLeft.Panel2.Controls.Add(this.labelGenOutput);
            this.splitContainerLeft.Size = new System.Drawing.Size(502, 252);
            this.splitContainerLeft.SplitterDistance = 124;
            this.splitContainerLeft.TabIndex = 0;
            // 
            // splitContainerOuter
            // 
            this.splitContainerOuter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerOuter.Location = new System.Drawing.Point(0, 429);
            this.splitContainerOuter.Name = "splitContainerOuter";
            // 
            // splitContainerOuter.Panel1
            // 
            this.splitContainerOuter.Panel1.Controls.Add(this.splitContainerLeft);
            // 
            // splitContainerOuter.Panel2
            // 
            this.splitContainerOuter.Panel2.Controls.Add(this.splitContainerRight);
            this.splitContainerOuter.Size = new System.Drawing.Size(1008, 252);
            this.splitContainerOuter.SplitterDistance = 502;
            this.splitContainerOuter.TabIndex = 1;
            // 
            // splitContainerRight
            // 
            this.splitContainerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerRight.IsSplitterFixed = true;
            this.splitContainerRight.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRight.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerRight.Name = "splitContainerRight";
            this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            this.splitContainerRight.Panel1.Controls.Add(this.labelOrigBitStream);
            this.splitContainerRight.Panel1.Controls.Add(this.textBoxOrigBitStream);
            // 
            // splitContainerRight.Panel2
            // 
            this.splitContainerRight.Panel2.Controls.Add(this.textBoxOrigOutput);
            this.splitContainerRight.Panel2.Controls.Add(this.labelOrigOutput);
            this.splitContainerRight.Size = new System.Drawing.Size(502, 252);
            this.splitContainerRight.SplitterDistance = 124;
            this.splitContainerRight.TabIndex = 0;
            // 
            // labelOrigBitStream
            // 
            this.labelOrigBitStream.AutoSize = true;
            this.labelOrigBitStream.Location = new System.Drawing.Point(9, 2);
            this.labelOrigBitStream.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelOrigBitStream.Name = "labelOrigBitStream";
            this.labelOrigBitStream.Size = new System.Drawing.Size(106, 15);
            this.labelOrigBitStream.TabIndex = 0;
            this.labelOrigBitStream.Text = "Original Bit Stream";
            // 
            // textBoxOrigBitStream
            // 
            this.textBoxOrigBitStream.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOrigBitStream.DetectUrls = false;
            this.textBoxOrigBitStream.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxOrigBitStream.Location = new System.Drawing.Point(12, 20);
            this.textBoxOrigBitStream.Name = "textBoxOrigBitStream";
            this.textBoxOrigBitStream.ReadOnly = true;
            this.textBoxOrigBitStream.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxOrigBitStream.Size = new System.Drawing.Size(478, 101);
            this.textBoxOrigBitStream.TabIndex = 1;
            this.textBoxOrigBitStream.Text = "";
            // 
            // textBoxOrigOutput
            // 
            this.textBoxOrigOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOrigOutput.DetectUrls = false;
            this.textBoxOrigOutput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxOrigOutput.Location = new System.Drawing.Point(12, 20);
            this.textBoxOrigOutput.Name = "textBoxOrigOutput";
            this.textBoxOrigOutput.ReadOnly = true;
            this.textBoxOrigOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxOrigOutput.Size = new System.Drawing.Size(478, 101);
            this.textBoxOrigOutput.TabIndex = 1;
            this.textBoxOrigOutput.Text = "";
            // 
            // labelOrigOutput
            // 
            this.labelOrigOutput.AutoSize = true;
            this.labelOrigOutput.Location = new System.Drawing.Point(9, 2);
            this.labelOrigOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 0);
            this.labelOrigOutput.Name = "labelOrigOutput";
            this.labelOrigOutput.Size = new System.Drawing.Size(90, 15);
            this.labelOrigOutput.TabIndex = 0;
            this.labelOrigOutput.Text = "Original Output";
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(1008, 709);
            this.Controls.Add(this.splitContainerOuter);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.flowPanelBlocks);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Menu = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(520, 640);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Brotli Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).EndInit();
            this.splitContainerLeft.Panel1.ResumeLayout(false);
            this.splitContainerLeft.Panel1.PerformLayout();
            this.splitContainerLeft.Panel2.ResumeLayout(false);
            this.splitContainerLeft.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerLeft)).EndInit();
            this.splitContainerLeft.ResumeLayout(false);
            this.splitContainerOuter.Panel1.ResumeLayout(false);
            this.splitContainerOuter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).EndInit();
            this.splitContainerOuter.ResumeLayout(false);
            this.splitContainerRight.Panel1.ResumeLayout(false);
            this.splitContainerRight.Panel1.PerformLayout();
            this.splitContainerRight.Panel2.ResumeLayout(false);
            this.splitContainerRight.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
            this.splitContainerRight.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem menuItemFile;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBlocks;
        private System.Windows.Forms.RichTextBox textBoxGenBitStream;
        private System.Windows.Forms.MenuItem menuItemSave;
        private System.Windows.Forms.Label labelGenBitStream;
        private System.Windows.Forms.MenuItem menuItemOpen;
        private System.Windows.Forms.Label labelGenOutput;
        private System.Windows.Forms.RichTextBox textBoxGenOutput;
        private System.Windows.Forms.StatusBar statusBar;
        private System.Windows.Forms.StatusBarPanel statusBarPanelTimeBits;
        private System.Windows.Forms.StatusBarPanel statusBarPanelTimeOutput;
        private System.Windows.Forms.StatusBarPanel statusBarPanelPadding1;
        private System.Windows.Forms.StatusBarPanel statusBarPanelPadding2;
        private System.Windows.Forms.Timer timerRegenerationDelay;
        private System.Windows.Forms.MenuItem menuItemTools;
        private System.Windows.Forms.MenuItem menuItemEncodeFile;
        private System.Windows.Forms.MenuItem menuItemEncodeUncompressedMBs;
        private System.Windows.Forms.MenuItem menuItemView;
        private System.Windows.Forms.MenuItem menuItemLimitOutput;
        private System.Windows.Forms.MenuItem menuItemStaticDictionary;
        private System.Windows.Forms.SplitContainer splitContainerLeft;
        private System.Windows.Forms.SplitContainer splitContainerOuter;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private System.Windows.Forms.Label labelOrigBitStream;
        private System.Windows.Forms.RichTextBox textBoxOrigBitStream;
        private System.Windows.Forms.RichTextBox textBoxOrigOutput;
        private System.Windows.Forms.Label labelOrigOutput;
    }
}