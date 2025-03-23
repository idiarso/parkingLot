using System;
using NLog;
using ParkingOut.Services;

namespace ParkingOut.Services.Implementations
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
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        /// <inheritdoc/>
        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        /// <inheritdoc/>
        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        /// <inheritdoc/>
        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.Error(ex, message, args);
        }
    }
}