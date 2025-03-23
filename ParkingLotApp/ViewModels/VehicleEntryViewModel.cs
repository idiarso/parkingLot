using System;
using System.Windows.Input;
using ReactiveUI;
using System.Collections.ObjectModel;
using ParkingLotApp.Models;
using ParkingLotApp.Services;
using ParkingLotApp.Services.Interfaces;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace ParkingLotApp.ViewModels
{
    public class VehicleEntryViewModel : ViewModelBase
    {
        private string _vehicleNumber = string.Empty;
        private string _vehicleType = "Car";
        private string _notes = string.Empty;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;
        private readonly IParkingService _parkingService;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly ILogger _logger;

        public string VehicleNumber
        {
            get => _vehicleNumber;
            set => this.RaiseAndSetIfChanged(ref _vehicleNumber, value?.ToUpper());
        }

        public string SelectedVehicleType
        {
            get => _vehicleType;
            set => this.RaiseAndSetIfChanged(ref _vehicleType, value);
        }

        public string Notes
        {
            get => _notes;
            set => this.RaiseAndSetIfChanged(ref _notes, value);
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

        public ObservableCollection<string> VehicleTypes { get; } = new()
        {
            "Car",
            "Motorcycle",
            "Truck",
            "Bus"
        };

        public ICommand RegisterEntryCommand { get; }
        public ICommand ClearFormCommand { get; }

        public VehicleEntryViewModel(IParkingService parkingService, MainWindowViewModel mainWindowViewModel, ILogger logger)
        {
            _mainWindowViewModel = mainWindowViewModel;
            _parkingService = parkingService;
            _logger = logger;
            RegisterEntryCommand = ReactiveCommand.CreateFromTask(RegisterEntryAsync);
            ClearFormCommand = ReactiveCommand.Create(ClearForm);
        }

        private async Task RegisterEntryAsync()
        {
            if (string.IsNullOrWhiteSpace(VehicleNumber))
            {
                StatusMessage = "Error: Vehicle number is required";
                return;
            }

            try
            {
                IsProcessing = true;
                var result = await _parkingService.RegisterVehicleEntryAsync(
                    VehicleNumber, 
                    SelectedVehicleType, 
                    Notes);

                if (result)
                {
                    StatusMessage = "Vehicle entry registered successfully";
                    ClearForm();
                }
                else
                {
                    StatusMessage = "Error: Failed to register vehicle entry";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                await _logger.LogErrorAsync($"Failed to register vehicle entry: {ex.Message}", ex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearForm()
        {
            VehicleNumber = string.Empty;
            SelectedVehicleType = "Car";
            Notes = string.Empty;
            StatusMessage = string.Empty;
        }
    }
} 