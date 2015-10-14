using System;
using System.Windows.Forms;

namespace NppSDPlugin
{
    public partial class frmSetRoot : Form
    {
        public string SelectedPath { get; set; }

        public frmSetRoot(string initPath)
        {
            InitializeComponent();

            if(!String.IsNullOrEmpty(initPath))
            {
                SelectedPath = initPath;
                tbxPath.Text = SelectedPath;
            }

            CheckBtnOKStatus();
        }
        public frmSetRoot() : this(null)
        {
        }

        private void btnBrowser_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDlg = new FolderBrowserDialog())
            {
                if (folderDlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    tbxPath.Text = folderDlg.SelectedPath;
                    CheckBtnOKStatus();
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string path = tbxPath.Text.Trim();
            if (!System.IO.Directory.Exists(path))
            {
                MessageBox.Show("The directory not exist, please select again!");
                return;
            }
            this.SelectedPath = tbxPath.Text.Trim();
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Hide();
        }

        private void tbxPath_TextChanged(object sender, EventArgs e)
        {
            CheckBtnOKStatus();
        }

        private void CheckBtnOKStatus()
        {
            SelectedPath = tbxPath.Text.Trim();
            btnOK.Enabled = System.IO.Directory.Exists(SelectedPath);
        }
    }
}
