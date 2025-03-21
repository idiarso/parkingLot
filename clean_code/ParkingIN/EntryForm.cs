#nullable enable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Collections.Generic;
using ParkingIN.Models;
using ParkingIN.Utils;

namespace ParkingIN
{
    public partial class EntryForm : Form
    {
        private readonly User _currentUser;
        private VideoCaptureDevice? videoSource;
        private string? imagePath;
        private readonly string entryImagesPath;
        
        private System.ComponentModel.Container? components = null;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        
        private void InitializeComponent()
        {
            this.components = new();
            this.SuspendLayout();
            // 
            // EntryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Name = "EntryForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parking IN - Vehicle Entry";
            this.Load += new System.EventHandler(this.EntryForm_Load);
            this.ResumeLayout(false);
        }
        
        public EntryForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            
            // Create directory for storing images
            entryImagesPath = Path.Combine(Application.StartupPath, "Images", "Entry");
            if (!Directory.Exists(entryImagesPath))
            {
                Directory.CreateDirectory(entryImagesPath);
            }

            this.Text = "Parking IN - Vehicle Entry";
            this.StartPosition = FormStartPosition.CenterScreen;
            InitializeControls();
        }

        private PictureBox picCamera;
        private Button btnCapture;
        private TextBox txtPlateNumber;
        private ComboBox cmbVehicleType;
        private Button btnGenerateTicket;

