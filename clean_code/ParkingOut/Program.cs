using System;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Globalization;
using SimpleParkingAdmin.Utils;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    static class Program
    {
        private static readonly IAppLogger _logger = CustomLogManager.GetLogger();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Configure Serilog
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Information()
                    .WriteTo.Console()
                    .WriteTo.File("logs/log-.txt", 
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                _logger.Information("Starting Parking Management System...");

                Application.SetHighDpiMode(HighDpiMode.SystemAware);
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Initialize database connection
                if (!Database.TestConnection())
                {
                    MessageBox.Show("Failed to connect to database. Please check your connection settings.",
                        "Database Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Show login form
                using (var loginForm = new LoginForm())
                {
                    if (loginForm.ShowDialog() == DialogResult.OK)
                    {
                        Application.Run(new DashboardForm(LoginForm.CurrentUser));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Application terminated unexpectedly", ex);
                MessageBox.Show($"An unexpected error occurred: {ex.Message}",
                    "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        
        private static void ConfigureLogger()
        {
            // Create logs directory if it doesn't exist
            string logDir = Path.Combine(Application.StartupPath, "logs");
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
            
            // Log application start
            _logger.Information("Application started");
        }
        
        private static void InitializeDatabase()
        {
            // Ensure database structure
            if (!Database.EnsureDatabaseStructure())
            {
                throw new Exception("Failed to initialize database structure");
            }
            
            // Explicitly check for the tarif_khusus table
            if (!Database.TableExists("tarif_khusus"))
            {
                // Try to create the table manually
                try
                {
                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS `tarif_khusus` (
                          `id` int(11) NOT NULL AUTO_INCREMENT,
                          `jenis_kendaraan` varchar(50) NOT NULL,
                          `jenis_tarif` varchar(50) NOT NULL,
                          `jam_mulai` time DEFAULT NULL,
                          `jam_selesai` time DEFAULT NULL,
                          `hari` varchar(100) DEFAULT NULL,
                          `tarif_flat` decimal(10,2) DEFAULT NULL,
                          `deskripsi` varchar(255) DEFAULT NULL,
                          `status` tinyint(1) DEFAULT 1,
                          `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                          `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                          PRIMARY KEY (`id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";
                    Database.ExecuteNonQuery(createTable);
                    
                    _logger.Information("tarif_khusus table created successfully");
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to create tarif_khusus table", ex);
                    throw;
                }
            }
            
            // Load schema from SQL file as a fallback
            try 
            {
                string schemaPath = Path.Combine(Application.StartupPath, "Database", "schema_mysql.sql");
                if (File.Exists(schemaPath))
                {
                    string schemaSql = File.ReadAllText(schemaPath);
                    // Split by SQL command terminator and execute each command
                    string[] commands = schemaSql.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string command in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            try
                            {
                                Database.ExecuteNonQuery(command);
                            }
                            catch (Exception cmdEx)
                            {
                                // Just log but continue with other commands
                                _logger.Warning($"Error executing schema command: {cmdEx.Message}");
                            }
                        }
                    }
                    _logger.Information("Schema loaded from SQL file");
                }
            }
            catch (Exception ex)
            {
                _logger.Warning($"Error loading schema from SQL file: {ex.Message}");
                // Not throwing here as we already tried direct creation
            }
        }
        
        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            _logger.Error("Unhandled Thread Exception", e.Exception);
            ShowErrorMessage(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                _logger.Error("Unhandled AppDomain Exception", ex);
                ShowErrorMessage(ex);
            }
        }

        private static void ShowErrorMessage(Exception ex)
        {
            try
            {
                string errorMessage = $"An error has occurred:\n\n{ex.Message}";
                
                if (ex.InnerException != null)
                {
                    errorMessage += $"\n\nInner Exception: {ex.InnerException.Message}";
                }
                
                MessageBox.Show(errorMessage, "Application Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch
            {
                // If showing the message fails, at least we tried
            }
        }
    }
} 