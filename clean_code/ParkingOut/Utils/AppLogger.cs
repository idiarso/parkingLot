using System;
using NLog;
using ParkingOut.Utils;

namespace ParkingOut.Utils
{
    /// <summary>
    /// Implementation of IAppLogger using NLog.
    /// </summary>
    public class AppLogger : IAppLogger
    {
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLogger"/> class.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        public AppLogger(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLogger"/> class.
        /// </summary>
        /// <param name="type">The type to use for the logger name.</param>
        public AppLogger(Type type)
        {
            _logger = LogManager.GetLogger(type.Name);
        }

        /// <inheritdoc/>
        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        /// <inheritdoc/>
        public void Debug(string message, Exception ex)
        {
            _logger.Debug(ex, message);
        }

        /// <inheritdoc/>
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        /// <inheritdoc/>
        public void Info(string message)
        {
            _logger.Info(message);
        }

        /// <inheritdoc/>
        public void Info(string message, Exception ex)
        {
            _logger.Info(ex, message);
        }

        /// <inheritdoc/>
        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        /// <inheritdoc/>
        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        /// <inheritdoc/>
        public void Warn(string message, Exception ex)
        {
            _logger.Warn(ex, message);
        }

        /// <inheritdoc/>
        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        /// <inheritdoc/>
        public void Error(string message)
        {
            _logger.Error(message);
        }

        /// <inheritdoc/>
        public void Error(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        /// <inheritdoc/>
        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        /// <inheritdoc/>
        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        /// <inheritdoc/>
        public void Fatal(string message, Exception ex)
        {
            _logger.Fatal(ex, message);
        }

        /// <inheritdoc/>
        public void Fatal(string message, params object[] args)
        {
            _logger.Fatal(message, args);
        }
        
        /// <inheritdoc/>
        public void LogInfo(string message)
        {
            _logger.Info(message);
        }
        
        /// <inheritdoc/>
        public void LogInfo(string message, Exception ex)
        {
            _logger.Info(ex, message);
        }
        
        /// <inheritdoc/>
        public void Warning(string message)
        {
            _logger.Warn(message);
        }
        
        /// <inheritdoc/>
        public void Warning(string message, Exception ex)
        {
            _logger.Warn(ex, message);
        }
        
        /// <inheritdoc/>
        public void LogError(string message)
        {
            _logger.Error(message);
        }
        
        /// <inheritdoc/>
        public void LogError(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }
    }
}