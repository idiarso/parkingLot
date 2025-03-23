using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Data;
using ParkingLotApp.Models;
using ParkingLotApp.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ParkingLotApp.Services
{
    public class DashboardService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;

        public DashboardService(IServiceProvider serviceProvider, ISettingsService settingsService, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService;
            _logger = logger;
        }

        // Metode helper untuk mendapatkan context baru untuk setiap operasi
        private ParkingDbContext GetDbContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ParkingDbContext>();
        }

        /// <summary>
        /// Mendapatkan jumlah total slot parkir yang tersedia
        /// </summary>
        public async Task<int> GetTotalSpotsAsync()
        {
            try
            {
                var setting = await _settingsService.GetSettingByKeyAsync("total_spots");
                if (setting != null && int.TryParse(setting.Value, out int totalSpots))
                {
                    return totalSpots;
                }
                return 100; // Default jika setting tidak ditemukan
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting total spots", ex);
                return 100; // Default jika terjadi error
            }
        }

        /// <summary>
        /// Mendapatkan jumlah slot parkir yang sedang digunakan
        /// </summary>
        public async Task<int> GetOccupiedSpotsAsync()
        {
            try
            {
                using var dbContext = GetDbContext();
                // Hitung kendaraan yang sudah masuk tetapi belum keluar
                return await dbContext.ParkingActivities
                    .CountAsync(a => a.Action == "Entry" && !dbContext.ParkingActivities
                        .Any(b => b.VehicleNumber == a.VehicleNumber && b.Action == "Exit" && b.EntryTime == a.EntryTime));
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting occupied spots", ex);
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan jumlah slot parkir yang masih tersedia
        /// </summary>
        public async Task<int> GetAvailableSpotsAsync()
        {
            int totalSpots = await GetTotalSpotsAsync();
            int occupiedSpots = await GetOccupiedSpotsAsync();
            return Math.Max(0, totalSpots - occupiedSpots);
        }

        /// <summary>
        /// Mendapatkan total pendapatan hari ini
        /// </summary>
        public async Task<decimal> GetTodayRevenueAsync()
        {
            try
            {
                using var dbContext = GetDbContext();
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                
                return await dbContext.ParkingActivities
                    .Where(a => a.Action == "Exit" && a.ExitTime >= today && a.ExitTime < tomorrow && a.Fee.HasValue)
                    .SumAsync(a => a.Fee ?? 0);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting today's revenue", ex);
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan pendapatan minggu ini
        /// </summary>
        public async Task<decimal> GetWeekRevenueAsync()
        {
            try
            {
                using var dbContext = GetDbContext();
                // Mendapatkan tanggal awal minggu (hari Senin)
                DateTime today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startOfWeek = today.AddDays(-diff);
                DateTime endOfWeek = startOfWeek.AddDays(7);
                
                return await dbContext.ParkingActivities
                    .Where(a => a.Action == "Exit" && a.ExitTime >= startOfWeek && a.ExitTime < endOfWeek && a.Fee.HasValue)
                    .SumAsync(a => a.Fee ?? 0);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting weekly revenue", ex);
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan pendapatan bulan ini
        /// </summary>
        public async Task<decimal> GetMonthRevenueAsync()
        {
            try
            {
                using var dbContext = GetDbContext();
                DateTime today = DateTime.Today;
                DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime startOfNextMonth = startOfMonth.AddMonths(1);
                
                return await dbContext.ParkingActivities
                    .Where(a => a.Action == "Exit" && a.ExitTime >= startOfMonth && a.ExitTime < startOfNextMonth && a.Fee.HasValue)
                    .SumAsync(a => a.Fee ?? 0);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting monthly revenue", ex);
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan distribusi jenis kendaraan yang sedang parkir
        /// </summary>
        public async Task<Dictionary<string, int>> GetVehicleDistributionAsync()
        {
            try
            {
                using var dbContext = GetDbContext();
                var distribution = new Dictionary<string, int>();
                
                // Mendapatkan kendaraan yang masih parkir (belum keluar)
                var activeVehicles = await dbContext.ParkingActivities
                    .Where(a => a.Action == "Entry" && !dbContext.ParkingActivities
                        .Any(b => b.VehicleNumber == a.VehicleNumber && b.Action == "Exit" && b.EntryTime == a.EntryTime))
                    .ToListAsync();
                
                // Mengelompokkan berdasarkan jenis kendaraan
                foreach (var vehicle in activeVehicles)
                {
                    if (!string.IsNullOrEmpty(vehicle.VehicleType))
                    {
                        if (distribution.ContainsKey(vehicle.VehicleType))
                        {
                            distribution[vehicle.VehicleType]++;
                        }
                        else
                        {
                            distribution[vehicle.VehicleType] = 1;
                        }
                    }
                }
                
                return distribution;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting vehicle distribution", ex);
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Mendapatkan aktivitas parkir terbaru
        /// </summary>
        public async Task<List<ParkingActivity>> GetRecentActivitiesAsync(int count = 10)
        {
            try
            {
                using var dbContext = GetDbContext();
                return await dbContext.ParkingActivities
                    .OrderByDescending(a => a.Action == "Exit" ? a.ExitTime : a.EntryTime)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting recent activities", ex);
                return new List<ParkingActivity>();
            }
        }

        /// <summary>
        /// Mendapatkan log sistem terbaru
        /// </summary>
        public async Task<List<Log>> GetRecentLogsAsync(int count = 10)
        {
            try
            {
                using var dbContext = GetDbContext();
                return await dbContext.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting recent logs", ex);
                return new List<Log>();
            }
        }

        /// <summary>
        /// Memeriksa koneksi ke database
        /// </summary>
        public async Task<bool> CheckDatabaseConnectionAsync()
        {
            try
            {
                using var dbContext = GetDbContext();
                // Coba akses database dengan query sederhana
                await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Database connection check failed", ex);
                return false;
            }
        }
    }
} 