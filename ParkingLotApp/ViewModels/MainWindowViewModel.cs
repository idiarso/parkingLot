using System;
using System.Windows.Input;
using ReactiveUI;
using ParkingLotApp.Data;
using ParkingLotApp.Services;
using ParkingLotApp.Services.Interfaces;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace ParkingLotApp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;
        private readonly ParkingDbContext _dbContext;
        private readonly IParkingService _parkingService;
        private readonly IUserService _userService;
        private readonly ISettingsService _settingsService;
        private readonly IReportService _reportService;
        private readonly DashboardService _dashboardService;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private string _statusMessage = "";

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public bool IsLoginViewModel => CurrentViewModel is LoginViewModel;

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowVehicleEntryCommand { get; }
        public ICommand ShowVehicleExitCommand { get; }
        public ICommand ShowReportsCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowUserManagementCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CheckDatabaseCommand { get; }

        public MainWindowViewModel(
            ParkingDbContext dbContext,
            IParkingService parkingService,
            IUserService userService,
            ISettingsService settingsService,
            IReportService reportService,
            DashboardService dashboardService,
            ILogger logger,
            IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _parkingService = parkingService;
            _userService = userService;
            _settingsService = settingsService;
            _reportService = reportService;
            _dashboardService = dashboardService;
            _logger = logger;
            _serviceProvider = serviceProvider;

            // Initialize with login view instead of dashboard
            try 
            {
                Console.WriteLine("[Debug] Initializing login view on startup");
                _currentViewModel = new LoginViewModel(this, dbContext, logger, userService, settingsService);
                Console.WriteLine("[Debug] Login view initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Failed to initialize login view: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                }
                _logger.LogErrorAsync("Error initializing login view", ex).Wait();
                
                // Provide a failsafe message
                _statusMessage = "Error initializing login screen. Check logs for details.";
            }

            // Set up navigation commands
            ShowDashboardCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Dashboard").Wait();
                try
                {
                    Console.WriteLine("[Debug] Creating new DashboardViewModel");
                    StatusMessage = "Loading dashboard...";
                    
                    // Verify database connection first
                    if (!_dashboardService.CheckDatabaseConnectionAsync().Result)
                    {
                        StatusMessage = "Database connection error. Cannot navigate to Dashboard.";
                        Console.WriteLine("[Error] Database connection check failed before navigating to Dashboard");
                        return;
                    }
                    
                    CurrentViewModel = new DashboardViewModel(_parkingService, _settingsService, _dashboardService, _logger, _serviceProvider);
                    StatusMessage = "";
                    _logger.LogInfoAsync("Successfully navigated to Dashboard").Wait();
                    Console.WriteLine("[Debug] Navigation to Dashboard completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] Dashboard navigation error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    _logger.LogErrorAsync("Error navigating to Dashboard", ex).Wait();
                }
            });

            ShowVehicleEntryCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Vehicle Entry").Wait();
                try
                {
                    StatusMessage = "Loading vehicle entry...";
                    CurrentViewModel = new VehicleEntryViewModel(_parkingService, this, _logger);
                    StatusMessage = "";
                    _logger.LogInfoAsync("Successfully navigated to Vehicle Entry").Wait();
                    Console.WriteLine("[Debug] Navigation to Vehicle Entry completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] Vehicle Entry navigation error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    _logger.LogErrorAsync("Error navigating to Vehicle Entry", ex).Wait();
                }
            });

            ShowVehicleExitCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Vehicle Exit").Wait();
                try
                {
                    StatusMessage = "Loading vehicle exit...";
                    CurrentViewModel = new VehicleExitViewModel(_parkingService, this, _logger);
                    StatusMessage = "";
                    _logger.LogInfoAsync("Successfully navigated to Vehicle Exit").Wait();
                    Console.WriteLine("[Debug] Navigation to Vehicle Exit completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] Vehicle Exit navigation error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    _logger.LogErrorAsync("Error navigating to Vehicle Exit", ex).Wait();
                }
            });

            ShowReportsCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Reports").Wait();
                try
                {
                    StatusMessage = "Loading reports...";
                    CurrentViewModel = new ReportsViewModel(_reportService, _settingsService, _logger);
                    StatusMessage = "";
                    _logger.LogInfoAsync("Successfully navigated to Reports").Wait();
                    Console.WriteLine("[Debug] Navigation to Reports completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] Reports navigation error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    _logger.LogErrorAsync("Error navigating to Reports", ex).Wait();
                }
            });

            ShowSettingsCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Settings").Wait();
                try
                {
                    StatusMessage = "Loading settings...";
                    CurrentViewModel = new SettingsViewModel(_settingsService, _userService);
                    StatusMessage = "";
                    _logger.LogInfoAsync("Successfully navigated to Settings").Wait();
                    Console.WriteLine("[Debug] Navigation to Settings completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] Settings navigation error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    _logger.LogErrorAsync("Error navigating to Settings", ex).Wait();
                }
            });

            ShowUserManagementCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to User Management").Wait();
                try
                {
                    StatusMessage = "Loading user management...";
                    CurrentViewModel = new UserManagementViewModel(_userService, this);
                    StatusMessage = "";
                    _logger.LogInfoAsync("Successfully navigated to User Management").Wait();
                    Console.WriteLine("[Debug] Navigation to User Management completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] User Management navigation error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    _logger.LogErrorAsync("Error navigating to User Management", ex).Wait();
                }
            });

            LogoutCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    StatusMessage = "Logging out...";
                    await _logger.LogInfoAsync("User logging out");
                    await _userService.LogoutAsync();
                    
                    // Buat instance LoginViewModel baru
                    var loginViewModel = new LoginViewModel(this, _dbContext, _logger, _userService, _settingsService);
                    
                    // Navigasi ke login screen
                    CurrentViewModel = loginViewModel;
                    StatusMessage = "";
                    await _logger.LogInfoAsync("Successfully navigated to Login screen");
                    Console.WriteLine("[Debug] Navigation to Login screen completed successfully");
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    Console.WriteLine($"[Error] Logout error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    await _logger.LogErrorAsync("Error during logout", ex);
                }
            });

            CheckDatabaseCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await _logger.LogInfoAsync("Checking database connection");
                try
                {
                    StatusMessage = "Checking database connection...";
                    
                    // Gunakan CancellationTokenSource untuk menerapkan timeout
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)); // 10 detik timeout
                    
                    // Buat task untuk pengecekan database
                    var connectionTask = _dashboardService.CheckDatabaseConnectionAsync();
                    
                    // Tunggu task selesai atau timeout
                    var completedTask = await Task.WhenAny(connectionTask, Task.Delay(10000, cts.Token));
                    
                    if (completedTask == connectionTask)
                    {
                        // Task selesai dalam batas waktu
                        var connectionResult = await connectionTask;
                        
                        if (!connectionResult)
                        {
                            StatusMessage = "Database connection error. Check logs for details.";
                            Console.WriteLine("[Error] Database connection check failed");
                        }
                        else
                        {
                            StatusMessage = "Database connection is healthy. All tables accessible.";
                            Console.WriteLine("[Debug] Database connection check succeeded");
                            
                            // Hanya tampilkan pesan sukses untuk beberapa detik
                            await Task.Delay(3000);
                            StatusMessage = "";
                        }
                    }
                    else
                    {
                        // Timeout
                        cts.Cancel(); // Cancel the pending task
                        StatusMessage = "Database connection check timed out after 10 seconds.";
                        Console.WriteLine("[Error] Database connection check timed out");
                        await _logger.LogErrorAsync("Database connection check timed out");
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error checking database: {ex.Message}";
                    Console.WriteLine($"[Error] Database connection check error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                    }
                    await _logger.LogErrorAsync("Error checking database connection", ex);
                }
            });
        }
    }
}
