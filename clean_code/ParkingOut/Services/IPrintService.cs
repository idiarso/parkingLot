using System;
using System.Threading.Tasks;
using ParkingOut.Models;

namespace ParkingOut.Services
{
    /// <summary>
    /// Interface for printing service
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// Prints an entry ticket
        /// </summary>
        /// <param name="entry">The vehicle entry details</param>
        /// <returns>True if printing successful, false otherwise</returns>
        Task<bool> PrintEntryTicket(VehicleEntry entry);

        /// <summary>
        /// Prints an exit receipt
        /// </summary>
        /// <param name="exit">The vehicle exit details</param>
        /// <returns>True if printing successful, false otherwise</returns>
        Task<bool> PrintExitReceipt(VehicleExit exit);

        /// <summary>
        /// Prints a daily report
        /// </summary>
        /// <param name="date">The date for the report</param>
        /// <returns>True if printing successful, false otherwise</returns>
        Task<bool> PrintDailyReport(DateTime date);

        /// <summary>
        /// Prints a monthly report
        /// </summary>
        /// <param name="year">The year</param>
        /// <param name="month">The month</param>
        /// <returns>True if printing successful, false otherwise</returns>
        Task<bool> PrintMonthlyReport(int year, int month);

        /// <summary>
        /// Gets the default printer name
        /// </summary>
        /// <returns>The default printer name</returns>
        string GetDefaultPrinter();

        /// <summary>
        /// Sets the default printer
        /// </summary>
        /// <param name="printerName">The printer name to set as default</param>
        /// <returns>True if successful, false otherwise</returns>
        bool SetDefaultPrinter(string printerName);
    }
}