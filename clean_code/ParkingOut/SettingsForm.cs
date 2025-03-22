using System;
using System.Drawing;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;
using System.Diagnostics;
using System.IO;

namespace SimpleParkingAdmin
{
    public partial class SettingsForm : Form
    {
        private readonly IAppLogger _logger = CustomLogManager.GetLogger();
        private readonly User _currentUser;
        private TabControl tabControl;
        private TabPage tabUsers;
        private TabPage tabRates;
        private TabPage tabShifts;
        private TabPage tabVehicleTypes;

        // Color scheme
        private readonly Color primaryColor = Color.FromArgb(24, 116, 205);
        private readonly Color textColor = Color.FromArgb(45, 52, 54);
        private readonly Color bgColor = Color.FromArgb(245, 246, 250);

        public SettingsForm(User currentUser)
        {
            _currentUser = currentUser;
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            try
            {
                this.Text = "System Settings";
                this.Size = new Size(1000, 600);
                this.StartPosition = FormStartPosition.CenterScreen;
                this.BackColor = bgColor;

                // Create tab control
                tabControl = new TabControl
                {
                    Dock = DockStyle.Fill,
                    Padding = new Point(20, 10)
                };

                // Create tabs
                CreateUserManagementTab();
                CreateRatesManagementTab();
                CreateShiftsManagementTab();
                CreateVehicleTypesTab();

                this.Controls.Add(tabControl);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error setting up settings form: {ex.Message}");
                MessageBox.Show("Failed to initialize settings form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateUserManagementTab()
        {
            tabUsers = new TabPage("User Management");
            tabUsers.BackColor = bgColor;
            tabUsers.Padding = new Padding(10);

            // User list
            DataGridView dgvUsers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            // Buttons panel
            Panel buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5)
            };

            Button btnAdd = CreateButton("Add User", primaryColor);
            Button btnEdit = CreateButton("Edit User", primaryColor);
            Button btnDelete = CreateButton("Delete User", Color.FromArgb(231, 76, 60));

            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Add controls to tab
            tabUsers.Controls.AddRange(new Control[] { dgvUsers, buttonPanel });
            tabControl.TabPages.Add(tabUsers);

            // Load users
            LoadUsers(dgvUsers);

            // Wire up events
            btnAdd.Click += (s, e) => AddUser();
            btnEdit.Click += (s, e) => EditUser(dgvUsers);
            btnDelete.Click += (s, e) => DeleteUser(dgvUsers);
        }

        private void CreateRatesManagementTab()
        {
            tabRates = new TabPage("Parking Rates");
            tabRates.BackColor = bgColor;
            tabRates.Padding = new Padding(10);

            // Rates list
            DataGridView dgvRates = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            // Buttons panel
            Panel buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5)
            };

            Button btnAdd = CreateButton("Add Rate", primaryColor);
            Button btnEdit = CreateButton("Edit Rate", primaryColor);
            Button btnDelete = CreateButton("Delete Rate", Color.FromArgb(231, 76, 60));

            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Add controls to tab
            tabRates.Controls.AddRange(new Control[] { dgvRates, buttonPanel });
            tabControl.TabPages.Add(tabRates);

            // Load rates
            LoadRates(dgvRates);

            // Wire up events
            btnAdd.Click += (s, e) => AddRate();
            btnEdit.Click += (s, e) => EditRate(dgvRates);
            btnDelete.Click += (s, e) => DeleteRate(dgvRates);
        }

