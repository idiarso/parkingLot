using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ParkingLotApp.Models;

namespace ParkingLotApp.Services.Interfaces
{
    public interface IParkingService
    {
        Task<bool> RegisterVehicleEntryAsync(string vehicleNumber, string vehicleType, string notes);
        Task<ParkingActivity?> GetVehicleEntryAsync(string vehicleNumber);
        Task<bool> RegisterVehicleExitAsync(string vehicleNumber, decimal fee, string duration);
        Task<ParkingStatistics> GetStatisticsAsync(DateTime date, string period);
        Task<List<ParkingActivity>> GetRecentActivitiesAsync(int limit = 10);
        Task<List<ParkingActivity>> GetParkingActivitiesAsync(DateTime startDate, DateTime endDate);
        Task<ParkingActivity?> GetParkingActivityByBarcodeAsync(string barcode);
        Task<bool> RegisterEntryAsync(ParkingActivity activity);
        Task<bool> RegisterExitAsync(string barcode, decimal fee);
        Task<int> GetAvailableSpotsAsync();
        Task<int> GetOccupiedSpotsAsync();
        Task<decimal> CalculateFeeAsync(string barcode);
    }
} 