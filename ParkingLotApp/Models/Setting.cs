using System;
using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class Setting : ReactiveObject
    {
        private string _key = string.Empty;
        private string _value = string.Empty;
        private string _description = string.Empty;
        private int? _updatedBy;
        private DateTime? _updatedAt;

        public string Key
        {
            get => _key;
            set => this.RaiseAndSetIfChanged(ref _key, value);
        }

        public string Value
        {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value);
        }

        public int? UpdatedBy
        {
            get => _updatedBy;
            set => this.RaiseAndSetIfChanged(ref _updatedBy, value);
        }

        public DateTime? UpdatedAt
        {
            get => _updatedAt;
            set => this.RaiseAndSetIfChanged(ref _updatedAt, value);
        }
    }
} 