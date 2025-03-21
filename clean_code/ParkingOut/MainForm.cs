using System;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using SimpleParkingAdmin.Forms;
using SimpleParkingAdmin.Models;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class MainForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;
        private Panel sidePanel;
        private Panel headerPanel;
        private Label lblHeader;
        private Label lblUserName;
        private Label lblUserRole;
        private Panel contentContainer;
        
        // Content panels
        private DashboardPanel dashboardPanel;
        private Panel parkingPanel;
        private Panel exitPanel;
        private Panel memberPanel;
        private Panel reportsPanel;
        private Panel usersPanel;
        private Panel settingsPanel;
        
        // Sidebar buttons
        private Button btnDashboard;
        private Button btnParkingEntry;
        private Button btnParkingExit;
        private Button btnMembers;
        private Button btnReports;
        private Button btnUsers;
        private Button btnSettings;
        private Button btnLogout;
        
        // Active button tracking
        private Button currentActiveButton;
        private Color activeButtonColor = Color.FromArgb(0, 102, 204);
        private Color inactiveButtonColor = Color.FromArgb(45, 45, 48);
        
        // Shadow effect
        private const int CS_DROPSHADOW = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
        
        public MainForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = new FileLogger();
            InitializeComponent();
            
            // Handle any uncaught exceptions
            Application.ThreadException += (sender, e) => {
                Debug.WriteLine($"Error no controlado: {e.Exception.Message}");
                using (ErrorHandlerForm errorForm = new ErrorHandlerForm(e.Exception))
                {
                    errorForm.ShowDialog();
                }
            };
            
            // Ensure required directories exist
            EnsureDirectoriesExist();
            
            // Configure access levels based on user role
            ConfigureAccessLevels();
            
            // Initialize the dashboard
            LoadDashboardData();
            
            // Create dashboard panel (with dark theme)
            dashboardPanel = new DashboardPanel(_currentUser);
            
            // Show dashboard
            contentContainer.Controls.Add(dashboardPanel);
            dashboardPanel.BringToFront();
            dashboardPanel.Visible = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "SimpleParkingAdmin - Modern Parking System";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.MinimumSize = new Size(1000, 700);
            
            // Initialize panels
            parkingPanel = new Panel();
            exitPanel = new Panel();
            memberPanel = new Panel();
            reportsPanel = new Panel();
            usersPanel = new Panel();
            settingsPanel = new Panel();
            
            // Set panel properties
            foreach (var panel in new[] { parkingPanel, exitPanel, memberPanel, reportsPanel, usersPanel, settingsPanel })
            {
                panel.Dock = DockStyle.Fill;
                panel.BackColor = Color.White;
                panel.Visible = false;
            }
            
            // Header Panel
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            headerPanel.BackColor = Color.FromArgb(0, 120, 215);
            
            // Header title
            lblHeader = new Label();
            lblHeader.Text = "MODERN PARKING SYSTEM";
            lblHeader.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblHeader.ForeColor = Color.White;
            lblHeader.Location = new Point(20, 15);
            lblHeader.AutoSize = true;
            headerPanel.Controls.Add(lblHeader);
            
            // User info panel (in header)
            Panel userPanel = new Panel();
            userPanel.Dock = DockStyle.Right;
            userPanel.Width = 200;
            userPanel.BackColor = Color.Transparent;
            
            // User name label
            lblUserName = new Label();
            lblUserName.Text = _currentUser?.NamaLengkap ?? "User";
            lblUserName.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            lblUserName.ForeColor = Color.White;
            lblUserName.Location = new Point(5, 10);
            lblUserName.AutoSize = true;
            userPanel.Controls.Add(lblUserName);
            
            // User role label
            lblUserRole = new Label();
            lblUserRole.Text = _currentUser?.Level ?? "User";
            lblUserRole.Font = new Font("Segoe UI", 8);
            lblUserRole.ForeColor = Color.FromArgb(230, 230, 230);
            lblUserRole.Location = new Point(5, 30);
            lblUserRole.AutoSize = true;
            userPanel.Controls.Add(lblUserRole);
            
            headerPanel.Controls.Add(userPanel);
            
            // Side Panel
            sidePanel = new Panel();
            sidePanel.Width = 220;
            sidePanel.Dock = DockStyle.Left;
            sidePanel.BackColor = Color.FromArgb(45, 45, 48);
            sidePanel.Padding = new Padding(0, 10, 0, 10);
            
            // Dashboard Button
            btnDashboard = CreateSidebarButton("Dashboard", "Home", 0);
            btnDashboard.Click += (s, e) => {
                if (dashboardPanel == null)
                {
                    // Create dashboard panel
                    dashboardPanel = new DashboardPanel(_currentUser);
                    dashboardPanel.Dock = DockStyle.Fill;
                    contentContainer.Controls.Add(dashboardPanel);
                }
                ShowPanel(dashboardPanel);
                SetActiveButton(btnDashboard);
            };
            sidePanel.Controls.Add(btnDashboard);
            
            // Combined Entry/Exit Button
            Button btnCombinedEntryExit = CreateSidebarButton("In & Out", "Combined", 1);
            btnCombinedEntryExit.Click += (s, e) => {
                // Create new panel each time to ensure fresh instance
                Panel combinedPanel = new Panel();
                combinedPanel.Dock = DockStyle.Fill;
                combinedPanel.BackColor = Color.White;
                
                // Add the CombinedEntryExitForm inside this panel
                CombinedEntryExitForm combinedForm = new CombinedEntryExitForm(_currentUser);
                combinedForm.TopLevel = false;
                combinedForm.FormBorderStyle = FormBorderStyle.None;
                combinedForm.Dock = DockStyle.Fill;
                
                combinedPanel.Controls.Add(combinedForm);
                combinedForm.Show();
                
                contentContainer.Controls.Clear();
                contentContainer.Controls.Add(combinedPanel);
                SetActiveButton(btnCombinedEntryExit);
            };
            sidePanel.Controls.Add(btnCombinedEntryExit);
            
            // Parking Entry Button
            btnParkingEntry = CreateSidebarButton("Kendaraan Masuk", "Entry", 2);
            btnParkingEntry.Click += (s, e) => {
                ShowParkingEntry();
            };
            sidePanel.Controls.Add(btnParkingEntry);
            
            // Parking Exit Button
            btnParkingExit = CreateSidebarButton("Kendaraan Keluar", "Exit", 3);
            btnParkingExit.Click += (s, e) => {
                ShowParkingExit();
            };
            sidePanel.Controls.Add(btnParkingExit);
            
            // Members Button
            btnMembers = CreateSidebarButton("Member", "Users", 4);
            btnMembers.Click += (s, e) => {
                ShowMemberManagement();
            };
            sidePanel.Controls.Add(btnMembers);
            
            // Reports Button
            btnReports = CreateSidebarButton("Laporan", "Reports", 5);
            btnReports.Click += (s, e) => {
                ShowReports();
            };
            sidePanel.Controls.Add(btnReports);
            
            // Users Button (Only for Admin)
            btnUsers = CreateSidebarButton("Pengguna", "User Management", 6);
            btnUsers.Click += (s, e) => {
                if (usersPanel == null)
                {
                    usersPanel = new Panel();
                    usersPanel.Dock = DockStyle.Fill;
                    usersPanel.BackColor = Color.White;
                    
                    // Add the UserManagementForm as the main content
                    UserManagementForm userForm = new UserManagementForm();
                    userForm.TopLevel = false;
                    userForm.FormBorderStyle = FormBorderStyle.None;
                    userForm.Dock = DockStyle.Fill;
                    
                    usersPanel.Controls.Add(userForm);
                    userForm.Show();
                    
                    contentContainer.Controls.Add(usersPanel);
                }
                
                ShowPanel(usersPanel);
                SetActiveButton(btnUsers);
            };
            sidePanel.Controls.Add(btnUsers);
            
            // Settings Button
            btnSettings = CreateSidebarButton("Pengaturan", "Settings", 7);
            btnSettings.Click += (s, e) => {
                if (settingsPanel == null)
                {
                    // Create settings panel
                    settingsPanel = new Panel();
                    settingsPanel.BackColor = Color.White;
                    settingsPanel.Dock = DockStyle.Fill;
                    settingsPanel.Padding = new Padding(20);
                    
                    // Add settings buttons
                    Button btnDatabaseSettings = new Button();
                    btnDatabaseSettings.Text = "Database Settings";
                    btnDatabaseSettings.Size = new Size(200, 40);
                    btnDatabaseSettings.Location = new Point(20, 20);
                    btnDatabaseSettings.Click += (sender, evt) => {
                        using (var form = new NetworkSettingsForm())
                        {
                            form.ShowDialog();
                        }
                    };
                    
                    Button btnCameraSettings = new Button();
                    btnCameraSettings.Text = "Camera Settings";
                    btnCameraSettings.Size = new Size(200, 40);
                    btnCameraSettings.Location = new Point(20, 70);
                    btnCameraSettings.Click += (sender, evt) => {
                        using (var form = new IPCameraSettingsForm())
                        {
                            form.ShowDialog();
                        }
                    };
                    
                    Button btnTariffSettings = new Button();
                    btnTariffSettings.Text = "Parking Tariff Settings";
                    btnTariffSettings.Size = new Size(200, 40);
                    btnTariffSettings.Location = new Point(20, 120);
                    btnTariffSettings.Click += (sender, evt) => {
                        using (var form = new TarifForm())
                        {
                            form.ShowDialog();
                        }
                    };
                    
                    settingsPanel.Controls.Add(btnDatabaseSettings);
                    settingsPanel.Controls.Add(btnCameraSettings);
                    settingsPanel.Controls.Add(btnTariffSettings);
                    
                    contentContainer.Controls.Add(settingsPanel);
                }
                
                ShowPanel(settingsPanel);
                SetActiveButton(btnSettings);
            };
            sidePanel.Controls.Add(btnSettings);
            
            // Logout Button (at the bottom)
            btnLogout = CreateSidebarButton("Logout", "Logout", -1);
            btnLogout.Dock = DockStyle.Bottom;
            btnLogout.Click += (s, e) => {
                DialogResult result = MessageBox.Show(
                    "Apakah Anda yakin ingin keluar dari sistem?",
                    "Konfirmasi Logout",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    // Close this form and return to login
                    this.Close();
                }
            };
            sidePanel.Controls.Add(btnLogout);
            
            // Content Container - this will hold all the different panels
            contentContainer = new Panel();
            contentContainer.Dock = DockStyle.Fill;
            contentContainer.BackColor = Color.FromArgb(245, 246, 250);
            contentContainer.Padding = new Padding(0);
            
            // Add panels to the form
            this.Controls.Add(contentContainer);
            this.Controls.Add(sidePanel);
            this.Controls.Add(headerPanel);
            
            this.ResumeLayout(false);
        }
        
        private Button CreateSidebarButton(string text, string icon, int position)
        {
            Button btn = new Button();
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = inactiveButtonColor;
            btn.ForeColor = Color.White;
            btn.Text = "   " + text; // Space for icon
            btn.Font = new Font("Segoe UI", 10F);
            btn.Height = 45;
            btn.Dock = DockStyle.Top;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(15, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            
            // Set position in the sidebar (if not special case like logout)
            if (position >= 0)
            {
                btn.Tag = position;
            }
            
            // Add hover effect
            btn.MouseEnter += (s, e) => {
                if (btn != currentActiveButton)
                    btn.BackColor = Color.FromArgb(55, 55, 58);
            };
            
            btn.MouseLeave += (s, e) => {
                if (btn != currentActiveButton)
                    btn.BackColor = inactiveButtonColor;
            };
            
            return btn;
        }
        
        private void ShowPanel(Control panel)
        {
            // Hide all panels
            foreach (Control ctrl in contentContainer.Controls)
            {
                ctrl.Visible = false;
            }
            
            // Show the selected panel
            panel.Visible = true;
            panel.BringToFront();
            
            // Update header text based on active panel
            if (panel == dashboardPanel)
                lblHeader.Text = "DASHBOARD";
            else if (panel == parkingPanel)
                lblHeader.Text = "KENDARAAN MASUK/KELUAR";
            else if (panel == reportsPanel)
                lblHeader.Text = "LAPORAN";
            else if (panel == settingsPanel)
                lblHeader.Text = "PENGATURAN";
        }
        
        private void SetActiveButton(Button button)
        {
            // Reset current active button
            if (currentActiveButton != null)
            {
                currentActiveButton.BackColor = inactiveButtonColor;
                currentActiveButton.ForeColor = Color.White;
                currentActiveButton.Font = new Font("Segoe UI", 10F);
            }
            
            // Set new active button
            button.BackColor = activeButtonColor;
            button.ForeColor = Color.White;
            button.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            
            currentActiveButton = button;
        }
        
        // Helper method to create a simple P icon for the form
        private Image CreateParkingIcon()
        {
            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw circle
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    g.FillEllipse(brush, 4, 4, 56, 56);
                }
                
                // Draw P
                using (Font font = new Font("Arial", 32, FontStyle.Bold))
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("P", font, textBrush, new RectangleF(4, 4, 56, 56), sf);
                }
            }
            return bmp;
        }
        
        private void ConfigureAccessLevels()
        {
            // Setup access rights based on user level
            if (_currentUser != null)
            {
                if (_currentUser.IsAdmin)
                {
                    // Admins have full access to everything
                    btnUsers.Visible = true;
                    btnSettings.Visible = true;
                }
                else if (_currentUser.IsSupervisor)
                {
                    // Supervisors have access to reports and some settings
                    btnUsers.Visible = false;
                    btnSettings.Visible = true;
                }
                else if (_currentUser.IsOperator)
                {
                    // Operators only have access to entry and exit forms
                    btnUsers.Visible = false;
                    btnSettings.Visible = false;
                }
            }
        }
        
        private void LoadDashboardData()
        {
            // Load data for the dashboard
            // ... implementation ...
        }
        
        private void EnsureDirectoriesExist()
        {
            try
            {
                // Create Images/Entry directory for vehicle images and barcodes
                string entryImagesPath = Path.Combine(Application.StartupPath, "Images", "Entry");
                if (!Directory.Exists(entryImagesPath))
                {
                    Directory.CreateDirectory(entryImagesPath);
                }
                
                // Create Images/Exit directory for vehicle exit images
                string exitImagesPath = Path.Combine(Application.StartupPath, "Images", "Exit");
                if (!Directory.Exists(exitImagesPath))
                {
                    Directory.CreateDirectory(exitImagesPath);
                }
                
                // Create logs directory
                string logsPath = Path.Combine(Application.StartupPath, "logs");
                if (!Directory.Exists(logsPath))
                {
                    Directory.CreateDirectory(logsPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error creating directories: {ex.Message}");
                // Don't throw - we'll handle missing directories when we need them
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Initialize the UI, menus, etc.
            
            // Create main menu items if they don't exist
            MenuStrip mainMenu = new MenuStrip();
            mainMenu.Dock = DockStyle.Top;
            
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem manajemenMenu = new ToolStripMenuItem("Manajemen");
            ToolStripMenuItem toolsMenu = new ToolStripMenuItem("Tools");
            
            // Add exit to file menu
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit");
            exitMenuItem.Click += exitToolStripMenuItem_Click;
            fileMenu.DropDownItems.Add(exitMenuItem);
            
            // Add menu items for new forms
            ToolStripMenuItem parkingCapacityMenuItem = new ToolStripMenuItem("Kapasitas Parkir");
            parkingCapacityMenuItem.Click += ParkingCapacityMenuItem_Click;
            
            ToolStripMenuItem notificationsMenuItem = new ToolStripMenuItem("Notifikasi");
            notificationsMenuItem.Click += NotificationsMenuItem_Click;
            
            ToolStripMenuItem backupRestoreMenuItem = new ToolStripMenuItem("Backup & Restore Database");
            backupRestoreMenuItem.Click += BackupRestoreMenuItem_Click;
            
            // Add items to appropriate menus
            manajemenMenu.DropDownItems.Add(parkingCapacityMenuItem);
            manajemenMenu.DropDownItems.Add(notificationsMenuItem);
            toolsMenu.DropDownItems.Add(backupRestoreMenuItem);
            
            // Add menus to menustrip
            mainMenu.Items.Add(fileMenu);
            mainMenu.Items.Add(manajemenMenu);
            mainMenu.Items.Add(toolsMenu);
            
            // Only show certain menu items for admin users
            if (!_currentUser.IsAdmin) // Not an admin
            {
                backupRestoreMenuItem.Visible = false;
                // Other permissions can be set here
            }
            
            // Add menustrip to form if not already there
            if (!Controls.Contains(mainMenu))
            {
                this.Controls.Add(mainMenu);
            }
        }
        
        private void ParkingCapacityMenuItem_Click(object sender, EventArgs e)
        {
            ParkingCapacityForm form = new ParkingCapacityForm();
            form.ShowDialog();
        }
        
        private void NotificationsMenuItem_Click(object sender, EventArgs e)
        {
            NotificationForm form = new NotificationForm();
            form.ShowDialog();
        }
        
        private void BackupRestoreMenuItem_Click(object sender, EventArgs e)
        {
            BackupRestoreForm form = new BackupRestoreForm();
            form.ShowDialog();
        }
        
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ShowParkingEntry()
        {
            try
            {
                ClearPanels();
                parkingPanel.Visible = true;
                
                var entryForm = new EntryForm(_currentUser);
                entryForm.TopLevel = false;
                entryForm.FormBorderStyle = FormBorderStyle.None;
                entryForm.Dock = DockStyle.Fill;
                
                parkingPanel.Controls.Add(entryForm);
                entryForm.Show();
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening entry form", ex);
                MessageBox.Show("Failed to open entry form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowParkingExit()
        {
            try
            {
                ClearPanels();
                exitPanel.Visible = true;
                
                var exitForm = new ExitForm(_currentUser);
                exitForm.TopLevel = false;
                exitForm.FormBorderStyle = FormBorderStyle.None;
                exitForm.Dock = DockStyle.Fill;
                
                exitPanel.Controls.Add(exitForm);
                exitForm.Show();
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening exit form", ex);
                MessageBox.Show("Failed to open exit form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowMemberManagement()
        {
            try
            {
                ClearPanels();
                memberPanel.Visible = true;
                
                var memberForm = new MemberManagementForm(_currentUser);
                memberForm.TopLevel = false;
                memberForm.FormBorderStyle = FormBorderStyle.None;
                memberForm.Dock = DockStyle.Fill;
                
                memberPanel.Controls.Add(memberForm);
                memberForm.Show();
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening member management", ex);
                MessageBox.Show("Failed to open member management form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowReports()
        {
            try
            {
                ClearPanels();
                reportsPanel.Visible = true;
                
                var reportForm = new ReportForm(_currentUser);
                reportForm.TopLevel = false;
                reportForm.FormBorderStyle = FormBorderStyle.None;
                reportForm.Dock = DockStyle.Fill;
                
                reportsPanel.Controls.Add(reportForm);
                reportForm.Show();
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening report form", ex);
                MessageBox.Show("Failed to open report form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearPanels()
        {
            parkingPanel.Visible = false;
            exitPanel.Visible = false;
            reportsPanel.Visible = false;
            memberPanel.Visible = false;
            
            parkingPanel.Controls.Clear();
            exitPanel.Controls.Clear();
            reportsPanel.Controls.Clear();
            memberPanel.Controls.Clear();
        }
    }
} 