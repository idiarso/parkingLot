using System;
using System.Collections.Generic;
using System.Linq;
using ParkingOut.Models;
using ParkingOut.Utils;

namespace ParkingOut.Services.Implementations
{
    /// <summary>
    /// Implementation of IVehicleEntryService
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
            _logger = new Services.AppLogger("VehicleEntryService");
            InitializeSampleData();
        }

        /// <inheritdoc/>
        public VehicleEntry? CreateEntry(string licensePlate, string vehicleType, DateTime entryTime, string notes = "")
        {
            try
            {
                _logger.Debug($"Creating entry for license plate: {licensePlate}");
                
                var ticketNo = GenerateTicketNumber();
                var entry = new VehicleEntry(ticketNo, licensePlate, vehicleType, entryTime, notes);
                
                _entries.Add(entry);
                
                _logger.Info($"Created entry with ticket number: {ticketNo}");
                
                return entry;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create entry for license plate: {licensePlate}", ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public VehicleEntry? GetEntryByTicketNo(string ticketNo)
        {
            return _entries.FirstOrDefault(e => e.TicketNo == ticketNo);
        }

        /// <inheritdoc/>
        public VehicleEntry? GetEntryByLicensePlate(string licensePlate)
        {
            return _entries.FirstOrDefault(e => e.LicensePlate == licensePlate);
        }

        /// <inheritdoc/>
        public List<VehicleEntry> GetAllEntries()
        {
            return _entries.ToList();
        }

        /// <inheritdoc/>
        public List<VehicleEntry> GetRecentEntries(int count)
        {
            return _entries.OrderByDescending(e => e.EntryTime).Take(count).ToList();
        }

        /// <summary>
        /// Generates a unique ticket number
        /// </summary>
        /// <returns>The generated ticket number</returns>
        private string GenerateTicketNumber()
        {
            return $"T{_ticketCounter++:D6}";
        }

        /// <summary>
        /// Initializes sample data for testing
        /// </summary>
        private void InitializeSampleData()
        {
            // Add some sample entries
            _entries.Add(new VehicleEntry("T000001", "B1234CD", "Car", DateTime.Now.AddHours(-1)));
            _entries.Add(new VehicleEntry("T000002", "D5678EF", "Motorcycle", DateTime.Now.AddMinutes(-20)));
            _entries.Add(new VehicleEntry("T000003", "F9012GH", "Truck", DateTime.Now.AddMinutes(-5)));
            
            _ticketCounter = 4;
        }
    }
}