using ReactiveUI;

namespace ParkingLotApp.Models
{
    public class VehicleDistributionItem : ReactiveObject
    {
        private string _vehicleType = string.Empty;
        private int _count;
        private string _color = "#3498db"; // Default color

        public string VehicleType
        {
            get => _vehicleType;
            set => this.RaiseAndSetIfChanged(ref _vehicleType, value);
        }

        public int Count
        {
            get => _count;
            set => this.RaiseAndSetIfChanged(ref _count, value);
        }

        public string Color
        {
            get => _color;
            set => this.RaiseAndSetIfChanged(ref _color, value);
        }

        public VehicleDistributionItem(string vehicleType, int count, string color = "#3498db")
        {
            VehicleType = vehicleType;
            Count = count;
            Color = color;
        }
    }
} 