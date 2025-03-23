using System;

namespace TestWpfApp.Models
{
    public class ParkingActivity
    {
        public DateTime Time { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string Action { get; set; }
        public string Duration { get; set; }
        public decimal Fee { get; set; }

        public ParkingActivity()
        {
            Time = DateTime.Now;
            Duration = "N/A";
            Fee = 0;
        }

        public ParkingActivity(DateTime time, string vehicleNumber, string vehicleType, string action, string duration, decimal fee)
        {
            Time = time;
            VehicleNumber = vehicleNumber;
            VehicleType = vehicleType;
            Action = action;
            Duration = duration;
            Fee = fee;
        }
    }
}
