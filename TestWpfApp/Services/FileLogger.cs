using System;
using System.IO;
using TestWpfApp.Services.Interfaces;

namespace TestWpfApp.Services
{
    public class FileLogger : IAppLogger
    {
        private readonly string _logFilePath;

        public FileLogger()
        {
            _logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TestWpfApp", "logs", "app.log");
            EnsureLogDirectoryExists();
        }

        private void EnsureLogDirectoryExists()
        {
            var logDir = Path.GetDirectoryName(_logFilePath);
            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }
        }

        private void WriteLog(string level, string message, Exception ex = null)
        {
            try
            {
                var logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}";
                if (ex != null)
                {
                    logMessage += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
                }

                File.AppendAllText(_logFilePath, logMessage + Environment.NewLine);
            }
            catch (Exception loggingException)
            {
                // If logging fails, we don't want to crash the application
                Console.WriteLine($"Failed to write log: {loggingException.Message}");
            }
        }

        public void Debug(string message)
        {
            WriteLog("DEBUG", message);
        }

        public void Debug(string message, Exception ex)
        {
            WriteLog("DEBUG", message, ex);
        }

        public void Info(string message)
        {
            WriteLog("INFO", message);
        }

        public void Info(string message, Exception ex)
        {
            WriteLog("INFO", message, ex);
        }

        public void Warning(string message)
        {
            WriteLog("WARNING", message);
        }

        public void Warning(string message, Exception ex)
        {
            WriteLog("WARNING", message, ex);
        }

        public void Error(string message)
        {
            WriteLog("ERROR", message);
        }

        public void Error(string message, Exception ex)
        {
            WriteLog("ERROR", message, ex);
        }

        public void Fatal(string message)
        {
            WriteLog("FATAL", message);
        }

        public void Fatal(string message, Exception ex)
        {
            WriteLog("FATAL", message, ex);
        }
    }
}
