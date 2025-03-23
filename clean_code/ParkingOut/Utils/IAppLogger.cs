using System;

namespace ParkingOut.Utils
{
    /// <summary>
    /// Interface for application logging
    /// </summary>
    public interface IAppLogger
    {
        /// <summary>
        /// Log debug message
        /// </summary>
        void Debug(string message);
        
        /// <summary>
        /// Log debug message with exception
        /// </summary>
        void Debug(string message, Exception ex);
        
        /// <summary>
        /// Log info message
        /// </summary>
        void Info(string message);
        
        /// <summary>
        /// Log info message with exception
        /// </summary>
        void Info(string message, Exception ex);
        
        /// <summary>
        /// Log warning message
        /// </summary>
        void Warning(string message);
        
        /// <summary>
        /// Log warning message with exception
        /// </summary>
        void Warning(string message, Exception ex);
        
        /// <summary>
        /// Log error message
        /// </summary>
        void Error(string message);
        
        /// <summary>
        /// Log error message with exception
        /// </summary>
        void Error(string message, Exception ex);
        
        /// <summary>
        /// Log fatal error message
        /// </summary>
        void Fatal(string message);
        
        /// <summary>
        /// Log fatal error message with exception
        /// </summary>
        void Fatal(string message, Exception ex);
        
        /// <summary>
        /// Log error message (alternative method)
        /// </summary>
        void LogError(string message);
        
        /// <summary>
        /// Log error message with exception (alternative method)
        /// </summary>
        void LogError(string message, Exception ex);
        
        /// <summary>
        /// Log warning message (alternative name)
        /// </summary>
        void Warn(string message);
        
        /// <summary>
        /// Log warning message with exception (alternative name)
        /// </summary>
        void Warn(string message, Exception ex);
        
        /// <summary>
        /// Log info message (alternative method)
        /// </summary>
        void LogInfo(string message);
        
        /// <summary>
        /// Log info message with exception (alternative method)
        /// </summary>
        void LogInfo(string message, Exception ex);
    }
}
