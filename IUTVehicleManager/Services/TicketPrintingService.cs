using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace IUTVehicleManager.Services
{
    public class TicketPrintingService
    {
        private PrintDocument printDocument;
        private string plateNumber;
        private string ticketNumber;
        private DateTime entryTime;
        private string vehicleType;
        private string priority;

        public TicketPrintingService()
        {
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
        }

        public void PrintTicket(string plateNumber, string vehicleType, string priority)
        {
            this.plateNumber = plateNumber;
            this.vehicleType = vehicleType;
            this.priority = priority;
            this.entryTime = DateTime.Now;
            this.ticketNumber = GenerateTicketNumber();

            try
            {
                // Get default printer
                PrintDialog printDialog = new PrintDialog
                {
                    Document = printDocument,
                    AllowSomePages = false,
                    AllowSelection = false
                };

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    printDocument.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing ticket: {ex.Message}", "Printing Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font headerFont = new Font("Arial", 12, FontStyle.Bold);
            Font contentFont = new Font("Arial", 10);
            Font barcodeFont = new Font("Arial", 8);

            // Draw title
            string title = "IUT VEHICLE PARKING";
            g.DrawString(title, titleFont, Brushes.Black, 50, 20);

            // Draw separator line
            g.DrawLine(Pens.Black, 50, 50, 300, 50);

            // Draw ticket details
            int yPos = 70;
            g.DrawString("Ticket Number:", headerFont, Brushes.Black, 50, yPos);
            g.DrawString(ticketNumber, contentFont, Brushes.Black, 150, yPos);

            yPos += 30;
            g.DrawString("Entry Time:", headerFont, Brushes.Black, 50, yPos);
            g.DrawString(entryTime.ToString("yyyy-MM-dd HH:mm:ss"), contentFont, Brushes.Black, 150, yPos);

            yPos += 30;
            g.DrawString("Plate Number:", headerFont, Brushes.Black, 50, yPos);
            g.DrawString(plateNumber, contentFont, Brushes.Black, 150, yPos);

            yPos += 30;
            g.DrawString("Vehicle Type:", headerFont, Brushes.Black, 50, yPos);
            g.DrawString(vehicleType, contentFont, Brushes.Black, 150, yPos);

            yPos += 30;
            g.DrawString("Priority:", headerFont, Brushes.Black, 50, yPos);
            g.DrawString(priority, contentFont, Brushes.Black, 150, yPos);

            // Draw barcode
            yPos += 40;
            g.DrawString(ticketNumber, barcodeFont, Brushes.Black, 50, yPos);

            // Draw footer
            yPos += 40;
            g.DrawString("Please keep this ticket for exit", contentFont, Brushes.Black, 50, yPos);
            g.DrawString("Thank you for using IUT Parking", contentFont, Brushes.Black, 50, yPos + 20);
        }

        private string GenerateTicketNumber()
        {
            return $"TKT{DateTime.Now:yyyyMMddHHmmss}";
        }

        public string GetTicketNumber()
        {
            return ticketNumber;
        }
    }
} 