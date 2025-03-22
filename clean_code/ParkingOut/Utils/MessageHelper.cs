using System;
using System.Windows; // Added for WPF MessageBox

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
            
            // Use WPF MessageBox explicitly to avoid ambiguity
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Show a warning message
        /// </summary>
        public static void ShowWarning(string message, string title = "Warning")
        {
            // Log to console for debugging
            Console.WriteLine($"[WARNING] {title}: {message}");
            
            // Use WPF MessageBox explicitly to avoid ambiguity
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// Show an error message
        /// </summary>
        public static void ShowError(string message, string title = "Error")
        {
            // Log to console for debugging
            Console.WriteLine($"[ERROR] {title}: {message}");
            
            // Use WPF MessageBox explicitly to avoid ambiguity
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        /// <summary>
        /// Show a confirmation dialog
        /// </summary>
        public static bool ShowConfirmation(string message, string title = "Confirmation")
        {
            // Log to console for debugging
            Console.WriteLine($"[CONFIRM] {title}: {message} (Y/N)");
            
            // Use WPF MessageBox explicitly to avoid ambiguity
            MessageBoxResult result = System.Windows.MessageBox.Show(
                message, 
                title, 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
                
            return result == MessageBoxResult.Yes;
        }
    }
} 