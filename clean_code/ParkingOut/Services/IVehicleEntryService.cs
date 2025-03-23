using System;
using System.Collections.Generic;
using ParkingOut.Models;

namespace ParkingOut.Services
{
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
}