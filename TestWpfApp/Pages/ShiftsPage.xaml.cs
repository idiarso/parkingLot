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
        private DateTime _currentDate = DateTime.Now;
        private ObservableCollection<ShiftAssignment> _shiftAssignments;
        private ShiftAssignment _selectedShift;
        private List<Border> _calendarCells = new List<Border>();

        // Event for property changed notification
        public event PropertyChangedEventHandler PropertyChanged;

        public ShiftsPage()
        {
            InitializeComponent();
            DataContext = this;
            
            // Initialize sample data
            LoadSampleData();
            
            // Set current date display
            UpdateCalendarDisplay();
        }

        #region Sample Data

        private void LoadSampleData()
        {
            // Initialize with some sample shift assignments
            _shiftAssignments = new ObservableCollection<ShiftAssignment>
            {
                new ShiftAssignment 
                { 
                    Date = DateTime.Now.Date, 
                    ShiftType = "Morning (06:00 - 14:00)", 
                    Operator = "Operator1",
                    Notes = "Regular shift" 
                },
                new ShiftAssignment 
                { 
                    Date = DateTime.Now.Date, 
                    ShiftType = "Evening (14:00 - 22:00)", 
                    Operator = "Operator2",
                    Notes = "Covering for Operator4" 
                },
                new ShiftAssignment 
                { 
                    Date = DateTime.Now.Date, 
                    ShiftType = "Night (22:00 - 06:00)", 
                    Operator = "Operator3",
                    Notes = "" 
                },
                new ShiftAssignment 
                { 
                    Date = DateTime.Now.Date.AddDays(1), 
                    ShiftType = "Morning (06:00 - 14:00)", 
                    Operator = "Cashier1",
                    Notes = "" 
                },
                new ShiftAssignment 
                { 
                    Date = DateTime.Now.Date.AddDays(1), 
                    ShiftType = "Evening (14:00 - 22:00)", 
                    Operator = "Operator1",
                    Notes = "" 
                }
            };

            // Set today's date in the date picker
            dpShiftDate.SelectedDate = DateTime.Now.Date;
        }

        #endregion

        #region Calendar Display

        private void UpdateCalendarDisplay()
        {
            // Update month/year display
            txtCurrentMonth.Text = _currentDate.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            // Clear existing calendar
            calendarGrid.Children.Clear();
            _calendarCells.Clear();

            // First day of the month
            DateTime firstDayOfMonth = new DateTime(_currentDate.Year, _currentDate.Month, 1);
            
            // Calculate what day of the week the first day is (0 = Sunday, 1 = Monday, etc.)
            int firstDayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            
            // Number of days in the month
            int daysInMonth = DateTime.DaysInMonth(_currentDate.Year, _currentDate.Month);
            
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
                        DateTime currentDay = new DateTime(_currentDate.Year, _currentDate.Month, day);
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
                                Text = $"{shiftTypeAbbrev}: {shift.Operator}",
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

        private void DayCell_Click(int day)
        {
            // Set selected date in date picker
            dpShiftDate.SelectedDate = new DateTime(_currentDate.Year, _currentDate.Month, day);
        }

        #endregion

        #region Event Handlers

        private void btnPreviousMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(-1);
            UpdateCalendarDisplay();
        }

        private void btnNextMonth_Click(object sender, RoutedEventArgs e)
        {
            _currentDate = _currentDate.AddMonths(1);
            UpdateCalendarDisplay();
        }

        private void dpShiftDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dpShiftDate.SelectedDate.HasValue)
            {
                // Update UI based on selected date
                UpdateShiftAssignmentForm();
            }
        }

        private void cmbShiftType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbShiftType.SelectedItem != null && dpShiftDate.SelectedDate.HasValue)
            {
                // Check if there's already an assignment for this date and shift
                string selectedShiftType = ((ComboBoxItem)cmbShiftType.SelectedItem).Content.ToString();
                DateTime selectedDate = dpShiftDate.SelectedDate.Value;

                var existingShift = _shiftAssignments.FirstOrDefault(s => 
                    s.Date.Date == selectedDate.Date && 
                    s.ShiftType == selectedShiftType);
                
                if (existingShift != null)
                {
                    // Fill form with existing data
                    cmbOperator.Text = existingShift.Operator;
                    txtNotes.Text = existingShift.Notes;
                    _selectedShift = existingShift;
                }
                else
                {
                    // Clear form for new assignment
                    cmbOperator.SelectedIndex = 0;
                    txtNotes.Text = string.Empty;
                    _selectedShift = null;
                }
            }
        }

        private void btnClearShift_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedShift != null && dpShiftDate.SelectedDate.HasValue)
            {
                // Ask for confirmation
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to clear this shift assignment?",
                    "Confirm Clear Shift",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Remove the shift assignment
                    _shiftAssignments.Remove(_selectedShift);
                    _selectedShift = null;

                    // Clear form
                    ClearForm();

                    // Update calendar display
                    UpdateCalendarDisplay();
                }
            }
        }

        private void btnAssignShift_Click(object sender, RoutedEventArgs e)
        {
            if (dpShiftDate.SelectedDate.HasValue && cmbShiftType.SelectedItem != null && cmbOperator.SelectedItem != null)
            {
                string shiftType = ((ComboBoxItem)cmbShiftType.SelectedItem).Content.ToString();
                string operatorName = ((ComboBoxItem)cmbOperator.SelectedItem).Content.ToString();
                string notes = txtNotes.Text.Trim();
                
                // Check if we're updating an existing assignment or creating a new one
                if (_selectedShift != null)
                {
                    // Update existing assignment
                    _selectedShift.ShiftType = shiftType;
                    _selectedShift.Operator = operatorName;
                    _selectedShift.Notes = notes;
                }
                else
                {
                    // Create new assignment
                    ShiftAssignment newShift = new ShiftAssignment
                    {
                        Date = dpShiftDate.SelectedDate.Value,
                        ShiftType = shiftType,
                        Operator = operatorName,
                        Notes = notes
                    };

                    // Add to collection
                    _shiftAssignments.Add(newShift);
                }

                // Update calendar display
                UpdateCalendarDisplay();

                // Show success message
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

        #endregion

        #region Helper Methods

        private void UpdateShiftAssignmentForm()
        {
            if (dpShiftDate.SelectedDate.HasValue)
            {
                DateTime selectedDate = dpShiftDate.SelectedDate.Value;
                
                // Update title to show selected date
                txtShiftAssignmentTitle.Text = $"Assign Shift - {selectedDate:MMM d, yyyy}";
                
                // Find shift for current selection
                if (cmbShiftType.SelectedItem != null)
                {
                    string selectedShiftType = ((ComboBoxItem)cmbShiftType.SelectedItem).Content.ToString();
                    
                    var existingShift = _shiftAssignments.FirstOrDefault(s => 
                        s.Date.Date == selectedDate.Date && 
                        s.ShiftType == selectedShiftType);
                    
                    if (existingShift != null)
                    {
                        // Fill form with existing data
                        cmbOperator.Text = existingShift.Operator;
                        txtNotes.Text = existingShift.Notes;
                        _selectedShift = existingShift;
                    }
                    else
                    {
                        // Clear form for new assignment
                        ClearForm();
                        _selectedShift = null;
                    }
                }
            }
        }

        private void ClearForm()
        {
            cmbOperator.SelectedIndex = 0;
            txtNotes.Text = string.Empty;
        }

        #endregion
    }

    // Model for shift assignments
    public class ShiftAssignment : INotifyPropertyChanged
    {
        private DateTime _date;
        private string _shiftType;
        private string _operator;
        private string _notes;

        public DateTime Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged(nameof(Date));
                }
            }
        }

        public string ShiftType
        {
            get => _shiftType;
            set
            {
                if (_shiftType != value)
                {
                    _shiftType = value;
                    OnPropertyChanged(nameof(ShiftType));
                }
            }
        }

        public string Operator
        {
            get => _operator;
            set
            {
                if (_operator != value)
                {
                    _operator = value;
                    OnPropertyChanged(nameof(Operator));
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 