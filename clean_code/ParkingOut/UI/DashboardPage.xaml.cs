using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NLog;
using ParkingOut.Models;
using ParkingOut.Services;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        #region Fields

        private readonly IAppLogger logger;
        private readonly IVehicleEntryService vehicleEntryService;
        private readonly IVehicleExitService vehicleExitService;

        private ObservableCollection<ActivityLogItem> _recentActivities = new ObservableCollection<ActivityLogItem>();
        private ObservableCollection<VehicleTypeStats> _vehicleTypeStats = new ObservableCollection<VehicleTypeStats>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardPage"/> class.
        /// </summary>
        public DashboardPage()
        {
            InitializeComponent();

            // Initialize services
            logger = LogManager.GetCurrentClassLogger() as IAppLogger;
            vehicleEntryService = new VehicleEntryService();
            vehicleExitService = new VehicleExitService(vehicleEntryService);

            // Set data contexts
            RecentActivityDataGrid.ItemsSource = _recentActivities;
            VehicleTypesDataGrid.ItemsSource = _vehicleTypeStats;

            // Load dashboard data
            LoadDashboardData();

            logger?.Debug("DashboardPage initialized");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the dashboard data.
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
                
                logger?.Debug("Dashboard data loaded");
            }
            catch (Exception ex)
            {
                StatusText.Text = "Error loading data";
                logger?.Error(ex, "Failed to load dashboard data");
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
                
                // Today's revenue
                var todayRevenue = new Random().Next(100, 1000);
                TodayRevenueTextBlock.Text = $"${todayRevenue:F2}";
                
                var todayVehicles = new Random().Next(10, 50);
                TodayVehiclesTextBlock.Text = $"From {todayVehicles} Vehicles";
                
                // Monthly revenue
                var monthlyRevenue = new Random().Next(1000, 10000);
                MonthlyRevenueTextBlock.Text = $"${monthlyRevenue:F2}";
                
                var monthlyVehicles = new Random().Next(100, 500);
                MonthlyVehiclesTextBlock.Text = $"From {monthlyVehicles} Vehicles";
                
                logger?.Debug("Summary cards updated");
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Failed to update summary cards");
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
                    Details = "Vehicle entered through Gate 1"
                });
                
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddMinutes(-20),
                    ActivityType = "Entry",
                    TicketNo = "T000002",
                    LicensePlate = "D5678EF",
                    VehicleType = "Motorcycle",
                    Details = "Vehicle entered through Gate 2"
                });
                
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddMinutes(-45),
                    ActivityType = "Exit",
                    TicketNo = "T000001",
                    LicensePlate = "B1234CD",
                    VehicleType = "Car",
                    Details = "Vehicle exited through Gate 1. Fee: $10.00"
                });
                
                _recentActivities.Add(new ActivityLogItem
                {
                    Timestamp = DateTime.Now.AddHours(-1),
                    ActivityType = "Entry",
                    TicketNo = "T000001",
                    LicensePlate = "B1234CD",
                    VehicleType = "Car",
                    Details = "Vehicle entered through Gate 1"
                });
                
                logger?.Debug("Recent activities loaded: {Count}", _recentActivities.Count);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Failed to load recent activities");
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
                    Type = "Car",
                    Count = 45,
                    Percentage = $"{45 * 100 / totalVehicles}%"
                });
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    Type = "Motorcycle",
                    Count = 20,
                    Percentage = $"{20 * 100 / totalVehicles}%"
                });
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    Type = "Truck",
                    Count = 8,
                    Percentage = $"{8 * 100 / totalVehicles}%"
                });
                
                _vehicleTypeStats.Add(new VehicleTypeStats
                {
                    Type = "Bus",
                    Count = 2,
                    Percentage = $"{2 * 100 / totalVehicles}%"
                });
                
                logger?.Debug("Vehicle type stats loaded: {Count}", _vehicleTypeStats.Count);
            }
            catch (Exception ex)
            {
                logger?.Error(ex, "Failed to load vehicle type stats");
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
                logger?.Error(ex, "Error refreshing dashboard data");
                MessageBox.Show($"Error refreshing dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    /// <summary>
    /// Represents an activity log item.
    /// </summary>
    public class ActivityLogItem
    {
        /// <summary>
        /// Gets or sets the timestamp of the activity.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the type of activity.
        /// </summary>
        public string ActivityType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ticket number.
        /// </summary>
        public string TicketNo { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the license plate.
        /// </summary>
        public string LicensePlate { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle type.
        /// </summary>
        public string VehicleType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets additional details.
        /// </summary>
        public string Details { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents vehicle type statistics.
    /// </summary>
    public class VehicleTypeStats
    {
        /// <summary>
        /// Gets or sets the vehicle type.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the count of vehicles of this type.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the percentage representation.
        /// </summary>
        public string Percentage { get; set; } = string.Empty;
    }
} 