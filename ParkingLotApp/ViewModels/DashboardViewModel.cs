using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Timers;
using ReactiveUI;
using ParkingLotApp.Models;
using ParkingLotApp.Services;
using ParkingLotApp.Services.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using Avalonia.Threading;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;

namespace ParkingLotApp.ViewModels
{
    public class DashboardViewModel : ViewModelBase, IDisposable
    {
        private readonly IParkingService _parkingService;
        private readonly ISettingsService _settingsService;
        private readonly DashboardService _dashboardService;
        private readonly ILogger _logger;
        private readonly DispatcherTimer _refreshTimer;
        private readonly DispatcherTimer _clockTimer;
        
        private string _currentTime;
        private string _dbStatus;
        private bool _isBusy;
        private DashboardStatistics _statistics = new();
        private ObservableCollection<ParkingActivity> _recentActivities = new();
        private ObservableCollection<Log> _systemLogs = new();
        private bool _isConnected;
        private string _statusMessage;
        private LogViewerViewModel _logViewer;
        private bool _isRefreshing;

        public LogViewerViewModel LogViewer
        {
            get => _logViewer;
            set => this.RaiseAndSetIfChanged(ref _logViewer, value);
        }

        public string CurrentTime
        {
            get => _currentTime;
            set => this.RaiseAndSetIfChanged(ref _currentTime, value);
        }

