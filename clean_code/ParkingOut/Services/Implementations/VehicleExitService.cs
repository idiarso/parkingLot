using System;
using System.Collections.Generic;
using System.Linq;
using ParkingOut.Models;
using ParkingOut.Utils;

namespace ParkingOut.Services.Implementations
{
    /// <summary>
    /// Implementation of IVehicleExitService
    /// </summary>
    public class VehicleExitService : IVehicleExitService
    {
        private readonly IAppLogger _logger;
        private readonly List<VehicleExit> _exits = new List<VehicleExit>();
        private readonly IVehicleEntryService _entryService;
        private int _exitCounter = 1;

        // Default hourly rates by vehicle type
        private readonly Dictionary<string, decimal> _hourlyRates = new Dictionary<string, decimal>
        {
            { "Car", 5.00m },
            { "Motorcycle", 2.00m },
            { "Truck", 10.00m },
            { "Bus", 8.00m },
            { "Default", 5.00m }
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleExitService"/> class.
        /// </summary>
        /// <param name="entryService">The vehicle entry service to use.</param>
        public VehicleExitService(IVehicleEntryService entryService)
        {
            _logger = new Services.AppLogger("VehicleExitService");
            _entryService = entryService;
            InitializeSampleData();
        }

        /// <inheritdoc/>
        public VehicleExit? CreateExit(string ticketNo, DateTime exitTime, string paymentMethod, string notes = "")
        {
            try
            {
                _logger.Debug($"Creating exit for ticket number: {ticketNo}");
                
                var entry = _entryService.GetEntryByTicketNo(ticketNo);
                if (entry == null)
                {
                    _logger.Warning($"No entry found for ticket number: {ticketNo}");
                    return null;
                }
                
                var fee = CalculateFee(ticketNo);
                var exit = new VehicleExit(ticketNo, entry.LicensePlate, entry.VehicleType, entry.EntryTime, exitTime, fee, paymentMethod, notes);
                
                _exits.Add(exit);
                
                _logger.Info($"Created exit with ticket number: {ticketNo}");
                
                return exit;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create exit for ticket number: {ticketNo}", ex);
                return null;
            }
        }

        /// <inheritdoc/>
        public VehicleExit? GetExitByTicketNo(string ticketNo)
        {
            return _exits.FirstOrDefault(e => e.TicketNo == ticketNo);
        }

        /// <inheritdoc/>
        public List<VehicleExit> GetAllExits()
        {
            return _exits.ToList();
        }

        /// <inheritdoc/>
        public List<VehicleExit> GetRecentExits(int count)
        {
            return _exits.OrderByDescending(e => e.ExitTime).Take(count).ToList();
        }

        /// <inheritdoc/>
        public decimal CalculateFee(string ticketNo)
        {
            var entry = _entryService.GetEntryByTicketNo(ticketNo);
            if (entry == null)
            {
                return 0;
            }
            
            var hourlyRate = _hourlyRates.ContainsKey(entry.VehicleType) 
                ? _hourlyRates[entry.VehicleType] 
                : _hourlyRates["Default"];
            
            var duration = DateTime.Now - entry.EntryTime;
            var hours = Math.Ceiling(duration.TotalHours);
            
            return (decimal)hours * hourlyRate;
        }

        /// <summary>
        /// Initializes sample data for testing
        /// </summary>
        private void InitializeSampleData()
        {
            // Add some sample exits
            _exits.Add(new VehicleExit("T000001", "B1234CD", "Car", DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1), 10.00m, "Cash"));
            
            _exitCounter = 2;
        }
    }
}