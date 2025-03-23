using System;

namespace ParkingOut.Models
{
    /// <summary>
    /// Represents statistics for a vehicle type
    /// </summary>
    public class VehicleTypeStats
    {
        /// <summary>
        /// Gets or sets the vehicle type
        /// </summary>
        public string VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the count of vehicles of this type
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Gets or sets the percentage of total vehicles
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Gets or sets the revenue generated from this vehicle type
        /// </summary>
        public decimal Revenue { get; set; }
    }
}