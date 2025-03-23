using System;
using System.Threading.Tasks;
using System.Drawing;

namespace ParkingOut.Services
{
    /// <summary>
    /// Interface for camera service operations
    /// </summary>
    public interface ICameraService
    {
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
    }
}