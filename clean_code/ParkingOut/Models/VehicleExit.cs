using System;

namespace ParkingOut.Models
{
    /// <summary>
    /// Represents a vehicle exit record
    /// </summary>
    public class VehicleExit
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleExit"/> class.
        /// </summary>
        public VehicleExit(string ticketNo, string licensePlate, DateTime exitTime, decimal fee, string paymentMethod, string notes = "")
        {
            TicketNo = ticketNo;
            LicensePlate = licensePlate;
            ExitTime = exitTime;
            Fee = fee;
            PaymentMethod = paymentMethod;
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
        /// Gets or sets the exit time
        /// </summary>
        public DateTime ExitTime { get; set; }

        /// <summary>
        /// Gets or sets the parking fee
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Gets or sets the payment method
        /// </summary>
        public string PaymentMethod { get; set; }

        /// <summary>
        /// Gets or sets any notes about the exit
        /// </summary>
        public string Notes { get; set; }
    }
}