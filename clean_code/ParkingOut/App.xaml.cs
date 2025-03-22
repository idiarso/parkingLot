using System;
using System.IO;
using System.Windows;
using NLog;

namespace ParkingOut
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                // Ensure resources directory exists
                EnsureResourcesDirectoryExists();
                
                // Initialize the main window
                // We're not setting StartupUri in XAML to have more control over startup
                var mainWindow = new ParkingOut.UI.MainWindow();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during application startup");
                System.Windows.MessageBox.Show($"Error starting application: {ex.Message}", 
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// Ensures that the resources directory exists.
        /// </summary>
        private void EnsureResourcesDirectoryExists()
        {
            try
            {
                // Create resources directories if they don't exist
                string resourcesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                string imagesDir = Path.Combine(resourcesDir, "Images");
                string iconsDir = Path.Combine(resourcesDir, "Icons");
                
                if (!Directory.Exists(resourcesDir))
                    Directory.CreateDirectory(resourcesDir);
                
                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);
                
                if (!Directory.Exists(iconsDir))
                    Directory.CreateDirectory(iconsDir);
                
                logger.Info("Resource directories created successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to create resources directory");
                throw new Exception($"Failed to create resources directory: {ex.Message}", ex);
            }
        }
    }
} 