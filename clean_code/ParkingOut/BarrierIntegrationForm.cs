using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using SimpleParkingAdmin.Utils;
using Serilog;
using Serilog.Events;
using SimpleParkingAdmin.Models;

namespace SimpleParkingAdmin.Forms
{
    public partial class BarrierIntegrationForm : Form
    {
        private ComboBox cmbEntryPort;
        private ComboBox cmbExitPort;
        private Button btnConnectEntry;
        private Button btnConnectExit;
        private Button btnDisconnectEntry;
        private Button btnDisconnectExit;
        private Panel pnlEntryBarrierStatus;
        private Panel pnlExitBarrierStatus;
        private Button btnTestEntryOpen;
        private Button btnTestEntryClose;
        private Button btnTestExitOpen;
        private Button btnTestExitClose;
        private TextBox txtBarrierLog;
        private CheckBox chkAutomaticMode;
        private GroupBox grpSettings;
        private GroupBox grpBarrierControl;
        private GroupBox grpBarrierLogs;
        private Button btnSaveSettings;
        private Button btnClose;
        private System.Windows.Forms.Timer tmrCheckStatus;

        // Serial port instances for entry and exit barriers
        private SerialPort entrySerialPort;
        private SerialPort exitSerialPort;
        
        // Connection status flags
        private bool entryConnected = false;
        private bool exitConnected = false;
        
        // Default commands for barrier control (can be modified based on actual hardware)
        private const string OPEN_COMMAND = "OPEN";
        private const string CLOSE_COMMAND = "CLOSE";
        private const string STATUS_COMMAND = "STATUS";
        
        // Queue for log messages
        private Queue<string> logMessages = new Queue<string>();
        
        private readonly IAppLogger _logger;

