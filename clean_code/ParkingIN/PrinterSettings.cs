using System;
using System.IO;

namespace ParkingIN
{
    public class PrinterSettings
    {
        public string Name { get; set; }
        public string Port { get; set; }
        public int PaperWidth { get; set; }
        public int DPI { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public bool ShowLogo { get; set; }
        public bool QRCode { get; set; }
        public bool PrintBarcode { get; set; }
        public bool AutoCut { get; set; }
        
        public PrinterSettings()
        {
            // Default values
            Name = "";
            Port = "USB001";
            PaperWidth = 80;
            DPI = 180;
            Header = "PARKING TICKET";
            Footer = "Thank You";
            ShowLogo = true;
            QRCode = true;
            PrintBarcode = true;
            AutoCut = true;
        }
        
        public static PrinterSettings LoadFromFile(string configPath)
        {
            PrinterSettings settings = new PrinterSettings();
            
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
                            case "Name":
                                settings.Name = value;
                                break;
                            case "Port":
                                settings.Port = value;
                                break;
                            case "Paper_Width":
                                if (int.TryParse(value, out int paperWidth))
                                    settings.PaperWidth = paperWidth;
                                break;
                            case "DPI":
                                if (int.TryParse(value, out int dpi))
                                    settings.DPI = dpi;
                                break;
                            case "Header":
                                settings.Header = value;
                                break;
                            case "Footer":
                                settings.Footer = value;
                                break;
                            case "Show_Logo":
                                if (bool.TryParse(value, out bool showLogo))
                                    settings.ShowLogo = showLogo;
                                break;
                            case "QR_Code":
                                if (bool.TryParse(value, out bool qrCode))
                                    settings.QRCode = qrCode;
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading printer settings: {ex.Message}");
                // Return default settings on error
                settings = new PrinterSettings();
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
                    writer.WriteLine("[Printer]");
                    writer.WriteLine($"Name={Name}");
                    writer.WriteLine($"Port={Port}");
                    writer.WriteLine($"Paper_Width={PaperWidth}");
                    writer.WriteLine($"DPI={DPI}");
                    writer.WriteLine();
                    writer.WriteLine("[Template]");
                    writer.WriteLine($"Header={Header}");
                    writer.WriteLine($"Footer={Footer}");
                    writer.WriteLine($"Show_Logo={ShowLogo}");
                    writer.WriteLine($"QR_Code={QRCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving printer settings: {ex.Message}");
            }
        }
    }
}