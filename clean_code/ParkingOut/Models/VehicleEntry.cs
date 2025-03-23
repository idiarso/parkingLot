using System;

namespace ParkingOut.Models
{
    /// <summary>
    /// Represents a vehicle entry record
    /// </summary>
    public class VehicleEntry
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleEntry"/> class.
        /// </summary>
        public VehicleEntry(string ticketNo, string licensePlate, string vehicleType, DateTime entryTime, string notes = "")
        {
            TicketNo = ticketNo;
            LicensePlate = licensePlate;
            VehicleType = vehicleType;
            EntryTime = entryTime;
            Notes = notes;
        }

        /// <summary>
        /// Gets or sets the ticket number
        /// </summary>
        public string TicketNo { get; set; }

        /// <summary>
        /// Gets or sets the license plate
        /// </summary>
        public string LicensePlate { get; set; }

        /// <summary>
        /// Gets or sets the vehicle type
        /// </summary>
        public string VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the entry time
        /// </summary>
        public DateTime EntryTime { get; set; }

        /// <summary>
        /// Gets or sets any notes about the entry
        /// </summary>
        public string Notes { get; set; }
    }
}