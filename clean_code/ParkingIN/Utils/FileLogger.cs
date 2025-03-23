using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace ParkingIN.Utils
{
    public class FileLogger : IAppLogger
    {
        private readonly ILogger _logger;
        private readonly string _logPath;

        public FileLogger()
        {
            _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            
            // Ensure logs directory exists
            if (!Directory.Exists(_logPath))
            {
                try
                {
                    Directory.CreateDirectory(_logPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Failed to create logs directory: {ex.Message}");
                    throw;
                }
            }

            try
            {
                // Set up Serilog with consistent format and location
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(
                        Path.Combine(_logPath, "parkingin.log"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                        fileSizeLimitBytes: 10485760, // 10MB per file
                        retainedFileCountLimit: 30 // Keep 30 days of logs
                    )
                    .WriteTo.Console(
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                    )
                    .CreateLogger();

                // Test logging to ensure it works
                _logger.Information("Logging system initialized successfully");
            }
            catch (Exception ex)
            {
                try
                {
                    // Write to emergency file if logging fails
                    string emergencyLog = Path.Combine(_logPath, "emergency_error.log");
                    File.WriteAllText(emergencyLog, 
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Failed to initialize logging system: {ex.Message}\n");
                }
                catch
                {
                    // If everything fails, write to console
                    Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Failed to initialize logging system");
                }
                throw;
            }
        }

        public void Information(string message)
        {
            _logger.Information(message);
        }

        public void Information(string message, Exception ex)
        {
            _logger.Information(ex, message);
        }

        public void Info(string message)
        {
            _logger.Information(message);
        }

        public void Info(string message, Exception ex)
        {
            _logger.Information(ex, message);
        }

        public void Warning(string message)
        {
            _logger.Warning(message);
        }

        public void Warning(string message, Exception ex)
        {
            _logger.Warning(ex, message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Debug(string message, Exception ex)
        {
            _logger.Debug(ex, message);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(ex, message);
        }

        public void LogInfo(string message)
        {
            _logger.Information(message);
        }

        public void LogInfo(string message, Exception ex)
        {
            _logger.Information(ex, message);
        }

        // Support for string format parameters
        public void Debug(string format, params object[] args)
        {
            _logger.Debug(string.Format(format, args));
        }

        public void Info(string format, params object[] args)
        {
            _logger.Information(string.Format(format, args));
        }

        public void Warning(string format, params object[] args)
        {
            _logger.Warning(string.Format(format, args));
        }

        public void Error(string format, params object[] args)
        {
            _logger.Error(string.Format(format, args));
        }

        public void Fatal(string format, params object[] args)
        {
            _logger.Fatal(string.Format(format, args));
        }

        public void Warn(string message)
        {
            Warning(message);
        }
        
        public void Warn(string message, Exception ex)
        {
            Warning(message, ex);
        }
        
        public void Warn(string format, params object[] args)
        {
            Warning(format, args);
        }
    }
}