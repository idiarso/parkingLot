using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using ParkingLotApp.Models;
using System.Data;
using ParkingLotApp.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.Services
{
    public class ParkingService : IParkingService
    {
        private readonly ParkingDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;

        public ParkingService(ParkingDbContext dbContext, ILogger logger, ISettingsService settingsService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _settingsService = settingsService;
        }

        public async Task<bool> RegisterVehicleEntryAsync(string vehicleNumber, string vehicleType, string notes)
        {
            try
            {
                var activity = new ParkingActivity
                {
                    VehicleNumber = vehicleNumber,
                    VehicleType = vehicleType,
                    Notes = notes,
                    EntryTime = DateTime.Now,
                    Action = "Entry"
                };

                _dbContext.ParkingActivities.Add(activity);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ParkingActivity?> GetVehicleEntryAsync(string vehicleNumber)
        {
            return await _dbContext.ParkingActivities
                .FirstOrDefaultAsync(a => a.VehicleNumber == vehicleNumber && a.Action == "Entry");
        }

        public async Task<bool> RegisterVehicleExitAsync(string vehicleNumber, decimal fee, string duration)
        {
            try
            {
                var activity = await _dbContext.ParkingActivities
                    .FirstOrDefaultAsync(a => a.VehicleNumber == vehicleNumber && a.Action == "Entry");

                if (activity != null)
                {
                    activity.Action = "Exit";
                    activity.Fee = fee;
                    activity.Duration = duration;
                    activity.ExitTime = DateTime.Now;
                    activity.UpdatedAt = DateTime.Now;
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<ParkingStatistics> GetStatisticsAsync(DateTime date, string period)
        {
            var stats = new ParkingStatistics();
            try
            {
                var startDate = period.ToLower() switch
                {
                    "today" => date.Date,
                    "week" => date.AddDays(-7),
                    "month" => date.AddMonths(-1),
                    _ => date.Date
                };

                var activities = await _dbContext.ParkingActivities
                    .Where(a => a.EntryTime >= startDate)
                    .ToListAsync();

                stats.TotalSpots = 100; // Get from settings
                stats.OccupiedSpots = activities.Count(a => a.Action == "Entry");
                stats.AvailableSpots = stats.TotalSpots - stats.OccupiedSpots;

                var revenue = activities
                    .Where(a => a.Action == "Exit")
                    .Sum(a => a.Fee ?? 0);

                stats.TodayRevenue = revenue;

                var vehicleTypes = activities
                    .GroupBy(a => a.VehicleType)
                    .ToDictionary(g => g.Key, g => g.Count());

                stats.VehicleTypes = vehicleTypes;

                return stats;
            }
            catch (Exception)
            {
                return stats;
            }
        }

        public async Task<List<ParkingActivity>> GetRecentActivitiesAsync(int limit = 10)
        {
            return await _dbContext.ParkingActivities
                .OrderByDescending(a => a.EntryTime)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<ParkingActivity>> GetParkingActivitiesAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.ParkingActivities
                .Where(a => a.EntryTime >= startDate && a.EntryTime <= endDate)
                .OrderByDescending(a => a.EntryTime)
                .ToListAsync();
        }

        public async Task<ParkingActivity> GetParkingActivityByIdAsync(int id)
        {
            await _dbContext.EnsureConnectedAsync();
            return await _dbContext.ParkingActivities.FindAsync(id);
        }

        public async Task<ParkingActivity?> GetParkingActivityByBarcodeAsync(string barcode)
        {
            return await _dbContext.ParkingActivities
                .FirstOrDefaultAsync(a => a.Barcode == barcode);
        }

        public async Task<ParkingActivity> CreateParkingActivityAsync(ParkingActivity activity)
        {
            await _dbContext.EnsureConnectedAsync();
            _dbContext.ParkingActivities.Add(activity);
            await _dbContext.SaveChangesAsync();
            return activity;
        }

        public async Task<ParkingActivity> UpdateParkingActivityAsync(ParkingActivity activity)
        {
            await _dbContext.EnsureConnectedAsync();
            _dbContext.ParkingActivities.Update(activity);
            await _dbContext.SaveChangesAsync();
            return activity;
        }

        public async Task<bool> DeleteParkingActivityAsync(int id)
        {
            await _dbContext.EnsureConnectedAsync();
            var activity = await _dbContext.ParkingActivities.FindAsync(id);
            if (activity == null)
                return false;

            _dbContext.ParkingActivities.Remove(activity);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetTotalVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            await _dbContext.EnsureConnectedAsync();
            return await _dbContext.ParkingActivities
                .Where(a => a.EntryTime >= startDate && a.EntryTime < endDate)
                .CountAsync();
        }

        public async Task<decimal?> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            await _dbContext.EnsureConnectedAsync();
            return await _dbContext.ParkingActivities
                .Where(a => a.EntryTime >= startDate && a.EntryTime < endDate && a.Action == "Exit")
                .SumAsync(a => a.Fee);
        }

        public async Task<bool> RegisterEntryAsync(ParkingActivity activity)
        {
            try
            {
                activity.EntryTime = DateTime.Now;
                activity.Action = "Entry";
                _dbContext.ParkingActivities.Add(activity);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RegisterExitAsync(string barcode, decimal fee)
        {
            try
            {
                var activity = await GetParkingActivityByBarcodeAsync(barcode);
                if (activity != null)
                {
                    activity.Action = "Exit";
                    activity.Fee = fee;
                    activity.ExitTime = DateTime.Now;
                    activity.UpdatedAt = DateTime.Now;
                    activity.Duration = CalculateDuration(activity.EntryTime, DateTime.Now);
                    await _dbContext.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetAvailableSpotsAsync()
        {
            var totalSpots = 100; // Get from settings
            var occupiedSpots = await GetOccupiedSpotsAsync();
            return totalSpots - occupiedSpots;
        }

        public async Task<int> GetOccupiedSpotsAsync()
        {
            return await _dbContext.ParkingActivities
                .CountAsync(a => a.Action == "Entry");
        }

        public async Task<decimal> CalculateFeeAsync(string barcode)
        {
            try
            {
                await _dbContext.EnsureConnectedAsync();

                var activity = await _dbContext.ParkingActivities
                    .FirstOrDefaultAsync(p => p.Barcode == barcode && p.ExitTime == null);

                if (activity == null)
                    return 0;

                // Calculate duration
                var duration = DateTime.Now - activity.EntryTime;
                
                // Get rate from settings
                var hourlyRate = await _settingsService.GetVehicleRateAsync(activity.VehicleType);
                
                // Calculate fee (round up to next hour)
                var hours = Math.Ceiling(duration.TotalHours);
                return hourlyRate * (decimal)hours;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error calculating fee", ex);
                return 0;
            }
        }

        private string CalculateDuration(DateTime start, DateTime end)
        {
            var duration = end - start;
            var hours = (int)duration.TotalHours;
            var minutes = duration.Minutes;

            return $"{hours}h {minutes}m";
        }
    }
} 