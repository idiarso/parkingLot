using System;
using System.Windows.Forms;
using System.Drawing;

namespace SimpleParkingAdmin
{
    public static class NotificationHelper
    {
        public static void ShowInformation(string message, string title = "Information")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        public static void ShowWarning(string message, string title = "Warning")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        
        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        
        public static DialogResult ShowQuestion(string message, string title = "Confirmation")
        {
            return MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        
        public static void ShowSuccess(string message, string title = "Success")
        {
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        public static void ShowToast(Form parentForm, string message, int durationMs = 3000, int xOffset = 20, int yOffset = 20)
        {
            // Create a new form for the toast notification
            Form toastForm = new Form
            {
                Size = new Size(300, 80),
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Opacity = 0.9,
                ShowInTaskbar = false,
                TopMost = true
            };
            
            // Add a label to display the message
            Label label = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White
            };
            
            toastForm.Controls.Add(label);
            
            // Position the toast in the bottom right corner of the parent form
            if (parentForm != null)
            {
                Point location = parentForm.PointToScreen(new Point(
                    parentForm.ClientSize.Width - toastForm.Width - xOffset,
                    parentForm.ClientSize.Height - toastForm.Height - yOffset
                ));
                
                toastForm.Location = location;
            }
            else
            {
                // If no parent form, position at the bottom right of the screen
                Rectangle screenBounds = Screen.PrimaryScreen.WorkingArea;
                toastForm.Location = new Point(
                    screenBounds.Width - toastForm.Width - xOffset,
                    screenBounds.Height - toastForm.Height - yOffset
                );
            }
            
            // Create a timer to auto-close the toast
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer
            {
                Interval = durationMs
            };
            
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                toastForm.Close();
                timer.Dispose();
            };
            
            // Show the toast and start the timer
            toastForm.Show();
            timer.Start();
        }
    }
} 