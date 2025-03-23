using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace ParkingLotApp.Models
{
    public class DashboardStatistics : ReactiveObject
    {
        private int _totalSpots;
        private int _occupiedSpots;
        private int _availableSpots;
        private decimal _todayRevenue;
        private decimal _weekRevenue;
        private decimal _monthRevenue;
        private ObservableCollection<VehicleDistributionItem> _vehicleDistribution = new();
        private DateTime _lastUpdated = DateTime.Now;

        public int TotalSpots
        {
            get => _totalSpots;
            set => this.RaiseAndSetIfChanged(ref _totalSpots, value);
        }

        public int OccupiedSpots
        {
            get => _occupiedSpots;
            set => this.RaiseAndSetIfChanged(ref _occupiedSpots, value);
        }

        public int AvailableSpots
        {
            get => _availableSpots;
            set => this.RaiseAndSetIfChanged(ref _availableSpots, value);
        }

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set => this.RaiseAndSetIfChanged(ref _todayRevenue, value);
        }

        public decimal WeekRevenue
        {
            get => _weekRevenue;
            set => this.RaiseAndSetIfChanged(ref _weekRevenue, value);
        }

        public decimal MonthRevenue
        {
            get => _monthRevenue;
            set => this.RaiseAndSetIfChanged(ref _monthRevenue, value);
        }

        public ObservableCollection<VehicleDistributionItem> VehicleDistribution
        {
            get => _vehicleDistribution;
            set => this.RaiseAndSetIfChanged(ref _vehicleDistribution, value);
        }

        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => this.RaiseAndSetIfChanged(ref _lastUpdated, value);
        }

        public string FormattedTodayRevenue => $"Rp {TodayRevenue:N0}";
        public string FormattedWeekRevenue => $"Rp {WeekRevenue:N0}";
        public string FormattedMonthRevenue => $"Rp {MonthRevenue:N0}";
        public string FormattedLastUpdated => LastUpdated.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Percentage total occupancy (0 to 100)
        public int OccupancyPercentage => TotalSpots > 0 ? (int)Math.Round((double)OccupiedSpots / TotalSpots * 100) : 0;
    }
} 