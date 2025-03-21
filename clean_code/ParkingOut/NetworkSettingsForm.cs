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
            this.Size = new Size(450, 350);
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
            
            // Create buttons
            btnTest = new Button();
            btnTest.Text = "Test Koneksi";
            btnTest.Location = new Point(30, 240);
            btnTest.Size = new Size(110, 35);
            btnTest.Click += new EventHandler(btnTest_Click);
            ApplyModernButtonStyle(btnTest);
            
            btnSave = new Button();
            btnSave.Text = "Simpan";
            btnSave.Location = new Point(230, 240);
            btnSave.Size = new Size(90, 35);
            btnSave.Click += new EventHandler(btnSave_Click);
            ApplyModernButtonStyle(btnSave);
            
            btnCancel = new Button();
            btnCancel.Text = "Batal";
            btnCancel.Location = new Point(330, 240);
            btnCancel.Size = new Size(90, 35);
            btnCancel.Click += new EventHandler(btnCancel_Click);
            ApplyModernButtonStyle(btnCancel);
            
            // Add all controls to form
            this.Controls.Add(lblServerIP);
            this.Controls.Add(lblServerPort);
            this.Controls.Add(lblDatabaseName);
            this.Controls.Add(lblUsername);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtServerIP);
            this.Controls.Add(txtServerPort);
            this.Controls.Add(txtDatabaseName);
            this.Controls.Add(txtUsername);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnTest);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
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
                }
                else
                {
                    // Set default values if no settings found
                    txtServerIP.Text = "localhost";
                    txtServerPort.Text = "3306";
                    txtDatabaseName.Text = "parkingdb";
                    txtUsername.Text = "root";
                    txtPassword.Text = "";
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
                    string.IsNullOrWhiteSpace(txtDatabaseName.Text))
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
                    Password = txtPassword.Text
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
        
        private string BuildConnectionString()
        {
            return $"Server={txtServerIP.Text};Port={txtServerPort.Text};" +
                   $"Database={txtDatabaseName.Text};Uid={txtUsername.Text};Pwd={txtPassword.Text};";
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
    }

    [Serializable]
    public class NetworkSettingsDTO
    {
        public string ServerIP { get; set; }
        public int ServerPort { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
} 