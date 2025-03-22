using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace TestWpfApp
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public ObservableCollection<DailySummary> ReportData { get; set; }

        public ReportsPage()
        {
            InitializeComponent();
            
            // Initialize date range (last 7 days)
            EndDate = DateTime.Now;
            StartDate = EndDate.AddDays(-7);
            
            // Initialize with sample data
            ReportData = GenerateSampleData();
            
            this.DataContext = this;
        }

        private void btnGenerateReport_Click(object sender, RoutedEventArgs e)
        {
            // Validate date range
            if (dpStartDate.SelectedDate == null || dpEndDate.SelectedDate == null)
            {
                MessageBox.Show("Please select a valid date range", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpStartDate.SelectedDate > dpEndDate.SelectedDate)
            {
                MessageBox.Show("Start date cannot be later than end date", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update report title based on selection
            string reportType = ((ComboBoxItem)cmbReportType.SelectedItem).Content.ToString();
            txtReportTitle.Text = $"{reportType} ({dpStartDate.SelectedDate:yyyy-MM-dd} to {dpEndDate.SelectedDate:yyyy-MM-dd})";
            
            // In a real app, this would query a database
            // For demo purposes, we'll just regenerate sample data
            ReportData = GenerateSampleData();
            dgReportData.ItemsSource = ReportData;
            
            MessageBox.Show("Report generated successfully", "Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnExportReport_Click(object sender, RoutedEventArgs e)
        {
            // In a real app, this would export to Excel
            // For demo purposes, we'll just show a success message
            MessageBox.Show("Report exported successfully to Excel", "Export Success", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private ObservableCollection<DailySummary> GenerateSampleData()
        {
            // Generate sample data for demo purposes
            var random = new Random();
            var data = new ObservableCollection<DailySummary>();
            
            DateTime currentDate = StartDate;
            while (currentDate <= EndDate)
            {
                int totalVehicles = random.Next(80, 200);
                int carCount = random.Next(40, 120);
                int motorcycleCount = totalVehicles - carCount;
                decimal revenue = carCount * 15000 + motorcycleCount * 5000;
                
                data.Add(new DailySummary
                {
                    Date = currentDate.ToString("yyyy-MM-dd"),
                    TotalVehicles = totalVehicles,
                    Cars = carCount,
                    Motorcycles = motorcycleCount,
                    Revenue = $"Rp {revenue:N0}",
                    AverageDuration = $"{random.Next(1, 5)}h {random.Next(1, 59)}m"
                });
                
                currentDate = currentDate.AddDays(1);
            }
            
            return data;
        }
    }

    public class DailySummary
    {
        public string Date { get; set; }
        public int TotalVehicles { get; set; }
        public int Cars { get; set; }
        public int Motorcycles { get; set; }
        public string Revenue { get; set; }
        public string AverageDuration { get; set; }
    }
} 