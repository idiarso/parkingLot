using System;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class ParkingActivity : ReactiveObject
    {
        private int _id;
        private string _vehicleNumber = string.Empty;
        private string _vehicleType = string.Empty;
        private string _action = string.Empty;
        private DateTime _entryTime = DateTime.Now;
        private DateTime? _exitTime;
        private string? _duration;
        private decimal? _fee;
        private string? _notes;
        private string? _imagePath;
        private string? _barcode;
        private DateTime _createdAt = DateTime.Now;
        private DateTime? _updatedAt;
        private string _formattedTime = string.Empty;

        public int Id 
        { 
            get => _id; 
            set => this.RaiseAndSetIfChanged(ref _id, value); 
        }
        
        public string VehicleNumber 
        { 
            get => _vehicleNumber; 
            set => this.RaiseAndSetIfChanged(ref _vehicleNumber, value); 
        }
        
        public string VehicleType 
        { 
            get => _vehicleType; 
            set => this.RaiseAndSetIfChanged(ref _vehicleType, value); 
        }
        
        public string Action 
        { 
            get => _action; 
            set => this.RaiseAndSetIfChanged(ref _action, value); 
        }
        
        public DateTime EntryTime 
        { 
            get => _entryTime; 
            set => this.RaiseAndSetIfChanged(ref _entryTime, value); 
        }
        
        public DateTime? ExitTime 
        { 
            get => _exitTime; 
            set => this.RaiseAndSetIfChanged(ref _exitTime, value); 
        }
        
        public string? Duration 
        { 
            get => _duration; 
            set => this.RaiseAndSetIfChanged(ref _duration, value); 
        }
        
        public decimal? Fee 
        { 
            get => _fee; 
            set => this.RaiseAndSetIfChanged(ref _fee, value); 
        }
        
        public string? Notes 
        { 
            get => _notes; 
            set => this.RaiseAndSetIfChanged(ref _notes, value); 
        }
        
        public string? ImagePath 
        { 
            get => _imagePath; 
            set => this.RaiseAndSetIfChanged(ref _imagePath, value); 
        }
        
        public string? Barcode 
        { 
            get => _barcode; 
            set => this.RaiseAndSetIfChanged(ref _barcode, value); 
        }
        
        public DateTime CreatedAt 
        { 
            get => _createdAt; 
            set => this.RaiseAndSetIfChanged(ref _createdAt, value); 
        }
        
        public DateTime? UpdatedAt 
        { 
            get => _updatedAt; 
            set => this.RaiseAndSetIfChanged(ref _updatedAt, value); 
        }

        public string FormattedTime
        {
            get => _formattedTime;
            set => this.RaiseAndSetIfChanged(ref _formattedTime, value);
        }

        // Add Time property for backward compatibility (with getter and setter)
        public DateTime Time 
        { 
            get => EntryTime; 
            set => EntryTime = value; 
        }

        public ParkingActivity()
        {
            EntryTime = DateTime.Now;
            FormattedTime = EntryTime.ToString("HH:mm");
        }

        public ParkingActivity(DateTime entryTime, string vehicleNumber, string vehicleType, string action, string? duration = null, decimal? fee = null)
        {
            EntryTime = entryTime;
            VehicleNumber = vehicleNumber;
            VehicleType = vehicleType;
            Action = action;
            Duration = duration;
            Fee = fee;
            FormattedTime = entryTime.ToString("HH:mm");
            CreatedAt = DateTime.Now;
        }
    }
} 