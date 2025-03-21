using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ParkingIN
{
    /// <summary>
    /// Form contoh untuk mendemonstrasikan penggunaan AVRController
    /// </summary>
    public partial class MicrocontrollerFormExample : Form
    {
        // Controller untuk mikrokontroler
        private AVRController avrController;
        
        // UI elements
        private Panel panelStatus;
        private Label lblStatus;
        private ComboBox cmbPorts;
        private Button btnConnect;
        private Button btnDisconnect;
        private Button btnOpenGate;
        private Button btnCloseGate;
        private ListBox lstLog;
        private Label lblVehicle;
        private Label lblGate;
        
        // Status indikator
        private bool isConnected = false;
        private bool vehicleDetected = false;
        private AVRController.GateStatus gateStatus = AVRController.GateStatus.Unknown;
        
        // Log writer
        private StreamWriter logWriter;
        private string logFilePath;

        public MicrocontrollerFormExample()
        {
            InitializeComponent();
            InitializeUI();
            
            // Inisialisasi controller
            avrController = new AVRController(this);
            RegisterEventHandlers();
            
            // Inisialisasi log
            PrepareLogFile();
        }
        
        /// <summary>
        /// Inisialisasi UI
        /// </summary>
        private void InitializeUI()
        {
            this.Text = "Mikrokontroler ATMEL RS232 Demo";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Panel status
            panelStatus = new Panel();
            panelStatus.Dock = DockStyle.Top;
            panelStatus.Height = 100;
            panelStatus.BorderStyle = BorderStyle.FixedSingle;
            panelStatus.Padding = new Padding(10);
            this.Controls.Add(panelStatus);
            
            // Label port
            Label lblPort = new Label();
            lblPort.Text = "Port COM:";
            lblPort.Location = new Point(20, 20);
            lblPort.AutoSize = true;
            panelStatus.Controls.Add(lblPort);
            
            // Combo port
            cmbPorts = new ComboBox();
            cmbPorts.Location = new Point(100, 17);
            cmbPorts.Width = 120;
            cmbPorts.DropDownStyle = ComboBoxStyle.DropDownList;
            FillPortsCombo();
            panelStatus.Controls.Add(cmbPorts);
            
            // Button Connect
            btnConnect = new Button();
            btnConnect.Text = "Connect";
            btnConnect.Location = new Point(240, 16);
            btnConnect.Width = 80;
            btnConnect.Click += BtnConnect_Click;
            panelStatus.Controls.Add(btnConnect);
            
            // Button Disconnect
            btnDisconnect = new Button();
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.Location = new Point(330, 16);
            btnDisconnect.Width = 80;
            btnDisconnect.Enabled = false;
            btnDisconnect.Click += BtnDisconnect_Click;
            panelStatus.Controls.Add(btnDisconnect);
            
            // Status connection
            lblStatus = new Label();
            lblStatus.Text = "Disconnected";
            lblStatus.ForeColor = Color.Red;
            lblStatus.Location = new Point(420, 20);
            lblStatus.AutoSize = true;
            panelStatus.Controls.Add(lblStatus);
            
            // Button Open Gate
            btnOpenGate = new Button();
            btnOpenGate.Text = "Open Gate";
            btnOpenGate.Location = new Point(100, 60);
            btnOpenGate.Width = 100;
            btnOpenGate.Enabled = false;
            btnOpenGate.Click += BtnOpenGate_Click;
            panelStatus.Controls.Add(btnOpenGate);
            
            // Button Close Gate
            btnCloseGate = new Button();
            btnCloseGate.Text = "Close Gate";
            btnCloseGate.Location = new Point(210, 60);
            btnCloseGate.Width = 100;
            btnCloseGate.Enabled = false;
            btnCloseGate.Click += BtnCloseGate_Click;
            panelStatus.Controls.Add(btnCloseGate);
            
            // Vehicle status
            lblVehicle = new Label();
            lblVehicle.Text = "Vehicle: Not detected";
            lblVehicle.Location = new Point(320, 63);
            lblVehicle.AutoSize = true;
            panelStatus.Controls.Add(lblVehicle);
            
            // Gate status
            lblGate = new Label();
            lblGate.Text = "Gate: Unknown";
            lblGate.Location = new Point(450, 63);
            lblGate.AutoSize = true;
            panelStatus.Controls.Add(lblGate);
            
            // Log listbox
            lstLog = new ListBox();
            lstLog.Dock = DockStyle.Fill;
            lstLog.Font = new Font("Consolas", 9F);
            lstLog.HorizontalScrollbar = true;
            this.Controls.Add(lstLog);
            
            // Refresh button
            Button btnRefresh = new Button();
            btnRefresh.Text = "Refresh Ports";
            btnRefresh.Location = new Point(20, 60);
            btnRefresh.Width = 70;
            btnRefresh.Click += (s, e) => { FillPortsCombo(); };
            panelStatus.Controls.Add(btnRefresh);
            
            // Form closing
            this.FormClosing += MicrocontrollerFormExample_FormClosing;
        }
        
        /// <summary>
        /// Register event handlers untuk AVRController
        /// </summary>
        private void RegisterEventHandlers()
        {
            avrController.ConnectionStatusChanged += (s, e) => {
                if (InvokeRequired)
                    Invoke(new Action(() => AvrController_ConnectionStatusChanged(s, e)));
                else
                    AvrController_ConnectionStatusChanged(s, e);
            };
            
            avrController.VehicleDetectionChanged += (s, e) => {
                if (InvokeRequired)
                    Invoke(new Action(() => AvrController_VehicleDetectionChanged(s, e)));
                else
                    AvrController_VehicleDetectionChanged(s, e);
            };
            
            avrController.GateStatusChanged += (s, e) => {
                if (InvokeRequired)
                    Invoke(new Action(() => AvrController_GateStatusChanged(s, e)));
                else
                    AvrController_GateStatusChanged(s, e);
            };
            
            avrController.LogMessage += (s, e) => {
                if (InvokeRequired)
                    Invoke(new Action(() => AvrController_LogMessage(s, e)));
                else
                    AvrController_LogMessage(s, e);
            };
        }
        
        /// <summary>
        /// Isi combo box dengan port yang tersedia
        /// </summary>
        private void FillPortsCombo()
        {
            cmbPorts.Items.Clear();
            string[] ports = System.IO.Ports.SerialPort.GetPortNames();
            cmbPorts.Items.AddRange(ports);
            if (cmbPorts.Items.Count > 0)
            {
                cmbPorts.SelectedIndex = 0;
            }
        }
        
        /// <summary>
        /// Siapkan file log
        /// </summary>
        private void PrepareLogFile()
        {
            try
            {
                string logDir = "logs";
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                logFilePath = Path.Combine(logDir, $"microcontroller_{DateTime.Now:yyyyMMdd}.log");
                logWriter = new StreamWriter(logFilePath, true);
                LogToFile("=== Session started at " + DateTime.Now.ToString() + " ===");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating log file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        /// <summary>
        /// Event handler untuk button Connect
        /// </summary>
        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (cmbPorts.SelectedItem == null)
            {
                MessageBox.Show("Please select a COM port first", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            string portName = cmbPorts.SelectedItem.ToString();
            
            // Inisialisasi controller
            if (avrController.Initialize(portName))
            {
                // Connect ke mikrokontroler
                if (avrController.Connect())
                {
                    UpdateConnectionUI(true);
                    LogToUI("Connected to " + portName, Color.Green);
                }
                else
                {
                    LogToUI("Failed to connect to " + portName, Color.Red);
                }
            }
            else
            {
                LogToUI("Failed to initialize controller for " + portName, Color.Red);
            }
        }
        
        /// <summary>
        /// Event handler untuk button Disconnect
        /// </summary>
        private void BtnDisconnect_Click(object sender, EventArgs e)
        {
            avrController.Disconnect();
            UpdateConnectionUI(false);
            LogToUI("Disconnected", Color.Orange);
        }
        
        /// <summary>
        /// Event handler untuk button Open Gate
        /// </summary>
        private void BtnOpenGate_Click(object sender, EventArgs e)
        {
            try
            {
                avrController.OpenGate();
                LogToUI("Open gate command sent", Color.Blue);
                
                // Simulate vehicle entry and print ticket
                SimulateVehicleEntry();
            }
            catch (Exception ex)
            {
                LogToUI($"Error sending open gate command: {ex.Message}", Color.Red);
            }
        }
        
        /// <summary>
        /// Event handler untuk button Close Gate
        /// </summary>
        private void BtnCloseGate_Click(object sender, EventArgs e)
        {
            avrController.CloseGate();
        }
        
        /// <summary>
        /// Event handler untuk form closing
        /// </summary>
        private void MicrocontrollerFormExample_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isConnected)
            {
                avrController.Disconnect();
            }
            
            if (logWriter != null)
            {
                LogToFile("=== Session ended at " + DateTime.Now.ToString() + " ===");
                logWriter.Close();
                logWriter = null;
            }
        }
        
        /// <summary>
        /// Event handler untuk perubahan status koneksi
        /// </summary>
        private void AvrController_ConnectionStatusChanged(object sender, ConnectionStatusEventArgs e)
        {
            UpdateConnectionUI(e.IsConnected);
        }
        
        /// <summary>
        /// Event handler untuk perubahan deteksi kendaraan
        /// </summary>
        private void AvrController_VehicleDetectionChanged(object sender, VehicleDetectionEventArgs e)
        {
            vehicleDetected = e.VehicleDetected;
            lblVehicle.Text = "Vehicle: " + (vehicleDetected ? "Detected" : "Not detected");
            lblVehicle.ForeColor = vehicleDetected ? Color.Green : Color.Black;
            
            LogToUI("Vehicle " + (vehicleDetected ? "detected" : "left"), vehicleDetected ? Color.Green : Color.Blue);
        }
        
        /// <summary>
        /// Event handler untuk perubahan status gate
        /// </summary>
        private void AvrController_GateStatusChanged(object sender, GateStatusEventArgs e)
        {
            gateStatus = e.Status;
            lblGate.Text = "Gate: " + gateStatus.ToString();
            
            // Set warna berdasarkan status
            switch (gateStatus)
            {
                case AVRController.GateStatus.Open:
                    lblGate.ForeColor = Color.Green;
                    break;
                case AVRController.GateStatus.Closed:
                    lblGate.ForeColor = Color.Black;
                    break;
                case AVRController.GateStatus.Opening:
                case AVRController.GateStatus.Closing:
                    lblGate.ForeColor = Color.Blue;
                    break;
                case AVRController.GateStatus.Error:
                    lblGate.ForeColor = Color.Red;
                    break;
                default:
                    lblGate.ForeColor = Color.Gray;
                    break;
            }
            
            LogToUI("Gate status changed to: " + gateStatus.ToString(), lblGate.ForeColor);
        }
        
        /// <summary>
        /// Event handler untuk log message
        /// </summary>
        private void AvrController_LogMessage(object sender, LogMessageEventArgs e)
        {
            // Set warna berdasarkan level
            Color color = Color.Black;
            switch (e.Level)
            {
                case LogLevel.Debug:
                    color = Color.Gray;
                    break;
                case LogLevel.Info:
                    color = Color.Black;
                    break;
                case LogLevel.Warning:
                    color = Color.Orange;
                    break;
                case LogLevel.Error:
                    color = Color.Red;
                    break;
            }
            
            LogToUI(e.Message, color);
        }
        
        /// <summary>
        /// Update UI berdasarkan status koneksi
        /// </summary>
        private void UpdateConnectionUI(bool connected)
        {
            isConnected = connected;
            btnConnect.Enabled = !connected;
            btnDisconnect.Enabled = connected;
            btnOpenGate.Enabled = connected;
            btnCloseGate.Enabled = connected;
            cmbPorts.Enabled = !connected;
            
            lblStatus.Text = connected ? "Connected" : "Disconnected";
            lblStatus.ForeColor = connected ? Color.Green : Color.Red;
        }
        
        /// <summary>
        /// Log ke UI listbox
        /// </summary>
        private void LogToUI(string message, Color color)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            string logMessage = $"[{timestamp}] {message}";
            
            // Thread-safe add ke listbox
            if (lstLog.InvokeRequired)
            {
                lstLog.Invoke(new Action(() => {
                    AddLogToList(logMessage, color);
                }));
            }
            else
            {
                AddLogToList(logMessage, color);
            }
            
            // Log ke file
            LogToFile(logMessage);
        }
        
        /// <summary>
        /// Helper untuk menambahkan log ke listbox
        /// </summary>
        private void AddLogToList(string message, Color color)
        {
            int index = lstLog.Items.Add(message);
            
            // Simpan warna di tag
            if (lstLog.Items.Count > 0)
            {
                lstLog.Items[index] = message;
            }
            
            // Scroll ke item terakhir
            lstLog.SelectedIndex = lstLog.Items.Count - 1;
            lstLog.ClearSelected();
        }
        
        /// <summary>
        /// Log ke file
        /// </summary>
        private void LogToFile(string message)
        {
            if (logWriter != null)
            {
                try
                {
                    logWriter.WriteLine(message);
                    logWriter.Flush();
                }
                catch (Exception)
                {
                    // Ignore error
                }
            }
        }

        private void SimulateVehicleEntry()
        {
            try
            {
                // Generate random vehicle ID
                string vehicleId = $"SIM{new Random().Next(10000, 99999)}";
                
                LogToUI($"Simulating vehicle entry: {vehicleId}", Color.Green);
                
                // Check if printer settings exist
                string configDir = Path.Combine(Application.StartupPath, "config");
                string printerConfigPath = Path.Combine(configDir, "printer.ini");
                
                if (!File.Exists(printerConfigPath))
                {
                    LogToUI("Printer configuration not found. Creating default configuration.", Color.Orange);
                    
                    if (!Directory.Exists(configDir))
                        Directory.CreateDirectory(configDir);
                        
                    // Create default printer settings
                    File.WriteAllText(printerConfigPath, 
                        "[Printer]\r\n" +
                        "Name=EPSON TM-T82X\r\n" +
                        "Port=USB001\r\n" +
                        "Paper_Width=80\r\n" +
                        "DPI=180\r\n" +
                        "Header=TIKET PARKIR\r\n" +
                        "Footer=Terima Kasih\r\n" +
                        "Show_Logo=false\r\n" +
                        "QR_Code=true\r\n" +
                        "Auto_Cut=true\r\n" +
                        "Stop_Bits=1\r\n");
                }
                
                // Load printer settings
                var settings = LoadPrinterSettings(printerConfigPath);
                
                // Print ticket
                PrintSimulatedTicket(vehicleId, settings);
            }
            catch (Exception ex)
            {
                LogToUI($"Error simulating vehicle entry: {ex.Message}", Color.Red);
            }
        }

        private Dictionary<string, string> LoadPrinterSettings(string configPath)
        {
            var settings = new Dictionary<string, string>();
            
            try
            {
                if (File.Exists(configPath))
                {
                    foreach (string line in File.ReadAllLines(configPath))
                    {
                        string trimmedLine = line.Trim();
                        
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("["))
                            continue;
                        
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length == 2)
                        {
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();
                            settings[key] = value;
                        }
                    }
                }
                
                LogToUI("Printer settings loaded successfully", Color.Green);
            }
            catch (Exception ex)
            {
                LogToUI($"Error loading printer settings: {ex.Message}", Color.Red);
            }
            
            return settings;
        }

        private void PrintSimulatedTicket(string vehicleId, Dictionary<string, string> settings)
        {
            try
            {
                LogToUI("Printing simulated ticket...", Color.Blue);
                
                // Display ticket preview in message box
                string ticketPreview = 
                    $"===============================\r\n" +
                    $"         TIKET PARKIR          \r\n" +
                    $"===============================\r\n" +
                    $"Date: {DateTime.Now:yyyy-MM-dd}\r\n" +
                    $"Time: {DateTime.Now:HH:mm:ss}\r\n" +
                    $"ID  : {vehicleId}\r\n\r\n";
                    
                if (settings.TryGetValue("QR_Code", out string qrCode) && qrCode.ToLower() == "true")
                {
                    ticketPreview += "[BARCODE: " + vehicleId + "]\r\n\r\n";
                }
                
                if (settings.TryGetValue("Footer", out string footer))
                {
                    ticketPreview += footer + "\r\n";
                }
                
                ticketPreview += "===============================\r\n";
                
                LogToUI("Ticket printed successfully", Color.Green);
                MessageBox.Show(ticketPreview, "Simulated Ticket", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogToUI($"Error printing simulated ticket: {ex.Message}", Color.Red);
            }
        }
    }
}