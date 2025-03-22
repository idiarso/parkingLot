using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ParkingOut.Utils;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for VehicleExitPage.xaml
    /// </summary>
    public partial class VehicleExitPage : Page
    {
        private readonly IAppLogger _logger;
        private ObservableCollection<VehicleExitInfo> _vehicles;

        public VehicleExitPage()
        {
            InitializeComponent();
            _logger = new AppLogger(GetType().Name);
            _vehicles = new ObservableCollection<VehicleExitInfo>();
            dgvVehicles.ItemsSource = _vehicles;
            
            // Wire up events
            btnSearch.Click += BtnSearch_Click;
            btnProcessExit.Click += BtnProcessExit_Click;
            btnCancel.Click += BtnCancel_Click;
            
            // Load active tickets
            LoadActiveTickets();
        }

        private void LoadActiveTickets()
        {
            try
            {
                _logger.Info("Loading active parking tickets");
                
                // Clear existing items
                _vehicles.Clear();
                
                // In a real implementation, this would query the database
                string sql = "SELECT * FROM t_parkir WHERE waktu_keluar IS NULL";
                
                // Use the sql variable in a real implementation
                // var data = Database.ExecuteQuery(sql);
                
                // Add some sample data for demonstration
                _vehicles.Add(new VehicleExitInfo
                {
                    TicketNo = "T001",
                    PlateNo = "B1234CD",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-2),
                    Duration = "2 hours",
                    TotalFee = "$10.00"
                });
                
                _vehicles.Add(new VehicleExitInfo
                {
                    TicketNo = "T002",
                    PlateNo = "D5678EF",
                    VehicleType = "Motorcycle",
                    EntryTime = DateTime.Now.AddHours(-1),
                    Duration = "1 hour",
                    TotalFee = "$5.00"
                });
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load active tickets", ex);
                System.Windows.MessageBox.Show("Error loading active tickets: " + ex.Message, "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(searchText))
                {
                    LoadActiveTickets();
                    return;
                }
                
                _logger.Info($"Searching for ticket: {searchText}");
                
                // In a real implementation, this would query the database
                // For now, just filter the current collection
                var filteredList = new ObservableCollection<VehicleExitInfo>();
                foreach (var vehicle in _vehicles)
                {
                    if ((vehicle.TicketNo?.Contains(searchText) == true) || 
                        (vehicle.PlateNo?.Contains(searchText) == true))
                    {
                        filteredList.Add(vehicle);
                    }
                }
                
                dgvVehicles.ItemsSource = filteredList;
            }
            catch (Exception ex)
            {
                _logger.Error("Search error", ex);
                System.Windows.MessageBox.Show("Error performing search: " + ex.Message, "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnProcessExit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgvVehicles.SelectedItem == null)
                {
                    System.Windows.MessageBox.Show("Please select a vehicle to process.", "Information", 
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                var selectedVehicle = dgvVehicles.SelectedItem as VehicleExitInfo;
                if (selectedVehicle == null)
                {
                    System.Windows.MessageBox.Show("Invalid selection.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _logger.Info($"Processing exit for ticket: {selectedVehicle.TicketNo}");
                
                // In a real implementation, this would update the database
                System.Windows.MessageBox.Show($"Vehicle with ticket {selectedVehicle.TicketNo} has been processed.\nTotal fee: {selectedVehicle.TotalFee}", 
                                "Exit Processed", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reload the list
                LoadActiveTickets();
            }
            catch (Exception ex)
            {
                _logger.Error("Error processing exit", ex);
                System.Windows.MessageBox.Show("Error processing vehicle exit: " + ex.Message, "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Clear();
            LoadActiveTickets();
        }
    }

    // A simple class to represent vehicle exit information
    public class VehicleExitInfo
    {
        public string? TicketNo { get; set; }
        public string? PlateNo { get; set; }
        public string? VehicleType { get; set; }
        public DateTime EntryTime { get; set; }
        public string? Duration { get; set; }
        public string? TotalFee { get; set; }
    }
} 