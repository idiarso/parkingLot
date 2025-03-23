using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ParkingIN.Utils
{
    /// <summary>
    /// Class for reading and writing to INI files
    /// </summary>
    public class IniFile
    {
        private string _filePath;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string section, string key, string value, string filePath);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// Constructor for IniFile class
        /// </summary>
        /// <param name="filePath">Path to the INI file</param>
        public IniFile(string filePath)
        {
            _filePath = filePath;

            // Create the file if it doesn't exist
            if (!File.Exists(filePath))
            {
                try
                {
                    // Ensure directory exists
                    string directory = Path.GetDirectoryName(filePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // Create empty file
                    File.WriteAllText(filePath, string.Empty);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to create INI file at {filePath}: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// Write data to the INI file
        /// </summary>
        /// <param name="key">The key name</param>
        /// <param name="value">The value</param>
        /// <param name="section">The section name</param>
        public void Write(string key, string value, string section)
        {
            WritePrivateProfileString(section, key, value, _filePath);
        }

        /// <summary>
        /// Read data from the INI file
        /// </summary>
        /// <param name="key">The key name</param>
        /// <param name="section">The section name</param>
        /// <param name="defaultValue">Default value if key is not found</param>
        /// <returns>The value read from the INI file</returns>
        public string Read(string key, string section, string defaultValue = "")
        {
            StringBuilder retVal = new StringBuilder(255);
            GetPrivateProfileString(section, key, defaultValue, retVal, 255, _filePath);
            return retVal.ToString();
        }

        /// <summary>
        /// Delete a key from the INI file
        /// </summary>
        /// <param name="key">The key to delete</param>
        /// <param name="section">The section containing the key</param>
        public void DeleteKey(string key, string section)
        {
            Write(key, null, section);
        }

        /// <summary>
        /// Delete a section from the INI file
        /// </summary>
        /// <param name="section">The section to delete</param>
        public void DeleteSection(string section)
        {
            Write(null, null, section);
        }

        /// <summary>
        /// Check if a key exists in the INI file
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <param name="section">The section to check in</param>
        /// <returns>True if the key exists, false otherwise</returns>
        public bool KeyExists(string key, string section)
        {
            return Read(key, section).Length > 0;
        }
    }
}