using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using QRCoder;
using SimpleParkingAdmin.Utils;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    public partial class PaymentForm : Form
    {
        private readonly IAppLogger _logger = CustomLogManager.GetLogger();
        private decimal amount;
        private string referenceId;
        private string paymentMethod;
        
        // UI Controls
        private Label lblAmount;
        private Label lblReference;
        private RadioButton rbCash;
        private RadioButton rbQris;
        private RadioButton rbTransfer;
        private Panel pnlCash;
        private Panel pnlQris;
        private Panel pnlTransfer;
        private TextBox txtCashAmount;
        private PictureBox picQris;
        private Label lblExpiry;
        private Label lblBankName;
        private Label lblAccountNumber;
        private Label lblAccountName;
        private Label lblTransferAmount;
        private Label lblChange;
        private Button btnConfirm;
        private Button btnCancel;
        
        public PaymentForm(decimal amount, string referenceId)
        {
            InitializeComponent();
            this.amount = amount;
            this.referenceId = referenceId;
        }
        
        private void InitializeComponent()
        {
            // Initialize form
            this.Text = "Payment";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Load += new EventHandler(this.PaymentForm_Load);
            
            // Labels for amount and reference
            this.lblAmount = new Label();
            this.lblAmount.AutoSize = true;
            this.lblAmount.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            this.lblAmount.Location = new Point(20, 20);
            this.lblAmount.Text = "Rp 0";
            
            this.lblReference = new Label();
            this.lblReference.AutoSize = true;
            this.lblReference.Location = new Point(20, 50);
            this.lblReference.Text = "Reference: ";
            
            // Radio buttons for payment methods
            this.rbCash = new RadioButton();
            this.rbCash.AutoSize = true;
            this.rbCash.Location = new Point(20, 80);
            this.rbCash.Text = "Cash";
            this.rbCash.Checked = true;
            this.rbCash.CheckedChanged += new EventHandler(this.paymentMethod_CheckedChanged);
            
            this.rbQris = new RadioButton();
            this.rbQris.AutoSize = true;
            this.rbQris.Location = new Point(100, 80);
            this.rbQris.Text = "QRIS";
            this.rbQris.CheckedChanged += new EventHandler(this.paymentMethod_CheckedChanged);
            
            this.rbTransfer = new RadioButton();
            this.rbTransfer.AutoSize = true;
            this.rbTransfer.Location = new Point(180, 80);
            this.rbTransfer.Text = "Bank Transfer";
            this.rbTransfer.CheckedChanged += new EventHandler(this.paymentMethod_CheckedChanged);
            
            // Cash payment panel
            this.pnlCash = new Panel();
            this.pnlCash.Location = new Point(20, 110);
            this.pnlCash.Size = new Size(450, 200);
            this.pnlCash.BorderStyle = BorderStyle.FixedSingle;
            
            Label lblCashAmount = new Label();
            lblCashAmount.AutoSize = true;
            lblCashAmount.Location = new Point(10, 10);
            lblCashAmount.Text = "Cash Amount:";
            
            this.txtCashAmount = new TextBox();
            this.txtCashAmount.Location = new Point(10, 30);
            this.txtCashAmount.Size = new Size(150, 25);
            this.txtCashAmount.TextChanged += new EventHandler(this.txtCashAmount_TextChanged);
            
            Label lblChangeLabel = new Label();
            lblChangeLabel.AutoSize = true;
            lblChangeLabel.Location = new Point(10, 60);
            lblChangeLabel.Text = "Change:";
            
            this.lblChange = new Label();
            this.lblChange.AutoSize = true;
            this.lblChange.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblChange.Location = new Point(10, 80);
            this.lblChange.Text = "Rp 0";
            
            this.pnlCash.Controls.Add(lblCashAmount);
            this.pnlCash.Controls.Add(this.txtCashAmount);
            this.pnlCash.Controls.Add(lblChangeLabel);
            this.pnlCash.Controls.Add(this.lblChange);
            
            // QRIS payment panel
            this.pnlQris = new Panel();
            this.pnlQris.Location = new Point(20, 110);
            this.pnlQris.Size = new Size(450, 200);
            this.pnlQris.BorderStyle = BorderStyle.FixedSingle;
            this.pnlQris.Visible = false;
            
            this.picQris = new PictureBox();
            this.picQris.Location = new Point(75, 10);
            this.picQris.Size = new Size(250, 250);
            this.picQris.SizeMode = PictureBoxSizeMode.Zoom;
            
            this.lblExpiry = new Label();
            this.lblExpiry.AutoSize = true;
            this.lblExpiry.Location = new Point(10, 270);
            this.lblExpiry.Text = "Valid until: ";
            
            this.pnlQris.Controls.Add(this.picQris);
            this.pnlQris.Controls.Add(this.lblExpiry);
            
            // Transfer payment panel
            this.pnlTransfer = new Panel();
            this.pnlTransfer.Location = new Point(20, 110);
            this.pnlTransfer.Size = new Size(450, 200);
            this.pnlTransfer.BorderStyle = BorderStyle.FixedSingle;
            this.pnlTransfer.Visible = false;
            
            this.lblBankName = new Label();
            this.lblBankName.AutoSize = true;
            this.lblBankName.Location = new Point(10, 10);
            this.lblBankName.Text = "Bank: ";
            
            this.lblAccountNumber = new Label();
            this.lblAccountNumber.AutoSize = true;
            this.lblAccountNumber.Location = new Point(10, 40);
            this.lblAccountNumber.Text = "Account Number: ";
            
            this.lblAccountName = new Label();
            this.lblAccountName.AutoSize = true;
            this.lblAccountName.Location = new Point(10, 70);
            this.lblAccountName.Text = "Account Name: ";
            
            this.lblTransferAmount = new Label();
            this.lblTransferAmount.AutoSize = true;
            this.lblTransferAmount.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblTransferAmount.Location = new Point(10, 100);
            this.lblTransferAmount.Text = "Rp 0";
            
            this.pnlTransfer.Controls.Add(this.lblBankName);
            this.pnlTransfer.Controls.Add(this.lblAccountNumber);
            this.pnlTransfer.Controls.Add(this.lblAccountName);
            this.pnlTransfer.Controls.Add(this.lblTransferAmount);
            
            // Buttons
            this.btnConfirm = new Button();
            this.btnConfirm.Location = new Point(290, 320);
            this.btnConfirm.Size = new Size(90, 30);
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new EventHandler(this.btnConfirm_Click);
            
            this.btnCancel = new Button();
            this.btnCancel.Location = new Point(390, 320);
            this.btnCancel.Size = new Size(90, 30);
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            // Add controls to form
            this.Controls.Add(this.lblAmount);
            this.Controls.Add(this.lblReference);
            this.Controls.Add(this.rbCash);
            this.Controls.Add(this.rbQris);
            this.Controls.Add(this.rbTransfer);
            this.Controls.Add(this.pnlCash);
            this.Controls.Add(this.pnlQris);
            this.Controls.Add(this.pnlTransfer);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.btnCancel);
        }
        
        private void PaymentForm_Load(object sender, EventArgs e)
        {
            // Set payment details
            lblAmount.Text = string.Format("Rp {0:N0}", amount);
            lblReference.Text = referenceId;
            
            // Default payment method
            rbCash.Checked = true;
        }
        
        private void paymentMethod_CheckedChanged(object sender, EventArgs e)
        {
            // Handle payment method selection
            if (rbQris.Checked)
            {
                // Generate QRIS code
                GenerateQrisCode();
                pnlQris.Visible = true;
                pnlCash.Visible = false;
                pnlTransfer.Visible = false;
                paymentMethod = "QRIS";
            }
            else if (rbTransfer.Checked)
            {
                // Show transfer instructions
                ShowTransferInstructions();
                pnlQris.Visible = false;
                pnlCash.Visible = false;
                pnlTransfer.Visible = true;
                paymentMethod = "Transfer";
            }
            else
            {
                // Show cash payment panel
                pnlQris.Visible = false;
                pnlCash.Visible = true;
                pnlTransfer.Visible = false;
                paymentMethod = "Cash";
                
                // Focus on cash amount textbox
                txtCashAmount.Focus();
            }
        }
        
        private void GenerateQrisCode()
        {
            try
            {
                // Generate QR code for payment
                string qrContent = $"BNI.ID12345678.MODERNPARKING.{referenceId}.{amount}";
                
                // Use custom QR code generation
                Bitmap qrImage = GenerateQRCodeImage(qrContent, 250, 250);
                
                // Display QR code
                if (picQris.Image != null)
                {
                    picQris.Image.Dispose();
                }
                picQris.Image = qrImage;
                
                // Show expiry time (15 minutes)
                DateTime expiry = DateTime.Now.AddMinutes(15);
                lblExpiry.Text = $"Berlaku hingga: {expiry.ToString("HH:mm:ss")}";
                
                // Show QRIS panel
                pnlQris.Visible = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in PaymentForm: {ex.Message}");
                _logger.Error($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error generating QRIS code: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ShowTransferInstructions()
        {
            // Set bank account details
            lblBankName.Text = "Bank BNI";
            lblAccountNumber.Text = "1234567890";
            lblAccountName.Text = "PT. Modern Parking System";
            lblTransferAmount.Text = string.Format("Rp {0:N0}", amount);
        }
        
        private void btnConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if payment method is selected
                if (string.IsNullOrEmpty(paymentMethod))
                {
                    MessageBox.Show("Silakan pilih metode pembayaran terlebih dahulu.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // For cash payment, validate the amount
                if (paymentMethod == "Cash")
                {
                    if (!decimal.TryParse(txtCashAmount.Text, out decimal cashAmount))
                    {
                        MessageBox.Show("Jumlah pembayaran tunai tidak valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCashAmount.Focus();
                        return;
                    }
                    
                    if (cashAmount < amount)
                    {
                        MessageBox.Show("Jumlah pembayaran kurang dari total tagihan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtCashAmount.Focus();
                        return;
                    }
                    
                    // Calculate change
                    decimal change = cashAmount - amount;
                    lblChange.Text = string.Format("Rp {0:N0}", change);
                    
                    // Record payment in database
                    RecordPayment(paymentMethod, cashAmount, change);
                }
                else
                {
                    // For non-cash payment
                    RecordPayment(paymentMethod, amount, 0);
                }
                
                MessageBox.Show("Pembayaran berhasil.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Close form with success
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error in PaymentForm: {ex.Message}");
                _logger.Error($"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Error processing payment: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void RecordPayment(string method, decimal amountPaid, decimal change)
        {
            try
            {
                // Check if payment table exists, create if not
                Database.ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS t_pembayaran (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        reference_id VARCHAR(50) NOT NULL,
                        metode_pembayaran VARCHAR(20) NOT NULL,
                        jumlah DECIMAL(10,2) NOT NULL,
                        pembayaran DECIMAL(10,2) NOT NULL,
                        kembalian DECIMAL(10,2) NOT NULL,
                        waktu_pembayaran TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        status VARCHAR(20) DEFAULT 'SUCCESS'
                    )
                ");
                
                // Insert payment record
                string query = @"
                    INSERT INTO t_pembayaran 
                    (reference_id, metode_pembayaran, jumlah, pembayaran, kembalian) 
                    VALUES 
                    (@reference_id, @metode_pembayaran, @jumlah, @pembayaran, @kembalian)
                ";
                
                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@reference_id", referenceId },
                    { "@metode_pembayaran", method },
                    { "@jumlah", amount },
                    { "@pembayaran", amountPaid },
                    { "@kembalian", change }
                };
                
                Database.ExecuteNonQuery(query, parameters);
                
                // Update original transaction with payment info
                if (referenceId.StartsWith("PKR"))
                {
                    // For parking transaction
                    string updateQuery = "UPDATE t_parkir SET status_pembayaran = 'PAID' WHERE id = @id";
                    Database.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { "@id", referenceId.Substring(3) } });
                }
                else if (referenceId.StartsWith("MBR"))
                {
                    // For membership transaction
                    string updateQuery = "UPDATE t_member_transaction SET status = 'PAID' WHERE id = @id";
                    Database.ExecuteNonQuery(updateQuery, new Dictionary<string, object> { { "@id", referenceId.Substring(3) } });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error recording payment: {ex.Message}");
            }
        }
        
        private void txtCashAmount_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (decimal.TryParse(txtCashAmount.Text, out decimal cashAmount))
                {
                    // Calculate change
                    decimal change = cashAmount - amount;
                    
                    // Display change (only if positive)
                    if (change >= 0)
                    {
                        lblChange.Text = string.Format("Rp {0:N0}", change);
                    }
                    else
                    {
                        lblChange.Text = "Insufficient amount";
                    }
                }
                else
                {
                    lblChange.Text = "Invalid amount";
                }
            }
            catch (Exception)
            {
                lblChange.Text = "Error";
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
        
        // Custom QR code generation method
        private Bitmap GenerateQRCodeImage(string content, int width, int height)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrBitmap = qrCode.GetGraphic(5); // 5 is the pixel size
            
            // Resize if needed
            if (qrBitmap.Width != width || qrBitmap.Height != height)
            {
                Bitmap resizedBitmap = new Bitmap(width, height);
                using (Graphics g = Graphics.FromImage(resizedBitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(qrBitmap, 0, 0, width, height);
                }
                qrBitmap.Dispose();
                return resizedBitmap;
            }
            
            return qrBitmap;
        }
        
        private void DrawPositionMarker(Graphics g, int x, int y, int size)
        {
            // Outer square
            g.FillRectangle(Brushes.Black, x, y, size, size);
            
            // Inner white square
            g.FillRectangle(Brushes.White, x + 10, y + 10, size - 20, size - 20);
            
            // Center black square
            g.FillRectangle(Brushes.Black, x + 20, y + 20, size - 40, size - 40);
        }
    }
} 