using System;

namespace ParkingOut.Utils
{
    /// <summary>
    /// Application logger implementation that wraps NLogLogger
    /// </summary>
    public class AppLogger : IAppLogger
    {
        private readonly NLogLogger _logger;

        /// <summary>
        /// Constructor with logger name
        /// </summary>
        public AppLogger(string loggerName)
        {
            _logger = new NLogLogger(loggerName ?? nameof(AppLogger));
        }

        /// <summary>
        /// Constructor with type
        /// </summary>
        public AppLogger(Type type)
        {
            _logger = new NLogLogger(type ?? typeof(AppLogger));
        }

        /// <summary>
        /// Log debug message
        /// </summary>
        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        /// <summary>
        /// Log debug message with exception
        /// </summary>
        public void Debug(string message, Exception ex)
        {
            _logger.Debug(message, ex);
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public void Info(string message)
        {
            _logger.Info(message);
        }

        /// <summary>
        /// Log info message with exception
        /// </summary>
        public void Info(string message, Exception ex)
        {
            _logger.Info(message, ex);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void Warning(string message)
        {
            _logger.Warning(message);
        }

        /// <summary>
        /// Log warning message with exception
        /// </summary>
        public void Warning(string message, Exception ex)
        {
            _logger.Warning(message, ex);
        }

        /// <summary>
        /// Log error message
        /// </summary>
        public void Error(string message)
        {
            _logger.Error(message);
        }

        /// <summary>
        /// Log error message with exception
        /// </summary>
        public void Error(string message, Exception ex)
        {
            _logger.Error(message, ex);
        }

        /// <summary>
        /// Log fatal error message
        /// </summary>
        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        /// <summary>
        /// Log fatal error message with exception
        /// </summary>
        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(message, ex);
        }
    }
} 