        public BarrierIntegrationForm()
        {
            _logger = new FileLogger();
            InitializeComponent();
            InitializeSerialPorts();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Integrasi Palang Otomatis";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            // Settings Group Box
            this.grpSettings = new GroupBox();
            this.grpSettings.Text = "Pengaturan Koneksi";
            this.grpSettings.Location = new Point(12, 12);
            this.grpSettings.Size = new Size(860, 150);
            
            // Entry Port
            Label lblEntryPort = new Label();
            lblEntryPort.Text = "COM Port Palang Masuk:";
            lblEntryPort.AutoSize = true;
            lblEntryPort.Location = new Point(20, 30);
            
            this.cmbEntryPort = new ComboBox();
            this.cmbEntryPort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbEntryPort.Location = new Point(180, 27);
            this.cmbEntryPort.Size = new Size(150, 25);
            
            // Exit Port
            Label lblExitPort = new Label();
            lblExitPort.Text = "COM Port Palang Keluar:";
            lblExitPort.AutoSize = true;
            lblExitPort.Location = new Point(20, 70);
            
            this.cmbExitPort = new ComboBox();
            this.cmbExitPort.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbExitPort.Location = new Point(180, 67);
            this.cmbExitPort.Size = new Size(150, 25);
            
            // Automatic Mode Checkbox
            this.chkAutomaticMode = new CheckBox();
            this.chkAutomaticMode.Text = "Mode Otomatis (Buka/Tutup Palang secara Otomatis)";
            this.chkAutomaticMode.AutoSize = true;
            this.chkAutomaticMode.Location = new Point(20, 110);
            this.chkAutomaticMode.CheckedChanged += new EventHandler(chkAutomaticMode_CheckedChanged);
            
            // Connect/Disconnect Entry
            this.btnConnectEntry = new Button();
            this.btnConnectEntry.Text = "Sambungkan";
            this.btnConnectEntry.Location = new Point(360, 27);
            this.btnConnectEntry.Size = new Size(100, 25);
            this.btnConnectEntry.Click += new EventHandler(btnConnectEntry_Click);
            
            this.btnDisconnectEntry = new Button();
            this.btnDisconnectEntry.Text = "Putuskan";
            this.btnDisconnectEntry.Location = new Point(470, 27);
            this.btnDisconnectEntry.Size = new Size(100, 25);
            this.btnDisconnectEntry.Enabled = false;
            this.btnDisconnectEntry.Click += new EventHandler(btnDisconnectEntry_Click);
            
            // Entry Status
            Label lblEntryStatus = new Label();
            lblEntryStatus.Text = "Status:";
            lblEntryStatus.AutoSize = true;
            lblEntryStatus.Location = new Point(590, 30);
            
            this.pnlEntryBarrierStatus = new Panel();
            this.pnlEntryBarrierStatus.Size = new Size(20, 20);
            this.pnlEntryBarrierStatus.Location = new Point(640, 30);
            this.pnlEntryBarrierStatus.BackColor = Color.Red;
            
            // Connect/Disconnect Exit
            this.btnConnectExit = new Button();
            this.btnConnectExit.Text = "Sambungkan";
            this.btnConnectExit.Location = new Point(360, 67);
            this.btnConnectExit.Size = new Size(100, 25);
            this.btnConnectExit.Click += new EventHandler(btnConnectExit_Click);
            
            this.btnDisconnectExit = new Button();
            this.btnDisconnectExit.Text = "Putuskan";
            this.btnDisconnectExit.Location = new Point(470, 67);
            this.btnDisconnectExit.Size = new Size(100, 25);
            this.btnDisconnectExit.Enabled = false;
            this.btnDisconnectExit.Click += new EventHandler(btnDisconnectExit_Click);
            
            // Exit Status
            Label lblExitStatus = new Label();
            lblExitStatus.Text = "Status:";
            lblExitStatus.AutoSize = true;
            lblExitStatus.Location = new Point(590, 70);
            
            this.pnlExitBarrierStatus = new Panel();
            this.pnlExitBarrierStatus.Size = new Size(20, 20);
            this.pnlExitBarrierStatus.Location = new Point(640, 70);
            this.pnlExitBarrierStatus.BackColor = Color.Red;
            
            // Save Settings
            this.btnSaveSettings = new Button();
            this.btnSaveSettings.Text = "Simpan Pengaturan";
            this.btnSaveSettings.Location = new Point(700, 110);
            this.btnSaveSettings.Size = new Size(140, 30);
            this.btnSaveSettings.Click += new EventHandler(btnSaveSettings_Click);
            
            // Add all controls to settings group
            this.grpSettings.Controls.Add(lblEntryPort);
            this.grpSettings.Controls.Add(this.cmbEntryPort);
            this.grpSettings.Controls.Add(lblExitPort);
            this.grpSettings.Controls.Add(this.cmbExitPort);
            this.grpSettings.Controls.Add(this.chkAutomaticMode);
            this.grpSettings.Controls.Add(this.btnConnectEntry);
            this.grpSettings.Controls.Add(this.btnDisconnectEntry);
            this.grpSettings.Controls.Add(lblEntryStatus);
            this.grpSettings.Controls.Add(this.pnlEntryBarrierStatus);
            this.grpSettings.Controls.Add(this.btnConnectExit);
            this.grpSettings.Controls.Add(this.btnDisconnectExit);
            this.grpSettings.Controls.Add(lblExitStatus);
            this.grpSettings.Controls.Add(this.pnlExitBarrierStatus);
            this.grpSettings.Controls.Add(this.btnSaveSettings);
            
            // Barrier Control Group Box
            this.grpBarrierControl = new GroupBox();
            this.grpBarrierControl.Text = "Kontrol Manual Palang";
            this.grpBarrierControl.Location = new Point(12, 170);
            this.grpBarrierControl.Size = new Size(860, 100);
            
            // Entry Barrier Controls
            Label lblEntryControls = new Label();
            lblEntryControls.Text = "Palang Masuk:";
            lblEntryControls.AutoSize = true;
            lblEntryControls.Location = new Point(20, 30);
            
            this.btnTestEntryOpen = new Button();
            this.btnTestEntryOpen.Text = "Buka Palang Masuk";
            this.btnTestEntryOpen.Location = new Point(150, 27);
            this.btnTestEntryOpen.Size = new Size(150, 30);
            this.btnTestEntryOpen.Enabled = false;
            this.btnTestEntryOpen.Click += new EventHandler(btnTestEntryOpen_Click);
            
            this.btnTestEntryClose = new Button();
            this.btnTestEntryClose.Text = "Tutup Palang Masuk";
            this.btnTestEntryClose.Location = new Point(320, 27);
            this.btnTestEntryClose.Size = new Size(150, 30);
            this.btnTestEntryClose.Enabled = false;
            this.btnTestEntryClose.Click += new EventHandler(btnTestEntryClose_Click);
            
            // Exit Barrier Controls
            Label lblExitControls = new Label();
            lblExitControls.Text = "Palang Keluar:";
            lblExitControls.AutoSize = true;
            lblExitControls.Location = new Point(20, 70);
            
            this.btnTestExitOpen = new Button();
            this.btnTestExitOpen.Text = "Buka Palang Keluar";
            this.btnTestExitOpen.Location = new Point(150, 67);
            this.btnTestExitOpen.Size = new Size(150, 30);
            this.btnTestExitOpen.Enabled = false;
            this.btnTestExitOpen.Click += new EventHandler(btnTestExitOpen_Click);
            
            this.btnTestExitClose = new Button();
            this.btnTestExitClose.Text = "Tutup Palang Keluar";
            this.btnTestExitClose.Location = new Point(320, 67);
            this.btnTestExitClose.Size = new Size(150, 30);
            this.btnTestExitClose.Enabled = false;
            this.btnTestExitClose.Click += new EventHandler(btnTestExitClose_Click);
            
            // Add all controls to barrier control group
            this.grpBarrierControl.Controls.Add(lblEntryControls);
            this.grpBarrierControl.Controls.Add(this.btnTestEntryOpen);
            this.grpBarrierControl.Controls.Add(this.btnTestEntryClose);
            this.grpBarrierControl.Controls.Add(lblExitControls);
            this.grpBarrierControl.Controls.Add(this.btnTestExitOpen);
            this.grpBarrierControl.Controls.Add(this.btnTestExitClose);
            
            // Barrier Logs Group Box
            this.grpBarrierLogs = new GroupBox();
            this.grpBarrierLogs.Text = "Log Aktivitas Palang";
            this.grpBarrierLogs.Location = new Point(12, 280);
            this.grpBarrierLogs.Size = new Size(860, 230);
            
            // Log TextBox
            this.txtBarrierLog = new TextBox();
            this.txtBarrierLog.Multiline = true;
            this.txtBarrierLog.ReadOnly = true;
            this.txtBarrierLog.ScrollBars = ScrollBars.Vertical;
            this.txtBarrierLog.BackColor = Color.Black;
            this.txtBarrierLog.ForeColor = Color.LightGreen;
            this.txtBarrierLog.Font = new Font("Consolas", 9F);
            this.txtBarrierLog.Location = new Point(20, 25);
            this.txtBarrierLog.Size = new Size(820, 190);
            
            this.grpBarrierLogs.Controls.Add(this.txtBarrierLog);
            
            // Close button
            this.btnClose = new Button();
            this.btnClose.Text = "Tutup";
            this.btnClose.Location = new Point(772, 520);
            this.btnClose.Size = new Size(100, 30);
            this.btnClose.Click += new EventHandler(btnClose_Click);
            
            // Timer for checking barrier status
            this.tmrCheckStatus = new System.Windows.Forms.Timer();
            this.tmrCheckStatus.Interval = 5000; // Check every 5 seconds
            this.tmrCheckStatus.Tick += new EventHandler(tmrCheckStatus_Tick);
            
            // Add all groups to form
            this.Controls.Add(this.grpSettings);
            this.Controls.Add(this.grpBarrierControl);
            this.Controls.Add(this.grpBarrierLogs);
            this.Controls.Add(this.btnClose);
            
            // Hook up form events
            this.Load += new EventHandler(BarrierIntegrationForm_Load);
            this.FormClosing += new FormClosingEventHandler(BarrierIntegrationForm_FormClosing);
        }
        
