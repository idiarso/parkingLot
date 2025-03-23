using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using ParkingLotApp.Models;
using ParkingLotApp.Services;
using System.Windows.Input;
using ParkingLotApp.Services.Interfaces;
using Avalonia.Threading;
using System.Linq;

namespace ParkingLotApp.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IUserService _userService;
        private List<Setting> _settings;
        private string _statusMessage = string.Empty;
        private bool _isBusy;
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;

        public List<Setting> Settings
        {
            get => _settings;
            set => this.RaiseAndSetIfChanged(ref _settings, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => this.RaiseAndSetIfChanged(ref _isBusy, value);
        }

        public string CurrentPassword
        {
            get => _currentPassword;
            set => this.RaiseAndSetIfChanged(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => this.RaiseAndSetIfChanged(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => this.RaiseAndSetIfChanged(ref _confirmPassword, value);
        }

        // Helper properties for easier binding
        public string TotalSpots 
        {
            get => GetSettingValue("total_spots", "100");
            set => UpdateSettingValue("total_spots", value);
        }

        public string CarRate
        {
            get => GetSettingValue("car_rate", "5000");
            set => UpdateSettingValue("car_rate", value);
        }

        public string MotorcycleRate
        {
            get => GetSettingValue("motorcycle_rate", "2000");
            set => UpdateSettingValue("motorcycle_rate", value);
        }

        public string TruckRate
        {
            get => GetSettingValue("truck_rate", "10000");
            set => UpdateSettingValue("truck_rate", value);
        }

        public string BusRate
        {
            get => GetSettingValue("bus_rate", "15000");
            set => UpdateSettingValue("bus_rate", value);
        }

        public string CompanyName
        {
            get => GetSettingValue("company_name", "Parking Management System");
            set => UpdateSettingValue("company_name", value);
        }

        public string CompanyAddress
        {
            get => GetSettingValue("company_address", "123 Main Street");
            set => UpdateSettingValue("company_address", value);
        }

        public string ReportFooter
        {
            get => GetSettingValue("report_footer", "Thank you for your business!");
            set => UpdateSettingValue("report_footer", value);
        }

        public ICommand SaveSettingsCommand { get; }
        public ICommand LoadSettingsCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        public SettingsViewModel(ISettingsService settingsService, IUserService userService)
        {
            _settingsService = settingsService;
            _userService = userService;
            _settings = new List<Setting>();
            
            SaveSettingsCommand = ReactiveCommand.CreateFromTask(SaveSettingsAsync);
            LoadSettingsCommand = ReactiveCommand.CreateFromTask(LoadSettingsAsync);
            ChangePasswordCommand = ReactiveCommand.CreateFromTask(ChangePasswordAsync);
            
            Task.Run(async () => await LoadSettingsAsync());
        }

        private string GetSettingValue(string key, string defaultValue)
        {
            var setting = _settings?.FirstOrDefault(s => s.Key == key);
            return setting?.Value ?? defaultValue;
        }

        private void UpdateSettingValue(string key, string value)
        {
            var setting = _settings?.FirstOrDefault(s => s.Key == key);
            if (setting != null)
            {
                setting.Value = value;
            }
            else if (_settings != null)
            {
                _settings.Add(new Setting
                {
                    Key = key,
                    Value = value,
                    Description = "Auto-created setting"
                });
            }
        }

        private async Task LoadSettingsAsync()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    IsBusy = true;
                    StatusMessage = "Loading settings...";
                });

                var settings = await _settingsService.GetSettingsListAsync();
                
                // If no settings yet, create default settings
                if (settings.Count == 0)
                {
                    settings = new List<Setting>
                    {
                        new Setting { Key = "total_spots", Value = "100", Description = "Total parking spots available" },
                        new Setting { Key = "car_rate", Value = "5000", Description = "Parking rate for cars (hourly)" },
                        new Setting { Key = "motorcycle_rate", Value = "2000", Description = "Parking rate for motorcycles (hourly)" },
                        new Setting { Key = "truck_rate", Value = "10000", Description = "Parking rate for trucks (hourly)" },
                        new Setting { Key = "bus_rate", Value = "15000", Description = "Parking rate for buses (hourly)" },
                        new Setting { Key = "company_name", Value = "Parking Management System", Description = "Company name for reports" },
                        new Setting { Key = "company_address", Value = "123 Main Street", Description = "Company address for reports" },
                        new Setting { Key = "report_footer", Value = "Thank you for your business!", Description = "Footer text for reports" }
                    };

                    // Save default settings
                    var currentUser = await _userService.GetCurrentUserAsync();
                    foreach (var setting in settings)
                    {
                        await _settingsService.UpdateSettingAsync(setting.Key, setting.Value, currentUser?.Id ?? 1);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    Settings = settings;
                    StatusMessage = "Settings loaded successfully";
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    StatusMessage = $"Error loading settings: {ex.Message}";
                });
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
            }
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    IsBusy = true;
                    StatusMessage = "Saving settings...";
                });

                var currentUser = await _userService.GetCurrentUserAsync();
                var userId = currentUser?.Id ?? 1;

                foreach (var setting in Settings)
                {
                    await _settingsService.UpdateSettingAsync(setting.Key, setting.Value, userId);
                }

                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    StatusMessage = "Settings saved successfully";
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    StatusMessage = $"Error saving settings: {ex.Message}";
                });
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
            }
        }

        private async Task ChangePasswordAsync()
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    IsBusy = true;
                    StatusMessage = "Changing password...";
                });

                // Validate password fields
                if (string.IsNullOrWhiteSpace(CurrentPassword) || 
                    string.IsNullOrWhiteSpace(NewPassword) || 
                    string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    StatusMessage = "All password fields are required";
                    return;
                }

                if (NewPassword != ConfirmPassword)
                {
                    StatusMessage = "New password and confirmation do not match";
                    return;
                }

                if (NewPassword.Length < 6)
                {
                    StatusMessage = "New password must be at least 6 characters";
                    return;
                }

                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null)
                {
                    StatusMessage = "You must be logged in to change your password";
                    return;
                }

                bool success = await _userService.ChangePasswordAsync(
                    currentUser.Id, CurrentPassword, NewPassword);

                if (success)
                {
                    await Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        StatusMessage = "Password changed successfully";
                        CurrentPassword = string.Empty;
                        NewPassword = string.Empty;
                        ConfirmPassword = string.Empty;
                    });
                }
                else
                {
                    await Dispatcher.UIThread.InvokeAsync(() => 
                    {
                        StatusMessage = "Failed to change password. Current password may be incorrect.";
                    });
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() => 
                {
                    StatusMessage = $"Error changing password: {ex.Message}";
                });
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() => IsBusy = false);
            }
        }
    }
} 