using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Configuration;

namespace ParkingIN.Utils
{
    public static class LogManager
    {
        private static ILogger _logger;
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

                _logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(LogFilePath,
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .CreateLogger();

                _logger.Information("LogManager initialized successfully");
            }
            catch (Exception ex)
            {
                // If we can't initialize logging, write to console
                Console.WriteLine($"Failed to initialize logging: {ex.Message}");
            }
        }

        public static ILogger GetLogger()
        {
            return _logger;
        }

        public static void LogError(string message, Exception ex = null)
        {
            GetLogger().Error(ex, message);
        }

        public static void LogWarning(string message)
        {
            GetLogger().Warning(message);
        }

        public static void LogInfo(string message)
        {
            GetLogger().Information(message);
        }

        public static void LogDebug(string message)
        {
            GetLogger().Debug(message);
        }
    }
} 