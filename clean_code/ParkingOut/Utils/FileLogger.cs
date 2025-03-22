using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace ParkingOut.Utils
{
    public class FileLogger : IAppLogger
    {
        private readonly ILogger _logger;

        public FileLogger()
        {
            _logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("logs/app.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .CreateLogger();
        }

        public void Information(string message)
        {
            _logger.Information(message);
        }

        public void Info(string message)
        {
            _logger.Information(message);
        }

        public void Warning(string message)
        {
            _logger.Warning(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(ex, message);
        }
    }
} 