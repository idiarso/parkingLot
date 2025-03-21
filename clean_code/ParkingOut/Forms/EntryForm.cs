using System;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;

namespace SimpleParkingAdmin.Forms
{
    public partial class EntryForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;

        public EntryForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = new FileLogger();
            InitializeComponent();
            InitializeEntryForm();
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            // Designer code will be here
        }
        #endregion

        private void InitializeEntryForm()
        {
            try
            {
                _logger.Information("Initializing entry form");
                // ... existing code ...
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize entry form", ex);
                MessageBox.Show("Failed to initialize entry form. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProcessEntry()
        {
            try
            {
                _logger.Information("Processing new entry");
                // ... existing code ...
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to process entry", ex);
                MessageBox.Show("Failed to process entry. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 