        private void InitializeSerialPorts()
        {
            entrySerialPort = new SerialPort();
            entrySerialPort.BaudRate = 9600;
            entrySerialPort.DataBits = 8;
            entrySerialPort.Parity = Parity.None;
            entrySerialPort.StopBits = StopBits.One;
            entrySerialPort.Handshake = Handshake.None;
            entrySerialPort.DataReceived += EntrySerialPort_DataReceived;
            
            exitSerialPort = new SerialPort();
            exitSerialPort.BaudRate = 9600;
            exitSerialPort.DataBits = 8;
            exitSerialPort.Parity = Parity.None;
            exitSerialPort.StopBits = StopBits.One;
            exitSerialPort.Handshake = Handshake.None;
            exitSerialPort.DataReceived += ExitSerialPort_DataReceived;
        }
        
        private void BarrierIntegrationForm_Load(object sender, EventArgs e)
        {
            // Populate COM ports
            string[] ports = SerialPort.GetPortNames();
            cmbEntryPort.Items.AddRange(ports);
            cmbExitPort.Items.AddRange(ports);
            
            if (ports.Length > 0)
            {
                cmbEntryPort.SelectedIndex = 0;
                cmbExitPort.SelectedIndex = Math.Min(1, ports.Length - 1); // Try to select second port if available
            }
            
            // Load saved settings
            LoadSettings();
            
            // Start the log updater
            StartLogUpdater();
            
            // Add initial log entry
            AddLogMessage("Sistem integrasi palang otomatis siap digunakan.");
        }
        
