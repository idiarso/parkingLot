using System;
using System.Collections.Generic;
using ParkingOut.Models;

namespace ParkingOut.Services
{
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