using System;
using System.Data;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
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
                var settings = Database.LoadNetworkSettings();
                foreach (DataRow row in settings.Rows)
                {
                    string key = row["setting_key"].ToString();
                    string value = row["setting_value"].ToString();
                    
                    switch (key)
                    {
                        case "network_server":
                            txtServer.Text = value;
                            break;
                        case "network_port":
                            txtPort.Text = value;
                            break;
                        case "network_database":
                            txtDatabase.Text = value;
                            break;
                        case "network_username":
                            txtUsername.Text = value;
                            break;
                        case "network_password":
                            txtPassword.Text = value;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load settings", ex);
                MessageBox.Show("Failed to load settings. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TestDatabaseConnection()
        {
            try
            {
                string connectionString = $"Server={txtServer.Text};Port={txtPort.Text};Database={txtDatabase.Text};Uid={txtUsername.Text};Pwd={txtPassword.Text};";
                Database.SetConnectionString(connectionString);
                
                if (Database.TestConnection())
                {
                    MessageBox.Show("Database connection test successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true;
                }
                else
                {
                    MessageBox.Show("Database connection test failed. Please check your settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Database connection test failed", ex);
                MessageBox.Show("Database connection test failed. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private bool SaveSettings()
        {
            try
            {
                Database.SaveSettings("network_server", txtServer.Text);
                Database.SaveSettings("network_port", txtPort.Text);
                Database.SaveSettings("network_database", txtDatabase.Text);
                Database.SaveSettings("network_username", txtUsername.Text);
                Database.SaveSettings("network_password", txtPassword.Text);
                
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save settings", ex);
                MessageBox.Show("Failed to save settings. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
} 