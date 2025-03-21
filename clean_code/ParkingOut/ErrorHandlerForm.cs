using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace SimpleParkingAdmin
{
    public partial class ErrorHandlerForm : Form
    {
        private Label lblErrorTitle;
        private Label lblErrorMessage;
        private TextBox txtErrorDetails;
        private Button btnCopy;
        private Button btnClose;
        private Exception exception;
        
        public ErrorHandlerForm(Exception ex, string title = "Application Error")
        {
            this.exception = ex;
            InitializeComponent(title, ex.Message);
        }
        
        private void InitializeComponent(string title, string message)
        {
            // Form properties
            this.Text = title;
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9F);
            
            // Error title
            this.lblErrorTitle = new Label();
            this.lblErrorTitle.Text = title;
            this.lblErrorTitle.Location = new Point(20, 20);
            this.lblErrorTitle.Size = new Size(560, 30);
            this.lblErrorTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblErrorTitle.ForeColor = Color.FromArgb(192, 0, 0);
            
            // Error message
            this.lblErrorMessage = new Label();
            this.lblErrorMessage.Text = "Error Message:";
            this.lblErrorMessage.Location = new Point(20, 60);
            this.lblErrorMessage.AutoSize = true;
            
            // Error details textbox
            this.txtErrorDetails = new TextBox();
            this.txtErrorDetails.Multiline = true;
            this.txtErrorDetails.ScrollBars = ScrollBars.Vertical;
            this.txtErrorDetails.ReadOnly = true;
            this.txtErrorDetails.Location = new Point(20, 85);
            this.txtErrorDetails.Size = new Size(560, 230);
            this.txtErrorDetails.Text = BuildErrorMessage();
            
            // Copy button
            this.btnCopy = new Button();
            this.btnCopy.Text = "Copy to Clipboard";
            this.btnCopy.Location = new Point(20, 325);
            this.btnCopy.Size = new Size(150, 35);
            this.btnCopy.Click += new EventHandler(btnCopy_Click);
            
            // Close button
            this.btnClose = new Button();
            this.btnClose.Text = "Close";
            this.btnClose.Location = new Point(490, 325);
            this.btnClose.Size = new Size(90, 35);
            this.btnClose.Click += new EventHandler(btnClose_Click);
            
            // Add controls to form
            this.Controls.Add(this.lblErrorTitle);
            this.Controls.Add(this.lblErrorMessage);
            this.Controls.Add(this.txtErrorDetails);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnClose);
            
            // Apply button styling
            ApplyModernButtonStyle(this.btnCopy);
            ApplyModernButtonStyle(this.btnClose);
            
            // Log the error
            LogError();
        }
        
        private string BuildErrorMessage()
        {
            return $"Error Message: {exception.Message}\r\n\r\n" +
                   $"Error Type: {exception.GetType().FullName}\r\n\r\n" +
                   $"Stack Trace:\r\n{exception.StackTrace}\r\n\r\n" +
                   $"Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n\r\n" +
                   (exception.InnerException != null ? 
                   $"Inner Exception: {exception.InnerException.Message}\r\n\r\n" +
                   $"Inner Stack Trace:\r\n{exception.InnerException.StackTrace}" : "");
        }
        
        private void LogError()
        {
            try
            {
                string logDir = "logs";
                string logFile = Path.Combine(logDir, "error_log.txt");
                
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR:\r\n{BuildErrorMessage()}\r\n" +
                                 $"--------------------------------------------------------------\r\n";
                                 
                File.AppendAllText(logFile, logEntry);
            }
            catch
            {
                // Ignore errors during logging
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
            
            // Add hover effects
            button.MouseEnter += (s, e) => {
                Button btn = (Button)s;
                btn.BackColor = Color.FromArgb(0, 102, 204);
            };
            
            button.MouseLeave += (s, e) => {
                Button btn = (Button)s;
                btn.BackColor = Color.FromArgb(0, 120, 215);
            };
        }
        
        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(txtErrorDetails.Text);
                MessageBox.Show("Error details copied to clipboard.", "Copy Successful",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("Failed to copy to clipboard.", "Copy Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 