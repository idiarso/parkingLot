using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleParkingAdmin
{
    public partial class IPCameraSettingsForm : Form
    {
        private TabControl tabCameras;
        private TabPage tabEntryCamera;
        private TabPage tabExitCamera;
        private PictureBox pbEntryCameraPreview;
        private PictureBox pbExitCameraPreview;
        private Button btnSave;
        private Button btnCancel;
        private Button btnTestEntryCamera;
        private Button btnTestExitCamera;
        
        // Entry Camera Controls
        private TextBox txtEntryCameraURL;
        private TextBox txtEntryCameraUsername;
        private TextBox txtEntryCameraPassword;
        private ComboBox cmbEntryCameraType;
        
        // Exit Camera Controls
        private TextBox txtExitCameraURL;
        private TextBox txtExitCameraUsername;
        private TextBox txtExitCameraPassword;
        private ComboBox cmbExitCameraType;
        
        // Timer for camera preview updates
        private System.Windows.Forms.Timer previewTimer;
        
        public IPCameraSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
            StartPreviewTimer();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Pengaturan IP Kamera";
            this.Size = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            
            // Create tab control
            tabCameras = new TabControl();
            tabCameras.Location = new Point(12, 12);
            tabCameras.Size = new Size(660, 400);
            tabCameras.Dock = DockStyle.None;
            
            // Create entry camera tab
            tabEntryCamera = new TabPage("Kamera Masuk");
            tabEntryCamera.BackColor = Color.White;
            
            // Create exit camera tab
            tabExitCamera = new TabPage("Kamera Keluar");
            tabExitCamera.BackColor = Color.White;
            
            // Add tabs to tab control
            tabCameras.TabPages.Add(tabEntryCamera);
            tabCameras.TabPages.Add(tabExitCamera);
            
            // Initialize Entry Camera tab controls
            InitializeEntryCameraTab();
            
            // Initialize Exit Camera tab controls
            InitializeExitCameraTab();
            
            // Create Save and Cancel buttons
            btnSave = new Button();
            btnSave.Text = "Simpan";
            btnSave.Location = new Point(482, 420);
            btnSave.Size = new Size(90, 35);
            btnSave.Click += new EventHandler(btnSave_Click);
            ApplyModernButtonStyle(btnSave);
            
            btnCancel = new Button();
            btnCancel.Text = "Batal";
            btnCancel.Location = new Point(582, 420);
            btnCancel.Size = new Size(90, 35);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            ApplyModernButtonStyle(btnCancel);
            
            // Add controls to form
            this.Controls.Add(tabCameras);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
            
            // Initialize preview timer
            previewTimer = new System.Windows.Forms.Timer();
            previewTimer.Interval = 2000; // Update every 2 seconds
            previewTimer.Tick += new EventHandler(previewTimer_Tick);
        }
        
        private void InitializeEntryCameraTab()
        {
            // Camera URL
            Label lblEntryCameraURL = new Label();
            lblEntryCameraURL.Text = "URL Kamera:";
            lblEntryCameraURL.Location = new Point(15, 20);
            lblEntryCameraURL.AutoSize = true;
            
            txtEntryCameraURL = new TextBox();
            txtEntryCameraURL.Location = new Point(120, 17);
            txtEntryCameraURL.Size = new Size(350, 23);
            txtEntryCameraURL.PlaceholderText = "http://192.168.1.100:8080/video";
            
            // Camera Type
            Label lblEntryCameraType = new Label();
            lblEntryCameraType.Text = "Tipe Kamera:";
            lblEntryCameraType.Location = new Point(15, 50);
            lblEntryCameraType.AutoSize = true;
            
            cmbEntryCameraType = new ComboBox();
            cmbEntryCameraType.Location = new Point(120, 47);
            cmbEntryCameraType.Size = new Size(200, 23);
            cmbEntryCameraType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEntryCameraType.Items.AddRange(new string[] {
                "IP Camera (MJPEG)",
                "IP Camera (RTSP)",
                "USB Webcam",
                "Smartphone Camera"
            });
            
            // Username
            Label lblEntryCameraUsername = new Label();
            lblEntryCameraUsername.Text = "Username:";
            lblEntryCameraUsername.Location = new Point(15, 80);
            lblEntryCameraUsername.AutoSize = true;
            
            txtEntryCameraUsername = new TextBox();
            txtEntryCameraUsername.Location = new Point(120, 77);
            txtEntryCameraUsername.Size = new Size(200, 23);
            
            // Password
            Label lblEntryCameraPassword = new Label();
            lblEntryCameraPassword.Text = "Password:";
            lblEntryCameraPassword.Location = new Point(15, 110);
            lblEntryCameraPassword.AutoSize = true;
            
            txtEntryCameraPassword = new TextBox();
            txtEntryCameraPassword.Location = new Point(120, 107);
            txtEntryCameraPassword.Size = new Size(200, 23);
            txtEntryCameraPassword.PasswordChar = '*';
            
            // Test button
            btnTestEntryCamera = new Button();
            btnTestEntryCamera.Text = "Test Kamera";
            btnTestEntryCamera.Location = new Point(120, 145);
            btnTestEntryCamera.Size = new Size(100, 30);
            btnTestEntryCamera.Click += new EventHandler(btnTestEntryCamera_Click);
            ApplyModernButtonStyle(btnTestEntryCamera);
            
            // Preview label
            Label lblEntryCameraPreview = new Label();
            lblEntryCameraPreview.Text = "Preview Kamera:";
            lblEntryCameraPreview.Location = new Point(15, 190);
            lblEntryCameraPreview.AutoSize = true;
            
            // Preview picture box
            pbEntryCameraPreview = new PictureBox();
            pbEntryCameraPreview.Location = new Point(120, 190);
            pbEntryCameraPreview.Size = new Size(320, 240);
            pbEntryCameraPreview.BorderStyle = BorderStyle.FixedSingle;
            pbEntryCameraPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pbEntryCameraPreview.BackColor = Color.Black;
            
            // Add controls to tab
            tabEntryCamera.Controls.Add(lblEntryCameraURL);
            tabEntryCamera.Controls.Add(txtEntryCameraURL);
            tabEntryCamera.Controls.Add(lblEntryCameraType);
            tabEntryCamera.Controls.Add(cmbEntryCameraType);
            tabEntryCamera.Controls.Add(lblEntryCameraUsername);
            tabEntryCamera.Controls.Add(txtEntryCameraUsername);
            tabEntryCamera.Controls.Add(lblEntryCameraPassword);
            tabEntryCamera.Controls.Add(txtEntryCameraPassword);
            tabEntryCamera.Controls.Add(btnTestEntryCamera);
            tabEntryCamera.Controls.Add(lblEntryCameraPreview);
            tabEntryCamera.Controls.Add(pbEntryCameraPreview);
        }
        
        private void InitializeExitCameraTab()
        {
            // Camera URL
            Label lblExitCameraURL = new Label();
            lblExitCameraURL.Text = "URL Kamera:";
            lblExitCameraURL.Location = new Point(15, 20);
            lblExitCameraURL.AutoSize = true;
            
            txtExitCameraURL = new TextBox();
            txtExitCameraURL.Location = new Point(120, 17);
            txtExitCameraURL.Size = new Size(350, 23);
            txtExitCameraURL.PlaceholderText = "http://192.168.1.101:8080/video";
            
            // Camera Type
            Label lblExitCameraType = new Label();
            lblExitCameraType.Text = "Tipe Kamera:";
            lblExitCameraType.Location = new Point(15, 50);
            lblExitCameraType.AutoSize = true;
            
            cmbExitCameraType = new ComboBox();
            cmbExitCameraType.Location = new Point(120, 47);
            cmbExitCameraType.Size = new Size(200, 23);
            cmbExitCameraType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbExitCameraType.Items.AddRange(new string[] {
                "IP Camera (MJPEG)",
                "IP Camera (RTSP)",
                "USB Webcam",
                "Smartphone Camera"
            });
            
            // Username
            Label lblExitCameraUsername = new Label();
            lblExitCameraUsername.Text = "Username:";
            lblExitCameraUsername.Location = new Point(15, 80);
            lblExitCameraUsername.AutoSize = true;
            
            txtExitCameraUsername = new TextBox();
            txtExitCameraUsername.Location = new Point(120, 77);
            txtExitCameraUsername.Size = new Size(200, 23);
            
            // Password
            Label lblExitCameraPassword = new Label();
            lblExitCameraPassword.Text = "Password:";
            lblExitCameraPassword.Location = new Point(15, 110);
            lblExitCameraPassword.AutoSize = true;
            
            txtExitCameraPassword = new TextBox();
            txtExitCameraPassword.Location = new Point(120, 107);
            txtExitCameraPassword.Size = new Size(200, 23);
            txtExitCameraPassword.PasswordChar = '*';
            
            // Test button
            btnTestExitCamera = new Button();
            btnTestExitCamera.Text = "Test Kamera";
            btnTestExitCamera.Location = new Point(120, 145);
            btnTestExitCamera.Size = new Size(100, 30);
            btnTestExitCamera.Click += new EventHandler(btnTestExitCamera_Click);
            ApplyModernButtonStyle(btnTestExitCamera);
            
            // Preview label
            Label lblExitCameraPreview = new Label();
            lblExitCameraPreview.Text = "Preview Kamera:";
            lblExitCameraPreview.Location = new Point(15, 190);
            lblExitCameraPreview.AutoSize = true;
            
            // Preview picture box
            pbExitCameraPreview = new PictureBox();
            pbExitCameraPreview.Location = new Point(120, 190);
            pbExitCameraPreview.Size = new Size(320, 240);
            pbExitCameraPreview.BorderStyle = BorderStyle.FixedSingle;
            pbExitCameraPreview.SizeMode = PictureBoxSizeMode.Zoom;
            pbExitCameraPreview.BackColor = Color.Black;
            
            // Add controls to tab
            tabExitCamera.Controls.Add(lblExitCameraURL);
            tabExitCamera.Controls.Add(txtExitCameraURL);
            tabExitCamera.Controls.Add(lblExitCameraType);
            tabExitCamera.Controls.Add(cmbExitCameraType);
            tabExitCamera.Controls.Add(lblExitCameraUsername);
            tabExitCamera.Controls.Add(txtExitCameraUsername);
            tabExitCamera.Controls.Add(lblExitCameraPassword);
            tabExitCamera.Controls.Add(txtExitCameraPassword);
            tabExitCamera.Controls.Add(btnTestExitCamera);
            tabExitCamera.Controls.Add(lblExitCameraPreview);
            tabExitCamera.Controls.Add(pbExitCameraPreview);
        }

        private void ApplyModernButtonStyle(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 1;
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
            button.BackColor = Color.FromArgb(0, 120, 215);
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            button.Cursor = Cursors.Hand;
            
            button.MouseEnter += (s, e) => {
                Button btn = (Button)s;
                btn.BackColor = Color.FromArgb(0, 102, 204);
            };
            
            button.MouseLeave += (s, e) => {
                Button btn = (Button)s;
                btn.BackColor = Color.FromArgb(0, 120, 215);
            };
        }

        private void LoadSettings()
        {
            try
            {
                CameraSettings settings = LoadCameraSettings();
                if (settings != null)
                {
                    // Entry camera settings
                    txtEntryCameraURL.Text = settings.EntryCameraURL;
                    cmbEntryCameraType.SelectedItem = settings.EntryCameraType;
                    txtEntryCameraUsername.Text = settings.EntryCameraUsername;
                    txtEntryCameraPassword.Text = settings.EntryCameraPassword;
                    
                    // Exit camera settings
                    txtExitCameraURL.Text = settings.ExitCameraURL;
                    cmbExitCameraType.SelectedItem = settings.ExitCameraType;
                    txtExitCameraUsername.Text = settings.ExitCameraUsername;
                    txtExitCameraPassword.Text = settings.ExitCameraPassword;
                }
                else
                {
                    // Set defaults
                    cmbEntryCameraType.SelectedIndex = 0;
                    cmbExitCameraType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading camera settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void StartPreviewTimer()
        {
            previewTimer.Start();
        }
        
        private void StopPreviewTimer()
        {
            previewTimer.Stop();
        }
        
        private void previewTimer_Tick(object sender, EventArgs e)
        {
            UpdateCameraPreviews();
        }
        
        private void UpdateCameraPreviews()
        {
            // In a real application, you'd connect to the cameras and get actual frames
            // For this example, we'll just use placeholder images
            
            // Update Entry Camera preview
            if (!string.IsNullOrEmpty(txtEntryCameraURL.Text))
            {
                pbEntryCameraPreview.Image = CreateCameraPlaceholder("Entry Camera Preview");
            }
            
            // Update Exit Camera preview
            if (!string.IsNullOrEmpty(txtExitCameraURL.Text))
            {
                pbExitCameraPreview.Image = CreateCameraPlaceholder("Exit Camera Preview");
            }
        }
        
        private Image CreateCameraPlaceholder(string text)
        {
            Bitmap placeholder = new Bitmap(320, 240);
            using (Graphics g = Graphics.FromImage(placeholder))
            {
                g.FillRectangle(Brushes.Black, 0, 0, 320, 240);
                
                // Draw a green camera icon
                g.FillEllipse(Brushes.Green, 135, 95, 50, 50);
                
                Font font = new Font("Arial", 10, FontStyle.Bold);
                SizeF textSize = g.MeasureString(text, font);
                g.DrawString(text, font, Brushes.White, 
                    new PointF((320 - textSize.Width) / 2, 160));
                
                g.DrawRectangle(new Pen(Color.FromArgb(0, 120, 215), 2), 1, 1, 317, 237);
            }
            
            return placeholder;
        }
        
        private async void btnTestEntryCamera_Click(object sender, EventArgs e)
        {
            try
            {
                btnTestEntryCamera.Enabled = false;
                btnTestEntryCamera.Text = "Testing...";
                
                // Test connection to entry camera
                bool success = await TestCameraConnection(txtEntryCameraURL.Text);
                
                if (success)
                {
                    MessageBox.Show("Successfully connected to entry camera!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not connect to entry camera. Please check the URL and credentials.",
                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing camera: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestEntryCamera.Enabled = true;
                btnTestEntryCamera.Text = "Test Connection";
            }
        }
        
        private async void btnTestExitCamera_Click(object sender, EventArgs e)
        {
            try
            {
                btnTestExitCamera.Enabled = false;
                btnTestExitCamera.Text = "Testing...";
                
                // Test connection to exit camera
                bool success = await TestCameraConnection(txtExitCameraURL.Text);
                
                if (success)
                {
                    MessageBox.Show("Successfully connected to exit camera!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Could not connect to exit camera. Please check the URL and credentials.",
                        "Connection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error testing camera: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTestExitCamera.Enabled = true;
                btnTestExitCamera.Text = "Test Connection";
            }
        }
        
        private async Task<bool> TestCameraConnection(string url)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    var response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Camera connection test failed: {ex.Message}");
                return false;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if ((cmbEntryCameraType.SelectedIndex >= 0 && string.IsNullOrWhiteSpace(txtEntryCameraURL.Text)) ||
                    (cmbExitCameraType.SelectedIndex >= 0 && string.IsNullOrWhiteSpace(txtExitCameraURL.Text)))
                {
                    MessageBox.Show("URL kamera harus diisi jika tipe kamera dipilih!", "Validasi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Create and save settings
                CameraSettings settings = new CameraSettings
                {
                    EntryCameraURL = txtEntryCameraURL.Text.Trim(),
                    EntryCameraType = cmbEntryCameraType.SelectedItem?.ToString(),
                    EntryCameraUsername = txtEntryCameraUsername.Text.Trim(),
                    EntryCameraPassword = txtEntryCameraPassword.Text,
                    
                    ExitCameraURL = txtExitCameraURL.Text.Trim(),
                    ExitCameraType = cmbExitCameraType.SelectedItem?.ToString(),
                    ExitCameraUsername = txtExitCameraUsername.Text.Trim(),
                    ExitCameraPassword = txtExitCameraPassword.Text
                };
                
                SaveCameraSettings(settings);
                
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menyimpan pengaturan: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
        
        private CameraSettings LoadCameraSettings()
        {
            string settingsPath = Path.Combine(Application.StartupPath, "config", "cameras.xml");
            
            if (!File.Exists(settingsPath))
                return null;
                
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(CameraSettings));
                using (StreamReader reader = new StreamReader(settingsPath))
                {
                    return (CameraSettings)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return null;
            }
        }
        
        private void SaveCameraSettings(CameraSettings settings)
        {
            string configDir = Path.Combine(Application.StartupPath, "config");
            string settingsPath = Path.Combine(configDir, "cameras.xml");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
                
            XmlSerializer serializer = new XmlSerializer(typeof(CameraSettings));
            using (StreamWriter writer = new StreamWriter(settingsPath))
            {
                serializer.Serialize(writer, settings);
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop the timer when closing the form
            StopPreviewTimer();
            
            // Clean up any image resources
            if (pbEntryCameraPreview.Image != null)
            {
                pbEntryCameraPreview.Image.Dispose();
                pbEntryCameraPreview.Image = null;
            }
            
            if (pbExitCameraPreview.Image != null)
            {
                pbExitCameraPreview.Image.Dispose();
                pbExitCameraPreview.Image = null;
            }
            
            base.OnFormClosing(e);
        }
    }

    [Serializable]
    public class CameraSettings
    {
        public string EntryCameraURL { get; set; }
        public string EntryCameraType { get; set; }
        public string EntryCameraUsername { get; set; }
        public string EntryCameraPassword { get; set; }
        
        public string ExitCameraURL { get; set; }
        public string ExitCameraType { get; set; }
        public string ExitCameraUsername { get; set; }
        public string ExitCameraPassword { get; set; }
    }
} 