using System;

namespace ParkingOut.Models
{
    /// <summary>
    /// Represents an activity log item for the dashboard
    /// </summary>
    public class ActivityLogItem
    {
        /// <summary>
        /// Gets or sets the ID of the activity
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the activity
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the type of activity (Entry, Exit, Payment, etc.)
        /// </summary>
        public string? ActivityType { get; set; }

        /// <summary>
        /// Gets or sets the ticket number associated with the activity
        /// </summary>
        public string? TicketNo { get; set; }

        /// <summary>
        /// Gets or sets the vehicle type associated with the activity
        /// </summary>
        public string? VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the user who performed the activity
        /// </summary>
        public string? User { get; set; }

        /// <summary>
        /// Gets or sets the license plate associated with the activity (if applicable)
        /// </summary>
        public string? LicensePlate { get; set; }
    }
}