using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ParkingOut.Services
{
    /// <summary>
    /// Interface for camera service operations
    /// </summary>
    public interface ICameraService
    {
        /// <summary>
        /// Gets a value indicating whether the camera is running
        /// </summary>
        bool IsRunning { get; }
        
        /// <summary>
        /// Initializes the camera
        /// </summary>
        /// <param name="cameraIndex">The camera index or IP address</param>
        /// <returns>True if initialization successful, false otherwise</returns>
        Task<bool> InitializeCamera(string cameraIndex);

        /// <summary>
        /// Captures an image from the camera
        /// </summary>
        /// <returns>The captured image or null if failed</returns>
        Task<Bitmap?> CaptureImage();

        /// <summary>
        /// Detects license plate from an image
        /// </summary>
        /// <param name="image">The image to process</param>
        /// <returns>The detected license plate number or null if not detected</returns>
        Task<string?> DetectLicensePlate(Bitmap image);

        /// <summary>
        /// Gets the list of available cameras
        /// </summary>
        /// <returns>Array of camera names/indices</returns>
        string[] GetAvailableCameras();

        /// <summary>
        /// Releases camera resources
        /// </summary>
        void ReleaseCamera();

        /// <summary>
        /// Gets or sets the camera resolution
        /// </summary>
        /// <param name="width">The width in pixels</param>
        /// <param name="height">The height in pixels</param>
        /// <returns>True if successful, false otherwise</returns>
        bool SetResolution(int width, int height);
        
        /// <summary>
        /// Legacy method to start the camera
        /// </summary>
        void Start();
        
        /// <summary>
        /// Legacy method to stop the camera
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Sets the image control to display the camera feed
        /// </summary>
        /// <param name="imageControl">The image control</param>
        void SetImageControl(System.Windows.Controls.Image imageControl);
        
        /// <summary>
        /// Legacy method to recognize license plate
        /// </summary>
        /// <returns>The recognized license plate or "Unknown" if failed</returns>
        string RecognizePlate();
        
        /// <summary>
        /// Legacy method to capture image and return file path
        /// </summary>
        /// <returns>The captured image file path or null if failed</returns>
        string CaptureImageToFile();
        
        /// <summary>
        /// Legacy method to capture plate number
        /// </summary>
        /// <returns>The captured plate number or "Unknown" if failed</returns>
        string CapturePlateNumber();
    }
}