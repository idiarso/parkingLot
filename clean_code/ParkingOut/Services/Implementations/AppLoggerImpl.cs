using System;
using NLog;
using ParkingOut.Utils;

namespace ParkingOut.Services.Implementations
{
    public class AppLoggerImpl : IAppLogger
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception ex, string message = null)
        {
            if (string.IsNullOrEmpty(message))
            {
                _logger.Error(ex);
            }
            else
            {
                _logger.Error(ex, message);
            }
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }
    }
}