using System.Collections.Generic;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class ParkingStatistics : ReactiveObject
    {
        private int _totalSpots;
        private int _occupiedSpots;
        private int _availableSpots;
        private decimal _todayRevenue;
        private decimal _weekRevenue;
        private decimal _monthRevenue;
        private Dictionary<string, int> _vehicleTypes = new();

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

        public Dictionary<string, int> VehicleTypes 
        { 
            get => _vehicleTypes;
            set => this.RaiseAndSetIfChanged(ref _vehicleTypes, value);
        }
    }
} 