        public string DbStatus
        {
            get => _dbStatus;
            set => this.RaiseAndSetIfChanged(ref _dbStatus, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => this.RaiseAndSetIfChanged(ref _isConnected, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public DashboardStatistics Statistics
        {
            get => _statistics;
            set => this.RaiseAndSetIfChanged(ref _statistics, value);
        }

        public ObservableCollection<ParkingActivity> RecentActivities
        {
            get => _recentActivities;
            set => this.RaiseAndSetIfChanged(ref _recentActivities, value);
        }

        public ObservableCollection<Log> SystemLogs
        {
            get => _systemLogs;
            set => this.RaiseAndSetIfChanged(ref _systemLogs, value);
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => this.RaiseAndSetIfChanged(ref _isRefreshing, value);
        }

        public ICommand RefreshCommand { get; }

        public DashboardViewModel(
            IParkingService parkingService,
            ISettingsService settingsService,
            DashboardService dashboardService,
            ILogger logger,
            IServiceProvider serviceProvider)
        {
            _parkingService = parkingService;
            _settingsService = settingsService;
            _dashboardService = dashboardService;
            _logger = logger;
            
            // Inisialisasi nilai dasar untuk property
            _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _dbStatus = "Checking...";
            _statusMessage = "Initializing...";
            
            // Buat LogViewer dengan serviceProvider
            _logViewer = new LogViewerViewModel(serviceProvider, logger);

            // Timer untuk pembaruan waktu (setiap detik)
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            _clockTimer.Start();

            // Timer untuk auto-refresh data (setiap 5 detik)
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _refreshTimer.Tick += async (s, e) => await RefreshDashboardDataAsync();
            _refreshTimer.Start();

            // Command untuk refresh manual
            RefreshCommand = ReactiveCommand.CreateFromTask(RefreshDashboardDataAsync);

            // Inisialisasi animasi
            InitializeAnimations();

            // Inisialisasi data
            Task.Run(async () =>
            {
                await _logger.LogInfoAsync("Dashboard initialized");
                await RefreshDashboardDataAsync();
            });
        }

        private async Task RefreshDashboardDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                IsRefreshing = true;
                StatusMessage = "Refreshing dashboard data...";

                // Cek koneksi database
                try
                {
                    IsConnected = await _dashboardService.CheckDatabaseConnectionAsync();
                    DbStatus = IsConnected ? "Connected" : "Disconnected";
                }
                catch (Exception ex)
                {
                    IsConnected = false;
                    DbStatus = "Error";
                    StatusMessage = $"Database error: {ex.Message}";
                    await _logger.LogErrorAsync("Database connection error in dashboard", ex);
                }

                if (IsConnected)
                {
                    try
                    {
                        // Buat semua task untuk dijalankan secara paralel tapi tidak saling menunggu
                        var totalSpotsTask = _dashboardService.GetTotalSpotsAsync();
                        var occupiedSpotsTask = _dashboardService.GetOccupiedSpotsAsync();
                        var availableSpotsTask = _dashboardService.GetAvailableSpotsAsync();
                        var todayRevenueTask = _dashboardService.GetTodayRevenueAsync();
                        var weekRevenueTask = _dashboardService.GetWeekRevenueAsync();
                        var monthRevenueTask = _dashboardService.GetMonthRevenueAsync();
                        var vehicleDistributionTask = _dashboardService.GetVehicleDistributionAsync();
                        var recentActivitiesTask = _dashboardService.GetRecentActivitiesAsync(15);
                        var systemLogsTask = _dashboardService.GetRecentLogsAsync(15);

                        // Tunggu semua task selesai
                        await Task.WhenAll(
                            totalSpotsTask, 
                            occupiedSpotsTask,
                            availableSpotsTask,
                            todayRevenueTask,
                            weekRevenueTask,
                            monthRevenueTask,
                            vehicleDistributionTask,
                            recentActivitiesTask,
                            systemLogsTask
                        );

                        // Ambil hasil dari task
                        int totalSpots = await totalSpotsTask;
                        int occupiedSpots = await occupiedSpotsTask;
                        int availableSpots = await availableSpotsTask;
                        decimal todayRevenue = await todayRevenueTask;
                        decimal weekRevenue = await weekRevenueTask;
                        decimal monthRevenue = await monthRevenueTask;
                        var vehicleDistribution = await vehicleDistributionTask;
                        var recentActivities = await recentActivitiesTask;
                        var systemLogs = await systemLogsTask;

                        // Update statistics model
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Statistics.TotalSpots = totalSpots;
                            Statistics.OccupiedSpots = occupiedSpots;
                            Statistics.AvailableSpots = availableSpots;
                            Statistics.TodayRevenue = todayRevenue;
                            Statistics.WeekRevenue = weekRevenue;
                            Statistics.MonthRevenue = monthRevenue;
                            Statistics.LastUpdated = DateTime.Now;

                            // Update vehicle distribution visualization
                            Statistics.VehicleDistribution.Clear();
                            Statistics.VehicleTypes.Clear();
                            string[] colors = { "#3498db", "#2ecc71", "#e74c3c", "#f39c12", "#9b59b6" };
                            int colorIndex = 0;
                            
                            foreach (var item in vehicleDistribution)
                            {
                                Statistics.VehicleDistribution.Add(new VehicleDistributionItem(
                                    item.Key,
                                    item.Value,
                                    colors[colorIndex % colors.Length]
                                ));
                                
                                Statistics.VehicleTypes.Add(new VehicleTypeCount(
                                    item.Key,
                                    item.Value
                                ));
                                
                                colorIndex++;
                            }

                            // Update recent activities
                            RecentActivities.Clear();
                            foreach (var activity in recentActivities)
                            {
                                // Tambahkan informasi waktu terformat
                                if (activity.Action == "Entry")
                                {
                                    activity.FormattedTime = activity.EntryTime.ToString("HH:mm:ss");
                                }
                                else if (activity.Action == "Exit" && activity.ExitTime.HasValue)
                                {
                                    activity.FormattedTime = activity.ExitTime.Value.ToString("HH:mm:ss");
                                }
                                RecentActivities.Add(activity);
                            }

                            // Update system logs
                            SystemLogs.Clear();
                            foreach (var log in systemLogs)
                            {
                                SystemLogs.Add(log);
                            }

                            StatusMessage = $"Data refreshed at {DateTime.Now:HH:mm:ss}";
                        });
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync("Error fetching dashboard data", ex);
                        StatusMessage = $"Error fetching data: {ex.Message}";
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error refreshing dashboard data", ex);
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
                
                // Set IsRefreshing false setelah delay singkat untuk animasi
                await Task.Delay(500);
                IsRefreshing = false;
            }
        }

        private async Task AnimateRefreshIndicatorsAsync()
        {
            // Buat animasi refresh hanya sekali
            // Animasi akan dikontrol dengan IsRefreshing property yang terikat ke View

            // Untuk menghindari memory leak, tambahkan cancellation
            var cancellationTokenSource = new CancellationTokenSource();
            try
            {
                // Tunggu sampai view model dibuang
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                // Normal ketika task dibatalkan
            }
            finally
            {
                cancellationTokenSource.Dispose();
            }
        }
        
        // Inisialisasi animasi
        private void InitializeAnimations()
        {
            // Mulai animasi refresh indicators secara latar belakang
            Task.Run(AnimateRefreshIndicatorsAsync);
        }

        public void Dispose()
        {
            _clockTimer?.Stop();
            _refreshTimer?.Stop();
            
            // Tambahkan logic untuk membatalkan task animasi jika perlu
            
            GC.SuppressFinalize(this);
        }
    }

