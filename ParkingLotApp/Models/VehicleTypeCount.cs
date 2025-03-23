using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class VehicleTypeCount : ReactiveObject
    {
        private string _type = string.Empty;
        private int _count;

        public string Type
        {
            get => _type;
            set => this.RaiseAndSetIfChanged(ref _type, value);
        }

        public int Count
        {
            get => _count;
            set => this.RaiseAndSetIfChanged(ref _count, value);
        }

        public VehicleTypeCount(string type, int count)
        {
            Type = type;
            Count = count;
        }
    }
} 