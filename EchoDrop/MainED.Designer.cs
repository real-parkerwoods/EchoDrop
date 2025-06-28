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
            panelFileBlocks = new Panel();
            msMain = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            msMainSettings = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            msMainGenerateBash = new ToolStripMenuItem();
            selectConsoleLogToolStripMenuItem = new ToolStripMenuItem();
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
            // panelFileBlocks
            // 
            panelFileBlocks.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panelFileBlocks.AutoScroll = true;
            panelFileBlocks.BackColor = SystemColors.ActiveCaptionText;
            panelFileBlocks.BackgroundImage = (Image)resources.GetObject("panelFileBlocks.BackgroundImage");
            panelFileBlocks.BackgroundImageLayout = ImageLayout.Zoom;
            panelFileBlocks.Location = new Point(12, 29);
            panelFileBlocks.Name = "panelFileBlocks";
            panelFileBlocks.Size = new Size(633, 380);
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
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { msMainSettings, toolStripSeparator1, msMainGenerateBash, selectConsoleLogToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "File";
            // 
            // msMainSettings
            // 
            msMainSettings.Name = "msMainSettings";
            msMainSettings.Size = new Size(180, 22);
            msMainSettings.Text = "Settings Menu";
            msMainSettings.Click += msMainSettings_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(177, 6);
            // 
            // msMainGenerateBash
            // 
            msMainGenerateBash.Name = "msMainGenerateBash";
            msMainGenerateBash.Size = new Size(180, 22);
            msMainGenerateBash.Text = "Generate Linux Bash";
            msMainGenerateBash.Click += msMainGenerateBash_Click;
            // 
            // selectConsoleLogToolStripMenuItem
            // 
            selectConsoleLogToolStripMenuItem.Name = "selectConsoleLogToolStripMenuItem";
            selectConsoleLogToolStripMenuItem.Size = new Size(180, 22);
            selectConsoleLogToolStripMenuItem.Text = "Select Console Log";
            selectConsoleLogToolStripMenuItem.Click += selectConsoleLogToolStripMenuItem_Click;
            // 
            // MainED
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1036, 420);
            Controls.Add(panelFileBlocks);
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
        private Panel panelFileBlocks;
        private MenuStrip msMain;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem msMainGenerateBash;
        private ToolStripMenuItem msMainSettings;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem selectConsoleLogToolStripMenuItem;
    }
}
