using System;
using System.Windows;
using System.Windows.Controls;
using NLog;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardPage"/> class.
        /// </summary>
        public DashboardPage()
        {
            try
            {
                InitializeComponent();
                LoadDashboardData();
                logger.Debug("DashboardPage initialized");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error initializing DashboardPage");
                System.Windows.MessageBox.Show($"Error initializing dashboard: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Loads data for the dashboard.
        /// In a real application, this would retrieve data from the database.
        /// For this example, we're using static data.
        /// </summary>
        private void LoadDashboardData()
        {
            try
            {
                // In a real application, you would load this data from a database or service
                // Here we're just using the hardcoded values in the XAML
                
                // For a production app, you might use:
                // 1. Data binding with view models
                // 2. Asynchronous loading with progress indicators
                // 3. Periodic refresh of dashboard data
                
                logger.Debug("Dashboard data loaded successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error loading dashboard data");
                System.Windows.MessageBox.Show($"Error loading dashboard data: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 