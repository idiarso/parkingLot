using System;
using System.IO;
using System.Windows;
using NLog;

namespace ParkingOut
{
    /// <summary>
    /// Main entry point for the application.
    /// </summary>
    public class Program
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Application entry point.
        /// This is a workaround for using both WPF and WinForms in the same application,
        /// allowing for a phased migration from WinForms to WPF.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            try
            {
                // Set up exception handling
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                
                // Initialize application
                logger.Info("Application starting");
                var app = new App();
                app.Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Application failed to start");
                System.Windows.MessageBox.Show($"Fatal application error: {ex.Message}", 
                    "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles unhandled exceptions at the application domain level.
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            logger.Fatal(exception, "Unhandled exception");
            
            System.Windows.MessageBox.Show($"A critical error occurred: {exception?.Message ?? "Unknown error"}", 
                "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Ensures that required resource directories exist.
        /// </summary>
        private static void EnsureResourceDirectoriesExist()
        {
            try
            {
                string resourcesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                string imagesDir = Path.Combine(resourcesDir, "Images");
                string iconsDir = Path.Combine(resourcesDir, "Icons");
                string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

                if (!Directory.Exists(resourcesDir))
                    Directory.CreateDirectory(resourcesDir);

                if (!Directory.Exists(imagesDir))
                    Directory.CreateDirectory(imagesDir);

                if (!Directory.Exists(iconsDir))
                    Directory.CreateDirectory(iconsDir);
                    
                if (!Directory.Exists(logsDir))
                    Directory.CreateDirectory(logsDir);
                    
                logger.Info("Resource directories created successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error creating resource directories");
                throw new Exception("Failed to create required directories", ex);
            }
        }
    }
} 