    public class DashboardStatistics : ReactiveObject
    {
        private int _totalSpots;
        private int _occupiedSpots;
        private int _availableSpots;
        private decimal _todayRevenue;
        private decimal _weekRevenue;
        private decimal _monthRevenue;
        private DateTime _lastUpdated;
        private ObservableCollection<VehicleTypeCount> _vehicleTypes = new();
        private ObservableCollection<VehicleDistributionItem> _vehicleDistribution = new();
        
        public int TotalSpots
        {
            get => _totalSpots;
            set => this.RaiseAndSetIfChanged(ref _totalSpots, value);
        }
        
        public int OccupiedSpots
        {
            get => _occupiedSpots;
            set => this.RaiseAndSetIfChanged(ref _occupiedSpots, value);
        }
        
        public int AvailableSpots
        {
            get => _availableSpots;
            set => this.RaiseAndSetIfChanged(ref _availableSpots, value);
        }
        
        public decimal TodayRevenue
        {
            get => _todayRevenue;
            set => this.RaiseAndSetIfChanged(ref _todayRevenue, value);
        }
        
        public decimal WeekRevenue
        {
            get => _weekRevenue;
            set => this.RaiseAndSetIfChanged(ref _weekRevenue, value);
        }
        
        public decimal MonthRevenue
        {
            get => _monthRevenue;
            set => this.RaiseAndSetIfChanged(ref _monthRevenue, value);
        }
        
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => this.RaiseAndSetIfChanged(ref _lastUpdated, value);
        }
        
        public string FormattedRevenue => $"Rp {_todayRevenue:N0}";
        
        public decimal OccupancyPercentage => _totalSpots > 0 ? (_occupiedSpots * 100.0m) / _totalSpots : 0;
        
        public decimal AvailabilityPercentage => _totalSpots > 0 ? (_availableSpots * 100.0m) / _totalSpots : 0;
        
        public ObservableCollection<VehicleTypeCount> VehicleTypes
        {
            get => _vehicleTypes;
            set => this.RaiseAndSetIfChanged(ref _vehicleTypes, value);
        }
        
        public ObservableCollection<VehicleDistributionItem> VehicleDistribution
        {
            get => _vehicleDistribution;
            set => this.RaiseAndSetIfChanged(ref _vehicleDistribution, value);
        }
    }
    
    public class VehicleTypeCount
    {
        public string VehicleType { get; }
        public int Count { get; }
        
        public VehicleTypeCount(string vehicleType, int count)
        {
            VehicleType = vehicleType;
            Count = count;
        }
    }
    
    public class VehicleDistributionItem
    {
        public string Label { get; }
        public int Value { get; }
        public string Color { get; }
        
        public VehicleDistributionItem(string label, int value, string color)
        {
            Label = label;
            Value = value;
            Color = color;
        }
    }
} 