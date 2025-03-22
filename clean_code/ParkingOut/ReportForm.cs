using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using SimpleParkingAdmin.Utils;
using Serilog;
using ClosedXML.Excel;
using SimpleParkingAdmin.Models;
using Serilog.Events;

namespace SimpleParkingAdmin.Forms
{
    public partial class ReportForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;
        private string reportDirectory;

        public ReportForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = new FileLogger();
            InitializeComponent();
            
            // Initialize directory for reports
            reportDirectory = Path.Combine(Application.StartupPath, "Reports");
            if (!Directory.Exists(reportDirectory))
            {
                Directory.CreateDirectory(reportDirectory);
            }
            
            // Set default dates
            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Today;
            
            // Load vehicle types
            LoadVehicleTypes();
        }

        private void LoadVehicleTypes()
        {
            try
            {
                // Use a query that only accesses the t_tarif table and avoids any joins
                string query = "SELECT jenis_kendaraan FROM t_tarif WHERE status = TRUE GROUP BY jenis_kendaraan ORDER BY jenis_kendaraan";
                _logger.Debug($"Loading vehicle types with query: {query}");
                
                var result = Database.ExecuteQuery(query);
                
                cmbVehicleType.Items.Clear();
                cmbVehicleType.Items.Add("All");
                
                foreach (DataRow row in result.Rows)
                {
                    cmbVehicleType.Items.Add(row["jenis_kendaraan"].ToString());
                }
                
                cmbVehicleType.SelectedIndex = 0;
                _logger.Info($"Loaded {result.Rows.Count} vehicle types");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to load vehicle types: {ex.Message}", ex);
                NotificationHelper.ShowError($"Failed to load vehicle types. Error: {ex.Message}");
            }
        }

        private void GenerateReport()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                btnExportExcel.Enabled = false; // Disable export button while generating report
                
                string vehicleFilter = cmbVehicleType.SelectedIndex > 0 
                                     ? $" AND p.jenis_kendaraan = '{cmbVehicleType.SelectedItem}'" 
                                     : "";
                
                string dateFormat = "yyyy-MM-dd";
                string startDate = dtpStartDate.Value.ToString(dateFormat);
                string endDate = dtpEndDate.Value.ToString(dateFormat);
                
                string query = "";
                
                // Build query based on report type
                if (rbDaily.Checked)
                {
                    // Daily report
                    query = BuildDailyReport(startDate, endDate, vehicleFilter);
                }
                else if (rbMonthly.Checked)
                {
                    // Monthly report
                    query = BuildMonthlyReport(startDate, endDate, vehicleFilter);
                }
                else if (rbVehicle.Checked)
                {
                    // Vehicle type report
                    query = BuildVehicleReport(startDate, endDate, vehicleFilter);
                }
                else if (rbMember.Checked)
                {
                    // Member report
                    query = BuildMemberReport(startDate, endDate, vehicleFilter);
                }

                _logger.Debug($"Executing report query: {query}");

                var result = Database.ExecuteQuery(query);
                dgvReport.DataSource = result;
                
                _logger.Debug($"Report executed successfully: {result.Rows.Count} rows returned");
                
                // Format columns
                foreach (DataGridViewColumn col in dgvReport.Columns)
                {
                    if (col.Name.Contains("total") || col.Name.Contains("tarif") || col.Name.Contains("pendapatan"))
                    {
                        col.DefaultCellStyle.Format = "N0";
                        col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                    else if (col.Name.Contains("tanggal"))
                    {
                        col.DefaultCellStyle.Format = "dd/MM/yyyy";
                    }
                }
                
                // Update summary
                decimal totalRevenue = 0;
                int totalVehicles = 0;

                foreach (DataRow row in result.Rows)
                {
                    totalVehicles += Convert.ToInt32(row["jumlah_kendaraan"]);
                    if (row["total_pendapatan"] != DBNull.Value)
                    {
                        totalRevenue += Convert.ToDecimal(row["total_pendapatan"]);
                    }
                }

                lblTotalVehicles.Text = $"Total Vehicles: {totalVehicles:N0}";
                lblTotalRevenue.Text = $"Total Revenue: Rp {totalRevenue:N0}";
                lblRecordCount.Text = $"Total Records: {result.Rows.Count:N0}";

                // Enable export button if we have data
                btnExportExcel.Enabled = result.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to generate report: {ex.Message}", ex);
                NotificationHelper.ShowError($"Failed to generate report. Error: {ex.Message}");
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private string BuildDailyReport(string startDate, string endDate, string vehicleFilter)
        {
            return $@"
                SELECT 
                    DATE(p.waktu_masuk) as tanggal,
                    p.jenis_kendaraan,
                    COUNT(*) as jumlah_kendaraan,
                    SUM(CASE WHEN p.nomor_kartu IS NOT NULL THEN 1 ELSE 0 END) as jumlah_member,
                    SUM(CASE WHEN p.nomor_kartu IS NULL THEN 1 ELSE 0 END) as jumlah_non_member,
                    SUM(COALESCE(p.biaya, 0)) as total_pendapatan
                FROM t_parkir p
                WHERE DATE(p.waktu_masuk) BETWEEN '{startDate}' 
                    AND '{endDate}'
                    {vehicleFilter}
                GROUP BY DATE(p.waktu_masuk), p.jenis_kendaraan
                ORDER BY DATE(p.waktu_masuk), p.jenis_kendaraan";
        }

        private string BuildMonthlyReport(string startDate, string endDate, string vehicleFilter)
        {
            return $@"
                SELECT 
                    EXTRACT(YEAR FROM p.waktu_masuk) as tahun,
                    EXTRACT(MONTH FROM p.waktu_masuk) as bulan,
                    p.jenis_kendaraan,
                    COUNT(*) as jumlah_kendaraan,
                    SUM(CASE WHEN p.nomor_kartu IS NOT NULL THEN 1 ELSE 0 END) as jumlah_member,
                    SUM(CASE WHEN p.nomor_kartu IS NULL THEN 1 ELSE 0 END) as jumlah_non_member,
                    SUM(COALESCE(p.biaya, 0)) as total_pendapatan
                FROM t_parkir p
                WHERE DATE(p.waktu_masuk) BETWEEN '{startDate}' 
                    AND '{endDate}'
                    {vehicleFilter}
                GROUP BY EXTRACT(YEAR FROM p.waktu_masuk), EXTRACT(MONTH FROM p.waktu_masuk), p.jenis_kendaraan
                ORDER BY EXTRACT(YEAR FROM p.waktu_masuk), EXTRACT(MONTH FROM p.waktu_masuk), p.jenis_kendaraan";
        }

        private string BuildVehicleReport(string startDate, string endDate, string vehicleFilter)
        {
            return $@"
                SELECT 
                    p.jenis_kendaraan,
                    COUNT(*) as jumlah_kendaraan,
                    SUM(CASE WHEN p.nomor_kartu IS NOT NULL THEN 1 ELSE 0 END) as jumlah_member,
                    SUM(CASE WHEN p.nomor_kartu IS NULL THEN 1 ELSE 0 END) as jumlah_non_member,
                    SUM(COALESCE(p.biaya, 0)) as total_pendapatan
                FROM t_parkir p
                WHERE DATE(p.waktu_masuk) BETWEEN '{startDate}' 
                    AND '{endDate}'
                    {vehicleFilter}
                GROUP BY p.jenis_kendaraan
                ORDER BY p.jenis_kendaraan";
        }

        private string BuildMemberReport(string startDate, string endDate, string vehicleFilter)
        {
            return $@"
                SELECT 
                    CASE WHEN m.member_id IS NOT NULL
                        THEN CONCAT(m.nama_pemilik, ' (', m.nomor_kartu, ')')
                        ELSE 'Non-Member'
                    END as member_info,
                    COUNT(*) as jumlah_kendaraan,
                    SUM(COALESCE(p.biaya, 0)) as total_pendapatan
                FROM t_parkir p
                LEFT JOIN m_member m ON p.nomor_kartu = m.nomor_kartu
                WHERE DATE(p.waktu_masuk) BETWEEN '{startDate}' 
                    AND '{endDate}'
                    {vehicleFilter}
                GROUP BY member_info
                ORDER BY member_info";
        }

        private void ExportToExcel()
        {
            try
            {
                if (dgvReport.Rows.Count == 0)
                {
                    NotificationHelper.ShowWarning("No data to export.");
                    return;
                }
                
                Cursor = Cursors.WaitCursor;
                btnExportExcel.Enabled = false; // Disable button while exporting
                
                // Create a unique filename based on report type and date range
                string reportType = rbDaily.Checked ? "Daily" : 
                                    rbMonthly.Checked ? "Monthly" : 
                                    rbVehicle.Checked ? "Vehicle" : "Member";
                
                string fileName = $"Report_{reportType}_{dtpStartDate.Value:yyyyMMdd}_to_{dtpEndDate.Value:yyyyMMdd}.xlsx";
                string filePath = Path.Combine(reportDirectory, fileName);
                
                // Create Excel workbook
                using (XLWorkbook wb = new XLWorkbook())
                {
                    DataTable dt = (DataTable)dgvReport.DataSource;
                    wb.Worksheets.Add(dt, "Report");
                    wb.SaveAs(filePath);
                }
                
                NotificationHelper.ShowInformation($"Report exported successfully to:\n{filePath}");
                
                // Open the file with the default application
                if (MessageBox.Show("Do you want to open the exported file?", "Open File", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to export report", ex);
                NotificationHelper.ShowError("Failed to export report.");
            }
            finally
            {
                Cursor = Cursors.Default;
                btnExportExcel.Enabled = dgvReport.Rows.Count > 0; // Re-enable button if we have data
            }
        }

        #region Event Handlers

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            ExportToExcel();
        }

        private void dtpStartDate_ValueChanged(object sender, EventArgs e)
        {
            // Ensure the end date is not before the start date
            if (dtpEndDate.Value < dtpStartDate.Value)
            {
                dtpEndDate.Value = dtpStartDate.Value;
            }
        }

        private void dtpEndDate_ValueChanged(object sender, EventArgs e)
        {
            // Optional: could add validation here if needed
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            // Generate initial report with today's data
            GenerateReport();
        }

        #endregion

        #region Windows Form Designer generated code

        private DataGridView dgvReport;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbVehicleType;
        private Button btnGenerateReport;
        private Button btnExportExcel;
        private Label lblTitle;
        private Label lblStartDate;
        private Label lblEndDate;
        private Label lblVehicleType;
        private Label lblReportType;
        private Label lblRecordCount;
        private RadioButton rbDaily;
        private RadioButton rbMonthly;
        private RadioButton rbVehicle;
        private RadioButton rbMember;
        private Panel pnlReportType;
        private Label lblTotalVehicles;
        private Label lblTotalRevenue;

        private void InitializeComponent()
        {
            this.dgvReport = new DataGridView();
            this.dtpStartDate = new DateTimePicker();
            this.dtpEndDate = new DateTimePicker();
            this.cmbVehicleType = new ComboBox();
            this.btnGenerateReport = new Button();
            this.btnExportExcel = new Button();
            this.lblTitle = new Label();
            this.lblStartDate = new Label();
            this.lblEndDate = new Label();
            this.lblVehicleType = new Label();
            this.lblReportType = new Label();
            this.lblRecordCount = new Label();
            this.rbDaily = new RadioButton();
            this.rbMonthly = new RadioButton();
            this.rbVehicle = new RadioButton();
            this.rbMember = new RadioButton();
            this.pnlReportType = new Panel();
            this.lblTotalVehicles = new Label();
            this.lblTotalRevenue = new Label();

            this.pnlReportType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReport)).BeginInit();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(143, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Parking Report";

            // lblStartDate
            this.lblStartDate.AutoSize = true;
            this.lblStartDate.Location = new Point(14, 50);
            this.lblStartDate.Name = "lblStartDate";
            this.lblStartDate.Size = new Size(85, 15);
            this.lblStartDate.TabIndex = 1;
            this.lblStartDate.Text = "Tanggal Mulai:";

            // dtpStartDate
            this.dtpStartDate.Format = DateTimePickerFormat.Short;
            this.dtpStartDate.Location = new Point(105, 46);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new Size(120, 23);
            this.dtpStartDate.TabIndex = 0;
            this.dtpStartDate.ValueChanged += new EventHandler(this.dtpStartDate_ValueChanged);

            // lblEndDate
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new Point(240, 50);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new Size(92, 15);
            this.lblEndDate.TabIndex = 2;
            this.lblEndDate.Text = "Tanggal Selesai:";

            // dtpEndDate
            this.dtpEndDate.Format = DateTimePickerFormat.Short;
            this.dtpEndDate.Location = new Point(338, 46);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new Size(120, 23);
            this.dtpEndDate.TabIndex = 1;
            this.dtpEndDate.ValueChanged += new EventHandler(this.dtpEndDate_ValueChanged);

            // lblVehicleType
            this.lblVehicleType.AutoSize = true;
            this.lblVehicleType.Location = new Point(14, 84);
            this.lblVehicleType.Name = "lblVehicleType";
            this.lblVehicleType.Size = new Size(98, 15);
            this.lblVehicleType.TabIndex = 3;
            this.lblVehicleType.Text = "Jenis Kendaraan:";

            // cmbVehicleType
            this.cmbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbVehicleType.FormattingEnabled = true;
            this.cmbVehicleType.Location = new Point(118, 80);
            this.cmbVehicleType.Name = "cmbVehicleType";
            this.cmbVehicleType.Size = new Size(200, 23);
            this.cmbVehicleType.TabIndex = 2;

            // lblReportType
            this.lblReportType.AutoSize = true;
            this.lblReportType.Location = new Point(14, 118);
            this.lblReportType.Name = "lblReportType";
            this.lblReportType.Size = new Size(78, 15);
            this.lblReportType.TabIndex = 4;
            this.lblReportType.Text = "Jenis Laporan:";

            // pnlReportType
            this.pnlReportType.Controls.Add(this.rbDaily);
            this.pnlReportType.Controls.Add(this.rbMonthly);
            this.pnlReportType.Controls.Add(this.rbVehicle);
            this.pnlReportType.Controls.Add(this.rbMember);
            this.pnlReportType.Location = new Point(118, 114);
            this.pnlReportType.Name = "pnlReportType";
            this.pnlReportType.Size = new Size(467, 30);
            this.pnlReportType.TabIndex = 3;

            // rbDaily
            this.rbDaily.AutoSize = true;
            this.rbDaily.Checked = true;
            this.rbDaily.Location = new Point(3, 3);
            this.rbDaily.Name = "rbDaily";
            this.rbDaily.Size = new Size(111, 19);
            this.rbDaily.TabIndex = 0;
            this.rbDaily.TabStop = true;
            this.rbDaily.Text = "Laporan Harian";
            this.rbDaily.UseVisualStyleBackColor = true;

            // rbMonthly
            this.rbMonthly.AutoSize = true;
            this.rbMonthly.Location = new Point(120, 3);
            this.rbMonthly.Name = "rbMonthly";
            this.rbMonthly.Size = new Size(119, 19);
            this.rbMonthly.TabIndex = 1;
            this.rbMonthly.Text = "Laporan Bulanan";
            this.rbMonthly.UseVisualStyleBackColor = true;

            // rbVehicle
            this.rbVehicle.AutoSize = true;
            this.rbVehicle.Location = new Point(245, 3);
            this.rbVehicle.Name = "rbVehicle";
            this.rbVehicle.Size = new Size(108, 19);
            this.rbVehicle.TabIndex = 2;
            this.rbVehicle.Text = "Detail Transaksi";
            this.rbVehicle.UseVisualStyleBackColor = true;

            // rbMember
            this.rbMember.AutoSize = true;
            this.rbMember.Location = new Point(359, 3);
            this.rbMember.Name = "rbMember";
            this.rbMember.Size = new Size(113, 19);
            this.rbMember.TabIndex = 3;
            this.rbMember.Text = "Laporan Member";
            this.rbMember.UseVisualStyleBackColor = true;

            // btnGenerateReport
            this.btnGenerateReport.Location = new Point(14, 150);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new Size(120, 30);
            this.btnGenerateReport.TabIndex = 4;
            this.btnGenerateReport.Text = "Generate Report";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new EventHandler(this.btnGenerateReport_Click);

            // btnExportExcel
            this.btnExportExcel.Enabled = false;
            this.btnExportExcel.Location = new Point(140, 150);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new Size(120, 30);
            this.btnExportExcel.TabIndex = 5;
            this.btnExportExcel.Text = "Export to Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new EventHandler(this.btnExportExcel_Click);

            // lblRecordCount
            this.lblRecordCount.Anchor = ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)));
            this.lblRecordCount.AutoSize = true;
            this.lblRecordCount.Location = new Point(498, 158);
            this.lblRecordCount.Name = "lblRecordCount";
            this.lblRecordCount.Size = new Size(87, 15);
            this.lblRecordCount.TabIndex = 6;
            this.lblRecordCount.Text = "Total Records: 0";
            this.lblRecordCount.TextAlign = ContentAlignment.MiddleRight;

            // dgvReport
            this.dgvReport.AllowUserToAddRows = false;
            this.dgvReport.AllowUserToDeleteRows = false;
            this.dgvReport.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                | AnchorStyles.Left) 
                | AnchorStyles.Right)));
            this.dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvReport.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvReport.Location = new Point(14, 189);
            this.dgvReport.Name = "dgvReport";
            this.dgvReport.ReadOnly = true;
            this.dgvReport.RowTemplate.Height = 25;
            this.dgvReport.Size = new Size(760, 360);
            this.dgvReport.TabIndex = 7;

            // lblTotalVehicles
            this.lblTotalVehicles.AutoSize = true;
            this.lblTotalVehicles.Location = new Point(14, 520);
            this.lblTotalVehicles.Name = "lblTotalVehicles";
            this.lblTotalVehicles.Size = new Size(100, 15);
            this.lblTotalVehicles.TabIndex = 8;
            this.lblTotalVehicles.Text = "Total Vehicles: 0";

            // lblTotalRevenue
            this.lblTotalRevenue.AutoSize = true;
            this.lblTotalRevenue.Location = new Point(14, 550);
            this.lblTotalRevenue.Name = "lblTotalRevenue";
            this.lblTotalRevenue.Size = new Size(100, 15);
            this.lblTotalRevenue.TabIndex = 9;
            this.lblTotalRevenue.Text = "Total Revenue: Rp 0";

            // ReportForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(788, 561);
            this.Controls.Add(this.lblTotalRevenue);
            this.Controls.Add(this.lblTotalVehicles);
            this.Controls.Add(this.dgvReport);
            this.Controls.Add(this.lblRecordCount);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.btnGenerateReport);
            this.Controls.Add(this.pnlReportType);
            this.Controls.Add(this.lblReportType);
            this.Controls.Add(this.cmbVehicleType);
            this.Controls.Add(this.lblVehicleType);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.dtpStartDate);
            this.Controls.Add(this.lblStartDate);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new Size(700, 600);
            this.Name = "ReportForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Parking Reports";
            this.Load += new EventHandler(this.ReportForm_Load);

            this.pnlReportType.ResumeLayout(false);
            this.pnlReportType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvReport)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 