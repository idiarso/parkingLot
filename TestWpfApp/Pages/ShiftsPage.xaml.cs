using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestWpfApp
{
    public partial class ShiftsPage : Page, INotifyPropertyChanged
    {
        // Fields for controls that are not in XAML
        private DataGrid dgShifts;
        private TextBox txtOperatorName;
        private TextBox txtStartTime;
        private TextBox txtEndTime;
        private ComboBox cmbStatus;
        private TextBox txtSearch;

        private ObservableCollection<ShiftAssignment> _shiftAssignments;
        private ShiftAssignment? _selectedShift;
        private DateTime _selectedDate = DateTime.Today;
        private string _searchText = string.Empty;
        private List<Border> _calendarCells = new List<Border>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ShiftsPage()
        {
            InitializeComponent();
            
            // These controls would normally be created in code or bound dynamically
            // For now, we'll just initialize them to avoid null reference exceptions
            dgShifts = new DataGrid();
            txtOperatorName = new TextBox();
            txtStartTime = new TextBox();
            txtEndTime = new TextBox();
            cmbStatus = new ComboBox();
            txtSearch = new TextBox();
            
            _shiftAssignments = new ObservableCollection<ShiftAssignment>();
            LoadSampleData();
            UpdateCalendarDisplay();
        }

        public ObservableCollection<ShiftAssignment> ShiftAssignments
        {
            get => _shiftAssignments;
            set
            {
                _shiftAssignments = value;
                OnPropertyChanged(nameof(ShiftAssignments));
            }
        }

        public ShiftAssignment? SelectedShift
        {
            get => _selectedShift;
            set
            {
                _selectedShift = value;
                OnPropertyChanged(nameof(SelectedShift));
                if (value != null)
                {
                    FillFormWithShift(value);
                }
            }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                FilterShifts();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterShifts();
            }
        }

        private void LoadSampleData()
        {
            // Sample data for shifts
            _shiftAssignments.Add(new ShiftAssignment
            {
                Date = DateTime.Today,
                OperatorName = "John Doe",
                ShiftType = "Morning",
                StartTime = "06:00",
                EndTime = "14:00",
                Status = "Active"
            });

            _shiftAssignments.Add(new ShiftAssignment
            {
                Date = DateTime.Today,
                OperatorName = "Jane Smith",
                ShiftType = "Afternoon",
                StartTime = "14:00",
                EndTime = "22:00",
                Status = "Active"
            });

            _shiftAssignments.Add(new ShiftAssignment
            {
                Date = DateTime.Today.AddDays(1),
                OperatorName = "Mike Johnson",
                ShiftType = "Night",
                StartTime = "22:00",
                EndTime = "06:00",
                Status = "Scheduled"
            });

            // Disable this line as dgShifts is just a placeholder
            // dgShifts.ItemsSource = _shiftAssignments;
        }

        private void FilterShifts()
        {
            // Skip filtering since dgShifts is just a placeholder
            /*
            var filteredShifts = _shiftAssignments.Where(s =>
                s.Date.Date == _selectedDate.Date &&
                (string.IsNullOrEmpty(_searchText) ||
                 s.OperatorName.Contains(_searchText, StringComparison.OrdinalIgnoreCase) ||
                 s.ShiftType.Contains(_searchText, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            dgShifts.ItemsSource = filteredShifts;
            */
        }

        private void FillFormWithShift(ShiftAssignment shift)
        {
            // Comment out lines that are placeholders
            /*
            txtOperatorName.Text = shift.OperatorName;
            cmbShiftType.SelectedItem = shift.ShiftType;
            txtStartTime.Text = shift.StartTime;
            txtEndTime.Text = shift.EndTime;
            cmbStatus.SelectedItem = shift.Status;
            */
            
            // Instead, update the XAML-defined controls
            if (cmbOperator.Items.Count > 0)
            {
                // Find the item that matches the operator name
                foreach (ComboBoxItem item in cmbOperator.Items)
                {
                    if (item.Content.ToString() == shift.OperatorName)
                    {
                        cmbOperator.SelectedItem = item;
                        break;
                    }
                }
            }
            
            if (cmbShiftType.Items.Count > 0)
            {
                // Find the item that matches the shift type
                foreach (ComboBoxItem item in cmbShiftType.Items)
                {
                    if (item.Content.ToString().StartsWith(shift.ShiftType))
                    {
                        cmbShiftType.SelectedItem = item;
                        break;
                    }
                }
            }
            
            if (txtNotes != null)
            {
                txtNotes.Text = shift.Notes ?? string.Empty;
            }
        }

        private ShiftAssignment GetShiftFromForm()
        {
            string operatorName = "Unknown";
            string shiftType = "Morning";
            
            if (cmbOperator.SelectedItem is ComboBoxItem selectedOperator)
            {
                operatorName = selectedOperator.Content.ToString() ?? "Unknown";
            }
            
            if (cmbShiftType.SelectedItem is ComboBoxItem selectedShiftType)
            {
                shiftType = selectedShiftType.Content.ToString()?.Split(' ')[0] ?? "Morning";
            }
            
            return new ShiftAssignment
            {
                Date = dpShiftDate.SelectedDate ?? DateTime.Today,
                OperatorName = operatorName,
                ShiftType = shiftType,
                StartTime = "06:00", // Default values since we don't have the actual controls
                EndTime = "14:00",   // Default values since we don't have the actual controls
                Status = "Active",   // Default values since we don't have the actual controls
                Notes = txtNotes?.Text
            };
        }

        private void ClearForm()
        {
            // Clear the XAML-defined controls
            dpShiftDate.SelectedDate = DateTime.Today;
            cmbShiftType.SelectedIndex = 0;
            cmbOperator.SelectedIndex = 0;
            if (txtNotes != null) txtNotes.Text = string.Empty;
            
            _selectedShift = null;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Event Handlers
        private void btnAddNew_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShift != null)
            {
                var updatedShift = GetShiftFromForm();
                var index = _shiftAssignments.IndexOf(_selectedShift);
                if (index != -1)
                {
                    _shiftAssignments[index] = updatedShift;
                    FilterShifts();
                }
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShift != null)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this shift?",
                    "Delete Shift",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _shiftAssignments.Remove(_selectedShift);
                    ClearForm();
                    FilterShifts();
                }
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShift == null)
            {
                var newShift = GetShiftFromForm();
                _shiftAssignments.Add(newShift);
            }
            else
            {
                var updatedShift = GetShiftFromForm();
                var index = _shiftAssignments.IndexOf(_selectedShift);
                if (index != -1)
                {
                    _shiftAssignments[index] = updatedShift;
                }
            }

            FilterShifts();
            ClearForm();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void dgShifts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // This method won't be used for now since dgShifts is just a placeholder
            /*
            if (dgShifts.SelectedItem is ShiftAssignment selectedShift)
            {
                SelectedShift = selectedShift;
            }
            */
        }

        private void dpShiftDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DatePicker datePicker && datePicker.SelectedDate.HasValue)
            {
                _selectedDate = datePicker.SelectedDate.Value;
                UpdateCalendarDisplay();
            }
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SearchText = textBox.Text;
            }
        }

        private void btnPreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            _selectedDate = _selectedDate.AddMonths(-1);
            UpdateCalendarDisplay();
        }

        private void btnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            _selectedDate = _selectedDate.AddMonths(1);
            UpdateCalendarDisplay();
        }

        private void cmbShiftType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShiftType.SelectedItem != null && dpShiftDate.SelectedDate.HasValue)
            {
                string selectedShiftType = cmbShiftType.SelectedItem.ToString() ?? string.Empty;
                DateTime selectedDate = dpShiftDate.SelectedDate.Value;

                var existingShift = _shiftAssignments.FirstOrDefault(s => 
                    s.Date.Date == selectedDate.Date && 
                    s.ShiftType == selectedShiftType);
                
                if (existingShift != null)
                {
                    FillFormWithShift(existingShift);
                    _selectedShift = existingShift;
                }
                else
                {
                    ClearForm();
                }
            }
        }

        private void btnClearShift_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShift != null)
            {
                var result = MessageBox.Show(
                    "Are you sure you want to clear this shift assignment?",
                    "Confirm Clear Shift",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _shiftAssignments.Remove(_selectedShift);
                    ClearForm();
                    UpdateCalendarDisplay();
                }
            }
        }

        private void btnAssignShift_Click(object sender, RoutedEventArgs e)
        {
            if (dpShiftDate.SelectedDate.HasValue && cmbShiftType.SelectedItem != null)
            {
                var newShift = GetShiftFromForm();
                
                if (_selectedShift != null)
                {
                    var index = _shiftAssignments.IndexOf(_selectedShift);
                    if (index != -1)
                    {
                        _shiftAssignments[index] = newShift;
                    }
                }
                else
                {
                    _shiftAssignments.Add(newShift);
                }

                UpdateCalendarDisplay();
                ClearForm();

                MessageBox.Show(
                    "Shift assigned successfully!",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(
                    "Please ensure all required fields are filled out.",
                    "Missing Information",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void btnManageShiftTypes_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Shift type management will be implemented in a future update.",
                "Coming Soon",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #region Calendar Display

        private void UpdateCalendarDisplay()
        {
            try
            {
                // Update month/year display
                txtCurrentMonth.Text = _selectedDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

                // Clear existing calendar
                calendarGrid.Children.Clear();
                _calendarCells.Clear();

                // First day of the month
                DateTime firstDayOfMonth = new DateTime(_selectedDate.Year, _selectedDate.Month, 1);
                
                // Calculate what day of the week the first day is (0 = Sunday, 1 = Monday, etc.)
                int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
                
                // Number of days in the month
                int daysInMonth = DateTime.DaysInMonth(_selectedDate.Year, _selectedDate.Month);
                
                // Create calendar cells
                int day = 1;
                for (int week = 0; week < 6; week++)
                {
                    for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
                    {
                        // Create a border for the day cell
                        Border dayCell = new Border
                        {
                            BorderBrush = new SolidColorBrush(Colors.LightGray),
                            BorderThickness = new Thickness(1),
                            Margin = new Thickness(-1)
                        };

                        // Set the grid position
                        Grid.SetRow(dayCell, week);
                        Grid.SetColumn(dayCell, dayOfWeek);

                        // Only add day content if it's within the current month
                        if ((week == 0 && dayOfWeek < firstDayOfWeek) || day > daysInMonth)
                        {
                            // Empty cell
                            dayCell.Background = new SolidColorBrush(Colors.WhiteSmoke);
                        }
                        else
                        {
                            // Valid day in month
                            Grid cellContent = new Grid();
                            cellContent.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                            cellContent.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

                            // Day number
                            TextBlock dayText = new TextBlock
                            {
                                Text = day.ToString(),
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(5, 2, 0, 0)
                            };
                            Grid.SetRow(dayText, 0);
                            cellContent.Children.Add(dayText);

                            // Container for shift assignments
                            StackPanel shiftsPanel = new StackPanel
                            {
                                Margin = new Thickness(2)
                            };
                            Grid.SetRow(shiftsPanel, 1);
                            cellContent.Children.Add(shiftsPanel);

                            // Check for shifts on this day
                            DateTime currentDay = new DateTime(_selectedDate.Year, _selectedDate.Month, day);
                            var shiftsOnDay = _shiftAssignments.Where(s => s.Date.Date == currentDay.Date).ToList();

                            // Add shift indicators
                            foreach (var shift in shiftsOnDay)
                            {
                                // Determine background color based on shift type
                                SolidColorBrush background;
                                if (shift.ShiftType.StartsWith("Morning"))
                                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E3F2FD"));
                                else if (shift.ShiftType.StartsWith("Evening"))
                                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                                else
                                    background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));

                                // Create shift indicator
                                Border shiftIndicator = new Border
                                {
                                    Background = background,
                                    CornerRadius = new CornerRadius(3),
                                    Margin = new Thickness(0, 2, 0, 0),
                                    Padding = new Thickness(3)
                                };

                                // Shift description text
                                string shiftTypeAbbrev = shift.ShiftType.Split(' ')[0]; // Just take Morning/Evening/Night
                                TextBlock shiftText = new TextBlock
                                {
                                    Text = $"{shiftTypeAbbrev}: {shift.OperatorName}",
                                    FontSize = 10,
                                    TextTrimming = TextTrimming.CharacterEllipsis
                                };
                                shiftIndicator.Child = shiftText;
                                shiftsPanel.Children.Add(shiftIndicator);
                            }

                            // Set cell content
                            dayCell.Child = cellContent;

                            // Highlight today's date
                            if (currentDay.Date == DateTime.Now.Date)
                            {
                                dayCell.BorderBrush = new SolidColorBrush(Colors.Blue);
                                dayCell.BorderThickness = new Thickness(2);
                            }

                            // Add click event to select the day
                            int clickDay = day; // Capture current day in closure
                            dayCell.MouseLeftButtonDown += (s, e) => DayCell_Click(clickDay);
                        }

                        // Add cell to grid and tracking list
                        calendarGrid.Children.Add(dayCell);
                        _calendarCells.Add(dayCell);
                        
                        // Move to next day if we added a valid day
                        if (!(week == 0 && dayOfWeek < firstDayOfWeek) && day <= daysInMonth)
                        {
                            day++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating calendar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DayCell_Click(int day)
        {
            // Set selected date in date picker
            dpShiftDate.SelectedDate = new DateTime(_selectedDate.Year, _selectedDate.Month, day);
        }

        #endregion
    }

    public class ShiftAssignment : INotifyPropertyChanged
    {
        private DateTime _date;
        private string _operatorName = string.Empty;
        private string _shiftType = string.Empty;
        private string _startTime = string.Empty;
        private string _endTime = string.Empty;
        private string _status = string.Empty;
        private string? _notes;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        public string OperatorName
        {
            get => _operatorName;
            set
            {
                _operatorName = value;
                OnPropertyChanged(nameof(OperatorName));
            }
        }

        public string ShiftType
        {
            get => _shiftType;
            set
            {
                _shiftType = value;
                OnPropertyChanged(nameof(ShiftType));
            }
        }

        public string StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }

        public string EndTime
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged(nameof(EndTime));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        
        public string? Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}