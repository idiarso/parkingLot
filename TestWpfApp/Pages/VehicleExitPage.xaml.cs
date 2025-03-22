using System;
using System.Windows;
using System.Windows.Controls;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for VehicleExitPage.xaml
    /// </summary>
    public partial class VehicleExitPage : Page
    {
        // Sample vehicle entry data (in a real app, this would come from a database)
        private class VehicleEntry
        {
            public string VehicleNumber { get; set; }
            public string VehicleType { get; set; }
            public DateTime EntryTime { get; set; }
        }

        // Mock database with sample vehicle entries
        private readonly VehicleEntry[] _vehicleEntries = new[]
        {
            new VehicleEntry { VehicleNumber = "B 1234 XYZ", VehicleType = "Car", EntryTime = DateTime.Now.AddHours(-2).AddMinutes(-20) },
            new VehicleEntry { VehicleNumber = "B 5678 ABC", VehicleType = "Car", EntryTime = DateTime.Now.AddHours(-1).AddMinutes(-15) },
            new VehicleEntry { VehicleNumber = "B 7890 DEF", VehicleType = "Motorcycle", EntryTime = DateTime.Now.AddHours(-3).AddMinutes(-45) }
        };

        // Selected vehicle
        private VehicleEntry _selectedVehicle;

        public VehicleExitPage()
        {
            InitializeComponent();
            
            // Initialize receipt date and time
            txtReceiptDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtReceiptTime.Text = DateTime.Now.ToString("HH:mm");
            
            // Generate a transaction ID
            txtReceiptTransactionId.Text = $"TRX-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchVehicleNumber.Text))
            {
                MessageBox.Show("Please enter a vehicle number to search.", "Search Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Search for the vehicle (in a real app, this would query a database)
            _selectedVehicle = Array.Find(_vehicleEntries, v => 
                v.VehicleNumber.Equals(txtSearchVehicleNumber.Text, StringComparison.OrdinalIgnoreCase));

            if (_selectedVehicle == null)
            {
                MessageBox.Show("Vehicle not found. Please check the vehicle number and try again.", 
                    "Search Result", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear the vehicle info and receipt
                ClearVehicleInfo();
                return;
            }

            // Calculate parking duration and fee
            TimeSpan duration = DateTime.Now - _selectedVehicle.EntryTime;
            int durationHours = (int)Math.Ceiling(duration.TotalHours);
            string durationText = $"{duration.Hours}h {duration.Minutes}m";
            
            // Calculate fee (simple calculation for demo)
            decimal fee = _selectedVehicle.VehicleType == "Car" ? 
                durationHours * 5000 : durationHours * 2000;
            string feeText = $"Rp {fee:N0}";

            // Update the vehicle info display
            txtVehicleNumber.Text = _selectedVehicle.VehicleNumber;
            txtVehicleType.Text = _selectedVehicle.VehicleType;
            txtEntryTime.Text = _selectedVehicle.EntryTime.ToString("yyyy-MM-dd HH:mm");
            txtDuration.Text = durationText;
            txtParkingFee.Text = feeText;

            // Update the receipt
            UpdateReceiptInfo(_selectedVehicle, durationText, feeText);
        }

        private void btnProcessExit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicle == null)
            {
                MessageBox.Show("Please search for a vehicle first.", "Process Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Here you would normally update the database
                // For demo purposes, we'll just show a success message
                
                string paymentMethod = ((ComboBoxItem)cmbPaymentMethod.SelectedItem).Content.ToString();
                txtReceiptPaymentMethod.Text = paymentMethod;
                
                MessageBox.Show($"Vehicle {_selectedVehicle.VehicleNumber} has been processed for exit.\nPayment method: {paymentMethod}", 
                    "Exit Processed", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear the vehicle search
                txtSearchVehicleNumber.Clear();
                
                // Keep the receipt info for printing
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnPrintReceipt_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedVehicle == null)
            {
                MessageBox.Show("No receipt to print. Please process a vehicle exit first.", 
                    "Print Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // In a real app, this would print the receipt
            // For demo purposes, we'll just show a success message
            MessageBox.Show("Receipt has been sent to the printer.", "Print Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            // Clear everything after printing
            ClearVehicleInfo();
            txtSearchVehicleNumber.Clear();
            _selectedVehicle = null;
            
            // Generate a new transaction ID for the next receipt
            txtReceiptTransactionId.Text = $"TRX-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        }

        private void UpdateReceiptInfo(VehicleEntry vehicle, string durationText, string feeText)
        {
            txtReceiptVehicleNumber.Text = vehicle.VehicleNumber;
            txtReceiptVehicleType.Text = vehicle.VehicleType;
            txtReceiptEntryTime.Text = vehicle.EntryTime.ToString("HH:mm");
            txtReceiptExitTime.Text = DateTime.Now.ToString("HH:mm");
            txtReceiptDuration.Text = durationText;
            txtReceiptParkingFee.Text = feeText;
        }

        private void ClearVehicleInfo()
        {
            txtVehicleNumber.Text = "-";
            txtVehicleType.Text = "-";
            txtEntryTime.Text = "-";
            txtDuration.Text = "-";
            txtParkingFee.Text = "-";
            
            txtReceiptVehicleNumber.Text = "-";
            txtReceiptVehicleType.Text = "-";
            txtReceiptEntryTime.Text = "-";
            txtReceiptExitTime.Text = DateTime.Now.ToString("HH:mm");
            txtReceiptDuration.Text = "-";
            txtReceiptParkingFee.Text = "-";
        }
    }
}