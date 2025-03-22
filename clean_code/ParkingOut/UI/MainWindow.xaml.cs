using System;
using System.Windows;
using NLog;
using ParkingOut.Utils;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IAppLogger _logger;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            _logger = new AppLogger(typeof(MainWindow));
            
            try
            {
                _logger.Info("Initializing MainWindow component");
                InitializeComponent();
                _logger.Info("MainWindow component initialized successfully");
                
                // Initialize sidebar
                InitializeSidebar();
                
                // Navigate to dashboard by default
                NavigateToDashboard();
            }
            catch (Exception ex)
            {
                // Use the MessageHelper to display error
                MessageHelper.ShowError("Error initializing main window", ex);
                _logger.Error("Failed to initialize MainWindow component", ex);
            }
        }
        
        /// <summary>
        /// Initializes the sidebar with menu items.
        /// </summary>
        private void InitializeSidebar()
        {
            try
            {
                _logger.Info("Initializing sidebar");
                
                // Dashboard menu item
                Sidebar.AddMenuItem("Dashboard", IconPaths.Dashboard, "dashboard");
                
                // Vehicle management menu items
                Sidebar.AddMenuItem("Vehicle Entry", IconPaths.CarEnter, "vehicle_entry");
                Sidebar.AddMenuItem("Vehicle Exit", IconPaths.CarExit, "vehicle_exit");
                
                // Reports menu item
                Sidebar.AddMenuItem("Reports", IconPaths.Report, "reports");
                
                // Settings menu item
                Sidebar.AddMenuItem("Settings", IconPaths.Settings, "settings");
                
                _logger.Info("Sidebar initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize sidebar", ex);
                MessageHelper.ShowError("Error initializing sidebar", ex);
            }
        }
        
        /// <summary>
        /// Handles the MenuItemClicked event of the Sidebar control.
        /// </summary>
        private void Sidebar_MenuItemClicked(object sender, MenuItem e)
        {
            try
            {
                _logger.Info($"Menu item clicked: {e.Tag}");
                
                switch (e.Tag)
                {
                    case "dashboard":
                        NavigateToDashboard();
                        break;
                    case "vehicle_entry":
                        NavigateToVehicleEntry();
                        break;
                    case "vehicle_exit":
                        NavigateToVehicleExit();
                        break;
                    case "reports":
                        // TODO: Implement reports page navigation
                        MessageHelper.ShowInfo("Reports feature is coming soon!");
                        break;
                    case "settings":
                        // TODO: Implement settings page navigation
                        MessageHelper.ShowInfo("Settings feature is coming soon!");
                        break;
                    default:
                        _logger.Warning($"Unknown menu item tag: {e.Tag}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error navigating to {e.Tag}", ex);
                MessageHelper.ShowError($"Error navigating to {e.Text}", ex);
            }
        }
        
        /// <summary>
        /// Handles the LogoutClicked event of the Sidebar control.
        /// </summary>
        private void Sidebar_LogoutClicked(object sender, EventArgs e)
        {
            try
            {
                _logger.Info("Logout clicked");
                
                // Show confirmation dialog
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to log out?",
                    "Confirm Logout",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    _logger.Info("User confirmed logout");
                    
                    // TODO: Implement actual logout logic
                    MessageHelper.ShowInfo("Logout feature is coming soon!");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error during logout", ex);
                MessageHelper.ShowError("Error during logout", ex);
            }
        }
        
        /// <summary>
        /// Navigates to the dashboard page.
        /// </summary>
        private void NavigateToDashboard()
        {
            try
            {
                _logger.Info("Navigating to Dashboard");
                
                // For now, just show a simple message
                var dashboardPage = new DashboardPage();
                MainFrame.Navigate(dashboardPage);
                
                // Set the active menu item
                Sidebar.ActiveMenuItem = Sidebar.GetMenuItem("dashboard");
            }
            catch (Exception ex)
            {
                _logger.Error("Error navigating to Dashboard", ex);
                MessageHelper.ShowError("Error navigating to Dashboard", ex);
            }
        }
        
        /// <summary>
        /// Navigates to the vehicle entry page.
        /// </summary>
        private void NavigateToVehicleEntry()
        {
            try
            {
                _logger.Info("Navigating to Vehicle Entry");
                
                var vehicleEntryPage = new VehicleEntryPage();
                MainFrame.Navigate(vehicleEntryPage);
                
                // Set the active menu item
                Sidebar.ActiveMenuItem = Sidebar.GetMenuItem("vehicle_entry");
            }
            catch (Exception ex)
            {
                _logger.Error("Error navigating to Vehicle Entry", ex);
                MessageHelper.ShowError("Error navigating to Vehicle Entry", ex);
            }
        }
        
        /// <summary>
        /// Navigates to the vehicle exit page.
        /// </summary>
        private void NavigateToVehicleExit()
        {
            try
            {
                _logger.Info("Navigating to Vehicle Exit");
                
                var vehicleExitPage = new VehicleExitPage();
                MainFrame.Navigate(vehicleExitPage);
                
                // Set the active menu item
                Sidebar.ActiveMenuItem = Sidebar.GetMenuItem("vehicle_exit");
            }
            catch (Exception ex)
            {
                _logger.Error("Error navigating to Vehicle Exit", ex);
                MessageHelper.ShowError("Error navigating to Vehicle Exit", ex);
            }
        }
    }
} 