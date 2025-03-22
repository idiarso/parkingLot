using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public ObservableCollection<ParkingActivity> ActivityList { get; set; }

        public DashboardPage()
        {
            InitializeComponent();
            
            // Initialize activity list with sample data
            ActivityList = new ObservableCollection<ParkingActivity>
            {
                new ParkingActivity { Time = "08:15", VehicleNumber = "B 1234 KLM", VehicleType = "Car", Action = "Entry", Duration = "-", Fee = "-" },
                new ParkingActivity { Time = "08:45", VehicleNumber = "B 7890 XYZ", VehicleType = "Motorcycle", Action = "Entry", Duration = "-", Fee = "-" },
                new ParkingActivity { Time = "09:30", VehicleNumber = "B 5678 ABC", VehicleType = "Car", Action = "Entry", Duration = "-", Fee = "-" },
                new ParkingActivity { Time = "10:15", VehicleNumber = "B 1234 DEF", VehicleType = "Motorcycle", Action = "Exit", Duration = "1.5 hours", Fee = "Rp 5.000" },
                new ParkingActivity { Time = "10:30", VehicleNumber = "B 1234 KLM", VehicleType = "Car", Action = "Exit", Duration = "2.25 hours", Fee = "Rp 15.000" }
            };
            
            // Set DataContext
            this.DataContext = this;
        }

        private void btnQuickVehicleEntry_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Vehicle Entry page
            NavigationService.Navigate(new VehicleEntryPage());
        }

        private void btnQuickVehicleExit_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Vehicle Exit page
            NavigationService.Navigate(new VehicleExitPage());
        }

        private void btnQuickVehicleMonitoring_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Vehicle Monitoring page
            NavigationService.Navigate(new VehicleMonitoringPage());
        }
    }

    public class ParkingActivity
    {
        public string Time { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string Action { get; set; }
        public string Duration { get; set; }
        public string Fee { get; set; }
    }
} 