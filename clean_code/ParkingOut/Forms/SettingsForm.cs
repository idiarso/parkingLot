using System;
using System.Data;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;
using Serilog;

namespace SimpleParkingAdmin.Forms
{
    public partial class SettingsForm : Form
    {
        private readonly IAppLogger _logger;
        private TextBox txtServer;
        private TextBox txtPort;
        private TextBox txtDatabase;
        private TextBox txtUsername;
        private TextBox txtPassword;

        public SettingsForm()
        {
            _logger = new FileLogger();
            InitializeComponent();
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            // Initialize form controls
            txtServer = new TextBox();
            txtPort = new TextBox();
            txtDatabase = new TextBox();
            txtUsername = new TextBox();
            txtPassword = new TextBox();

            // Set up layout
            // ... designer code will be here
        }
        #endregion

        private void LoadSettings()
        {
            try
            {
                // Get settings as an object instead of DataTable
                var settings = Database.LoadNetworkSettingsObject();
                
                txtServer.Text = settings.ServerIP;
                txtPort.Text = settings.Port.ToString();
                txtDatabase.Text = settings.DatabaseName;
                txtUsername.Text = settings.Username;
                txtPassword.Text = settings.Password;
                
                _logger.Debug("Network settings loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load settings", ex);
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TestDatabaseConnection()
        {
            try
            {
                // Create a temporary NetworkSettings object with the form values
                var tempSettings = new NetworkSettings
                {
                    ServerIP = txtServer.Text.Trim(),
                    Port = int.Parse(txtPort.Text.Trim()),
                    DatabaseName = txtDatabase.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    DatabaseType = "PostgreSQL" // Default to PostgreSQL
                };
                
                // Get the connection string
                string connectionString = tempSettings.GetConnectionString();
                
                // Test the connection
                string errorMessage;
                bool success = Database.TestConnectionWithString(connectionString, out errorMessage);
                
                if (success)
                {
                    MessageBox.Show("Database connection test successful!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MessageBox.Show($"Database connection test failed: {errorMessage}", 
                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Database connection test failed", ex);
                MessageBox.Show($"Database connection test failed: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool SaveSettings()
        {
            try
            {
                // Create a NetworkSettings object and save it
                var settings = new SimpleParkingAdmin.Models.NetworkSettings
                {
                    ServerIP = txtServer.Text.Trim(),
                    Port = int.Parse(txtPort.Text.Trim()),
                    DatabaseName = txtDatabase.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    DatabaseType = "PostgreSQL" // Default to PostgreSQL
                };

                // Save to database
                bool success = settings.SaveToDatabase();
                
                if (success)
                {
                    _logger.Information("Network settings saved successfully");
                    MessageBox.Show("Settings saved successfully. You may need to restart the application for changes to take effect.", 
                        "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _logger.Warning("Failed to save network settings");
                    MessageBox.Show("Failed to save settings. Please try again.", 
                        "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.Error("Error saving settings", ex);
                MessageBox.Show($"Error saving settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
} 