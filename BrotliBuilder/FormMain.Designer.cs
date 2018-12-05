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
            this.textBoxBitStream = new System.Windows.Forms.RichTextBox();
            this.labelBitStream = new System.Windows.Forms.Label();
            this.labelDecompressedOutput = new System.Windows.Forms.Label();
            this.textBoxDecompressedOutput = new System.Windows.Forms.RichTextBox();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.statusBarPanelPadding1 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeBits = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelPadding2 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeOutput = new System.Windows.Forms.StatusBarPanel();
            this.timerRegenerationDelay = new System.Windows.Forms.Timer(this.components);
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).BeginInit();
            this.splitContainerMain.Panel1.SuspendLayout();
            this.splitContainerMain.Panel2.SuspendLayout();
            this.splitContainerMain.SuspendLayout();
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
            // textBoxBitStream
            // 
            this.textBoxBitStream.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxBitStream.DetectUrls = false;
            this.textBoxBitStream.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxBitStream.Location = new System.Drawing.Point(12, 18);
            this.textBoxBitStream.Name = "textBoxBitStream";
            this.textBoxBitStream.ReadOnly = true;
            this.textBoxBitStream.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxBitStream.Size = new System.Drawing.Size(984, 98);
            this.textBoxBitStream.TabIndex = 1;
            this.textBoxBitStream.Text = "";
            // 
            // labelBitStream
            // 
            this.labelBitStream.AutoSize = true;
            this.labelBitStream.Location = new System.Drawing.Point(9, 0);
            this.labelBitStream.Name = "labelBitStream";
            this.labelBitStream.Size = new System.Drawing.Size(61, 15);
            this.labelBitStream.TabIndex = 0;
            this.labelBitStream.Text = "Bit Stream";
            // 
            // labelDecompressedOutput
            // 
            this.labelDecompressedOutput.AutoSize = true;
            this.labelDecompressedOutput.Location = new System.Drawing.Point(9, 0);
            this.labelDecompressedOutput.Name = "labelDecompressedOutput";
            this.labelDecompressedOutput.Size = new System.Drawing.Size(126, 15);
            this.labelDecompressedOutput.TabIndex = 0;
            this.labelDecompressedOutput.Text = "Decompressed Output";
            // 
            // textBoxDecompressedOutput
            // 
            this.textBoxDecompressedOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDecompressedOutput.DetectUrls = false;
            this.textBoxDecompressedOutput.Font = new System.Drawing.Font("Consolas", 9.75F);
            this.textBoxDecompressedOutput.Location = new System.Drawing.Point(12, 18);
            this.textBoxDecompressedOutput.Name = "textBoxDecompressedOutput";
            this.textBoxDecompressedOutput.ReadOnly = true;
            this.textBoxDecompressedOutput.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.textBoxDecompressedOutput.Size = new System.Drawing.Size(984, 98);
            this.textBoxDecompressedOutput.TabIndex = 1;
            this.textBoxDecompressedOutput.Text = "";
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
            // splitContainerMain
            // 
            this.splitContainerMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerMain.IsSplitterFixed = true;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 435);
            this.splitContainerMain.Name = "splitContainerMain";
            this.splitContainerMain.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.labelBitStream);
            this.splitContainerMain.Panel1.Controls.Add(this.textBoxBitStream);
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.textBoxDecompressedOutput);
            this.splitContainerMain.Panel2.Controls.Add(this.labelDecompressedOutput);
            this.splitContainerMain.Size = new System.Drawing.Size(1008, 246);
            this.splitContainerMain.SplitterDistance = 119;
            this.splitContainerMain.SplitterWidth = 8;
            this.splitContainerMain.TabIndex = 1;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(1008, 709);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.flowPanelBlocks);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Menu = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(480, 640);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Brotli Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).EndInit();
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel1.PerformLayout();
            this.splitContainerMain.Panel2.ResumeLayout(false);
            this.splitContainerMain.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem menuItemFile;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBlocks;
        private System.Windows.Forms.RichTextBox textBoxBitStream;
        private System.Windows.Forms.MenuItem menuItemSave;
        private System.Windows.Forms.Label labelBitStream;
        private System.Windows.Forms.MenuItem menuItemOpen;
        private System.Windows.Forms.Label labelDecompressedOutput;
        private System.Windows.Forms.RichTextBox textBoxDecompressedOutput;
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
        private System.Windows.Forms.SplitContainer splitContainerMain;
    }
}