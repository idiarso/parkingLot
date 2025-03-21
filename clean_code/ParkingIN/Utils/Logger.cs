using System;
using System.IO;
using System.Reflection;
using System.Configuration;
using Serilog;

namespace ParkingIN.Utils
{
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();
        private static readonly ILogger _logger;

        static Logger()
        {
            try
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                string logFileName = "app.log";
                
                // Create logs directory if it doesn't exist
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                
                LogFilePath = Path.Combine(logPath, logFileName);
                
                // Initialize Serilog
                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(LogFilePath, 
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();
                
                Info("Logger initialized successfully");
            }
            catch (Exception ex)
            {
                // Can't use our own logger here since it's not initialized yet
                Console.WriteLine($"Error initializing logger: {ex.Message}");
                
                // Create a minimal console logger
                _logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .CreateLogger();
            }
        }

        public static void Info(string message)
        {
            try
            {
                _logger.Information(message);
            }
            catch
            {
                Log("INFO", message);
            }
        }

        public static void Warning(string message)
        {
            try
            {
                _logger.Warning(message);
            }
            catch
            {
                Log("WARNING", message);
            }
        }

        public static void Error(string message)
        {
            try
            {
                _logger.Error(message);
            }
            catch
            {
                Log("ERROR", message);
            }
        }

        public static void Debug(string message)
        {
            try
            {
                _logger.Debug(message);
            }
            catch
            {
                Log("DEBUG", message);
            }
        }

        public static void LogException(Exception ex, string context = "")
        {
            string message = $"Context: {context}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
            
            if (ex.InnerException != null)
            {
                message += $"\nInnerException: {ex.InnerException.Message}\nInnerStackTrace: {ex.InnerException.StackTrace}";
            }
            
            Error(message);
        }

        public static void Error(Exception ex, string message)
        {
            try
            {
                _logger.Error(ex, message);
            }
            catch
            {
                Log("ERROR", $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        public static void Fatal(string message)
        {
            try
            {
                _logger.Fatal(message);
            }
            catch
            {
                Log("FATAL", message);
            }
        }

        public static void Fatal(Exception ex, string message)
        {
            try
            {
                _logger.Fatal(ex, message);
            }
            catch
            {
                Log("FATAL", $"{message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
        }

        private static void Log(string level, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logMessage = $"[{timestamp}] [{level}] {message}";
                
                lock (LockObject)
                {
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Last-resort fallback if logging itself fails
                Console.WriteLine($"Error writing to log file: {ex.Message}");
                Console.WriteLine($"Original message: {message}");
            }
        }
    }
} 