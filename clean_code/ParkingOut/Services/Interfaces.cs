using System;
using System.Collections.Generic;
using ParkingOut.Models;

namespace ParkingOut.Services
{
    /// <summary>
    /// Interface for application logging service
    /// </summary>
    public interface IAppLogger
    {
        /// <summary>
        /// Log debug message
        /// </summary>
        void Debug(string message, params object[] args);

        /// <summary>
        /// Log info message
        /// </summary>
        void Info(string message, params object[] args);

        /// <summary>
        /// Log warning message
        /// </summary>
        void Warn(string message, params object[] args);

        /// <summary>
        /// Log error message with exception
        /// </summary>
        void Error(Exception ex, string message, params object[] args);
    }

    /// <summary>
    /// Interface for vehicle entry service
    /// </summary>
    public interface IVehicleEntryService
    {
        /// <summary>
        /// Creates a new vehicle entry
        /// </summary>
        /// <param name="licensePlate">The license plate</param>
        /// <param name="vehicleType">The vehicle type</param>
        /// <param name="entryTime">The entry time</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created vehicle entry or null if failed</returns>
        VehicleEntry? CreateEntry(string licensePlate, string vehicleType, DateTime entryTime, string notes = "");

        /// <summary>
        /// Gets a vehicle entry by ticket number
        /// </summary>
        /// <param name="ticketNo">The ticket number</param>
        /// <returns>The vehicle entry or null if not found</returns>
        VehicleEntry? GetEntryByTicketNo(string ticketNo);

        /// <summary>
        /// Gets a vehicle entry by license plate
        /// </summary>
        /// <param name="licensePlate">The license plate</param>
        /// <returns>The vehicle entry or null if not found</returns>
        VehicleEntry? GetEntryByLicensePlate(string licensePlate);

        /// <summary>
        /// Gets all vehicle entries
        /// </summary>
        /// <returns>List of vehicle entries</returns>
        List<VehicleEntry> GetAllEntries();

        /// <summary>
        /// Gets recent vehicle entries
        /// </summary>
        /// <param name="count">Number of entries to return</param>
        /// <returns>List of recent vehicle entries</returns>
        List<VehicleEntry> GetRecentEntries(int count);
    }

    /// <summary>
    /// Interface for vehicle exit service
    /// </summary>
    public interface IVehicleExitService
    {
        /// <summary>
        /// Creates a new vehicle exit
        /// </summary>
        /// <param name="ticketNo">The ticket number</param>
        /// <param name="exitTime">The exit time</param>
        /// <param name="paymentMethod">The payment method</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The created vehicle exit or null if failed</returns>
        VehicleExit? CreateExit(string ticketNo, DateTime exitTime, string paymentMethod, string notes = "");

        /// <summary>
        /// Gets a vehicle exit by ticket number
        /// </summary>
        /// <param name="ticketNo">The ticket number</param>
        /// <returns>The vehicle exit or null if not found</returns>
        VehicleExit? GetExitByTicketNo(string ticketNo);

        /// <summary>
        /// Gets all vehicle exits
        /// </summary>
        /// <returns>List of vehicle exits</returns>
        List<VehicleExit> GetAllExits();

        /// <summary>
        /// Gets recent vehicle exits
        /// </summary>
        /// <param name="count">Number of exits to return</param>
        /// <returns>List of recent vehicle exits</returns>
        List<VehicleExit> GetRecentExits(int count);

        /// <summary>
        /// Calculates the parking fee
        /// </summary>
        /// <param name="ticketNo">The ticket number</param>
        /// <returns>The calculated fee</returns>
        decimal CalculateFee(string ticketNo);
    }
}