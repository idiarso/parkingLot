using System;
using System.Windows;
using System.Windows.Controls;
using TestWpfApp.Pages;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Initialize by loading the login page
            MainFrame.Navigate(new LoginPage());
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user is logged in
            if (!UserSession.IsUserLoggedIn)
            {
                MessageBox.Show("You must login first.", "Authentication Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                MainFrame.Navigate(new LoginPage());
                return;
            }

            // Clear back stack to prevent navigation issues
            MainFrame.NavigationService.RemoveBackEntry();

            if (sender == btnDashboard)
            {
                MainFrame.Navigate(new DashboardPage());
            }
            else if (sender == btnVehicleEntry)
            {
                MainFrame.Navigate(new VehicleEntryPage());
            }
            else if (sender == btnVehicleExit)
            {
                MainFrame.Navigate(new VehicleExitPage());
            }
            else if (sender == btnVehicleMonitoring)
            {
                MainFrame.Navigate(new VehicleMonitoringPage());
            }
            else if (sender == btnReports)
            {
                MainFrame.Navigate(new ReportsPage());
            }
            else if (sender == btnUserManagement)
            {
                MainFrame.Navigate(new UserManagementPage());
            }
            else if (sender == btnShifts)
            {
                MainFrame.Navigate(new ShiftsPage());
            }
            else if (sender == btnSettings)
            {
                MainFrame.Navigate(new SettingsPage());
            }
        }
    }
}
