using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            
            // Set default password (in a real app, this would be retrieved from secure storage)
            txtDbPassword.Password = "root@rsi";
        }

        private void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            // In a real app, this would test the database connection
            // For demo purposes, we'll just show a success message
            
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtDbServer.Text) || 
                string.IsNullOrWhiteSpace(txtDbPort.Text) ||
                string.IsNullOrWhiteSpace(txtDbName.Text) ||
                string.IsNullOrWhiteSpace(txtDbUsername.Text))
            {
                MessageBox.Show("Please fill in all database connection fields", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Simulate connection test
                bool isSuccess = true;
                
                if (isSuccess)
                {
                    MessageBox.Show("Database connection successful!", "Connection Test", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Failed to connect to the database. Please check your settings.", 
                        "Connection Test", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing connection: {ex.Message}", "Connection Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Open folder browser dialog
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "Select Backup Directory",
                ShowNewFolderButton = true
            };

            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtBackupDir.Text = dialog.SelectedPath;
            }
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            // Validate rate inputs (ensure they are numbers)
            if (!int.TryParse(txtCarRate.Text, out _) ||
                !int.TryParse(txtMotorcycleRate.Text, out _) ||
                !int.TryParse(txtBusRate.Text, out _) ||
                !int.TryParse(txtTruckRate.Text, out _))
            {
                MessageBox.Show("Parking rates must be valid numbers", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // In a real app, this would save settings to a config file or database
            // For demo purposes, we'll just show a success message
            MessageBox.Show("Settings saved successfully!", "Settings Saved", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
} 