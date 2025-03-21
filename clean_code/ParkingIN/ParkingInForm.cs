using System;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Linq;
using ParkingIN.Models;
using ParkingIN.Utils;

namespace ParkingIN
{
    public class ParkingInForm : Form
    {
        private string printerName;
        private int paperWidth;
        private bool showLogo;
        private bool showQrCode;
        private string headerText;
        private string footerText;

        public string PrinterName => printerName;
        public int PaperWidth => paperWidth;
        public bool ShowLogo => showLogo;
        public bool ShowQrCode => showQrCode;
        public string HeaderText => headerText;
        public string FooterText => footerText;

        public ParkingInForm()
        {
            InitializeComponent();
            LoadPrinterSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Parking Entry";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void LoadPrinterSettings()
        {
            try
            {
                // Initialize default values
                printerName = "";
                paperWidth = 80; // Default paper width
                showLogo = true;
                showQrCode = true;
                headerText = "Parking Ticket";
                footerText = "Thank you";

                // Set default printer if available
                foreach (string printer in System.Drawing.Printing.PrinterSettings.InstalledPrinters.Cast<string>())
                {
                    printerName = printer;
                    Logger.Info($"Found printer: {printer}");
                    break; // Get the first available printer
                }

                if (string.IsNullOrEmpty(printerName))
                {
                    Logger.Warning("No printers found in the system");
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error loading printer settings");
                MessageBox.Show($"Error loading printer settings: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                // Additional initialization code can be added here
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "Error initializing form");
                MessageBox.Show($"Error initializing form: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}