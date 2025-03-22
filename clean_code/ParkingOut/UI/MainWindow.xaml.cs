using System;
using System.Windows;
using System.Windows.Controls;
using NLog;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                SetupSidebar();
                
                // Navigate to dashboard by default
                ContentFrame.Navigate(new DashboardPage());
                
                logger.Info("MainWindow initialized");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error initializing MainWindow");
                System.Windows.MessageBox.Show($"Error initializing application: {ex.Message}", 
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Sets up the sidebar with menu items and event handlers.
        /// </summary>
        private void SetupSidebar()
        {
            try
            {
                if (Sidebar != null)
                {
                    // Set basic properties
                    Sidebar.Title = "ParkingOut";
                    Sidebar.UserName = "Administrator";
                    
                    // Try to load logo
                    try
                    {
                        var logoPath = System.IO.Path.Combine(
                            AppDomain.CurrentDomain.BaseDirectory, 
                            "Resources", "Images", "logo.png");
                            
                        if (System.IO.File.Exists(logoPath))
                        {
                            var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(logoPath));
                            Sidebar.LogoSource = bitmap;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "Could not load logo image");
                    }
                    
                    // Add menu items
                    Sidebar.AddMenuItem(new MenuItem("Dashboard", IconPaths.DashboardIcon, "dashboard"));
                    Sidebar.AddMenuItem(new MenuItem("Vehicle Entry", IconPaths.CarIcon, "entry"));
                    Sidebar.AddMenuItem(new MenuItem("Vehicle Exit", IconPaths.CarCheckIcon, "exit"));
                    Sidebar.AddMenuItem(new MenuItem("Reports", IconPaths.ReportIcon, "reports"));
                    Sidebar.AddMenuItem(new MenuItem("Members", IconPaths.PeopleIcon, "members"));
                    Sidebar.AddMenuItem(new MenuItem("Settings", IconPaths.SettingsIcon, "settings"));
                    
                    // Set default active item
                    var dashboardItem = new MenuItem("Dashboard", IconPaths.DashboardIcon, "dashboard");
                    Sidebar.ActiveMenuItem = dashboardItem;
                    
                    // Set up event handlers
                    Sidebar.MenuItemClicked += Sidebar_MenuItemClicked;
                    Sidebar.LogoutClicked += Sidebar_LogoutClicked;
                }
                else
                {
                    logger.Error("Sidebar control not found in XAML");
                    System.Windows.MessageBox.Show("Could not find sidebar control in the application layout.", 
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error setting up sidebar");
                System.Windows.MessageBox.Show($"Error setting up sidebar: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handles menu item clicks in the sidebar.
        /// </summary>
        private void Sidebar_MenuItemClicked(object sender, MenuItem menuItem)
        {
            try
            {
                string tag = menuItem.Tag?.ToString() ?? "";
                
                // Handle navigation based on menu item tag
                switch (tag)
                {
                    case "dashboard":
                        ContentFrame.Navigate(new DashboardPage());
                        break;
                    case "entry":
                        ContentFrame.Navigate(new VehicleEntryPage());
                        break;
                    case "exit":
                        ContentFrame.Navigate(new VehicleExitPage());
                        break;
                    case "reports":
                        // For simplicity, just display a message - in a real app, navigate to a reports page
                        System.Windows.MessageBox.Show("Reports functionality coming soon", "Information", MessageBoxButton.OK);
                        break;
                    case "members":
                        // For simplicity, just display a message - in a real app, navigate to a members page
                        System.Windows.MessageBox.Show("Member management functionality coming soon", "Information", MessageBoxButton.OK);
                        break;
                    case "settings":
                        // For simplicity, just display a message - in a real app, navigate to a settings page
                        System.Windows.MessageBox.Show("Settings functionality coming soon", "Information", MessageBoxButton.OK);
                        break;
                    default:
                        logger.Warn($"Unknown menu item clicked: {tag}");
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling menu item click");
                System.Windows.MessageBox.Show($"Error navigating: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// Handles logout button clicks in the sidebar.
        /// </summary>
        private void Sidebar_LogoutClicked(object sender, EventArgs e)
        {
            try
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(
                    "Are you sure you want to log out?", 
                    "Confirm Logout", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);
                    
                if (result == MessageBoxResult.Yes)
                {
                    // In a real app, perform logout actions here
                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during logout");
                System.Windows.MessageBox.Show($"Error during logout: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 