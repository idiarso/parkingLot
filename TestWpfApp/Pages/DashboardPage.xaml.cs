using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using TestWpfApp.Models;
using TestWpfApp.Services;
using TestWpfApp.Services.Interfaces;
using TestWpfApp.ViewModels;

namespace TestWpfApp.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private readonly IAppLogger _logger;
        private readonly IDatabaseService _databaseService;
        private readonly DispatcherTimer _timer;
        private readonly ObservableCollection<ActivityLogItem> _activityLogs;
        private ObservableCollection<ParkingActivity> _recentActivities;
        private int _currentPage = 1;
        private const int _pageSize = 5;

        public DashboardPage(IAppLogger logger, IDatabaseService databaseService)
        {
            InitializeComponent();
            _logger = logger;
            _databaseService = databaseService;
            _activityLogs = new ObservableCollection<ActivityLogItem>();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Check if user is authenticated
            if (!UserSession.IsUserLoggedIn)
            {
                NavigationService?.Navigate(new LoginPage());
                return;
            }

            // Initialize view model with dependencies
            DataContext = new DashboardViewModel(logger, databaseService);

            // Initialize UI elements with user information
            txtWelcome.Text = $"Welcome, {UserSession.CurrentUser.DisplayName} | Role: {UserSession.CurrentUser.Role} | Last Login: {UserSession.CurrentUser.LastLogin:g}";

            LoadSampleData();

            // Initial update
            UpdateDashboard();
        }

        private void LoadSampleData()
        {
            // Sample data for recent activities
            var activities = new ObservableCollection<ParkingActivity>
            {
                new ParkingActivity
                {
                    Time = DateTime.Now.AddMinutes(-5),
                    VehicleNumber = "B 1234 CD",
                    VehicleType = "Car",
                    Action = "Entry",
                    Duration = "0:05",
                    Fee = 2000
                },

                new ParkingActivity
                {
                    Time = DateTime.Now.AddMinutes(-15),
                    VehicleNumber = "B 5678 EF",
                    VehicleType = "Motorcycle",
                    Action = "Exit",
                    Duration = "0:30",
                    Fee = 3000
                },

                new ParkingActivity
                {
                    Time = DateTime.Now.AddMinutes(-30),
                    VehicleNumber = "B 9012 GH",
                    VehicleType = "Car",
                    Action = "Entry",
                    Duration = "0:30",
                    Fee = 2000
                }
            };

            dgRecentActivities.ItemsSource = activities;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Update time
            CurrentTimeText.Text = DateTime.Now.ToString("HH:mm:ss");

            // Update dashboard data
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
                    TotalSpotsText.Text = parkingStats.TotalSpots.ToString();
                    OccupiedSpotsText.Text = parkingStats.OccupiedSpots.ToString();
                    AvailableSpotsText.Text = parkingStats.AvailableSpots.ToString();
                    RevenueTodayText.Text = parkingStats.RevenueToday.ToString("C");
                    RevenueWeekText.Text = parkingStats.RevenueWeek.ToString("C");
                    RevenueMonthText.Text = parkingStats.RevenueMonth.ToString("C");

                    // Update vehicle types
                    foreach (var type in parkingStats.VehicleTypes)
                    {
                        var vehicleTypeControl = FindName($"VehicleType{type.Key}") as TextBlock;
                        if (vehicleTypeControl != null)
                        {
                            vehicleTypeControl.Text = type.Value.ToString();
                        }
                    }
                }

                // Update system status
                DbStatusText.Text = _databaseService.IsConnected ? "Connected" : "Disconnected";
                CameraStatusText.Text = await _databaseService.IsCameraOnlineAsync() ? "Online" : "Offline";
                PrinterStatusText.Text = await _databaseService.IsPrinterReadyAsync() ? "Ready" : "Not Ready";

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

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _logger.Info("User logged out");
            UserSession.Logout();
            NavigationService?.Navigate(new LoginPage());
        }

        private void QuickVehicleEntry_Click(object sender, RoutedEventArgs e)
        {
            _logger.Info("Navigating to vehicle entry page");
            NavigationService?.Navigate(new VehicleEntryPage());
        }

        private void QuickVehicleExit_Click(object sender, RoutedEventArgs e)
        {
            _logger.Info("Navigating to vehicle exit page");
            NavigationService?.Navigate(new VehicleExitPage());
        }

        private void QuickVehicleMonitoring_Click(object sender, RoutedEventArgs e)
        {
            _logger.Info("Navigating to vehicle monitoring page");
            NavigationService?.Navigate(new VehicleMonitoringPage());
        }
    }

    public class ParkingActivity
    {
        public DateTime Time { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string Action { get; set; }
        public string Duration { get; set; }
        public decimal Fee { get; set; }
    }
}