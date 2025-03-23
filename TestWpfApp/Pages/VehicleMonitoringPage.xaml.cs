using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace TestWpfApp.Pages
{
    /// <summary>
    /// Interaction logic for VehicleMonitoringPage.xaml
    /// </summary>
    public partial class VehicleMonitoringPage : Page, INotifyPropertyChanged
    {
        // UI Controls
        private TextBlock txtVehiclesInParking;
        private TextBlock txtEntryCount;
        private TextBlock txtExitCount;
        private TextBlock txtRevenue;
        private DataGrid gridVehicles;
        private TextBox txtTicketId;
        private TextBox txtPlateNumber;
        private ComboBox cmbVehicleType;
        private TextBox txtEntryTime;
        private TextBox txtExitTime;
        private TextBox txtDuration;
        private TextBox txtFee;
        private ComboBox cmbStatus;
        private TextBox txtSearch;

        private ObservableCollection<VehicleRecord> _allVehicles;
        private VehicleRecord? _selectedVehicle;
        private string _searchText = string.Empty;
        private bool _isPlaceholderVisible = true;
        private string _selectedTab = "All";

        public event PropertyChangedEventHandler? PropertyChanged;

        public VehicleMonitoringPage()
        {
            InitializeComponent();
            
            // Create value converter for visibility binding
            this.Resources.Add("BoolToVisibilityConverter", new BooleanToVisibilityConverter());
            
            _allVehicles = new ObservableCollection<VehicleRecord>();
            LoadSampleData();
            FilterVehicles();
            UpdateStatistics();
        }

        public ObservableCollection<VehicleRecord> AllVehicles
        {
            get => _allVehicles;
            set
            {
                _allVehicles = value;
                OnPropertyChanged(nameof(AllVehicles));
            }
        }

        public VehicleRecord? SelectedVehicle
        {
            get => _selectedVehicle;
            set
            {
                _selectedVehicle = value;
                OnPropertyChanged(nameof(SelectedVehicle));
                if (value != null)
                {
                    FillFormWithVehicle(value);
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterVehicles();
            }
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged(nameof(SelectedTab));
                FilterVehicles();
            }
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
            var parkedVehicles = _allVehicles.Count(v => v.Status == "Parked");
            var todayEntries = _allVehicles.Count(v => v.EntryTime.Date == DateTime.Today);
            var todayExits = _allVehicles.Count(v => v.ExitTime?.Date == DateTime.Today);
            
            decimal totalRevenue = 0;
            foreach (var vehicle in _allVehicles.Where(v => v.ExitTime.HasValue))
            {
                if (!string.IsNullOrEmpty(vehicle.Fee) && 
                    decimal.TryParse(vehicle.Fee.Replace("$", ""), out decimal fee))
                {
                    totalRevenue += fee;
                }
            }

            txtVehiclesInParking.Text = $"{parkedVehicles} Vehicles in Parking";
            txtEntryCount.Text = todayEntries.ToString();
            txtExitCount.Text = todayExits.ToString();
            txtRevenue.Text = $"Rp {totalRevenue:N0}";
        }

        private void FilterVehicles()
        {
            var filteredVehicles = _allVehicles.Where(v =>
                (string.IsNullOrEmpty(_searchText) ||
                 v.PlateNumber.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                 v.TicketId.Contains(_searchText, StringComparison.OrdinalIgnoreCase)) &&
                (_selectedTab == "All" ||
                 (_selectedTab == "Parked" && v.Status == "Parked") ||
                 (_selectedTab == "Completed" && v.Status == "Completed")))
                .ToList();

            gridVehicles.ItemsSource = filteredVehicles;
        }

        private void FillFormWithVehicle(VehicleRecord vehicle)
        {
            txtTicketId.Text = vehicle.TicketId;
            txtPlateNumber.Text = vehicle.PlateNumber;
            cmbVehicleType.SelectedItem = vehicle.VehicleType;
            txtEntryTime.Text = vehicle.EntryTime.ToString("HH:mm");
            txtExitTime.Text = vehicle.ExitTime?.ToString("HH:mm") ?? string.Empty;
            txtDuration.Text = vehicle.Duration;
            txtFee.Text = vehicle.Fee;
            cmbStatus.SelectedItem = vehicle.Status;
        }

        private VehicleRecord GetVehicleFromForm()
        {
            return new VehicleRecord
            {
                TicketId = txtTicketId.Text,
                PlateNumber = txtPlateNumber.Text,
                VehicleType = cmbVehicleType.SelectedItem?.ToString() ?? "Car",
                EntryTime = DateTime.Parse(txtEntryTime.Text),
                ExitTime = string.IsNullOrEmpty(txtExitTime.Text) ? null : DateTime.Parse(txtExitTime.Text),
                Duration = txtDuration.Text,
                Fee = txtFee.Text,
                Status = cmbStatus.SelectedItem?.ToString() ?? "Parked"
            };
        }

        private void ClearForm()
        {
            txtTicketId.Text = string.Empty;
            txtPlateNumber.Text = string.Empty;
            cmbVehicleType.SelectedIndex = 0;
            txtEntryTime.Text = string.Empty;
            txtExitTime.Text = string.Empty;
            txtDuration.Text = string.Empty;
            txtFee.Text = string.Empty;
            cmbStatus.SelectedIndex = 0;
            _selectedVehicle = null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Event Handlers

        private void tabVehicleActivity_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles();
        }

        private void tabEntryOnly_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles();
        }

        private void tabExitOnly_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles();
        }

        private void tabCurrentlyParked_Checked(object sender, RoutedEventArgs e)
        {
            FilterVehicles();
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
                SearchText = txtSearch.Text;
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
            MessageBox.Show("Printing report...", "Print Report", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnRefreshData_Click(object sender, RoutedEventArgs e)
        {
            LoadSampleData();
            FilterVehicles();
            UpdateStatistics();
        }

        private void gridVehicles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridVehicles.SelectedItem is VehicleRecord selectedVehicle)
            {
                SelectedVehicle = selectedVehicle;
            }
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

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                SelectedTab = selectedTab.Header?.ToString() ?? "All";
            }
        }

        #endregion
    }

    public class VehicleRecord : INotifyPropertyChanged
    {
        private string _ticketId = string.Empty;
        private string _plateNumber = string.Empty;
        private string _vehicleType = string.Empty;
        private DateTime _entryTime;
        private DateTime? _exitTime;
        private string _duration = string.Empty;
        private string _fee = string.Empty;
        private string _status = string.Empty;
        private bool _isExitVisible = true;
        private bool _isPrintVisible = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string TicketId
        {
            get => _ticketId;
            set
            {
                _ticketId = value;
                OnPropertyChanged(nameof(TicketId));
            }
        }

        public string PlateNumber
        {
            get => _plateNumber;
            set
            {
                _plateNumber = value;
                OnPropertyChanged(nameof(PlateNumber));
            }
        }

        public string VehicleType
        {
            get => _vehicleType;
            set
            {
                _vehicleType = value;
                OnPropertyChanged(nameof(VehicleType));
            }
        }

        public DateTime EntryTime
        {
            get => _entryTime;
            set
            {
                _entryTime = value;
                OnPropertyChanged(nameof(EntryTime));
            }
        }

        public DateTime? ExitTime
        {
            get => _exitTime;
            set
            {
                _exitTime = value;
                OnPropertyChanged(nameof(ExitTime));
            }
        }

        public string Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public string Fee
        {
            get => _fee;
            set
            {
                _fee = value;
                OnPropertyChanged(nameof(Fee));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public bool IsExitVisible
        {
            get => _isExitVisible;
            set
            {
                _isExitVisible = value;
                OnPropertyChanged(nameof(IsExitVisible));
            }
        }

        public bool IsPrintVisible
        {
            get => _isPrintVisible;
            set
            {
                _isPrintVisible = value;
                OnPropertyChanged(nameof(IsPrintVisible));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 