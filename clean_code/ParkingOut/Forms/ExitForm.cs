using System;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;

namespace SimpleParkingAdmin.Forms
{
    public partial class ExitForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;

        public ExitForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = new FileLogger();
            InitializeComponent();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            // Designer code will be here
        }
        #endregion
    }
} 