using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NLog;
using ParkingOut.Models;
using ParkingOut.Services;
using ParkingOut.Utils;
using ParkingOut.Services.Implementations;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        #region Fields

        private IAppLogger _logger;
        private readonly IVehicleEntryService vehicleEntryService;
        private readonly IVehicleExitService vehicleExitService;

        private ObservableCollection<ActivityLogItem> _recentActivities = new ObservableCollection<ActivityLogItem>();
        private ObservableCollection<VehicleTypeStats> _vehicleTypeStats = new ObservableCollection<VehicleTypeStats>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardPage"/> class.
        /// </summary>
        public DashboardPage(IAppLogger logger)
        {
            InitializeComponent();

            // Initialize services
            _logger = logger;
            _entryService = Services.ServiceLocator.GetService<IVehicleEntryService>();
            _exitService = Services.ServiceLocator.GetService<IVehicleExitService>();

            // Set data contexts
            RecentActivityDataGrid.ItemsSource = _recentActivities;
            VehicleTypesDataGrid.ItemsSource = _vehicleTypeStats;

            // Load dashboard data
            LoadDashboardData();

            _logger.Debug("DashboardPage initialized");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads dashboard data.
        /// </summary>
        private void LoadDashboardData()
        {
            try
            {
                StatusText.Text = "Loading data...";
                
                // Update summary cards
                UpdateSummaryCards();
                
                // Load recent activities
                LoadRecentActivities();
                
                // Load vehicle type stats
                LoadVehicleTypeStats();
                
                // Update last update time
                LastUpdateText.Text = $"Last Update: {DateTime.Now.ToString("g")}";
                
                StatusText.Text = "Ready";
                
                _logger.Debug("Dashboard data loaded");
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading data";
                _logger.Error("Failed to load dashboard data", ex);
                MessageBox.Show($"Failed to load dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the summary cards with current data.
        /// </summary>
        private void UpdateSummaryCards()
        {
            try
            {
                // In a real application, these values would come from the database
                
                // Total vehicles currently parked
                var totalVehicles = new Random().Next(10, 80);
                TotalVehiclesTextBlock.Text = totalVehicles.ToString();
                
                // Available spaces
                const int totalSpaces = 100;
                var availableSpaces = totalSpaces - totalVehicles;
                AvailableSpacesTextBlock.Text = availableSpaces.ToString();
                
                // Revenue
                var revenue = new Random().Next(100, 1000);
                RevenueTextBlock.Text = $"${revenue:F2}";
                
                // We don't have these UI elements in the XAML, so we'll skip them
                // var todayVehicles = new Random().Next(10, 50);
                // var monthlyRevenue = new Random().Next(1000, 10000);
                // var monthlyVehicles = new Random().Next(100, 500);
                
                _logger.Debug("Summary cards updated");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to update summary cards", ex);
            }
        }

        /// <summary>
        /// Loads recent activities.
        /// </summary>
        private void LoadRecentActivities()
        {
            try
            {
                _recentActivities.Clear();
                
                // In a real application, these would come from the database
                // Add some sample activities
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddMinutes(-5),
                    ActivityType = "Entry",
                    TicketNo = "T000003",
                    LicensePlate = "F9012GH",
                    VehicleType = "Truck",
                    Description = "Vehicle entered through Gate 1"
                });
                
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddMinutes(-20),
                    ActivityType = "Entry",
                    TicketNo = "T000002",
                    LicensePlate = "D5678EF",
                    VehicleType = "Motorcycle",
                    Description = "Vehicle entered through Gate 2"
                });
                
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddMinutes(-45),
                    ActivityType = "Exit",
                    TicketNo = "T000001",
                    LicensePlate = "B1234CD",
                    VehicleType = "Car",
                    Description = "Vehicle exited through Gate 1. Fee: $10.00"
                });
                
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddHours(-1),
                    ActivityType = "Entry",
                    TicketNo = "T000001",
                    LicensePlate = "B1234CD",
                    VehicleType = "Car",
                    Description = "Vehicle entered through Gate 1"
                });
                
                _logger.Debug($"Recent activities loaded: {_recentActivities.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to load recent activities", ex);
            }
        }

        /// <summary>
        /// Loads vehicle type statistics.
        /// </summary>
        private void LoadVehicleTypeStats()
        {
            try
            {
                _vehicleTypeStats.Clear();
                
                // In a real application, these would come from the database
                // Add some sample stats
                var totalVehicles = 75;
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    VehicleType = "Car",
                    Count = 45,
                    Percentage = 45.0 * 100 / totalVehicles,
                    Revenue = 450.00m
                });
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    VehicleType = "Motorcycle",
                    Count = 20,
                    Percentage = 20.0 * 100 / totalVehicles,
                    Revenue = 200.00m
                });
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    VehicleType = "Truck",
                    Count = 8,
                    Percentage = 8.0 * 100 / totalVehicles,
                    Revenue = 160.00m
                });
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    VehicleType = "Bus",
                    Count = 2,
                    Percentage = 2.0 * 100 / totalVehicles,
                    Revenue = 50.00m
                });
                
                _logger.Debug($"Vehicle type stats loaded: {_vehicleTypeStats.Count}");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load vehicle type stats", ex);
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the RefreshButton control.
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadDashboardData();
            }
            catch (Exception ex)
            {
                _logger.Error("Error refreshing dashboard data", ex);
                MessageBox.Show($"Error refreshing dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    // Using ActivityLogItem from ParkingOut.Models namespace

    /// <summary>
    /// Represents vehicle type statistics.
    /// </summary>
    public class VehicleTypeStats
    {
        /// <summary>
        /// Gets or sets the vehicle type.
        /// </summary>
        public string VehicleType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the count of vehicles of this type.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the percentage representation.
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Gets or sets the revenue generated from this vehicle type.
        /// </summary>
        public decimal Revenue { get; set; }
    }
}