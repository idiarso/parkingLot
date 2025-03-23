using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using ParkingOut.Models;
using ParkingOut.Services;
using ParkingOut.Utils;

namespace ParkingOut.Services
{
    /// <summary>
    /// Implementation of IVehicleExitService.
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
            _logger = new AppLogger("VehicleExitService");
            _entryService = entryService;
            InitializeSampleData();
        }

        /// <inheritdoc/>
        public VehicleExit? CreateExit(string ticketNo, DateTime exitTime, string paymentMethod, string notes = "")
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
                
                var fee = CalculateFee(ticketNo);
                var exit = new VehicleExit(ticketNo, entry.LicensePlate, exitTime, fee, paymentMethod, notes);
                
                _exits.Add(exit);
                
                _logger.Info("Created exit with ticket number: {TicketNo}", ticketNo);
                
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
        public List<VehicleExit> GetAllExits()
        {
            try
            {
                _logger.Debug("Getting all exits");
                
                return _exits.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get all exits");
                return new List<VehicleExit>();
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
        public decimal CalculateFee(string ticketNo)
        {
            try
            {
                _logger.Debug("Calculating fee for ticket number: {TicketNo}", ticketNo);
                
                var entry = _entryService.GetEntryByTicketNo(ticketNo);
                if (entry == null)
                {
                    _logger.Warn("Cannot calculate fee: Entry not found for ticket number: {TicketNo}", ticketNo);
                    return 0;
                }
                
                // In a real application, this would calculate based on rates, duration, etc.
                // For now, we'll use a simple calculation
                var duration = DateTime.Now - entry.EntryTime;
                var hours = (int)Math.Ceiling(duration.TotalHours);
                
                decimal baseRate = 5.0m; // Base rate for first hour
                decimal hourlyRate = 2.0m; // Rate for each additional hour
                
                if (hours <= 0)
                {
                    return baseRate; // Minimum fee
                }
                
                return baseRate + (hourlyRate * (hours - 1));
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to calculate fee for ticket number: {TicketNo}", ticketNo);
                return 0;
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
                
                // Add some sample exits
                _exits.Add(new VehicleExit("T001", "ABC123", DateTime.Now.AddHours(-1), 10.00m, "Cash", "Sample exit 1"));
                _exits.Add(new VehicleExit("T002", "DEF456", DateTime.Now.AddHours(-2), 15.00m, "Credit Card", "Sample exit 2"));
                _exits.Add(new VehicleExit("T003", "GHI789", DateTime.Now.AddHours(-3), 20.00m, "Cash", "Sample exit 3"));
                
                _logger.Info("Sample data initialized with {Count} exits", _exits.Count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize sample data");
            }
        }
        
        /// <inheritdoc/>
        public decimal CalculateFee(string ticketNo)
        {
            try
            {
                _logger.Debug("Calculating fee for ticket: {0}", ticketNo);
                
                // Get the entry record
                var entry = _entryService.GetEntryByTicketNo(ticketNo);
                if (entry == null)
                {
                    _logger.Warn("Cannot calculate fee: Entry not found for ticket {0}", ticketNo);
                    return 0;
                }
                
                // Calculate the duration in hours
                var now = DateTime.Now;
                var duration = now - entry.EntryTime;
                var hours = Math.Ceiling(duration.TotalHours);
                
                // Get the hourly rate for this vehicle type
                decimal hourlyRate = _hourlyRates.ContainsKey(entry.VehicleType) 
                    ? _hourlyRates[entry.VehicleType] 
                    : _hourlyRates["Default"];
                
                // Calculate the fee
                decimal fee = (decimal)hours * hourlyRate;
                
                // Apply minimum fee
                fee = Math.Max(hourlyRate, fee);
                
                _logger.Info("Fee calculated for ticket {0}: {1:C} ({2} hours at {3:C}/hour)", 
                    ticketNo, fee, hours, hourlyRate);
                
                return fee;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to calculate fee for ticket: {0}", ticketNo);
                return 0;
            }
        }
    }
}