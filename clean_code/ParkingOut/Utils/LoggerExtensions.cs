using System;
using NLog;

namespace ParkingOut.Utils
{
    /// <summary>
    /// Extension methods for NLog Logger class to provide compatibility with IAppLogger interface
    /// </summary>
    public static class LoggerExtensions
    {
        /// <summary>
        /// Log info message (extension method for NLog.Logger)
        /// </summary>
        public static void LogInfo(this Logger logger, string message)
        {
            logger.Info(message);
        }
        
        /// <summary>
        /// Log info message with exception (extension method for NLog.Logger)
        /// </summary>
        public static void LogInfo(this Logger logger, string message, Exception ex)
        {
            logger.Info(ex, message);
        }
        
        /// <summary>
        /// Log error message (extension method for NLog.Logger)
        /// </summary>
        public static void LogError(this Logger logger, string message)
        {
            logger.Error(message);
        }
        
        /// <summary>
        /// Log error message with exception (extension method for NLog.Logger)
        /// </summary>
        public static void LogError(this Logger logger, string message, Exception ex)
        {
            logger.Error(ex, message);
        }
    }
}