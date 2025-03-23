using System;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class Shift : ReactiveObject
    {
        private int _id;
        private string _name = string.Empty;
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _description = string.Empty;
        private DateTime _createdAt;
        private DateTime? _updatedAt;

        public int Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public TimeSpan StartTime
        {
            get => _startTime;
            set => this.RaiseAndSetIfChanged(ref _startTime, value);
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set => this.RaiseAndSetIfChanged(ref _endTime, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
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
    }
} 