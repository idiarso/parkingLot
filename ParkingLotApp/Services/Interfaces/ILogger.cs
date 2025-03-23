using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLotApp.Models;

namespace ParkingLotApp.Services.Interfaces
{
    public interface ILogger
    {
        Task LogInfoAsync(string message);
        Task LogWarningAsync(string message);
        Task LogErrorAsync(string message, Exception? ex = null);
        Task LogLoginAttemptAsync(string username, bool success);
        Task<List<Log>> GetRecentLogsAsync(int count = 100);
    }
} 