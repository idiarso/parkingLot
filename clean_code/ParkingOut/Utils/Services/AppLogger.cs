using System;
using NLog;

namespace ParkingOut.Utils.Services
{
    public class AppLogger : IAppLogger
    {
        private readonly Logger _logger;

        public AppLogger()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void LogError(string message)
        {
            _logger.Error(message);
        }

        public void LogError(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        public void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public void LogWarning(string message)
        {
            _logger.Warn(message);
        }

        public void LogInfo(string message)
        {
            _logger.Info(message);
        }
    }
}
