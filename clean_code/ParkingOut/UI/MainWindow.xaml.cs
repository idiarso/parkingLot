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
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            try
            {
                logger.Info("Initializing MainWindow component");
                System.Diagnostics.Debug.WriteLine("Initializing MainWindow component");
                InitializeComponent();
                logger.Info("MainWindow component initialized successfully");
                System.Diagnostics.Debug.WriteLine("MainWindow component initialized successfully");
            }
            catch (Exception ex)
            {
                // Use the MessageHelper to display error
                MessageHelper.ShowError($"Error initializing main window: {ex.Message}", "Initialization Error");
                MessageHelper.ShowError($"Inner Exception: {ex.InnerException?.Message}", "Initialization Error");
                
                logger.Error(ex, "Failed to initialize MainWindow component");
            }
        }
    }
} 