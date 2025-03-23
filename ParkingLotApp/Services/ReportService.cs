using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Data;
using ParkingLotApp.Models;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.Services
{
    public class ReportService : IReportService
    {
        private readonly ParkingDbContext _dbContext;

        public ReportService(ParkingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<ParkingActivity>> GetDailyReportAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate)
                .OrderBy(a => a.Time)
                .ToListAsync();
        }

        public async Task<List<ParkingActivity>> GetMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate)
                .OrderBy(a => a.Time)
                .ToListAsync();
        }

        public async Task<List<ParkingActivity>> GetYearlyReportAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate)
                .OrderBy(a => a.Time)
                .ToListAsync();
        }

        public async Task<decimal?> GetDailyRevenueAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate && a.Action == "Exit")
                .SumAsync(a => a.Fee);
        }

        public async Task<decimal?> GetMonthlyRevenueAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate && a.Action == "Exit")
                .SumAsync(a => a.Fee);
        }

        public async Task<decimal?> GetYearlyRevenueAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate && a.Action == "Exit")
                .SumAsync(a => a.Fee);
        }

        public async Task<int> GetDailyVehicleCountAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate && a.Action == "Entry")
                .CountAsync();
        }

        public async Task<int> GetMonthlyVehicleCountAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate && a.Action == "Entry")
                .CountAsync();
        }

        public async Task<int> GetYearlyVehicleCountAsync(int year)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time < endDate && a.Action == "Entry")
                .CountAsync();
        }

        public async Task<List<ParkingActivity>> GetActivityReportAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time <= endDate)
                .OrderBy(a => a.Time)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            var activities = await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time <= endDate && a.Action == "Exit" && a.Fee.HasValue)
                .ToListAsync();

            return activities
                .GroupBy(a => a.Time.Date)
                .ToDictionary(
                    g => g.Key.ToString("yyyy-MM-dd"),
                    g => g.Sum(a => a.Fee ?? 0)
                );
        }

        public async Task<Dictionary<string, int>> GetOccupancyReportAsync(DateTime startDate, DateTime endDate)
        {
            var activities = await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time <= endDate)
                .ToListAsync();

            return activities
                .GroupBy(a => a.Time.Hour)
                .ToDictionary(
                    g => g.Key.ToString("D2") + ":00",
                    g => g.Count(a => a.Action == "Entry") - g.Count(a => a.Action == "Exit")
                );
        }

        public async Task<Dictionary<string, object>> GetSummaryReportAsync(DateTime startDate, DateTime endDate)
        {
            var activities = await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time <= endDate)
                .ToListAsync();

            var totalVehicles = activities.Count(a => a.Action == "Entry");
            var totalRevenue = activities.Where(a => a.Action == "Exit").Sum(a => a.Fee ?? 0);
            var averageDuration = activities
                .Where(a => a.Action == "Exit" && !string.IsNullOrEmpty(a.Duration))
                .Select(a => a.Duration)
                .DefaultIfEmpty("0 hours")
                .Average(d => ParseDuration(d));

            var vehicleTypes = activities
                .Where(a => a.Action == "Entry")
                .GroupBy(a => a.VehicleType)
                .ToDictionary(g => g.Key, g => g.Count());

            return new Dictionary<string, object>
            {
                { "Total Vehicles", totalVehicles },
                { "Total Revenue", totalRevenue },
                { "Average Duration (hours)", Math.Round(averageDuration, 2) },
                { "Vehicle Types", vehicleTypes }
            };
        }

        public async Task<decimal> GetTotalRevenueTodayAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= today && a.Time < tomorrow && a.Action == "Exit" && a.Fee.HasValue)
                .SumAsync(a => a.Fee ?? 0);
        }

        public async Task<decimal?> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.ParkingActivities
                .Where(a => a.Time >= startDate && a.Time <= endDate && a.Action == "Exit" && a.Fee.HasValue)
                .SumAsync(a => a.Fee);
        }

        private double ParseDuration(string duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;
            var parts = duration.Split(' ');
            if (parts.Length < 2) return 0;
            if (double.TryParse(parts[0], out double hours))
                return hours;
            return 0;
        }
    }
} 