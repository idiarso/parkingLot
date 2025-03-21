using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
using System.Threading;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using SimpleParkingAdmin.Models;
using SimpleParkingAdmin.Utils;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    public partial class CombinedEntryExitForm : Form
    {
        // Common properties
        private TabControl tabControl;
        private TabPage entryTab;
        private TabPage exitTab;
        private User _currentUser;

        // Entry tab properties
        private VideoCaptureDevice videoSource;
        private string entryImagePath;
        private string barcodeImagePath;
        private readonly string entryImagesPath;
        private Panel pnlEntryCamera;
        private Panel pnlEntryData;
        private TextBox txtPlateNumber;
        private ComboBox cmbVehicleType;
        private Button btnCapture;
        private Button btnGenerateTicket;
        private Button btnViewImages;
        private PictureBox picCamera;
        private PictureBox picCaptured;

        // Exit tab properties
        private SerialPort gateControlPort;
        private string gateConfigPath;
        private string exitImagesPath;
        private TextBox txtTicketNumber;
        private Button btnFindTicket;
        private Panel pnlExitData;
        private Label lblEntryTime;
        private Label lblExitTime;
        private Label lblDuration;
        private Label lblFee;
        private Button btnProcessExit;
        private Button btnLostTicket;
        
        private readonly IAppLogger _logger;
        private FilterInfoCollection videoDevices;
        
        public CombinedEntryExitForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = CustomLogManager.GetLogger();
            InitializeComponent();
            
            // Create directories for storing images
            entryImagesPath = Path.Combine(Application.StartupPath, "Images", "Entry");
            exitImagesPath = Path.Combine(Application.StartupPath, "Images", "Exit");
            gateConfigPath = Path.Combine(Application.StartupPath, "config", "gate.ini");
            
            if (!Directory.Exists(entryImagesPath))
            {
                Directory.CreateDirectory(entryImagesPath);
            }
            
            if (!Directory.Exists(exitImagesPath))
            {
                Directory.CreateDirectory(exitImagesPath);
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Entry & Exit Management";
            this.Size = new Size(1200, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            
            // Create tab control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;
            tabControl.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            
            // Create entry tab
            entryTab = new TabPage("Entry Management");
            entryTab.Padding = new Padding(10);
            entryTab.BackColor = Color.FromArgb(250, 250, 250);
            
            // Create exit tab
            exitTab = new TabPage("Exit Management");
            exitTab.Padding = new Padding(10);
            exitTab.BackColor = Color.FromArgb(250, 250, 250);
            
            // Add tabs to tab control
            tabControl.Controls.Add(entryTab);
            tabControl.Controls.Add(exitTab);
            
            // Add tab control to form
            this.Controls.Add(tabControl);
            
            // Database connection info at the bottom of the form
            Label lblDbConnection = new Label();
            lblDbConnection.Text = Database.IsUsingNetworkDatabase ? 
                $"Connected to network database" : 
                "Using local database";
            lblDbConnection.Dock = DockStyle.Bottom;
            lblDbConnection.Height = 20;
            lblDbConnection.TextAlign = ContentAlignment.MiddleCenter;
            lblDbConnection.Font = new Font("Segoe UI", 8F);
            lblDbConnection.ForeColor = Database.IsUsingNetworkDatabase ? 
                Color.Green : Color.Blue;
            this.Controls.Add(lblDbConnection);
            
            // Initialize entry tab controls
            InitializeEntryControls();
            
            // Initialize exit tab controls
            InitializeExitControls();
            
            this.ResumeLayout(false);
        }
        
        private void InitializeEntryControls()
        {
            // Create entry layout
            TableLayoutPanel tlpEntry = new TableLayoutPanel();
            tlpEntry.Dock = DockStyle.Fill;
            tlpEntry.ColumnCount = 2;
            tlpEntry.RowCount = 1;
            tlpEntry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpEntry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            
            // Camera panel
            pnlEntryCamera = new Panel();
            pnlEntryCamera.Dock = DockStyle.Fill;
            pnlEntryCamera.BorderStyle = BorderStyle.FixedSingle;
            pnlEntryCamera.Margin = new Padding(10);
            
            // Camera picture box
            picCamera = new PictureBox();
            picCamera.Dock = DockStyle.Fill;
            picCamera.SizeMode = PictureBoxSizeMode.Zoom;
            pnlEntryCamera.Controls.Add(picCamera);
            
            // Captured image
            picCaptured = new PictureBox();
            picCaptured.Size = new Size(180, 140);
            picCaptured.Location = new Point(10, 10);
            picCaptured.BorderStyle = BorderStyle.FixedSingle;
            picCaptured.SizeMode = PictureBoxSizeMode.Zoom;
            
            // Entry data panel
            pnlEntryData = new Panel();
            pnlEntryData.Dock = DockStyle.Fill;
            pnlEntryData.BorderStyle = BorderStyle.FixedSingle;
            pnlEntryData.Margin = new Padding(10);
            pnlEntryData.Padding = new Padding(10);
            
            // Vehicle plate label
            Label lblPlateNumber = new Label();
            lblPlateNumber.Text = "Plate Number:";
            lblPlateNumber.Location = new Point(10, 160);
            lblPlateNumber.Size = new Size(100, 25);
            lblPlateNumber.Font = new Font("Segoe UI", 10F);
            pnlEntryData.Controls.Add(lblPlateNumber);
            
            // Plate number textbox
            txtPlateNumber = new TextBox();
            txtPlateNumber.Location = new Point(120, 160);
            txtPlateNumber.Size = new Size(200, 30);
            txtPlateNumber.Font = new Font("Segoe UI", 10F);
            pnlEntryData.Controls.Add(txtPlateNumber);
            
            // Vehicle type label
            Label lblVehicleType = new Label();
            lblVehicleType.Text = "Vehicle Type:";
            lblVehicleType.Location = new Point(10, 200);
            lblVehicleType.Size = new Size(100, 25);
            lblVehicleType.Font = new Font("Segoe UI", 10F);
            pnlEntryData.Controls.Add(lblVehicleType);
            
            // Vehicle type combobox
            cmbVehicleType = new ComboBox();
            cmbVehicleType.Location = new Point(120, 200);
            cmbVehicleType.Size = new Size(200, 30);
            cmbVehicleType.Font = new Font("Segoe UI", 10F);
            cmbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            pnlEntryData.Controls.Add(cmbVehicleType);
            
            // Capture button
            btnCapture = new Button();
            btnCapture.Text = "Capture Image";
            btnCapture.Location = new Point(10, 250);
            btnCapture.Size = new Size(150, 40);
            btnCapture.Font = new Font("Segoe UI", 10F);
            btnCapture.BackColor = Color.FromArgb(70, 130, 180);
            btnCapture.ForeColor = Color.White;
            btnCapture.FlatStyle = FlatStyle.Flat;
            btnCapture.Click += BtnCapture_Click;
            pnlEntryData.Controls.Add(btnCapture);
            
            // Generate ticket button
            btnGenerateTicket = new Button();
            btnGenerateTicket.Text = "Generate Ticket";
            btnGenerateTicket.Location = new Point(170, 250);
            btnGenerateTicket.Size = new Size(150, 40);
            btnGenerateTicket.Font = new Font("Segoe UI", 10F);
            btnGenerateTicket.BackColor = Color.FromArgb(46, 139, 87);
            btnGenerateTicket.ForeColor = Color.White;
            btnGenerateTicket.FlatStyle = FlatStyle.Flat;
            btnGenerateTicket.Click += BtnGenerateTicket_Click;
            pnlEntryData.Controls.Add(btnGenerateTicket);
            
            // View Images button
            btnViewImages = new Button();
            btnViewImages.Text = "View Images";
            btnViewImages.Location = new Point(10, 300);
            btnViewImages.Size = new Size(310, 40);
            btnViewImages.Font = new Font("Segoe UI", 10F);
            btnViewImages.BackColor = Color.FromArgb(100, 149, 237);
            btnViewImages.ForeColor = Color.White;
            btnViewImages.FlatStyle = FlatStyle.Flat;
            btnViewImages.Click += BtnViewImages_Click;
            pnlEntryData.Controls.Add(btnViewImages);
            
            // Add to tab layout
            tlpEntry.Controls.Add(pnlEntryCamera, 0, 0);
            tlpEntry.Controls.Add(pnlEntryData, 1, 0);
            
            // Add layout to tab
            entryTab.Controls.Add(tlpEntry);
            
            // Add captured image to camera panel
            pnlEntryCamera.Controls.Add(picCaptured);
        }
        
        private void InitializeExitControls()
        {
            // Create exit layout
            TableLayoutPanel tlpExit = new TableLayoutPanel();
            tlpExit.Dock = DockStyle.Fill;
            tlpExit.ColumnCount = 1;
            tlpExit.RowCount = 2;
            tlpExit.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
            tlpExit.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            
            // Search panel
            Panel pnlSearch = new Panel();
            pnlSearch.Dock = DockStyle.Fill;
            pnlSearch.BorderStyle = BorderStyle.FixedSingle;
            pnlSearch.Margin = new Padding(10);
            pnlSearch.Padding = new Padding(10);
            
            // Ticket number label
            Label lblTicketNumber = new Label();
            lblTicketNumber.Text = "Ticket Number:";
            lblTicketNumber.Location = new Point(20, 20);
            lblTicketNumber.Size = new Size(120, 25);
            lblTicketNumber.Font = new Font("Segoe UI", 10F);
            pnlSearch.Controls.Add(lblTicketNumber);
            
            // Ticket number textbox
            txtTicketNumber = new TextBox();
            txtTicketNumber.Location = new Point(150, 20);
            txtTicketNumber.Size = new Size(250, 30);
            txtTicketNumber.Font = new Font("Segoe UI", 10F);
            pnlSearch.Controls.Add(txtTicketNumber);
            
            // Find ticket button
            btnFindTicket = new Button();
            btnFindTicket.Text = "Find Ticket";
            btnFindTicket.Location = new Point(420, 20);
            btnFindTicket.Size = new Size(120, 40);
            btnFindTicket.Font = new Font("Segoe UI", 10F);
            btnFindTicket.BackColor = Color.FromArgb(70, 130, 180);
            btnFindTicket.ForeColor = Color.White;
            btnFindTicket.FlatStyle = FlatStyle.Flat;
            btnFindTicket.Click += BtnFindTicket_Click;
            pnlSearch.Controls.Add(btnFindTicket);
            
            // Lost ticket button
            btnLostTicket = new Button();
            btnLostTicket.Text = "Lost Ticket";
            btnLostTicket.Location = new Point(550, 20);
            btnLostTicket.Size = new Size(120, 40);
            btnLostTicket.Font = new Font("Segoe UI", 10F);
            btnLostTicket.BackColor = Color.FromArgb(220, 53, 69);
            btnLostTicket.ForeColor = Color.White;
            btnLostTicket.FlatStyle = FlatStyle.Flat;
            btnLostTicket.Click += BtnLostTicket_Click;
            pnlSearch.Controls.Add(btnLostTicket);
            
            // View Exit Images button
            Button btnViewExitImages = new Button();
            btnViewExitImages.Text = "View Exit Images";
            btnViewExitImages.Location = new Point(690, 20);
            btnViewExitImages.Size = new Size(150, 40);
            btnViewExitImages.Font = new Font("Segoe UI", 10F);
            btnViewExitImages.BackColor = Color.FromArgb(100, 149, 237);
            btnViewExitImages.ForeColor = Color.White;
            btnViewExitImages.FlatStyle = FlatStyle.Flat;
            btnViewExitImages.Click += BtnViewImages_Click; // Reuse the same event handler
            pnlSearch.Controls.Add(btnViewExitImages);
            
            // Exit data panel
            pnlExitData = new Panel();
            pnlExitData.Dock = DockStyle.Fill;
            pnlExitData.BorderStyle = BorderStyle.FixedSingle;
            pnlExitData.Margin = new Padding(10);
            pnlExitData.Padding = new Padding(20);
            
            // Ticket information group
            GroupBox gbTicketInfo = new GroupBox();
            gbTicketInfo.Text = "Ticket Information";
            gbTicketInfo.Location = new Point(20, 20);
            gbTicketInfo.Size = new Size(500, 200);
            gbTicketInfo.Font = new Font("Segoe UI", 10F);
            pnlExitData.Controls.Add(gbTicketInfo);
            
            // Entry time
            Label lblEntryTimeTitle = new Label();
            lblEntryTimeTitle.Text = "Entry Time:";
            lblEntryTimeTitle.Location = new Point(20, 40);
            lblEntryTimeTitle.Size = new Size(100, 25);
            lblEntryTimeTitle.Font = new Font("Segoe UI", 10F);
            gbTicketInfo.Controls.Add(lblEntryTimeTitle);
            
            lblEntryTime = new Label();
            lblEntryTime.Location = new Point(150, 40);
            lblEntryTime.Size = new Size(200, 25);
            lblEntryTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gbTicketInfo.Controls.Add(lblEntryTime);
            
            // Exit time
            Label lblExitTimeTitle = new Label();
            lblExitTimeTitle.Text = "Exit Time:";
            lblExitTimeTitle.Location = new Point(20, 80);
            lblExitTimeTitle.Size = new Size(100, 25);
            lblExitTimeTitle.Font = new Font("Segoe UI", 10F);
            gbTicketInfo.Controls.Add(lblExitTimeTitle);
            
            lblExitTime = new Label();
            lblExitTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lblExitTime.Location = new Point(150, 80);
            lblExitTime.Size = new Size(200, 25);
            lblExitTime.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gbTicketInfo.Controls.Add(lblExitTime);
            
            // Duration
            Label lblDurationTitle = new Label();
            lblDurationTitle.Text = "Duration:";
            lblDurationTitle.Location = new Point(20, 120);
            lblDurationTitle.Size = new Size(100, 25);
            lblDurationTitle.Font = new Font("Segoe UI", 10F);
            gbTicketInfo.Controls.Add(lblDurationTitle);
            
            lblDuration = new Label();
            lblDuration.Location = new Point(150, 120);
            lblDuration.Size = new Size(200, 25);
            lblDuration.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gbTicketInfo.Controls.Add(lblDuration);
            
            // Fee
            Label lblFeeTitle = new Label();
            lblFeeTitle.Text = "Fee:";
            lblFeeTitle.Location = new Point(20, 160);
            lblFeeTitle.Size = new Size(100, 25);
            lblFeeTitle.Font = new Font("Segoe UI", 10F);
            gbTicketInfo.Controls.Add(lblFeeTitle);
            
            lblFee = new Label();
            lblFee.Location = new Point(150, 160);
            lblFee.Size = new Size(200, 25);
            lblFee.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gbTicketInfo.Controls.Add(lblFee);
            
            // Process exit button
            btnProcessExit = new Button();
            btnProcessExit.Text = "Process Exit";
            btnProcessExit.Location = new Point(300, 240);
            btnProcessExit.Size = new Size(200, 50);
            btnProcessExit.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            btnProcessExit.BackColor = Color.FromArgb(46, 139, 87);
            btnProcessExit.ForeColor = Color.White;
            btnProcessExit.FlatStyle = FlatStyle.Flat;
            btnProcessExit.Click += BtnProcessExit_Click;
            btnProcessExit.Enabled = false;
            pnlExitData.Controls.Add(btnProcessExit);
            
            // Add to tab layout
            tlpExit.Controls.Add(pnlSearch, 0, 0);
            tlpExit.Controls.Add(pnlExitData, 0, 1);
            
            // Add layout to tab
            exitTab.Controls.Add(tlpExit);
        }
        
        private void CombinedEntryExitForm_Load(object sender, EventArgs e)
        {
            // Load vehicle types for entry tab
            LoadVehicleTypes();
            
            // Initialize camera
            InitializeCamera();
            
            // Set focus to plate number
            txtPlateNumber.Focus();
            
            // Initialize gate control
            InitializeGateControl();
        }
        
        #region Entry Tab Methods
        
        private void LoadVehicleTypes()
        {
            try
            {
                // Get vehicle types from database
                DataTable dt = Database.GetData("SELECT * FROM kendaraan ORDER BY jenis_kendaraan");
                cmbVehicleType.DataSource = dt;
                cmbVehicleType.DisplayMember = "jenis_kendaraan";
                cmbVehicleType.ValueMember = "id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vehicle types: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void InitializeCamera()
        {
            try
            {
                // Get all available video devices
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (videoDevices.Count > 0)
                {
                    // Use the first camera
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    videoSource.NewFrame += VideoSource_NewFrame;
                    videoSource.Start();
                }
                else
                {
                    MessageBox.Show("No camera found", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing camera: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Display the current frame
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            picCamera.Image = bitmap;
        }
        
        private void BtnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                if (picCamera.Image != null)
                {
                    // For now just store temporary image - we'll save properly with ENT prefix when ticket is generated
                    string tempFileName = $"TEMP_{DateTime.Now:yyyyMMddHHmmss}_{txtPlateNumber.Text}.jpg";
                    entryImagePath = Path.Combine(entryImagesPath, tempFileName);
                    picCamera.Image.Save(entryImagePath);
                    
                    // Display the captured image
                    picCaptured.Image = (Bitmap)picCamera.Image.Clone();
                    
                    MessageBox.Show("Gambar berhasil di-capture", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat mengambil gambar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void BtnGenerateTicket_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtPlateNumber.Text))
                {
                    MessageBox.Show("Harap masukkan nomor plat", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (cmbVehicleType.SelectedIndex == -1)
                {
                    MessageBox.Show("Harap pilih jenis kendaraan", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (string.IsNullOrEmpty(entryImagePath))
                {
                    MessageBox.Show("Harap ambil gambar terlebih dahulu", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Generate ticket number
                string ticketNumber = $"T{DateTime.Now:yyyyMMddHHmmss}";
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                
                // Rename and save image with proper format: ENT-{ticketNumber}-{timestamp}.jpg
                string finalImageFileName = $"ENT-{ticketNumber}-{timestamp}.jpg";
                string finalImagePath = Path.Combine(entryImagesPath, finalImageFileName);
                
                // Check if we have a temporary image, if so, move/rename it
                if (File.Exists(entryImagePath))
                {
                    File.Copy(entryImagePath, finalImagePath, true);
                    File.Delete(entryImagePath); // Delete the temporary image
                    entryImagePath = finalImagePath;
                }
                
                // Generate barcode
                BarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>();
                writer.Format = BarcodeFormat.QR_CODE;
                writer.Options = new EncodingOptions
                {
                    Width = 300,
                    Height = 300,
                    Margin = 0
                };
                
                Bitmap barcodeBitmap = writer.Write(ticketNumber);
                
                // Save barcode with proper naming format
                string barcodeFileName = $"BARCODE-{ticketNumber}-{timestamp}.png";
                barcodeImagePath = Path.Combine(entryImagesPath, barcodeFileName);
                barcodeBitmap.Save(barcodeImagePath);
                
                // Get selected vehicle type
                DataTable dt = cmbVehicleType.DataSource as DataTable;
                DataRowView selectedRow = cmbVehicleType.SelectedItem as DataRowView;
                int vehicleTypeId = Convert.ToInt32(selectedRow["id"]);
                string vehicleType = selectedRow["jenis_kendaraan"].ToString();
                
                // Insert entry record into shared database
                string query = "INSERT INTO parking_log (ticket_number, plate_number, vehicle_type_id, entry_time, image_path, barcode_path, operator_id) " +
                               "VALUES (@ticket_number, @plate_number, @vehicle_type_id, @entry_time, @image_path, @barcode_path, @operator_id)";
                
                Database.ExecuteNonQuery(query, new Dictionary<string, object>
                {
                    { "@ticket_number", ticketNumber },
                    { "@plate_number", txtPlateNumber.Text },
                    { "@vehicle_type_id", vehicleTypeId },
                    { "@entry_time", DateTime.Now },
                    { "@image_path", entryImagePath },
                    { "@barcode_path", barcodeImagePath },
                    { "@operator_id", _currentUser.UserId }
                });
                
                // Print ticket
                PrintTicket(ticketNumber, txtPlateNumber.Text, vehicleType, DateTime.Now);
                
                // Clear fields for next entry
                txtPlateNumber.Text = "";
                cmbVehicleType.SelectedIndex = -1;
                picCaptured.Image = null;
                entryImagePath = null;
                barcodeImagePath = null;
                
                MessageBox.Show($"Tiket {ticketNumber} berhasil dibuat", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Set focus to plate number
                txtPlateNumber.Focus();
                
                _logger.Information("Vehicle entry processed successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat membuat tiket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void PrintTicket(string ticketNumber, string plateNumber, string vehicleType, DateTime entryTime)
        {
            try
            {
                // Print ticket implementation would go here
                // This is just a placeholder
                
                // In a real implementation, you would use a thermal printer library
                // to print the ticket with the QR code, etc.
                
                // For now, we'll just log it
                _logger.Information($"Ticket printed: {ticketNumber} for {plateNumber}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing ticket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error("Error printing ticket", ex);
            }
        }
        
        #endregion
        
        #region Exit Tab Methods
        
        private void InitializeGateControl()
        {
            try
            {
                // Read gate configuration
                if (File.Exists(gateConfigPath))
                {
                    string[] lines = File.ReadAllLines(gateConfigPath);
                    foreach (string line in lines)
                    {
                        if (line.StartsWith("COM="))
                        {
                            string comPort = line.Substring(4).Trim();
                            gateControlPort = new SerialPort(comPort, 9600, Parity.None, 8, StopBits.One);
                            gateControlPort.ReadTimeout = 500;
                            gateControlPort.WriteTimeout = 500;
                            
                            try
                            {
                                gateControlPort.Open();
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Could not open COM port for gate control: {ex.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                _logger.Error(ex.Message);
                            }
                            
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing gate control: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void BtnFindTicket_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtTicketNumber.Text))
                {
                    MessageBox.Show("Please enter a ticket number", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Find ticket in database
                string query = "SELECT p.*, k.jenis_kendaraan, k.tarif_per_jam, k.tarif_flat " +
                               "FROM parking_log p " +
                               "INNER JOIN kendaraan k ON p.vehicle_type_id = k.id " +
                               "WHERE p.ticket_number = @ticket_number AND p.exit_time IS NULL";
                
                DataTable dt = Database.GetData(query, new Dictionary<string, object>
                {
                    { "@ticket_number", txtTicketNumber.Text }
                });
                
                if (dt.Rows.Count > 0)
                {
                    // Get ticket information
                    DataRow row = dt.Rows[0];
                    DateTime entryTime = Convert.ToDateTime(row["entry_time"]);
                    DateTime exitTime = DateTime.Now;
                    double tarifPerJam = Convert.ToDouble(row["tarif_per_jam"]);
                    double tarifFlat = Convert.ToDouble(row["tarif_flat"]);
                    
                    // Calculate duration
                    TimeSpan duration = exitTime - entryTime;
                    int hours = (int)Math.Ceiling(duration.TotalHours);
                    if (hours == 0) hours = 1; // Minimum 1 hour
                    
                    // Calculate fee
                    double fee = 0;
                    if (tarifFlat > 0)
                    {
                        fee = tarifFlat;
                    }
                    else
                    {
                        fee = hours * tarifPerJam;
                    }
                    
                    // Display information
                    lblEntryTime.Text = entryTime.ToString("yyyy-MM-dd HH:mm:ss");
                    lblExitTime.Text = exitTime.ToString("yyyy-MM-dd HH:mm:ss");
                    lblDuration.Text = $"{duration.Days} days, {duration.Hours} hours, {duration.Minutes} minutes";
                    lblFee.Text = $"Rp {fee:N0}";
                    
                    // Enable process exit button
                    btnProcessExit.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Ticket not found or already processed", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearExitData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error finding ticket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void BtnLostTicket_Click(object sender, EventArgs e)
        {
            try
            {
                // Show lost ticket form
                // This would be implemented in a real application
                MessageBox.Show("This would show a lost ticket form for processing", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error with lost ticket: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void BtnProcessExit_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtTicketNumber.Text))
                {
                    MessageBox.Show("Harap masukkan nomor tiket", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string ticketNumber = txtTicketNumber.Text;
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                
                // Capture exit image if camera is available
                string exitImagePath = null;
                if (videoSource != null && videoSource.IsRunning && picCamera.Image != null)
                {
                    // Save exit image with proper naming format: EXIT-{ticketNumber}-{timestamp}.jpg
                    string exitImageFileName = $"EXIT-{ticketNumber}-{timestamp}.jpg";
                    exitImagePath = Path.Combine(exitImagesPath, exitImageFileName);
                    
                    try
                    {
                        // Clone the current camera image and save it
                        using (Bitmap exitImage = (Bitmap)picCamera.Image.Clone())
                        {
                            exitImage.Save(exitImagePath);
                            _logger.Information($"Gambar exit disimpan: {exitImagePath}");
                        }
                    }
                    catch (Exception imgEx)
                    {
                        _logger.Error("Error saat menyimpan gambar exit", imgEx);
                        // Continue processing even if image saving fails
                    }
                }
                
                // Update exit time in shared database
                string query = "UPDATE parking_log SET exit_time = @exit_time, exit_operator_id = @operator_id";
                
                // Add exit image path if available
                if (!string.IsNullOrEmpty(exitImagePath))
                {
                    query += ", exit_image_path = @exit_image_path";
                }
                
                query += " WHERE ticket_number = @ticket_number";
                
                var parameters = new Dictionary<string, object>
                {
                    { "@ticket_number", ticketNumber },
                    { "@exit_time", DateTime.Now },
                    { "@operator_id", _currentUser.UserId }
                };
                
                // Add exit image path parameter if available
                if (!string.IsNullOrEmpty(exitImagePath))
                {
                    parameters.Add("@exit_image_path", exitImagePath);
                }
                
                Database.ExecuteNonQuery(query, parameters);
                
                // Open gate
                if (gateControlPort != null && gateControlPort.IsOpen)
                {
                    gateControlPort.Write("OPEN\r\n");
                }
                
                MessageBox.Show("Proses exit berhasil", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Clear fields for next exit
                ClearExitData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat proses exit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }
        
        private void ClearExitData()
        {
            txtTicketNumber.Text = "";
            lblEntryTime.Text = "";
            lblExitTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lblDuration.Text = "";
            lblFee.Text = "";
            btnProcessExit.Enabled = false;
        }
        
        #endregion
        
        private void CombinedEntryExitForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop video source
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
            
            // Close serial port
            if (gateControlPort != null && gateControlPort.IsOpen)
            {
                gateControlPort.Close();
            }
        }

        private void BtnViewImages_Click(object sender, EventArgs e)
        {
            try
            {
                // Determine which images folder to open based on active tab
                string imagesPath = tabControl.SelectedTab == entryTab ? entryImagesPath : exitImagesPath;
                
                // Check if directory exists
                if (Directory.Exists(imagesPath))
                {
                    // Open folder in explorer
                    Process.Start("explorer.exe", imagesPath);
                }
                else
                {
                    MessageBox.Show("Image directory not found", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening images folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _logger.Error(ex.Message);
            }
        }

        private void ValidateUserAccess(string username, string password)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "username", username },
                    { "password", password }
                };
                
                // Change from ExecuteQuery to ExecuteQueryWithParams
                DataTable dt = Database.ExecuteQueryWithParams("SELECT * FROM users", parameters);
                
                // ... existing code ...
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in CombinedEntryExitForm: {ex.Message}");
                _logger.Error($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 