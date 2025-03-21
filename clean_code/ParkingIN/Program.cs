using System;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using ParkingIN.Utils;
using Serilog;

namespace ParkingIN
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Ensure logs directory exists
                string logsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                }

                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Console() // Add console logging for debugging
                    .WriteTo.File(Path.Combine(logsDirectory, "app.log"), rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                Log.Information("Application starting...");

                // Check for simulator files for direct bypass
                bool bypassFromFile = File.Exists("simulator_bypass.txt");
                bool noDatabaseMode = File.Exists("no_database.txt") || args.Any(arg => arg.ToLower() == "--no-database");
                
                // Check for simulator mode
                bool simulatorMode = bypassFromFile || 
                                     args != null && (args.Any(arg => arg.ToLower() == "--simulator") || 
                                                     args.Any(arg => arg.ToLower() == "--skip-login"));
                
                if (simulatorMode)
                {
                    Log.Information("Starting in simulator mode - launching Microcontroller Simulator directly");
                    
                    // Setup DB connection bypass if in no-database mode
                    if (noDatabaseMode)
                    {
                        Log.Information("No database mode enabled - bypassing database connections");
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
                MessageBox.Show($"An unexpected error occurred: {ex.Message}\n\nStack trace: {ex.StackTrace}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                try
                {
                    Log.Error(ex, "Unhandled exception in Main");
                }
                catch
                {
                    // Fallback if logging itself fails
                    File.WriteAllText("error.txt", $"Critical error: {ex.Message}\n{ex.StackTrace}");
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
    }
}