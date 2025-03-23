using System;
using System.Threading.Tasks;
using ParkingLotApp.Data;
using ParkingLotApp.Models;
using ParkingLotApp.Services.Hardware;
using ParkingLotApp.Services.WebSocket;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.Services.ParkingIN
{
    public interface IPrinter
    {
        Task PrintAsync(byte[] data);
    }

    public class NetworkPrinter : IPrinter
    {
        private readonly string _printerName;
        private readonly int _port;

        public NetworkPrinter(string printerName, int port)
        {
            _printerName = printerName;
            _port = port;
        }

        public Task PrintAsync(byte[] data)
        {
            // TODO: Implement actual printer communication
            return Task.CompletedTask;
        }
    }

    public class ParkingEntryService
    {
        private readonly HardwareManager _hardwareManager;
        private readonly WebSocketServer _webSocketServer;
        private readonly ParkingDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IPrinter _printer;

        public event EventHandler<ParkingActivity>? EntryProcessed;

        public ParkingEntryService(
            HardwareManager hardwareManager,
            WebSocketServer webSocketServer,
            ParkingDbContext dbContext,
            ILogger logger)
        {
            _hardwareManager = hardwareManager;
            _webSocketServer = webSocketServer;
            _dbContext = dbContext;
            _logger = logger;

            // Initialize printer
            var printerName = LoadPrinterConfig();
            _printer = new NetworkPrinter(printerName, 9100);

            // Subscribe to barcode scanner events
            _hardwareManager.BarcodeScanned += HardwareManager_BarcodeScanned;
        }

        private string LoadPrinterConfig()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "printer.ini");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("PrinterName="))
                        {
                            return line.Substring(12);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load printer configuration: {ex.Message}");
                Task.Run(async () => await _logger.LogErrorAsync("Failed to load printer configuration", ex));
            }
            return "TM-T82"; // Default printer name
        }

        private async void HardwareManager_BarcodeScanned(object? sender, string barcode)
        {
            try
            {
                await ProcessEntryAsync(barcode);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to process barcode scan", ex);
            }
        }

        public async Task<ParkingActivity> ProcessEntryAsync(string vehicleNumber)
        {
            try
            {
                await _logger.LogInfoAsync($"Processing entry for vehicle: {vehicleNumber}");

                // Capture vehicle image
                var imagePath = await _hardwareManager.CaptureImageAsync();
                
                // Create parking activity
                var activity = new ParkingActivity
                {
                    VehicleNumber = vehicleNumber,
                    Time = DateTime.Now,
                    Action = "Entry"
                };

                // Save to database
                await SaveEntryToDatabase(activity, imagePath);

                // Print ticket
                await PrintTicketAsync(activity);

                // Open gate
                await _hardwareManager.OpenGateBarrierAsync();

                // Notify clients through WebSocket
                await _webSocketServer.BroadcastMessage(new WebSocketMessage
                {
                    Type = "ParkingEntry",
                    Data = activity,
                    Timestamp = DateTime.Now
                });

                // Raise event
                EntryProcessed?.Invoke(this, activity);

                await _logger.LogInfoAsync($"Entry processed successfully for vehicle: {vehicleNumber}");
                return activity;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to process entry for vehicle: {vehicleNumber}", ex);
                throw;
            }
        }

        private async Task SaveEntryToDatabase(ParkingActivity activity, string imagePath)
        {
            await _dbContext.EnsureConnectedAsync();

            try
            {
                using var cmd = new Npgsql.NpgsqlCommand(
                    @"INSERT INTO parking_activities 
                      (vehicle_number, action, entry_time, image_path, created_at)
                      VALUES (@vehicleNumber, @action, @entryTime, @imagePath, @createdAt)",
                    _dbContext.Connection);

                cmd.Parameters.AddWithValue("vehicleNumber", activity.VehicleNumber);
                cmd.Parameters.AddWithValue("action", activity.Action);
                cmd.Parameters.AddWithValue("entryTime", activity.Time);
                cmd.Parameters.AddWithValue("imagePath", imagePath);
                cmd.Parameters.AddWithValue("createdAt", DateTime.Now);

                await cmd.ExecuteNonQueryAsync();
            }
            finally
            {
                _dbContext.CloseConnection();
            }
        }

        private async Task PrintTicketAsync(ParkingActivity activity)
        {
            try
            {
                using var ms = new MemoryStream();
                using var writer = new StreamWriter(ms);
                
                // Write ticket content
                writer.WriteLine("PARKING TICKET");
                writer.WriteLine("------------------------");
                writer.WriteLine($"Vehicle: {activity.VehicleNumber}");
                writer.WriteLine($"Entry Time: {activity.Time:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine("------------------------");
                writer.WriteLine("Please keep this ticket safe");
                writer.WriteLine("Thank you for parking with us");
                
                writer.Flush();
                await _printer.PrintAsync(ms.ToArray());
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to print ticket", ex);
                throw;
            }
        }

        private string GenerateBarcode(string vehicleNumber)
        {
            // Format: PYYYYMMDDHHMMSS-VEHICLE
            return $"P{DateTime.Now:yyyyMMddHHmmss}-{vehicleNumber}";
        }
    }
} 