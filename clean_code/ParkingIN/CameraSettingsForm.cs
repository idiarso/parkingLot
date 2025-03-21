using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Threading;
// Tambahkan referensi untuk AForge
using AForge.Video;
using AForge.Video.DirectShow;
using System.Linq;
using System.Diagnostics;
using System.Net.Http;

namespace ParkingIN
{
    public partial class CameraSettingsForm : Form
    {
        private string cameraConfigPath;
        private VideoCaptureDevice localWebcam;
        // Use HttpClient for IP camera streaming
        private System.Net.Http.HttpClient ipCameraClient;
        private System.Threading.CancellationTokenSource ipCameraCts;
        private bool isWebcamRunning = false;
        private bool isIpCameraRunning = false;
        private System.Threading.Timer ipCameraTimer;
        
        public CameraSettingsForm()
        {
            InitializeComponent();
            cameraConfigPath = Path.Combine(Application.StartupPath, "config", "camera.ini");
        }

        private void CameraSettingsForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadCameraSettings();
                LoadLocalCameras();
                
                // Set up camera preview controls
                cmbCameraType.Items.Clear();
                cmbCameraType.Items.Add("Local Webcam");
                cmbCameraType.Items.Add("IP Camera");
                cmbCameraType.SelectedIndex = 0;
                
