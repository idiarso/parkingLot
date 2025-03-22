using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Common;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    public partial class EntryForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;
        private VideoCaptureDevice videoSource;
        private string imagePath;
        private string barcodeImagePath;
        private readonly string entryImagesPath;
        private PictureBox pictureBox;
        private TextBox txtPlateNumber;
        private ComboBox cmbVehicleType;
        private Button btnCapture;
        private Button btnGenerate;
        private Button btnSave;
        private Button btnCancel;
        private Panel pnlCamera;
        private Panel pnlDetails;
        private PictureBox picVehicle;
        private Label lblStatus;
        private Label lblPlateNumber;
        private Label lblVehicleType;
        private DataGridView dgvEntry;
        private Button btnClear;

        public EntryForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = CustomLogManager.GetLogger();
            InitializeComponent();
            InitializeControls();
            LoadVehicleTypes();
            this.Text = "Vehicle Entry";
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Create directory for storing images
            entryImagesPath = Path.Combine(Application.StartupPath, "Images", "Entry");
            if (!Directory.Exists(entryImagesPath))
            {
                Directory.CreateDirectory(entryImagesPath);
            }
        }

        private void InitializeControls()
        {
            try
            {
                // Camera Panel
                pnlCamera = new Panel
                {
                    Location = new Point(12, 12),
                    Size = new Size(400, 300),
                    BorderStyle = BorderStyle.FixedSingle
                };

                pictureBox = new PictureBox
                {
                    Dock = DockStyle.Fill,
                    SizeMode = PictureBoxSizeMode.Zoom
                };
                pnlCamera.Controls.Add(pictureBox);

                // Entry Details Panel
                pnlDetails = new Panel
                {
                    Location = new Point(418, 12),
                    Size = new Size(300, 300),
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Vehicle Type
                lblVehicleType = new Label
                {
                    Text = "Vehicle Type:",
                    Location = new Point(10, 15),
                    AutoSize = true
                };

                cmbVehicleType = new ComboBox
                {
                    Location = new Point(10, 35),
                    Size = new Size(280, 23),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };

                // Plate Number
                lblPlateNumber = new Label
                {
                    Text = "Plate Number:",
                    Location = new Point(10, 65),
                    AutoSize = true
                };

                txtPlateNumber = new TextBox
                {
                    Location = new Point(10, 85),
                    Size = new Size(280, 23)
                };

                // Picture Box
                picVehicle = new PictureBox
                {
                    Location = new Point(350, 20),
                    Size = new Size(400, 300),
                    BorderStyle = BorderStyle.FixedSingle,
                    SizeMode = PictureBoxSizeMode.Zoom
                };

                // Buttons
                btnCapture = new Button
                {
                    Text = "Capture Photo",
                    Location = new Point(350, 330),
                    Size = new Size(120, 35)
                };
                btnCapture.Click += BtnCapture_Click;

                btnGenerate = new Button
                {
                    Text = "Generate Ticket",
                    Location = new Point(480, 330),
                    Size = new Size(120, 35),
                    Enabled = false
                };
                btnGenerate.Click += BtnGenerate_Click;

                btnSave = new Button
                {
                    Text = "Save",
                    Location = new Point(610, 330),
                    Size = new Size(120, 35),
                    Enabled = false
                };
                btnSave.Click += BtnSave_Click;

                btnCancel = new Button
                {
                    Text = "Cancel",
                    Location = new Point(130, 330),
                    Size = new Size(120, 35)
                };
                btnCancel.Click += BtnCancel_Click;

                // Status Label
                lblStatus = new Label
                {
                    Location = new Point(20, 380),
                    Size = new Size(730, 25),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                // Add controls to details panel
                pnlDetails.Controls.AddRange(new Control[] {
                    lblVehicleType, cmbVehicleType,
                    lblPlateNumber, txtPlateNumber,
                    picVehicle,
                    btnCapture, btnGenerate, btnSave, btnCancel,
                    lblStatus
                });

                // Add panels to form
                this.Controls.AddRange(new Control[] { pnlCamera, pnlDetails });
                this.Size = new Size(740, 360);

                InitializeCamera();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing controls: {ex.Message}");
                MessageBox.Show("Failed to initialize form controls.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeCamera()
        {
            try
            {
                // Get all available video devices
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (videoDevices.Count > 0)
                {
                    // Use the first available camera
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    videoSource.NewFrame += VideoSource_NewFrame;
                    videoSource.Start();
                    
                    btnCapture.Enabled = true;
                }
                else
                {
                    btnCapture.Enabled = false;
                    pictureBox.Image = new Bitmap(pictureBox.Width, pictureBox.Height);
                    using (Graphics g = Graphics.FromImage(pictureBox.Image))
                    {
                        g.FillRectangle(Brushes.Black, 0, 0, pictureBox.Width, pictureBox.Height);
                        g.DrawString("No camera detected", new Font("Arial", 10), Brushes.White, 
                            new PointF(pictureBox.Width / 2 - 50, pictureBox.Height / 2 - 5));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing camera: {ex.Message}");
                MessageBox.Show("Failed to initialize camera. Please check camera connection.", 
                    "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                btnCapture.Enabled = false;
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Create a copy of the current frame
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            
            if (pictureBox.InvokeRequired)
            {
                pictureBox.Invoke(new Action(() => {
                    // Dispose of the previous image to avoid memory leaks
                    if (pictureBox.Image != null)
                    {
                        var temp = pictureBox.Image;
                        pictureBox.Image = bitmap;
                        temp.Dispose();
                    }
                    else
                    {
                        pictureBox.Image = bitmap;
                    }
                }));
            }
            else
            {
                if (pictureBox.Image != null)
                {
                    var temp = pictureBox.Image;
                    pictureBox.Image = bitmap;
                    temp.Dispose();
                }
                else
                {
                    pictureBox.Image = bitmap;
                }
            }
        }

        private void BtnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                if (pictureBox.Image == null)
                {
                    MessageBox.Show("No image to capture.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Generate unique filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string plateNumber = txtPlateNumber.Text.Trim().ToUpper();
                string filename = $"entry_{timestamp}_{plateNumber}.jpg";
                imagePath = Path.Combine(entryImagesPath, filename);

                // Save the image
                pictureBox.Image.Save(imagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                _logger.Information($"Captured image saved: {imagePath}");

                // Update status
                lblStatus.Text = $"Image captured and saved: {filename}";
                btnGenerate.Enabled = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error capturing image: {ex.Message}");
                MessageBox.Show("Failed to capture image.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVehicleTypes()
        {
            try
            {
                // PostgreSQL uses TRUE instead of integer 1
                string query = "SELECT DISTINCT jenis_kendaraan FROM t_tarif WHERE status = true ORDER BY jenis_kendaraan";
                DataTable dt = Database.ExecuteQuery(query);
                cmbVehicleType.Items.Clear();
                
                foreach (DataRow row in dt.Rows)
                {
                    cmbVehicleType.Items.Add(row["jenis_kendaraan"].ToString());
                }
                
                // Select first item if available
                if (cmbVehicleType.Items.Count > 0)
                {
                    cmbVehicleType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load vehicle types", ex);
                MessageBox.Show("Error loading vehicle types: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(imagePath))
                {
                    MessageBox.Show("Please capture an image first.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Generate QR code with entry details
                string entryDetails = $"ENTRY|{txtPlateNumber.Text}|{cmbVehicleType.Text}|{DateTime.Now:yyyyMMddHHmmss}";
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string plateNumber = txtPlateNumber.Text.Trim().ToUpper();
                string filename = $"barcode_{timestamp}_{plateNumber}.jpg";
                barcodeImagePath = Path.Combine(entryImagesPath, filename);

                using (Bitmap qrCode = GenerateQRCode(entryDetails, 200, 200))
                {
                    qrCode.Save(barcodeImagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                // Display the QR code
                picVehicle.Image = Image.FromFile(barcodeImagePath);
                _logger.Information($"Generated QR code: {barcodeImagePath}");

                // Update status
                lblStatus.Text = "QR code generated successfully.";
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generating QR code: {ex.Message}");
                MessageBox.Show("Failed to generate QR code.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateEntry())
                {
                    return;
                }

                string query = @"
                    INSERT INTO t_parkir (
                        nomor_polisi, jenis_kendaraan, waktu_masuk, 
                        created_by, status
                    ) VALUES (
                        @plateNumber, @vehicleType, NOW(), 
                        @createdBy, 1
                    )";

                var parameters = new Dictionary<string, object>
                {
                    { "@plateNumber", txtPlateNumber.Text.Trim().ToUpper() },
                    { "@vehicleType", cmbVehicleType.Text },
                    { "@createdBy", _currentUser.Id }
                };

                Database.ExecuteNonQuery(query, parameters);
                _logger.Information($"Vehicle entry saved: {txtPlateNumber.Text}");

                // Clear form and reset status
                ClearForm();
                lblStatus.Text = "Vehicle entry saved successfully.";
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving vehicle entry: {ex.Message}");
                MessageBox.Show("Failed to save vehicle entry.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool ValidateEntry()
        {
            if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
            {
                MessageBox.Show("Please enter a plate number.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (cmbVehicleType.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select a vehicle type.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                MessageBox.Show("Please capture a vehicle image.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(barcodeImagePath))
            {
                MessageBox.Show("Please generate a QR code.", "Validation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            txtPlateNumber.Clear();
            cmbVehicleType.SelectedIndex = 0;
            pictureBox.Image = null;
            picVehicle.Image = null;
            imagePath = null;
            barcodeImagePath = null;
            btnGenerate.Enabled = false;
            btnSave.Enabled = false;
            lblStatus.Text = string.Empty;
        }

        private void CloseCamera()
        {
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            CloseCamera();
        }

        private void txtPlateNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only letters, numbers, and control characters
            if (!char.IsLetterOrDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // EntryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 360);
            this.Name = "EntryForm";
            this.Text = "Vehicle Entry";
            this.ResumeLayout(false);
        }

        private Bitmap GenerateQRCode(string content, int width, int height)
        {
            var writer = new BarcodeWriter<Bitmap>
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 0
                }
            };

            return writer.Write(content);
        }
    }
} 