using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using NLog;
using NLog.Config;
using NLog.Targets;
using ParkingOut.UI;

namespace ParkingOut
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="App"/> class.
        /// </summary>
        public App()
        {
            // Subscribe to unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            // Configure logging
            ConfigureLogging();
            
            logger.Info("Application starting...");
        }

        /// <summary>
        /// Handles the Startup event of the Application.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            try
            {
                logger.Debug("Initializing database tables");
                
                // Pastikan tabel users ada dan user admin terbuat
                try
                {
                    // Pastikan database sudah terhubung
                    if (ParkingOut.Utils.Database.IsDatabaseAvailable)
                    {
                        logger.Info("Ensuring users table exists");
                        ParkingOut.Utils.UserManager.Instance.EnsureUsersTableExists();
                        logger.Info("Users table initialized successfully");
                    }
                    else
                    {
                        logger.Warn("Database not available, skipping users table initialization");
                    }
                }
                catch (Exception dbEx)
                {
                    logger.Error(dbEx, "Error initializing users table");
                    MessageBox.Show($"Error initializing database tables: {dbEx.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                logger.Debug("Initializing main window");
                
                // Create and show the main window
                MainWindow = new MainWindow();
                MainWindow.Show();
                
                logger.Info("Application started successfully");
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "Failed to start application");
                MessageBox.Show($"Failed to start application: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }

        /// <summary>
        /// Handles the Exit event of the Application.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            
            logger.Info("Application exiting...");
            
            // Clean up resources
            LogManager.Shutdown();
        }

        /// <summary>
        /// Handles unhandled exceptions in the current AppDomain.
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            logger.Fatal(exception, "Unhandled AppDomain exception");
            
            if (e.IsTerminating)
            {
                MessageBox.Show($"A fatal error occurred and the application must close: {exception?.Message}",
                    "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles unhandled exceptions in the dispatcher.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            logger.Error(e.Exception, "Unhandled dispatcher exception");
            
            MessageBox.Show($"An error occurred: {e.Exception.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            
            e.Handled = true;
        }

        /// <summary>
        /// Configures NLog for application logging.
        /// </summary>
        private static void ConfigureLogging()
        {
            try
            {
                // Create NLog configuration
                var config = new LoggingConfiguration();
                
                // Create targets
                var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                Directory.CreateDirectory(logDirectory);
                
                // File target for all logs
                var logFile = new FileTarget("file")
                {
                    FileName = Path.Combine(logDirectory, "ParkingOut_${shortdate}.log"),
                    Layout = "${longdate}|${level:uppercase=true}|${logger}|${message}|${exception:format=tostring}"
                };
                
                // Console target for debugging
                var consoleTarget = new ConsoleTarget("console")
                {
                    Layout = "${date:format=HH\\:mm\\:ss}|${level:uppercase=true}|${message}|${exception:format=message}"
                };
                
                // Add targets to configuration
                config.AddTarget(logFile);
                config.AddTarget(consoleTarget);
                
                // Add rules
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, consoleTarget);
                
                // Apply configuration
                LogManager.Configuration = config;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to configure logging: {ex.Message}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}