        private void LoadSettings()
        {
            try
            {
                // Try to load settings from the database
                string query = "SELECT nama, nilai FROM settings WHERE nama IN ('entry_port', 'exit_port', 'barrier_auto_mode')";
                DataTable settings = Database.ExecuteQuery(query);
                
                foreach (DataRow row in settings.Rows)
                {
                    string name = row["nama"].ToString();
                    string value = row["nilai"].ToString();
                    
                    switch (name)
                    {
                        case "entry_port":
                            SelectComboBoxItem(cmbEntryPort, value);
                            break;
                        case "exit_port":
                            SelectComboBoxItem(cmbExitPort, value);
                            break;
                        case "barrier_auto_mode":
                            chkAutomaticMode.Checked = value == "1" || value.ToLower() == "true";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error loading settings: {ex.Message}");
            }
        }
        
        private void SelectComboBoxItem(ComboBox comboBox, string value)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                if (comboBox.Items[i].ToString() == value)
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }
        
        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // Save settings to database
                string entryPort = cmbEntryPort.SelectedItem?.ToString() ?? "";
                string exitPort = cmbExitPort.SelectedItem?.ToString() ?? "";
                string autoMode = chkAutomaticMode.Checked ? "1" : "0";
                
                SaveSetting("entry_port", entryPort);
                SaveSetting("exit_port", exitPort);
                SaveSetting("barrier_auto_mode", autoMode);
                
                AddLogMessage("Pengaturan berhasil disimpan.");
                MessageBox.Show("Pengaturan berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error saving settings: {ex.Message}");
                MessageBox.Show($"Error saat menyimpan pengaturan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SaveSetting(string name, string value)
        {
            try
            {
                // Check if setting exists
                string checkQuery = $"SELECT COUNT(*) FROM settings WHERE nama = '{name}'";
                int count = Convert.ToInt32(Database.ExecuteScalar(checkQuery));
                
                if (count > 0)
                {
                    // Update
                    string updateQuery = $"UPDATE settings SET nilai = '{value}' WHERE nama = '{name}'";
                    Database.ExecuteNonQuery(updateQuery);
                }
                else
                {
                    // Insert
                    string insertQuery = $"INSERT INTO settings (nama, nilai) VALUES ('{name}', '{value}')";
                    Database.ExecuteNonQuery(insertQuery);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving setting {name}: {ex.Message}");
            }
        }
        
        private void btnConnectEntry_Click(object sender, EventArgs e)
        {
            if (cmbEntryPort.SelectedItem == null)
            {
                MessageBox.Show("Silakan pilih COM Port untuk palang masuk.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                if (!entrySerialPort.IsOpen)
                {
                    entrySerialPort.PortName = cmbEntryPort.SelectedItem.ToString();
                    entrySerialPort.Open();
                    
                    // Update UI
                    pnlEntryBarrierStatus.BackColor = Color.Green;
                    btnConnectEntry.Enabled = false;
                    btnDisconnectEntry.Enabled = true;
                    btnTestEntryOpen.Enabled = true;
                    btnTestEntryClose.Enabled = true;
                    cmbEntryPort.Enabled = false;
                    
                    entryConnected = true;
                    AddLogMessage($"Berhasil terhubung ke palang masuk pada port {entrySerialPort.PortName}");
                    
                    // Start status checking timer if either entry or exit is connected
                    if (!tmrCheckStatus.Enabled && (entryConnected || exitConnected))
                    {
                        tmrCheckStatus.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error connecting to entry barrier: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnDisconnectEntry_Click(object sender, EventArgs e)
        {
            try
            {
                if (entrySerialPort.IsOpen)
                {
                    entrySerialPort.Close();
                    
                    // Update UI
                    pnlEntryBarrierStatus.BackColor = Color.Red;
                    btnConnectEntry.Enabled = true;
                    btnDisconnectEntry.Enabled = false;
                    btnTestEntryOpen.Enabled = false;
                    btnTestEntryClose.Enabled = false;
                    cmbEntryPort.Enabled = true;
                    
                    entryConnected = false;
                    AddLogMessage("Koneksi ke palang masuk terputus.");
                    
                    // Stop timer if neither is connected
                    if (tmrCheckStatus.Enabled && !entryConnected && !exitConnected)
                    {
                        tmrCheckStatus.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error disconnecting from entry barrier: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Disconnection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnConnectExit_Click(object sender, EventArgs e)
        {
            if (cmbExitPort.SelectedItem == null)
            {
                MessageBox.Show("Silakan pilih COM Port untuk palang keluar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            try
            {
                if (!exitSerialPort.IsOpen)
                {
                    exitSerialPort.PortName = cmbExitPort.SelectedItem.ToString();
                    exitSerialPort.Open();
                    
                    // Update UI
                    pnlExitBarrierStatus.BackColor = Color.Green;
                    btnConnectExit.Enabled = false;
                    btnDisconnectExit.Enabled = true;
                    btnTestExitOpen.Enabled = true;
                    btnTestExitClose.Enabled = true;
                    cmbExitPort.Enabled = false;
                    
                    exitConnected = true;
                    AddLogMessage($"Berhasil terhubung ke palang keluar pada port {exitSerialPort.PortName}");
                    
                    // Start status checking timer if either entry or exit is connected
                    if (!tmrCheckStatus.Enabled && (entryConnected || exitConnected))
                    {
                        tmrCheckStatus.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error connecting to exit barrier: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnDisconnectExit_Click(object sender, EventArgs e)
        {
            try
            {
                if (exitSerialPort.IsOpen)
                {
                    exitSerialPort.Close();
                    
                    // Update UI
                    pnlExitBarrierStatus.BackColor = Color.Red;
                    btnConnectExit.Enabled = true;
                    btnDisconnectExit.Enabled = false;
                    btnTestExitOpen.Enabled = false;
                    btnTestExitClose.Enabled = false;
                    cmbExitPort.Enabled = true;
                    
                    exitConnected = false;
                    AddLogMessage("Koneksi ke palang keluar terputus.");
                    
                    // Stop timer if neither is connected
                    if (tmrCheckStatus.Enabled && !entryConnected && !exitConnected)
                    {
                        tmrCheckStatus.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error disconnecting from exit barrier: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}", "Disconnection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void EntrySerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (entrySerialPort.IsOpen)
                {
                    string data = entrySerialPort.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        // Process the received data
                        AddLogMessage($"[ENTRY] Received: {data}");
                        ProcessEntryBarrierData(data);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error reading from entry barrier: {ex.Message}");
            }
        }
        
        private void ExitSerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (exitSerialPort.IsOpen)
                {
                    string data = exitSerialPort.ReadLine().Trim();
                    if (!string.IsNullOrEmpty(data))
                    {
                        // Process the received data
                        AddLogMessage($"[EXIT] Received: {data}");
                        ProcessExitBarrierData(data);
                    }
                }
            }
            catch (Exception ex)
            {
                AddLogMessage($"Error reading from exit barrier: {ex.Message}");
            }
        }
        
        private void ProcessEntryBarrierData(string data)
        {
            // Here you would implement the logic to process data from the entry barrier
            // This is dependent on the specific hardware protocol
            
            // Example: If the barrier sends a vehicle detection message, open the barrier
            if (data.Contains("VEHICLE_DETECTED") && chkAutomaticMode.Checked)
            {
                OpenEntryBarrier();
            }
        }
        
        private void ProcessExitBarrierData(string data)
        {
            // Here you would implement the logic to process data from the exit barrier
            // This is dependent on the specific hardware protocol
            
            // Example: If the barrier sends a vehicle detection message, open the barrier
            if (data.Contains("VEHICLE_DETECTED") && chkAutomaticMode.Checked)
            {
                OpenExitBarrier();
            }
        }
        
        private void btnTestEntryOpen_Click(object sender, EventArgs e)
        {
            OpenEntryBarrier();
        }
        
        private void btnTestEntryClose_Click(object sender, EventArgs e)
        {
            CloseEntryBarrier();
        }
        
        private void btnTestExitOpen_Click(object sender, EventArgs e)
        {
            OpenExitBarrier();
        }
        
        private void btnTestExitClose_Click(object sender, EventArgs e)
        {
            CloseExitBarrier();
        }
        
        private void OpenEntryBarrier()
        {
            if (entryConnected && entrySerialPort.IsOpen)
            {
                try
                {
                    AddLogMessage("[ENTRY] Sending open command...");
                    entrySerialPort.WriteLine(OPEN_COMMAND);
                    // In a real implementation, you might wait for confirmation
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error opening entry barrier: {ex.Message}");
                }
            }
            else
            {
                AddLogMessage("Cannot open entry barrier - not connected");
            }
        }
        
        private void CloseEntryBarrier()
        {
            if (entryConnected && entrySerialPort.IsOpen)
            {
                try
                {
                    AddLogMessage("[ENTRY] Sending close command...");
                    entrySerialPort.WriteLine(CLOSE_COMMAND);
                    // In a real implementation, you might wait for confirmation
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error closing entry barrier: {ex.Message}");
                }
            }
            else
            {
                AddLogMessage("Cannot close entry barrier - not connected");
            }
        }
        
        private void OpenExitBarrier()
        {
            if (exitConnected && exitSerialPort.IsOpen)
            {
                try
                {
                    AddLogMessage("[EXIT] Sending open command...");
                    exitSerialPort.WriteLine(OPEN_COMMAND);
                    // In a real implementation, you might wait for confirmation
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error opening exit barrier: {ex.Message}");
                }
            }
            else
            {
                AddLogMessage("Cannot open exit barrier - not connected");
            }
        }
        
        private void CloseExitBarrier()
        {
            if (exitConnected && exitSerialPort.IsOpen)
            {
                try
                {
                    AddLogMessage("[EXIT] Sending close command...");
                    exitSerialPort.WriteLine(CLOSE_COMMAND);
                    // In a real implementation, you might wait for confirmation
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error closing exit barrier: {ex.Message}");
                }
            }
            else
            {
                AddLogMessage("Cannot close exit barrier - not connected");
            }
        }
        
        private void tmrCheckStatus_Tick(object sender, EventArgs e)
        {
            // Periodically check status of barriers
            CheckBarrierStatus();
        }
        
        private void CheckBarrierStatus()
        {
            // Check entry barrier status
            if (entryConnected && entrySerialPort.IsOpen)
            {
                try
                {
                    entrySerialPort.WriteLine(STATUS_COMMAND);
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error checking entry barrier status: {ex.Message}");
                }
            }
            
            // Check exit barrier status
            if (exitConnected && exitSerialPort.IsOpen)
            {
                try
                {
                    exitSerialPort.WriteLine(STATUS_COMMAND);
                }
                catch (Exception ex)
                {
                    AddLogMessage($"Error checking exit barrier status: {ex.Message}");
                }
            }
        }
        
        private void chkAutomaticMode_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAutomaticMode.Checked)
            {
                AddLogMessage("Mode otomatis diaktifkan. Palang akan terbuka dan tertutup otomatis.");
            }
            else
            {
                AddLogMessage("Mode otomatis dinonaktifkan. Palang perlu dikontrol secara manual.");
            }
        }
        
        private void AddLogMessage(string message)
        {
            lock (logMessages)
            {
                // Add timestamp
                string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                string logEntry = $"[{timestamp}] {message}";
                
                // Add to queue
                logMessages.Enqueue(logEntry);
                
                // Keep the queue size reasonable
                while (logMessages.Count > 100)
                {
                    logMessages.Dequeue();
                }
            }
        }
        
        private void StartLogUpdater()
        {
            // Run a task to periodically update the log text box
            Task.Run(async () =>
            {
                while (!this.IsDisposed)
                {
                    try
                    {
                        await UpdateLogTextBox();
                    }
                    catch
                    {
                        // Ignore exceptions in background thread
                    }
                    
                    await Task.Delay(500); // Update every 500ms
                }
            });
        }
        
        private async Task UpdateLogTextBox()
        {
            if (this.IsDisposed || txtBarrierLog == null || txtBarrierLog.IsDisposed)
                return;
            
            StringBuilder sb = new StringBuilder();
            string[] logEntries;
            
            lock (logMessages)
            {
                logEntries = logMessages.ToArray();
            }
            
            foreach (string entry in logEntries)
            {
                sb.AppendLine(entry);
            }
            
            // Update UI on the UI thread
            await this.InvokeAsync(() =>
            {
                if (!this.IsDisposed && txtBarrierLog != null && !txtBarrierLog.IsDisposed)
                {
                    txtBarrierLog.Text = sb.ToString();
                    txtBarrierLog.SelectionStart = txtBarrierLog.TextLength;
                    txtBarrierLog.ScrollToCaret();
                }
            });
        }
        
        private Task InvokeAsync(Action action)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        action();
                        tcs.SetResult(true);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                    }
                }));
            }
            else
            {
                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
            
            return tcs.Task;
        }
        
        // Public method for other forms to open the entry barrier (e.g., from ticket validation)
        public void OpenEntryBarrierFromExternal()
        {
            if (chkAutomaticMode.Checked)
            {
                OpenEntryBarrier();
            }
        }
        
        // Public method for other forms to open the exit barrier (e.g., from fee payment)
        public void OpenExitBarrierFromExternal()
        {
            if (chkAutomaticMode.Checked)
            {
                OpenExitBarrier();
            }
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void BarrierIntegrationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Clean up connections
            if (entrySerialPort != null && entrySerialPort.IsOpen)
            {
                entrySerialPort.Close();
            }
            
            if (exitSerialPort != null && exitSerialPort.IsOpen)
            {
                exitSerialPort.Close();
            }
            
            // Stop timer
            if (tmrCheckStatus != null && tmrCheckStatus.Enabled)
            {
                tmrCheckStatus.Stop();
            }
        }
    }
} 