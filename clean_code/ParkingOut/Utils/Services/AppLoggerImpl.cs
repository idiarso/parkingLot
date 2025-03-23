using System;
using NLog;

namespace ParkingOut.Utils.Services
{
    /// <summary>
    /// Implementation of IAppLogger using NLog
    /// </summary>
    public class AppLoggerImpl : IAppLogger
    {
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLoggerImpl"/> class.
        /// </summary>
        public AppLoggerImpl()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLoggerImpl"/> class with a specific logger name.
        /// </summary>
        /// <param name="loggerName">The name of the logger</param>
        public AppLoggerImpl(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
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
        public void LogError(string message)
        {
            Error(message);
        }

        /// <inheritdoc/>
        public void LogError(string message, Exception ex)
        {
            Error(message, ex);
        }

        /// <inheritdoc/>
        public void Warn(string message)
        {
            Warning(message);
        }

        /// <inheritdoc/>
        public void Warn(string message, Exception ex)
        {
            Warning(message, ex);
        }
    }
}