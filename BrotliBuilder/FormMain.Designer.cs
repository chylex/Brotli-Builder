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
            this.menuItemOpenBrotli = new System.Windows.Forms.MenuItem();
            this.menuItemSaveBrotli = new System.Windows.Forms.MenuItem();
            this.menuItemSaveOutput = new System.Windows.Forms.MenuItem();
            this.menuItemExit = new System.Windows.Forms.MenuItem();
            this.menuItemView = new System.Windows.Forms.MenuItem();
            this.menuItemFileStructure = new System.Windows.Forms.MenuItem();
            this.menuItemMarkerInfo = new System.Windows.Forms.MenuItem();
            this.menuItemWrapOutput = new System.Windows.Forms.MenuItem();
            this.menuItemWrapMarkerInfo = new System.Windows.Forms.MenuItem();
            this.menuItemTools = new System.Windows.Forms.MenuItem();
            this.menuItemStaticDictionary = new System.Windows.Forms.MenuItem();
            this.menuItemCompareMarkers = new System.Windows.Forms.MenuItem();
            this.menuItemSeparatorT1 = new System.Windows.Forms.MenuItem();
            this.menuItemSeparatorT2 = new System.Windows.Forms.MenuItem();
            this.menuItemCloneGeneratedToOriginal = new System.Windows.Forms.MenuItem();
            this.menuItemCloneOriginalToGenerated = new System.Windows.Forms.MenuItem();
            this.menuItemEncodeFile = new System.Windows.Forms.MenuItem();
            this.menuItemTransform = new System.Windows.Forms.MenuItem();
            this.menuItemConfigureSerializationParameters = new System.Windows.Forms.MenuItem();
            this.flowPanelBlocks = new System.Windows.Forms.FlowLayoutPanel();
            this.statusBar = new System.Windows.Forms.StatusBar();
            this.statusBarPanelPadding1 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeStructure = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelPadding2 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeBits = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelPadding3 = new System.Windows.Forms.StatusBarPanel();
            this.statusBarPanelTimeOutput = new System.Windows.Forms.StatusBarPanel();
            this.timerRegenerationDelay = new System.Windows.Forms.Timer(this.components);
            this.splitContainerRightBottom = new System.Windows.Forms.SplitContainer();
            this.brotliFilePanelGenerated = new BrotliBuilder.Components.BrotliFilePanel();
            this.brotliFilePanelOriginal = new BrotliBuilder.Components.BrotliFilePanel();
            this.splitContainerRight = new System.Windows.Forms.SplitContainer();
            this.labelFileStructure = new System.Windows.Forms.Label();
            this.splitContainerMain = new System.Windows.Forms.SplitContainer();
            this.brotliMarkerInfoPanel = new BrotliBuilder.Components.BrotliMarkerInfoPanel();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeStructure)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRightBottom)).BeginInit();
            this.splitContainerRightBottom.Panel1.SuspendLayout();
            this.splitContainerRightBottom.Panel2.SuspendLayout();
            this.splitContainerRightBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).BeginInit();
            this.splitContainerRight.Panel1.SuspendLayout();
            this.splitContainerRight.Panel2.SuspendLayout();
            this.splitContainerRight.SuspendLayout();
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
            this.menuItemTools,
            this.menuItemEncodeFile,
            this.menuItemTransform});
            // 
            // menuItemFile
            // 
            this.menuItemFile.Index = 0;
            this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemOpenBrotli,
            this.menuItemSaveBrotli,
            this.menuItemSaveOutput,
            this.menuItemExit});
            this.menuItemFile.Text = "&File";
            // 
            // menuItemOpenBrotli
            // 
            this.menuItemOpenBrotli.Index = 0;
            this.menuItemOpenBrotli.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.menuItemOpenBrotli.Text = "Open Brotli";
            this.menuItemOpenBrotli.Click += new System.EventHandler(this.menuItemOpenBrotli_Click);
            // 
            // menuItemSaveBrotli
            // 
            this.menuItemSaveBrotli.Index = 1;
            this.menuItemSaveBrotli.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.menuItemSaveBrotli.Text = "Save Brotli";
            this.menuItemSaveBrotli.Click += new System.EventHandler(this.menuItemSaveBrotli_Click);
            // 
            // menuItemSaveOutput
            // 
            this.menuItemSaveOutput.Index = 2;
            this.menuItemSaveOutput.Text = "Save Output";
            this.menuItemSaveOutput.Click += new System.EventHandler(this.menuItemSaveOutput_Click);
            // 
            // menuItemExit
            // 
            this.menuItemExit.Index = 3;
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
            // 
            // menuItemView
            // 
            this.menuItemView.Index = 1;
            this.menuItemView.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemFileStructure,
            this.menuItemMarkerInfo,
            this.menuItemWrapOutput,
            this.menuItemWrapMarkerInfo});
            this.menuItemView.Text = "&View";
            // 
            // menuItemFileStructure
            // 
            this.menuItemFileStructure.Checked = true;
            this.menuItemFileStructure.Index = 0;
            this.menuItemFileStructure.Text = "File Structure Panel";
            this.menuItemFileStructure.Click += new System.EventHandler(this.menuItemFileStructure_Click);
            // 
            // menuItemMarkerInfo
            // 
            this.menuItemMarkerInfo.Checked = true;
            this.menuItemMarkerInfo.Index = 1;
            this.menuItemMarkerInfo.Text = "Marker Info Panel";
            this.menuItemMarkerInfo.Click += new System.EventHandler(this.menuItemMarkerInfo_Click);
            // 
            // menuItemWrapOutput
            // 
            this.menuItemWrapOutput.Index = 2;
            this.menuItemWrapOutput.Text = "Wrap Output";
            this.menuItemWrapOutput.Click += new System.EventHandler(this.menuItemWrapOutput_Click);
            // 
            // menuItemWrapMarkerInfo
            // 
            this.menuItemWrapMarkerInfo.Index = 3;
            this.menuItemWrapMarkerInfo.Text = "Wrap Marker Info";
            this.menuItemWrapMarkerInfo.Click += new System.EventHandler(this.menuItemWrapMarkerInfo_Click);
            // 
            // menuItemTools
            // 
            this.menuItemTools.Index = 2;
            this.menuItemTools.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItemConfigureSerializationParameters,
            this.menuItemSeparatorT1,
            this.menuItemStaticDictionary,
            this.menuItemCompareMarkers,
            this.menuItemSeparatorT2,
            this.menuItemCloneGeneratedToOriginal,
            this.menuItemCloneOriginalToGenerated});
            this.menuItemTools.Text = "&Tools";
            //
            // menuItemConfigureParameters
            //
            this.menuItemConfigureSerializationParameters.Index = 0;
            this.menuItemConfigureSerializationParameters.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.menuItemConfigureSerializationParameters.Text = "Configure Serialization Parameters";
            this.menuItemConfigureSerializationParameters.Click += new System.EventHandler(this.menuItemConfigureSerializationParameters_Click);
            // 
            // menuItemStaticDictionary
            // 
            this.menuItemStaticDictionary.Index = 2;
            this.menuItemStaticDictionary.Shortcut = System.Windows.Forms.Shortcut.CtrlD;
            this.menuItemStaticDictionary.Text = "Static Dictionary";
            this.menuItemStaticDictionary.Click += new System.EventHandler(this.menuItemStaticDictionary_Click);
            // 
            // menuItemCompareMarkers
            // 
            this.menuItemCompareMarkers.Enabled = false;
            this.menuItemCompareMarkers.Index = 3;
            this.menuItemCompareMarkers.Shortcut = System.Windows.Forms.Shortcut.CtrlM;
            this.menuItemCompareMarkers.Text = "Compare Markers";
            this.menuItemCompareMarkers.Click += new System.EventHandler(this.menuItemCompareMarkers_Click);
            // 
            // menuItemSeparatorT1
            // 
            this.menuItemSeparatorT1.Index = 1;
            this.menuItemSeparatorT1.Text = "-";
            // 
            // menuItemSeparatorT2
            // 
            this.menuItemSeparatorT2.Index = 4;
            this.menuItemSeparatorT2.Text = "-";
            // 
            // menuItemCloneGeneratedToOriginal
            // 
            this.menuItemCloneGeneratedToOriginal.Enabled = false;
            this.menuItemCloneGeneratedToOriginal.Index = 5;
            this.menuItemCloneGeneratedToOriginal.Text = "Generated >> Original";
            this.menuItemCloneGeneratedToOriginal.Click += new System.EventHandler(this.menuItemCloneGeneratedToOriginal_Click);
            // 
            // menuItemCloneOriginalToGenerated
            // 
            this.menuItemCloneOriginalToGenerated.Enabled = false;
            this.menuItemCloneOriginalToGenerated.Index = 6;
            this.menuItemCloneOriginalToGenerated.Text = "Generated << Original";
            this.menuItemCloneOriginalToGenerated.Click += new System.EventHandler(this.menuItemCloneOriginalToGenerated_Click);
            // 
            // menuItemEncodeFile
            // 
            this.menuItemEncodeFile.Index = 3;
            this.menuItemEncodeFile.Text = "&Encode";
            // 
            // menuItemTransform
            // 
            this.menuItemTransform.Index = 4;
            this.menuItemTransform.Text = "Transfor&m";
            // 
            // flowPanelBlocks
            // 
            this.flowPanelBlocks.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowPanelBlocks.BackColor = System.Drawing.SystemColors.Control;
            this.flowPanelBlocks.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowPanelBlocks.Location = new System.Drawing.Point(14, 28);
            this.flowPanelBlocks.Margin = new System.Windows.Forms.Padding(14, 6, 6, 3);
            this.flowPanelBlocks.Name = "flowPanelBlocks";
            this.flowPanelBlocks.Size = new System.Drawing.Size(855, 177);
            this.flowPanelBlocks.TabIndex = 1;
            this.flowPanelBlocks.WrapContents = false;
            this.flowPanelBlocks.SizeChanged += new System.EventHandler(this.flowPanelBlocks_SizeChanged);
            this.flowPanelBlocks.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.flowPanelBlocks_ControlAdded);
            // 
            // statusBar
            // 
            this.statusBar.Location = new System.Drawing.Point(0, 580);
            this.statusBar.Name = "statusBar";
            this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statusBarPanelPadding1,
            this.statusBarPanelTimeStructure,
            this.statusBarPanelPadding2,
            this.statusBarPanelTimeBits,
            this.statusBarPanelPadding3,
            this.statusBarPanelTimeOutput});
            this.statusBar.ShowPanels = true;
            this.statusBar.Size = new System.Drawing.Size(1264, 22);
            this.statusBar.SizingGrip = false;
            this.statusBar.TabIndex = 1;
            // 
            // statusBarPanelPadding1
            // 
            this.statusBarPanelPadding1.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.statusBarPanelPadding1.MinWidth = 12;
            this.statusBarPanelPadding1.Name = "statusBarPanelPadding1";
            this.statusBarPanelPadding1.Width = 12;
            // 
            // statusBarPanelTimeStructure
            // 
            this.statusBarPanelTimeStructure.Name = "statusBarPanelTimeStructure";
            this.statusBarPanelTimeStructure.Width = 200;
            // 
            // statusBarPanelPadding2
            // 
            this.statusBarPanelPadding2.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.statusBarPanelPadding2.MinWidth = 9;
            this.statusBarPanelPadding2.Name = "statusBarPanelPadding2";
            this.statusBarPanelPadding2.Width = 9;
            // 
            // statusBarPanelTimeBits
            // 
            this.statusBarPanelTimeBits.Name = "statusBarPanelTimeBits";
            this.statusBarPanelTimeBits.Width = 200;
            // 
            // statusBarPanelPadding3
            // 
            this.statusBarPanelPadding3.BorderStyle = System.Windows.Forms.StatusBarPanelBorderStyle.None;
            this.statusBarPanelPadding3.MinWidth = 9;
            this.statusBarPanelPadding3.Name = "statusBarPanelPadding3";
            this.statusBarPanelPadding3.Width = 9;
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
            // splitContainerRightBottom
            // 
            this.splitContainerRightBottom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerRightBottom.BackColor = System.Drawing.SystemColors.ActiveBorder;
            this.splitContainerRightBottom.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRightBottom.Margin = new System.Windows.Forms.Padding(0, 0, 0, 3);
            this.splitContainerRightBottom.Name = "splitContainerRightBottom";
            // 
            // splitContainerRightBottom.Panel1
            // 
            this.splitContainerRightBottom.Panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainerRightBottom.Panel1.Controls.Add(this.brotliFilePanelGenerated);
            // 
            // splitContainerRightBottom.Panel2
            // 
            this.splitContainerRightBottom.Panel2.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitContainerRightBottom.Panel2.Controls.Add(this.brotliFilePanelOriginal);
            this.splitContainerRightBottom.Size = new System.Drawing.Size(884, 375);
            this.splitContainerRightBottom.SplitterDistance = 441;
            this.splitContainerRightBottom.SplitterWidth = 2;
            this.splitContainerRightBottom.TabIndex = 0;
            // 
            // brotliFilePanelGenerated
            // 
            this.brotliFilePanelGenerated.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brotliFilePanelGenerated.Location = new System.Drawing.Point(0, 5);
            this.brotliFilePanelGenerated.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.brotliFilePanelGenerated.Name = "brotliFilePanelGenerated";
            this.brotliFilePanelGenerated.Size = new System.Drawing.Size(440, 365);
            this.brotliFilePanelGenerated.TabIndex = 0;
            this.brotliFilePanelGenerated.Title = "Generated";
            this.brotliFilePanelGenerated.MarkersUpdated += new System.EventHandler<BrotliBuilder.Components.MarkedTextBox.MarkerUpdateEventArgs>(this.brotliFilePanel_MarkersUpdated);
            // 
            // brotliFilePanelOriginal
            // 
            this.brotliFilePanelOriginal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brotliFilePanelOriginal.Location = new System.Drawing.Point(0, 5);
            this.brotliFilePanelOriginal.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.brotliFilePanelOriginal.Name = "brotliFilePanelOriginal";
            this.brotliFilePanelOriginal.Size = new System.Drawing.Size(439, 365);
            this.brotliFilePanelOriginal.TabIndex = 0;
            this.brotliFilePanelOriginal.Title = "Original";
            this.brotliFilePanelOriginal.MarkersUpdated += new System.EventHandler<BrotliBuilder.Components.MarkedTextBox.MarkerUpdateEventArgs>(this.brotliFilePanel_MarkersUpdated);
            // 
            // splitContainerRight
            // 
            this.splitContainerRight.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainerRight.Location = new System.Drawing.Point(0, 0);
            this.splitContainerRight.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainerRight.Name = "splitContainerRight";
            this.splitContainerRight.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerRight.Panel1
            // 
            this.splitContainerRight.Panel1.Controls.Add(this.labelFileStructure);
            this.splitContainerRight.Panel1.Controls.Add(this.flowPanelBlocks);
            this.splitContainerRight.Panel1MinSize = 200;
            // 
            // splitContainerRight.Panel2
            // 
            this.splitContainerRight.Panel2.Controls.Add(this.splitContainerRightBottom);
            this.splitContainerRight.Panel2MinSize = 200;
            this.splitContainerRight.Size = new System.Drawing.Size(884, 580);
            this.splitContainerRight.SplitterDistance = 208;
            this.splitContainerRight.SplitterWidth = 2;
            this.splitContainerRight.TabIndex = 0;
            // 
            // labelFileStructure
            // 
            this.labelFileStructure.AutoSize = true;
            this.labelFileStructure.Location = new System.Drawing.Point(11, 7);
            this.labelFileStructure.Margin = new System.Windows.Forms.Padding(11, 7, 3, 0);
            this.labelFileStructure.Name = "labelFileStructure";
            this.labelFileStructure.Size = new System.Drawing.Size(76, 15);
            this.labelFileStructure.TabIndex = 0;
            this.labelFileStructure.Text = "File Structure";
            // 
            // splitContainerMain
            // 
            this.splitContainerMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMain.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMain.Name = "splitContainerMain";
            // 
            // splitContainerMain.Panel1
            // 
            this.splitContainerMain.Panel1.Controls.Add(this.brotliMarkerInfoPanel);
            this.splitContainerMain.Panel1MinSize = 150;
            // 
            // splitContainerMain.Panel2
            // 
            this.splitContainerMain.Panel2.Controls.Add(this.splitContainerRight);
            this.splitContainerMain.Panel2MinSize = 250;
            this.splitContainerMain.Size = new System.Drawing.Size(1264, 580);
            this.splitContainerMain.SplitterDistance = 376;
            this.splitContainerMain.TabIndex = 0;
            // 
            // brotliMarkerInfoPanel
            // 
            this.brotliMarkerInfoPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.brotliMarkerInfoPanel.Location = new System.Drawing.Point(0, 5);
            this.brotliMarkerInfoPanel.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.brotliMarkerInfoPanel.Name = "brotliMarkerInfoPanel";
            this.brotliMarkerInfoPanel.Size = new System.Drawing.Size(377, 575);
            this.brotliMarkerInfoPanel.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ClientSize = new System.Drawing.Size(1264, 602);
            this.Controls.Add(this.splitContainerMain);
            this.Controls.Add(this.statusBar);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Menu = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(640, 320);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Brotli Builder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeStructure)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelPadding3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statusBarPanelTimeOutput)).EndInit();
            this.splitContainerRightBottom.Panel1.ResumeLayout(false);
            this.splitContainerRightBottom.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRightBottom)).EndInit();
            this.splitContainerRightBottom.ResumeLayout(false);
            this.splitContainerRight.Panel1.ResumeLayout(false);
            this.splitContainerRight.Panel1.PerformLayout();
            this.splitContainerRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerRight)).EndInit();
            this.splitContainerRight.ResumeLayout(false);
            this.splitContainerMain.Panel1.ResumeLayout(false);
            this.splitContainerMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMain)).EndInit();
            this.splitContainerMain.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem menuItemFile;
        private System.Windows.Forms.MenuItem menuItemExit;
        private System.Windows.Forms.FlowLayoutPanel flowPanelBlocks;
        private System.Windows.Forms.MenuItem menuItemSaveBrotli;
        private System.Windows.Forms.MenuItem menuItemOpenBrotli;
        private System.Windows.Forms.StatusBar statusBar;
        private System.Windows.Forms.StatusBarPanel statusBarPanelTimeBits;
        private System.Windows.Forms.StatusBarPanel statusBarPanelTimeOutput;
        private System.Windows.Forms.StatusBarPanel statusBarPanelPadding1;
        private System.Windows.Forms.StatusBarPanel statusBarPanelPadding2;
        private System.Windows.Forms.Timer timerRegenerationDelay;
        private System.Windows.Forms.MenuItem menuItemTools;
        private System.Windows.Forms.MenuItem menuItemEncodeFile;
        private System.Windows.Forms.MenuItem menuItemView;
        private System.Windows.Forms.MenuItem menuItemStaticDictionary;
        private System.Windows.Forms.SplitContainer splitContainerRightBottom;
        private System.Windows.Forms.MenuItem menuItemWrapOutput;
        private Components.BrotliFilePanel brotliFilePanelGenerated;
        private Components.BrotliFilePanel brotliFilePanelOriginal;
        private System.Windows.Forms.SplitContainer splitContainerRight;
        private System.Windows.Forms.MenuItem menuItemFileStructure;
        private System.Windows.Forms.MenuItem menuItemTransform;
        private System.Windows.Forms.SplitContainer splitContainerMain;
        private Components.BrotliMarkerInfoPanel brotliMarkerInfoPanel;
        private System.Windows.Forms.Label labelFileStructure;
        private System.Windows.Forms.MenuItem menuItemMarkerInfo;
        private System.Windows.Forms.MenuItem menuItemWrapMarkerInfo;
        private System.Windows.Forms.MenuItem menuItemCompareMarkers;
        private System.Windows.Forms.StatusBarPanel statusBarPanelTimeStructure;
        private System.Windows.Forms.StatusBarPanel statusBarPanelPadding3;
        private System.Windows.Forms.MenuItem menuItemSaveOutput;
        private System.Windows.Forms.MenuItem menuItemSeparatorT1;
        private System.Windows.Forms.MenuItem menuItemSeparatorT2;
        private System.Windows.Forms.MenuItem menuItemCloneGeneratedToOriginal;
        private System.Windows.Forms.MenuItem menuItemCloneOriginalToGenerated;
        private System.Windows.Forms.MenuItem menuItemConfigureSerializationParameters;
    }
}