                // Set up preview panel
                picPreview.SizeMode = PictureBoxSizeMode.StretchImage;
                picPreview.BorderStyle = BorderStyle.FixedSingle;
            }
            catch (Exception ex)
            {
                LogError($"Error initializing camera form: {ex.Message}");
                MessageBox.Show($"Error initializing camera form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadLocalCameras()
        {
            try
            {
                cmbLocalCamera.Items.Clear();
                
                // Get available webcams
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (videoDevices.Count > 0)
                {
                    foreach (FilterInfo device in videoDevices)
                    {
                        cmbLocalCamera.Items.Add(device.Name);
                    }
                    cmbLocalCamera.SelectedIndex = 0;
                }
                else
                {
                    cmbLocalCamera.Items.Add("No webcams found");
                    cmbLocalCamera.SelectedIndex = 0;
                    cmbLocalCamera.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading local cameras: {ex.Message}");
                MessageBox.Show($"Error loading local cameras: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadCameraSettings()
        {
            try
            {
                if (File.Exists(cameraConfigPath))
                {
                    string[] lines = File.ReadAllLines(cameraConfigPath);
                    int cameraType = 0; // Default to local webcam
                    
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("["))
                            continue;
                        
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length != 2)
                            continue;
                        
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        
                        switch (key)
                        {
                            case "Type":
                                cameraType = int.Parse(value);
                                break;
                            case "PreferredCamera":
                                // Find and select the preferred camera in the combo box
                                for (int i = 0; i < cmbLocalCamera.Items.Count; i++)
                                {
                                    if (cmbLocalCamera.Items[i].ToString().Equals(value, StringComparison.OrdinalIgnoreCase))
                                    {
                                        cmbLocalCamera.SelectedIndex = i;
                                        break;
                                    }
                                }
                                break;
                            case "IP":
                                txtCameraIP.Text = value;
                                break;
                            case "Username":
                                txtUsername.Text = value;
                                break;
                            case "Password":
                                txtPassword.Text = value;
                                break;
                            case "Port":
                                txtPort.Text = value;
                                break;
                            case "Resolution":
                                cmbResolution.Text = value;
                                break;
                            case "Capture_Interval":
                                txtCaptureInterval.Text = value;
                                break;
                            case "OCR_Enabled":
                                chkOCREnabled.Checked = bool.Parse(value);
                                break;
                            case "Min_Confidence":
                                txtMinConfidence.Text = value;
                                break;
                            case "Plate_Region":
                                cmbPlateRegion.Text = value;
                                break;
                            case "Max_Angle":
                                txtMaxAngle.Text = value;
                                break;
                        }
                    }
                    
                    // Set camera type and update UI
                    cmbCameraType.SelectedIndex = cameraType;
                    cmbCameraType_SelectedIndexChanged(null, null);
                }
                else
                {
                    SetDefaultCameraSettings();
                    MessageBox.Show("Camera configuration file not found. Default settings will be used.", 
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error loading camera settings: {ex.Message}");
                SetDefaultCameraSettings();
                MessageBox.Show($"Error loading camera settings: {ex.Message}. Default settings will be used.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SetDefaultCameraSettings()
        {
            // Set default values
            txtCameraIP.Text = "192.168.1.100";
            txtUsername.Text = "admin";
            txtPassword.Text = "admin123";
            txtPort.Text = "8080";
            cmbResolution.Text = "1280x720";
            txtCaptureInterval.Text = "1000";
            chkOCREnabled.Checked = true;
            txtMinConfidence.Text = "80";
            cmbPlateRegion.Text = "ID";
            txtMaxAngle.Text = "30";
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (cmbCameraType.SelectedIndex == 0) // Local webcam
                {
                    if (cmbLocalCamera.SelectedIndex < 0 || !cmbLocalCamera.Enabled)
                    {
                        MessageBox.Show("Please select a valid webcam.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cmbLocalCamera.Focus();
                        return;
                    }
                }
                else // IP camera
                {
                    if (string.IsNullOrEmpty(txtCameraIP.Text))
                    {
                        MessageBox.Show("Camera IP cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtCameraIP.Focus();
                        return;
                    }
                    
                    if (string.IsNullOrEmpty(txtPort.Text) || !int.TryParse(txtPort.Text, out _))
                    {
                        MessageBox.Show("Port must be a valid number.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPort.Focus();
                        return;
                    }
                }
                
                // Ensure directory exists
                string configDir = Path.Combine(Application.StartupPath, "config");
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // Write to config file
                using (StreamWriter writer = new StreamWriter(cameraConfigPath))
                {
                    writer.WriteLine("[Camera]");
                    writer.WriteLine($"Type={cmbCameraType.SelectedIndex}");
                    
                    if (cmbCameraType.SelectedIndex == 0) // Local webcam
                    {
                        writer.WriteLine($"PreferredCamera={cmbLocalCamera.SelectedItem}");
                    }
                    else // IP camera
                    {
                        writer.WriteLine($"IP={txtCameraIP.Text}");
                        writer.WriteLine($"Username={txtUsername.Text}");
                        writer.WriteLine($"Password={txtPassword.Text}");
                        writer.WriteLine($"Port={txtPort.Text}");
                    }
                    
                    writer.WriteLine($"Resolution={cmbResolution.Text}");
                    writer.WriteLine($"Capture_Interval={txtCaptureInterval.Text}");
                    writer.WriteLine($"OCR_Enabled={chkOCREnabled.Checked}");
                    writer.WriteLine();
                    writer.WriteLine("[OCR]");
                    writer.WriteLine($"Min_Confidence={txtMinConfidence.Text}");
                    writer.WriteLine($"Plate_Region={cmbPlateRegion.Text}");
                    writer.WriteLine($"Max_Angle={txtMaxAngle.Text}");
                }
                
                LogSystemMessage("Camera settings saved successfully");
                MessageBox.Show("Camera settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogError($"Error saving camera settings: {ex.Message}");
                MessageBox.Show($"Error saving camera settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnTestConnection_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Visible = true;
                lblStatus.Text = "Testing camera connection...";
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                
                // Test IP connectivity with ping
                string cameraIp = txtCameraIP.Text;
                bool pingSuccess = false;
                
                // Run ping test
                using (Ping ping = new Ping())
                {
                    try
                    {
                        PingReply reply = ping.Send(cameraIp, 3000);
                        pingSuccess = (reply.Status == IPStatus.Success);
                    }
                    catch
                    {
                        pingSuccess = false;
                    }
                }
                
                if (pingSuccess)
                {
                    // Try to connect to camera stream if ping successful
                    lblStatus.Text = "Ping successful. Trying to connect to camera stream...";
                    Application.DoEvents();
                    
                    // Test stream connection
                    Task.Run(() => 
                    {
                        try 
                        {
                            string testUrl = GetCameraStreamUrl();
                            // Use HttpClient instead of MJPEGStream
                            bool connected = false;
                            using (var client = new System.Net.Http.HttpClient())
                            {
                                client.Timeout = TimeSpan.FromSeconds(5);
                                var response = client.GetAsync(testUrl).Result;
                                if (response.IsSuccessStatusCode)
                                {
                                    connected = true;
                                }
                            }
                            
                            // Wait up to 5 seconds for a frame
                            for (int i = 0; i < 50 && !connected; i++)
                            {
                                System.Threading.Thread.Sleep(100);
                            }
                            
                            // Don't need to stop the stream as we're using HttpClient which is disposed
                            // testStream.Stop();
                            
                            if (connected)
                            {
                                this.Invoke((MethodInvoker)delegate 
                                {
                                    lblStatus.Text = "Camera connection successful";
                                    MessageBox.Show($"Successfully connected to camera at {cameraIp}.", 
                                        "Connection Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                });
                            }
                            else
                            {
                                throw new Exception("Could not receive video stream.");
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Invoke((MethodInvoker)delegate 
                            {
                                lblStatus.Text = "Camera stream connection failed";
                                MessageBox.Show($"Failed to connect to camera stream: {ex.Message}\nDevice might be reachable but not streaming correctly.", 
                                    "Stream Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            });
                        }
                        finally
                        {
                            this.Invoke((MethodInvoker)delegate 
                            {
                                Cursor = Cursors.Default;
                                lblStatus.Visible = false;
                            });
                        }
                    });
                }
                else
                {
                    lblStatus.Text = "Ping failed";
                    MessageBox.Show($"Failed to connect to camera at {cameraIp}. Please check your network connection and camera settings.", 
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Cursor = Cursors.Default;
                    lblStatus.Visible = false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error testing camera connection: {ex.Message}");
                MessageBox.Show($"Error testing camera connection: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor = Cursors.Default;
                lblStatus.Visible = false;
            }
        }
        
        private string GetCameraStreamUrl()
        {
            string ip = txtCameraIP.Text;
            string port = txtPort.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            
            // Build MJPEG URL (format varies by camera model - this is a common format)
            string mjpegUrl;
            
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                mjpegUrl = $"http://{username}:{password}@{ip}:{port}/videostream.cgi";
            }
            else
            {
                mjpegUrl = $"http://{ip}:{port}/videostream.cgi";
            }
            
            return mjpegUrl;
        }
        
        private void btnStartPreview_Click(object sender, EventArgs e)
        {
            try
            {
                StopAllCameraStreams(); // Stop any existing streams
                
                if (cmbCameraType.SelectedIndex == 0) // Local webcam
                {
                    if (cmbLocalCamera.SelectedIndex >= 0 && cmbLocalCamera.Enabled)
                    {
                        StartWebcamPreview();
                    }
                    else
                    {
                        MessageBox.Show("No local webcam available.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else // IP camera
                {
                    if (!string.IsNullOrEmpty(txtCameraIP.Text))
                    {
                        StartIpCameraPreview();
                    }
                    else
                    {
                        MessageBox.Show("Please enter valid IP camera settings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error starting camera preview: {ex.Message}");
                MessageBox.Show($"Error starting camera preview: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnStopPreview_Click(object sender, EventArgs e)
        {
            StopAllCameraStreams();
            if (picPreview.Image != null)
            {
                picPreview.Image.Dispose();
                picPreview.Image = null;
            }
        }
        
        private void StartWebcamPreview()
        {
            try
            {
                // Get list of video devices
                FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (videoDevices.Count == 0)
                {
                    throw new Exception("No webcams found. Please check your camera connection.");
                }
                
                if (cmbLocalCamera.SelectedIndex >= videoDevices.Count)
                {
                    throw new Exception("Selected camera is no longer available.");
                }
                
                // Create video source
                localWebcam = new VideoCaptureDevice(videoDevices[cmbLocalCamera.SelectedIndex].MonikerString);
                
                // Validate and set resolution
                if (localWebcam.VideoCapabilities.Length > 0)
                {
                    string selectedResolution = cmbResolution.Text;
                    bool resolutionFound = false;
                    
                    foreach (VideoCapabilities capability in localWebcam.VideoCapabilities)
                    {
                        string resolution = $"{capability.FrameSize.Width}x{capability.FrameSize.Height}";
                        if (resolution.Equals(selectedResolution, StringComparison.OrdinalIgnoreCase))
                        {
                            localWebcam.VideoResolution = capability;
                            resolutionFound = true;
                            break;
                        }
                    }
                    
                    if (!resolutionFound)
                    {
                        LogWarning($"Selected resolution {selectedResolution} not available. Using highest available resolution.");
                        // Use highest available resolution
                        VideoCapabilities highestResolution = localWebcam.VideoCapabilities[0];
                        for (int i = 1; i < localWebcam.VideoCapabilities.Length; i++)
                        {
                            if (localWebcam.VideoCapabilities[i].FrameSize.Width > highestResolution.FrameSize.Width)
                            {
                                highestResolution = localWebcam.VideoCapabilities[i];
                            }
                        }
                        localWebcam.VideoResolution = highestResolution;
                    }
                }
                
                // Set up event handler for new frames
                localWebcam.NewFrame += LocalWebcam_NewFrame;
                
                // Start the camera with retry logic
                int retryCount = 0;
                const int maxRetries = 3;
                bool started = false;
                
                while (retryCount < maxRetries && !started)
                {
                    try
                    {
                        localWebcam.Start();
                        started = true;
                        isWebcamRunning = true;
                        
                        // Update status
                        lblStatus.Visible = true;
                        lblStatus.Text = $"Local webcam running at {localWebcam.VideoResolution.FrameSize.Width}x{localWebcam.VideoResolution.FrameSize.Height}";
                        LogSystemMessage($"Started local webcam preview at {localWebcam.VideoResolution.FrameSize.Width}x{localWebcam.VideoResolution.FrameSize.Height}");
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        LogWarning($"Failed to start webcam (attempt {retryCount}/{maxRetries}): {ex.Message}");
                        
                        if (retryCount < maxRetries)
                        {
                            Thread.Sleep(1000); // Wait 1 second before retry
                            continue;
                        }
                        
                        throw new Exception($"Failed to start webcam after {maxRetries} attempts: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error starting webcam: {ex.Message}");
                MessageBox.Show($"Error starting webcam: {ex.Message}\nPlease check your camera connection and try again.", 
                    "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Clean up
                if (localWebcam != null)
                {
                    try
                    {
                        localWebcam.SignalToStop();
                        localWebcam.WaitForStop();
                        localWebcam.NewFrame -= LocalWebcam_NewFrame;
                        localWebcam = null;
                    }
                    catch { }
                }
                isWebcamRunning = false;
                throw;
            }
        }
        
        private void StartIpCameraPreview()
        {
            try
            {
                // Validate IP address format
                if (!System.Net.IPAddress.TryParse(txtCameraIP.Text, out _))
                {
                    throw new Exception("Invalid IP address format.");
                }
                
                // Validate port number
                if (!int.TryParse(txtPort.Text, out int port) || port <= 0 || port > 65535)
                {
                    throw new Exception("Invalid port number. Must be between 1 and 65535.");
                }
                
                // Get the MJPEG URL
                string mjpegUrl = GetCameraStreamUrl();
                
                // Test connection first
                using (var ping = new System.Net.NetworkInformation.Ping())
                {
                    var reply = ping.Send(txtCameraIP.Text, 3000);
                    if (reply.Status != System.Net.NetworkInformation.IPStatus.Success)
                    {
                        throw new Exception($"Cannot reach camera at {txtCameraIP.Text}. Please check network connection.");
                    }
                }
                
                // Create a new HttpClient for IP camera
                ipCameraClient = new System.Net.Http.HttpClient();
                ipCameraCts = new System.Threading.CancellationTokenSource();
                
                // Start fetching frames periodically with timer instead of event
                ipCameraTimer = new System.Threading.Timer(FetchIpCameraFrame, null, 0, 100);
                isIpCameraRunning = true;
                
                // Start the camera with retry logic
                int retryCount = 0;
                const int maxRetries = 3;
                bool started = false;
                
                while (retryCount < maxRetries && !started)
                {
                    try
                    {
                        started = true;
                        isIpCameraRunning = true;
                        
                        // Update status
                        lblStatus.Visible = true;
                        lblStatus.Text = $"IP camera running at {txtCameraIP.Text}:{port}";
                        LogSystemMessage($"Started IP camera preview: {mjpegUrl}");
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        LogWarning($"Failed to start IP camera (attempt {retryCount}/{maxRetries}): {ex.Message}");
                        
                        if (retryCount < maxRetries)
                        {
                            Thread.Sleep(1000); // Wait 1 second before retry
                            continue;
                        }
                        
                        throw new Exception($"Failed to start IP camera after {maxRetries} attempts: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error connecting to IP camera: {ex.Message}");
                MessageBox.Show($"Error connecting to IP camera: {ex.Message}\nPlease check your network connection and camera settings.", 
                    "Camera Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Clean up
                if (ipCameraClient != null)
                {
                    try
                    {
                        ipCameraClient.Dispose();
                        ipCameraClient = null;
                    }
                    catch { }
                }
                isIpCameraRunning = false;
                throw;
            }
        }
        
        private void LocalWebcam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Make a copy of the frame
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
                
                // Display the frame
                if (picPreview.InvokeRequired)
                {
                    picPreview.Invoke((MethodInvoker)delegate 
                    {
                        DisplayFrame(frame);
                    });
                }
                else
                {
                    DisplayFrame(frame);
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore frame errors after form is closed
            }
            catch (Exception ex)
            {
                LogError($"Error processing webcam frame: {ex.Message}");
            }
        }
        
        private void IpCamera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Make a copy of the frame
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
                
                // Display the frame
                if (picPreview.InvokeRequired)
                {
                    picPreview.Invoke((MethodInvoker)delegate 
                    {
                        DisplayFrame(frame);
                    });
                }
                else
                {
                    DisplayFrame(frame);
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore frame errors after form is closed
            }
            catch (Exception ex)
            {
                LogError($"Error processing IP camera frame: {ex.Message}");
            }
        }
        
        private void DisplayFrame(Bitmap frame)
        {
            try
            {
                // If OCR is enabled, prepare frame for license plate recognition
                if (chkOCREnabled.Checked)
                {
                    // Here we would apply image preprocessing for better OCR
                    // Could include: grayscale conversion, thresholding, edge detection
                    // This requires more complex image processing library
                    
                    // For now, just display the frame as is
                    // In a full implementation, you could integrate Tesseract OCR or similar
                }
                
                // Display the frame
                if (picPreview.Image != null)
                {
                    picPreview.Image.Dispose();
                }
                picPreview.Image = frame;
            }
            catch (Exception ex)
            {
                LogError($"Error displaying frame: {ex.Message}");
            }
        }
        
        private void StopAllCameraStreams()
        {
            // Stop local webcam if running
            if (localWebcam != null && isWebcamRunning)
            {
                try
                {
                    localWebcam.SignalToStop();
                    localWebcam.WaitForStop();
                    localWebcam.NewFrame -= LocalWebcam_NewFrame;
                    localWebcam = null;
                    isWebcamRunning = false;
                    LogSystemMessage("Stopped local webcam preview");
                }
                catch (Exception ex)
                {
                    LogError($"Error stopping webcam: {ex.Message}");
                }
            }
            
            // Stop IP camera if running
            if (ipCameraClient != null && isIpCameraRunning)
            {
                try
                {
                    ipCameraClient.Dispose();
                    ipCameraClient = null;
                    isIpCameraRunning = false;
                    LogSystemMessage("Stopped IP camera preview");
                }
                catch (Exception ex)
                {
                    LogError($"Error stopping IP camera: {ex.Message}");
                }
            }
            
            lblStatus.Visible = false;
        }
        
        private void btnCapture_Click(object sender, EventArgs e)
        {
            try
            {
                if (picPreview.Image != null)
                {
                    // Create directory if it doesn't exist
                    string captureDir = Path.Combine(Application.StartupPath, "Images", "Entry");
                    if (!Directory.Exists(captureDir))
                    {
                        Directory.CreateDirectory(captureDir);
                    }
                    
                    // Generate filename with timestamp
                    string fileName = $"Capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                    string filePath = Path.Combine(captureDir, fileName);
                    
                    // Save image
                    using (Bitmap image = new Bitmap(picPreview.Image))
                    {
                        image.Save(filePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    
                    LogSystemMessage($"Image captured and saved to: {filePath}");
                    MessageBox.Show($"Image captured and saved to:\n{filePath}", 
                        "Capture Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("No camera preview to capture. Please start the camera preview first.",
                        "Capture Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error capturing image: {ex.Message}");
                MessageBox.Show($"Error capturing image: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void cmbCameraType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCameraType.SelectedIndex == 0) // Local webcam
            {
                pnlLocalCamera.Visible = true;
                pnlIpCamera.Visible = false;
            }
            else // IP camera
            {
                pnlLocalCamera.Visible = false;
                pnlIpCamera.Visible = true;
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop all camera streams when form is closing
            StopAllCameraStreams();
            
            // Clean up any remaining images
            if (picPreview.Image != null)
            {
                picPreview.Image.Dispose();
                picPreview.Image = null;
            }
            
            base.OnFormClosing(e);
        }
        
        private void LogSystemMessage(string message)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "system.log");
                string directory = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string logMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [CameraSettings] {message}\n";
                File.AppendAllText(logPath, logMsg);
            }
            catch
            {
                // Ignore logging errors
            }
        }
        
        private void LogWarning(string message)
        {
            try
            {
                var logMessage = $"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(Path.Combine("logs", "camera.log"), logMessage + Environment.NewLine);
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
        
        private void LogError(string message)
        {
            try
            {
                var logMessage = $"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(Path.Combine("logs", "camera.log"), logMessage + Environment.NewLine);
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        private void LogInfo(string message)
        {
            try
            {
                var logMessage = $"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(Path.Combine("logs", "camera.log"), logMessage + Environment.NewLine);
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }

        #region Windows Form Designer generated code

        private Label lblTitle;
        private Label lblCameraIP;
        private TextBox txtCameraIP;
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblResolution;
        private ComboBox cmbResolution;
        private Label lblCaptureInterval;
        private TextBox txtCaptureInterval;
        private CheckBox chkOCREnabled;
        private Label lblOCRSettings;
        private Label lblMinConfidence;
        private TextBox txtMinConfidence;
        private Label lblPlateRegion;
        private ComboBox cmbPlateRegion;
        private Label lblMaxAngle;
        private TextBox txtMaxAngle;
        private Button btnTestConnection;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;
        // New controls
        private ComboBox cmbCameraType;
        private Label lblCameraType;
        private Panel pnlLocalCamera;
        private Label lblLocalCamera;
        private ComboBox cmbLocalCamera;
        private Panel pnlIpCamera;
        private PictureBox picPreview;
        private Label lblPreview;
        private Button btnStartPreview;
        private Button btnStopPreview;
        private Button btnCapture;
        private Panel pnlPreview;

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblCameraIP = new Label();
            this.txtCameraIP = new TextBox();
            this.lblUsername = new Label();
            this.txtUsername = new TextBox();
            this.lblPassword = new Label();
            this.txtPassword = new TextBox();
            this.lblPort = new Label();
            this.txtPort = new TextBox();
            this.lblResolution = new Label();
            this.cmbResolution = new ComboBox();
            this.lblCaptureInterval = new Label();
            this.txtCaptureInterval = new TextBox();
            this.chkOCREnabled = new CheckBox();
            this.lblOCRSettings = new Label();
            this.lblMinConfidence = new Label();
            this.txtMinConfidence = new TextBox();
            this.lblPlateRegion = new Label();
            this.cmbPlateRegion = new ComboBox();
            this.lblMaxAngle = new Label();
            this.txtMaxAngle = new TextBox();
            this.btnTestConnection = new Button();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.lblStatus = new Label();
            this.cmbCameraType = new ComboBox();
            this.lblCameraType = new Label();
            this.pnlLocalCamera = new Panel();
            this.lblLocalCamera = new Label();
            this.cmbLocalCamera = new ComboBox();
            this.pnlIpCamera = new Panel();
            this.picPreview = new PictureBox();
            this.lblPreview = new Label();
            this.btnStartPreview = new Button();
            this.btnStopPreview = new Button();
            this.btnCapture = new Button();
            this.pnlPreview = new Panel();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(187, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Camera Settings";
            // 
            // lblCameraIP
            // 
            this.lblCameraIP.AutoSize = true;
            this.lblCameraIP.Location = new Point(25, 70);
            this.lblCameraIP.Name = "lblCameraIP";
            this.lblCameraIP.Size = new Size(65, 15);
            this.lblCameraIP.TabIndex = 1;
            this.lblCameraIP.Text = "Camera IP:";
            // 
            // txtCameraIP
            // 
            this.txtCameraIP.Location = new Point(140, 67);
            this.txtCameraIP.Name = "txtCameraIP";
            this.txtCameraIP.Size = new Size(200, 23);
            this.txtCameraIP.TabIndex = 2;
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new Point(25, 100);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new Size(63, 15);
            this.lblUsername.TabIndex = 3;
            this.lblUsername.Text = "Username:";
            // 
            // txtUsername
            // 
            this.txtUsername.Location = new Point(140, 97);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new Size(200, 23);
            this.txtUsername.TabIndex = 4;
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(25, 130);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(60, 15);
            this.lblPassword.TabIndex = 5;
            this.lblPassword.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Location = new Point(140, 127);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(200, 23);
            this.txtPassword.TabIndex = 6;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new Point(25, 160);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new Size(32, 15);
            this.lblPort.TabIndex = 7;
            this.lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new Point(140, 157);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new Size(80, 23);
            this.txtPort.TabIndex = 8;
            // 
            // lblResolution
            // 
            this.lblResolution.AutoSize = true;
            this.lblResolution.Location = new Point(25, 190);
            this.lblResolution.Name = "lblResolution";
            this.lblResolution.Size = new Size(66, 15);
            this.lblResolution.TabIndex = 9;
            this.lblResolution.Text = "Resolution:";
            // 
            // cmbResolution
            // 
            this.cmbResolution.FormattingEnabled = true;
            this.cmbResolution.Items.AddRange(new object[] {
            "640x480",
            "800x600",
            "1024x768",
            "1280x720",
            "1920x1080"});
            this.cmbResolution.Location = new Point(140, 187);
            this.cmbResolution.Name = "cmbResolution";
            this.cmbResolution.Size = new Size(121, 23);
            this.cmbResolution.TabIndex = 10;
            // 
            // lblCaptureInterval
            // 
            this.lblCaptureInterval.AutoSize = true;
            this.lblCaptureInterval.Location = new Point(25, 220);
            this.lblCaptureInterval.Name = "lblCaptureInterval";
            this.lblCaptureInterval.Size = new Size(109, 15);
            this.lblCaptureInterval.TabIndex = 11;
            this.lblCaptureInterval.Text = "Capture Interval (ms):";
            // 
            // txtCaptureInterval
            // 
            this.txtCaptureInterval.Location = new Point(140, 217);
            this.txtCaptureInterval.Name = "txtCaptureInterval";
            this.txtCaptureInterval.Size = new Size(80, 23);
            this.txtCaptureInterval.TabIndex = 12;
            // 
            // chkOCREnabled
            // 
            this.chkOCREnabled.AutoSize = true;
            this.chkOCREnabled.Location = new Point(140, 250);
            this.chkOCREnabled.Name = "chkOCREnabled";
            this.chkOCREnabled.Size = new Size(95, 19);
            this.chkOCREnabled.TabIndex = 13;
            this.chkOCREnabled.Text = "OCR Enabled";
            this.chkOCREnabled.UseVisualStyleBackColor = true;
            // 
            // lblOCRSettings
            // 
            this.lblOCRSettings.AutoSize = true;
            this.lblOCRSettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblOCRSettings.Location = new Point(25, 290);
            this.lblOCRSettings.Name = "lblOCRSettings";
            this.lblOCRSettings.Size = new Size(116, 21);
            this.lblOCRSettings.TabIndex = 14;
            this.lblOCRSettings.Text = "OCR Settings:";
            // 
            // lblMinConfidence
            // 
            this.lblMinConfidence.AutoSize = true;
            this.lblMinConfidence.Location = new Point(45, 330);
            this.lblMinConfidence.Name = "lblMinConfidence";
            this.lblMinConfidence.Size = new Size(121, 15);
            this.lblMinConfidence.TabIndex = 15;
            this.lblMinConfidence.Text = "Minimum Confidence:";
            // 
            // txtMinConfidence
            // 
            this.txtMinConfidence.Location = new Point(170, 327);
            this.txtMinConfidence.Name = "txtMinConfidence";
            this.txtMinConfidence.Size = new Size(80, 23);
            this.txtMinConfidence.TabIndex = 16;
            // 
            // lblPlateRegion
            // 
            this.lblPlateRegion.AutoSize = true;
            this.lblPlateRegion.Location = new Point(45, 360);
            this.lblPlateRegion.Name = "lblPlateRegion";
            this.lblPlateRegion.Size = new Size(75, 15);
            this.lblPlateRegion.TabIndex = 17;
            this.lblPlateRegion.Text = "Plate Region:";
            // 
            // cmbPlateRegion
            // 
            this.cmbPlateRegion.FormattingEnabled = true;
            this.cmbPlateRegion.Items.AddRange(new object[] {
            "ID",
            "MY",
            "SG",
            "TH"});
            this.cmbPlateRegion.Location = new Point(170, 357);
            this.cmbPlateRegion.Name = "cmbPlateRegion";
            this.cmbPlateRegion.Size = new Size(80, 23);
            this.cmbPlateRegion.TabIndex = 18;
            // 
            // lblMaxAngle
            // 
            this.lblMaxAngle.AutoSize = true;
            this.lblMaxAngle.Location = new Point(45, 390);
            this.lblMaxAngle.Name = "lblMaxAngle";
            this.lblMaxAngle.Size = new Size(94, 15);
            this.lblMaxAngle.TabIndex = 19;
            this.lblMaxAngle.Text = "Max Angle (deg):";
            // 
            // txtMaxAngle
            // 
            this.txtMaxAngle.Location = new Point(170, 387);
            this.txtMaxAngle.Name = "txtMaxAngle";
            this.txtMaxAngle.Size = new Size(80, 23);
            this.txtMaxAngle.TabIndex = 20;
            // 
            // btnTestConnection
            // 
            this.btnTestConnection.Location = new Point(371, 67);
            this.btnTestConnection.Name = "btnTestConnection";
            this.btnTestConnection.Size = new Size(120, 23);
            this.btnTestConnection.TabIndex = 21;
            this.btnTestConnection.Text = "Test Connection";
            this.btnTestConnection.UseVisualStyleBackColor = true;
            this.btnTestConnection.Click += new EventHandler(this.btnTestConnection_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new Point(170, 430);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(80, 30);
            this.btnSave.TabIndex = 22;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new Point(270, 430);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(80, 30);
            this.btnCancel.TabIndex = 23;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = Color.Blue;
            this.lblStatus.Location = new Point(25, 475);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(42, 15);
            this.lblStatus.TabIndex = 24;
            this.lblStatus.Text = "Status";
            this.lblStatus.Visible = false;
            // 
            // cmbCameraType
            // 
            this.cmbCameraType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbCameraType.FormattingEnabled = true;
            this.cmbCameraType.Location = new Point(120, 250);
            this.cmbCameraType.Name = "cmbCameraType";
            this.cmbCameraType.Size = new Size(150, 23);
            this.cmbCameraType.TabIndex = 25;
            this.cmbCameraType.SelectedIndexChanged += new EventHandler(this.cmbCameraType_SelectedIndexChanged);
            // 
            // lblCameraType
            // 
            this.lblCameraType.AutoSize = true;
            this.lblCameraType.Location = new Point(25, 250);
            this.lblCameraType.Name = "lblCameraType";
            this.lblCameraType.Size = new Size(80, 15);
            this.lblCameraType.TabIndex = 26;
            this.lblCameraType.Text = "Camera Type:";
            // 
            // pnlLocalCamera
            // 
            this.pnlLocalCamera.Location = new Point(25, 280);
            this.pnlLocalCamera.Name = "pnlLocalCamera";
            this.pnlLocalCamera.Size = new Size(450, 40);
            this.pnlLocalCamera.TabIndex = 27;
            // 
            // lblLocalCamera
            // 
            this.lblLocalCamera.AutoSize = true;
            this.lblLocalCamera.Location = new Point(0, 10);
            this.lblLocalCamera.Name = "lblLocalCamera";
            this.lblLocalCamera.Size = new Size(90, 15);
            this.lblLocalCamera.Text = "Select Webcam:";
            // 
            // cmbLocalCamera
            // 
            this.cmbLocalCamera.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbLocalCamera.FormattingEnabled = true;
            this.cmbLocalCamera.Location = new Point(95, 7);
            this.cmbLocalCamera.Name = "cmbLocalCamera";
            this.cmbLocalCamera.Size = new Size(250, 23);
            this.cmbLocalCamera.TabIndex = 1;
            // 
            // pnlIpCamera
            // 
            this.pnlIpCamera.Location = new Point(25, 280);
            this.pnlIpCamera.Name = "pnlIpCamera";
            this.pnlIpCamera.Size = new Size(450, 40);
            this.pnlIpCamera.TabIndex = 28;
            this.pnlIpCamera.Visible = false;
            // 
            // picPreview
            // 
            this.picPreview.Location = new Point(5, 5);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new Size(438, 235);
            this.picPreview.TabIndex = 0;
            this.picPreview.TabStop = false;
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Location = new Point(25, 310);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new Size(48, 15);
            this.lblPreview.TabIndex = 30;
            this.lblPreview.Text = "Preview:";
            // 
            // btnStartPreview
            // 
            this.btnStartPreview.Location = new Point(5, 245);
            this.btnStartPreview.Name = "btnStartPreview";
            this.btnStartPreview.Size = new Size(100, 30);
            this.btnStartPreview.TabIndex = 1;
            this.btnStartPreview.Text = "Start Preview";
            this.btnStartPreview.UseVisualStyleBackColor = true;
            this.btnStartPreview.Click += new EventHandler(this.btnStartPreview_Click);
            // 
            // btnStopPreview
            // 
            this.btnStopPreview.Location = new Point(115, 245);
            this.btnStopPreview.Name = "btnStopPreview";
            this.btnStopPreview.Size = new Size(100, 30);
            this.btnStopPreview.TabIndex = 2;
            this.btnStopPreview.Text = "Stop Preview";
            this.btnStopPreview.UseVisualStyleBackColor = true;
            this.btnStopPreview.Click += new EventHandler(this.btnStopPreview_Click);
            // 
            // btnCapture
            // 
            this.btnCapture.Location = new Point(343, 245);
            this.btnCapture.Name = "btnCapture";
            this.btnCapture.Size = new Size(100, 30);
            this.btnCapture.TabIndex = 3;
            this.btnCapture.Text = "Capture";
            this.btnCapture.UseVisualStyleBackColor = true;
            this.btnCapture.Click += new EventHandler(this.btnCapture_Click);
            // 
            // pnlPreview
            // 
            this.pnlPreview.BorderStyle = BorderStyle.FixedSingle;
            this.pnlPreview.Location = new Point(25, 330);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new Size(450, 285);
            this.pnlPreview.TabIndex = 29;
            // 
            // CameraSettingsForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(524, 640);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnTestConnection);
            this.Controls.Add(this.txtMaxAngle);
            this.Controls.Add(this.lblMaxAngle);
            this.Controls.Add(this.cmbPlateRegion);
            this.Controls.Add(this.lblPlateRegion);
            this.Controls.Add(this.txtMinConfidence);
            this.Controls.Add(this.lblMinConfidence);
            this.Controls.Add(this.lblOCRSettings);
            this.Controls.Add(this.chkOCREnabled);
            this.Controls.Add(this.txtCaptureInterval);
            this.Controls.Add(this.lblCaptureInterval);
            this.Controls.Add(this.cmbResolution);
            this.Controls.Add(this.lblResolution);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtCameraIP);
            this.Controls.Add(this.lblCameraIP);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.cmbCameraType);
            this.Controls.Add(this.lblCameraType);
            this.Controls.Add(this.pnlLocalCamera);
            this.Controls.Add(this.pnlIpCamera);
            this.Controls.Add(this.lblPreview);
            this.Controls.Add(this.pnlPreview);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CameraSettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Camera Settings";
            this.Load += new EventHandler(this.CameraSettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private void InitializeCamera()
        {
            try
            {
                if (cmbCameraType.SelectedItem.ToString() == "Local Webcam")
                {
                    localWebcam = new VideoCaptureDevice(cmbLocalCamera.SelectedItem.ToString());
                    localWebcam.VideoResolution = localWebcam.VideoCapabilities.FirstOrDefault();
                }
                else
                {
                    // Removed MJPEGStream usage
                    ipCameraClient = new System.Net.Http.HttpClient();
                    ipCameraCts = new System.Threading.CancellationTokenSource();
                }

                if (cmbCameraType.SelectedItem.ToString() == "Local Webcam")
                {
                    localWebcam.NewFrame += LocalWebcam_NewFrame;
                    localWebcam.Start();
                }
                else
                {
                    // Start fetching frames periodically with timer instead of event
                    ipCameraTimer = new System.Threading.Timer(FetchIpCameraFrame, null, 0, 100);
                    isIpCameraRunning = true;
                }
                
                LogInfo("Camera initialized successfully");
            }
            catch (Exception ex)
            {
                LogError($"Error initializing camera: {ex.Message}");
            }
        }

        private void FetchIpCameraFrame(object state)
        {
            if (!isIpCameraRunning || ipCameraClient == null || ipCameraCts.IsCancellationRequested)
                return;
                
            try
            {
                string url = GetCameraStreamUrl();
                var response = ipCameraClient.GetAsync(url, ipCameraCts.Token).Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var imageData = response.Content.ReadAsByteArrayAsync().Result;
                    using (var ms = new MemoryStream(imageData))
                    {
                        var frame = new Bitmap(ms);
                        DisplayFrame(frame);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching IP camera frame: {ex.Message}");
            }
        }
    }
} 