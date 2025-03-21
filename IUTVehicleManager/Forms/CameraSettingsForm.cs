using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video;
using AForge.Video.DirectShow;
using Newtonsoft.Json;
using System.IO;

namespace IUTVehicleManager.Forms
{
    public partial class CameraSettingsForm : Form
    {
        private FilterInfoCollection _videoDevices;
        private VideoCaptureDevice _videoSource;
        private bool _isPreviewRunning = false;
        private readonly string _configPath;
        private ILogger _logger;
        private CameraConfig _currentConfig;

        public CameraSettingsForm(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "camera_config.json");
            
            // Load default config if file doesn't exist
            _currentConfig = new CameraConfig
            {
                DeviceIndex = 0,
                Resolution = new Size(640, 480),
                Brightness = 0,
                Contrast = 0,
                CaptureIntervalMs = 1000,
                EnableRegionOfInterest = false,
                RegionOfInterest = new Rectangle(0, 0, 640, 480)
            };
        }

        private void InitializeComponent()
        {
            this.cmbCameras = new System.Windows.Forms.ComboBox();
            this.cmbResolutions = new System.Windows.Forms.ComboBox();
            this.btnStartPreview = new System.Windows.Forms.Button();
            this.btnStopPreview = new System.Windows.Forms.Button();
            this.picturePreview = new System.Windows.Forms.PictureBox();
            this.lblCamera = new System.Windows.Forms.Label();
            this.lblResolution = new System.Windows.Forms.Label();
            this.trackBrightness = new System.Windows.Forms.TrackBar();
            this.trackContrast = new System.Windows.Forms.TrackBar();
            this.lblBrightness = new System.Windows.Forms.Label();
            this.lblContrast = new System.Windows.Forms.Label();
            this.numCaptureInterval = new System.Windows.Forms.NumericUpDown();
            this.lblCaptureInterval = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkRegionOfInterest = new System.Windows.Forms.CheckBox();
            this.grpRegionOfInterest = new System.Windows.Forms.GroupBox();
            this.numRoiX = new System.Windows.Forms.NumericUpDown();
            this.numRoiY = new System.Windows.Forms.NumericUpDown();
            this.numRoiWidth = new System.Windows.Forms.NumericUpDown();
            this.numRoiHeight = new System.Windows.Forms.NumericUpDown();
            this.lblRoiX = new System.Windows.Forms.Label();
            this.lblRoiY = new System.Windows.Forms.Label();
            this.lblRoiWidth = new System.Windows.Forms.Label();
            this.lblRoiHeight = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBrightness)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackContrast)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCaptureInterval)).BeginInit();
            this.grpRegionOfInterest.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiHeight)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbCameras
            // 
            this.cmbCameras.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCameras.FormattingEnabled = true;
            this.cmbCameras.Location = new System.Drawing.Point(120, 15);
            this.cmbCameras.Name = "cmbCameras";
            this.cmbCameras.Size = new System.Drawing.Size(250, 23);
            this.cmbCameras.TabIndex = 0;
            this.cmbCameras.SelectedIndexChanged += new System.EventHandler(this.cmbCameras_SelectedIndexChanged);
            // 
            // cmbResolutions
            // 
            this.cmbResolutions.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResolutions.FormattingEnabled = true;
            this.cmbResolutions.Location = new System.Drawing.Point(120, 45);
            this.cmbResolutions.Name = "cmbResolutions";
            this.cmbResolutions.Size = new System.Drawing.Size(250, 23);
            this.cmbResolutions.TabIndex = 1;
            // 
            // btnStartPreview
            // 
            this.btnStartPreview.Location = new System.Drawing.Point(400, 15);
            this.btnStartPreview.Name = "btnStartPreview";
            this.btnStartPreview.Size = new System.Drawing.Size(120, 30);
            this.btnStartPreview.TabIndex = 2;
            this.btnStartPreview.Text = "Start Preview";
            this.btnStartPreview.UseVisualStyleBackColor = true;
            this.btnStartPreview.Click += new System.EventHandler(this.btnStartPreview_Click);
            // 
            // btnStopPreview
            // 
            this.btnStopPreview.Enabled = false;
            this.btnStopPreview.Location = new System.Drawing.Point(400, 55);
            this.btnStopPreview.Name = "btnStopPreview";
            this.btnStopPreview.Size = new System.Drawing.Size(120, 30);
            this.btnStopPreview.TabIndex = 3;
            this.btnStopPreview.Text = "Stop Preview";
            this.btnStopPreview.UseVisualStyleBackColor = true;
            this.btnStopPreview.Click += new System.EventHandler(this.btnStopPreview_Click);
            // 
            // picturePreview
            // 
            this.picturePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picturePreview.Location = new System.Drawing.Point(15, 100);
            this.picturePreview.Name = "picturePreview";
            this.picturePreview.Size = new System.Drawing.Size(505, 320);
            this.picturePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picturePreview.TabIndex = 4;
            this.picturePreview.TabStop = false;
            // 
            // lblCamera
            // 
            this.lblCamera.AutoSize = true;
            this.lblCamera.Location = new System.Drawing.Point(15, 18);
            this.lblCamera.Name = "lblCamera";
            this.lblCamera.Size = new System.Drawing.Size(51, 15);
            this.lblCamera.TabIndex = 5;
            this.lblCamera.Text = "Camera:";
            // 
            // lblResolution
            // 
            this.lblResolution.AutoSize = true;
            this.lblResolution.Location = new System.Drawing.Point(15, 48);
            this.lblResolution.Name = "lblResolution";
            this.lblResolution.Size = new System.Drawing.Size(66, 15);
            this.lblResolution.TabIndex = 6;
            this.lblResolution.Text = "Resolution:";
            // 
            // trackBrightness
            // 
            this.trackBrightness.Location = new System.Drawing.Point(120, 440);
            this.trackBrightness.Maximum = 100;
            this.trackBrightness.Minimum = -100;
            this.trackBrightness.Name = "trackBrightness";
            this.trackBrightness.Size = new System.Drawing.Size(250, 45);
            this.trackBrightness.TabIndex = 7;
            this.trackBrightness.TickFrequency = 10;
            // 
            // trackContrast
            // 
            this.trackContrast.Location = new System.Drawing.Point(120, 485);
            this.trackContrast.Maximum = 100;
            this.trackContrast.Minimum = -100;
            this.trackContrast.Name = "trackContrast";
            this.trackContrast.Size = new System.Drawing.Size(250, 45);
            this.trackContrast.TabIndex = 8;
            this.trackContrast.TickFrequency = 10;
            // 
            // lblBrightness
            // 
            this.lblBrightness.AutoSize = true;
            this.lblBrightness.Location = new System.Drawing.Point(15, 440);
            this.lblBrightness.Name = "lblBrightness";
            this.lblBrightness.Size = new System.Drawing.Size(65, 15);
            this.lblBrightness.TabIndex = 9;
            this.lblBrightness.Text = "Brightness:";
            // 
            // lblContrast
            // 
            this.lblContrast.AutoSize = true;
            this.lblContrast.Location = new System.Drawing.Point(15, 485);
            this.lblContrast.Name = "lblContrast";
            this.lblContrast.Size = new System.Drawing.Size(55, 15);
            this.lblContrast.TabIndex = 10;
            this.lblContrast.Text = "Contrast:";
            // 
            // numCaptureInterval
            // 
            this.numCaptureInterval.Location = new System.Drawing.Point(120, 530);
            this.numCaptureInterval.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.numCaptureInterval.Minimum = new decimal(new int[] { 100, 0, 0, 0 });
            this.numCaptureInterval.Name = "numCaptureInterval";
            this.numCaptureInterval.Size = new System.Drawing.Size(120, 23);
            this.numCaptureInterval.TabIndex = 11;
            this.numCaptureInterval.Value = new decimal(new int[] { 1000, 0, 0, 0 });
            // 
            // lblCaptureInterval
            // 
            this.lblCaptureInterval.AutoSize = true;
            this.lblCaptureInterval.Location = new System.Drawing.Point(15, 532);
            this.lblCaptureInterval.Name = "lblCaptureInterval";
            this.lblCaptureInterval.Size = new System.Drawing.Size(99, 15);
            this.lblCaptureInterval.TabIndex = 12;
            this.lblCaptureInterval.Text = "Capture Interval:";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(175, 580);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(150, 35);
            this.btnSave.TabIndex = 13;
            this.btnSave.Text = "Save Settings";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(340, 580);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(150, 35);
            this.btnCancel.TabIndex = 14;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkRegionOfInterest
            // 
            this.chkRegionOfInterest.AutoSize = true;
            this.chkRegionOfInterest.Location = new System.Drawing.Point(550, 100);
            this.chkRegionOfInterest.Name = "chkRegionOfInterest";
            this.chkRegionOfInterest.Size = new System.Drawing.Size(157, 19);
            this.chkRegionOfInterest.TabIndex = 15;
            this.chkRegionOfInterest.Text = "Enable Region of Interest";
            this.chkRegionOfInterest.UseVisualStyleBackColor = true;
            this.chkRegionOfInterest.CheckedChanged += new System.EventHandler(this.chkRegionOfInterest_CheckedChanged);
            // 
            // grpRegionOfInterest
            // 
            this.grpRegionOfInterest.Controls.Add(this.lblRoiHeight);
            this.grpRegionOfInterest.Controls.Add(this.lblRoiWidth);
            this.grpRegionOfInterest.Controls.Add(this.lblRoiY);
            this.grpRegionOfInterest.Controls.Add(this.lblRoiX);
            this.grpRegionOfInterest.Controls.Add(this.numRoiHeight);
            this.grpRegionOfInterest.Controls.Add(this.numRoiWidth);
            this.grpRegionOfInterest.Controls.Add(this.numRoiY);
            this.grpRegionOfInterest.Controls.Add(this.numRoiX);
            this.grpRegionOfInterest.Enabled = false;
            this.grpRegionOfInterest.Location = new System.Drawing.Point(550, 125);
            this.grpRegionOfInterest.Name = "grpRegionOfInterest";
            this.grpRegionOfInterest.Size = new System.Drawing.Size(220, 150);
            this.grpRegionOfInterest.TabIndex = 16;
            this.grpRegionOfInterest.TabStop = false;
            this.grpRegionOfInterest.Text = "Region of Interest";
            // 
            // numRoiX
            // 
            this.numRoiX.Location = new System.Drawing.Point(100, 25);
            this.numRoiX.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.numRoiX.Name = "numRoiX";
            this.numRoiX.Size = new System.Drawing.Size(100, 23);
            this.numRoiX.TabIndex = 0;
            // 
            // numRoiY
            // 
            this.numRoiY.Location = new System.Drawing.Point(100, 55);
            this.numRoiY.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.numRoiY.Name = "numRoiY";
            this.numRoiY.Size = new System.Drawing.Size(100, 23);
            this.numRoiY.TabIndex = 1;
            // 
            // numRoiWidth
            // 
            this.numRoiWidth.Location = new System.Drawing.Point(100, 85);
            this.numRoiWidth.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.numRoiWidth.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numRoiWidth.Name = "numRoiWidth";
            this.numRoiWidth.Size = new System.Drawing.Size(100, 23);
            this.numRoiWidth.TabIndex = 2;
            this.numRoiWidth.Value = new decimal(new int[] { 640, 0, 0, 0 });
            // 
            // numRoiHeight
            // 
            this.numRoiHeight.Location = new System.Drawing.Point(100, 115);
            this.numRoiHeight.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            this.numRoiHeight.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            this.numRoiHeight.Name = "numRoiHeight";
            this.numRoiHeight.Size = new System.Drawing.Size(100, 23);
            this.numRoiHeight.TabIndex = 3;
            this.numRoiHeight.Value = new decimal(new int[] { 480, 0, 0, 0 });
            // 
            // lblRoiX
            // 
            this.lblRoiX.AutoSize = true;
            this.lblRoiX.Location = new System.Drawing.Point(15, 27);
            this.lblRoiX.Name = "lblRoiX";
            this.lblRoiX.Size = new System.Drawing.Size(17, 15);
            this.lblRoiX.TabIndex = 4;
            this.lblRoiX.Text = "X:";
            // 
            // lblRoiY
            // 
            this.lblRoiY.AutoSize = true;
            this.lblRoiY.Location = new System.Drawing.Point(15, 57);
            this.lblRoiY.Name = "lblRoiY";
            this.lblRoiY.Size = new System.Drawing.Size(17, 15);
            this.lblRoiY.TabIndex = 5;
            this.lblRoiY.Text = "Y:";
            // 
            // lblRoiWidth
            // 
            this.lblRoiWidth.AutoSize = true;
            this.lblRoiWidth.Location = new System.Drawing.Point(15, 87);
            this.lblRoiWidth.Name = "lblRoiWidth";
            this.lblRoiWidth.Size = new System.Drawing.Size(42, 15);
            this.lblRoiWidth.TabIndex = 6;
            this.lblRoiWidth.Text = "Width:";
            // 
            // lblRoiHeight
            // 
            this.lblRoiHeight.AutoSize = true;
            this.lblRoiHeight.Location = new System.Drawing.Point(15, 117);
            this.lblRoiHeight.Name = "lblRoiHeight";
            this.lblRoiHeight.Size = new System.Drawing.Size(46, 15);
            this.lblRoiHeight.TabIndex = 7;
            this.lblRoiHeight.Text = "Height:";
            // 
            // CameraSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 631);
            this.Controls.Add(this.grpRegionOfInterest);
            this.Controls.Add(this.chkRegionOfInterest);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.lblCaptureInterval);
            this.Controls.Add(this.numCaptureInterval);
            this.Controls.Add(this.lblContrast);
            this.Controls.Add(this.lblBrightness);
            this.Controls.Add(this.trackContrast);
            this.Controls.Add(this.trackBrightness);
            this.Controls.Add(this.lblResolution);
            this.Controls.Add(this.lblCamera);
            this.Controls.Add(this.picturePreview);
            this.Controls.Add(this.btnStopPreview);
            this.Controls.Add(this.btnStartPreview);
            this.Controls.Add(this.cmbResolutions);
            this.Controls.Add(this.cmbCameras);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "CameraSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Camera Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraSettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.CameraSettingsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picturePreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBrightness)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackContrast)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numCaptureInterval)).EndInit();
            this.grpRegionOfInterest.ResumeLayout(false);
            this.grpRegionOfInterest.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRoiHeight)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void CameraSettingsForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Load available cameras
                LoadAvailableCameras();
                
                // Load saved configuration if exists
                LoadConfiguration();
                
                // Apply loaded configuration to UI
                ApplyConfigToUI();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing camera settings form: {ex.Message}");
                MessageBox.Show($"Error loading camera settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAvailableCameras()
        {
            try
            {
                _videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                
                if (_videoDevices.Count == 0)
                {
                    _logger.Warning("No video devices found");
                    MessageBox.Show("No video devices found on this system.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                cmbCameras.Items.Clear();
                foreach (FilterInfo device in _videoDevices)
                {
                    cmbCameras.Items.Add(device.Name);
                }
                
                if (cmbCameras.Items.Count > 0)
                {
                    cmbCameras.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading available cameras: {ex.Message}");
                throw;
            }
        }

        private void LoadConfiguration()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    _currentConfig = JsonConvert.DeserializeObject<CameraConfig>(json);
                    _logger.Info("Camera configuration loaded from file");
                }
                else
                {
                    _logger.Info("No camera configuration file found, using defaults");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading camera configuration: {ex.Message}");
                throw;
            }
        }

        private void ApplyConfigToUI()
        {
            try
            {
                // Set camera selection
                if (_videoDevices != null && _videoDevices.Count > _currentConfig.DeviceIndex)
                {
                    cmbCameras.SelectedIndex = _currentConfig.DeviceIndex;
                }
                
                // Set brightness and contrast
                trackBrightness.Value = _currentConfig.Brightness;
                trackContrast.Value = _currentConfig.Contrast;
                
                // Set capture interval
                numCaptureInterval.Value = _currentConfig.CaptureIntervalMs;
                
                // Set ROI settings
                chkRegionOfInterest.Checked = _currentConfig.EnableRegionOfInterest;
                numRoiX.Value = _currentConfig.RegionOfInterest.X;
                numRoiY.Value = _currentConfig.RegionOfInterest.Y;
                numRoiWidth.Value = _currentConfig.RegionOfInterest.Width;
                numRoiHeight.Value = _currentConfig.RegionOfInterest.Height;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error applying configuration to UI: {ex.Message}");
                throw;
            }
        }

        private void cmbCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int deviceIndex = cmbCameras.SelectedIndex;
                if (deviceIndex >= 0 && _videoDevices.Count > deviceIndex)
                {
                    LoadResolutions(deviceIndex);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error selecting camera: {ex.Message}");
                MessageBox.Show($"Error selecting camera: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadResolutions(int deviceIndex)
        {
            try
            {
                cmbResolutions.Items.Clear();
                
                // Create temporary video source to get capabilities
                var tempSource = new VideoCaptureDevice(_videoDevices[deviceIndex].MonikerString);
                
                foreach (var capability in tempSource.VideoCapabilities)
                {
                    cmbResolutions.Items.Add($"{capability.FrameSize.Width} x {capability.FrameSize.Height}");
                }
                
                // Select the default or previously used resolution
                if (cmbResolutions.Items.Count > 0)
                {
                    // Find closest match to current config
                    int bestIndex = 0;
                    int minDiff = int.MaxValue;
                    
                    for (int i = 0; i < tempSource.VideoCapabilities.Length; i++)
                    {
                        var cap = tempSource.VideoCapabilities[i];
                        int diff = Math.Abs(cap.FrameSize.Width - _currentConfig.Resolution.Width) + 
                                  Math.Abs(cap.FrameSize.Height - _currentConfig.Resolution.Height);
                        
                        if (diff < minDiff)
                        {
                            minDiff = diff;
                            bestIndex = i;
                        }
                    }
                    
                    cmbResolutions.SelectedIndex = bestIndex;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading resolutions: {ex.Message}");
                throw;
            }
        }

        private void btnStartPreview_Click(object sender, EventArgs e)
        {
            try
            {
                if (_isPreviewRunning)
                    return;
                
                int deviceIndex = cmbCameras.SelectedIndex;
                if (deviceIndex < 0 || _videoDevices.Count <= deviceIndex)
                {
                    MessageBox.Show("Please select a camera first.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                _videoSource = new VideoCaptureDevice(_videoDevices[deviceIndex].MonikerString);
                
                // Set selected resolution
                if (cmbResolutions.SelectedIndex >= 0 && _videoSource.VideoCapabilities.Length > 0)
                {
                    _videoSource.VideoResolution = _videoSource.VideoCapabilities[cmbResolutions.SelectedIndex];
                }
                
                _videoSource.NewFrame += VideoSource_NewFrame;
                _videoSource.Start();
                
                _isPreviewRunning = true;
                btnStartPreview.Enabled = false;
                btnStopPreview.Enabled = true;
                cmbCameras.Enabled = false;
                cmbResolutions.Enabled = false;
                
                _logger.Info("Camera preview started");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error starting camera preview: {ex.Message}");
                MessageBox.Show($"Error starting camera preview: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Make a copy of the frame to display in the picture box
                Bitmap frame = (Bitmap)eventArgs.Frame.Clone();
                
                // Apply ROI rectangle if enabled
                if (chkRegionOfInterest.Checked)
                {
                    using (Graphics g = Graphics.FromImage(frame))
                    {
                        int x = (int)numRoiX.Value;
                        int y = (int)numRoiY.Value;
                        int width = (int)numRoiWidth.Value;
                        int height = (int)numRoiHeight.Value;
                        
                        g.DrawRectangle(Pens.Red, x, y, width, height);
                    }
                }
                
                // Update PictureBox
                if (picturePreview.InvokeRequired)
                {
                    picturePreview.Invoke(new MethodInvoker(delegate {
                        picturePreview.Image?.Dispose();
                        picturePreview.Image = frame;
                    }));
                }
                else
                {
                    picturePreview.Image?.Dispose();
                    picturePreview.Image = frame;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error processing video frame: {ex.Message}");
            }
        }

        private void btnStopPreview_Click(object sender, EventArgs e)
        {
            StopPreview();
        }

        private void StopPreview()
        {
            try
            {
                if (_videoSource != null && _videoSource.IsRunning)
                {
                    _videoSource.SignalToStop();
                    _videoSource.WaitForStop();
                    _videoSource.NewFrame -= VideoSource_NewFrame;
                    _videoSource = null;
                }
                
                _isPreviewRunning = false;
                btnStartPreview.Enabled = true;
                btnStopPreview.Enabled = false;
                cmbCameras.Enabled = true;
                cmbResolutions.Enabled = true;
                
                // Clear preview image
                if (picturePreview.Image != null)
                {
                    picturePreview.Image.Dispose();
                    picturePreview.Image = null;
                }
                
                _logger.Info("Camera preview stopped");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error stopping camera preview: {ex.Message}");
            }
        }

        private void chkRegionOfInterest_CheckedChanged(object sender, EventArgs e)
        {
            grpRegionOfInterest.Enabled = chkRegionOfInterest.Checked;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Stop preview if running
                if (_isPreviewRunning)
                {
                    StopPreview();
                }
                
                // Update config object
                _currentConfig.DeviceIndex = cmbCameras.SelectedIndex;
                
                // Parse resolution
                if (cmbResolutions.SelectedIndex >= 0 && _videoDevices.Count > 0)
                {
                    var tempSource = new VideoCaptureDevice(_videoDevices[cmbCameras.SelectedIndex].MonikerString);
                    if (tempSource.VideoCapabilities.Length > cmbResolutions.SelectedIndex)
                    {
                        var selectedCapability = tempSource.VideoCapabilities[cmbResolutions.SelectedIndex];
                        _currentConfig.Resolution = new Size(selectedCapability.FrameSize.Width, selectedCapability.FrameSize.Height);
                    }
                }
                
                _currentConfig.Brightness = trackBrightness.Value;
                _currentConfig.Contrast = trackContrast.Value;
                _currentConfig.CaptureIntervalMs = (int)numCaptureInterval.Value;
                _currentConfig.EnableRegionOfInterest = chkRegionOfInterest.Checked;
                _currentConfig.RegionOfInterest = new Rectangle(
                    (int)numRoiX.Value,
                    (int)numRoiY.Value,
                    (int)numRoiWidth.Value,
                    (int)numRoiHeight.Value
                );
                
                // Save to file
                string json = JsonConvert.SerializeObject(_currentConfig, Formatting.Indented);
                File.WriteAllText(_configPath, json);
                
                _logger.Info("Camera settings saved successfully");
                MessageBox.Show("Camera settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving camera settings: {ex.Message}");
                MessageBox.Show($"Error saving camera settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CameraSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop preview if running
            if (_isPreviewRunning)
            {
                StopPreview();
            }
        }

        // Controls
        private System.Windows.Forms.ComboBox cmbCameras;
        private System.Windows.Forms.ComboBox cmbResolutions;
        private System.Windows.Forms.Button btnStartPreview;
        private System.Windows.Forms.Button btnStopPreview;
        private System.Windows.Forms.PictureBox picturePreview;
        private System.Windows.Forms.Label lblCamera;
        private System.Windows.Forms.Label lblResolution;
        private System.Windows.Forms.TrackBar trackBrightness;
        private System.Windows.Forms.TrackBar trackContrast;
        private System.Windows.Forms.Label lblBrightness;
        private System.Windows.Forms.Label lblContrast;
        private System.Windows.Forms.NumericUpDown numCaptureInterval;
        private System.Windows.Forms.Label lblCaptureInterval;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.CheckBox chkRegionOfInterest;
        private System.Windows.Forms.GroupBox grpRegionOfInterest;
        private System.Windows.Forms.NumericUpDown numRoiX;
        private System.Windows.Forms.NumericUpDown numRoiY;
        private System.Windows.Forms.NumericUpDown numRoiWidth;
        private System.Windows.Forms.NumericUpDown numRoiHeight;
        private System.Windows.Forms.Label lblRoiX;
        private System.Windows.Forms.Label lblRoiY;
        private System.Windows.Forms.Label lblRoiWidth;
        private System.Windows.Forms.Label lblRoiHeight;
    }

    public class CameraConfig
    {
        public int DeviceIndex { get; set; }
        public Size Resolution { get; set; }
        public int Brightness { get; set; }
        public int Contrast { get; set; }
        public int CaptureIntervalMs { get; set; }
        public bool EnableRegionOfInterest { get; set; }
        public Rectangle RegionOfInterest { get; set; }
    }
} 