using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using NLog;
using ParkingOut.Models;
using ParkingOut.Services;
using ParkingOut.Utils;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for VehicleEntryPage.xaml
    /// </summary>
    public partial class VehicleEntryPage : Page
    {
        #region Fields

        private IAppLogger _logger;
        private readonly IVehicleEntryService vehicleEntryService;
        private readonly IPrintService printService;
        private readonly ICameraService? cameraService;

        private ObservableCollection<VehicleEntry> _recentEntries = new ObservableCollection<VehicleEntry>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="VehicleEntryPage"/> class.
        /// </summary>
        public VehicleEntryPage(IAppLogger logger)
        {
            InitializeComponent();

            // Initialize services
            _logger = logger;
            vehicleEntryService = ServiceLocator.GetService<IVehicleEntryService>();
            printService = ServiceLocator.GetService<IPrintService>();
            try
            {
                cameraService = ServiceLocator.GetService<ICameraService>();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to initialize camera service");
                cameraService = null;
            }

            // Set data context
            RecentEntriesDataGrid.ItemsSource = _recentEntries;

            // Load recent entries
            LoadRecentEntries();

            // Set current date and time
            SetCurrentDateTime();

            // Update status
            UpdateStatus("Ready");

            _logger.Debug("VehicleEntryPage initialized");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads recent vehicle entries.
        /// </summary>
        private void LoadRecentEntries()
        {
            try
            {
                UpdateStatus("Loading recent entries...");
                
                _recentEntries.Clear();
                var entries = vehicleEntryService.GetRecentEntries(20);
                
                foreach (var entry in entries)
                {
                    _recentEntries.Add(entry);
                }
                
                UpdateStatus($"Loaded {entries.Count.ToString()} recent entries");
                _logger.Debug("Loaded recent entries: {Count}", entries.Count);
            }
            catch (Exception ex)
            {
                UpdateStatus("Error loading recent entries");
                _logger.Error(ex, "Failed to load recent entries");
                MessageBox.Show($"Failed to load recent entries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Sets the current date and time in the entry time fields.
        /// </summary>
        private void SetCurrentDateTime()
        {
            try
            {
                var now = DateTime.Now;
                EntryDatePicker.SelectedDate = now.Date;
                EntryTimeHourTextBox.Text = now.Hour.ToString("00");
                EntryTimeMinuteTextBox.Text = now.Minute.ToString("00");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to set current date and time");
            }
        }

        /// <summary>
        /// Updates the status bar text.
        /// </summary>
        /// <param name="status">The status text.</param>
        private void UpdateStatus(string status)
        {
            StatusBarTextBlock.Text = status;
        }

        /// <summary>
        /// Gets the entry time from the date and time controls.
        /// </summary>
        /// <returns>The entry time.</returns>
        private DateTime GetEntryTime()
        {
            var date = EntryDatePicker.SelectedDate ?? DateTime.Now.Date;
            
            int hour = 0;
            int minute = 0;
            
            if (!int.TryParse(EntryTimeHourTextBox.Text, out hour))
            {
                hour = DateTime.Now.Hour;
            }
            
            if (!int.TryParse(EntryTimeMinuteTextBox.Text, out minute))
            {
                minute = DateTime.Now.Minute;
            }
            
            return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the StartCameraButton control.
        /// </summary>
        private void StartCameraButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cameraService == null)
                {
                    MessageBox.Show("Camera service is not available", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                UpdateStatus("Starting camera...");
                
                if (cameraService.IsRunning)
                {
                    cameraService.Stop();
                    StartCameraButton.Content = "Start Camera";
                    CameraFeedImage.Visibility = Visibility.Collapsed;
                    UpdateStatus("Camera stopped");
                }
                else
                {
                    cameraService.Start();
                    cameraService.SetImageControl(CameraFeedImage);
                    StartCameraButton.Content = "Stop Camera";
                    CameraFeedImage.Visibility = Visibility.Visible;
                    UpdateStatus("Camera started");
                }
                
                _logger.Debug("Camera {Status}", cameraService.IsRunning ? "started" : "stopped");
            }
            catch (Exception ex)
            {
                UpdateStatus("Error controlling camera");
                _logger.Error(ex, "Failed to control camera");
                MessageBox.Show($"Failed to control camera: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the CaptureButton control.
        /// </summary>
        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cameraService == null || !cameraService.IsRunning)
                {
                    MessageBox.Show("Camera is not running", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                UpdateStatus("Capturing image...");
                
                var image = cameraService.CaptureImage();
                if (image != null)
                {
                    // Here we would save the image or process it further
                    UpdateStatus("Image captured");
                    _logger.Debug("Image captured");
                }
                else
                {
                    UpdateStatus("Failed to capture image");
                    _logger.Warn("Failed to capture image");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error capturing image");
                _logger.Error(ex, "Failed to capture image");
                MessageBox.Show($"Failed to capture image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the RecognizeButton control.
        /// </summary>
        private void RecognizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cameraService == null || !cameraService.IsRunning)
                {
                    MessageBox.Show("Camera is not running", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                UpdateStatus("Recognizing license plate...");
                
                var plate = cameraService.RecognizePlate();
                if (!string.IsNullOrEmpty(plate))
                {
                    DetectedPlateTextBox.Text = plate;
                    LicensePlateTextBox.Text = plate;
                    UpdateStatus($"License plate recognized: {plate}");
                    _logger.Debug("License plate recognized: {Plate}", plate);
                }
                else
                {
                    UpdateStatus("Failed to recognize license plate");
                    _logger.Warn("Failed to recognize license plate");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error recognizing license plate");
                _logger.Error(ex, "Failed to recognize license plate");
                MessageBox.Show($"Failed to recognize license plate: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the GenerateTicketButton control.
        /// </summary>
        private void GenerateTicketButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var licensePlate = LicensePlateTextBox.Text.Trim();
                if (string.IsNullOrEmpty(licensePlate))
                {
                    MessageBox.Show("Please enter a license plate", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    LicensePlateTextBox.Focus();
                    return;
                }
                
                var vehicleType = (VehicleTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Car";
                var entryTime = GetEntryTime();
                var notes = NotesTextBox.Text.Trim();
                
                UpdateStatus("Generating ticket...");
                
                var entry = vehicleEntryService.CreateEntry(licensePlate, vehicleType, entryTime, notes);
                if (entry != null)
                {
                    _recentEntries.Insert(0, entry);
                    
                    // Print ticket
                    if (printService.PrintTicket(entry))
                    {
                        UpdateStatus($"Ticket generated and printed: {entry.TicketNo}");
                        _logger.Debug("Ticket generated and printed: {TicketNo}", entry.TicketNo);
                    }
                    else
                    {
                        UpdateStatus($"Ticket generated but printing failed: {entry.TicketNo}");
                        _logger.Warn("Ticket generated but printing failed: {TicketNo}", entry.TicketNo);
                    }
                    
                    // Clear form
                    LicensePlateTextBox.Text = string.Empty;
                    VehicleTypeComboBox.SelectedIndex = 0;
                    NotesTextBox.Text = string.Empty;
                    SetCurrentDateTime();
                    
                    MessageBox.Show($"Vehicle entry recorded. Ticket: {entry.TicketNo}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    UpdateStatus("Failed to generate ticket");
                    _logger.Warn("Failed to generate ticket");
                    MessageBox.Show("Failed to generate ticket", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error generating ticket");
                _logger.Error(ex, "Failed to generate ticket");
                MessageBox.Show($"Failed to generate ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the PrintTicket button.
        /// </summary>
        private void PrintTicket_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var ticketNo = button?.Tag.ToString();
                
                if (string.IsNullOrEmpty(ticketNo))
                {
                    _logger.Warn("Print ticket clicked with no ticket number");
                    return;
                }
                
                UpdateStatus($"Printing ticket {ticketNo}...");
                
                var entry = vehicleEntryService.GetEntryByTicketNo(ticketNo);
                if (entry != null)
                {
                    if (printService.PrintTicket(entry))
                    {
                        UpdateStatus($"Ticket printed: {ticketNo}");
                        _logger.Debug("Ticket printed: {TicketNo}", ticketNo);
                    }
                    else
                    {
                        UpdateStatus($"Failed to print ticket: {ticketNo}");
                        _logger.Warn("Failed to print ticket: {TicketNo}", ticketNo);
                        MessageBox.Show("Failed to print ticket", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    UpdateStatus($"Ticket not found: {ticketNo}");
                    _logger.Warn("Ticket not found: {TicketNo}", ticketNo);
                    MessageBox.Show("Ticket not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error printing ticket");
                _logger.Error(ex, "Failed to print ticket");
                MessageBox.Show($"Failed to print ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the Click event of the EditEntry button.
        /// </summary>
        private void EditEntry_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var ticketNo = button?.Tag.ToString();
                
                if (string.IsNullOrEmpty(ticketNo))
                {
                    _logger.Warn("Edit entry clicked with no ticket number");
                    return;
                }
                
                UpdateStatus($"Editing entry {ticketNo}...");
                
                var entry = vehicleEntryService.GetEntryByTicketNo(ticketNo);
                if (entry != null)
                {
                    // Set form values
                    LicensePlateTextBox.Text = entry.LicensePlate;
                    
                    for (int i = 0; i < VehicleTypeComboBox.Items.Count; i++)
                    {
                        var item = VehicleTypeComboBox.Items[i] as ComboBoxItem;
                        if (item?.Content.ToString() == entry.VehicleType)
                        {
                            VehicleTypeComboBox.SelectedIndex = i;
                            break;
                        }
                    }
                    
                    EntryDatePicker.SelectedDate = entry.EntryTime.Date;
                    EntryTimeHourTextBox.Text = entry.EntryTime.Hour.ToString("00");
                    EntryTimeMinuteTextBox.Text = entry.EntryTime.Minute.ToString("00");
                    
                    NotesTextBox.Text = entry.Notes;
                    
                    UpdateStatus($"Editing entry: {ticketNo}");
                    _logger.Debug("Editing entry: {TicketNo}", ticketNo);
                }
                else
                {
                    UpdateStatus($"Entry not found: {ticketNo}");
                    _logger.Warn("Entry not found: {TicketNo}", ticketNo);
                    MessageBox.Show("Entry not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("Error editing entry");
                _logger.Error(ex, "Failed to edit entry");
                MessageBox.Show($"Failed to edit entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the RecentEntriesDataGrid control.
        /// </summary>
        private void RecentEntriesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var selectedEntry = RecentEntriesDataGrid.SelectedItem as VehicleEntry;
                if (selectedEntry != null)
                {
                    UpdateStatus($"Selected entry: {selectedEntry.TicketNo}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error in RecentEntriesDataGrid_SelectionChanged");
            }
        }

        #endregion
    }
}