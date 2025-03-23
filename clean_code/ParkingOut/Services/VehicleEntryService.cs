using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ParkingOut.Models;
using ParkingOut.Services;

namespace ParkingOut.Services
{
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
        public List<VehicleEntry> GetAllEntries()
        {
            try
            {
                _logger.Debug("Getting all entries");
                
                return _entries.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get all entries");
                return new List<VehicleEntry>();
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
}