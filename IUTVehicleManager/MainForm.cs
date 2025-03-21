using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;
using ZXing;
using ZXing.Windows.Compatibility;
using IUTVehicleManager.WebSocket;
using IUTVehicleManager.Forms;

namespace IUTVehicleManager
{
    public partial class MainForm : Form
    {
        private SerialPort? thermalPrinter;
        private bool isPrinterConnected = false;
        private List<VehicleRecord> vehicleHistory = new();
        private System.Windows.Forms.Timer? clockTimer;
        private Panel statusPanel = null!;
        private Label lblOccupancyStatus = null!;
        private Label lblOccupancyCount = null!;
        private Label lblLastUpdate = null!;
        private ProgressBar occupancyBar = null!;
        private int currentOccupancy = 0;
        private int maxCapacity = 100; // Default value, can be configured in settings

        private ComboBox cmbPorts = null!;
        private Button btnConnect = null!;
        private Button btnTestPrint = null!;
        private Label lblPrinterStatus = null!;
        private Label lblDateTime = null!;
        private Button btnScan = null!;
        private Button btnPrint = null!;
        private Button btnClear = null!;
        private TextBox txtVehicleInfo = null!;
        private TextBox txtPlateNumber = null!;
        private ComboBox cmbPriority = null!;
        private ComboBox cmbVehicleType = null!;
        private ListView lstHistory = null!;

        private SerialPort? mcuSerialPort;
        private bool isMcuConnected = false;

        private ComboBox cmbMcuPorts = null!;
        private Button btnMcuConnect = null!;
        private Label lblMcuStatus = null!;
        private Label lblLastTrigger = null!;

        private TabControl mainTabControl;
        private TabPage getInPage;
        private TabPage getOutPage;
        private TabPage reportPage;
        private TabPage settingsPage;

        // Add new fields for payment
        private TextBox txtPaymentAmount = null!;
        private TextBox txtPaymentBarcode = null!;
        private TextBox txtExitPlateNumber = null!;
        private Button btnScanPayment = null!;
        private Button btnProcessPayment = null!;
        private Label lblPaymentStatus = null!;
        private ComboBox cmbPaymentMethod = null!;

        private MenuStrip mainMenu = null!;
        private ToolStrip toolStrip = null!;
        
        // New fields for WebSocket server
        private WebSocketServer? _webSocketServer;
        private ILogger _logger = new ConsoleLogger(); // Temporary logger, will be replaced
        
        // New fields for advanced features
        private Button btnOpenCameraSettings = null!;
        private Button btnOpenReports = null!;

        public MainForm()
        {
            // Initialize all required fields first
            mainTabControl = new TabControl();
            getInPage = new TabPage("GET IN");
            getOutPage = new TabPage("GET OUT");
            reportPage = new TabPage("Reports & Analytics");
            settingsPage = new TabPage("Settings");
            mainMenu = new MenuStrip();
            toolStrip = new ToolStrip();
            statusPanel = new Panel();
            lblOccupancyStatus = new Label();
            lblOccupancyCount = new Label();
            lblLastUpdate = new Label();
            occupancyBar = new ProgressBar();
            lblDateTime = new Label();
            cmbPorts = new ComboBox();
            btnConnect = new Button();
            btnTestPrint = new Button();
            lblPrinterStatus = new Label();
            btnScan = new Button();
            btnPrint = new Button();
            btnClear = new Button();
            txtVehicleInfo = new TextBox();
            txtPlateNumber = new TextBox();
            cmbPriority = new ComboBox();
            cmbVehicleType = new ComboBox();
            lstHistory = new ListView();
            cmbMcuPorts = new ComboBox();
            btnMcuConnect = new Button();
            lblMcuStatus = new Label();
            lblLastTrigger = new Label();
            txtPaymentAmount = new TextBox();
            txtPaymentBarcode = new TextBox();
            txtExitPlateNumber = new TextBox();
            btnScanPayment = new Button();
            btnProcessPayment = new Button();
            lblPaymentStatus = new Label();
            cmbPaymentMethod = new ComboBox();

            // Initialize the form
            InitializeComponent();

            // Initialize other components
            InitializeMenus();
            InitializeTabControl();

            // Set form properties
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.CenterScreen;

            // Load data and start timer
            LoadComPorts();
            InitializeTimer();

            // Force form to redraw
            this.Refresh();
        }

        private void InitializeTimer()
        {
            clockTimer = new System.Windows.Forms.Timer();
            clockTimer.Interval = 1000; // Update every second
            clockTimer.Tick += (s, e) => lblDateTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            clockTimer.Start();
        }

        private void InitializeTabControl()
        {
            // Create main tab control with proper docking
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                Padding = new Point(12, 4),
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(150, 30),
                Margin = new Padding(0)
            };

            // Create GET IN page
            getInPage = new TabPage("GET IN")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };
            InitializeGetInPage(getInPage);

