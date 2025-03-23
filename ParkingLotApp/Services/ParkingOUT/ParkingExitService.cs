using System;
using System.Threading.Tasks;
using ParkingLotApp.Models;
using ParkingLotApp.Data;
using ParkingLotApp.Services.Hardware;
using ParkingLotApp.Services.WebSocket;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.Services.ParkingOUT
{
    public class ParkingExitService
    {
        private readonly HardwareManager _hardwareManager;
        private readonly WebSocketServer _webSocketServer;
        private readonly Data.ParkingDbContext _dbContext;
        private readonly ILogger _logger;

        public event EventHandler<ParkingActivity>? ExitProcessed;

        public ParkingExitService(
            HardwareManager hardwareManager,
            WebSocketServer webSocketServer,
            Data.ParkingDbContext dbContext,
            ILogger logger)
        {
            _hardwareManager = hardwareManager;
            _webSocketServer = webSocketServer;
            _dbContext = dbContext;
            _logger = logger;

            // Subscribe to barcode scanner events
            _hardwareManager.BarcodeScanned += HardwareManager_BarcodeScanned;
        }

        private async void HardwareManager_BarcodeScanned(object? sender, string barcode)
        {
            try
            {
                await ProcessExitAsync(barcode);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error processing barcode scan for exit", ex);
            }
        }

        public async Task<ParkingActivity> ProcessExitAsync(string barcode)
        {
            try
            {
                await _logger.LogInfoAsync($"Processing exit for barcode: {barcode}");

                // Get entry record
                var entry = await GetEntryRecord(barcode);
                if (entry == null)
                {
                    throw new Exception($"No entry record found for barcode: {barcode}");
                }

                // Calculate parking duration and fee
                var exitTime = DateTime.Now;
                var duration = exitTime - entry.Time;
                var fee = CalculateParkingFee(duration);

                // Capture exit image
                var imagePath = await _hardwareManager.CaptureImageAsync();

                // Create exit activity
                var activity = new ParkingActivity
                {
                    VehicleNumber = entry.VehicleNumber,
                    Action = "Exit",
                    Time = exitTime,
                    ImagePath = imagePath,
                    Fee = fee
                };

                // Save to database
                await SaveExitToDatabase(activity);

                // Open gate
                await _hardwareManager.OpenGateBarrierAsync();

                // Notify clients through WebSocket
                await _webSocketServer.BroadcastMessage(new WebSocketMessage
                {
                    Type = "ParkingExit",
                    Data = activity,
                    Timestamp = DateTime.Now
                });

                // Raise event
                ExitProcessed?.Invoke(this, activity);

                await _logger.LogInfoAsync($"Exit processed successfully for vehicle: {entry.VehicleNumber}");
                return activity;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to process exit for barcode: {barcode}", ex);
                throw;
            }
        }

        private async Task<ParkingActivity?> GetEntryRecord(string barcode)
        {
            await _dbContext.EnsureConnectedAsync();

            try
            {
                using var cmd = new Npgsql.NpgsqlCommand(
                    @"SELECT id, vehicle_number, entry_time 
                      FROM parking_activities 
                      WHERE action = 'Entry' 
                      AND vehicle_number = (
                          SELECT vehicle_number 
                          FROM parking_activities 
                          WHERE barcode = @barcode
                      )
                      AND NOT EXISTS (
                          SELECT 1 
                          FROM parking_activities 
                          WHERE action = 'Exit' 
                          AND vehicle_number = parking_activities.vehicle_number
                      )
                      ORDER BY entry_time DESC 
                      LIMIT 1",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("barcode", barcode);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new ParkingActivity
                    {
                        Id = reader.GetInt32(0),
                        VehicleNumber = reader.GetString(1),
                        Time = reader.GetDateTime(2),
                        Action = "Entry"
                    };
                }

                return null;
            }
            finally
            {
                _dbContext.CloseConnection();
            }
        }

        private async Task SaveExitToDatabase(ParkingActivity activity)
        {
            await _dbContext.EnsureConnectedAsync();

            try
            {
                using var cmd = new Npgsql.NpgsqlCommand(
                    @"INSERT INTO parking_activities 
                      (vehicle_number, action, exit_time, image_path, fee, created_at)
                      VALUES (@vehicleNumber, @action, @exitTime, @imagePath, @fee, @createdAt)",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("vehicleNumber", activity.VehicleNumber);
                cmd.Parameters.AddWithValue("action", activity.Action);
                cmd.Parameters.AddWithValue("exitTime", activity.Time);
                cmd.Parameters.AddWithValue("imagePath", activity.ImagePath);
                cmd.Parameters.AddWithValue("fee", activity.Fee);
                cmd.Parameters.AddWithValue("createdAt", DateTime.Now);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                _dbContext.CloseConnection();
            }
        }

        private decimal CalculateParkingFee(TimeSpan duration)
        {
            // Example fee calculation:
            // First hour: 5000
            // Each additional hour: 3000
            decimal baseFee = 5000;
            decimal hourlyFee = 3000;
            
            int totalHours = (int)Math.Ceiling(duration.TotalHours);
            if (totalHours <= 1)
            {
                return baseFee;
            }
            
            return baseFee + (totalHours - 1) * hourlyFee;
        }
    }
} 