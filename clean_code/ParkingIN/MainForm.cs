using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Configuration;
using ParkingIN.Utils;

namespace ParkingIN
{
    public partial class MainForm : Form
    {
        private readonly string _stationName;
        private SerialPort _controllerPort;
        private SerialPort _printerPort;
        private NotifyIcon _trayIcon;
        private AppConfig _config;
        private Button _btnTestConnection;

        public bool IsMinimized { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            _stationName = ConfigurationManager.AppSettings["ApplicationTitle"] ?? "Modern Parking System";
            
            // Initialize configuration
            _config = new AppConfig();

            // Setup tray icon
            SetupTrayIcon();

            // Setup minimal UI
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.ShowInTaskbar = true;
            this.Size = new Size(400, 400);
            this.Text = $"ParkingIN - {_stationName}";

            // Initialize ports
            InitializePorts();

            // Handle form closing
            this.FormClosing += (s, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    MinimizeToTray();
                }
            };

            // Start automatic operation
            StartAutoOperation();
        }

        private void SetupTrayIcon()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = this.Icon,
                Text = "ParkingIN System",
                Visible = true
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, (s, e) => 
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                IsMinimized = false;
            });
            contextMenu.Items.Add("View Logs", null, (s, e) => 
            {
                var logViewer = new LogViewerForm();
                logViewer.Show();
            });
            contextMenu.Items.Add("Exit", null, (s, e) => 
            {
                if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    _trayIcon.Visible = false;
                    Application.Exit();
                }
            });

            _trayIcon.ContextMenuStrip = contextMenu;
            _trayIcon.DoubleClick += (s, e) =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                IsMinimized = false;
            };
        }

        private void MinimizeToTray()
        {
            this.Hide();
            IsMinimized = true;
            _trayIcon.ShowBalloonTip(3000, "ParkingIN System",
                "Application is still running in the background.", ToolTipIcon.Info);
        }

        private void InitializePorts()
        {
            try
            {
                // Get port settings from configuration
                string controllerPort = ConfigurationManager.AppSettings["ControllerPort"] ?? "COM1";
                string printerPort = ConfigurationManager.AppSettings["PrinterPort"] ?? "COM2";
                
                try
                {
                    int controllerBaudRate = int.Parse(ConfigurationManager.AppSettings["ControllerBaudRate"] ?? "9600");
                    int printerBaudRate = int.Parse(ConfigurationManager.AppSettings["PrinterBaudRate"] ?? "9600");
                    
                    // Initialize controller port
                    _controllerPort = new SerialPort
                    {
                        PortName = controllerPort,
                        BaudRate = controllerBaudRate,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        ReadTimeout = 1000,
                        WriteTimeout = 1000
                    };
                    _controllerPort.DataReceived += ControllerPort_DataReceived;
    
                    // Initialize printer port
                    _printerPort = new SerialPort
                    {
                        PortName = printerPort,
                        BaudRate = printerBaudRate,
                        DataBits = 8,
                        Parity = Parity.None,
                        StopBits = StopBits.One
                    };
                }
                catch (Exception configEx)
                {
                    // Log configuration error but continue
                    LogHelper.LogError("MainForm.InitializePorts", configEx);
                    MessageBox.Show($"Error in port configuration: {configEx.Message}\nThe application will continue but some features may not work.", 
                        "Configuration Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Try to open ports in a safe way
                try
                {
                    if (_controllerPort != null && System.IO.Ports.SerialPort.GetPortNames().Contains(_controllerPort.PortName))
                    {
                        _controllerPort.Open();
                        Console.WriteLine($"Controller port {_controllerPort.PortName} opened successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Controller port {_controllerPort?.PortName} not available");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not open controller port: {ex.Message}");
                }

                try
                {
                    if (_printerPort != null && System.IO.Ports.SerialPort.GetPortNames().Contains(_printerPort.PortName))
                    {
                        _printerPort.Open();
                        Console.WriteLine($"Printer port {_printerPort.PortName} opened successfully");
                    }
                    else
                    {
                        Console.WriteLine($"Printer port {_printerPort?.PortName} not available");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not open printer port: {ex.Message}");
                }

                // Add a dashboard panel to make the form look better
                CreateDashboard();
                
                // Update the status UI
                UpdateStatus();
            }
            catch (Exception ex)
            {
                LogHelper.LogError("MainForm.InitializePorts", ex);
                MessageBox.Show($"Error initializing ports: {ex.Message}\nThe application will continue but some features may not work.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateDashboard()
        {
            try
            {
                // Create a dashboard panel
                Panel dashboardPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(240, 240, 240),
                    Padding = new Padding(10)
                };

                // Add a title label
                Label titleLabel = new Label
                {
                    Text = "ParkingIN Dashboard",
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 64, 128),
                    AutoSize = true,
                    Location = new Point(10, 10)
                };
                dashboardPanel.Controls.Add(titleLabel);

                // Add a status panel
                Panel statusPanel = new Panel
                {
                    Width = 360,
                    Height = 100,
                    BackColor = Color.White,
                    Location = new Point(10, 50),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Add status info
                statusPanel.Controls.Add(new Label
                {
                    Text = "System Status",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 64, 128),
                    AutoSize = true,
                    Location = new Point(10, 10)
                });

                statusPanel.Controls.Add(new Label
                {
                    Text = $"Controller: {(_controllerPort?.IsOpen == true ? "Connected" : "Disconnected")}",
                    Font = new Font("Segoe UI", 10),
                    AutoSize = true,
                    Location = new Point(10, 40)
                });

                statusPanel.Controls.Add(new Label
                {
                    Text = $"Printer: {(_printerPort?.IsOpen == true ? "Connected" : "Disconnected")}",
                    Font = new Font("Segoe UI", 10),
                    AutoSize = true,
                    Location = new Point(10, 65)
                });

                dashboardPanel.Controls.Add(statusPanel);

                // Add a quick actions panel
                Panel actionsPanel = new Panel
                {
                    Width = 360,
                    Height = 150, 
                    BackColor = Color.White,
                    Location = new Point(10, 160),
                    BorderStyle = BorderStyle.FixedSingle
                };

                actionsPanel.Controls.Add(new Label
                {
                    Text = "Quick Actions",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    ForeColor = Color.FromArgb(0, 64, 128),
                    AutoSize = true,
                    Location = new Point(10, 10)
                });

                // Add quick action buttons
                Button btnViewLogs = new Button
                {
                    Text = "View System Logs",
                    Location = new Point(10, 50),
                    Size = new Size(160, 35),
                    BackColor = Color.FromArgb(0, 120, 215),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                };
                btnViewLogs.FlatAppearance.BorderSize = 0;
                btnViewLogs.Click += (s, e) => 
                {
                    var logViewer = new LogViewerForm();
                    logViewer.Show();
                };
                actionsPanel.Controls.Add(btnViewLogs);

                Button btnTestDb = new Button
                {
                    Text = "Test Database",
                    Location = new Point(180, 50),
                    Size = new Size(160, 35),
                    BackColor = Color.FromArgb(0, 150, 0),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                };
                btnTestDb.FlatAppearance.BorderSize = 0;
                btnTestDb.Click += TestConnectionButton_Click;
                actionsPanel.Controls.Add(btnTestDb);

                dashboardPanel.Controls.Add(actionsPanel);

                // Add copyright info
                Label copyrightLabel = new Label
                {
                    Text = " 2025 ParkingIN System",
                    ForeColor = Color.Gray,
                    AutoSize = true,
                    Location = new Point(10, 320)
                };
                dashboardPanel.Controls.Add(copyrightLabel);

                // Add the dashboard to the form
                this.Controls.Add(dashboardPanel);
            }
            catch (Exception ex)
            {
                LogHelper.LogError("MainForm.CreateDashboard", ex);
                Console.WriteLine($"Error creating dashboard: {ex.Message}");
            }
        }

        private void UpdateStatus()
        {
            // Update status labels
            foreach (Control c in this.Controls)
            {
                if (c is Panel panel)
                {
                    foreach (Control control in panel.Controls)
                    {
                        if (control is Label lbl)
                        {
                            if (lbl.Text.StartsWith("Controller:"))
                                lbl.Text = $"Controller: {(_controllerPort?.IsOpen == true ? "Connected" : "Disconnected")}";
                            else if (lbl.Text.StartsWith("Printer:"))
                                lbl.Text = $"Printer: {(_printerPort?.IsOpen == true ? "Connected" : "Disconnected")}";
                        }
                    }
                }
            }
        }

        private void StartAutoOperation()
        {
            // Start monitoring in background
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        // Check ports - only if they exist and were previously opened
                        if (_controllerPort != null && _controllerPort.PortName != null)
                        {
                            try
                            {
                                if (!_controllerPort.IsOpen)
                                    _controllerPort.Open();
                            }
                            catch (Exception portEx)
                            {
                                // Log error but continue
                                Console.WriteLine($"Could not open controller port: {portEx.Message}");
                            }
                        }

                        if (_printerPort != null && _printerPort.PortName != null)
                        {
                            try
                            {
                                if (!_printerPort.IsOpen)
                                    _printerPort.Open();
                            }
                            catch (Exception portEx)
                            {
                                // Log error but continue
                                Console.WriteLine($"Could not open printer port: {portEx.Message}");
                            }
                        }

                        // Update UI on the UI thread
                        this.Invoke(new Action(() => {
                            UpdateStatus();
                            LogHelper.LogSystemAction("HEARTBEAT", "System is running");
                        }));

                        await Task.Delay(5000); // Check every 5 seconds instead of every second
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        Console.WriteLine($"Error in auto operation: {ex.Message}");
                        LogHelper.LogError("MainForm.StartAutoOperation", ex);
                        await Task.Delay(10000); // Wait longer on error - 10 seconds
                    }
                }
            });
        }

        private void ControllerPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                {
                    string data = _controllerPort.ReadLine().Trim();
                    ProcessIncomingData(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data: {ex.Message}");
            }
        }

        private void ProcessIncomingData(string data)
        {
            try
            {
                // Update last activity
                this.Invoke(new Action(() =>
                {
                    foreach (Control c in this.Controls)
                    {
                        if (c is Panel panel)
                        {
                            foreach (Control control in panel.Controls)
                            {
                                if (control is Label lbl && lbl.Text.StartsWith("Last Activity:"))
                                {
                                    lbl.Text = $"Last Activity: {DateTime.Now:HH:mm:ss} - {data}";
                                    break;
                                }
                            }
                        }
                    }
                }));

                // Process data format: "IN:VEHICLEID"
                if (data.StartsWith("IN:"))
                {
                    string vehicleId = data.Substring(3);
                    HandleVehicleEntry(vehicleId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing data: {ex.Message}");
            }
        }

        private void HandleVehicleEntry(string vehicleId)
        {
            try
            {
                // TODO: Save to database
                
                // Print ticket if configured
                if (_config != null && _config.EntrySettings != null && _config.EntrySettings.AutoPrint)
                {
                    PrintTicket(vehicleId);
                }

                // Send acknowledgment
                if (_controllerPort != null && _controllerPort.IsOpen)
                {
                    _controllerPort.WriteLine($"ACK:{vehicleId}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling vehicle entry: {ex.Message}");
            }
        }
        
        private void PrintTicket(string vehicleId)
        {
            try
            {
                if (!_printerPort.IsOpen)
                    return;

                // Simple ticket format
                string ticket = 
                    $"\x1B@" + // Initialize printer
                    $"\x1Ba\x01" + // Center alignment
                    $"\x1B!0" + // Normal text
                    $"{_config.StationConfig.StationName}\n" +
                    $"{_config.StationConfig.Location}\n\n" +
                    $"Date: {DateTime.Now:yyyy-MM-dd}\n" +
                    $"Time: {DateTime.Now:HH:mm:ss}\n" +
                    $"Vehicle: {vehicleId}\n" +
                    $"Type: {_config.EntrySettings.DefaultVehicleType}\n\n";

                if (_config.PrinterSettings.PrintBarcode)
                {
                    // Add barcode
                    ticket += 
                        $"\x1Dh\x50" + // Barcode height
                        $"\x1Dw\x02" + // Barcode width
                        $"\x1Dk\x45\x0C" + // Code128
                        $"{vehicleId}\x00"; // Barcode data
                }

                ticket += 
                    $"\n\n\n" + // Feed lines
                    $"\x1B@"; // Reset printer

                _printerPort.Write(ticket);

                if (_config.PrinterSettings.AutoCut)
                {
                    _printerPort.Write("\x1Bm"); // Partial cut
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error printing ticket: {ex.Message}");
            }
        }

        private void TestConnectionButton_Click(object sender, EventArgs e)
        {
            ConnectionTest.TestDatabaseConnection();
        }
    }
}