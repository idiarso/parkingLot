using System;

namespace ParkingIN
{
    public class AppConfig
    {
        public EntrySettings EntrySettings { get; set; }
        public StationConfig StationConfig { get; set; }
        public PrinterSettings PrinterSettings { get; set; }
        
        public AppConfig()
        {
            // Initialize with default values
            EntrySettings = new EntrySettings();
            StationConfig = new StationConfig();
            PrinterSettings = new PrinterSettings();
        }
    }
    
    public class EntrySettings
    {
        public bool AutoPrint { get; set; }
        public string DefaultVehicleType { get; set; }
        
        public EntrySettings()
        {
            // Default values
            AutoPrint = true;
            DefaultVehicleType = "Car";
        }
    }
    
    public class StationConfig
    {
        public string StationName { get; set; }
        public string Location { get; set; }
        
        public StationConfig()
        {
            // Default values
            StationName = "Parking Station";
            Location = "Main Entrance";
        }
    }
}