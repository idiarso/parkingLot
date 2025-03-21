using System;
using System.IO;
using System.Reflection;
using System.Configuration;

namespace SimpleParkingAdmin
{
    public static class Logger
    {
        private static readonly string LogFilePath;
        private static readonly object LockObject = new object();

        static Logger()
        {
            try
            {
                string logPath = ConfigurationManager.AppSettings["LogPath"] ?? "Logs";
                string logFileName = "app.log";
                
                // Create logs directory if it doesn't exist
                if (!Directory.Exists(logPath))
                {
                    Directory.CreateDirectory(logPath);
                }
                
                LogFilePath = Path.Combine(logPath, logFileName);
            }
            catch (Exception ex)
            {
                // Can't use our own logger here since it's not initialized yet
                Console.WriteLine($"Error initializing logger: {ex.Message}");
                
                // Fallback to current directory
                LogFilePath = "application.log";
            }
        }

        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARNING", message);
        }

        public static void Error(string message)
        {
            Log("ERROR", message);
        }

        public static void Debug(string message)
        {
            Log("DEBUG", message);
        }

        public static void LogException(Exception ex, string context = "")
        {
            string message = $"Context: {context}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}";
            
            if (ex.InnerException != null)
            {
                message += $"\nInnerException: {ex.InnerException.Message}\nInnerStackTrace: {ex.InnerException.StackTrace}";
            }
            
            Error(message);
        }

        private static void Log(string level, string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string logMessage = $"[{timestamp}] [{level}] {message}";
                
                lock (LockObject)
                {
                    File.AppendAllText(LogFilePath, logMessage + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                // Last-resort fallback if logging itself fails
                Console.WriteLine($"Error writing to log file: {ex.Message}");
                Console.WriteLine($"Original message: {message}");
            }
        }
    }
} 