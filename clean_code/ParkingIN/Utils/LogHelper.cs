using System;
using System.Collections.Generic;
using Npgsql;

namespace ParkingIN.Utils
{
    public static class LogHelper
    {
        public static void AddLog(string action, string description, int? userId = null)
        {
            try
            {
                string query = @"
                    INSERT INTO t_log (user_id, action, description, created_at)
                    VALUES (@userId, @action, @description, @createdAt)";

                var parameters = new Dictionary<string, object>
                {
                    { "userId", userId ?? (object)DBNull.Value },
                    { "action", action.ToUpper() },
                    { "description", description },
                    { "createdAt", DateTime.Now }
                };

                SimpleDatabaseHelper.GetData(query, parameters);
            }
            catch (Exception ex)
            {
                // Log to file if database logging fails
                string logDirectory = System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, "logs");
                
                if (!System.IO.Directory.Exists(logDirectory))
                {
                    System.IO.Directory.CreateDirectory(logDirectory);
                }

                string logFile = System.IO.Path.Combine(logDirectory, "error.log");
                System.IO.File.AppendAllText(logFile, 
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - Failed to add log: {ex.Message}\n");
            }
        }

        public static void LogUserAction(int userId, string action, string details)
        {
            AddLog(action, details, userId);
        }

        public static void LogSystemAction(string action, string details)
        {
            AddLog(action, details);
        }

        public static void LogError(string source, Exception ex)
        {
            string description = $"Error in {source}: {ex.Message}";
            if (ex.InnerException != null)
            {
                description += $" | Inner: {ex.InnerException.Message}";
            }
            AddLog("ERROR", description);
        }
    }
}
