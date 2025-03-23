using System;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ParkingLotApp.Services;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ParkingLotApp.Data;
using Avalonia.Threading;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isProcessing;
        private bool _rememberMe;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly Data.ParkingDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IUserService _userService;
        private readonly ISettingsService _settingsService;

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => this.RaiseAndSetIfChanged(ref _isProcessing, value);
        }
        
        public bool RememberMe
        {
            get => _rememberMe;
            set => this.RaiseAndSetIfChanged(ref _rememberMe, value);
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel(
            MainWindowViewModel mainWindowViewModel, 
            Data.ParkingDbContext dbContext, 
            ILogger logger, 
            IUserService userService,
            ISettingsService settingsService)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            _settingsService = settingsService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
            
            // Load saved credentials if available
            LoadSavedCredentials();
        }
        
        private void LoadSavedCredentials()
        {
            try
            {
                // Baca username dari settings jika ada
                var settings = _settingsService?.GetAllSettingsAsync().Result;
                if (settings != null && settings.TryGetValue("saved_username", out string savedUsername))
                {
                    Username = savedUsername;
                    RememberMe = true;
                    Console.WriteLine("[Debug] Loaded saved username from settings");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to load saved credentials: {ex.Message}");
                // Tidak perlu menampilkan error ke user
            }
        }
        
        private async Task SaveCredentials()
        {
            if (RememberMe && !string.IsNullOrEmpty(Username))
            {
                try
                {
                    // Simpan username ke settings
                    await _settingsService.UpdateSettingAsync("saved_username", Username, 1); // 1 = system user ID
                    Console.WriteLine("[Debug] Saved username to settings");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Warning] Failed to save credentials: {ex.Message}");
                    // Tidak perlu menampilkan error ke user
                }
            }
            else if (!RememberMe)
            {
                try
                {
                    // Hapus saved username jika RememberMe dimatikan
                    await _settingsService.UpdateSettingAsync("saved_username", "", 1);
                    Console.WriteLine("[Debug] Removed saved username from settings");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Warning] Failed to remove saved credentials: {ex.Message}");
                }
            }
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and password are required";
                return;
            }

            IsProcessing = true;
            ErrorMessage = string.Empty;

            try
            {
                await _logger.LogInfoAsync($"Login attempt: {Username}");
                Console.WriteLine($"[Debug] Login attempt for user: {Username}");
                
                // Gunakan IUserService untuk login
                var user = await _userService.AuthenticateAsync(Username, Password);

                if (user != null && user.Id > 0)
                {
                    // Login successful
                    ErrorMessage = "";
                    
                    // Simpan kredensial jika remember me dicentang
                    await SaveCredentials();
                    
                    await _logger.LogInfoAsync($"Login successful: {Username} (User ID: {user.Id})");
                    Console.WriteLine($"[Debug] User {Username} logged in successfully");
                    
                    // Update status in main view model
                    _mainWindowViewModel.StatusMessage = $"Welcome, {user.FirstName}! Preparing dashboard...";
                    
                    // Navigasi ke dashboard
                    _mainWindowViewModel.ShowDashboardCommand.Execute(null);
                    
                    // Clear status message after successful navigation
                    await Task.Delay(2000); // Give user time to see welcome message
                    _mainWindowViewModel.StatusMessage = "";
                }
                else
                {
                    // Login failed
                    ErrorMessage = "Invalid username or password";
                    await _logger.LogWarningAsync($"Failed login attempt: {Username} (Invalid credentials)");
                    Console.WriteLine($"[Debug] Login failed for user: {Username} - Invalid credentials");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                await _logger.LogErrorAsync($"Login error for user {Username}: {ex.Message}", ex);
                Console.WriteLine($"[Error] Login error for {Username}: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Error] Inner exception: {ex.InnerException.Message}");
                }
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
} 