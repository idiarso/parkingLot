using System.Threading.Tasks;
using ParkingOut.Models;
using ParkingOut.Services;

namespace ParkingOut.Services
{
    /// <summary>
    /// Extension methods for IPrintService
    /// </summary>
    public static class PrintServiceExtensions
    {
        /// <summary>
        /// Prints a ticket for a vehicle entry (alias for PrintEntryTicket)
        /// </summary>
        /// <param name="printService">The print service</param>
        /// <param name="entry">The vehicle entry</param>
        /// <returns>True if printing successful, false otherwise</returns>
        public static Task<bool> PrintTicket(this IPrintService printService, VehicleEntry entry)
        {
            return printService.PrintEntryTicket(entry);
        }
    }
}