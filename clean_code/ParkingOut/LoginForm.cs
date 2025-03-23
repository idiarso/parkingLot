using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using Npgsql;
using System.Configuration;
using System.Diagnostics;
using SimpleParkingAdmin.Utils;
using System.Collections.Generic;
using System.Data;
using SimpleParkingAdmin.Models;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    public partial class LoginForm : Form
    {
        private Panel leftPanel;
        private Panel rightPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private PictureBox logoBox;
        private Label lblUsername;
        private Label lblPassword;
        private CheckBox chkRemember;
        private bool isAnimating = false;
        private readonly IAppLogger _logger = CustomLogManager.GetLogger();
        private Label lblStatus;
        
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
        
        // Add a static UserInfo property to hold the current user information
        private static User _currentUser = new User();
        
        // Properties to access user info throughout the application
        public static User CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }
        
        public User LoggedInUser { get; private set; }
        
        public LoginForm()
        {
            InitializeComponent();
            
            _logger.Information("LoginForm constructor started");
            
            // First add the title and subtitle to the left panel (this should always display)
            this.lblTitle = new Label();
            this.lblTitle.Text = "ParkingOut";
            this.lblTitle.Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(50, 150);
            this.lblTitle.AutoSize = true;
            this.leftPanel.Controls.Add(this.lblTitle);
            
            // Add subtitle
            this.lblSubtitle = new Label();
            this.lblSubtitle.Text = "Sistem Manajemen Parkir Modern";
            this.lblSubtitle.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblSubtitle.ForeColor = Color.White;
            this.lblSubtitle.Location = new Point(50, 190);
            this.lblSubtitle.AutoSize = true;
            this.leftPanel.Controls.Add(this.lblSubtitle);
            
            // Check database connection when the form loads
            TestDatabaseConnection();
            
            AnimateLoginForm();
            
            _logger.Information("LoginForm constructor completed");
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form properties
            this.Text = "Login - Sistem Parkir Modern";
            this.Size = new Size(900, 500); // Wider, more modern aspect ratio
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // Borderless for modern look
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            
            // Left Panel (for branding/banner)
            this.leftPanel = new Panel();
            this.leftPanel.Dock = DockStyle.Left;
            this.leftPanel.Width = 400;
            this.leftPanel.BackColor = Color.FromArgb(0, 120, 215); // Blue theme

            // Create gradient background for left panel
            this.leftPanel.Paint += (s, e) => {
                Graphics g = e.Graphics;
                Rectangle rect = new Rectangle(0, 0, leftPanel.Width, leftPanel.Height);
                using (LinearGradientBrush brush = new LinearGradientBrush(
                    rect, Color.FromArgb(0, 120, 215), Color.FromArgb(0, 80, 170), 
                    LinearGradientMode.ForwardDiagonal))
                {
                    g.FillRectangle(brush, rect);
                }
            };

            // Right Panel (for login form)
            this.rightPanel = new Panel();
            this.rightPanel.Dock = DockStyle.Fill;
            this.rightPanel.BackColor = Color.White;
            this.rightPanel.Padding = new Padding(40);

            // Username Label
            this.lblUsername = new Label();
            this.lblUsername.Text = "Username";
            this.lblUsername.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblUsername.ForeColor = Color.FromArgb(64, 64, 64);
            this.lblUsername.Location = new Point(60, 180);
            this.lblUsername.AutoSize = true;
            this.rightPanel.Controls.Add(this.lblUsername);

            // Username TextBox
            this.txtUsername = new TextBox();
            this.txtUsername.Location = new Point(60, 205);
            this.txtUsername.Size = new Size(380, 30);
            this.txtUsername.Font = new Font("Segoe UI", 12F);
            this.txtUsername.BorderStyle = BorderStyle.FixedSingle;
            this.rightPanel.Controls.Add(this.txtUsername);

            // Password Label
            this.lblPassword = new Label();
            this.lblPassword.Text = "Password";
            this.lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblPassword.ForeColor = Color.FromArgb(64, 64, 64);
            this.lblPassword.Location = new Point(60, 250);
            this.lblPassword.AutoSize = true;
            this.rightPanel.Controls.Add(this.lblPassword);

            // Password TextBox
            this.txtPassword = new TextBox();
            this.txtPassword.Location = new Point(60, 275);
            this.txtPassword.Size = new Size(380, 30);
            this.txtPassword.Font = new Font("Segoe UI", 12F);
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.BorderStyle = BorderStyle.FixedSingle;
            this.rightPanel.Controls.Add(this.txtPassword);

            // Add panels to form first
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.leftPanel);
            
            this.Name = "LoginForm";
            this.Text = "Login";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void TestDatabaseConnection()
        {
            try
            {
                _logger.Information("Testing database connection...");
                
                // Add debug label for development purposes
                Label debugLabel = new Label();
                debugLabel.Text = "Testing database connection...";
                debugLabel.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
                debugLabel.ForeColor = Color.Black;
                debugLabel.Location = new Point(60, 400);
                debugLabel.AutoSize = true;
                this.rightPanel.Controls.Add(debugLabel);
                
                // Cek apakah kelas Database sudah terinisialisasi dengan benar
                bool dbAvailable = Database.IsDatabaseAvailable;
                debugLabel.Text += $"\nIsDatabaseAvailable: {dbAvailable}";
                
                if (!dbAvailable)
                {
                    string errorMessage = "Failed to connect to database. " + (Database.LastError ?? "Please check your connection settings.");
                    _logger.Error("Database connection test failed: " + errorMessage);
                    debugLabel.Text += $"\nError: {errorMessage}";
                    
                    // Tambahkan label status error ke form
                    this.lblStatus = new Label();
                    this.lblStatus.Text = errorMessage;
                    this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
                    this.lblStatus.ForeColor = Color.Red;
                    this.lblStatus.Location = new Point(60, 350);
                    this.lblStatus.AutoSize = true;
                    this.rightPanel.Controls.Add(this.lblStatus);
                    
                    // Tetap tampilkan form tapi disable tombol login
                    DisableLoginControls();
                }
                else
                {
                    _logger.Information("Database connection test successful");
                    debugLabel.Text += "\nConnection successful";
                    
                    // Tambahkan tombol login yang fungsional
                    AddLoginButton();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Database connection test failed", ex);
                
                // Add a visible error message
                Label exceptionLabel = new Label();
                exceptionLabel.Text = $"Exception: {ex.Message}\n{ex.StackTrace}";
                exceptionLabel.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
                exceptionLabel.ForeColor = Color.Red;
                exceptionLabel.Location = new Point(60, 420);
                exceptionLabel.AutoSize = true;
                this.rightPanel.Controls.Add(exceptionLabel);
                
                MessageBox.Show("Failed to connect to database: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Always show login controls, even if there's an exception
                DisableLoginControls();
            }
        }

        private void DisableLoginControls()
        {
            // Tetap tampilkan form tapi disable tombol login
            this.btnLogin = new Button();
            this.btnLogin.Text = "Login";
            this.btnLogin.Location = new Point(60, 320);
            this.btnLogin.Size = new Size(380, 40);
            this.btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnLogin.BackColor = Color.FromArgb(200, 200, 200); // Abu-abu untuk menandakan disabled
            this.btnLogin.Enabled = false;
            this.rightPanel.Controls.Add(this.btnLogin);
            
            // Tambahkan tombol exit
            this.btnExit = new Button();
            this.btnExit.Text = "Exit";
            this.btnExit.Location = new Point(340, 370);
            this.btnExit.Size = new Size(100, 30);
            this.btnExit.Click += (s, e) => this.Close();
            this.rightPanel.Controls.Add(this.btnExit);
        }

        private void AddLoginButton()
        {
            // Tambahkan tombol login yang fungsional
            this.btnLogin = new Button();
            this.btnLogin.Text = "Login";
            this.btnLogin.Location = new Point(60, 320);
            this.btnLogin.Size = new Size(380, 40);
            this.btnLogin.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnLogin.BackColor = Color.FromArgb(0, 120, 215);
            this.btnLogin.ForeColor = Color.White;
            this.btnLogin.FlatStyle = FlatStyle.Flat;
            this.btnLogin.FlatAppearance.BorderSize = 0;
            this.btnLogin.Cursor = Cursors.Hand;
            this.btnLogin.Click += BtnLogin_Click;
            this.rightPanel.Controls.Add(this.btnLogin);
            
            // Tambahkan tombol exit
            this.btnExit = new Button();
            this.btnExit.Text = "Exit";
            this.btnExit.Location = new Point(340, 370);
            this.btnExit.Size = new Size(100, 30);
            this.btnExit.Click += (s, e) => this.Close();
            this.rightPanel.Controls.Add(this.btnExit);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                _logger.Information("Login button clicked");
                
                // Validasi input
                if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Please enter both username and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string username = txtUsername.Text;
                string passwordText = txtPassword.Text;
                
                // Disable login controls and show loading message
                btnLogin.Enabled = false;
                btnLogin.Text = "Logging in...";
                this.Cursor = Cursors.WaitCursor;
                
                // Gunakan UserManager untuk autentikasi
                var userTask = UserManager.Instance.AuthenticateAsync(username, passwordText);
                userTask.ContinueWith(t => {
                    // Jalankan di UI thread
                    this.Invoke((Action)(() => {
                        if (t.Result != null)
                        {
                            _logger.Information($"User {username} login successful");
                            
                            // Set current user di LoginForm.CurrentUser (static) untuk backward compatibility
                            _currentUser = t.Result;
                            
                            // Tutup form login dan buka form utama
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            _logger.Warning($"Login failed for user {username}: Invalid credentials");
                            MessageBox.Show("Invalid username or password.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            
                            // Re-enable login controls
                            btnLogin.Enabled = true;
                            btnLogin.Text = "Login";
                            this.Cursor = Cursors.Default;
                        }
                    }));
                });
            }
            catch (Exception ex)
            {
                _logger.Error($"Unexpected error during login: {ex.Message}", ex);
                MessageBox.Show($"Error: {ex.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Re-enable login controls
                btnLogin.Enabled = true;
                btnLogin.Text = "Login";
                this.Cursor = Cursors.Default;
            }
        }

        // Metode untuk hash password menggunakan SHA-256 - Sudah dipindahkan ke UserManager
        private string ComputeSHA256Hash(string rawData)
        {
            return UserManager.Instance.ComputeSHA256Hash(rawData);
        }

        private void AnimateLoginForm()
        {
            try
            {
                isAnimating = true;
                // Add your login form animation logic here
            }
            catch (Exception ex)
            {
                _logger.Error("Login form animation failed", ex);
            }
            finally
            {
                isAnimating = false;
            }
        }
    }
}