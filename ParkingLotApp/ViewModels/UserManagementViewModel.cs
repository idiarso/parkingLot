using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ReactiveUI;
using ParkingLotApp.Models;
using ParkingLotApp.Services;
using ParkingLotApp.Services.Interfaces;
using Avalonia.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParkingLotApp.ViewModels
{
    public class UserManagementViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private string _statusMessage = string.Empty;
        private bool _isProcessing;
        private string _selectedTab = "Users";

        // User Management
        private ObservableCollection<User> _users;
        private User? _selectedUser;
        private string _username;
        private string _password;
        private string _email;
        private string _firstName;
        private string _lastName;
        private string _role;
        private int _selectedRoleId;
        private bool _isActive;

        // Shift Management
        private ObservableCollection<Shift> _shifts;
        private Shift? _selectedShift;
        private string _newShiftName = string.Empty;
        private TimeSpan _newShiftStartTime;
        private TimeSpan _newShiftEndTime;
        private string _newShiftDescription = string.Empty;

        // Shift Assignment
        private ObservableCollection<UserShift> _userShifts;
        private DateTime _selectedDate = DateTime.Today;
        private User? _selectedOperator;
        private Shift? _selectedAssignShift;

        public new event PropertyChangedEventHandler? PropertyChanged;

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

        public string SelectedTab
        {
            get => _selectedTab;
            set => this.RaiseAndSetIfChanged(ref _selectedTab, value);
        }

        // User Management Properties
        public ObservableCollection<User> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedUser, value);
                if (value != null)
                {
                    Username = value.Username;
                    Email = value.Email;
                    FirstName = value.FirstName;
                    LastName = value.LastName;
                    Role = value.Role.Name;
                }
            }
        }

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

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string FirstName
        {
            get => _firstName;
            set => this.RaiseAndSetIfChanged(ref _firstName, value);
        }

        public string LastName
        {
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }

        public string Role
        {
            get => _role;
            set => this.RaiseAndSetIfChanged(ref _role, value);
        }

        public int SelectedRoleId
        {
            get => _selectedRoleId;
            set
            {
                if (_selectedRoleId != value)
                {
                    _selectedRoleId = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        // Shift Management Properties
        public ObservableCollection<Shift> Shifts
        {
            get => _shifts;
            set => this.RaiseAndSetIfChanged(ref _shifts, value);
        }

        public Shift? SelectedShift
        {
            get => _selectedShift;
            set => this.RaiseAndSetIfChanged(ref _selectedShift, value);
        }

        public string NewShiftName
        {
            get => _newShiftName;
            set => this.RaiseAndSetIfChanged(ref _newShiftName, value);
        }

        public TimeSpan NewShiftStartTime
        {
            get => _newShiftStartTime;
            set => this.RaiseAndSetIfChanged(ref _newShiftStartTime, value);
        }

        public TimeSpan NewShiftEndTime
        {
            get => _newShiftEndTime;
            set => this.RaiseAndSetIfChanged(ref _newShiftEndTime, value);
        }

        public string NewShiftDescription
        {
            get => _newShiftDescription;
            set => this.RaiseAndSetIfChanged(ref _newShiftDescription, value);
        }

        // Shift Assignment Properties
        public ObservableCollection<UserShift> UserShifts
        {
            get => _userShifts;
            set => this.RaiseAndSetIfChanged(ref _userShifts, value);
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
        }

        public User? SelectedOperator
        {
            get => _selectedOperator;
            set => this.RaiseAndSetIfChanged(ref _selectedOperator, value);
        }

        public Shift? SelectedAssignShift
        {
            get => _selectedAssignShift;
            set => this.RaiseAndSetIfChanged(ref _selectedAssignShift, value);
        }

        public ObservableCollection<string> Roles { get; } = new()
        {
            "Admin",
            "Operator",
            "Viewer"
        };

        // Commands
        public ICommand CreateUserCommand { get; }
        public ICommand UpdateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand AddShiftCommand { get; }
        public ICommand UpdateShiftCommand { get; }
        public ICommand DeleteShiftCommand { get; }
        public ICommand AssignShiftCommand { get; }
        public ICommand RemoveShiftAssignmentCommand { get; }
        public ICommand RefreshDataCommand { get; }

        public UserManagementViewModel(IUserService userService, MainWindowViewModel mainWindowViewModel)
        {
            _userService = userService;
            _mainWindowViewModel = mainWindowViewModel;
            _users = new ObservableCollection<User>();
            _shifts = new ObservableCollection<Shift>();
            _userShifts = new ObservableCollection<UserShift>();

            CreateUserCommand = ReactiveCommand.CreateFromTask(CreateUserAsync);
            UpdateUserCommand = ReactiveCommand.CreateFromTask(UpdateUserAsync);
            DeleteUserCommand = ReactiveCommand.CreateFromTask(DeleteUserAsync);
            AddShiftCommand = ReactiveCommand.CreateFromTask(AddShiftAsync);
            UpdateShiftCommand = ReactiveCommand.CreateFromTask(UpdateShiftAsync);
            DeleteShiftCommand = ReactiveCommand.CreateFromTask(DeleteShiftAsync);
            AssignShiftCommand = ReactiveCommand.CreateFromTask(AssignShiftAsync);
            RemoveShiftAssignmentCommand = ReactiveCommand.CreateFromTask(RemoveShiftAssignmentAsync);
            RefreshDataCommand = ReactiveCommand.CreateFromTask(LoadDataAsync);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsProcessing = true;
                var users = await _userService.GetAllUsersAsync();
                var shifts = await _userService.GetAllShiftsAsync();
                var userShifts = await _userService.GetUserShiftsAsync(SelectedDate);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Users.Clear();
                    foreach (var user in users)
                    {
                        Users.Add(user);
                    }

                    Shifts.Clear();
                    foreach (var shift in shifts)
                    {
                        Shifts.Add(shift);
                    }

                    UserShifts.Clear();
                    foreach (var userShift in userShifts)
                    {
                        UserShifts.Add(userShift);
                    }
                    
                    StatusMessage = "Data loaded successfully";
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StatusMessage = $"Error loading data: {ex.Message}";
                });
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    IsProcessing = false;
                });
            }
        }

        private async Task CreateUserAsync()
        {
            try
            {
                IsProcessing = true;
                var user = new User
                {
                    Username = Username,
                    Email = Email,
                    FirstName = FirstName,
                    LastName = LastName,
                    RoleId = SelectedRoleId,
                    IsActive = IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser = await _userService.CreateUserAsync(user, Password);
                await LoadDataAsync();
                StatusMessage = "User created successfully";
                ClearForm();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task UpdateUserAsync()
        {
            if (SelectedUser == null)
            {
                StatusMessage = "Error: No user selected";
                return;
            }

            try
            {
                IsProcessing = true;
                SelectedUser.Username = Username;
                SelectedUser.Email = Email;
                SelectedUser.FirstName = FirstName;
                SelectedUser.LastName = LastName;
                SelectedUser.RoleId = SelectedRoleId;
                SelectedUser.IsActive = IsActive;

                var updatedUser = await _userService.UpdateUserAsync(SelectedUser, string.IsNullOrEmpty(Password) ? null : Password);
                
                await LoadDataAsync();
                StatusMessage = "User updated successfully";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null)
            {
                StatusMessage = "Error: No user selected";
                return;
            }

            try
            {
                IsProcessing = true;
                var success = await _userService.DeleteUserAsync(SelectedUser.Id);
                
                if (success)
                {
                    await LoadDataAsync();
                    StatusMessage = "User deleted successfully";
                }
                else
                {
                    StatusMessage = "Error: Failed to delete user";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task AddShiftAsync()
        {
            if (string.IsNullOrWhiteSpace(NewShiftName))
            {
                StatusMessage = "Error: Shift name is required";
                return;
            }

            try
            {
                IsProcessing = true;
                var shift = new Shift
                {
                    Name = NewShiftName,
                    StartTime = NewShiftStartTime,
                    EndTime = NewShiftEndTime,
                    Description = NewShiftDescription
                };

                var success = await _userService.CreateShiftAsync(shift);
                
                if (success)
                {
                    await LoadDataAsync();
                    NewShiftName = string.Empty;
                    NewShiftDescription = string.Empty;
                    StatusMessage = "Shift created successfully";
                }
                else
                {
                    StatusMessage = "Error: Failed to create shift";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task UpdateShiftAsync()
        {
            if (SelectedShift == null)
            {
                StatusMessage = "Error: No shift selected";
                return;
            }

            try
            {
                IsProcessing = true;
                var success = await _userService.UpdateShiftAsync(SelectedShift);
                
                if (success)
                {
                    await LoadDataAsync();
                    StatusMessage = "Shift updated successfully";
                }
                else
                {
                    StatusMessage = "Error: Failed to update shift";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task DeleteShiftAsync()
        {
            if (SelectedShift == null)
            {
                StatusMessage = "Error: No shift selected";
                return;
            }

            try
            {
                IsProcessing = true;
                var success = await _userService.DeleteShiftAsync(SelectedShift.Id);
                
                if (success)
                {
                    await LoadDataAsync();
                    StatusMessage = "Shift deleted successfully";
                }
                else
                {
                    StatusMessage = "Error: Failed to delete shift";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task AssignShiftAsync()
        {
            if (SelectedOperator == null || SelectedAssignShift == null)
            {
                StatusMessage = "Error: Please select both operator and shift";
                return;
            }

            try
            {
                IsProcessing = true;
                var success = await _userService.AssignShiftAsync(SelectedOperator.Id, SelectedAssignShift.Id, SelectedDate);
                
                if (success)
                {
                    await LoadDataAsync();
                    StatusMessage = "Shift assigned successfully";
                }
                else
                {
                    StatusMessage = "Error: Failed to assign shift";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private async Task RemoveShiftAssignmentAsync()
        {
            if (SelectedOperator == null || SelectedAssignShift == null)
            {
                StatusMessage = "Error: Please select both operator and shift";
                return;
            }

            try
            {
                IsProcessing = true;
                var success = await _userService.RemoveShiftAssignmentAsync(SelectedOperator.Id, SelectedAssignShift.Id, SelectedDate);
                
                if (success)
                {
                    await LoadDataAsync();
                    StatusMessage = "Shift assignment removed successfully";
                }
                else
                {
                    StatusMessage = "Error: Failed to remove shift assignment";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            Email = string.Empty;
            FirstName = string.Empty;
            LastName = string.Empty;
            SelectedRoleId = 0;
            IsActive = false;
            SelectedUser = null;
        }

        protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 