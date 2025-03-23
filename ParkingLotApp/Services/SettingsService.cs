using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Data;
using ParkingLotApp.Models;
using ParkingLotApp.Services.Interfaces;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace ParkingLotApp.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _isDbInitialized = false;
        private Dictionary<string, string> _defaultSettings;

        public SettingsService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
            // Initialize default settings
            _defaultSettings = new Dictionary<string, string>
            {
                { "total_spots", "100" },
                { "car_rate", "5000" },
                { "motorcycle_rate", "2000" },
                { "truck_rate", "10000" },
                { "bus_rate", "15000" },
                { "company_name", "Parking Management System" },
                { "company_address", "123 Main Street" },
                { "report_footer", "Thank you for your business!" }
            };
            
            // Check database connection asynchronously
            Task.Run(async () =>
            {
                try
                {
                    // Wait a short time to let the application initialize the database
                    await Task.Delay(5000);
                    
                    using var scope = _serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ParkingDbContext>();
                    
                    var canConnect = await dbContext.Database.CanConnectAsync();
                    if (canConnect)
                    {
                        // Check if Settings table exists to confirm initialization
                        try
                        {
                            await dbContext.Settings.FirstOrDefaultAsync();
                            _isDbInitialized = true;
                            Console.WriteLine("[Info] SettingsService database connection established");
                            
                            // Ensure default settings exist
                            await EnsureDefaultSettingsAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Warning] Settings table not available yet: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] SettingsService initialization error: {ex.Message}");
                }
            });
        }
        
        private ParkingDbContext GetDbContext()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<ParkingDbContext>();
        }
        
        private async Task EnsureDefaultSettingsAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                
                using var dbContext = GetDbContext();
                foreach (var setting in _defaultSettings)
                {
                    var existingSetting = await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == setting.Key);
                    if (existingSetting == null)
                    {
                        dbContext.Settings.Add(new Setting
                        {
                            Key = setting.Key,
                            Value = setting.Value,
                            Description = $"Default setting for {setting.Key}"
                        });
                    }
                }
                
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to ensure default settings: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // This method is used internally
        public async Task<List<Setting>> GetSettingsListAsync()
        {
            if (!_isDbInitialized)
            {
                return _defaultSettings.Select(s => new Setting
                {
                    Key = s.Key,
                    Value = s.Value,
                    Description = $"Default setting for {s.Key}"
                }).ToList();
            }
            
            try
            {
                using var dbContext = GetDbContext();
                return await dbContext.Settings.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to get settings list: {ex.Message}");
                // Return default settings as fallback
                return _defaultSettings.Select(s => new Setting
                {
                    Key = s.Key,
                    Value = s.Value,
                    Description = $"Default setting for {s.Key}"
                }).ToList();
            }
        }

        // This method implements the interface
        public async Task<Dictionary<string, string>> GetAllSettingsAsync()
        {
            if (!_isDbInitialized)
            {
                return new Dictionary<string, string>(_defaultSettings);
            }
            
            try
            {
                using var dbContext = GetDbContext();
                var settings = await dbContext.Settings.ToListAsync();
                return settings.ToDictionary(s => s.Key, s => s.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to get all settings: {ex.Message}");
                // Return default settings as fallback
                return new Dictionary<string, string>(_defaultSettings);
            }
        }

        public async Task<Setting?> GetSettingByKeyAsync(string key)
        {
            if (!_isDbInitialized)
            {
                if (_defaultSettings.TryGetValue(key, out string? value))
                {
                    return new Setting
                    {
                        Key = key,
                        Value = value,
                        Description = $"Default setting for {key}"
                    };
                }
                return null;
            }
            
            try
            {
                using var dbContext = GetDbContext();
                return await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to get setting by key '{key}': {ex.Message}");
                // Return default setting as fallback
                if (_defaultSettings.TryGetValue(key, out string? value))
                {
                    return new Setting
                    {
                        Key = key,
                        Value = value,
                        Description = $"Default setting for {key}"
                    };
                }
                return null;
            }
        }

        public async Task<bool> UpdateSettingAsync(string key, string value, int userId)
        {
            if (!_isDbInitialized)
            {
                _defaultSettings[key] = value;
                return true;
            }
            
            try
            {
                await _semaphore.WaitAsync();
                try
                {
                    using var dbContext = GetDbContext();
                    var setting = await dbContext.Settings.FirstOrDefaultAsync(s => s.Key == key);
                    if (setting == null)
                    {
                        setting = new Setting
                        {
                            Key = key,
                            Value = value,
                            Description = key,
                            UpdatedBy = userId,
                            UpdatedAt = DateTime.Now
                        };
                        dbContext.Settings.Add(setting);
                    }
                    else
                    {
                        setting.Value = value;
                        setting.UpdatedBy = userId;
                        setting.UpdatedAt = DateTime.Now;
                    }

                    await dbContext.SaveChangesAsync();
                    return true;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to update setting '{key}': {ex.Message}");
                // Update default settings as fallback
                _defaultSettings[key] = value;
                return false;
            }
        }

        public async Task<decimal> GetVehicleRateAsync(string vehicleType)
        {
            string key = $"{vehicleType.ToLower()}_rate";
            var setting = await GetSettingByKeyAsync(key);
            
            if (setting != null && decimal.TryParse(setting.Value, out decimal rate))
            {
                return rate;
            }
            
            // Default rates if not defined in settings
            return vehicleType.ToLower() switch
            {
                "car" => 5000m,
                "motorcycle" => 2000m,
                "truck" => 10000m,
                "bus" => 15000m,
                _ => 5000m // Default to car rate for unknown types
            };
        }
        
        public async Task<Dictionary<string, decimal>> GetVehicleRatesAsync()
        {
            var rates = new Dictionary<string, decimal>();
            
            try
            {
                using var dbContext = GetDbContext();
                var settings = await dbContext.Settings
                    .Where(s => s.Key.EndsWith("_rate"))
                    .ToListAsync();
                
                foreach (var setting in settings)
                {
                    string vehicleType = setting.Key.Replace("_rate", "");
                    if (decimal.TryParse(setting.Value, out decimal rate))
                    {
                        rates[vehicleType] = rate;
                    }
                }
                
                // Ensure all standard rates exist
                if (!rates.ContainsKey("car")) rates["car"] = 5000m;
                if (!rates.ContainsKey("motorcycle")) rates["motorcycle"] = 2000m;
                if (!rates.ContainsKey("truck")) rates["truck"] = 10000m;
                if (!rates.ContainsKey("bus")) rates["bus"] = 15000m;
                
                return rates;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to get vehicle rates: {ex.Message}");
                
                // Return default rates as fallback
                return new Dictionary<string, decimal>
                {
                    { "car", 5000m },
                    { "motorcycle", 2000m },
                    { "truck", 10000m },
                    { "bus", 15000m }
                };
            }
        }
        
        public async Task<Dictionary<string, string>> GetReportSettingsAsync()
        {
            var settings = new Dictionary<string, string>();
            
            try
            {
                using var dbContext = GetDbContext();
                var keys = new[] { "company_name", "company_address", "report_footer" };
                var dbSettings = await dbContext.Settings
                    .Where(s => keys.Contains(s.Key))
                    .ToListAsync();
                
                foreach (var setting in dbSettings)
                {
                    settings[setting.Key] = setting.Value;
                }
                
                // Fill in missing values with defaults
                if (!settings.ContainsKey("company_name"))
                    settings["company_name"] = "Parking Management System";
                    
                if (!settings.ContainsKey("company_address"))
                    settings["company_address"] = "123 Main Street";
                    
                if (!settings.ContainsKey("report_footer"))
                    settings["report_footer"] = "Thank you for your business!";
                
                return settings;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to get report settings: {ex.Message}");
                
                // Return default settings as fallback
                return new Dictionary<string, string>
                {
                    { "company_name", "Parking Management System" },
                    { "company_address", "123 Main Street" },
                    { "report_footer", "Thank you for your business!" }
                };
            }
        }
    }
} 