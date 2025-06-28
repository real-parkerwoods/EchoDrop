namespace EchoDrop
{
    partial class Settings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Settings));
            btnSave = new Button();
            btnExit = new Button();
            lblNewLineDelim = new Label();
            tbNewLineDelim = new TextBox();
            tbOutputDirectory = new TextBox();
            lblDefaultOutputDir = new Label();
            btnBrowse = new Button();
            SuspendLayout();
            // 
            // btnSave
            // 
            btnSave.Location = new Point(339, 126);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(96, 23);
            btnSave.TabIndex = 4;
            btnSave.Text = "Save Changes";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnExit
            // 
            btnExit.Location = new Point(441, 126);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(63, 23);
            btnExit.TabIndex = 5;
            btnExit.Text = "Exit";
            btnExit.UseVisualStyleBackColor = true;
            btnExit.Click += btnExit_Click;
            // 
            // lblNewLineDelim
            // 
            lblNewLineDelim.AutoSize = true;
            lblNewLineDelim.Location = new Point(12, 13);
            lblNewLineDelim.Name = "lblNewLineDelim";
            lblNewLineDelim.Size = new Size(107, 15);
            lblNewLineDelim.TabIndex = 0;
            lblNewLineDelim.Text = "Newline Delimeter:";
            // 
            // tbNewLineDelim
            // 
            tbNewLineDelim.Location = new Point(12, 31);
            tbNewLineDelim.Name = "tbNewLineDelim";
            tbNewLineDelim.Size = new Size(323, 23);
            tbNewLineDelim.TabIndex = 1;
            // 
            // tbOutputDirectory
            // 
            tbOutputDirectory.Location = new Point(12, 75);
            tbOutputDirectory.Name = "tbOutputDirectory";
            tbOutputDirectory.ReadOnly = true;
            tbOutputDirectory.Size = new Size(323, 23);
            tbOutputDirectory.TabIndex = 2;
            // 
            // lblDefaultOutputDir
            // 
            lblDefaultOutputDir.AutoSize = true;
            lblDefaultOutputDir.Location = new Point(12, 57);
            lblDefaultOutputDir.Name = "lblDefaultOutputDir";
            lblDefaultOutputDir.Size = new Size(99, 15);
            lblDefaultOutputDir.TabIndex = 0;
            lblDefaultOutputDir.Text = "Output Directory:";
            // 
            // btnBrowse
            // 
            btnBrowse.Location = new Point(341, 75);
            btnBrowse.Name = "btnBrowse";
            btnBrowse.Size = new Size(63, 23);
            btnBrowse.TabIndex = 3;
            btnBrowse.Text = "Browse";
            btnBrowse.UseVisualStyleBackColor = true;
            btnBrowse.Click += btnBrowse_Click;
            // 
            // Settings
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnExit;
            ClientSize = new Size(516, 173);
            ControlBox = false;
            Controls.Add(btnBrowse);
            Controls.Add(tbOutputDirectory);
            Controls.Add(lblDefaultOutputDir);
            Controls.Add(tbNewLineDelim);
            Controls.Add(lblNewLineDelim);
            Controls.Add(btnExit);
            Controls.Add(btnSave);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Settings";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Settings";
            TopMost = true;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnSave;
        private Button btnExit;
        private Label lblNewLineDelim;
        private TextBox tbNewLineDelim;
        private TextBox tbOutputDirectory;
        private Label lblDefaultOutputDir;
        private Button btnBrowse;
    }
}