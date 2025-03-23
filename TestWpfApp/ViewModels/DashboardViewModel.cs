using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;
using TestWpfApp.Models;
using TestWpfApp.Services;
using TestWpfApp.Services.Interfaces;

namespace TestWpfApp.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IAppLogger _logger;
        private readonly IDatabaseService _databaseService;
        private string _currentTime;
        private string _dbStatus;
        private string _cameraStatus;
        private string _printerStatus;
        private int _totalSpots;
        private int _occupiedSpots;
        private int _availableSpots;
        private decimal _revenueToday;
        private decimal _revenueWeek;
        private decimal _revenueMonth;
        private int _vehicleTypeCars;
        private int _vehicleTypeMotorcycles;
        private int _vehicleTypeBuses;
        private ObservableCollection<ActivityLogItem> _activityLogs;
        private ObservableCollection<ParkingActivity> _recentActivities;
        private int _currentPage = 1;
        private const int _pageSize = 5;
        private int _totalPages;

        public DashboardViewModel(IAppLogger logger, IDatabaseService databaseService)
        {
            _logger = logger;
            _databaseService = databaseService;
            _activityLogs = new ObservableCollection<ActivityLogItem>();
            _recentActivities = new ObservableCollection<ParkingActivity>();

            // Initialize commands
            LogoutCommand = new RelayCommand(Logout);
            NavigateToVehicleEntryPageCommand = new RelayCommand(NavigateToVehicleEntryPage);
            NavigateToVehicleExitPageCommand = new RelayCommand(NavigateToVehicleExitPage);
            NavigateToVehicleMonitoringPageCommand = new RelayCommand(NavigateToVehicleMonitoringPage);
            NavigateToPreviousPageCommand = new RelayCommand(NavigateToPreviousPage);
            NavigateToNextPageCommand = new RelayCommand(NavigateToNextPage);

            // Start timer for updates
            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(1);
            Timer.Tick += Timer_Tick;
            Timer.Start();

            // Initial update
            UpdateDashboard();
            LoadRecentActivities();
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public string DbStatus
        {
            get => _dbStatus;
            set => SetProperty(ref _dbStatus, value);
        }

        public string CameraStatus
        {
            get => _cameraStatus;
            set => SetProperty(ref _cameraStatus, value);
        }

        public string PrinterStatus
        {
            get => _printerStatus;
            set => SetProperty(ref _printerStatus, value);
        }

        public int TotalSpots
        {
            get => _totalSpots;
            set => SetProperty(ref _totalSpots, value);
        }

        public int OccupiedSpots
        {
            get => _occupiedSpots;
            set => SetProperty(ref _occupiedSpots, value);
        }

        public int AvailableSpots
        {
            get => _availableSpots;
            set => SetProperty(ref _availableSpots, value);
        }

        public decimal RevenueToday
        {
            get => _revenueToday;
            set => SetProperty(ref _revenueToday, value);
        }

        public decimal RevenueWeek
        {
            get => _revenueWeek;
            set => SetProperty(ref _revenueWeek, value);
        }

        public decimal RevenueMonth
        {
            get => _revenueMonth;
            set => SetProperty(ref _revenueMonth, value);
        }

        public int VehicleTypeCars
        {
            get => _vehicleTypeCars;
            set => SetProperty(ref _vehicleTypeCars, value);
        }

        public int VehicleTypeMotorcycles
        {
            get => _vehicleTypeMotorcycles;
            set => SetProperty(ref _vehicleTypeMotorcycles, value);
        }

        public int VehicleTypeBuses
        {
            get => _vehicleTypeBuses;
            set => SetProperty(ref _vehicleTypeBuses, value);
        }

        public ObservableCollection<ActivityLogItem> ActivityLogs
        {
            get => _activityLogs;
            set => SetProperty(ref _activityLogs, value);
        }

        public ObservableCollection<ParkingActivity> RecentActivities
        {
            get => _recentActivities;
            set => SetProperty(ref _recentActivities, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public ICommand LogoutCommand { get; }
        public ICommand NavigateToVehicleEntryPageCommand { get; }
        public ICommand NavigateToVehicleExitPageCommand { get; }
        public ICommand NavigateToVehicleMonitoringPageCommand { get; }
        public ICommand NavigateToPreviousPageCommand { get; }
        public ICommand NavigateToNextPageCommand { get; }

        private DispatcherTimer Timer { get; }

        private void Timer_Tick(object sender, EventArgs e)
        {
            CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            UpdateDashboard();
        }

        private async void UpdateDashboard()
        {
            try
            {
                // Update parking statistics
                var parkingStats = await _databaseService.GetParkingStatisticsAsync();
                if (parkingStats != null)
                {
                    TotalSpots = parkingStats.TotalSpots;
                    OccupiedSpots = parkingStats.OccupiedSpots;
                    AvailableSpots = parkingStats.AvailableSpots;
                    RevenueToday = parkingStats.TodayRevenue;
                    RevenueWeek = parkingStats.WeekRevenue;
                    RevenueMonth = parkingStats.MonthRevenue;

                    // Update vehicle types
                    VehicleTypeCars = parkingStats.VehicleTypes.TryGetValue("car", out var cars) ? cars : 0;
                    VehicleTypeMotorcycles = parkingStats.VehicleTypes.TryGetValue("motorcycle", out var motorcycles) ? motorcycles : 0;
                    VehicleTypeBuses = parkingStats.VehicleTypes.TryGetValue("bus", out var buses) ? buses : 0;
                }

                // Update system status
                DbStatus = _databaseService.IsConnected ? "Connected" : "Disconnected";
                CameraStatus = await _databaseService.IsCameraOnlineAsync() ? "Online" : "Offline";
                PrinterStatus = await _databaseService.IsPrinterReadyAsync() ? "Ready" : "Not Ready";

                // Add new activity logs
                var newLogs = await _databaseService.GetRecentActivityLogsAsync();
                foreach (var log in newLogs)
                {
                    if (!_activityLogs.Contains(log))
                    {
                        _activityLogs.Insert(0, log);
                        if (_activityLogs.Count > 10) // Keep only last 10 entries
                        {
                            _activityLogs.RemoveAt(_activityLogs.Count - 1);
                        }
                        _logger.Info($"New activity logged: {log.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating dashboard: {ex.Message}", ex);
                MessageBox.Show($"Error updating dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LoadRecentActivities()
        {
            try
            {
                var activities = await _databaseService.GetRecentActivityLogsAsync();
                RecentActivities.Clear();
                foreach (var activity in activities)
                {
                    RecentActivities.Add(new ParkingActivity
                    {
                        Time = activity.CreatedAt,
                        VehicleNumber = activity.Message, // Using message as vehicle number for now
                        VehicleType = activity.Type,
                        Action = activity.Type,
                        Duration = "N/A",
                        Fee = 0
                    });
                }

                TotalPages = (int)Math.Ceiling(RecentActivities.Count / (double)_pageSize);
                if (CurrentPage > TotalPages)
                {
                    CurrentPage = TotalPages;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading recent activities: {ex.Message}", ex);
                MessageBox.Show($"Error loading recent activities: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateToPreviousPage()
        {
            if (CurrentPage > 1)
            {
                CurrentPage--;
            }
        }

        private void NavigateToNextPage()
        {
            if (CurrentPage < TotalPages)
            {
                CurrentPage++;
            }
        }

        private void Logout()
        {
            _logger.Info("User logged out");
            UserSession.Current.Logout();
            NavigationService.Navigate(new LoginPage());
        }

        private void NavigateToVehicleEntryPage()
        {
            _logger.Info("Navigating to vehicle entry page");
            NavigationService.Navigate(new VehicleEntryPage());
        }

        private void NavigateToVehicleExitPage()
        {
            _logger.Info("Navigating to vehicle exit page");
            NavigationService.Navigate(new VehicleExitPage());
        }

        private void NavigateToVehicleMonitoringPage()
        {
            _logger.Info("Navigating to vehicle monitoring page");
            NavigationService.Navigate(new VehicleMonitoringPage());
        }
    }
}
