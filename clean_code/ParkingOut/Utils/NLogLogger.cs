using System;
using NLog;

namespace ParkingOut.Utils
{
    /// <summary>
    /// Implementation of logging using NLog
    /// </summary>
    public class NLogLogger
    {
        private readonly Logger _logger;

        /// <summary>
        /// Constructor that takes a name for the logger
        /// </summary>
        public NLogLogger(string name)
        {
            _logger = LogManager.GetLogger(name);
        }

        /// <summary>
        /// Constructor that takes a type
        /// </summary>
        public NLogLogger(Type type)
        {
            _logger = LogManager.GetLogger(type.FullName);
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
            _logger.Debug(ex, message);
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
            _logger.Info(ex, message);
        }

        /// <summary>
        /// Log warning message
        /// </summary>
        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        /// <summary>
        /// Log warning message with exception
        /// </summary>
        public void Warning(string message, Exception ex)
        {
            _logger.Warn(ex, message);
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
            _logger.Error(ex, message);
        }

        /// <summary>
        /// Log fatal message
        /// </summary>
        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        /// <summary>
        /// Log fatal message with exception
        /// </summary>
        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(ex, message);
        }
    }
} 