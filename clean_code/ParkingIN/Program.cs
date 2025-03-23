using System;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using ParkingIN.Utils;
using Serilog;
using Serilog.Events;

namespace ParkingIN
{
    static class Program
    {
        private static IAppLogger _logger;

        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Set up global exception handlers
                Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                // Initialize logging
                _logger = CustomLogManager.GetLogger();
                _logger.Info("Application starting...");

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Check for simulator files for direct bypass
                bool bypassFromFile = File.Exists("simulator_bypass.txt");
                bool noDatabaseMode = File.Exists("no_database.txt") || args.Any(arg => arg.ToLower() == "--no-database");
                
                // Check for simulator mode
                bool simulatorMode = bypassFromFile || 
                                     args != null && (args.Any(arg => arg.ToLower() == "--simulator") || 
                                                     args.Any(arg => arg.ToLower() == "--skip-login"));

                // Test database connection before proceeding
                if (!noDatabaseMode && !Database.TestConnection())
                {
                    _logger.Error("Failed to connect to database");
                    MessageBox.Show("Failed to connect to database. Please check your connection settings and try again.",
                        "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (simulatorMode)
                {
                    _logger.Info("Starting in simulator mode - launching Microcontroller Simulator directly");
                    
                    // Setup DB connection bypass if in no-database mode
                    if (noDatabaseMode)
                    {
                        _logger.Info("No database mode enabled - bypassing database connections");
                        // Here you would set any necessary configurations to bypass DB checks
                    }
                    
                    Application.Run(new MicrocontrollerFormExample());
                }
                else
                {
                    // Create and run the main application form
                    using (var loginForm = new LoginForm())
                    {
                        Application.Run(loginForm);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"An unexpected error occurred: {ex.Message}";
                string detailedError = $"{errorMessage}\n\nStack trace: {ex.StackTrace}";
                
                try
                {
                    _logger?.Error($"Unhandled exception in Main: {ex.Message}", ex);
                    
                    // Write to emergency log file if logger fails
                    string emergencyLog = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory, 
                        "logs",
                        "emergency_error.log"
                    );
                    File.AppendAllText(emergencyLog, 
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {detailedError}\n\n");
                }
                catch
                {
                    // Last resort - show error in message box
                    MessageBox.Show(detailedError, "Critical Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                try
                {
                    Log.CloseAndFlush();
                }
                catch
                {
                    // Ignore errors during log closing
                }
            }
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception, "Thread Exception");
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex, "Unhandled Exception");
            }
        }

        private static void HandleException(Exception ex, string context)
        {
            try
            {
                string message = $"{context}: {ex.Message}";
                string detail = $"Stack trace:\n{ex.StackTrace}";

                if (ex.InnerException != null)
                {
                    detail += $"\n\nInner exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }

                _logger?.Error(message, ex);

                MessageBox.Show($"{message}\n\n{detail}", "Application Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // If logging fails, still show the error
                MessageBox.Show($"{ex.Message}\n\n{ex.StackTrace}", "Application Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}