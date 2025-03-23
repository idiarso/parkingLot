using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ParkingOut.Models;

namespace ParkingOut.Services
{
    /// <summary>
    /// Interface for print service
    /// </summary>
    public interface IPrintService
    {
        /// <summary>
        /// Prints a ticket for a vehicle entry
        /// </summary>
        /// <param name="entry">The vehicle entry</param>
        /// <returns>True if successful, false otherwise</returns>
        bool PrintTicket(VehicleEntry entry);

        /// <summary>
        /// Prints a receipt for a vehicle exit
        /// </summary>
        /// <param name="exit">The vehicle exit</param>
        /// <returns>True if successful, false otherwise</returns>
        bool PrintReceipt(VehicleExit exit);
    }

    /// <summary>
    /// Interface for camera service
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// Gets a value indicating whether the camera is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the camera
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the camera
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets the image control to display the camera feed
        /// </summary>
        /// <param name="imageControl">The image control</param>
        void SetImageControl(Image imageControl);

        /// <summary>
        /// Captures an image from the camera
        /// </summary>
        /// <returns>The captured image or null if failed</returns>
        BitmapSource? CaptureImage();

        /// <summary>
        /// Recognizes a license plate from the camera
        /// </summary>
        /// <returns>The recognized license plate or null if failed</returns>
        string? RecognizePlate();
    }
}