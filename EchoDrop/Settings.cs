using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EchoDrop
{
    public partial class Settings : Form
    {
        string tmpOutputDirectory = string.Empty;
        public Settings()
        {
            InitializeComponent();
            tbNewLineDelim.Text = MainED.newLineDelim;
            tbOutputDirectory.Text = MainED.outputDirectory;
        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            MainED.newLineDelim = tbNewLineDelim.Text;
            if (Directory.Exists(tmpOutputDirectory)) MainED.outputDirectory = tmpOutputDirectory;
            Close();
        }
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            tmpOutputDirectory = tbOutputDirectory.Text;
            using (var folderDlg = new FolderBrowserDialog())
            {
                folderDlg.Description = "File Output Directory";
                folderDlg.ShowNewFolderButton = true;
                if (folderDlg.ShowDialog() == DialogResult.OK) tmpOutputDirectory = folderDlg.SelectedPath;
                folderDlg.Dispose();
            }
            tbOutputDirectory.Text = tmpOutputDirectory;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
