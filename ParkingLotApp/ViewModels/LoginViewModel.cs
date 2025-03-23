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
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly Data.ParkingDbContext _dbContext;
        private readonly ILogger _logger;
        private readonly IUserService _userService;

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

        public ICommand LoginCommand { get; }

        public LoginViewModel(MainWindowViewModel mainWindowViewModel, Data.ParkingDbContext dbContext, ILogger logger, IUserService userService)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _dbContext = dbContext;
            _logger = logger;
            _userService = userService;
            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync);
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
                // Gunakan IUserService untuk login
                var user = await _userService.AuthenticateAsync(Username, Password);

                if (user != null && user.Id > 0)
                {
                    // Login successful
                    ErrorMessage = "";
                    // Tidak perlu menunggu tampilan dashboard
                    _mainWindowViewModel.ShowDashboardCommand.Execute(null);
                }
                else
                {
                    // Login failed
                    ErrorMessage = "Invalid username or password";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
                await _logger.LogErrorAsync("Login error", ex);
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