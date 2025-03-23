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
using Avalonia.Media;
using Microsoft.VisualBasic;

namespace ParkingLotApp.ViewModels
{
    public class DashboardViewModel : ViewModelBase, IDisposable
    {
        private readonly IParkingService _parkingService;
        private readonly ISettingsService _settingsService;
        private readonly DashboardService _dashboardService;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly DispatcherTimer _refreshTimer;
        private readonly DispatcherTimer _statusClearTimer;
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
        private int _totalSpots;
        private int _occupiedSpots;
        private int _availableSpots;
        private decimal _todayRevenue;
        private decimal _weekRevenue;
        private decimal _monthRevenue;
        private bool _isLoading;
        private int _refreshErrorCount = 0;
        private bool _isInitialLoad = true;
        private Dictionary<string, int> _vehicleDistribution = new Dictionary<string, int>();

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

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public int TotalSpots
        {
            get => _totalSpots;
            private set => this.RaiseAndSetIfChanged(ref _totalSpots, value);
        }

        public int OccupiedSpots
        {
            get => _occupiedSpots;
            private set => this.RaiseAndSetIfChanged(ref _occupiedSpots, value);
        }

        public int AvailableSpots
        {
            get => _availableSpots;
            private set => this.RaiseAndSetIfChanged(ref _availableSpots, value);
        }

        public decimal TodayRevenue
        {
            get => _todayRevenue;
            private set => this.RaiseAndSetIfChanged(ref _todayRevenue, value);
        }

        public decimal WeekRevenue
        {
            get => _weekRevenue;
            private set => this.RaiseAndSetIfChanged(ref _weekRevenue, value);
        }

        public decimal MonthRevenue
        {
            get => _monthRevenue;
            private set => this.RaiseAndSetIfChanged(ref _monthRevenue, value);
        }

        public Dictionary<string, int> VehicleDistribution
        {
            get => _vehicleDistribution;
            private set => this.RaiseAndSetIfChanged(ref _vehicleDistribution, value);
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
            _serviceProvider = serviceProvider;
            
            // Inisialisasi nilai dasar untuk property
            _currentTime = DateTime.Now.ToString("HH:mm:ss");
            _dbStatus = "Unknown";
            _statusMessage = "Initializing...";
            
            // Buat LogViewer dengan serviceProvider
            _logViewer = new LogViewerViewModel(serviceProvider, logger);

            // Inisialisasi vehicle distribution dengan dictionary kosong
            _vehicleDistribution = new Dictionary<string, int>
            {
                { "Car", 0 },
                { "Motorcycle", 0 },
                { "Truck", 0 },
                { "Bus", 0 }
            };

            // Timer untuk pembaruan waktu (setiap detik)
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += (s, e) => CurrentTime = DateTime.Now.ToString("HH:mm:ss");
            _clockTimer.Start();

            // Timer untuk auto-refresh data (setiap 10 detik)
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(10)
            };
            _refreshTimer.Tick += async (s, e) => 
            {
                try
                {
                    Console.WriteLine($"[Debug] Auto-refresh timer triggered at {DateTime.Now:HH:mm:ss}");
                    await RefreshDataAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Exception in refresh timer: {ex.Message}");
                    await _logger.LogErrorAsync("Exception in dashboard refresh timer", ex);
                }
            };
            _refreshTimer.Start();

            // Timer untuk menghapus pesan status setelah beberapa detik
            _statusClearTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _statusClearTimer.Tick += (sender, e) =>
            {
                StatusMessage = string.Empty;
                _statusClearTimer.Stop();
            };

            // Command untuk refresh manual
            RefreshCommand = ReactiveCommand.CreateFromTask(async () => 
            {
                Console.WriteLine($"[Debug] Manual refresh triggered at {DateTime.Now:HH:mm:ss}");
                await RefreshDataAsync();
            });

            // Inisialisasi animasi
            InitializeAnimations();

