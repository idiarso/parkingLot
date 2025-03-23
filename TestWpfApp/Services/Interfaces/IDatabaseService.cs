using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestWpfApp.Models;

namespace TestWpfApp.Services.Interfaces
{
    public interface IDatabaseService
    {
        bool IsConnected { get; }
        Task<bool> ConnectAsync();
        Task<bool> IsCameraOnlineAsync();
        Task<bool> IsPrinterReadyAsync();
        Task<ParkingStatistics> GetParkingStatisticsAsync();
        Task<List<ActivityLogItem>> GetRecentActivityLogsAsync();
        Task<bool> LogActivityAsync(ActivityLogItem activity);
        Task<bool> EnsureUserTableExistsAsync();
        Task<User?> AuthenticateUserAsync(string username, string password);
        Task<bool> UpdateLastLoginAsync(User user);
    }
}
