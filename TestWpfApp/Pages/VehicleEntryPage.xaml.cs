using System;
using System.Windows;
using System.Windows.Controls;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for VehicleEntryPage.xaml
    /// </summary>
    public partial class VehicleEntryPage : Page
    {
        public string CurrentTime { get; set; }

        public VehicleEntryPage()
        {
            InitializeComponent();
            
            // Set current date and time
            dpEntryDate.SelectedDate = DateTime.Now;
            CurrentTime = DateTime.Now.ToString("HH:mm");
            
            // Set DataContext for binding
            this.DataContext = this;
        }

        private void btnSubmitEntry_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtVehicleNumber.Text))
            {
                MessageBox.Show("Please enter a vehicle number.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Here you would normally save to the database
                // For demo purposes, we'll just show a success message
                
                string vehicleType = ((ComboBoxItem)cmbVehicleType.SelectedItem).Content.ToString();
                string entryTime = $"{dpEntryDate.SelectedDate?.ToString("yyyy-MM-dd")} {txtEntryTime.Text}";
                
                MessageBox.Show($"Vehicle {txtVehicleNumber.Text} ({vehicleType}) has been registered at {entryTime}.", 
                    "Vehicle Entry", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear the form
                txtVehicleNumber.Clear();
                cmbVehicleType.SelectedIndex = 0;
                txtDriverName.Clear();
                txtNotes.Clear();
                
                // Reset date and time
                dpEntryDate.SelectedDate = DateTime.Now;
                CurrentTime = DateTime.Now.ToString("HH:mm");
                txtEntryTime.Text = CurrentTime;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnUseDetected_Click(object sender, RoutedEventArgs e)
        {
            // Use the detected license plate number
            txtVehicleNumber.Text = txtDetectedNumber.Text;
        }
    }
} 