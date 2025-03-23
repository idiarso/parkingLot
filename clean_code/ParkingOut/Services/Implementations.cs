using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using NLog;
using ParkingOut.Models;

namespace ParkingOut.Services
{
    /// <summary>
    /// Implementation of IAppLogger using NLog.
    /// </summary>
    public class AppLogger : IAppLogger
    {
        private readonly Logger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLogger"/> class.
        /// </summary>
        /// <param name="loggerName">The name of the logger.</param>
        public AppLogger(string loggerName)
        {
            _logger = LogManager.GetLogger(loggerName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AppLogger"/> class.
        /// </summary>
        /// <param name="type">The type to use for the logger name.</param>
        public AppLogger(Type type)
        {
            _logger = LogManager.GetLogger(type.Name);
        }

        /// <inheritdoc/>
        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        /// <inheritdoc/>
        public void Info(string message, params object[] args)
        {
            _logger.Info(message, args);
        }

        /// <inheritdoc/>
        public void Warn(string message, params object[] args)
        {
            _logger.Warn(message, args);
        }

        /// <inheritdoc/>
        public void Error(Exception ex, string message, params object[] args)
        {
            _logger.Error(ex, message, args);
        }
    }

    /// <summary>
    /// Implementation of IVehicleEntryService.
    /// </summary>
    public class VehicleEntryService : IVehicleEntryService
    {
        private readonly IAppLogger _logger;
        private readonly List<VehicleEntry> _entries = new List<VehicleEntry>();
        private int _ticketCounter = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleEntryService"/> class.
        /// </summary>
        public VehicleEntryService()
        {
            _logger = new AppLogger("VehicleEntryService");
            InitializeSampleData();
        }

        /// <inheritdoc/>
        public VehicleEntry? CreateEntry(string licensePlate, string vehicleType, DateTime entryTime, string notes = "")
        {
            try
            {
                _logger.Debug("Creating entry for license plate: {LicensePlate}", licensePlate);
                
                var ticketNo = GenerateTicketNumber();
                var entry = new VehicleEntry(ticketNo, licensePlate, vehicleType, entryTime, notes);
                
                _entries.Add(entry);
                
                _logger.Info("Created entry with ticket number: {TicketNo}", ticketNo);
                
                return entry;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create entry for license plate: {LicensePlate}", licensePlate);
                return null;
            }
        }

        /// <inheritdoc/>
        public VehicleEntry? GetEntryByTicketNo(string ticketNo)
        {
            try
            {
                _logger.Debug("Getting entry by ticket number: {TicketNo}", ticketNo);
                
                var entry = _entries.FirstOrDefault(e => e.TicketNo == ticketNo);
                
                if (entry == null)
                {
                    _logger.Warn("Entry not found for ticket number: {TicketNo}", ticketNo);
                }
                
                return entry;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get entry by ticket number: {TicketNo}", ticketNo);
                return null;
            }
        }

        /// <inheritdoc/>
        public VehicleEntry? GetEntryByLicensePlate(string licensePlate)
        {
            try
            {
                _logger.Debug("Getting entry by license plate: {LicensePlate}", licensePlate);
                
                var entry = _entries.FirstOrDefault(e => e.LicensePlate == licensePlate);
                
                if (entry == null)
                {
                    _logger.Warn("Entry not found for license plate: {LicensePlate}", licensePlate);
                }
                
                return entry;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get entry by license plate: {LicensePlate}", licensePlate);
                return null;
            }
        }

        /// <inheritdoc/>
        public List<VehicleEntry> GetRecentEntries(int count)
        {
            try
            {
                _logger.Debug("Getting recent entries, count: {Count}", count);
                
                return _entries.OrderByDescending(e => e.EntryTime).Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get recent entries");
                return new List<VehicleEntry>();
            }
        }

        /// <summary>
        /// Generates a new ticket number.
        /// </summary>
        /// <returns>The generated ticket number.</returns>
        private string GenerateTicketNumber()
        {
            return $"T{_ticketCounter++:D6}";
        }

        /// <summary>
        /// Initializes sample data for testing.
        /// </summary>
        private void InitializeSampleData()
        {
            try
            {
                _logger.Debug("Initializing sample data");
                
                // Add some sample entries
                _entries.Add(new VehicleEntry("T000001", "B1234CD", "Car", DateTime.Now.AddHours(-2), "Sample entry 1"));
                _entries.Add(new VehicleEntry("T000002", "D5678EF", "Motorcycle", DateTime.Now.AddHours(-1), "Sample entry 2"));
                _entries.Add(new VehicleEntry("T000003", "F9012GH", "Truck", DateTime.Now.AddMinutes(-30), "Sample entry 3"));
                
                _ticketCounter = 4;
                
                _logger.Info("Sample data initialized with {Count} entries", _entries.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize sample data");
            }
        }
    }

    /// <summary>
    /// Implementation of IVehicleExitService.
    /// </summary>
    public class VehicleExitService : IVehicleExitService
    {
        private readonly IAppLogger _logger;
        private readonly List<VehicleExit> _exits = new List<VehicleExit>();
        private readonly IVehicleEntryService _entryService;
        private int _exitCounter = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleExitService"/> class.
        /// </summary>
        /// <param name="entryService">The vehicle entry service to use.</param>
        public VehicleExitService(IVehicleEntryService entryService)
        {
            _logger = new AppLogger("VehicleExitService");
            _entryService = entryService;
            InitializeSampleData();
        }

        /// <inheritdoc/>
        public VehicleExit? CreateExit(string ticketNo, DateTime exitTime, decimal fee, string notes = "")
        {
            try
            {
                _logger.Debug("Creating exit for ticket number: {TicketNo}", ticketNo);
                
                var entry = _entryService.GetEntryByTicketNo(ticketNo);
                if (entry == null)
                {
                    _logger.Warn("Cannot create exit: Entry not found for ticket number: {TicketNo}", ticketNo);
                    return null;
                }
                
                var exitId = GenerateExitId();
                var baseFee = CalculateBaseFee(entry.VehicleType, entry.EntryTime, exitTime);
                var additionalFee = fee - baseFee;
                
                var exit = new VehicleExit(
                    exitId, 
                    ticketNo, 
                    entry.LicensePlate, 
                    entry.VehicleType, 
                    entry.EntryTime, 
                    exitTime, 
                    baseFee, 
                    additionalFee, 
                    notes);
                
                _exits.Add(exit);
                
                _logger.Info("Created exit with exit ID: {ExitId}", exitId);
                
                return exit;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to create exit for ticket number: {TicketNo}", ticketNo);
                return null;
            }
        }

        /// <inheritdoc/>
        public VehicleExit? GetExitByTicketNo(string ticketNo)
        {
            try
            {
                _logger.Debug("Getting exit by ticket number: {TicketNo}", ticketNo);
                
                var exit = _exits.FirstOrDefault(e => e.TicketNo == ticketNo);
                
                if (exit == null)
                {
                    _logger.Warn("Exit not found for ticket number: {TicketNo}", ticketNo);
                }
                
                return exit;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get exit by ticket number: {TicketNo}", ticketNo);
                return null;
            }
        }

        /// <inheritdoc/>
        public List<VehicleExit> GetRecentExits(int count)
        {
            try
            {
                _logger.Debug("Getting recent exits, count: {Count}", count);
                
                return _exits.OrderByDescending(e => e.ExitTime).Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get recent exits");
                return new List<VehicleExit>();
            }
        }

        /// <inheritdoc/>
        public decimal CalculateFee(VehicleEntry entry, DateTime exitTime)
        {
            try
            {
                _logger.Debug("Calculating fee for ticket number: {TicketNo}", entry.TicketNo);
                
                return CalculateBaseFee(entry.VehicleType, entry.EntryTime, exitTime);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to calculate fee for ticket number: {TicketNo}", entry.TicketNo);
                return 0;
            }
        }

        /// <summary>
        /// Generates a new exit ID.
        /// </summary>
        /// <returns>The generated exit ID.</returns>
        private string GenerateExitId()
        {
            return $"E{_exitCounter++:D6}";
        }

        /// <summary>
        /// Calculates the base fee based on vehicle type and duration.
        /// </summary>
        /// <param name="vehicleType">The vehicle type.</param>
        /// <param name="entryTime">The entry time.</param>
        /// <param name="exitTime">The exit time.</param>
        /// <returns>The calculated base fee.</returns>
        private decimal CalculateBaseFee(string vehicleType, DateTime entryTime, DateTime exitTime)
        {
            try
            {
                _logger.Debug("Calculating base fee for vehicle type: {VehicleType}", vehicleType);
                
                var duration = exitTime - entryTime;
                var hours = (int)Math.Ceiling(duration.TotalHours);
                
                if (hours <= 0)
                {
                    return 0;
                }
                
                decimal baseRate = GetBaseRate(vehicleType);
                decimal hourlyRate = GetHourlyRate(vehicleType);
                
                decimal fee = baseRate;
                
                if (hours > 1)
                {
                    fee += (hours - 1) * hourlyRate;
                }
                
                _logger.Debug("Calculated base fee: {Fee} for duration: {Hours} hours", fee, hours);
                
                return fee;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to calculate base fee for vehicle type: {VehicleType}", vehicleType);
                return 0;
            }
        }

        /// <summary>
        /// Gets the base rate for the first hour based on vehicle type.
        /// </summary>
        /// <param name="vehicleType">The vehicle type.</param>
        /// <returns>The base rate.</returns>
        private decimal GetBaseRate(string vehicleType)
        {
            switch (vehicleType)
            {
                case "Motorcycle":
                    return 3.00m;
                case "Car":
                    return 5.00m;
                case "Truck":
                    return 10.00m;
                case "Bus":
                    return 15.00m;
                default:
                    return 5.00m;
            }
        }

        /// <summary>
        /// Gets the hourly rate for additional hours based on vehicle type.
        /// </summary>
        /// <param name="vehicleType">The vehicle type.</param>
        /// <returns>The hourly rate.</returns>
        private decimal GetHourlyRate(string vehicleType)
        {
            switch (vehicleType)
            {
                case "Motorcycle":
                    return 1.50m;
                case "Car":
                    return 2.50m;
                case "Truck":
                    return 5.00m;
                case "Bus":
                    return 7.50m;
                default:
                    return 2.50m;
            }
        }

        /// <summary>
        /// Initializes sample data for testing.
        /// </summary>
        private void InitializeSampleData()
        {
            try
            {
                _logger.Debug("Initializing sample data");
                
                // We don't need sample exits for now
                
                _logger.Info("Sample data initialized");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize sample data");
            }
        }
    }

    /// <summary>
    /// Implementation of IPrintService.
    /// </summary>
    public class PrintService : IPrintService
    {
        private readonly IAppLogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrintService"/> class.
        /// </summary>
        public PrintService()
        {
            _logger = new AppLogger("PrintService");
        }

        /// <inheritdoc/>
        public bool PrintTicket(VehicleEntry entry)
        {
            try
            {
                _logger.Debug("Printing ticket for ticket number: {TicketNo}", entry.TicketNo);
                
                // In a real application, this would print to a physical printer
                // For now, we'll just log the ticket details
                
                _logger.Info("Ticket printed for ticket number: {TicketNo}", entry.TicketNo);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to print ticket for ticket number: {TicketNo}", entry.TicketNo);
                return false;
            }
        }

        /// <inheritdoc/>
        public bool PrintReceipt(VehicleExit exit)
        {
            try
            {
                _logger.Debug("Printing receipt for exit ID: {ExitId}", exit.ExitId);
                
                // In a real application, this would print to a physical printer
                // For now, we'll just log the receipt details
                
                _logger.Info("Receipt printed for exit ID: {ExitId}", exit.ExitId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to print receipt for exit ID: {ExitId}", exit.ExitId);
                return false;
            }
        }
    }

    /// <summary>
    /// Implementation of ICameraService.
    /// </summary>
    public class CameraService : ICameraService
    {
        private readonly IAppLogger _logger;
        private Image? _imageControl;
        private bool _isRunning = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="CameraService"/> class.
        /// </summary>
        public CameraService()
        {
            _logger = new AppLogger("CameraService");
        }

        /// <inheritdoc/>
        public bool IsRunning => _isRunning;

        /// <inheritdoc/>
        public void Start()
        {
            try
            {
                _logger.Debug("Starting camera");
                
                // In a real application, this would initialize the camera hardware
                _isRunning = true;
                
                _logger.Info("Camera started");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to start camera");
                throw;
            }
        }

        /// <inheritdoc/>
        public void Stop()
        {
            try
            {
                _logger.Debug("Stopping camera");
                
                // In a real application, this would release the camera hardware
                _isRunning = false;
                
                _logger.Info("Camera stopped");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to stop camera");
                throw;
            }
        }

        /// <inheritdoc/>
        public void SetImageControl(Image imageControl)
        {
            try
            {
                _logger.Debug("Setting image control");
                
                _imageControl = imageControl;
                
                _logger.Debug("Image control set");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to set image control");
                throw;
            }
        }

        /// <inheritdoc/>
        public BitmapSource? CaptureImage()
        {
            try
            {
                _logger.Debug("Capturing image");
                
                if (!_isRunning)
                {
                    _logger.Warn("Cannot capture image: Camera is not running");
                    return null;
                }
                
                // In a real application, this would capture an image from the camera
                // For now, we'll just return null
                
                _logger.Info("Image captured");
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to capture image");
                return null;
            }
        }

        /// <inheritdoc/>
        public string? RecognizePlate()
        {
            try
            {
                _logger.Debug("Recognizing license plate");
                
                if (!_isRunning)
                {
                    _logger.Warn("Cannot recognize plate: Camera is not running");
                    return null;
                }
                
                // In a real application, this would use ALPR (Automatic License Plate Recognition)
                // For now, we'll just return a sample license plate
                
                var plate = "B" + new Random().Next(1000, 9999) + "XYZ";
                
                _logger.Info("License plate recognized: {Plate}", plate);
                
                return plate;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to recognize license plate");
                return null;
            }
        }
    }
}