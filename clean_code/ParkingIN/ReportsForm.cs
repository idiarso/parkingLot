using System;
using System.IO;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using ParkingIN.Utils;  // Add explicit namespace reference

namespace ParkingIN
{
    public partial class ReportsForm : Form
    {
        public ReportsForm()
        {
            InitializeComponent();
        }

        private void ReportsForm_Load(object sender, EventArgs e)
        {
            // Set default date range to current day
            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Today;
            
            // Load report data
            LoadReportData();
        }
        
        private void LoadReportData()
        {
            try
            {
                // Clear previous data
                lvwReportData.Items.Clear();
                
                // Show loading indicator
                lblStatus.Text = "Loading data...";
                lblStatus.Visible = true;
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                
                // Get date range
                DateTime startDate = dtpStartDate.Value.Date;
                DateTime endDate = dtpEndDate.Value.Date.AddDays(1).AddSeconds(-1); // End of the day
                
                // Format dates for SQL query
                string startDateStr = startDate.ToString("yyyy-MM-dd HH:mm:ss");
                string endDateStr = endDate.ToString("yyyy-MM-dd HH:mm:ss");
                
                // Query for daily summary based on selected report type
                string query = "";
                
                switch (cmbReportType.SelectedIndex)
                {
                    case 0: // Daily Summary
                        query = $@"
                            SELECT 
                                DATE(waktu_masuk) as Tanggal,
                                COUNT(*) as JumlahKendaraan
                            FROM 
                                t_parkir
                            WHERE 
                                waktu_masuk BETWEEN '{startDateStr}' AND '{endDateStr}'
                            GROUP BY 
                                DATE(waktu_masuk)
                            ORDER BY 
                                DATE(waktu_masuk)
                        ";
                        break;
                        
                    case 1: // Vehicle Type
                        query = $@"
                            SELECT 
                                jenis_kendaraan as JenisKendaraan,
                                COUNT(*) as JumlahKendaraan
                            FROM 
                                t_parkir
                            WHERE 
                                waktu_masuk BETWEEN '{startDateStr}' AND '{endDateStr}'
                            GROUP BY 
                                jenis_kendaraan
                            ORDER BY 
                                COUNT(*) DESC
                        ";
                        break;
                        
                    case 2: // Hourly Summary
                        query = $@"
                            SELECT 
                                HOUR(waktu_masuk) as Jam,
                                COUNT(*) as JumlahKendaraan
                            FROM 
                                t_parkir
                            WHERE 
                                waktu_masuk BETWEEN '{startDateStr}' AND '{endDateStr}'
                            GROUP BY 
                                HOUR(waktu_masuk)
                            ORDER BY 
                                HOUR(waktu_masuk)
                        ";
                        break;
                        
                    case 3: // Current Status
                        query = $@"
                            SELECT 
                                status as Status,
                                COUNT(*) as JumlahKendaraan
                            FROM 
                                t_parkir
                            WHERE 
                                waktu_masuk BETWEEN '{startDateStr}' AND '{endDateStr}'
                            GROUP BY 
                                status
                            ORDER BY 
                                status
                        ";
                        break;
                }
                
                // Execute query
                DataTable results = ParkingIN.Utils.Database.GetData(query);  // Use fully qualified name
                
                if (results != null && results.Rows.Count > 0)
                {
                    // Update column headers based on report type
                    lvwReportData.Columns.Clear();
                    
                    switch (cmbReportType.SelectedIndex)
                    {
                        case 0: // Daily Summary
                            lvwReportData.Columns.Add("Tanggal", 150);
                            lvwReportData.Columns.Add("Jumlah Kendaraan", 150);
                            break;
                            
                        case 1: // Vehicle Type
                            lvwReportData.Columns.Add("Jenis Kendaraan", 150);
                            lvwReportData.Columns.Add("Jumlah Kendaraan", 150);
                            break;
                            
                        case 2: // Hourly Summary
                            lvwReportData.Columns.Add("Jam", 150);
                            lvwReportData.Columns.Add("Jumlah Kendaraan", 150);
                            break;
                            
                        case 3: // Current Status
                            lvwReportData.Columns.Add("Status", 150);
                            lvwReportData.Columns.Add("Jumlah Kendaraan", 150);
                            break;
                    }
                    
                    // Add data to ListView
                    int totalCount = 0;
                    
                    foreach (DataRow row in results.Rows)
                    {
                        ListViewItem item = new ListViewItem();
                        
                        switch (cmbReportType.SelectedIndex)
                        {
                            case 0: // Daily Summary
                                DateTime date = Convert.ToDateTime(row["Tanggal"]);
                                item.Text = date.ToString("dd/MM/yyyy");
                                item.SubItems.Add(row["JumlahKendaraan"].ToString());
                                totalCount += Convert.ToInt32(row["JumlahKendaraan"]);
                                break;
                                
                            case 1: // Vehicle Type
                                item.Text = row["JenisKendaraan"].ToString();
                                item.SubItems.Add(row["JumlahKendaraan"].ToString());
                                totalCount += Convert.ToInt32(row["JumlahKendaraan"]);
                                break;
                                
                            case 2: // Hourly Summary
                                int hour = Convert.ToInt32(row["Jam"]);
                                item.Text = hour.ToString("00") + ":00 - " + hour.ToString("00") + ":59";
                                item.SubItems.Add(row["JumlahKendaraan"].ToString());
                                totalCount += Convert.ToInt32(row["JumlahKendaraan"]);
                                break;
                                
                            case 3: // Current Status
                                item.Text = row["Status"].ToString();
                                item.SubItems.Add(row["JumlahKendaraan"].ToString());
                                totalCount += Convert.ToInt32(row["JumlahKendaraan"]);
                                break;
                        }
                        
                        lvwReportData.Items.Add(item);
                    }
                    
                    // Update total
                    lblTotalCount.Text = $"Total: {totalCount} kendaraan";
                    
                    // Show summary
                    string summaryText = "";
                    
                    switch (cmbReportType.SelectedIndex)
                    {
                        case 0: // Daily Summary
                            summaryText = $"Laporan Harian ({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";
                            break;
                            
                        case 1: // Vehicle Type
                            summaryText = $"Laporan Jenis Kendaraan ({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";
                            break;
                            
                        case 2: // Hourly Summary
                            summaryText = $"Laporan Per Jam ({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";
                            break;
                            
                        case 3: // Current Status
                            summaryText = $"Status Parkir ({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";
                            break;
                    }
                    
                    lblSummary.Text = summaryText;
                }
                else
                {
                    MessageBox.Show("Tidak ada data untuk ditampilkan dalam rentang tanggal yang dipilih.", 
                        "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    lblTotalCount.Text = "Total: 0 kendaraan";
                    lblSummary.Text = "Tidak ada data";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading report data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                lblStatus.Visible = false;
                Cursor = Cursors.Default;
            }
        }
        
        private void btnGenerate_Click(object sender, EventArgs e)
        {
            // Validate date range
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                MessageBox.Show("Tanggal akhir tidak boleh lebih awal dari tanggal mulai.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Load report data
            LoadReportData();
        }
        
        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvwReportData.Items.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk dicetak.", 
                        "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Print report
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();
                    printDoc.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(PrintReport);
                    printDoc.Print();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing report: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PrintReport(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            try
            {
                // Print header
                Font headerFont = new Font("Arial", 14, FontStyle.Bold);
                Font subHeaderFont = new Font("Arial", 12, FontStyle.Regular);
                Font normalFont = new Font("Arial", 10, FontStyle.Regular);
                Font boldFont = new Font("Arial", 10, FontStyle.Bold);
                
                // Start position
                int yPos = 100;
                int leftMargin = 50;
                
                // Print title
                e.Graphics.DrawString("LAPORAN PARKIR", headerFont, Brushes.Black, leftMargin, yPos);
                yPos += 30;
                
                // Print summary
                e.Graphics.DrawString(lblSummary.Text, subHeaderFont, Brushes.Black, leftMargin, yPos);
                yPos += 30;
                
                // Print date generated
                e.Graphics.DrawString($"Dicetak pada: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", normalFont, Brushes.Black, leftMargin, yPos);
                yPos += 30;
                
                // Draw separator line
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, e.PageBounds.Width - leftMargin, yPos);
                yPos += 20;
                
                // Print column headers
                int col1Width = 200;
                e.Graphics.DrawString(lvwReportData.Columns[0].Text, boldFont, Brushes.Black, leftMargin, yPos);
                e.Graphics.DrawString(lvwReportData.Columns[1].Text, boldFont, Brushes.Black, leftMargin + col1Width, yPos);
                yPos += 25;
                
                // Print data rows
                foreach (ListViewItem item in lvwReportData.Items)
                {
                    e.Graphics.DrawString(item.Text, normalFont, Brushes.Black, leftMargin, yPos);
                    e.Graphics.DrawString(item.SubItems[1].Text, normalFont, Brushes.Black, leftMargin + col1Width, yPos);
                    yPos += 20;
                    
                    // Check if we need to start a new page
                    if (yPos > e.PageBounds.Height - 100)
                    {
                        e.HasMorePages = true;
                        return;
                    }
                }
                
                // Print total
                yPos += 10;
                e.Graphics.DrawLine(Pens.Black, leftMargin, yPos, e.PageBounds.Width - leftMargin, yPos);
                yPos += 20;
                e.Graphics.DrawString(lblTotalCount.Text, boldFont, Brushes.Black, leftMargin, yPos);
                
                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during print: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvwReportData.Items.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diekspor.", 
                        "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Create save file dialog
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveDialog.Title = "Export Report";
                saveDialog.FileName = $"Parking_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create CSV content
                    List<string> lines = new List<string>();
                    
                    // Add header
                    lines.Add($"{lvwReportData.Columns[0].Text},{lvwReportData.Columns[1].Text}");
                    
                    // Add data
                    foreach (ListViewItem item in lvwReportData.Items)
                    {
                        lines.Add($"{item.Text},{item.SubItems[1].Text}");
                    }
                    
                    // Add summary
                    lines.Add("");
                    lines.Add(lblSummary.Text);
                    lines.Add(lblTotalCount.Text);
                    lines.Add($"Exported on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                    
                    // Write to file
                    File.WriteAllLines(saveDialog.FileName, lines);
                    
                    MessageBox.Show($"Report exported successfully to {saveDialog.FileName}", 
                        "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void cmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Reload data when report type changes
            if (cmbReportType.SelectedIndex >= 0)
            {
                LoadReportData();
            }
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Windows Form Designer generated code

        private Label lblTitle;
        private Label lblDateRange;
        private DateTimePicker dtpStartDate;
        private Label lblTo;
        private DateTimePicker dtpEndDate;
        private Label lblReportType;
        private ComboBox cmbReportType;
        private Button btnGenerate;
        private ListView lvwReportData;
        private Button btnPrint;
        private Button btnExport;
        private Button btnClose;
        private Label lblStatus;
        private Label lblSummary;
        private Label lblTotalCount;
        private Panel pnlHeader;
        private Panel pnlFooter;

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.lblDateRange = new Label();
            this.dtpStartDate = new DateTimePicker();
            this.lblTo = new Label();
            this.dtpEndDate = new DateTimePicker();
            this.lblReportType = new Label();
            this.cmbReportType = new ComboBox();
            this.btnGenerate = new Button();
            this.lvwReportData = new ListView();
            this.btnPrint = new Button();
            this.btnExport = new Button();
            this.btnClose = new Button();
            this.lblStatus = new Label();
            this.lblSummary = new Label();
            this.lblTotalCount = new Label();
            this.pnlHeader = new Panel();
            this.pnlFooter = new Panel();
            this.pnlHeader.SuspendLayout();
            this.pnlFooter.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlHeader.Controls.Add(this.lblTitle);
            this.pnlHeader.Controls.Add(this.lblDateRange);
            this.pnlHeader.Controls.Add(this.dtpStartDate);
            this.pnlHeader.Controls.Add(this.lblTo);
            this.pnlHeader.Controls.Add(this.dtpEndDate);
            this.pnlHeader.Controls.Add(this.lblReportType);
            this.pnlHeader.Controls.Add(this.cmbReportType);
            this.pnlHeader.Controls.Add(this.btnGenerate);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(800, 120);
            this.pnlHeader.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(158, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Parking Report";
            // 
            // lblDateRange
            // 
            this.lblDateRange.AutoSize = true;
            this.lblDateRange.Location = new Point(15, 55);
            this.lblDateRange.Name = "lblDateRange";
            this.lblDateRange.Size = new Size(70, 15);
            this.lblDateRange.TabIndex = 1;
            this.lblDateRange.Text = "Date Range:";
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.Format = DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new Point(90, 52);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new Size(100, 23);
            this.dtpStartDate.TabIndex = 2;
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new Point(196, 55);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new Size(19, 15);
            this.lblTo.TabIndex = 3;
            this.lblTo.Text = "to";
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.Format = DateTimePickerFormat.Short;
            this.dtpEndDate.Location = new Point(220, 52);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new Size(100, 23);
            this.dtpEndDate.TabIndex = 4;
            // 
            // lblReportType
            // 
            this.lblReportType.AutoSize = true;
            this.lblReportType.Location = new Point(15, 85);
            this.lblReportType.Name = "lblReportType";
            this.lblReportType.Size = new Size(73, 15);
            this.lblReportType.TabIndex = 5;
            this.lblReportType.Text = "Report Type:";
            // 
            // cmbReportType
            // 
            this.cmbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbReportType.FormattingEnabled = true;
            this.cmbReportType.Items.AddRange(new object[] {
            "Daily Summary",
            "Vehicle Type",
            "Hourly Summary",
            "Current Status"});
            this.cmbReportType.Location = new Point(90, 82);
            this.cmbReportType.Name = "cmbReportType";
            this.cmbReportType.Size = new Size(150, 23);
            this.cmbReportType.TabIndex = 6;
            this.cmbReportType.SelectedIndexChanged += new EventHandler(this.cmbReportType_SelectedIndexChanged);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new Point(350, 80);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new Size(100, 30);
            this.btnGenerate.TabIndex = 7;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
            // 
            // lvwReportData
            // 
            this.lvwReportData.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) | AnchorStyles.Left) | AnchorStyles.Right)));
            this.lvwReportData.FullRowSelect = true;
            this.lvwReportData.GridLines = true;
            this.lvwReportData.Location = new Point(12, 150);
            this.lvwReportData.Name = "lvwReportData";
            this.lvwReportData.Size = new Size(776, 300);
            this.lvwReportData.TabIndex = 1;
            this.lvwReportData.UseCompatibleStateImageBehavior = false;
            this.lvwReportData.View = View.Details;
            // 
            // pnlFooter
            // 
            this.pnlFooter.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlFooter.Controls.Add(this.lblSummary);
            this.pnlFooter.Controls.Add(this.lblTotalCount);
            this.pnlFooter.Controls.Add(this.btnPrint);
            this.pnlFooter.Controls.Add(this.btnExport);
            this.pnlFooter.Controls.Add(this.btnClose);
            this.pnlFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlFooter.Location = new Point(0, 460);
            this.pnlFooter.Name = "pnlFooter";
            this.pnlFooter.Size = new System.Drawing.Size(800, 60);
            this.pnlFooter.TabIndex = 2;
            // 
            // lblSummary
            // 
            this.lblSummary.AutoSize = true;
            this.lblSummary.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblSummary.Location = new Point(15, 15);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new Size(61, 15);
            this.lblSummary.TabIndex = 0;
            this.lblSummary.Text = "Summary";
            // 
            // lblTotalCount
            // 
            this.lblTotalCount.AutoSize = true;
            this.lblTotalCount.Location = new Point(15, 35);
            this.lblTotalCount.Name = "lblTotalCount";
            this.lblTotalCount.Size = new Size(39, 15);
            this.lblTotalCount.TabIndex = 1;
            this.lblTotalCount.Text = "Total: ";
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnPrint.Location = new Point(525, 15);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new Size(80, 30);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new EventHandler(this.btnPrint_Click);
            // 
            // btnExport
            // 
            this.btnExport.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnExport.Location = new Point(615, 15);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new Size(80, 30);
            this.btnExport.TabIndex = 3;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new EventHandler(this.btnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnClose.Location = new Point(705, 15);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(80, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Italic, GraphicsUnit.Point);
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblStatus.Location = new Point(12, 130);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(42, 15);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "Status";
            this.lblStatus.Visible = false;
            // 
            // ReportsForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(800, 520);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.pnlFooter);
            this.Controls.Add(this.lvwReportData);
            this.Controls.Add(this.pnlHeader);
            this.MinimumSize = new Size(700, 500);
            this.Name = "ReportsForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Parking Reports";
            this.Load += new EventHandler(this.ReportsForm_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.pnlFooter.ResumeLayout(false);
            this.pnlFooter.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 