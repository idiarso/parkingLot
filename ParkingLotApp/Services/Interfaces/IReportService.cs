using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLotApp.Models;

namespace ParkingLotApp.Services.Interfaces
{
    public interface IReportService
    {
        Task<List<ParkingActivity>> GetActivityReportAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOccupancyReportAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetSummaryReportAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueTodayAsync();
        Task<decimal?> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    }
} 