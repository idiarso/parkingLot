using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using SimpleParkingAdmin.Models;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class NetworkSettingsForm : Form
    {
        private TextBox txtServerIP;
        private TextBox txtServerPort;
        private TextBox txtDatabaseName;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private ComboBox cmbDatabaseType;
        private Button btnSave;
        private Button btnCancel;
        private Button btnTest;
        
        public NetworkSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Pengaturan Jaringan";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            
            // Create labels
            Label lblServerIP = new Label();
            lblServerIP.Text = "Server IP:";
            lblServerIP.Location = new Point(30, 30);
            lblServerIP.AutoSize = true;
            
            Label lblServerPort = new Label();
            lblServerPort.Text = "Server Port:";
            lblServerPort.Location = new Point(30, 70);
            lblServerPort.AutoSize = true;
            
            Label lblDatabaseName = new Label();
            lblDatabaseName.Text = "Database Name:";
            lblDatabaseName.Location = new Point(30, 110);
            lblDatabaseName.AutoSize = true;
            
            Label lblUsername = new Label();
            lblUsername.Text = "Username:";
            lblUsername.Location = new Point(30, 150);
            lblUsername.AutoSize = true;
            
            Label lblPassword = new Label();
            lblPassword.Text = "Password:";
            lblPassword.Location = new Point(30, 190);
            lblPassword.AutoSize = true;
            
            Label lblDatabaseType = new Label();
            lblDatabaseType.Text = "Database Type:";
            lblDatabaseType.Location = new Point(30, 230);
            lblDatabaseType.AutoSize = true;
            
            // Create textboxes
            txtServerIP = new TextBox();
            txtServerIP.Location = new Point(150, 27);
            txtServerIP.Size = new Size(250, 23);
            
            txtServerPort = new TextBox();
            txtServerPort.Location = new Point(150, 67);
            txtServerPort.Size = new Size(250, 23);
            
            txtDatabaseName = new TextBox();
            txtDatabaseName.Location = new Point(150, 107);
            txtDatabaseName.Size = new Size(250, 23);
            
            txtUsername = new TextBox();
            txtUsername.Location = new Point(150, 147);
            txtUsername.Size = new Size(250, 23);
            
            txtPassword = new TextBox();
            txtPassword.Location = new Point(150, 187);
            txtPassword.Size = new Size(250, 23);
            txtPassword.PasswordChar = '*';
            
            cmbDatabaseType = new ComboBox();
            cmbDatabaseType.Location = new Point(150, 227);
            cmbDatabaseType.Size = new Size(250, 23);
            cmbDatabaseType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDatabaseType.Items.AddRange(new object[] { "PostgreSQL", "MySQL" });
            cmbDatabaseType.SelectedIndexChanged += new EventHandler(cmbDatabaseType_SelectedIndexChanged);
            
            // Create buttons
            btnTest = new Button();
            btnTest.Text = "Test Koneksi";
            btnTest.Location = new Point(30, 280);
            btnTest.Size = new Size(110, 35);
            btnTest.Click += new EventHandler(btnTest_Click);
            ApplyModernButtonStyle(btnTest);
            
            btnSave = new Button();
            btnSave.Text = "Simpan";
            btnSave.Location = new Point(230, 280);
            btnSave.Size = new Size(90, 35);
            btnSave.Click += new EventHandler(btnSave_Click);
            ApplyModernButtonStyle(btnSave);
            
            btnCancel = new Button();
            btnCancel.Text = "Batal";
            btnCancel.Location = new Point(330, 280);
            btnCancel.Size = new Size(90, 35);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            ApplyModernButtonStyle(btnCancel);
            
            // Add all controls to form
            this.Controls.Add(lblServerIP);
            this.Controls.Add(lblServerPort);
            this.Controls.Add(lblDatabaseName);
            this.Controls.Add(lblUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(lblDatabaseType);
            this.Controls.Add(txtServerIP);
            this.Controls.Add(txtServerPort);
            this.Controls.Add(txtDatabaseName);
            this.Controls.Add(txtUsername);
            this.Controls.Add(txtPassword);
            this.Controls.Add(cmbDatabaseType);
            this.Controls.Add(btnTest);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void cmbDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Update port based on database type selection
            if (cmbDatabaseType.SelectedItem.ToString() == "PostgreSQL")
            {
                if (txtServerPort.Text == "3306") // MySQL default port
                {
                    txtServerPort.Text = "5432"; // PostgreSQL default port
                }
                
                // Update default username if using default
                if (txtUsername.Text == "root")
                {
                    txtUsername.Text = "postgres";
                }
            }
            else if (cmbDatabaseType.SelectedItem.ToString() == "MySQL")
            {
                if (txtServerPort.Text == "5432") // PostgreSQL default port
                {
                    txtServerPort.Text = "3306"; // MySQL default port
                }
                
                // Update default username if using default
                if (txtUsername.Text == "postgres")
                {
                    txtUsername.Text = "root";
                }
            }
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
                NetworkSettingsDTO settings = LoadNetworkSettings();
                if (settings != null)
                {
                    txtServerIP.Text = settings.ServerIP;
                    txtServerPort.Text = settings.ServerPort.ToString();
                    txtDatabaseName.Text = settings.DatabaseName;
                    txtUsername.Text = settings.Username;
                    txtPassword.Text = settings.Password;
                    
                    // Select database type
                    cmbDatabaseType.SelectedItem = settings.DatabaseType ?? "PostgreSQL";
                    if (cmbDatabaseType.SelectedIndex == -1)
                    {
                        cmbDatabaseType.SelectedIndex = 0; // Default to PostgreSQL
                    }
                }
                else
                {
                    // Set default values if no settings found
                    txtServerIP.Text = "localhost";
                    txtServerPort.Text = "5432";
                    txtDatabaseName.Text = "parkirdb";
                    txtUsername.Text = "postgres";
                    txtPassword.Text = "root@rsi";
                    cmbDatabaseType.SelectedIndex = 0; // PostgreSQL
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading network settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                // Test database connection
                string connectionString = BuildConnectionString();
                string errorMessage;
                bool success = Database.TestConnection(out errorMessage);
                
                if (success)
                {
                    MessageBox.Show("Koneksi ke database berhasil!", "Test Koneksi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Gagal terhubung ke database. Silakan periksa pengaturan koneksi. Pesan kesalahan: {errorMessage}", 
                        "Test Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menguji koneksi: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(txtServerIP.Text) ||
                    string.IsNullOrWhiteSpace(txtServerPort.Text) ||
                    string.IsNullOrWhiteSpace(txtDatabaseName.Text) ||
                    cmbDatabaseType.SelectedItem == null)
                {
                    MessageBox.Show("Semua field harus diisi!", "Validasi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Create and save settings
                NetworkSettingsDTO settings = new NetworkSettingsDTO
                {
                    ServerIP = txtServerIP.Text.Trim(),
                    ServerPort = int.Parse(txtServerPort.Text.Trim()),
                    DatabaseName = txtDatabaseName.Text.Trim(),
                    Username = txtUsername.Text.Trim(),
                    Password = txtPassword.Text,
                    DatabaseType = cmbDatabaseType.SelectedItem.ToString()
                };
                
                SaveNetworkSettings(settings);
                
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
        
        private NetworkSettingsDTO LoadNetworkSettings()
        {
            string settingsPath = Path.Combine(Application.StartupPath, "config", "network.xml");
            
            if (!File.Exists(settingsPath))
                return null;
                
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(NetworkSettingsDTO));
                using (StreamReader reader = new StreamReader(settingsPath))
                {
                    return (NetworkSettingsDTO)serializer.Deserialize(reader);
                }
            }
            catch
            {
                return null;
            }
        }
        
        private void SaveNetworkSettings(NetworkSettingsDTO settings)
        {
            string configDir = Path.Combine(Application.StartupPath, "config");
            string settingsPath = Path.Combine(configDir, "network.xml");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
                
            XmlSerializer serializer = new XmlSerializer(typeof(NetworkSettingsDTO));
            using (StreamWriter writer = new StreamWriter(settingsPath))
            {
                serializer.Serialize(writer, settings);
            }
        }
        
        private string BuildConnectionString()
        {
            string databaseType = cmbDatabaseType.SelectedItem?.ToString() ?? "PostgreSQL";
            if (databaseType == "PostgreSQL")
            {
                return $"Host={txtServerIP.Text};Port={txtServerPort.Text};Database={txtDatabaseName.Text};Username={txtUsername.Text};Password={txtPassword.Text};";
            }
            else
            {
                return $"Server={txtServerIP.Text};Port={txtServerPort.Text};Database={txtDatabaseName.Text};Uid={txtUsername.Text};Pwd={txtPassword.Text};CharSet=utf8mb4;SslMode=none;";
            }
        }
    }

    [Serializable]
    public class NetworkSettingsDTO
    {
        public string ServerIP { get; set; } = "localhost";
        public int ServerPort { get; set; } = 5432;
        public string DatabaseName { get; set; } = "parkirdb";
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "root@rsi";
        public string DatabaseType { get; set; } = "PostgreSQL";
    }
} 