        private void CreateShiftsManagementTab()
        {
            tabShifts = new TabPage("Work Shifts");
            tabShifts.BackColor = bgColor;
            tabShifts.Padding = new Padding(10);

            // Shifts list
            DataGridView dgvShifts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            // Buttons panel
            Panel buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5)
            };

            Button btnAdd = CreateButton("Add Shift", primaryColor);
            Button btnEdit = CreateButton("Edit Shift", primaryColor);
            Button btnDelete = CreateButton("Delete Shift", Color.FromArgb(231, 76, 60));

            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Add controls to tab
            tabShifts.Controls.AddRange(new Control[] { dgvShifts, buttonPanel });
            tabControl.TabPages.Add(tabShifts);

            // Load shifts
            LoadShifts(dgvShifts);

            // Wire up events
            btnAdd.Click += (s, e) => AddShift();
            btnEdit.Click += (s, e) => EditShift(dgvShifts);
            btnDelete.Click += (s, e) => DeleteShift(dgvShifts);
        }

        private void CreateVehicleTypesTab()
        {
            tabVehicleTypes = new TabPage("Vehicle Types");
            tabVehicleTypes.BackColor = bgColor;
            tabVehicleTypes.Padding = new Padding(10);

            // Vehicle types list
            DataGridView dgvVehicleTypes = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AllowUserToAddRows = false,
                BackgroundColor = Color.White
            };

            // Buttons panel
            Panel buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom,
                Padding = new Padding(5)
            };

            Button btnAdd = CreateButton("Add Type", primaryColor);
            Button btnEdit = CreateButton("Edit Type", primaryColor);
            Button btnDelete = CreateButton("Delete Type", Color.FromArgb(231, 76, 60));

            buttonPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete });

            // Add controls to tab
            tabVehicleTypes.Controls.AddRange(new Control[] { dgvVehicleTypes, buttonPanel });
            tabControl.TabPages.Add(tabVehicleTypes);

            // Load vehicle types
            LoadVehicleTypes(dgvVehicleTypes);

            // Wire up events
            btnAdd.Click += (s, e) => AddVehicleType();
            btnEdit.Click += (s, e) => EditVehicleType(dgvVehicleTypes);
            btnDelete.Click += (s, e) => DeleteVehicleType(dgvVehicleTypes);
        }

        private Button CreateButton(string text, Color color)
        {
            Button btn = new Button
            {
                Text = text,
                BackColor = color,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 9),
                Margin = new Padding(5),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        #region User Management
        private void LoadUsers(DataGridView dgv)
        {
            try
            {
                string query = @"
                    SELECT 
                        id,
                        username,
                        nama_lengkap as full_name,
                        role,
                        active
                    FROM users 
                    ORDER BY username";

                var dt = Database.GetData(query);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading users: {ex.Message}");
                MessageBox.Show("Failed to load users.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddUser()
        {
            // TODO: Implement user add form
            MessageBox.Show("Add user functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditUser(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement user edit form
            MessageBox.Show("Edit user functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteUser(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement user delete functionality
            MessageBox.Show("Delete user functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Rates Management
        private void LoadRates(DataGridView dgv)
        {
            try
            {
                string query = @"
                    SELECT 
                        id,
                        jenis_kendaraan,
                        tarif_perjam,
                        tarif_maksimal,
                        denda_tiket_hilang
                    FROM t_tarif 
                    WHERE status = 1
                    ORDER BY jenis_kendaraan";

                var dt = Database.GetData(query);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading rates: {ex.Message}");
                MessageBox.Show("Failed to load parking rates.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddRate()
        {
            // TODO: Implement rate add form
            MessageBox.Show("Add rate functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditRate(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement rate edit form
            MessageBox.Show("Edit rate functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteRate(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement rate delete functionality
            MessageBox.Show("Delete rate functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Shifts Management
        private void LoadShifts(DataGridView dgv)
        {
            try
            {
                string query = @"
                    SELECT 
                        id,
                        nama_shift,
                        jam_mulai,
                        jam_selesai,
                        status
                    FROM t_shift 
                    ORDER BY jam_mulai";

                var dt = Database.GetData(query);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading shifts: {ex.Message}");
                MessageBox.Show("Failed to load work shifts.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddShift()
        {
            // TODO: Implement shift add form
            MessageBox.Show("Add shift functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditShift(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement shift edit form
            MessageBox.Show("Edit shift functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteShift(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement shift delete functionality
            MessageBox.Show("Delete shift functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        #region Vehicle Types Management
        private void LoadVehicleTypes(DataGridView dgv)
        {
            try
            {
                string query = @"
                    SELECT DISTINCT 
                        jenis_kendaraan,
                        status
                    FROM t_tarif 
                    ORDER BY jenis_kendaraan";

                var dt = Database.GetData(query);
                dgv.DataSource = dt;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading vehicle types: {ex.Message}");
                MessageBox.Show("Failed to load vehicle types.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AddVehicleType()
        {
            // TODO: Implement vehicle type add form
            MessageBox.Show("Add vehicle type functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void EditVehicleType(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement vehicle type edit form
            MessageBox.Show("Edit vehicle type functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteVehicleType(DataGridView dgv)
        {
            if (dgv.SelectedRows.Count == 0) return;
            // TODO: Implement vehicle type delete functionality
            MessageBox.Show("Delete vehicle type functionality coming soon!", "Info",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            try
            {
                LoadSettings();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading settings: {ex.Message}");
                MessageBox.Show("Failed to load settings.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                TestDatabaseConnection();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error testing connection: {ex.Message}");
                MessageBox.Show("Failed to test connection.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBrowseLog_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Log files (*.log)|*.log|All files (*.*)|*.*";
                    dialog.InitialDirectory = Path.Combine(Application.StartupPath, "Logs");

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = dialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error browsing log file: {ex.Message}");
                MessageBox.Show("Failed to open log file.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveSettings();
                MessageBox.Show("Settings saved successfully.", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving settings: {ex.Message}");
                MessageBox.Show("Failed to save settings.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoadSettings()
        {
            try
            {
                _logger.Information("Loading settings");
                // Load settings from database or config file
                // You can implement this based on your requirements
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load settings", ex);
                MessageBox.Show("Failed to load settings. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TestDatabaseConnection()
        {
            try
            {
                _logger.Information("Testing database connection");
                string errorMessage;
                if (Database.TestConnection(out errorMessage))
                {
                    MessageBox.Show("Database connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to connect to database: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to test database connection", ex);
                MessageBox.Show($"Error testing database connection: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                _logger.Information("Saving settings");
                // Save settings to database or config file
                // You can implement this based on your requirements
                MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to save settings", ex);
                MessageBox.Show("Failed to save settings. Please check the logs for details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 
