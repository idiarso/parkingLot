using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using ParkingOut.Utils;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for VehicleEntryPage.xaml
    /// </summary>
    public partial class VehicleEntryPage : Page
    {
        private readonly IAppLogger _logger;
        private ObservableCollection<VehicleEntryInfo> _recentEntries;
        private int _ticketCounter = 1;

        public VehicleEntryPage()
        {
            InitializeComponent();
            _logger = new AppLogger(GetType().Name);
            _recentEntries = new ObservableCollection<VehicleEntryInfo>();
            dgvRecentEntries.ItemsSource = _recentEntries;
            
            // Generate a new ticket number
            GenerateTicketNumber();
            
            // Set default vehicle type
            cmbVehicleType.SelectedIndex = 0;
            
            // Wire up events
            btnSave.Click += BtnSave_Click;
            btnClear.Click += BtnClear_Click;
            chkMember.Checked += ChkMember_CheckedChanged;
            chkMember.Unchecked += ChkMember_CheckedChanged;
            
            // Load recent entries
            LoadRecentEntries();
        }

        private void LoadRecentEntries()
        {
            try
            {
                _logger.Info("Loading recent entries");
                
                // Clear existing items
                _recentEntries.Clear();
                
                // In a real implementation, this would query the database
                // For demonstration purposes, add some sample data
                _recentEntries.Add(new VehicleEntryInfo
                {
                    TicketNo = "T001",
                    PlateNo = "B1234CD",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddMinutes(-30)
                });
                
                _recentEntries.Add(new VehicleEntryInfo
                {
                    TicketNo = "T002",
                    PlateNo = "D5678EF",
                    VehicleType = "Motorcycle",
                    EntryTime = DateTime.Now.AddMinutes(-15)
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load recent entries", ex);
                System.Windows.MessageBox.Show("Error loading recent entries: " + ex.Message, "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateTicketNumber()
        {
            // In a real implementation, this would generate a unique ticket number
            // based on database entries or other logic
            txtTicketNumber.Text = $"T{_ticketCounter:D3}";
            _ticketCounter++;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(txtPlateNumber.Text.Trim()))
                {
                    System.Windows.MessageBox.Show("Plate number is required.", "Validation Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPlateNumber.Focus();
                    return;
                }
                
                if (chkMember.IsChecked == true && string.IsNullOrEmpty(txtMemberId.Text.Trim()))
                {
                    System.Windows.MessageBox.Show("Member ID is required for member vehicles.", "Validation Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtMemberId.Focus();
                    return;
                }
                
                // Get the vehicle type
                var selectedItem = cmbVehicleType.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    System.Windows.MessageBox.Show("Please select a vehicle type.", "Validation Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    cmbVehicleType.Focus();
                    return;
                }
                string vehicleType = selectedItem.Content?.ToString() ?? "Unknown";
                
                _logger.Info($"Saving entry for plate number: {txtPlateNumber.Text}, ticket: {txtTicketNumber.Text}");
                
                // In a real implementation, this would save to the database
                // For demonstration, add to the recent entries list
                _recentEntries.Insert(0, new VehicleEntryInfo
                {
                    TicketNo = txtTicketNumber.Text,
                    PlateNo = txtPlateNumber.Text,
                    VehicleType = vehicleType,
                    EntryTime = DateTime.Now
                });
                
                System.Windows.MessageBox.Show($"Vehicle entry saved successfully.\nTicket Number: {txtTicketNumber.Text}", 
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear the form and generate a new ticket number
                ClearForm();
            }
            catch (Exception ex)
            {
                _logger.Error("Error saving vehicle entry", ex);
                System.Windows.MessageBox.Show("Error saving vehicle entry: " + ex.Message, "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ChkMember_CheckedChanged(object sender, RoutedEventArgs e)
        {
            panelMember.Visibility = chkMember.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearForm()
        {
            GenerateTicketNumber();
            txtPlateNumber.Clear();
            cmbVehicleType.SelectedIndex = 0;
            chkMember.IsChecked = false;
            txtMemberId.Clear();
            txtPlateNumber.Focus();
        }
    }

    // A simple class to represent vehicle entry information
    public class VehicleEntryInfo
    {
        public string? TicketNo { get; set; }
        public string? PlateNo { get; set; }
        public string? VehicleType { get; set; }
        public DateTime EntryTime { get; set; }
    }
} 