            // Inisialisasi data
            Task.Run(async () =>
            {
                try
                {
                    await _logger.LogInfoAsync("Dashboard initialized");
                    Console.WriteLine("[Debug] Dashboard initialized, performing initial data load");
                    await InitializeAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error] Error during dashboard initialization: {ex.Message}");
                    await _logger.LogErrorAsync("Error initializing dashboard", ex);
                }
            });
        }

        private async Task InitializeAsync()
        {
            try
            {
                await _logger.LogInfoAsync("Dashboard initialized");
                Console.WriteLine("[Debug] Dashboard initialized, performing initial data load");
                
                // Memastikan database sudah siap (jika belum, akan mengembalikan default values)
                var dbConnected = await _dashboardService.CheckDatabaseConnectionAsync();
                IsConnected = dbConnected;
                DbStatus = dbConnected ? "Connected" : "Disconnected";
                
                if (!dbConnected)
                {
                    Dispatcher.UIThread.Post(() => 
                    {
                        StatusMessage = "Database connection error. Data may not be accurate.";
                    });
                    Console.WriteLine("[Debug] Database connection check failed during initialization");
                }
                else
                {
                    Console.WriteLine("[Debug] Database connection successful");
                }
                
                await RefreshDataAsync();
                _isInitialLoad = false;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error initializing dashboard", ex);
                Console.WriteLine($"[Error] Dashboard initialization error: {ex.Message}");
                Dispatcher.UIThread.Post(() => 
                {
                    StatusMessage = "Error loading dashboard data. Check logs for details.";
                });
            }
        }

        private async Task RefreshDataAsync()
        {
            if (_isRefreshing)
            {
                return; // Hindari refresh bersamaan
            }

            _isRefreshing = true;
            
            try
            {
                // Ubah IsLoading hanya jika ini adalah load pertama
                if (_isInitialLoad)
                {
                    Dispatcher.UIThread.Post(() => IsLoading = true);
                }

                // Menyembunyikan pesan status jika ada
                if (!string.IsNullOrEmpty(StatusMessage))
                {
                    Dispatcher.UIThread.Post(() => StatusMessage = string.Empty);
                }

                // Memuat data dari service
                bool hasError = false;
                try
                {
                    var totalSpots = await _dashboardService.GetTotalSpotsAsync();
                    var occupiedSpots = await _dashboardService.GetOccupiedSpotsAsync();
                    var availableSpots = await _dashboardService.GetAvailableSpotsAsync();

                    // Set values pada UI thread
                    Dispatcher.UIThread.Post(() =>
                    {
                        TotalSpots = totalSpots;
                        OccupiedSpots = occupiedSpots;
                        AvailableSpots = availableSpots;
                    });
                }
                catch (Exception ex)
                {
                    hasError = true;
                    Console.WriteLine($"[Error] Error refreshing spots data: {ex.Message}");
                    await _logger.LogErrorAsync("Error refreshing spots data", ex);
                }

                // Memuat data revenue
                try
                {
                    var todayRevenue = await _dashboardService.GetTodayRevenueAsync();
                    var weekRevenue = await _dashboardService.GetWeekRevenueAsync();
                    var monthRevenue = await _dashboardService.GetMonthRevenueAsync();

                    // Set values pada UI thread
                    Dispatcher.UIThread.Post(() =>
                    {
                        TodayRevenue = todayRevenue;
                        WeekRevenue = weekRevenue;
                        MonthRevenue = monthRevenue;
                    });

                    // Reset error counter jika berhasil
                    _refreshErrorCount = 0;
                }
                catch (Exception ex)
                {
                    hasError = true;
                    _refreshErrorCount++;
                    
                    await _logger.LogErrorAsync("Error getting revenue data", ex);
                    Console.WriteLine($"[Error] Error getting revenue: {ex.Message}");
                    
                    if (_refreshErrorCount >= 3)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            StatusMessage = "Error refreshing revenue data. Will try again later.";
                        });
                    }
                }

                // Memuat aktivitas terbaru
                try
                {
                    var activities = await _dashboardService.GetRecentActivitiesAsync();
                    Dispatcher.UIThread.Post(() =>
                    {
                        RecentActivities.Clear();
                        foreach (var activity in activities)
                        {
                            RecentActivities.Add(activity);
                        }
                    });
                }
                catch (Exception ex)
                {
                    hasError = true;
                    await _logger.LogErrorAsync("Error getting recent activities", ex);
                    Console.WriteLine($"[Error] Error getting recent activities: {ex.Message}");
                }

                // Memuat log terbaru
                try
                {
                    var logs = await _dashboardService.GetRecentLogsAsync();
                    Dispatcher.UIThread.Post(() =>
                    {
                        SystemLogs.Clear();
                        foreach (var log in logs)
                        {
                            SystemLogs.Add(log);
                        }
                    });
                }
                catch (Exception ex)
                {
                    hasError = true;
                    await _logger.LogErrorAsync("Error getting recent logs", ex);
                    Console.WriteLine($"[Error] Error getting recent logs: {ex.Message}");
                }

                // Memuat distribusi kendaraan
                try
                {
                    var distribution = await _dashboardService.GetVehicleDistributionAsync();
                    Dispatcher.UIThread.Post(() =>
                    {
                        VehicleDistribution = distribution;
                        this.RaisePropertyChanged(nameof(VehicleDistribution));
                    });
                }
                catch (Exception ex)
                {
                    hasError = true;
                    await _logger.LogErrorAsync("Error getting vehicle distribution", ex);
                    Console.WriteLine($"[Error] Error getting vehicle distribution: {ex.Message}");
                }

                if (hasError)
                {
                    if (_refreshErrorCount >= 3)
                    {
                        Dispatcher.UIThread.Post(() =>
                        {
                            StatusMessage = "Some data could not be loaded. Check connection.";
                            _statusClearTimer.Start(); // Otomatis clear setelah beberapa detik
                        });
                    }
                }
                else
                {
                    // Reset error counter jika berhasil
                    _refreshErrorCount = 0;
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error refreshing dashboard data", ex);
                Console.WriteLine($"[Error] General error refreshing dashboard: {ex.Message}");
                
                Dispatcher.UIThread.Post(() =>
                {
                    StatusMessage = "Error refreshing data. Check logs for details.";
                    _statusClearTimer.Start();
                });
            }
            finally
            {
                // Reset loading status pada UI thread
                if (_isInitialLoad)
                {
                    Dispatcher.UIThread.Post(() => IsLoading = false);
                }
                
                _isRefreshing = false;
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
            _statusClearTimer?.Stop();
            
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