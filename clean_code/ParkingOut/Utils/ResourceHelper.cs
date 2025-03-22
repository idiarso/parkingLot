using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ParkingOut.Utils
{
    public static class ResourceHelper
    {
        private static readonly IAppLogger _logger = new FileLogger();

        /// <summary>
        /// Attempts to load an icon from a file in the icons directory
        /// </summary>
        /// <param name="iconName">Name of the icon file (without extension)</param>
        /// <returns>Image object or null if the icon can't be loaded</returns>
        public static Image LoadIcon(string iconName)
        {
            try
            {
                string iconPath = Path.Combine(Application.StartupPath, "Icons", $"{iconName}.png");
                
                if (File.Exists(iconPath))
                {
                    return Image.FromFile(iconPath);
                }
                
                // Try alternative locations
                iconPath = Path.Combine(Application.StartupPath, "Resources", "Icons", $"{iconName}.png");
                if (File.Exists(iconPath))
                {
                    return Image.FromFile(iconPath);
                }
                
                // If not found, check embedded resources
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = $"ParkingOut.Resources.{iconName}.png";
                
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        return Image.FromStream(stream);
                    }
                }
                
                _logger.Warning($"Icon not found: {iconName}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading icon '{iconName}': {ex.Message}", ex);
                return null;
            }
        }
        
        /// <summary>
        /// Creates a directory for icons if it doesn't exist
        /// </summary>
        public static void EnsureIconsDirectory()
        {
            try
            {
                string iconsDir = Path.Combine(Application.StartupPath, "Icons");
                if (!Directory.Exists(iconsDir))
                {
                    Directory.CreateDirectory(iconsDir);
                    _logger.Information($"Created icons directory: {iconsDir}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create icons directory: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Creates a default placeholder icon if an icon is not found
        /// </summary>
        /// <returns>A generic icon image</returns>
        public static Image CreatePlaceholderIcon(Color color)
        {
            try
            {
                // Create a 32x32 bitmap with the specified color
                Bitmap bitmap = new Bitmap(32, 32);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(color);
                    g.DrawRectangle(Pens.White, 0, 0, 31, 31);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create placeholder icon: {ex.Message}", ex);
                return null;
            }
        }
    }
} 