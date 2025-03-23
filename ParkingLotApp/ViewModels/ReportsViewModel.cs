using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using ReactiveUI;
using ParkingLotApp.Models;
using ParkingLotApp.Services;
using Avalonia.Threading;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly IReportService _reportService;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private DateTime _startDate;
        private DateTime _endDate;
        private string _selectedReportType;
        private bool _isGenerating;
        private string _statusMessage = string.Empty;
        private ObservableCollection<ParkingActivity> _activityData;
        private Dictionary<string, decimal> _revenueData;
        private Dictionary<string, int> _occupancyData;
        private Dictionary<string, object> _summaryData;
        private Dictionary<string, string> _reportSettings;

        public DateTime StartDate
        {
            get => _startDate;
            set => this.RaiseAndSetIfChanged(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => this.RaiseAndSetIfChanged(ref _endDate, value);
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set => this.RaiseAndSetIfChanged(ref _selectedReportType, value);
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set => this.RaiseAndSetIfChanged(ref _isGenerating, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ObservableCollection<ParkingActivity> ActivityData
        {
            get => _activityData;
            set => this.RaiseAndSetIfChanged(ref _activityData, value);
        }

        public Dictionary<string, decimal> RevenueData
        {
            get => _revenueData;
            set => this.RaiseAndSetIfChanged(ref _revenueData, value);
        }

        public Dictionary<string, int> OccupancyData
        {
            get => _occupancyData;
            set => this.RaiseAndSetIfChanged(ref _occupancyData, value);
        }

        public Dictionary<string, object> SummaryData
        {
            get => _summaryData;
            set => this.RaiseAndSetIfChanged(ref _summaryData, value);
        }

        public Dictionary<string, string> ReportSettings
        {
            get => _reportSettings;
            set => this.RaiseAndSetIfChanged(ref _reportSettings, value);
        }

        public ObservableCollection<string> ReportTypes { get; }

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportReportCommand { get; }

        public ReportsViewModel(IReportService reportService, ISettingsService settingsService, ILogger logger)
        {
            _reportService = reportService;
            _settingsService = settingsService;
            _logger = logger;
            _startDate = DateTime.Today.AddDays(-7);
            _endDate = DateTime.Today;
            _selectedReportType = "Activity";
            _activityData = new ObservableCollection<ParkingActivity>();
            _revenueData = new Dictionary<string, decimal>();
            _occupancyData = new Dictionary<string, int>();
            _summaryData = new Dictionary<string, object>();
            _reportSettings = new Dictionary<string, string>();

            ReportTypes = new ObservableCollection<string>
            {
                "Activity Report",
                "Revenue Report",
                "Occupancy Report",
                "Summary Report"
            };

            GenerateReportCommand = ReactiveCommand.CreateFromTask(GenerateReportAsync);
            ExportReportCommand = ReactiveCommand.CreateFromTask(ExportReportAsync);

            // Load report settings
            _ = LoadReportSettingsAsync();
        }

        private async Task LoadReportSettingsAsync()
        {
            try
            {
                ReportSettings = await _settingsService.GetReportSettingsAsync() ?? new Dictionary<string, string>();
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = $"Error loading report settings: {ex.Message}";
                });
            }
        }

        private async Task GenerateReportAsync()
        {
            if (EndDate < StartDate)
            {
                StatusMessage = "Error: End date cannot be before start date";
                return;
            }

            IsGenerating = true;
            StatusMessage = "Generating report...";

            try
            {
                switch (SelectedReportType)
                {
                    case "Activity Report":
                        var activities = await _reportService.GetActivityReportAsync(StartDate, EndDate);
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ActivityData.Clear();
                            foreach (var activity in activities)
                            {
                                ActivityData.Add(activity);
                            }
                        });
                        break;

                    case "Revenue Report":
                        RevenueData = await _reportService.GetRevenueReportAsync(StartDate, EndDate);
                        break;

                    case "Occupancy Report":
                        OccupancyData = await _reportService.GetOccupancyReportAsync(StartDate, EndDate);
                        break;

                    case "Summary Report":
                        SummaryData = await _reportService.GetSummaryReportAsync(StartDate, EndDate);
                        break;
                }

                StatusMessage = "Report generated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error generating report: {ex.Message}";
                await _logger.LogErrorAsync(ex.Message);
            }
            finally
            {
                IsGenerating = false;
            }
        }

        private async Task ExportReportAsync()
        {
            try
            {
                StatusMessage = "Exporting report...";
                var settings = await _settingsService.GetReportSettingsAsync();
                var exportPath = settings.GetValueOrDefault("report_export_path", "reports");
                var fileName = $"parking_report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var fullPath = System.IO.Path.Combine(exportPath, fileName);

                // Ensure directory exists
                System.IO.Directory.CreateDirectory(exportPath);

                // Export data
                using (var writer = new System.IO.StreamWriter(fullPath))
                {
                    writer.WriteLine("Date,Vehicle Number,Vehicle Type,Duration,Fee");
                    foreach (var activity in ActivityData)
                    {
                        writer.WriteLine($"{activity.EntryTime:yyyy-MM-dd},{activity.VehicleNumber},{activity.VehicleType},{activity.Duration},{activity.Fee}");
                    }
                }

                StatusMessage = $"Report exported successfully to {fullPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting report: {ex.Message}";
                await _logger.LogErrorAsync(ex.Message);
            }
        }
    }
} 