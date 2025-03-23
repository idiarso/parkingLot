using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLotApp.Models;

namespace ParkingLotApp.Services.Interfaces
{
    public interface ISettingsService
    {
        Task<Dictionary<string, string>> GetAllSettingsAsync();
        Task<Setting?> GetSettingByKeyAsync(string key);
        Task<bool> UpdateSettingAsync(string key, string value, int userId);
        Task<decimal> GetVehicleRateAsync(string vehicleType);
        Task<Dictionary<string, string>> GetReportSettingsAsync();
        Task<List<Setting>> GetSettingsListAsync();
    }
} 