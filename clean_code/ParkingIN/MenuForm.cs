using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Serilog;
using ParkingIN.Models;

namespace ParkingIN
{
    public partial class MenuForm : Form
    {
        private System.Windows.Forms.Timer clockTimer;
        private WebSocketServer webSocketServer;
        private readonly ILogger _logger;
        private readonly User _currentUser;

        public MenuForm()
        {
            try
            {
                // Initialize logger
                _logger = Log.ForContext<MenuForm>();
                _logger.Information("Initializing MenuForm");
                
                // Get current user from LoginForm
                _currentUser = LoginForm.CurrentUser;
                _logger.Information($"Current user: {_currentUser.Username} (Role: {_currentUser.Role})");
                
                InitializeComponent();
                
                // Initialize and start WebSocket server
                InitializeWebSocketServer();
                
                SetupForm();
                
                _logger.Information("MenuForm initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error initializing MenuForm");
                MessageBox.Show($"Error initializing menu: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void InitializeWebSocketServer()
        {
            try
            {
                // Get WebSocket instance and start it
                webSocketServer = WebSocketServer.Instance;
                
                if (!webSocketServer.IsRunning)
                {
                    // Start the server with retry logic
                    int retryCount = 0;
                    const int maxRetries = 3;
                    bool success = false;
                    
                    while (retryCount < maxRetries && !success)
                    {
                        try
                        {
                            success = webSocketServer.Start();
                            if (success)
                            {
                                LogSystemMessage("WebSocket server started successfully");
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            LogError($"Failed to start WebSocket server (attempt {retryCount}/{maxRetries}): {ex.Message}", ex);
                            
                            if (retryCount < maxRetries)
                            {
                                System.Threading.Thread.Sleep(1000); // Wait 1 second before retry
                                continue;
                            }
                        }
                    }
                    
                    // Check if all retries failed
                    if (!success)
                    {
                        LogSystemMessage("Failed to start WebSocket server after maximum retries");
                    }
                }
                else
                {
                    LogSystemMessage("WebSocket server was already running");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error initializing WebSocket server: {ex.Message}", ex);
            }
        }

        private void SetupForm()
        {
            try
            {
                // Basic form setup
                this.Text = "Modern Parking System - Main Menu";
                this.Size = new Size(800, 500);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.FormBorderStyle = FormBorderStyle.FixedSingle;
                this.MaximizeBox = false;

                // Create header panel
                var headerPanel = new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 60,
                    BackColor = Color.FromArgb(0, 120, 215)
                };

                var lblTitle = new Label
                {
                    Text = "MODERN PARKING SYSTEM",
                    Font = new Font("Segoe UI", 20, FontStyle.Bold),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(20, 15)
                };

                var lblUser = new Label
                {
                    Text = $"Welcome, {_currentUser.NamaLengkap}",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.White,
                    AutoSize = true,
                    Location = new Point(headerPanel.Width - 200, 20)
                };

                headerPanel.Controls.AddRange(new Control[] { lblTitle, lblUser });

                // Create main menu buttons
                var btnEntryGate = CreateMenuButton("Entry Gate", 0);
                var btnExitGate = CreateMenuButton("Exit Gate", 1);
                var btnReports = CreateMenuButton("Reports", 2);
                var btnSettings = CreateMenuButton("Settings", 3);
                var btnLogout = CreateMenuButton("Logout", 4);

                // Add click handlers
                btnEntryGate.Click += (s, e) => OpenEntryGate();
                btnExitGate.Click += (s, e) => OpenExitGate();
                btnReports.Click += (s, e) => OpenReports();
                btnSettings.Click += (s, e) => OpenSettings();
                btnLogout.Click += (s, e) => Logout();

                // Add controls to form
                this.Controls.AddRange(new Control[] { 
                    headerPanel,
                    btnEntryGate,
                    btnExitGate,
                    btnReports,
                    btnSettings,
                    btnLogout
                });

                // Setup clock timer
                clockTimer = new System.Windows.Forms.Timer
                {
                    Interval = 1000
                };
                clockTimer.Tick += (s, e) => UpdateClock();
                clockTimer.Start();

                // Check if required directories exist
                EnsureDirectoriesExist();
                
                // Show build version
                lblVersion.Text = $"Version: {Application.ProductVersion}";
                
                // Broadcast initial system status
                if (webSocketServer != null && webSocketServer.IsRunning)
                {
                    webSocketServer.BroadcastSystemStatus();
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in SetupForm: {ex.Message}", ex);
                throw;
            }
        }

        private Button CreateMenuButton(string text, int index)
        {
            return new Button
            {
                Text = text,
                Size = new Size(200, 50),
                Location = new Point(300, 100 + (index * 70)),
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
        }

        private void UpdateClock()
        {
            // Update clock display if needed
        }

        private void OpenEntryGate()
        {
            try
            {
                _logger.Information("Opening Entry Gate form");
                var entryForm = new EntryForm(_currentUser);
                entryForm.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Entry Gate");
                MessageBox.Show("Error opening Entry Gate: " + ex.Message);
            }
        }

        private void OpenExitGate()
        {
            try
            {
                _logger.Information("Opening Exit Gate form");
                MessageBox.Show("Exit Gate functionality coming soon!");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Exit Gate");
                MessageBox.Show("Error opening Exit Gate: " + ex.Message);
            }
        }

        private void OpenReports()
        {
            try
            {
                _logger.Information("Opening Reports form");
                MessageBox.Show("Reports functionality coming soon!");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Reports");
                MessageBox.Show("Error opening Reports: " + ex.Message);
            }
        }

        private void OpenSettings()
        {
            try
            {
                _logger.Information("Opening Settings form");
                MessageBox.Show("Settings functionality coming soon!");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Settings");
                MessageBox.Show("Error opening Settings: " + ex.Message);
            }
        }

        private void Logout()
        {
            try
            {
                _logger.Information("User logging out");
                if (MessageBox.Show("Are you sure you want to logout?", "Confirm Logout",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Restart();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during logout");
                MessageBox.Show("Error during logout: " + ex.Message);
            }
        }

        private void EnsureDirectoriesExist()
        {
            try
            {
                // Ensure Images directory exists
                string imagesDir = Path.Combine(Application.StartupPath, "Images", "Entry");
                if (!Directory.Exists(imagesDir))
                {
                    Directory.CreateDirectory(imagesDir);
                }
                
                // Ensure logs directory exists
                string logsDir = Path.Combine(Application.StartupPath, "logs");
                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }
                
                // Ensure config directory exists
                string configDir = Path.Combine(Application.StartupPath, "config");
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
            }
            catch (Exception ex)
            {
                LogError($"Error creating directories: {ex.Message}", ex);
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Stop WebSocket server when application closes
            try
            {
                if (webSocketServer != null && webSocketServer.IsRunning)
                {
                    webSocketServer.Stop();
                    LogSystemMessage("WebSocket server stopped");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error stopping WebSocket server: {ex.Message}", ex);
            }
            
            base.OnFormClosing(e);
            if (e.CloseReason == CloseReason.UserClosing)
            {
                _logger.Information("MenuForm closing");
                Application.Exit();
            }
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
                
                string logMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logPath, logMsg);
            }
            catch
            {
                // Ignore logging errors
            }
        }
        
        private void LogError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "error.log");
                string directory = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [MenuForm] ERROR: {ex.Message}\nStackTrace: {ex.StackTrace}\n\n";
                File.AppendAllText(logPath, errorMsg);
            }
            catch
            {
                // Ignore errors in logging
            }
        }
        
        private void LogError(string message, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "error.log");
                string directory = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string errorMsg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [MenuForm] ERROR: {message}\nException: {ex.Message}\nStackTrace: {ex.StackTrace}\n\n";
                File.AppendAllText(logPath, errorMsg);
            }
            catch
            {
                // Ignore errors in logging
            }
        }

        #region Windows Form Designer generated code

        private Label lblTitle;
        private Label lblTimeCaption;
        private Label lblCurrentTime;
        private Button btnEntryGate;
        private Button btnCameraSettings;
        private Button btnGateSettings;
        private Button btnPrinterSettings;
        private Button btnReports;
        private Button btnViewLogs;
        private Button btnExit;
        private Panel pnlHeader;
        private Panel pnlFooter;
        private Label lblVersion;
        private PictureBox pictureBoxLogo;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblTimeCaption = new Label();
            this.lblCurrentTime = new Label();
            this.btnEntryGate = new Button();
            this.btnCameraSettings = new Button();
            this.btnGateSettings = new Button();
            this.btnPrinterSettings = new Button();
            this.btnReports = new Button();
            this.btnViewLogs = new Button();
            this.btnExit = new Button();
            this.pnlHeader = new Panel();
            this.pictureBoxLogo = new PictureBox();
            this.pnlFooter = new Panel();
            this.lblVersion = new Label();
            this.lblStatus = new Label();
            this.pnlHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).BeginInit();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.DarkGreen;
            this.pnlHeader.Controls.Add(this.pictureBoxLogo);
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(800, 80);
            this.pnlHeader.TabIndex = 0;
            // 
            // pictureBoxLogo
            // 
            this.pictureBoxLogo.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxLogo.Name = "pictureBoxLogo";
            this.pictureBoxLogo.Size = new System.Drawing.Size(60, 60);
            this.pictureBoxLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxLogo.TabIndex = 1;
            this.pictureBoxLogo.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(78, 22);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(405, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "PARKING SYSTEM - ENTRY GATE";
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.DarkGreen;
            this.pnlFooter.Controls.Add(this.lblVersion);
            this.pnlFooter.Controls.Add(this.lblTimeCaption);
            this.pnlFooter.Controls.Add(this.lblCurrentTime);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new System.Drawing.Point(0, 470);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(800, 30);
            this.pnlFooter.TabIndex = 1;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblVersion.ForeColor = System.Drawing.Color.White;
            this.lblVersion.Location = new System.Drawing.Point(12, 8);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(48, 15);
            this.lblVersion.TabIndex = 2;
            this.lblVersion.Text = "Version:";
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblStatus.ForeColor = System.Drawing.Color.Black;
            this.lblStatus.Location = new System.Drawing.Point(12, 440);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(776, 20);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "Ready";
            // 
            // lblTimeCaption
            // 
            this.lblTimeCaption.AutoSize = true;
            this.lblTimeCaption.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblTimeCaption.ForeColor = System.Drawing.Color.White;
            this.lblTimeCaption.Location = new System.Drawing.Point(570, 8);
            this.lblTimeCaption.Name = "lblTimeCaption";
            this.lblTimeCaption.Size = new System.Drawing.Size(84, 15);
            this.lblTimeCaption.TabIndex = 0;
            this.lblTimeCaption.Text = "Current Time: ";
            // 
            // lblCurrentTime
            // 
            this.lblCurrentTime.AutoSize = true;
            this.lblCurrentTime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCurrentTime.ForeColor = System.Drawing.Color.White;
            this.lblCurrentTime.Location = new System.Drawing.Point(660, 8);
            this.lblCurrentTime.Name = "lblCurrentTime";
            this.lblCurrentTime.Size = new System.Drawing.Size(125, 15);
            this.lblCurrentTime.TabIndex = 1;
            this.lblCurrentTime.Text = "00/00/0000 00:00:00";
            // 
            // btnEntryGate
            // 
            this.btnEntryGate.BackColor = System.Drawing.Color.Green;
            this.btnEntryGate.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnEntryGate.ForeColor = System.Drawing.Color.White;
            this.btnEntryGate.Location = new System.Drawing.Point(100, 120);
            this.btnEntryGate.Name = "btnEntryGate";
            this.btnEntryGate.Size = new System.Drawing.Size(250, 80);
            this.btnEntryGate.TabIndex = 2;
            this.btnEntryGate.Text = "ENTRY GATE";
            this.btnEntryGate.UseVisualStyleBackColor = false;
            this.btnEntryGate.Click += new System.EventHandler(this.btnEntryGate_Click);
            // 
            // btnCameraSettings
            // 
            this.btnCameraSettings.BackColor = System.Drawing.Color.DarkOrange;
            this.btnCameraSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnCameraSettings.ForeColor = System.Drawing.Color.White;
            this.btnCameraSettings.Location = new System.Drawing.Point(100, 220);
            this.btnCameraSettings.Name = "btnCameraSettings";
            this.btnCameraSettings.Size = new System.Drawing.Size(250, 60);
            this.btnCameraSettings.TabIndex = 3;
            this.btnCameraSettings.Text = "CAMERA SETTINGS";
            this.btnCameraSettings.UseVisualStyleBackColor = false;
            this.btnCameraSettings.Click += new System.EventHandler(this.btnCameraSettings_Click);
            // 
            // btnGateSettings
            // 
            this.btnGateSettings.BackColor = System.Drawing.Color.DarkOrange;
            this.btnGateSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnGateSettings.ForeColor = System.Drawing.Color.White;
            this.btnGateSettings.Location = new System.Drawing.Point(100, 300);
            this.btnGateSettings.Name = "btnGateSettings";
            this.btnGateSettings.Size = new System.Drawing.Size(250, 60);
            this.btnGateSettings.TabIndex = 4;
            this.btnGateSettings.Text = "GATE SETTINGS";
            this.btnGateSettings.UseVisualStyleBackColor = false;
            this.btnGateSettings.Click += new System.EventHandler(this.btnGateSettings_Click);
            // 
            // btnPrinterSettings
            // 
            this.btnPrinterSettings.BackColor = System.Drawing.Color.DarkOrange;
            this.btnPrinterSettings.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPrinterSettings.ForeColor = System.Drawing.Color.White;
            this.btnPrinterSettings.Location = new System.Drawing.Point(450, 220);
            this.btnPrinterSettings.Name = "btnPrinterSettings";
            this.btnPrinterSettings.Size = new System.Drawing.Size(250, 60);
            this.btnPrinterSettings.TabIndex = 5;
            this.btnPrinterSettings.Text = "PRINTER SETTINGS";
            this.btnPrinterSettings.UseVisualStyleBackColor = false;
            this.btnPrinterSettings.Click += new System.EventHandler(this.btnPrinterSettings_Click);
            // 
            // btnReports
            // 
            this.btnReports.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnReports.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnReports.ForeColor = System.Drawing.Color.White;
            this.btnReports.Location = new System.Drawing.Point(450, 120);
            this.btnReports.Name = "btnReports";
            this.btnReports.Size = new System.Drawing.Size(250, 80);
            this.btnReports.TabIndex = 6;
            this.btnReports.Text = "VIEW REPORTS";
            this.btnReports.UseVisualStyleBackColor = false;
            this.btnReports.Click += new System.EventHandler(this.btnReports_Click);
            // 
            // btnViewLogs
            // 
            this.btnViewLogs.BackColor = System.Drawing.Color.RoyalBlue;
            this.btnViewLogs.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnViewLogs.ForeColor = System.Drawing.Color.White;
            this.btnViewLogs.Location = new System.Drawing.Point(450, 300);
            this.btnViewLogs.Name = "btnViewLogs";
            this.btnViewLogs.Size = new System.Drawing.Size(250, 60);
            this.btnViewLogs.TabIndex = 7;
            this.btnViewLogs.Text = "VIEW LOGS";
            this.btnViewLogs.UseVisualStyleBackColor = false;
            this.btnViewLogs.Click += new System.EventHandler(this.btnViewLogs_Click);
            // 
            // btnExit
            // 
            this.btnExit.BackColor = System.Drawing.Color.DarkRed;
            this.btnExit.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnExit.ForeColor = System.Drawing.Color.White;
            this.btnExit.Location = new System.Drawing.Point(275, 390);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(250, 60);
            this.btnExit.TabIndex = 8;
            this.btnExit.Text = "EXIT";
            this.btnExit.UseVisualStyleBackColor = false;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // MenuForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnViewLogs);
            this.Controls.Add(this.btnReports);
            this.Controls.Add(this.btnPrinterSettings);
            this.Controls.Add(this.btnGateSettings);
            this.Controls.Add(this.btnCameraSettings);
            this.Controls.Add(this.btnEntryGate);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MenuForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Parking System - Entry Gate Menu";
            this.Load += new System.EventHandler(this.MenuForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLogo)).EndInit();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private void btnEntryGate_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Opening Entry Gate form");
                var entryForm = new EntryForm(_currentUser);
                entryForm.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Entry Gate form");
                MessageBox.Show($"Error opening Entry Gate: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCameraSettings_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Opening Camera Settings");
                MessageBox.Show("Camera Settings functionality coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Camera Settings");
                MessageBox.Show($"Error opening Camera Settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGateSettings_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Opening Gate Settings");
                MessageBox.Show("Gate Settings functionality coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Gate Settings");
                MessageBox.Show($"Error opening Gate Settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrinterSettings_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Opening Printer Settings");
                MessageBox.Show("Printer Settings functionality coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Printer Settings");
                MessageBox.Show($"Error opening Printer Settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReports_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Opening Reports");
                MessageBox.Show("Reports functionality coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Reports");
                MessageBox.Show($"Error opening Reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnViewLogs_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Opening Logs Viewer");
                MessageBox.Show("Logs Viewer functionality coming soon!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error opening Logs Viewer");
                MessageBox.Show($"Error opening Logs Viewer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("User clicked Exit button");
                if (MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during exit");
                MessageBox.Show($"Error during exit: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MenuForm_Load(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("MenuForm loading");
                lblCurrentTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                clockTimer.Start();
                
                // Update status
                lblStatus.Text = "Ready";
                
                // Load logo if exists
                string logoPath = Path.Combine(Application.StartupPath, "Images", "logo.png");
                if (File.Exists(logoPath))
                {
                    pictureBoxLogo.Image = Image.FromFile(logoPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error during form load");
                MessageBox.Show($"Error loading form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 