            // Create GET OUT page
            getOutPage = new TabPage("GET OUT")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };
            InitializeGetOutPage(getOutPage);

            // Create Report page
            reportPage = new TabPage("Reports & Analytics")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };
            InitializeReportPage(reportPage);

            // Create Settings page
            settingsPage = new TabPage("Settings")
            {
                Padding = new Padding(10),
                BackColor = Color.White
            };
            InitializeSettingsPage(settingsPage);

            // Add all pages to tab control
            mainTabControl.Controls.AddRange(new Control[] {
                getInPage,
                getOutPage,
                reportPage,
                settingsPage
            });

            // Add tab control to form
            this.Controls.Add(mainTabControl);
        }

        private void InitializeGetInPage(TabPage page)
        {
            // Create simulation button with modern styling
            Button btnSimulation = new Button
            {
                Text = "Simulation",
                Location = new Point(10, 10),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Image = null, // You can add an icon here if needed
                ImageAlign = ContentAlignment.MiddleLeft,
                TextAlign = ContentAlignment.MiddleCenter,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                Padding = new Padding(10, 0, 10, 0)
            };
            btnSimulation.FlatAppearance.BorderSize = 0;
            btnSimulation.Click += (s, e) => 
            {
                var simulationForm = new Forms.GetInSimulationForm();
                simulationForm.ShowDialog();
            };

            // Create a container panel for better organization
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 55,
                Padding = new Padding(10),
                BackColor = Color.FromArgb(248, 248, 248)
            };

            // Add simulation button to panel
            buttonPanel.Controls.Add(btnSimulation);

            // Add panel to page
            page.Controls.Add(buttonPanel);

            // Move existing GET IN controls to this page
            foreach (Control control in this.Controls.Cast<Control>().ToList())
            {
                if (control != mainTabControl)
                {
                    this.Controls.Remove(control);
                    page.Controls.Add(control);
                }
            }
        }

        private void InitializeGetOutPage(TabPage page)
        {
            // Create printer configuration group
            var printerGroup = new GroupBox
            {
                Text = "Printer Configuration",
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(10)
            };

            var outCmbPorts = new ComboBox
            {
                Location = new Point(15, 25),
                Size = new Size(150, 25),
                Font = new Font("Segoe UI", 9F)
            };
            outCmbPorts.Items.AddRange(SerialPort.GetPortNames());
            if (outCmbPorts.Items.Count > 0)
                outCmbPorts.SelectedIndex = 0;

            var outBtnConnect = new Button
            {
                Text = "Connect",
                Location = new Point(175, 25),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F)
            };

            var outLblStatus = new Label
            {
                Text = "Status: Not Connected",
                Location = new Point(15, 60),
                Size = new Size(200, 20),
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9F)
            };

            printerGroup.Controls.AddRange(new Control[] { outCmbPorts, outBtnConnect, outLblStatus });

            // Create vehicle and payment information group
            var vehicleGroup = new GroupBox
            {
                Text = "Vehicle & Payment Information",
                Dock = DockStyle.Top,
                Height = 350,
                Top = printerGroup.Bottom + 10,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(10)
            };

            // Vehicle info controls
            txtExitPlateNumber = new TextBox
            {
                Location = new Point(130, 30),
                Size = new Size(250, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            var lblExitPlate = new Label
            {
                Text = "Plate Number:",
                Location = new Point(15, 33),
                Size = new Size(110, 20),
                Font = new Font("Segoe UI", 9F)
            };

            // Payment controls
            txtPaymentBarcode = new TextBox
            {
                Location = new Point(130, 70),
                Size = new Size(250, 25),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9.5F)
            };
            var lblPaymentBarcode = new Label
            {
                Text = "Payment Code:",
                Location = new Point(15, 73),
                Size = new Size(110, 20),
                Font = new Font("Segoe UI", 9F)
            };

            btnScanPayment = new Button
            {
                Text = "Scan Payment Code",
                Location = new Point(390, 70),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9F)
            };
            btnScanPayment.Click += BtnScanPayment_Click;

            txtPaymentAmount = new TextBox
            {
                Location = new Point(130, 110),
                Size = new Size(250, 25),
                ReadOnly = true,
                Font = new Font("Segoe UI", 9.5F)
            };
            var lblAmount = new Label
            {
                Text = "Amount:",
                Location = new Point(15, 113),
                Size = new Size(110, 20),
                Font = new Font("Segoe UI", 9F)
            };

            cmbPaymentMethod = new ComboBox
            {
                Location = new Point(130, 150),
                Size = new Size(250, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9.5F)
            };
            cmbPaymentMethod.Items.AddRange(new string[] { "Cash", "Credit Card", "Debit Card", "E-Wallet" });
            cmbPaymentMethod.SelectedIndex = 0;
            var lblPaymentMethod = new Label
            {
                Text = "Payment Method:",
                Location = new Point(15, 153),
                Size = new Size(110, 20),
                Font = new Font("Segoe UI", 9F)
            };

            btnProcessPayment = new Button
            {
                Text = "Process Payment",
                Location = new Point(130, 190),
                Size = new Size(250, 35),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnProcessPayment.Click += BtnProcessPayment_Click;

            lblPaymentStatus = new Label
            {
                Text = "Payment Status: Waiting for payment",
                Location = new Point(130, 235),
                Size = new Size(250, 20),
                ForeColor = Color.Orange,
                Font = new Font("Segoe UI", 9F)
            };

            vehicleGroup.Controls.AddRange(new Control[] {
                lblExitPlate, txtExitPlateNumber,
                lblPaymentBarcode, txtPaymentBarcode, btnScanPayment,
                lblAmount, txtPaymentAmount,
                lblPaymentMethod, cmbPaymentMethod,
                btnProcessPayment, lblPaymentStatus
            });

            // Create history group
            var historyGroup = new GroupBox
            {
                Text = "Exit History",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(10)
            };

            var exitHistory = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            exitHistory.Columns.AddRange(new ColumnHeader[]
            {
                new ColumnHeader { Text = "Time", Width = 150 },
                new ColumnHeader { Text = "Plate", Width = 120 },
                new ColumnHeader { Text = "Duration", Width = 100 },
                new ColumnHeader { Text = "Amount", Width = 120 },
                new ColumnHeader { Text = "Payment Method", Width = 120 },
                new ColumnHeader { Text = "Status", Width = 100 }
            });

            historyGroup.Controls.Add(exitHistory);

            page.Controls.Add(historyGroup);
            page.Controls.Add(vehicleGroup);
            page.Controls.Add(printerGroup);
        }

        private void InitializeReportPage(TabPage page)
        {
            var reportPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(15)
            };

            // Date Range Group
            var dateRangeGroup = new GroupBox
            {
                Text = "Date Range",
                Dock = DockStyle.Top,
                Height = 100,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(15)
            };

            var lblStartDate = new Label
            {
                Text = "Start Date:",
                Location = new Point(20, 35),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var startDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(120, 35),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };

            var lblEndDate = new Label
            {
                Text = "End Date:",
                Location = new Point(340, 35),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var endDate = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(440, 35),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };

            var generateBtn = new Button
            {
                Text = "Generate Report",
                Location = new Point(660, 35),
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            dateRangeGroup.Controls.AddRange(new Control[] { 
                lblStartDate, startDate,
                lblEndDate, endDate,
                generateBtn
            });

            // Report Filter Group
            var filterGroup = new GroupBox
            {
                Text = "Report Filters",
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 9F),
                Padding = new Padding(15),
                Top = dateRangeGroup.Bottom + 10
            };

            var lblVehicleType = new Label
            {
                Text = "Vehicle Type:",
                Location = new Point(20, 35),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var cmbVehicleType = new ComboBox
            {
                Location = new Point(120, 35),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbVehicleType.Items.AddRange(new string[] { "All", "Car", "Motorcycle", "Truck", "Bus", "Other" });
            cmbVehicleType.SelectedIndex = 0;

            var lblPriority = new Label
            {
                Text = "Priority:",
                Location = new Point(340, 35),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9F),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var cmbPriority = new ComboBox
            {
                Location = new Point(440, 35),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPriority.Items.AddRange(new string[] { "All", "High", "Medium", "Low" });
            cmbPriority.SelectedIndex = 0;

            var chkIncludePayment = new CheckBox
            {
                Text = "Include Payment Details",
                Location = new Point(120, 70),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F),
                Checked = true
            };

            filterGroup.Controls.AddRange(new Control[] {
                lblVehicleType, cmbVehicleType,
                lblPriority, cmbPriority,
                chkIncludePayment
            });

            // Report Data Grid
            var reportGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 9F),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToResizeRows = false,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(240, 240, 240) }
            };

            // Add columns
            reportGrid.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { HeaderText = "Date/Time", Width = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "Plate Number", Width = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "Vehicle Type", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Priority", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Duration", Width = 80 },
                new DataGridViewTextBoxColumn { HeaderText = "Amount", Width = 100 },
                new DataGridViewTextBoxColumn { HeaderText = "Status", Width = 80 }
            });

            // Button Panel
            var buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(0, 8, 0, 8)
            };

            var btnExportExcel = new Button
            {
                Text = "Export to Excel",
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Margin = new Padding(10, 0, 0, 0)
            };

            var btnExportPdf = new Button
            {
                Text = "Export to PDF",
                Size = new Size(150, 35),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Right,
                Margin = new Padding(10, 0, 10, 0)
            };

            buttonPanel.Controls.AddRange(new Control[] { btnExportExcel, btnExportPdf });

            // Add all controls to panel
            reportPanel.Controls.Add(buttonPanel);
            reportPanel.Controls.Add(reportGrid);
            reportPanel.Controls.Add(filterGroup);
            reportPanel.Controls.Add(dateRangeGroup);

            // Add panel to page
            page.Controls.Add(reportPanel);

            // Add open reports button
            btnOpenReports = new Button
            {
                Text = "Laporan Detail",
                Location = new Point(20, 450),
                Size = new Size(180, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnOpenReports.Click += BtnOpenReports_Click;
            page.Controls.Add(btnOpenReports);
        }

        private void InitializeSettingsPage(TabPage page)
        {
            var settingsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(10)
            };

            // Printer Settings Group
            var printerSettings = new GroupBox
            {
                Text = "Printer Settings",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(15),
                Font = new Font("Segoe UI", 9F)
            };

            var printerPort = new ComboBox
            {
                Location = new Point(150, 30),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            printerPort.Items.AddRange(SerialPort.GetPortNames());

            var lblPrinterPort = new Label
            {
                Text = "Default COM Port:",
                Location = new Point(20, 33),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var baudRate = new ComboBox
            {
                Location = new Point(150, 65),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            baudRate.Items.AddRange(new string[] { "9600", "19200", "38400", "57600", "115200" });
            baudRate.SelectedIndex = 0;

            var lblBaudRate = new Label
            {
                Text = "Baud Rate:",
                Location = new Point(20, 68),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var chkAutoCut = new CheckBox
            {
                Text = "Auto Cut Paper",
                Location = new Point(150, 100),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F),
                Checked = true
            };

            var btnSavePrinter = new Button
            {
                Text = "Save Printer Settings",
                Location = new Point(150, 140),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            printerSettings.Controls.AddRange(new Control[] {
                lblPrinterPort, printerPort,
                lblBaudRate, baudRate,
                chkAutoCut, btnSavePrinter
            });

            // MCU Settings Group
            var mcuSettings = new GroupBox
            {
                Text = "Microcontroller Settings",
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(15),
                Font = new Font("Segoe UI", 9F),
                Top = printerSettings.Bottom + 10
            };

            var mcuPort = new ComboBox
            {
                Location = new Point(150, 30),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            mcuPort.Items.AddRange(SerialPort.GetPortNames());

            var lblMcuPort = new Label
            {
                Text = "Default COM Port:",
                Location = new Point(20, 33),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var mcuBaudRate = new ComboBox
            {
                Location = new Point(150, 65),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            mcuBaudRate.Items.AddRange(new string[] { "9600", "19200", "38400", "57600", "115200" });
            mcuBaudRate.SelectedIndex = 0;

            var lblMcuBaudRate = new Label
            {
                Text = "Baud Rate:",
                Location = new Point(20, 68),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var timeout = new NumericUpDown
            {
                Location = new Point(150, 100),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F),
                Minimum = 100,
                Maximum = 5000,
                Value = 1000,
                Increment = 100
            };

            var lblTimeout = new Label
            {
                Text = "Timeout (ms):",
                Location = new Point(20, 103),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var btnSaveMcu = new Button
            {
                Text = "Save MCU Settings",
                Location = new Point(150, 140),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            mcuSettings.Controls.AddRange(new Control[] {
                lblMcuPort, mcuPort,
                lblMcuBaudRate, mcuBaudRate,
                lblTimeout, timeout,
                btnSaveMcu
            });

            // General Settings Group
            var generalSettings = new GroupBox
            {
                Text = "General Settings",
                Dock = DockStyle.Top,
                Height = 250,
                Padding = new Padding(15),
                Font = new Font("Segoe UI", 9F),
                Top = mcuSettings.Bottom + 10
            };

            var defaultVehicleType = new ComboBox
            {
                Location = new Point(150, 30),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            defaultVehicleType.Items.AddRange(new string[] { "Car", "Motorcycle", "Truck", "Bus", "Other" });
            defaultVehicleType.SelectedIndex = 0;

            var lblDefaultType = new Label
            {
                Text = "Default Vehicle Type:",
                Location = new Point(20, 33),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var defaultPriority = new ComboBox
            {
                Location = new Point(150, 65),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9.5F)
            };
            defaultPriority.Items.AddRange(new string[] { "Normal", "High", "VIP" });
            defaultPriority.SelectedIndex = 0;

            var lblDefaultPriority = new Label
            {
                Text = "Default Priority:",
                Location = new Point(20, 68),
                Size = new Size(120, 20),
                Font = new Font("Segoe UI", 9F)
            };

            var chkAutoPrint = new CheckBox
            {
                Text = "Auto Print Ticket",
                Location = new Point(150, 100),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F),
                Checked = true
            };

            var chkSaveLog = new CheckBox
            {
                Text = "Save Transaction Log",
                Location = new Point(150, 130),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 9F),
                Checked = true
            };

            var btnBackupNow = new Button
            {
                Text = "Backup Database Now",
                Location = new Point(150, 170),
                Size = new Size(200, 35),
                Font = new Font("Segoe UI", 9F),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            generalSettings.Controls.AddRange(new Control[] {
                lblDefaultType, defaultVehicleType,
                lblDefaultPriority, defaultPriority,
                chkAutoPrint, chkSaveLog,
                btnBackupNow
            });

            settingsPanel.Controls.Add(generalSettings);
            settingsPanel.Controls.Add(mcuSettings);
            settingsPanel.Controls.Add(printerSettings);
            page.Controls.Add(settingsPanel);

            // Add camera settings button
            btnOpenCameraSettings = new Button
            {
                Text = "Pengaturan Kamera",
                Location = new Point(20, 400),
                Size = new Size(180, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnOpenCameraSettings.Click += BtnOpenCameraSettings_Click;
            page.Controls.Add(btnOpenCameraSettings);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F2:
                    BtnScan_Click(sender, e);
                    break;
                case Keys.F3:
                    BtnPrint_Click(sender, e);
                    break;
                case Keys.F4:
                    BtnClear_Click(sender, e);
                    break;
            }
        }

        private void BtnTestPrint_Click(object sender, EventArgs e)
        {
            if (!isPrinterConnected)
            {
                MessageBox.Show("Printer not connected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string testData = "\x1B@"; // Initialize printer
                testData += "\x1B!1"; // Bold text
                testData += "TEST PRINT\n\n";
                testData += "\x1B!0"; // Normal text
                testData += "Printer test successful\n";
                testData += DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\n");
                testData += "\n\n\n"; // Paper feed
                testData += "\x1D\x56\x41\x03"; // Cut paper

                thermalPrinter?.Write(testData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Test print failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtPlateNumber.Clear();
            txtVehicleInfo.Clear();
            cmbPriority.SelectedIndex = 0;
            cmbVehicleType.SelectedIndex = 0;
        }

        private void LoadComPorts()
        {
            cmbPorts.Items.Clear();
            string[] ports = SerialPort.GetPortNames();
            cmbPorts.Items.AddRange(ports);
            if (ports.Length > 0)
                cmbPorts.SelectedIndex = 0;

            cmbMcuPorts.Items.Clear();
            cmbMcuPorts.Items.AddRange(ports);
            if (ports.Length > 1)
                cmbMcuPorts.SelectedIndex = 1;
            else if (ports.Length > 0)
                cmbMcuPorts.SelectedIndex = 0;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (isPrinterConnected)
            {
                DisconnectPrinter();
            }
            else
            {
                ConnectPrinter();
            }
        }

        private void ConnectPrinter()
        {
            try
            {
                if (cmbPorts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                thermalPrinter = new SerialPort();
                thermalPrinter.PortName = cmbPorts.SelectedItem.ToString();
                thermalPrinter.BaudRate = 9600;
                thermalPrinter.DataBits = 8;
                thermalPrinter.Parity = Parity.None;
                thermalPrinter.StopBits = StopBits.One;
                thermalPrinter.Open();
                
                isPrinterConnected = true;
                btnConnect.Text = "Disconnect";
                lblPrinterStatus.Text = "Status: Connected";
                lblPrinterStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printer initialization failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectPrinter()
        {
            if (thermalPrinter?.IsOpen == true)
            {
                thermalPrinter.Close();
            }
            isPrinterConnected = false;
            btnConnect.Text = "Connect";
            lblPrinterStatus.Text = "Status: Not Connected";
            lblPrinterStatus.ForeColor = Color.Red;
        }

        private void BtnScan_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var bitmap = new Bitmap(openFileDialog.FileName);
                        var barcodeReader = new BarcodeReaderGeneric();
                        var result = barcodeReader.Decode(bitmap);
                        if (result != null)
                        {
                            ProcessVehicleBarcode(result.Text);
                        }
                        else
                        {
                            MessageBox.Show("No barcode found in the image.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error scanning barcode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ProcessVehicleBarcode(string barcodeData)
        {
            txtVehicleInfo.Text = $"Scanned Data:\r\n{barcodeData}\r\nScanned at: {DateTime.Now}";
            // Try to extract plate number if it's in the barcode
            if (barcodeData.Length >= 6)
            {
                txtPlateNumber.Text = barcodeData;
            }

            // Add to history
            var record = new VehicleRecord
            {
                Timestamp = DateTime.Now,
                PlateNumber = txtPlateNumber.Text,
                VehicleType = cmbVehicleType.SelectedItem?.ToString(),
                Priority = cmbPriority.SelectedItem?.ToString(),
                Info = barcodeData
            };
            vehicleHistory.Add(record);

            // Add to list view
            var item = new ListViewItem(new[]
            {
                record.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                record.PlateNumber,
                record.VehicleType,
                record.Priority,
                record.Info
            });
            lstHistory.Items.Insert(0, item);
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            if (!isPrinterConnected)
            {
                MessageBox.Show("Printer not connected!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
            {
                MessageBox.Show("Please enter a plate number!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Format the ticket data
                string ticketData = $"\x1B@"; // Initialize printer
                ticketData += $"\x1B!0"; // Normal text
                ticketData += "\x1B!1"; // Bold text
                ticketData += "IUT VEHICLE PRIORITY TICKET\n\n";
                ticketData += "\x1B!0"; // Normal text
                ticketData += $"Date: {DateTime.Now}\n";
                ticketData += $"Plate Number: {txtPlateNumber.Text}\n";
                ticketData += $"Vehicle Type: {cmbVehicleType.SelectedItem}\n";
                ticketData += $"Priority Level: {cmbPriority.SelectedItem}\n\n";
                ticketData += "Additional Information:\n";
                ticketData += $"{txtVehicleInfo.Text}\n";
                ticketData += "\n\n\n"; // Paper feed
                ticketData += "\x1D\x56\x41\x03"; // Cut paper

                // Send to printer
                thermalPrinter?.Write(ticketData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnMcuConnect_Click(object sender, EventArgs e)
        {
            if (isMcuConnected)
            {
                DisconnectMcu();
            }
            else
            {
                ConnectMcu();
            }
        }

        private void ConnectMcu()
        {
            try
            {
                if (cmbMcuPorts.SelectedItem == null)
                {
                    MessageBox.Show("Please select a COM port for MCU.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                mcuSerialPort = new SerialPort
                {
                    PortName = cmbMcuPorts.SelectedItem.ToString(),
                    BaudRate = 9600,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One
                };

                mcuSerialPort.DataReceived += McuSerialPort_DataReceived;
                mcuSerialPort.Open();
                
                isMcuConnected = true;
                btnMcuConnect.Text = "Disconnect MCU";
                lblMcuStatus.Text = "MCU Status: Connected";
                lblMcuStatus.ForeColor = Color.Green;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"MCU initialization failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectMcu()
        {
            if (mcuSerialPort?.IsOpen == true)
            {
                mcuSerialPort.DataReceived -= McuSerialPort_DataReceived;
                mcuSerialPort.Close();
            }
            isMcuConnected = false;
            btnMcuConnect.Text = "Connect MCU";
            lblMcuStatus.Text = "MCU Status: Not Connected";
            lblMcuStatus.ForeColor = Color.Red;
        }

        private void McuSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (mcuSerialPort?.IsOpen != true) return;

            string data = mcuSerialPort.ReadLine().Trim();
            this.BeginInvoke(new Action(() => ProcessMcuData(data)));
        }

        private void ProcessMcuData(string data)
        {
            try
            {
                if (data.StartsWith("IN:") || data.StartsWith("OUT:"))
                {
                    string[] parts = data.Split(':');
                    if (parts.Length != 2) return;

                    string direction = parts[0];
                    string vehicleId = parts[1];

                    // Update last trigger label
                    lblLastTrigger.Text = $"Last Trigger: {direction} - {vehicleId} at {DateTime.Now:HH:mm:ss}";

                    // Process vehicle data
                    txtPlateNumber.Text = vehicleId;
                    txtVehicleInfo.Text = $"Auto-detected vehicle\nDirection: {direction}\nTime: {DateTime.Now}";

                    // Print ticket
                    if (isPrinterConnected)
                    {
                        PrintTicket(direction, vehicleId);
                    }

                    // Send acknowledgment
                    mcuSerialPort?.WriteLine($"ACK:{vehicleId}");

                    // Broadcast event to WebSocket clients
                    if (_webSocketServer != null)
                    {
                        var vehicleEvent = new
                        {
                            Type = "VehicleDetected",
                            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            PlateNumber = txtPlateNumber.Text,
                            VehicleType = cmbVehicleType.Text,
                            Direction = "IN"
                        };
                        
                        Task.Run(async () => await _webSocketServer.BroadcastAsync(vehicleEvent));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing MCU data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintTicket(string direction, string vehicleId)
        {
            try
            {
                string ticketData = $"\x1B@"; // Initialize printer
                ticketData += $"\x1B!0"; // Normal text
                ticketData += "\x1B!1"; // Bold text
                ticketData += $"IUT VEHICLE {direction} TICKET\n\n";
                ticketData += "\x1B!0"; // Normal text
                ticketData += $"Date: {DateTime.Now}\n";
                ticketData += $"Vehicle ID: {vehicleId}\n";
                ticketData += $"Direction: {direction}\n";
                if (direction == "IN")
                {
                    ticketData += $"Priority Level: {cmbPriority.SelectedItem}\n";
                }
                ticketData += "\n\n\n"; // Paper feed
                ticketData += "\x1D\x56\x41\x03"; // Cut paper

                thermalPrinter?.Write(ticketData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Printing error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnScanPayment_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using var bitmap = new Bitmap(openFileDialog.FileName);
                        var barcodeReader = new BarcodeReaderGeneric();
                        var result = barcodeReader.Decode(bitmap);
                        if (result != null)
                        {
                            ProcessPaymentBarcode(result.Text);
                        }
                        else
                        {
                            MessageBox.Show("No payment code found in the image.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error scanning payment code: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ProcessPaymentBarcode(string barcodeData)
        {
            try
            {
                txtPaymentBarcode.Text = barcodeData;
                
                // Simulate payment amount calculation based on duration
                // In a real system, this would be calculated based on actual entry time
                Random rnd = new Random();
                decimal amount = rnd.Next(10000, 50000);
                txtPaymentAmount.Text = amount.ToString("N0");

                lblPaymentStatus.Text = "Payment Status: Ready to process";
                lblPaymentStatus.ForeColor = Color.Blue;

                // Broadcast event to WebSocket clients
                if (_webSocketServer != null)
                {
                    var paymentEvent = new
                    {
                        Type = "PaymentProcessed",
                        Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        PlateNumber = txtExitPlateNumber.Text,
                        Amount = txtPaymentAmount.Text,
                        Method = cmbPaymentMethod.Text
                    };
                    
                    Task.Run(async () => await _webSocketServer.BroadcastAsync(paymentEvent));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing payment code: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnProcessPayment_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtPaymentBarcode.Text))
            {
                MessageBox.Show("Please scan payment code first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Simulate payment processing
                lblPaymentStatus.Text = "Payment Status: Processing...";
                lblPaymentStatus.ForeColor = Color.Orange;

                // In a real system, you would:
                // 1. Connect to payment gateway
                // 2. Process the payment
                // 3. Get confirmation
                Thread.Sleep(1000); // Simulate processing time

                // Simulate successful payment
                lblPaymentStatus.Text = "Payment Status: Completed";
                lblPaymentStatus.ForeColor = Color.Green;

                // Print receipt
                if (isPrinterConnected)
                {
                    PrintPaymentReceipt();
                }

                // Add to exit history
                AddToExitHistory();

                // Clear form after successful payment
                ClearPaymentForm();
            }
            catch (Exception ex)
            {
                lblPaymentStatus.Text = "Payment Status: Failed";
                lblPaymentStatus.ForeColor = Color.Red;
                MessageBox.Show($"Payment processing error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintPaymentReceipt()
        {
            try
            {
                string receiptData = $"\x1B@"; // Initialize printer
                receiptData += $"\x1B!0"; // Normal text
                receiptData += "\x1B!1"; // Bold text
                receiptData += "IUT VEHICLE PAYMENT RECEIPT\n\n";
                receiptData += "\x1B!0"; // Normal text
                receiptData += $"Date: {DateTime.Now}\n";
                receiptData += $"Payment Code: {txtPaymentBarcode.Text}\n";
                receiptData += $"Amount: Rp {txtPaymentAmount.Text}\n";
                receiptData += $"Payment Method: {cmbPaymentMethod.SelectedItem}\n";
                receiptData += $"Status: PAID\n\n";
                receiptData += "Thank you for your payment\n";
                receiptData += "\n\n\n"; // Paper feed
                receiptData += "\x1D\x56\x41\x03"; // Cut paper

                thermalPrinter?.Write(receiptData);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing receipt: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddToExitHistory()
        {
            var exitHistoryList = mainTabControl.TabPages[1].Controls
                .OfType<GroupBox>()
                .First(g => g.Text == "Exit History")
                .Controls.OfType<ListView>()
                .First();

            var item = new ListViewItem(new[]
            {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                txtExitPlateNumber?.Text ?? "",
                "1 hour", // In real system, calculate actual duration
                $"Rp {txtPaymentAmount.Text}",
                cmbPaymentMethod.SelectedItem?.ToString(),
                "Paid"
            });
            exitHistoryList.Items.Insert(0, item);
        }

        private void ClearPaymentForm()
        {
            txtPaymentBarcode.Clear();
            txtPaymentAmount.Clear();
            txtExitPlateNumber?.Clear();
            lblPaymentStatus.Text = "Payment Status: Waiting for payment";
            lblPaymentStatus.ForeColor = Color.Orange;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Stop WebSocket server if running
                if (_webSocketServer != null)
                {
                    Task.Run(async () => await _webSocketServer.Stop()).Wait();
                    _logger.Info("WebSocket server stopped");
                }
                
                // Close existing connections
                DisconnectAllDevices(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during application shutdown: {ex.Message}");
            }
            
            base.OnFormClosing(e);
        }

        private void InitializeComponent()
        {
            this.Text = "IUT Vehicle Manager";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;
            this.KeyDown += MainForm_KeyDown;

            // Create layout container
            var layoutContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            // Configure row styles
            layoutContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Menu
            layoutContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Toolbar
            layoutContainer.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // DateTime panel
            layoutContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Content

            // Create datetime panel
            var pnlTop = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10, 0, 10, 0)
            };

            lblDateTime.AutoSize = true;
            lblDateTime.Location = new Point(10, 5);
            lblDateTime.Font = new Font("Segoe UI", 11F, FontStyle.Regular);
            pnlTop.Controls.Add(lblDateTime);

            // Create content panel
            var contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            // Add controls to layout container in correct order
            layoutContainer.Controls.Add(mainMenu, 0, 0);
            layoutContainer.Controls.Add(toolStrip, 0, 1);
            layoutContainer.Controls.Add(pnlTop, 0, 2);
            layoutContainer.Controls.Add(contentPanel, 0, 3);

            // Add layout container to form
            this.Controls.Add(layoutContainer);
            this.MainMenuStrip = mainMenu;

            // Add WebSocket initialization and logger setup
            SetupLogger();
            StartWebSocketServer();
        }

        private void InitializeMenus()
        {
            // Create main menu
            mainMenu = new MenuStrip
            {
                Font = new Font("Segoe UI", 9.5F),
                Padding = new Padding(0, 2, 0, 2),
                BackColor = Color.FromArgb(248, 248, 248),
                RenderMode = ToolStripRenderMode.Professional,
                Dock = DockStyle.Top
            };

            // File Menu
            var fileMenu = new ToolStripMenuItem("File")
            {
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5F)
            };
            fileMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Connect All Devices", null, ConnectAllDevices) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Control | Keys.D },
                new ToolStripMenuItem("Disconnect All", null, DisconnectAllDevices) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripSeparator(),
                new ToolStripMenuItem("Exit", null, (s, e) => this.Close()) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Alt | Keys.F4 }
            });

            // Vehicle Menu
            var vehicleMenu = new ToolStripMenuItem("Vehicle")
            {
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5F)
            };
            vehicleMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("New Entry", null, (s, e) => mainTabControl.SelectedIndex = 0) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Control | Keys.N },
                new ToolStripMenuItem("Process Exit", null, (s, e) => mainTabControl.SelectedIndex = 1) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Control | Keys.E },
                new ToolStripSeparator(),
                new ToolStripMenuItem("Vehicle Types", null, ManageVehicleTypes) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripMenuItem("Priority Levels", null, ManagePriorityLevels) { Padding = new Padding(8, 4, 8, 4) }
            });

            // Reports Menu
            var reportsMenu = new ToolStripMenuItem("Reports")
            {
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5F)
            };
            reportsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Daily Report", null, GenerateDailyReport) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Control | Keys.R },
                new ToolStripMenuItem("Monthly Report", null, GenerateMonthlyReport) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Control | Keys.M },
                new ToolStripMenuItem("Custom Report", null, (s, e) => mainTabControl.SelectedIndex = 2) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripSeparator(),
                new ToolStripMenuItem("Export Data", null, ExportData) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.Control | Keys.S }
            });

            // Settings Menu
            var settingsMenu = new ToolStripMenuItem("Settings")
            {
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5F)
            };
            settingsMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Printer Settings", null, (s, e) => ShowSettingsTab("Printer")) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripMenuItem("MCU Settings", null, (s, e) => ShowSettingsTab("MCU")) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripMenuItem("General Settings", null, (s, e) => ShowSettingsTab("General")) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripSeparator(),
                new ToolStripMenuItem("Backup Database", null, BackupDatabase) { Padding = new Padding(8, 4, 8, 4) },
                new ToolStripMenuItem("Restore Database", null, RestoreDatabase) { Padding = new Padding(8, 4, 8, 4) }
            });

            // Help Menu
            var helpMenu = new ToolStripMenuItem("Help")
            {
                Padding = new Padding(8, 0, 8, 0),
                Font = new Font("Segoe UI", 9.5F)
            };
            helpMenu.DropDownItems.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("User Manual", null, ShowUserManual) { Padding = new Padding(8, 4, 8, 4), ShortcutKeys = Keys.F1 },
                new ToolStripMenuItem("About", null, ShowAbout) { Padding = new Padding(8, 4, 8, 4) }
            });

            // Add menus to menu strip
            mainMenu.Items.AddRange(new ToolStripItem[] { fileMenu, vehicleMenu, reportsMenu, settingsMenu, helpMenu });

            // Create toolbar with modern styling
            toolStrip = new ToolStrip
            {
                Font = new Font("Segoe UI", 9.5F),
                Padding = new Padding(5),
                BackColor = Color.FromArgb(248, 248, 248),
                RenderMode = ToolStripRenderMode.Professional,
                GripStyle = ToolStripGripStyle.Hidden,
                ImageScalingSize = new Size(20, 20),
                Dock = DockStyle.Top
            };

            // Create toolbar buttons with consistent sizing and spacing
            var btnNewEntry = new ToolStripButton
            {
                Text = "New Entry",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                AutoSize = false,
                Width = 120,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };
            btnNewEntry.Click += (s, e) => mainTabControl.SelectedIndex = 0;

            var btnProcessExit = new ToolStripButton
            {
                Text = "Process Exit",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                AutoSize = false,
                Width = 120,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };
            btnProcessExit.Click += (s, e) => mainTabControl.SelectedIndex = 1;

            var btnConnectDevices = new ToolStripButton
            {
                Text = "Connect All",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                AutoSize = false,
                Width = 120,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };
            btnConnectDevices.Click += ConnectAllDevices;

            var btnPrintTest = new ToolStripButton
            {
                Text = "Print Test",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                AutoSize = false,
                Width = 120,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };
            btnPrintTest.Click += BtnTestPrint_Click;

            var btnReports = new ToolStripButton
            {
                Text = "Reports",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                AutoSize = false,
                Width = 120,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };
            btnReports.Click += (s, e) => mainTabControl.SelectedIndex = 2;

            var btnSettings = new ToolStripButton
            {
                Text = "Settings",
                DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
                AutoSize = false,
                Width = 120,
                Padding = new Padding(10, 5, 10, 5),
                BackColor = Color.Transparent
            };
            btnSettings.Click += (s, e) => mainTabControl.SelectedIndex = 3;

            // Add buttons to toolbar with separators
            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnNewEntry,
                btnProcessExit,
                new ToolStripSeparator { Margin = new Padding(6, 0, 6, 0) },
                btnConnectDevices,
                btnPrintTest,
                new ToolStripSeparator { Margin = new Padding(6, 0, 6, 0) },
                btnReports,
                btnSettings
            });

            // Add menu and toolbar to form
            this.MainMenuStrip = mainMenu;
            this.Controls.Add(mainMenu);
            this.Controls.Add(toolStrip);
        }

        private void ConnectAllDevices(object? sender, EventArgs e)
        {
            try
            {
                // Connect printer if not connected
                if (!isPrinterConnected)
                {
                    ConnectPrinter();
                }

                // Connect MCU if not connected
                if (!isMcuConnected)
                {
                    ConnectMcu();
                }

                MessageBox.Show("All devices connected successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting devices: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisconnectAllDevices(object? sender, EventArgs e)
        {
            DisconnectPrinter();
            DisconnectMcu();
            MessageBox.Show("All devices disconnected.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ManageVehicleTypes(object? sender, EventArgs e)
        {
            MessageBox.Show("Vehicle Types Management - Coming Soon", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ManagePriorityLevels(object? sender, EventArgs e)
        {
            MessageBox.Show("Priority Levels Management - Coming Soon", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GenerateDailyReport(object? sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 2; // Switch to Reports tab
            // Set date range to today
            var dateRangeGroup = reportPage.Controls.OfType<Panel>()
                .First()
                .Controls.OfType<GroupBox>()
                .First(g => g.Text == "Date Range");

            var datePickers = dateRangeGroup.Controls.OfType<DateTimePicker>().ToList();
            if (datePickers.Count >= 2)
            {
                datePickers[0].Value = DateTime.Today;
                datePickers[1].Value = DateTime.Today;
            }
        }

        private void GenerateMonthlyReport(object? sender, EventArgs e)
        {
            mainTabControl.SelectedIndex = 2; // Switch to Reports tab
            // Set date range to current month
            var dateRangeGroup = reportPage.Controls.OfType<Panel>()
                .First()
                .Controls.OfType<GroupBox>()
                .First(g => g.Text == "Date Range");

            var datePickers = dateRangeGroup.Controls.OfType<DateTimePicker>().ToList();
            if (datePickers.Count >= 2)
            {
                datePickers[0].Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                datePickers[1].Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));
            }
        }

        private void ExportData(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Excel Files|*.xlsx|CSV Files|*.csv|All Files|*.*";
                saveDialog.Title = "Export Data";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Export functionality will be implemented soon.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ShowSettingsTab(string section)
        {
            mainTabControl.SelectedIndex = 3; // Switch to Settings tab
            var settingsPanel = settingsPage.Controls.OfType<Panel>().First();
            var targetGroup = settingsPanel.Controls.OfType<GroupBox>()
                .FirstOrDefault(g => g.Text.Contains(section));
            
            if (targetGroup != null)
            {
                // Scroll to the target group
                settingsPanel.AutoScrollPosition = new Point(0, targetGroup.Top);
            }
        }

        private void BackupDatabase(object? sender, EventArgs e)
        {
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Backup Files|*.bak|All Files|*.*";
                saveDialog.Title = "Backup Database";
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Backup functionality will be implemented soon.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RestoreDatabase(object? sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "Backup Files|*.bak|All Files|*.*";
                openDialog.Title = "Restore Database";
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    MessageBox.Show("Restore functionality will be implemented soon.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ShowUserManual(object? sender, EventArgs e)
        {
            MessageBox.Show("User Manual - Coming Soon", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowAbout(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "IUT Vehicle Manager\nVersion 1.0\n\n" +
                "A comprehensive vehicle management system for\n" +
                "managing vehicle entry and exit with payment processing.\n\n" +
                "© 2024 IUT Vehicle Manager",
                "About IUT Vehicle Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }

        private void SetupLogger()
        {
            try
            {
                // Create a proper logger for the application
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                // In a real application, we would use Serilog or similar library
                _logger = new FileLogger(logDirectory);
                _logger.Info("Application started - Logger initialized");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing logger: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger = new ConsoleLogger(); // Fallback to console logger
            }
        }
        
        private void StartWebSocketServer()
        {
            try
            {
                // Start WebSocket server for real-time monitoring
                _webSocketServer = new WebSocketServer("http://localhost:8080/vehicle-management/", _logger);
                Task.Run(async () => await _webSocketServer.Start());
                _logger.Info("WebSocket server started");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting WebSocket server: {ex.Message}");
                MessageBox.Show($"Error starting WebSocket server: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenCameraSettings_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var cameraSettingsForm = new CameraSettingsForm(_logger))
                {
                    cameraSettingsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error opening camera settings: {ex.Message}");
                MessageBox.Show($"Error opening camera settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnOpenReports_Click(object? sender, EventArgs e)
        {
            try
            {
                using (var reportsForm = new ReportsForm(_logger))
                {
                    reportsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error opening reports: {ex.Message}");
                MessageBox.Show($"Error opening reports: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    // Simple logger interface and implementations
    public interface ILogger
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Debug(string message);
    }
    
    public class ConsoleLogger : ILogger
    {
        public void Info(string message) => Console.WriteLine($"[INFO] {DateTime.Now}: {message}");
        public void Warning(string message) => Console.WriteLine($"[WARN] {DateTime.Now}: {message}");
        public void Error(string message) => Console.WriteLine($"[ERROR] {DateTime.Now}: {message}");
        public void Debug(string message) => Console.WriteLine($"[DEBUG] {DateTime.Now}: {message}");
    }
    
    public class FileLogger : ILogger
    {
        private readonly string _logDirectory;
        
        public FileLogger(string logDirectory)
        {
            _logDirectory = logDirectory;
        }
        
        public void Info(string message) => WriteLog("INFO", message);
        public void Warning(string message) => WriteLog("WARN", message);
        public void Error(string message) => WriteLog("ERROR", message);
        public void Debug(string message) => WriteLog("DEBUG", message);
        
        private void WriteLog(string level, string message)
        {
            try
            {
                string logFile = Path.Combine(_logDirectory, $"iut_vehicle_manager_{DateTime.Now:yyyyMMdd}.log");
                string logEntry = $"[{level}] {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}: {message}";
                
                File.AppendAllText(logFile, logEntry + Environment.NewLine);
            }
            catch
            {
                // Silently fail if logging fails
                Console.WriteLine($"[{level}] {DateTime.Now}: {message} (Failed to write to log file)");
            }
        }
    }
    
    class VehicleRecord
    {
        public DateTime Timestamp { get; set; }
        public string PlateNumber { get; set; } = "";
        public string VehicleType { get; set; } = "";
        public string Priority { get; set; } = "";
        public string Info { get; set; } = "";
    }
}