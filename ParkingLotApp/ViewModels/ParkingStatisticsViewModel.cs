using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ReactiveUI;
using ParkingLotApp.Models;

namespace ParkingLotApp.ViewModels
{
    public class ParkingStatisticsViewModel : ViewModelBase
    {
        private DateTime _selectedDate;
        private string _selectedPeriod = "Today";
        private int _totalVehicles;
        private double _currentOccupancy;
        private decimal _totalRevenue;
        private string _averageDuration = "0 hours";
        private Dictionary<string, int> _vehicleTypeDistribution = new();
        private Dictionary<int, int> _hourlyDistribution = new();
        private ObservableCollection<ParkingActivity> _activityLog = new();

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
        }

        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set => this.RaiseAndSetIfChanged(ref _selectedPeriod, value);
        }

        public int TotalVehicles
        {
            get => _totalVehicles;
            set => this.RaiseAndSetIfChanged(ref _totalVehicles, value);
        }

        public double CurrentOccupancy
        {
            get => _currentOccupancy;
            set => this.RaiseAndSetIfChanged(ref _currentOccupancy, value);
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => this.RaiseAndSetIfChanged(ref _totalRevenue, value);
        }

        public string AverageDuration
        {
            get => _averageDuration;
            set => this.RaiseAndSetIfChanged(ref _averageDuration, value);
        }

        public Dictionary<string, int> VehicleTypeDistribution
        {
            get => _vehicleTypeDistribution;
            set => this.RaiseAndSetIfChanged(ref _vehicleTypeDistribution, value);
        }

        public Dictionary<int, int> HourlyDistribution
        {
            get => _hourlyDistribution;
            set => this.RaiseAndSetIfChanged(ref _hourlyDistribution, value);
        }

        public ObservableCollection<ParkingActivity> ActivityLog
        {
            get => _activityLog;
            set => this.RaiseAndSetIfChanged(ref _activityLog, value);
        }

        public ObservableCollection<string> Periods { get; } = new()
        {
            "Today",
            "This Week",
            "This Month",
            "Custom Range"
        };

        public ICommand RefreshCommand { get; }

        public ParkingStatisticsViewModel()
        {
            _selectedDate = DateTime.Today;
            _selectedPeriod = "Today";
            _vehicleTypeDistribution = new Dictionary<string, int>();
            _hourlyDistribution = new Dictionary<int, int>();
            _activityLog = new ObservableCollection<ParkingActivity>();

            RefreshCommand = ReactiveCommand.Create(LoadStatistics);

            // Initial load
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            // TODO: Replace with actual database queries
            // For now, using sample data
            TotalVehicles = 150;
            CurrentOccupancy = 45.5;
            TotalRevenue = 7500000;
            AverageDuration = "2.5 hours";

            VehicleTypeDistribution = new Dictionary<string, int>
            {
                { "Car", 80 },
                { "Motorcycle", 45 },
                { "Truck", 15 },
                { "Bus", 10 }
            };

            HourlyDistribution = new Dictionary<int, int>();
            for (int i = 0; i < 24; i++)
            {
                HourlyDistribution[i] = new Random().Next(5, 30);
            }

            ActivityLog.Clear();
            // Add sample activities
            ActivityLog.Add(new ParkingActivity
            {
                Time = DateTime.Now.AddHours(-1),
                VehicleNumber = "B 1234 CD",
                VehicleType = "Car",
                Action = "Entry"
            });
            ActivityLog.Add(new ParkingActivity
            {
                Time = DateTime.Now.AddHours(-2),
                VehicleNumber = "B 5678 EF",
                VehicleType = "Motorcycle",
                Action = "Exit",
                Duration = "1.5 hours",
                Fee = 15000
            });
        }
    }
} 