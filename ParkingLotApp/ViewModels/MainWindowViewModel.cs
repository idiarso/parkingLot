using System;
using System.Windows.Input;
using ReactiveUI;
using ParkingLotApp.Data;
using ParkingLotApp.Services;
using ParkingLotApp.Services.Interfaces;

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

        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set => this.RaiseAndSetIfChanged(ref _currentViewModel, value);
        }

        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowVehicleEntryCommand { get; }
        public ICommand ShowVehicleExitCommand { get; }
        public ICommand ShowReportsCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowUserManagementCommand { get; }
        public ICommand LogoutCommand { get; }

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

            // Initialize with the dashboard view
            _currentViewModel = new DashboardViewModel(parkingService, settingsService, dashboardService, logger, serviceProvider);

            // Set up navigation commands
            ShowDashboardCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Dashboard").Wait();
                try
                {
                    CurrentViewModel = new DashboardViewModel(_parkingService, _settingsService, _dashboardService, _logger, _serviceProvider);
                    _logger.LogInfoAsync("Successfully navigated to Dashboard").Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorAsync("Error navigating to Dashboard", ex).Wait();
                }
            });

            ShowVehicleEntryCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Vehicle Entry").Wait();
                try
                {
                    CurrentViewModel = new VehicleEntryViewModel(_parkingService, this, _logger);
                    _logger.LogInfoAsync("Successfully navigated to Vehicle Entry").Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorAsync("Error navigating to Vehicle Entry", ex).Wait();
                }
            });

            ShowVehicleExitCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Vehicle Exit").Wait();
                try
                {
                    CurrentViewModel = new VehicleExitViewModel(_parkingService, this, _logger);
                    _logger.LogInfoAsync("Successfully navigated to Vehicle Exit").Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorAsync("Error navigating to Vehicle Exit", ex).Wait();
                }
            });

            ShowReportsCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Reports").Wait();
                try
                {
                    CurrentViewModel = new ReportsViewModel(_reportService, _settingsService, _logger);
                    _logger.LogInfoAsync("Successfully navigated to Reports").Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorAsync("Error navigating to Reports", ex).Wait();
                }
            });

            ShowSettingsCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to Settings").Wait();
                try
                {
                    CurrentViewModel = new SettingsViewModel(_settingsService, _userService);
                    _logger.LogInfoAsync("Successfully navigated to Settings").Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorAsync("Error navigating to Settings", ex).Wait();
                }
            });

            ShowUserManagementCommand = ReactiveCommand.Create(() =>
            {
                _logger.LogInfoAsync("Navigating to User Management").Wait();
                try
                {
                    CurrentViewModel = new UserManagementViewModel(_userService, this);
                    _logger.LogInfoAsync("Successfully navigated to User Management").Wait();
                }
                catch (Exception ex)
                {
                    _logger.LogErrorAsync("Error navigating to User Management", ex).Wait();
                }
            });

            LogoutCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                try
                {
                    await _logger.LogInfoAsync("User logging out");
                    await _userService.LogoutAsync();
                    
                    // Buat instance LoginViewModel baru
                    var loginViewModel = new LoginViewModel(this, _dbContext, _logger, _userService);
                    
                    // Navigasi ke login screen
                    CurrentViewModel = loginViewModel;
                    await _logger.LogInfoAsync("Successfully navigated to Login screen");
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync("Error during logout", ex);
                }
            });
        }
    }
}
