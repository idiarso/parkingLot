using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for VehicleMonitoringPage.xaml
    /// </summary>
    public partial class VehicleMonitoringPage : Page
    {
        private ObservableCollection<VehicleRecord> _allVehicles;
        private string _searchText = string.Empty;
        private bool _isPlaceholderVisible = true;

        public VehicleMonitoringPage()
        {
            InitializeComponent();
            
            // Create value converter for visibility binding
            this.Resources.Add("BoolToVisibilityConverter", new BooleanToVisibilityConverter());
            
            // Initialize with sample data
            LoadSampleData();
            
            // Initial filter to show all vehicles
            FilterVehicles("all");
            
            // Update statistics
            UpdateStatistics();
        }

        #region Data Management

        private void LoadSampleData()
        {
            // In a real application, this would load from a database
            _allVehicles = new ObservableCollection<VehicleRecord>
            {
                new VehicleRecord
                {
                    TicketId = "T-1001",
                    PlateNumber = "ABC 123",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-5),
                    ExitTime = null,
                    Status = "Parked",
                    IsExitVisible = true,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1002",
                    PlateNumber = "DEF 456",
                    VehicleType = "Motorcycle",
                    EntryTime = DateTime.Now.AddHours(-4),
                    ExitTime = null,
                    Status = "Parked",
                    IsExitVisible = true,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1003",
                    PlateNumber = "GHI 789",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-3.5),
                    ExitTime = DateTime.Now.AddHours(-1),
                    Duration = "2h 30m",
                    Fee = "$5.00",
                    Status = "Completed",
                    IsExitVisible = false,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1004",
                    PlateNumber = "JKL 012",
                    VehicleType = "Truck",
                    EntryTime = DateTime.Now.AddHours(-7),
                    ExitTime = null,
                    Status = "Parked",
                    IsExitVisible = true,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1005",
                    PlateNumber = "MNO 345",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-2),
                    ExitTime = null,
                    Status = "Parked",
                    IsExitVisible = true,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1006",
                    PlateNumber = "PQR 678",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-8),
                    ExitTime = DateTime.Now.AddHours(-3),
                    Duration = "5h 00m",
                    Fee = "$10.00",
                    Status = "Completed",
                    IsExitVisible = false,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1007",
                    PlateNumber = "STU 901",
                    VehicleType = "Motorcycle",
                    EntryTime = DateTime.Now.AddHours(-1),
                    ExitTime = null,
                    Status = "Parked",
                    IsExitVisible = true,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1008",
                    PlateNumber = "VWX 234",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-6),
                    ExitTime = DateTime.Now.AddHours(-2),
                    Duration = "4h 00m",
                    Fee = "$8.00",
                    Status = "Completed",
                    IsExitVisible = false,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1009",
                    PlateNumber = "YZA 567",
                    VehicleType = "SUV",
                    EntryTime = DateTime.Now.AddMinutes(-30),
                    ExitTime = null,
                    Status = "Parked",
                    IsExitVisible = true,
                    IsPrintVisible = true
                },
                new VehicleRecord
                {
                    TicketId = "T-1010",
                    PlateNumber = "BCD 890",
                    VehicleType = "Car",
                    EntryTime = DateTime.Now.AddHours(-5.5),
                    ExitTime = DateTime.Now.AddHours(-0.5),
                    Duration = "5h 00m",
                    Fee = "$10.00",
                    Status = "Completed",
                    IsExitVisible = false,
                    IsPrintVisible = true
                },
            };
        }

        private void UpdateStatistics()
        {
            // Count vehicles currently parked
            int parkedVehicles = _allVehicles.Count(v => v.Status == "Parked");
            txtVehiclesInParking.Text = $"{parkedVehicles} Vehicles in Parking";

            // Count entry and exit activity for today
            int entryCount = _allVehicles.Count(v => v.EntryTime.Date == DateTime.Now.Date);
            int exitCount = _allVehicles.Count(v => v.ExitTime.HasValue && v.ExitTime.Value.Date == DateTime.Now.Date);

            txtEntryCount.Text = entryCount.ToString();
            txtExitCount.Text = exitCount.ToString();

            // Calculate average duration and total revenue for today
            var completedToday = _allVehicles.Where(v => 
                v.Status == "Completed" && 
                v.ExitTime.HasValue && 
                v.ExitTime.Value.Date == DateTime.Now.Date).ToList();

            if (completedToday.Any())
            {
                double avgMinutes = completedToday.Average(v => 
                {
                    TimeSpan duration = v.ExitTime.Value - v.EntryTime;
                    return duration.TotalMinutes;
                });
                
                int hours = (int)(avgMinutes / 60);
                int minutes = (int)(avgMinutes % 60);
                txtAvgDuration.Text = $"{hours}h {minutes}m";

                // Calculate revenue (remove $ and convert to double)
                double revenue = completedToday.Sum(v => 
                {
                    if (double.TryParse(v.Fee.Replace("$", ""), out double fee))
                        return fee;
                    return 0;
                });
                
                txtRevenue.Text = $"${revenue:N2}";
            }
            else
            {
                txtAvgDuration.Text = "0h 0m";
                txtRevenue.Text = "$0.00";
            }
        }

        private void FilterVehicles(string filter, string searchText = "")
        {
            if (_allVehicles == null)
                return;

            ICollectionView view = CollectionViewSource.GetDefaultView(_allVehicles);
            
            // Apply filter based on tab and search text
            view.Filter = item =>
            {
                var vehicle = item as VehicleRecord;
                if (vehicle == null)
                    return false;

                // First apply the tab filter
                bool passesTabFilter = filter switch
                {
                    "all" => true,
                    "entry" => vehicle.EntryTime.Date == DateTime.Now.Date,
                    "exit" => vehicle.ExitTime.HasValue && vehicle.ExitTime.Value.Date == DateTime.Now.Date,
                    "parked" => vehicle.Status == "Parked",
                    _ => true
                };

                if (!passesTabFilter)
                    return false;

                // Then apply the search filter if needed
                if (string.IsNullOrWhiteSpace(searchText) || searchText == "Search by plate number, ticket, or vehicle type...")
                    return true;

                // Search in multiple fields
                return 
                    vehicle.PlateNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    vehicle.TicketId.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    vehicle.VehicleType.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            };

            gridVehicles.ItemsSource = view;
        }

        #endregion

        #region Event Handlers

        private void tabVehicleActivity_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles("all", _searchText);
        }

        private void tabEntryOnly_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles("entry", _searchText);
        }

        private void tabExitOnly_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles("exit", _searchText);
        }

        private void tabCurrentlyParked_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles("parked", _searchText);
        }

        private void txtSearch_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_isPlaceholderVisible)
            {
                txtSearch.Text = string.Empty;
                txtSearch.Foreground = new SolidColorBrush(Colors.Black);
                _isPlaceholderVisible = false;
            }
        }

        private void txtSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "Search by plate number, ticket, or vehicle type...";
                txtSearch.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAA"));
                _isPlaceholderVisible = true;
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isPlaceholderVisible)
            {
                _searchText = txtSearch.Text;
                
                // Determine which filter to apply based on selected tab
                string filter = "all";
                if (tabEntryOnly.IsChecked == true)
                    filter = "entry";
                else if (tabExitOnly.IsChecked == true)
                    filter = "exit";
                else if (tabCurrentlyParked.IsChecked == true)
                    filter = "parked";
                
                FilterVehicles(filter, _searchText);
            }
        }

        private void btnNewEntry_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would open a form to register a new vehicle entry
            MessageBox.Show("This would open the vehicle entry form.", "New Entry", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnProcessExit_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would open a form to process a vehicle exit
            MessageBox.Show("This would open the vehicle exit form.", "Process Exit", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnPrintReport_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would generate a report
            MessageBox.Show("This would generate and print a report.", "Print Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnRefreshData_Click(object sender, RoutedEventArgs e)
        {
            // In a real application, this would refresh data from the database
            MessageBox.Show("Data refreshed successfully!", "Refresh Data", MessageBoxButton.OK, MessageBoxImage.Information);
            
            // For demo purposes, we'll just update statistics
            UpdateStatistics();
        }

        private void gridVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Handle selection changed event
        }

        private void btnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is VehicleRecord vehicle)
            {
                // In a real application, this would open a detailed view
                MessageBox.Show($"Viewing details for vehicle {vehicle.PlateNumber}", "Vehicle Details", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnPrintTicket_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is VehicleRecord vehicle)
            {
                // In a real application, this would print a ticket
                MessageBox.Show($"Printing ticket for {vehicle.TicketId}", "Print Ticket", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnProcessExitRow_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is VehicleRecord vehicle)
            {
                // In a real application, this would process the exit for this vehicle
                vehicle.ExitTime = DateTime.Now;
                vehicle.Status = "Completed";
                
                // Calculate duration
                TimeSpan duration = vehicle.ExitTime.Value - vehicle.EntryTime;
                int hours = duration.Hours;
                int minutes = duration.Minutes;
                vehicle.Duration = $"{hours}h {minutes}m";
                
                // Calculate fee (simplified for demo)
                double rate = 2.0; // $2 per hour
                double fee = Math.Ceiling(duration.TotalHours) * rate;
                vehicle.Fee = $"${fee:N2}";
                
                // Update visibility of buttons
                vehicle.IsExitVisible = false;
                
                // Update statistics
                UpdateStatistics();
                
                // Refresh the view
                ICollectionView view = CollectionViewSource.GetDefaultView(_allVehicles);
                view.Refresh();
                
                MessageBox.Show($"Exit processed for {vehicle.PlateNumber}. Fee: {vehicle.Fee}", "Exit Processed", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion
    }

    public class VehicleRecord : INotifyPropertyChanged
    {
        public string TicketId { get; set; }
        public string PlateNumber { get; set; }
        public string VehicleType { get; set; }
        public DateTime EntryTime { get; set; }
        
        private DateTime? _exitTime;
        public DateTime? ExitTime
        {
            get => _exitTime;
            set
            {
                if (_exitTime != value)
                {
                    _exitTime = value;
                    OnPropertyChanged(nameof(ExitTime));
                }
            }
        }
        
        public string Duration { get; set; }
        public string Fee { get; set; }
        
        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        
        private bool _isExitVisible;
        public bool IsExitVisible
        {
            get => _isExitVisible;
            set
            {
                if (_isExitVisible != value)
                {
                    _isExitVisible = value;
                    OnPropertyChanged(nameof(IsExitVisible));
                }
            }
        }
        
        private bool _isPrintVisible;
        public bool IsPrintVisible
        {
            get => _isPrintVisible;
            set
            {
                if (_isPrintVisible != value)
                {
                    _isPrintVisible = value;
                    OnPropertyChanged(nameof(IsPrintVisible));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 