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
        private bool _isDbInitialized = false;

        public DashboardService(IServiceProvider serviceProvider, ISettingsService settingsService, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _settingsService = settingsService;
            _logger = logger;
            
            // Check database connection asynchronously
            Task.Run(async () =>
            {
                try
                {
                    // Wait a short time to let the application initialize the database
                    await Task.Delay(3000);
                    
                    var isConnected = await CheckDatabaseConnectionAsync();
                    _isDbInitialized = isConnected;
                    Console.WriteLine($"[Debug] DashboardService database initialization status: {_isDbInitialized}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] DashboardService initialization error: {ex.Message}");
                }
            });
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
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetTotalSpotsAsync: Database not initialized, returning default");
                return 100; // Default jika DB belum siap
            }
            
            try
            {
                var setting = await _settingsService.GetSettingByKeyAsync("total_spots");
                if (setting != null && int.TryParse(setting.Value, out int totalSpots))
                {
                    return totalSpots;
                }
                Console.WriteLine("[Debug] GetTotalSpotsAsync: Setting not found or invalid, returning default");
                return 100; // Default jika setting tidak ditemukan
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting total spots", ex);
                Console.WriteLine($"[Debug] GetTotalSpotsAsync error: {ex.Message}");
                return 100; // Default jika terjadi error
            }
        }

        /// <summary>
        /// Mendapatkan jumlah slot parkir yang sedang digunakan
        /// </summary>
        public async Task<int> GetOccupiedSpotsAsync()
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetOccupiedSpotsAsync: Database not initialized, returning 0");
                return 0; // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                // Hitung kendaraan yang sudah masuk tetapi belum keluar
                var count = await dbContext.ParkingActivities
                    .CountAsync(a => a.Action == "Entry" && !dbContext.ParkingActivities
                        .Any(b => b.VehicleNumber == a.VehicleNumber && b.Action == "Exit" && b.EntryTime == a.EntryTime));
                Console.WriteLine($"[Debug] GetOccupiedSpotsAsync: Found {count} occupied spots");
                return count;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting occupied spots", ex);
                Console.WriteLine($"[Debug] GetOccupiedSpotsAsync error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Debug] Inner exception: {ex.InnerException.Message}");
                }
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
            int available = Math.Max(0, totalSpots - occupiedSpots);
            Console.WriteLine($"[Debug] GetAvailableSpotsAsync: {available} spots available (total: {totalSpots}, occupied: {occupiedSpots})");
            return available;
        }

        /// <summary>
        /// Mendapatkan total pendapatan hari ini
        /// </summary>
        public async Task<decimal> GetTodayRevenueAsync()
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetTodayRevenueAsync: Database not initialized, returning 0");
                return 0; // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                
                Console.WriteLine($"[Debug] Getting today's revenue for {today:yyyy-MM-dd}");
                
                // Pastikan tabel ParkingActivities dapat diakses
                try
                {
                    var count = await dbContext.ParkingActivities.CountAsync();
                    Console.WriteLine($"[Debug] Found {count} parking activities in database");
                } 
                catch (Exception ex)
                {
                    Console.WriteLine($"[Debug] Error accessing ParkingActivities table: {ex.Message}");
                    return 0;
                }
                
                var result = await dbContext.ParkingActivities
                    .Where(a => a.Action == "Exit" && a.ExitTime >= today && a.ExitTime < tomorrow && a.Fee.HasValue)
                    .SumAsync(a => a.Fee ?? 0);
                
                Console.WriteLine($"[Debug] Today's revenue: {result:C}");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Error getting today's revenue: {ex.Message}", ex);
                Console.WriteLine($"[Debug] Revenue error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Debug] Inner exception: {ex.InnerException.Message}");
                }
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan pendapatan minggu ini
        /// </summary>
        public async Task<decimal> GetWeekRevenueAsync()
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetWeekRevenueAsync: Database not initialized, returning 0");
                return 0; // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                // Mendapatkan tanggal awal minggu (hari Senin)
                DateTime today = DateTime.Today;
                int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
                DateTime startOfWeek = today.AddDays(-diff);
                DateTime endOfWeek = startOfWeek.AddDays(7);
                
                Console.WriteLine($"[Debug] Getting weekly revenue from {startOfWeek:yyyy-MM-dd} to {endOfWeek:yyyy-MM-dd}");
                
                var result = await dbContext.ParkingActivities
                    .Where(a => a.Action == "Exit" && a.ExitTime >= startOfWeek && a.ExitTime < endOfWeek && a.Fee.HasValue)
                    .SumAsync(a => a.Fee ?? 0);
                
                Console.WriteLine($"[Debug] Weekly revenue: {result:C}");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Error getting weekly revenue: {ex.Message}", ex);
                Console.WriteLine($"[Debug] Weekly revenue error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Debug] Inner exception: {ex.InnerException.Message}");
                }
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan pendapatan bulan ini
        /// </summary>
        public async Task<decimal> GetMonthRevenueAsync()
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetMonthRevenueAsync: Database not initialized, returning 0");
                return 0; // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                DateTime today = DateTime.Today;
                DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime startOfNextMonth = startOfMonth.AddMonths(1);
                
                Console.WriteLine($"[Debug] Getting monthly revenue from {startOfMonth:yyyy-MM-dd} to {startOfNextMonth:yyyy-MM-dd}");
                
                var result = await dbContext.ParkingActivities
                    .Where(a => a.Action == "Exit" && a.ExitTime >= startOfMonth && a.ExitTime < startOfNextMonth && a.Fee.HasValue)
                    .SumAsync(a => a.Fee ?? 0);
                
                Console.WriteLine($"[Debug] Monthly revenue: {result:C}");
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Error getting monthly revenue: {ex.Message}", ex);
                Console.WriteLine($"[Debug] Monthly revenue error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Debug] Inner exception: {ex.InnerException.Message}");
                }
                return 0;
            }
        }

        /// <summary>
        /// Mendapatkan distribusi jenis kendaraan yang sedang parkir
        /// </summary>
        public async Task<Dictionary<string, int>> GetVehicleDistributionAsync()
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetVehicleDistributionAsync: Database not initialized, returning empty");
                return new Dictionary<string, int>(); // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                var distribution = new Dictionary<string, int>();
                
                // Mendapatkan kendaraan yang masih parkir (belum keluar)
                var activeVehicles = await dbContext.ParkingActivities
                    .Where(a => a.Action == "Entry" && !dbContext.ParkingActivities
                        .Any(b => b.VehicleNumber == a.VehicleNumber && b.Action == "Exit" && b.EntryTime == a.EntryTime))
                    .ToListAsync();
                
                Console.WriteLine($"[Debug] Found {activeVehicles.Count} active vehicles for distribution");
                
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
                Console.WriteLine($"[Debug] Vehicle distribution error: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Mendapatkan aktivitas parkir terbaru
        /// </summary>
        public async Task<List<ParkingActivity>> GetRecentActivitiesAsync(int count = 10)
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetRecentActivitiesAsync: Database not initialized, returning empty");
                return new List<ParkingActivity>(); // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                var activities = await dbContext.ParkingActivities
                    .OrderByDescending(a => a.Action == "Exit" ? a.ExitTime : a.EntryTime)
                    .Take(count)
                    .ToListAsync();
                
                Console.WriteLine($"[Debug] Found {activities.Count} recent activities");
                return activities;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting recent activities", ex);
                Console.WriteLine($"[Debug] Recent activities error: {ex.Message}");
                return new List<ParkingActivity>();
            }
        }

        /// <summary>
        /// Mendapatkan log sistem terbaru
        /// </summary>
        public async Task<List<Log>> GetRecentLogsAsync(int count = 10)
        {
            if (!_isDbInitialized)
            {
                Console.WriteLine("[Debug] GetRecentLogsAsync: Database not initialized, returning empty");
                return new List<Log>(); // Default jika DB belum siap
            }
            
            try
            {
                using var dbContext = GetDbContext();
                var logs = await dbContext.Logs
                    .OrderByDescending(l => l.Timestamp)
                    .Take(count)
                    .ToListAsync();
                
                Console.WriteLine($"[Debug] Found {logs.Count} recent logs");
                return logs;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error getting recent logs", ex);
                Console.WriteLine($"[Debug] Recent logs error: {ex.Message}");
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
                Console.WriteLine("[Debug] Checking database connection...");
                await dbContext.Database.ExecuteSqlRawAsync("SELECT 1");
                Console.WriteLine("[Debug] Database connection successful");
                
                // Cek apakah tabel-tabel bisa diakses
                try {
                    var parkingCount = await dbContext.ParkingActivities.CountAsync();
                    Console.WriteLine($"[Debug] Found {parkingCount} parking activities");
                    
                    var logCount = await dbContext.Logs.CountAsync();
                    Console.WriteLine($"[Debug] Found {logCount} logs");
                    
                    // Update status database berhasil diinisialisasi
                    _isDbInitialized = true;
                } catch (Exception ex) {
                    Console.WriteLine($"[Debug] Error accessing tables: {ex.Message}");
                    // Koneksi berhasil tapi ada masalah dengan tabel
                    await _logger.LogWarningAsync($"Database connection OK but error accessing tables: {ex.Message}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Database connection check failed: {ex.Message}", ex);
                Console.WriteLine($"[Debug] Database connection error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Debug] Inner exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
} 