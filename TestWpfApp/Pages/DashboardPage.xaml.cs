using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TestWpfApp.Models;

namespace TestWpfApp.Pages
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        private ObservableCollection<ParkingActivity> _recentActivities;
        private int _currentPage = 1;
        private const int _pageSize = 5;

        public DashboardPage()
        {
            InitializeComponent();
            
            // Check if user is logged in, if not redirect to login page
            if (!UserSession.IsUserLoggedIn)
            {
                NavigationService?.Navigate(new LoginPage());
                return;
            }
            
            // Set welcome message with user's display name
            txtWelcome.Text = $"Welcome, {UserSession.CurrentUser.DisplayName} | Role: {UserSession.CurrentUser.Role} | Last Login: {UserSession.CurrentUser.LastLogin:g}";
            
            _recentActivities = new ObservableCollection<ParkingActivity>();
            LoadSampleData();
        }

        private void LoadSampleData()
        {
            // Sample data for recent activities
            _recentActivities.Add(new ParkingActivity
            {
                Time = DateTime.Now.AddMinutes(-5),
                VehicleNumber = "B 1234 CD",
                VehicleType = "Car",
                Action = "Entry",
                Duration = "0:05",
                Fee = 2000
            });

            _recentActivities.Add(new ParkingActivity
            {
                Time = DateTime.Now.AddMinutes(-15),
                VehicleNumber = "B 5678 EF",
                VehicleType = "Motorcycle",
                Action = "Exit",
                Duration = "0:30",
                Fee = 3000
            });

            _recentActivities.Add(new ParkingActivity
            {
                Time = DateTime.Now.AddMinutes(-30),
                VehicleNumber = "B 9012 GH",
                VehicleType = "Car",
                Action = "Entry",
                Duration = "0:30",
                Fee = 2000
            });

            dgRecentActivities.ItemsSource = _recentActivities;
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
        
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Confirm logout
            var result = MessageBox.Show("Are you sure you want to log out?", "Logout Confirmation", 
                MessageBoxButton.YesNo, MessageBoxImage.Question);
                
            if (result == MessageBoxResult.Yes)
            {
                // Clear user session
                UserSession.Logout();
                
                // Navigate to login page
                NavigationService.Navigate(new LoginPage());
            }
        }
    }

    public class ParkingActivity
    {
        public DateTime Time { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string Action { get; set; }
        public string Duration { get; set; }
        public decimal Fee { get; set; }
    }
}