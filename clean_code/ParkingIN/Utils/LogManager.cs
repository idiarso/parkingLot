using System;
using System.IO;
using Microsoft.Extensions.Logging; // Untuk ILogger dari Microsoft
using Serilog; // Untuk konfigurasi Serilog
using Serilog.Extensions.Logging; // Untuk integrasi Serilog dengan Microsoft.Extensions.Logging

namespace ParkingIN.Utils
{
    public static class LogManager
    {
        private static Microsoft.Extensions.Logging.ILogger _logger; // Tentukan namespace eksplisit
        private static readonly string LogDirectory = "Logs";
        private static readonly string LogFilePath = Path.Combine(LogDirectory, "log-.txt");

        static LogManager()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }

                var serilogLogger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(LogFilePath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                // Convert Serilog logger ke Microsoft.Extensions.Logging.ILogger
                var loggerFactory = new LoggerFactory();
                loggerFactory.AddSerilog(serilogLogger);
                _logger = loggerFactory.CreateLogger("ParkingIN");

                LogInfo("LogManager initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize logging: {ex.Message}");
            }
        }

        public static Microsoft.Extensions.Logging.ILogger GetLogger() // Tentukan namespace eksplisit
        {
            return _logger;
        }

        public static void LogError(string message, Exception ex = null)
        {
            if (ex != null)
                GetLogger().LogError(ex, message);
            else
                GetLogger().LogError(message);
        }

        public static void LogWarning(string message)
        {
            GetLogger().LogWarning(message);
        }

        public static void LogInfo(string message)
        {
            GetLogger().LogInformation(message);
        }

        public static void LogDebug(string message)
        {
            GetLogger().LogDebug(message);
        }
    }
}