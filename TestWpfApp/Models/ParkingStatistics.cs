using System.Collections.Generic;

namespace TestWpfApp.Models
{
    public class ParkingStatistics
    {
        public int TotalSpots { get; set; }
        public int OccupiedSpots { get; set; }
        public int AvailableSpots { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public Dictionary<string, int> VehicleTypes { get; set; }
    }
}
