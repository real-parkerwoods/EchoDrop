namespace EchoDrop
{
    partial class MainED
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainED));
            tbStatus = new TextBox();
            tbFileLocation = new TextBox();
            btnSelectFile = new Button();
            btnLoadFile = new Button();
            panelFileBlocks = new Panel();
            msMain = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            msMainGenerateBash = new ToolStripMenuItem();
            msMainSettings = new ToolStripMenuItem();
            msMain.SuspendLayout();
            SuspendLayout();
            // 
            // tbStatus
            // 
            tbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            tbStatus.BackColor = Color.White;
            tbStatus.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tbStatus.ForeColor = SystemColors.WindowText;
            tbStatus.Location = new Point(651, 29);
            tbStatus.Multiline = true;
            tbStatus.Name = "tbStatus";
            tbStatus.ReadOnly = true;
            tbStatus.Size = new Size(373, 380);
            tbStatus.TabIndex = 0;
            tbStatus.TabStop = false;
            tbStatus.Text = ">Welcome to EchoDrop. Please select a console log.";
            // 
            // tbFileLocation
            // 
            tbFileLocation.AllowDrop = true;
            tbFileLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbFileLocation.Location = new Point(12, 28);
            tbFileLocation.Name = "tbFileLocation";
            tbFileLocation.Size = new Size(485, 23);
            tbFileLocation.TabIndex = 1;
            tbFileLocation.TextChanged += tbFileLocation_TextChanged;
            tbFileLocation.Leave += tbFileLocation_Leave;
            // 
            // btnSelectFile
            // 
            btnSelectFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelectFile.Location = new Point(503, 28);
            btnSelectFile.Name = "btnSelectFile";
            btnSelectFile.Size = new Size(61, 23);
            btnSelectFile.TabIndex = 2;
            btnSelectFile.Text = "Browse";
            btnSelectFile.UseVisualStyleBackColor = true;
            btnSelectFile.Click += btnSelectFile_Click;
            // 
            // btnLoadFile
            // 
            btnLoadFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoadFile.Enabled = false;
            btnLoadFile.Location = new Point(570, 29);
            btnLoadFile.Name = "btnLoadFile";
            btnLoadFile.Size = new Size(75, 22);
            btnLoadFile.TabIndex = 3;
            btnLoadFile.Text = "Load File";
            btnLoadFile.UseVisualStyleBackColor = true;
            btnLoadFile.Click += btnLoadFile_Click;
            // 
            // panelFileBlocks
            // 
            panelFileBlocks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelFileBlocks.AutoScroll = true;
            panelFileBlocks.BackColor = SystemColors.ActiveCaptionText;
            panelFileBlocks.BackgroundImage = (Image)resources.GetObject("panelFileBlocks.BackgroundImage");
            panelFileBlocks.BackgroundImageLayout = ImageLayout.Zoom;
            panelFileBlocks.Location = new Point(12, 57);
            panelFileBlocks.Name = "panelFileBlocks";
            panelFileBlocks.Size = new Size(633, 352);
            panelFileBlocks.TabIndex = 0;
            // 
            // msMain
            // 
            msMain.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem });
            msMain.Location = new Point(0, 0);
            msMain.Name = "msMain";
            msMain.Size = new Size(1036, 24);
            msMain.TabIndex = 0;
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { msMainGenerateBash, msMainSettings });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // msMainGenerateBash
            // 
            msMainGenerateBash.Name = "msMainGenerateBash";
            msMainGenerateBash.Size = new Size(180, 22);
            msMainGenerateBash.Text = "Generate Linux Bash";
            msMainGenerateBash.Click += msMainGenerateBash_Click;
            // 
            // msMainSettings
            // 
            msMainSettings.Name = "msMainSettings";
            msMainSettings.Size = new Size(180, 22);
            msMainSettings.Text = "Settings Menu";
            msMainSettings.Click += msMainSettings_Click;
            // 
            // MainED
            // 
            AcceptButton = btnLoadFile;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1036, 420);
            Controls.Add(panelFileBlocks);
            Controls.Add(btnLoadFile);
            Controls.Add(btnSelectFile);
            Controls.Add(tbFileLocation);
            Controls.Add(tbStatus);
            Controls.Add(msMain);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = msMain;
            MinimumSize = new Size(1052, 434);
            Name = "MainED";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "EchoDrop";
            msMain.ResumeLayout(false);
            msMain.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbStatus;
        private TextBox tbFileLocation;
        private Button btnSelectFile;
        private Button btnLoadFile;
        private Panel panelFileBlocks;
        private MenuStrip msMain;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem msMainGenerateBash;
        private ToolStripMenuItem msMainSettings;
    }
}
