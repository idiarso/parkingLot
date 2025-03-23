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
using System.Collections.Generic;
using System.Data;
using ParkingIN.Models;
using ParkingIN.Utils;

namespace ParkingIN
{
    public class LoginForm : Form
    {
        private Panel leftPanel;
        private Panel rightPanel;
        private Label lblTitle;
        private Label lblSubtitle;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Button btnExit;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblError;
        private CheckBox chkRemember;
        private bool isAnimating;
        private Button btnLoading;
        private System.Windows.Forms.Timer animTimer;
        private int animStep;

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

        private static User _currentUser = new();
        public static User CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        public LoginForm()
        {
            try
            {
                // Initialize form before setting up controls
                this.Size = new Size(900, 500);
                this.FormBorderStyle = FormBorderStyle.None;
                this.BackColor = Color.White;
                this.StartPosition = FormStartPosition.CenterScreen;
                
                SetupForm();
                
                // Move database test to a background operation
                Task.Run(() => TestDatabaseConnection());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupForm()
        {
            // Remove redundant form setup since it's now in constructor
            // Create panels
            leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 400,
                BackColor = Color.FromArgb(0, 120, 215)
            };

            rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(40)
            };

            // Add panels first
            this.Controls.Clear();
            this.Controls.Add(rightPanel);
            this.Controls.Add(leftPanel);

            // Setup gradient for left panel
            leftPanel.Paint += (s, e) => {
                using var brush = new LinearGradientBrush(
                    new Rectangle(0, 0, leftPanel.Width, leftPanel.Height),
                    Color.FromArgb(0, 120, 215),
                    Color.FromArgb(0, 80, 170),
                    LinearGradientMode.ForwardDiagonal);
                e.Graphics.FillRectangle(brush, leftPanel.ClientRectangle);
            };

            // Create controls
            lblTitle = new Label
            {
                Text = "Modern Parking System",
                Font = new Font("Segoe UI", 24F, FontStyle.Bold),
                ForeColor = Color.FromArgb(64, 64, 64),
                AutoSize = true
            };

            lblSubtitle = new Label
            {
                Text = "Please login to continue",
                Font = new Font("Segoe UI", 12F),
                ForeColor = Color.FromArgb(120, 120, 120),
                AutoSize = true
            };

            lblUsername = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(64, 64, 64),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12F),
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(64, 64, 64),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 12F),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '•',
                UseSystemPasswordChar = true
            };

            btnLogin = new Button
            {
                Text = "Login",
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 12F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            btnExit = new Button
            {
                Text = "Exit",
                Size = new Size(300, 40),
                Font = new Font("Segoe UI", 12F),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 240, 240),
                ForeColor = Color.FromArgb(64, 64, 64)
            };
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.Click += (s, e) => Application.Exit();

            lblError = new Label
            {
                AutoSize = true,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9F),
                Visible = false
            };

            // Add controls to right panel with proper positioning
            int startY = 60;
            int spacing = 20;

            lblTitle.Location = new Point(40, startY);
            startY += lblTitle.Height + spacing;

            lblSubtitle.Location = new Point(40, startY);
            startY += lblSubtitle.Height + spacing * 2;

            lblUsername.Location = new Point(40, startY);
            startY += lblUsername.Height + 5;

            txtUsername.Location = new Point(40, startY);
            startY += txtUsername.Height + spacing;

            lblPassword.Location = new Point(40, startY);
            startY += lblPassword.Height + 5;

            txtPassword.Location = new Point(40, startY);
            startY += txtPassword.Height + spacing * 2;

            btnLogin.Location = new Point(40, startY);
            startY += btnLogin.Height + spacing;

            btnExit.Location = new Point(40, startY);
            startY += btnExit.Height + spacing;

            lblError.Location = new Point(40, startY);

            // Add controls to right panel
            rightPanel.Controls.AddRange(new Control[] {
                lblTitle, lblSubtitle,
                lblUsername, txtUsername,
                lblPassword, txtPassword,
                btnLogin, btnExit,
                lblError
            });

            // Setup loading animation
            btnLoading = new Button
            {
                Size = new Size(32, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Visible = false
            };
            btnLoading.FlatAppearance.BorderSize = 0;
            btnLoading.Location = new Point(
                btnLogin.Right - btnLoading.Width - 10,
                btnLogin.Top + (btnLogin.Height - btnLoading.Height) / 2
            );

            btnLoading.Paint += (s, pe) => {
                pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, btnLoading.Width, btnLoading.Height);
                int arcSize = Math.Min(rect.Width, rect.Height) - 4;
                var arcRect = new Rectangle(
                    rect.X + (rect.Width - arcSize) / 2,
                    rect.Y + (rect.Height - arcSize) / 2,
                    arcSize, arcSize
                );

                using var pen = new Pen(Color.FromArgb(0, 120, 215), 2);
                for (int i = 0; i < 8; i++)
                {
                    int alpha = (i + 8 - animStep) % 8 * 255 / 8;
                    pen.Color = Color.FromArgb(alpha, pen.Color);
                    pe.Graphics.DrawArc(pen, arcRect, i * 45, 45 / 2);
                }
            };

            rightPanel.Controls.Add(btnLoading);

            animTimer = new System.Windows.Forms.Timer
            {
                Interval = 100
            };
            animTimer.Tick += (s, e) => {
                animStep = (animStep + 1) % 8;
                btnLoading.Invalidate();
            };

            // Set initial focus
            this.ActiveControl = txtUsername;
        }

        private void TestDatabaseConnection()
        {
            try
            {
                // Use the simplified database helper
                string errorMessage;
                bool success = SimpleDatabaseHelper.TestConnection(out errorMessage);
                
                if (!success)
                {
                    ShowError($"Database connection failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Database connection failed: {ex.Message}");
            }
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                ShowError("Please enter username and password");
                return;
            }

            // Show loading animation
            StartLoadingAnimation();

            try
            {
                // Use Task to prevent UI freezing
                bool isAuthenticated = await Task.Run(() => AuthenticateUser(txtUsername.Text, txtPassword.Text));

                if (isAuthenticated)
                {
                    // Success - hide loading and proceed
                    StopLoadingAnimation();
                    
                    // Log successful login
                    LogHelper.LogUserAction(_currentUser.UserId, "LOGIN", $"User {_currentUser.Username} logged in successfully");
                    
                    // Open main form
                    this.Hide();
                    var mainForm = new MainForm();
                    mainForm.FormClosed += (s, args) => {
                        // Log logout
                        LogHelper.LogUserAction(_currentUser.UserId, "LOGOUT", $"User {_currentUser.Username} logged out");
                        this.Close();
                    };
                    
                    // Show main form and ensure it's visible
                    mainForm.Show();
                    mainForm.BringToFront();
                    mainForm.Activate();
                }
                else
                {
                    StopLoadingAnimation();
                    ShowError("Invalid username or password");
                }
            }
            catch (Exception ex)
            {
                StopLoadingAnimation();
                ShowError($"Authentication error: {ex.Message}");
                LogHelper.LogUserAction(_currentUser.UserId, "ERROR", $"Login failed: {ex.Message}");
            }
        }

        private bool AuthenticateUser(string username, string password)
        {
            try
            {
                // Switch to our simplified database helper
                string errorMessage;
                bool isValid = SimpleDatabaseHelper.VerifyLogin(username, password, out errorMessage);
                
                if (!isValid && !string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show($"Authentication error: {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
            lblError.ForeColor = Color.Red;
            lblError.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            lblError.AutoSize = true;
            
            // Ensure error label is visible
            lblError.BringToFront();
        }

        private void StartLoadingAnimation()
        {
            btnLogin.Enabled = false;
            btnLoading.Visible = true;
            if (animTimer != null) animTimer.Start();
        }

        private void StopLoadingAnimation()
        {
            btnLogin.Enabled = true;
            btnLoading.Visible = false;
            if (animTimer != null) animTimer.Stop();
        }
    }
}