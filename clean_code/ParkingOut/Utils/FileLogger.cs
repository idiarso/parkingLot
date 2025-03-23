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
        
        public void LogInfo(string message)
        {
            Info(message);
        }
        
        public void LogInfo(string message, Exception ex)
        {
            Info(message, ex);
        }
    }
}