        private void InitializeControls()
        {
            // Initialize form controls
            this.Size = new Size(800, 600);
            
            // Camera panel
            Panel cameraPanel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(320, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            picCamera = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            cameraPanel.Controls.Add(picCamera);
            
            btnCapture = new Button
            {
                Text = "Capture",
                Location = new Point(10, 260),
                Size = new Size(320, 30)
            };
            btnCapture.Click += BtnCapture_Click;
            
            // Entry details panel
            Panel detailsPanel = new Panel
            {
                Location = new Point(340, 10),
                Size = new Size(440, 240),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            Label lblPlateNumber = new Label
            {
                Text = "Plate Number:",
                Location = new Point(10, 20),
                Size = new Size(100, 20)
            };
            
            txtPlateNumber = new TextBox
            {
                Location = new Point(120, 20),
                Size = new Size(200, 20)
            };
            
            Label lblVehicleType = new Label
            {
                Text = "Vehicle Type:",
                Location = new Point(10, 50),
                Size = new Size(100, 20)
            };
            
            cmbVehicleType = new ComboBox
            {
                Location = new Point(120, 50),
                Size = new Size(200, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            
            btnGenerateTicket = new Button
            {
                Text = "Generate Ticket",
                Location = new Point(120, 90),
                Size = new Size(200, 30)
            };
            btnGenerateTicket.Click += BtnGenerateTicket_Click;
            
            detailsPanel.Controls.AddRange(new Control[] {
                lblPlateNumber, txtPlateNumber,
                lblVehicleType, cmbVehicleType,
                btnGenerateTicket
            });
            
            // Add all controls to form
            this.Controls.AddRange(new Control[] {
                cameraPanel,
                btnCapture,
                detailsPanel
            });
        }

        private void EntryForm_Load(object sender, EventArgs e)
        {
            LoadVehicleTypes();
            InitializeCamera();
            txtPlateNumber.Focus();
        }
        
        private void InitializeCamera()
        {
            try
            {
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (videoDevices.Count > 0)
                {
                    videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                    videoSource.NewFrame += VideoSource_NewFrame;
                    videoSource.Start();
                    btnCapture.Enabled = true;
                }
                else
                {
                    btnCapture.Enabled = false;
                    picCamera.Image = new Bitmap(picCamera.Width, picCamera.Height);
                    using (Graphics g = Graphics.FromImage(picCamera.Image))
                    {
                        g.FillRectangle(Brushes.Black, 0, 0, picCamera.Width, picCamera.Height);
                        g.DrawString("No camera detected", new Font("Arial", 10), Brushes.White, 
                            new PointF(picCamera.Width / 2 - 50, picCamera.Height / 2 - 5));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error initializing camera");
                MessageBox.Show($"Error initializing camera: {ex.Message}", 
                    "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnCapture.Enabled = false;
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            
            if (picCamera.InvokeRequired)
            {
                picCamera.Invoke(new Action(() => UpdateCameraImage(bitmap)));
            }
            else
            {
                UpdateCameraImage(bitmap);
            }
        }

        private void UpdateCameraImage(Bitmap bitmap)
        {
            if (picCamera.Image != null)
            {
                var temp = picCamera.Image;
                picCamera.Image = bitmap;
                temp.Dispose();
            }
            else
            {
                picCamera.Image = bitmap;
            }
        }
        
        private void BtnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                if (videoSource != null && videoSource.IsRunning)
                {
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    imagePath = Path.Combine(entryImagesPath, $"capture_{timestamp}.jpg");
                    
                    if (picCamera.Image != null)
                    {
                        using Bitmap capture = new(picCamera.Image);
                        {
                            capture.Save(imagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                            MessageBox.Show("Image captured successfully!", 
                                "Capture", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Camera is not running. Cannot capture image.", 
                        "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error capturing image");
                MessageBox.Show($"Error capturing image: {ex.Message}", 
                    "Capture Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadVehicleTypes()
        {
            try
            {
                cmbVehicleType.Items.Clear();
                DataTable vehicleTypes = ParkingIN.Utils.Database.GetData("SELECT VehicleType FROM VehicleTypes ORDER BY VehicleType");
                foreach (DataRow row in vehicleTypes.Rows)
                {
                    cmbVehicleType.Items.Add(row["VehicleType"]);
                }

                if (cmbVehicleType.Items.Count > 0)
                {
                    cmbVehicleType.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("No vehicle types found. Please configure vehicle types in settings.", 
                        "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading vehicle types");
                MessageBox.Show($"Error loading vehicle types: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerateTicket_Click(object sender, EventArgs e)
        {
            try
            {
                string plateNumber = txtPlateNumber.Text.Trim().ToUpper();
                string vehicleType = cmbVehicleType.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(plateNumber))
                {
                    MessageBox.Show("Please enter a plate number.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPlateNumber.Focus();
                    return;
                }

                if (string.IsNullOrEmpty(vehicleType))
                {
                    MessageBox.Show("Please select a vehicle type.", "Validation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbVehicleType.Focus();
                    return;
                }

                var parameters = new Dictionary<string, object>
                {
                    { "@PlateNumber", plateNumber }
                };

                var existingVehicle = ParkingIN.Utils.Database.ExecuteScalar(
                    "SELECT COUNT(*) FROM Vehicles WHERE PlateNumber = @PlateNumber AND ExitTime IS NULL",
                    parameters
                );

                if (Convert.ToInt32(existingVehicle) > 0)
                {
                    MessageBox.Show("This vehicle is already in the parking area.", 
                        "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                RegisterEntry(plateNumber, vehicleType);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error generating ticket");
                MessageBox.Show($"Error generating ticket: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RegisterEntry(string plateNumber, string vehicleType)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                var parameters = new Dictionary<string, object>
                {
                    { "@PlateNumber", plateNumber },
                    { "@VehicleType", vehicleType },
                    { "@EntryTime", DateTime.Now },
                    { "@ImagePath", imagePath ?? ""},
                    { "@UserId", _currentUser.UserId }
                };

                int entryId = Convert.ToInt32(ParkingIN.Utils.Database.ExecuteScalar(@"
                    INSERT INTO Vehicles (PlateNumber, VehicleType, EntryTime, ImagePath, EntryUserId)
                    VALUES (@PlateNumber, @VehicleType, @EntryTime, @ImagePath, @UserId);
                    SELECT LAST_INSERT_ID();",
                    parameters
                ));

                MessageBox.Show($"Vehicle entry registered successfully!\nTicket Number: {entryId}", 
                    "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Clear form for next entry
                txtPlateNumber.Clear();
                if (cmbVehicleType.Items.Count > 0)
                    cmbVehicleType.SelectedIndex = 0;
                imagePath = null;
                txtPlateNumber.Focus();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error registering vehicle entry");
                MessageBox.Show($"Error registering vehicle entry: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        // Form controls

    }
}