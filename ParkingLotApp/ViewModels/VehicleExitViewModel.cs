using System;
using System.Windows.Input;
using ReactiveUI;
using ParkingLotApp.Models;
using System.Threading.Tasks;
using ParkingLotApp.Services;
using ParkingLotApp.Services.Interfaces;
using Avalonia.Threading;
using ParkingLotApp.Utils;

namespace ParkingLotApp.ViewModels
{
    public class VehicleExitViewModel : ViewModelBase
    {
        private string _searchVehicleNumber = string.Empty;
        private ParkingActivity? _currentParking;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;
        private decimal _calculatedFee;
        private string _duration = string.Empty;
        private string _vehicleNumber = string.Empty;
        private readonly IParkingService _parkingService;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ILogger _logger;

        public string SearchVehicleNumber
        {
            get => _searchVehicleNumber;
            set => this.RaiseAndSetIfChanged(ref _searchVehicleNumber, value?.ToUpper() ?? string.Empty);
        }

        public ParkingActivity? CurrentParking
        {
            get => _currentParking;
            set => this.RaiseAndSetIfChanged(ref _currentParking, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set => this.RaiseAndSetIfChanged(ref _isProcessing, value);
        }

        public decimal CalculatedFee
        {
            get => _calculatedFee;
            set => this.RaiseAndSetIfChanged(ref _calculatedFee, value);
        }

        public string Duration
        {
            get => _duration;
            set => this.RaiseAndSetIfChanged(ref _duration, value);
        }

        public bool HasVehicleFound => CurrentParking != null;

        public ICommand SearchCommand { get; }
        public ICommand ProcessExitCommand { get; }
        public ICommand ClearCommand { get; }

        public VehicleExitViewModel(IParkingService parkingService, MainWindowViewModel mainWindowViewModel, ILogger logger)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _parkingService = parkingService;
            _logger = logger;
            SearchCommand = ReactiveCommand.CreateFromTask(SearchVehicleAsync);
            ProcessExitCommand = ReactiveCommand.CreateFromTask(ProcessExitAsync);
            ClearCommand = ReactiveCommand.Create(ClearForm);
        }

        private async Task SearchVehicleAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchVehicleNumber))
            {
                StatusMessage = "Error: Please enter a vehicle number";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Searching...";

            try
            {
                CurrentParking = await _parkingService.GetVehicleEntryAsync(SearchVehicleNumber);
                
                if (CurrentParking != null)
                {
                    var parkingDuration = DateTime.Now - CurrentParking.Time;
                    Duration = FormatDuration(parkingDuration);
                    CalculatedFee = CalculateFee(parkingDuration, CurrentParking.VehicleType);
                    StatusMessage = "Vehicle found";
                }
                else
                {
                    StatusMessage = $"Error: No parking record found for vehicle {SearchVehicleNumber}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                await _logger.LogErrorAsync($"Failed to search vehicle: {ex.Message}", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task ProcessExitAsync()
        {
            if (CurrentParking == null)
            {
                StatusMessage = "Error: No vehicle selected";
                return;
            }

            IsProcessing = true;
            StatusMessage = "Processing exit...";

            try
            {
                var success = await _parkingService.RegisterVehicleExitAsync(
                    CurrentParking.VehicleNumber,
                    CalculatedFee,
                    Duration);

                if (success)
                {
                    StatusMessage = $"Vehicle {CurrentParking.VehicleNumber} successfully exited";
                    ClearForm();
                }
                else
                {
                    StatusMessage = "Error: Failed to process vehicle exit";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                await _logger.LogErrorAsync($"Failed to process vehicle exit: {ex.Message}", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearForm()
        {
            SearchVehicleNumber = string.Empty;
            CurrentParking = null;
            Duration = string.Empty;
            CalculatedFee = 0;
            StatusMessage = string.Empty;
        }

        private string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays} days {duration.Hours} hours {duration.Minutes} minutes";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{(int)duration.TotalHours} hours {duration.Minutes} minutes";
            }
            else
            {
                return $"{duration.Minutes} minutes";
            }
        }

        private decimal CalculateFee(TimeSpan duration, string vehicleType)
        {
            // TODO: Get rates from settings
            decimal hourlyRate = vehicleType.ToLower() switch
            {
                "car" => 5000m,
                "motorcycle" => 2000m,
                "truck" => 10000m,
                "bus" => 8000m,
                _ => 5000m
            };

            var hours = Math.Ceiling(duration.TotalHours);
            return hourlyRate * (decimal)hours;
        }
    }
} 