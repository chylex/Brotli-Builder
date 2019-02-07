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
            this.menuItemWrapOutput = new System.Windows.Forms.MenuItem();
            this.menuItemLimitOutput = new System.Windows.Forms.MenuItem();
            this.menuItemTools = new System.Windows.Forms.MenuItem();
            this.menuItemStaticDictionary = new System.Windows.Forms.MenuItem();
            this.menuItemEncodeFile = new System.Windows.Forms.MenuItem();
            this.menuItemEncodeUncompressedMBs = new System.Windows.Forms.MenuItem();
            this.flowPanelBlocks = new System.Windows.Forms.FlowLayoutPanel();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.statusBarPanelPadding1 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeBits = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelPadding2 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeOutput = new System.Windows.Forms.StatusBarPanel();
            this.timerRegenerationDelay = new System.Windows.Forms.Timer(this.components);
            this.splitContainerOuter = new System.Windows.Forms.SplitContainer();
            this.brotliFilePanelGenerated = new BrotliBuilder.Components.BrotliFilePanel();
            this.brotliFilePanelOriginal = new BrotliBuilder.Components.BrotliFilePanel();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).BeginInit();
            this.splitContainerOuter.Panel1.SuspendLayout();
            this.splitContainerOuter.Panel2.SuspendLayout();
            this.splitContainerOuter.SuspendLayout();
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
            this.menuItemWrapOutput,
            this.menuItemLimitOutput});
            this.menuItemView.Text = "&View";
            // 
            // menuItemWrapOutput
            // 
            this.menuItemWrapOutput.Index = 0;
            this.menuItemWrapOutput.Text = "Wrap Output";
            this.menuItemWrapOutput.Click += new System.EventHandler(this.menuItemWrapOutput_Click);
            // 
            // menuItemLimitOutput
            // 
            this.menuItemLimitOutput.Index = 1;
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
            this.splitContainerOuter.Panel1.Controls.Add(this.brotliFilePanelGenerated);
            // 
            // splitContainerOuter.Panel2
            // 
            this.splitContainerOuter.Panel2.Controls.Add(this.brotliFilePanelOriginal);
            this.splitContainerOuter.Size = new System.Drawing.Size(1008, 252);
            this.splitContainerOuter.SplitterDistance = 502;
            this.splitContainerOuter.TabIndex = 1;
            // 
            // brotliFilePanelGenerated
            // 
            this.brotliFilePanelGenerated.Dock = System.Windows.Forms.DockStyle.Fill;
            this.brotliFilePanelGenerated.LabelPrefix = "Generated";
            this.brotliFilePanelGenerated.Location = new System.Drawing.Point(0, 0);
            this.brotliFilePanelGenerated.Name = "brotliFilePanelGenerated";
            this.brotliFilePanelGenerated.Size = new System.Drawing.Size(502, 252);
            this.brotliFilePanelGenerated.TabIndex = 0;
            // 
            // brotliFilePanelOriginal
            // 
            this.brotliFilePanelOriginal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.brotliFilePanelOriginal.LabelPrefix = "Original";
            this.brotliFilePanelOriginal.Location = new System.Drawing.Point(0, 0);
            this.brotliFilePanelOriginal.Name = "brotliFilePanelOriginal";
            this.brotliFilePanelOriginal.Size = new System.Drawing.Size(502, 252);
            this.brotliFilePanelOriginal.TabIndex = 0;
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
            this.splitContainerOuter.Panel1.ResumeLayout(false);
            this.splitContainerOuter.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerOuter)).EndInit();
            this.splitContainerOuter.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem menuItemFile;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBlocks;
        private System.Windows.Forms.MenuItem menuItemSave;
        private System.Windows.Forms.MenuItem menuItemOpen;
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
        private System.Windows.Forms.SplitContainer splitContainerOuter;
        private System.Windows.Forms.MenuItem menuItemWrapOutput;
        private Components.BrotliFilePanel brotliFilePanelGenerated;
        private Components.BrotliFilePanel brotliFilePanelOriginal;
    }
}