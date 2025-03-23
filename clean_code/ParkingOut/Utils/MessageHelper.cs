using System;
using System.Windows.Forms; // Using Windows Forms MessageBox

namespace ParkingOut.Utils
{
    /// <summary>
    /// Helper class for displaying messages without MessageBox ambiguity issues
    /// </summary>
    public static class MessageHelper
    {
        /// <summary>
        /// Show an information message
        /// </summary>
        public static void ShowInfo(string message, string title = "Information")
        {
            // Log to console for debugging
            Console.WriteLine($"[INFO] {title}: {message}");
            
            // Use Windows Forms MessageBox
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Show a warning message
        /// </summary>
        public static void ShowWarning(string message, string title = "Warning")
        {
            // Log to console for debugging
            Console.WriteLine($"[WARNING] {title}: {message}");
            
            // Use Windows Forms MessageBox
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Show an error message
        /// </summary>
        public static void ShowError(string message, string title = "Error")
        {
            // Log to console for debugging
            Console.WriteLine($"[ERROR] {title}: {message}");
            
            // Use Windows Forms MessageBox
            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        public static bool ShowConfirmation(string message, string title = "Confirmation")
        {
            // Log to console for debugging
            Console.WriteLine($"[CONFIRM] {title}: {message} (Y/N)");
            
            // Use Windows Forms MessageBox
            DialogResult result = MessageBox.Show(
                message, 
                title, 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            return result == DialogResult.Yes;
        }
    }
}