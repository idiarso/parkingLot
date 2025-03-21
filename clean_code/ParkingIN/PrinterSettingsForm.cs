using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Drawing.Imaging;
using System.Threading;
using ZXing;
using ZXing.Common;
using System.Linq;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using Serilog;
using ParkingIN.Models;
using ParkingIN.Utils;

namespace ParkingIN
{
    public partial class PrinterSettingsForm : Form
    {
        private string printerConfigPath;
        private readonly object logRotationLock = new object();
        private const long MAX_LOG_SIZE_MB = 10;
        private const string LOG_ARCHIVE_SUFFIX = ".old";
        private const int MAX_PRINT_RETRIES = 2;
        private readonly ILogger logger;
        private readonly StringCollection printerList = new StringCollection();
        
        public PrinterSettingsForm(ILogger logger)
        {
            InitializeComponent();
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            printerConfigPath = Path.Combine(Application.StartupPath, "config", "printer.ini");
            LoadPrinters();
        }

        private void PrinterSettingsForm_Load(object sender, EventArgs e)
        {
            LoadAvailablePrinters();
            LoadPrinterSettings();
        }
        
        private void LoadPrinters()
        {
            try
            {
                cmbPrinter.Items.Clear();
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    if (!printer.StartsWith('.'))
                    {
                        cmbPrinter.Items.Add(printer);
                        printerList.Add(printer);
                    }
                }

                if (cmbPrinter.Items.Count > 0)
                {
                    cmbPrinter.SelectedIndex = 0;
                }
                else
                {
                    logger.Warning("Tidak ada printer yang terdeteksi");
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error saat memuat daftar printer: {ex.Message}");
                MessageBox.Show("Gagal memuat daftar printer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadAvailablePrinters()
        {
            try
            {
                cmbPrinter.Items.Clear();
                
                // Get available printers
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
                {
                    cmbPrinter.Items.Add(printer);
                }
                
                if (cmbPrinter.Items.Count > 0)
                {
                    cmbPrinter.SelectedIndex = 0;
                    logger.Information($"Found {cmbPrinter.Items.Count} printers, selected: {cmbPrinter.Items[0]}");
                }
                else
                {
                    logger.Warning("No printers found on this system");
                    MessageBox.Show("No printers found on this system.", 
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading printers: {ex.Message}");
                MessageBox.Show($"Error loading printers: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadPrinterSettings()
        {
            try
            {
                if (File.Exists(printerConfigPath))
                {
                    string[] lines = File.ReadAllLines(printerConfigPath);
                    
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
                                // Check if the printer exists in the list
                                if (cmbPrinter.Items.Contains(value))
                                {
                                    cmbPrinter.SelectedItem = value;
                                }
                                break;
                            case "Port":
                                txtPort.Text = value;
                                break;
                            case "Paper_Width":
                                txtPaperWidth.Text = value;
                                break;
                            case "DPI":
                                txtDPI.Text = value;
                                break;
                            case "Header":
                                txtHeader.Text = value;
                                break;
                            case "Footer":
                                txtFooter.Text = value;
                                break;
                            case "Show_Logo":
                                chkShowLogo.Checked = bool.Parse(value);
                                break;
                            case "QR_Code":
                                chkQRCode.Checked = bool.Parse(value);
                                break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Printer configuration file not found. Default settings will be used.", 
                        "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Set default values
                    if (cmbPrinter.Items.Count > 0)
                    {
                        // Look for thermal printer in the list (common names)
                        string[] thermalPrinters = { "EPSON", "TM-T", "Thermal", "POS" };
                        for (int i = 0; i < cmbPrinter.Items.Count; i++)
                        {
                            string printer = cmbPrinter.Items[i].ToString();
                            foreach (string term in thermalPrinters)
                            {
                                if (printer.ToUpper().Contains(term))
                                {
                                    cmbPrinter.SelectedIndex = i;
                                    break;
                                }
                            }
                            if (cmbPrinter.SelectedIndex != 0)
                                break;
                        }
                    }
                    
                    txtPort.Text = "USB001";
                    txtPaperWidth.Text = "80";
                    txtDPI.Text = "180";
                    txtHeader.Text = "TIKET PARKIR";
                    txtFooter.Text = "Terima Kasih";
                    chkShowLogo.Checked = true;
                    chkQRCode.Checked = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error loading printer settings: {ex.Message}");
                MessageBox.Show($"Error loading printer settings: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LogSystemMessage(string message)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "system.log");
                string directory = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Check log file size and rotate if needed
                if (File.Exists(logPath))
                {
                    var fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length > MAX_LOG_SIZE_MB * 1024 * 1024)
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string archivePath = $"{logPath}.{timestamp}{LOG_ARCHIVE_SUFFIX}";
                        
                        lock (logRotationLock)
                        {
                            if (File.Exists(archivePath))
                            {
                                File.Delete(archivePath);
                            }
                            File.Move(logPath, archivePath);
                        }
                    }
                }
                
                lock (logRotationLock)
                {
                    string threadId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();
                    string logMsg = $"[INFO] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Thread:{threadId}] {message}\n";
                    File.AppendAllText(logPath, logMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to system log: {ex.Message}");
            }
        }

        private void LogError(Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "error.log");
                string directory = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Check log file size and rotate if needed
                if (File.Exists(logPath))
                {
                    var fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length > MAX_LOG_SIZE_MB * 1024 * 1024)
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string backupPath = Path.Combine(directory, $"error_{timestamp}.log");
                        
                        lock (logRotationLock)
                        {
                            if (File.Exists(logPath)) // Double-check inside lock
                            {
                                File.Move(logPath, backupPath);
                            }
                        }
                    }
                }
                
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [PrinterSettings] ERROR: {ex.Message}\n";
                File.AppendAllText(logPath, logMessage);
                
                Debug.WriteLine($"ERROR: {ex.Message}");
            }
            catch
            {
                // Ignore errors in logging
                Debug.WriteLine($"Failed to log error: {ex.Message}");
            }
        }
        
        private void LogError(string message)
        {
            try
            {
                string logPath = Path.Combine(Application.StartupPath, "logs", "error.log");
                string directory = Path.GetDirectoryName(logPath);
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                // Check log file size and rotate if needed
                if (File.Exists(logPath))
                {
                    var fileInfo = new FileInfo(logPath);
                    if (fileInfo.Length > MAX_LOG_SIZE_MB * 1024 * 1024)
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string backupPath = Path.Combine(directory, $"error_{timestamp}.log");
                        
                        lock (logRotationLock)
                        {
                            if (File.Exists(logPath)) // Double-check inside lock
                            {
                                File.Move(logPath, backupPath);
                            }
                        }
                    }
                }
                
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [PrinterSettings] ERROR: {message}\n";
                File.AppendAllText(logPath, logMessage);
                
                Debug.WriteLine($"ERROR: {message}");
            }
            catch (Exception ex)
            {
                // Ignore errors in logging
                Debug.WriteLine($"Failed to log error: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validation
                if (cmbPrinter.SelectedItem == null)
                {
                    MessageBox.Show("Please select a printer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbPrinter.Focus();
                    return;
                }
                
                if (!int.TryParse(txtPaperWidth.Text, out int paperWidth) || paperWidth <= 0)
                {
                    MessageBox.Show("Paper width must be a valid positive number.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPaperWidth.Focus();
                    return;
                }

                // Validate paper width range (50-100mm)
                if (paperWidth < 50 || paperWidth > 100)
                {
                    MessageBox.Show("Paper width must be between 50 and 100mm.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPaperWidth.Focus();
                    return;
                }
                
                if (!int.TryParse(txtDPI.Text, out int dpi) || dpi <= 0)
                {
                    MessageBox.Show("DPI must be a valid positive number.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDPI.Focus();
                    return;
                }

                // Validate DPI range (100-300)
                if (dpi < 100 || dpi > 300)
                {
                    MessageBox.Show("DPI must be between 100 and 300.", 
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDPI.Focus();
                    return;
                }
                
                // Ensure directory exists
                string configDir = Path.Combine(Application.StartupPath, "config");
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // Write to config file
                using (StreamWriter writer = new StreamWriter(printerConfigPath))
                {
                    writer.WriteLine("[Printer]");
                    writer.WriteLine($"Name={cmbPrinter.SelectedItem}");
                    writer.WriteLine($"Port={txtPort.Text}");
                    writer.WriteLine($"Paper_Width={txtPaperWidth.Text}");
                    writer.WriteLine($"DPI={txtDPI.Text}");
                    writer.WriteLine();
                    writer.WriteLine("[Template]");
                    writer.WriteLine($"Header={txtHeader.Text}");
                    writer.WriteLine($"Footer={txtFooter.Text}");
                    writer.WriteLine($"Show_Logo={chkShowLogo.Checked}");
                    writer.WriteLine($"QR_Code={chkQRCode.Checked}");
                }
                
                LogSystemMessage($"Printer settings saved successfully for printer: {cmbPrinter.SelectedItem}");
                MessageBox.Show("Printer settings saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Error($"Error saat menyimpan pengaturan printer: {ex.Message}");
                MessageBox.Show("Gagal menyimpan pengaturan printer", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private Bitmap GenerateBarcodeImage(string data, string filename, int paperWidth)
        {
            try
            {
                // Scale barcode width based on paper width
                int barcodeWidth = (int)(paperWidth * 1.5 * 4);
                int barcodeHeight = 50;
                
                var writer = new BarcodeWriter<Bitmap>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Width = barcodeWidth,
                        Height = barcodeHeight,
                        Margin = 0
                    }
                };
                var bitmap = writer.Write(data);
                
                logger.Information($"Barcode generated successfully for data: {data}, size: {barcodeWidth}x{barcodeHeight}");
                return bitmap;
            }
            catch (Exception ex)
            {
                logger.Error($"Error generating barcode for {data}: {ex.Message}");
                MessageBox.Show($"Error generating barcode: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        
        private void btnTestPrint_Click(object sender, EventArgs e)
        {
            int retryCount = 0;
            bool printSuccess = false;
            
            while (!printSuccess && retryCount <= MAX_PRINT_RETRIES)
            {
                try
                {
                    if (cmbPrinter.SelectedItem == null)
                    {
                        MessageBox.Show("Please select a printer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        cmbPrinter.Focus();
                        return;
                    }
                    
                    lblStatus.Visible = true;
                    lblStatus.Text = retryCount > 0 ? $"Printing test page... (Attempt {retryCount + 1}/{MAX_PRINT_RETRIES + 1})" : "Printing test page...";
                    Cursor = Cursors.WaitCursor;
                    Application.DoEvents();
                    
                    PrintDocument printDoc = new PrintDocument();
                    printDoc.PrinterSettings.PrinterName = cmbPrinter.SelectedItem.ToString();
                    
                    if (!int.TryParse(txtPaperWidth.Text, out int paperWidth))
                    {
                        paperWidth = 80;
                        logger.Warning($"Invalid paper width, using default: {paperWidth}mm");
                    }
                    
                    PaperSize thermalPaperSize = new PaperSize("Thermal", paperWidth * 4, 500);
                    printDoc.DefaultPageSettings.PaperSize = thermalPaperSize;
                    
                    printDoc.PrintPage += (sender, e) => {
                        Graphics g = e.Graphics;
                        Font headerFont = new Font("Arial", 12, FontStyle.Bold);
                        Font normalFont = new Font("Arial", 10);
                        Font smallFont = new Font("Arial", 8);
                        int yPos = 10;
                        int leftMargin = 10;
                        string header = string.IsNullOrEmpty(txtHeader.Text) ? "TEST PRINT" : txtHeader.Text;
                        g.DrawString(header, headerFont, Brushes.Black, leftMargin, yPos);
                        yPos += 30;
                        g.DrawLine(Pens.Black, leftMargin, yPos, paperWidth * 3, yPos);
                        yPos += 15;
                        g.DrawString($"Printer: {cmbPrinter.SelectedItem}", normalFont, Brushes.Black, leftMargin, yPos);
                        yPos += 20;
                        g.DrawString($"Date: {DateTime.Now:dd/MM/yyyy}", normalFont, Brushes.Black, leftMargin, yPos);
                        yPos += 20;
                        g.DrawString($"Time: {DateTime.Now:HH:mm:ss}", normalFont, Brushes.Black, leftMargin, yPos);
                        yPos += 20;
                        g.DrawString($"Paper Width: {paperWidth}mm", normalFont, Brushes.Black, leftMargin, yPos);
                        yPos += 20;
                        g.DrawString($"DPI: {txtDPI.Text}", normalFont, Brushes.Black, leftMargin, yPos);
                        yPos += 30;
                        g.DrawLine(Pens.Black, leftMargin, yPos, paperWidth * 3, yPos);
                        yPos += 15;
                        g.DrawString("Test Pattern:", normalFont, Brushes.Black, leftMargin, yPos);
                        yPos += 20;
                        g.DrawRectangle(Pens.Black, leftMargin, yPos, 100, 25);
                        g.FillRectangle(Brushes.LightGray, leftMargin + 1, yPos + 1, 98, 23);
                        yPos += 40;
                        if (chkQRCode.Checked)
                        {
                            // Scale barcode size based on paper width
                            var barcodeImage = GenerateBarcodeImage("TEST123", "TEST", paperWidth);
                            if (barcodeImage != null)
                            {
                                int barcodeWidth = (int)(paperWidth * 1.5);
                                g.DrawImage(barcodeImage, leftMargin, yPos, barcodeWidth * 3, 50);
                                yPos += 60;
                            }
                        }
                        string footer = string.IsNullOrEmpty(txtFooter.Text) ? "End of Test" : txtFooter.Text;
                        g.DrawString(footer, smallFont, Brushes.Black, leftMargin, yPos);
                        e.HasMorePages = false;
                    };
                    
                    printDoc.Print();
                    LogSystemMessage($"Test page successfully sent to printer: {cmbPrinter.SelectedItem}");
                    MessageBox.Show("Test page sent to printer.", "Print Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    printSuccess = true;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    logger.Error($"Error printing test page (attempt {retryCount}/{MAX_PRINT_RETRIES + 1}): {ex.Message}");
                    
                    if (retryCount <= MAX_PRINT_RETRIES)
                    {
                        DialogResult result = MessageBox.Show(
                            $"Error printing test page: {ex.Message}\n\nWould you like to retry? (Attempt {retryCount}/{MAX_PRINT_RETRIES})",
                            "Print Error",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );
                        
                        if (result != DialogResult.Yes)
                        {
                            MessageBox.Show($"Test printing abandoned after {retryCount} attempts.", 
                                "Print Test Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Failed to print test page after {MAX_PRINT_RETRIES + 1} attempts: {ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                finally
                {
                    if (printSuccess || retryCount > MAX_PRINT_RETRIES)
                    {
                        lblStatus.Visible = false;
                        Cursor = Cursors.Default;
                    }
                }
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Windows Form Designer generated code

        private Label lblTitle;
        private Label lblPrinter;
        private ComboBox cmbPrinter;
        private Label lblPort;
        private TextBox txtPort;
        private Label lblPaperWidth;
        private TextBox txtPaperWidth;
        private Label lblWidthUnit;
        private Label lblDPI;
        private TextBox txtDPI;
        private Label lblTemplateSettings;
        private Label lblHeader;
        private TextBox txtHeader;
        private Label lblFooter;
        private TextBox txtFooter;
        private CheckBox chkShowLogo;
        private CheckBox chkQRCode;
        private Button btnTestPrint;
        private Button btnSave;
        private Button btnCancel;
        private Label lblStatus;

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblPrinter = new Label();
            this.cmbPrinter = new ComboBox();
            this.lblPort = new Label();
            this.txtPort = new TextBox();
            this.lblPaperWidth = new Label();
            this.txtPaperWidth = new TextBox();
            this.lblWidthUnit = new Label();
            this.lblDPI = new Label();
            this.txtDPI = new TextBox();
            this.lblTemplateSettings = new Label();
            this.lblHeader = new Label();
            this.txtHeader = new TextBox();
            this.lblFooter = new Label();
            this.txtFooter = new TextBox();
            this.chkShowLogo = new CheckBox();
            this.chkQRCode = new CheckBox();
            this.btnTestPrint = new Button();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.lblStatus = new Label();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(185, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Printer Settings";
            // 
            // lblPrinter
            // 
            this.lblPrinter.AutoSize = true;
            this.lblPrinter.Location = new Point(25, 70);
            this.lblPrinter.Name = "lblPrinter";
            this.lblPrinter.Size = new Size(45, 15);
            this.lblPrinter.TabIndex = 1;
            this.lblPrinter.Text = "Printer:";
            // 
            // cmbPrinter
            // 
            this.cmbPrinter.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbPrinter.FormattingEnabled = true;
            this.cmbPrinter.Location = new Point(140, 67);
            this.cmbPrinter.Name = "cmbPrinter";
            this.cmbPrinter.Size = new Size(200, 23);
            this.cmbPrinter.TabIndex = 2;
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new Point(25, 100);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new Size(32, 15);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "Port:";
            // 
            // txtPort
            // 
            this.txtPort.Location = new Point(140, 97);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new Size(100, 23);
            this.txtPort.TabIndex = 4;
            // 
            // lblPaperWidth
            // 
            this.lblPaperWidth.AutoSize = true;
            this.lblPaperWidth.Location = new Point(25, 130);
            this.lblPaperWidth.Name = "lblPaperWidth";
            this.lblPaperWidth.Size = new Size(78, 15);
            this.lblPaperWidth.TabIndex = 5;
            this.lblPaperWidth.Text = "Paper Width:";
            // 
            // txtPaperWidth
            // 
            this.txtPaperWidth.Location = new Point(140, 127);
            this.txtPaperWidth.Name = "txtPaperWidth";
            this.txtPaperWidth.Size = new Size(60, 23);
            this.txtPaperWidth.TabIndex = 6;
            // 
            // lblWidthUnit
            // 
            this.lblWidthUnit.AutoSize = true;
            this.lblWidthUnit.Location = new Point(206, 130);
            this.lblWidthUnit.Name = "lblWidthUnit";
            this.lblWidthUnit.Size = new Size(28, 15);
            this.lblWidthUnit.TabIndex = 7;
            this.lblWidthUnit.Text = "mm";
            // 
            // lblDPI
            // 
            this.lblDPI.AutoSize = true;
            this.lblDPI.Location = new Point(25, 160);
            this.lblDPI.Name = "lblDPI";
            this.lblDPI.Size = new Size(28, 15);
            this.lblDPI.TabIndex = 8;
            this.lblDPI.Text = "DPI:";
            // 
            // txtDPI
            // 
            this.txtDPI.Location = new Point(140, 157);
            this.txtDPI.Name = "txtDPI";
            this.txtDPI.Size = new Size(60, 23);
            this.txtDPI.TabIndex = 9;
            // 
            // lblTemplateSettings
            // 
            this.lblTemplateSettings.AutoSize = true;
            this.lblTemplateSettings.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTemplateSettings.Location = new Point(25, 200);
            this.lblTemplateSettings.Name = "lblTemplateSettings";
            this.lblTemplateSettings.Size = new Size(164, 21);
            this.lblTemplateSettings.TabIndex = 10;
            this.lblTemplateSettings.Text = "Template Settings:";
            // 
            // lblHeader
            // 
            this.lblHeader.AutoSize = true;
            this.lblHeader.Location = new Point(45, 240);
            this.lblHeader.Name = "lblHeader";
            this.lblHeader.Size = new Size(48, 15);
            this.lblHeader.TabIndex = 11;
            this.lblHeader.Text = "Header:";
            // 
            // txtHeader
            // 
            this.txtHeader.Location = new Point(140, 237);
            this.txtHeader.Name = "txtHeader";
            this.txtHeader.Size = new Size(200, 23);
            this.txtHeader.TabIndex = 12;
            // 
            // lblFooter
            // 
            this.lblFooter.AutoSize = true;
            this.lblFooter.Location = new Point(45, 270);
            this.lblFooter.Name = "lblFooter";
            this.lblFooter.Size = new Size(43, 15);
            this.lblFooter.TabIndex = 13;
            this.lblFooter.Text = "Footer:";
            // 
            // txtFooter
            // 
            this.txtFooter.Location = new Point(140, 267);
            this.txtFooter.Name = "txtFooter";
            this.txtFooter.Size = new Size(200, 23);
            this.txtFooter.TabIndex = 14;
            // 
            // chkShowLogo
            // 
            this.chkShowLogo.AutoSize = true;
            this.chkShowLogo.Location = new Point(140, 300);
            this.chkShowLogo.Name = "chkShowLogo";
            this.chkShowLogo.Size = new Size(86, 19);
            this.chkShowLogo.TabIndex = 15;
            this.chkShowLogo.Text = "Show Logo";
            this.chkShowLogo.UseVisualStyleBackColor = true;
            // 
            // chkQRCode
            // 
            this.chkQRCode.AutoSize = true;
            this.chkQRCode.Location = new Point(240, 300);
            this.chkQRCode.Name = "chkQRCode";
            this.chkQRCode.Size = new Size(72, 19);
            this.chkQRCode.TabIndex = 16;
            this.chkQRCode.Text = "QR Code";
            this.chkQRCode.UseVisualStyleBackColor = true;
            // 
            // btnTestPrint
            // 
            this.btnTestPrint.Location = new Point(350, 67);
            this.btnTestPrint.Name = "btnTestPrint";
            this.btnTestPrint.Size = new Size(100, 30);
            this.btnTestPrint.TabIndex = 17;
            this.btnTestPrint.Text = "Test Print";
            this.btnTestPrint.UseVisualStyleBackColor = true;
            this.btnTestPrint.Click += new EventHandler(this.btnTestPrint_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new Point(140, 340);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(80, 30);
            this.btnSave.TabIndex = 18;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new Point(240, 340);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(80, 30);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = Color.Blue;
            this.lblStatus.Location = new Point(25, 385);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(42, 15);
            this.lblStatus.TabIndex = 20;
            this.lblStatus.Text = "Status";
            this.lblStatus.Visible = false;
            // 
            // PrinterSettingsForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(464, 411);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnTestPrint);
            this.Controls.Add(this.chkQRCode);
            this.Controls.Add(this.chkShowLogo);
            this.Controls.Add(this.txtFooter);
            this.Controls.Add(this.lblFooter);
            this.Controls.Add(this.txtHeader);
            this.Controls.Add(this.lblHeader);
            this.Controls.Add(this.lblTemplateSettings);
            this.Controls.Add(this.txtDPI);
            this.Controls.Add(this.lblDPI);
            this.Controls.Add(this.lblWidthUnit);
            this.Controls.Add(this.txtPaperWidth);
            this.Controls.Add(this.lblPaperWidth);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.cmbPrinter);
            this.Controls.Add(this.lblPrinter);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PrinterSettingsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Printer Settings";
            this.Load += new EventHandler(this.PrinterSettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}