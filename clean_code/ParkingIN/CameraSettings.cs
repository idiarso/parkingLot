using System;
using System.IO;

namespace ParkingIN
{
    public class CameraSettings
    {
        public int CameraType { get; set; } // 0 = Local Webcam, 1 = IP Camera
        public string DeviceId { get; set; }
        public string CameraName { get; set; }
        public string IpAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string Resolution { get; set; }
        public int CaptureInterval { get; set; }
        public bool OcrEnabled { get; set; }
        public int MinConfidence { get; set; }
        public string PlateRegion { get; set; }
        public int MaxAngle { get; set; }
        
        public CameraSettings()
        {
            // Default values
            CameraType = 0;
            DeviceId = "";
            CameraName = "";
            IpAddress = "192.168.1.100";
            Username = "admin";
            Password = "admin123";
            Port = 8080;
            Resolution = "1280x720";
            CaptureInterval = 1000;
            OcrEnabled = true;
            MinConfidence = 80;
            PlateRegion = "ID";
            MaxAngle = 30;
        }
        
        public static CameraSettings LoadFromFile(string configPath)
        {
            CameraSettings settings = new CameraSettings();
            
            try
            {
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("["))
                            continue;
                        
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length != 2)
                            continue;
                        
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        
                        switch (key)
                        {
                            case "Type":
                                settings.CameraType = int.Parse(value);
                                break;
                            case "PreferredCamera":
                                settings.CameraName = value;
                                break;
                            case "DeviceId":
                                settings.DeviceId = value;
                                break;
                            case "IP":
                                settings.IpAddress = value;
                                break;
                            case "Username":
                                settings.Username = value;
                                break;
                            case "Password":
                                settings.Password = value;
                                break;
                            case "Port":
                                settings.Port = int.Parse(value);
                                break;
                            case "Resolution":
                                settings.Resolution = value;
                                break;
                            case "Capture_Interval":
                                settings.CaptureInterval = int.Parse(value);
                                break;
                            case "OCR_Enabled":
                                settings.OcrEnabled = bool.Parse(value);
                                break;
                            case "Min_Confidence":
                                settings.MinConfidence = int.Parse(value);
                                break;
                            case "Plate_Region":
                                settings.PlateRegion = value;
                                break;
                            case "Max_Angle":
                                settings.MaxAngle = int.Parse(value);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading camera settings: {ex.Message}");
                // Return default settings on error
                settings = new CameraSettings();
            }
            
            return settings;
        }
        
        public void SaveToFile(string configPath)
        {
            try
            {
                // Ensure directory exists
                string directory = Path.GetDirectoryName(configPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Write to config file
                using (StreamWriter writer = new StreamWriter(configPath))
                {
                    writer.WriteLine("[Camera]");
                    writer.WriteLine($"Type={CameraType}");
                    
                    if (CameraType == 0) // Local webcam
                    {
                        writer.WriteLine($"PreferredCamera={CameraName}");
                        writer.WriteLine($"DeviceId={DeviceId}");
                    }
                    else // IP camera
                    {
                        writer.WriteLine($"IP={IpAddress}");
                        writer.WriteLine($"Username={Username}");
                        writer.WriteLine($"Password={Password}");
                        writer.WriteLine($"Port={Port}");
                    }
                    
                    writer.WriteLine($"Resolution={Resolution}");
                    writer.WriteLine($"Capture_Interval={CaptureInterval}");
                    writer.WriteLine($"OCR_Enabled={OcrEnabled}");
                    writer.WriteLine();
                    writer.WriteLine("[OCR]");
                    writer.WriteLine($"Min_Confidence={MinConfidence}");
                    writer.WriteLine($"Plate_Region={PlateRegion}");
                    writer.WriteLine($"Max_Angle={MaxAngle}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving camera settings: {ex.Message}");
            }
        }
    }
} 