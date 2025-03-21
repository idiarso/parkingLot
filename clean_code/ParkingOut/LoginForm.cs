using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using MySql.Data.MySqlClient;
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
            
            // Check database connection when the form loads
            TestDatabaseConnection();
            
            AnimateLoginForm();
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
            this.lblUsername.Location = new Point(460, 180);
            this.lblUsername.AutoSize = true;

            // Username TextBox
            this.txtUsername = new TextBox();
            this.txtUsername.Location = new Point(460, 205);
            this.txtUsername.Size = new Size(380, 30);
            this.txtUsername.Font = new Font("Segoe UI", 12F);
            this.txtUsername.BorderStyle = BorderStyle.FixedSingle;

            // Password Label
            this.lblPassword = new Label();
            this.lblPassword.Text = "Password";
            this.lblPassword.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.lblPassword.ForeColor = Color.FromArgb(64, 64, 64);
            this.lblPassword.Location = new Point(460, 250);
            this.lblPassword.AutoSize = true;

            // Password TextBox
            this.txtPassword = new TextBox();
            this.txtPassword.Location = new Point(460, 275);
            this.txtPassword.Size = new Size(380, 30);
            this.txtPassword.Font = new Font("Segoe UI", 12F);
            this.txtPassword.UseSystemPasswordChar = true;
            this.txtPassword.BorderStyle = BorderStyle.FixedSingle;

            // Add controls to form
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
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
                // Add your database connection test logic here
            }
            catch (Exception ex)
            {
                _logger.Error("Database connection test failed", ex);
                MessageBox.Show("Failed to connect to database. Please check your connection settings.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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