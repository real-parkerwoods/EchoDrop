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
            SuspendLayout();
            // 
            // tbStatus
            // 
            tbStatus.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            tbStatus.BackColor = Color.White;
            tbStatus.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            tbStatus.ForeColor = SystemColors.WindowText;
            tbStatus.Location = new Point(651, 13);
            tbStatus.Multiline = true;
            tbStatus.Name = "tbStatus";
            tbStatus.ReadOnly = true;
            tbStatus.Size = new Size(373, 371);
            tbStatus.TabIndex = 0;
            tbStatus.TabStop = false;
            tbStatus.Text = ">Welcome to EchoDrop. Please select a console log.";
            // 
            // tbFileLocation
            // 
            tbFileLocation.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tbFileLocation.Location = new Point(12, 12);
            tbFileLocation.Name = "tbFileLocation";
            tbFileLocation.Size = new Size(485, 23);
            tbFileLocation.TabIndex = 1;
            tbFileLocation.TextChanged += tbFileLocation_TextChanged;
            tbFileLocation.Leave += tbFileLocation_Leave;
            // 
            // btnSelectFile
            // 
            btnSelectFile.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSelectFile.Location = new Point(503, 12);
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
            btnLoadFile.Location = new Point(570, 13);
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
            panelFileBlocks.Location = new Point(12, 41);
            panelFileBlocks.Name = "panelFileBlocks";
            panelFileBlocks.Size = new Size(633, 343);
            panelFileBlocks.TabIndex = 0;
            // 
            // MainED
            // 
            AcceptButton = btnLoadFile;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1036, 395);
            Controls.Add(panelFileBlocks);
            Controls.Add(btnLoadFile);
            Controls.Add(btnSelectFile);
            Controls.Add(tbFileLocation);
            Controls.Add(tbStatus);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MinimumSize = new Size(1052, 434);
            Name = "MainED";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "EchoDrop";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox tbStatus;
        private TextBox tbFileLocation;
        private Button btnSelectFile;
        private Button btnLoadFile;
        private Panel panelFileBlocks;
    }
}
