using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;
using System.Globalization;
using IUTVehicleManager.Database;

namespace IUTVehicleManager.Forms
{
    public partial class ReportsForm : Form
    {
        private ILogger _logger;
        private SqlConnection _connection;
        private readonly string _connectionString;

        public ReportsForm(ILogger logger)
        {
            InitializeComponent();
            _logger = logger;
            
            // Get connection string from app config
            _connectionString = ConfigurationManager.ConnectionStrings["GetInConnection"].ConnectionString;
        }

        private void InitializeComponent()
        {
            this.dtpStartDate = new System.Windows.Forms.DateTimePicker();
            this.dtpEndDate = new System.Windows.Forms.DateTimePicker();
            this.lblStartDate = new System.Windows.Forms.Label();
            this.lblEndDate = new System.Windows.Forms.Label();
            this.btnGenerateReport = new System.Windows.Forms.Button();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.lblTotalVehicles = new System.Windows.Forms.Label();
            this.cmbReportType = new System.Windows.Forms.ComboBox();
            this.lblReportType = new System.Windows.Forms.Label();
            this.lblFilterByPlate = new System.Windows.Forms.Label();
            this.txtFilterByPlate = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dtpStartDate
            // 
            this.dtpStartDate.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpStartDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpStartDate.Location = new System.Drawing.Point(120, 15);
            this.dtpStartDate.Name = "dtpStartDate";
            this.dtpStartDate.Size = new System.Drawing.Size(150, 23);
            this.dtpStartDate.TabIndex = 0;
            // 
            // dtpEndDate
            // 
            this.dtpEndDate.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpEndDate.Location = new System.Drawing.Point(390, 15);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(150, 23);
            this.dtpEndDate.TabIndex = 1;
            // 
            // lblStartDate
            // 
            this.lblStartDate.AutoSize = true;
            this.lblStartDate.Location = new System.Drawing.Point(15, 19);
            this.lblStartDate.Name = "lblStartDate";
            this.lblStartDate.Size = new System.Drawing.Size(99, 15);
            this.lblStartDate.TabIndex = 2;
            this.lblStartDate.Text = "Tanggal Mulai:";
            // 
            // lblEndDate
            // 
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(285, 19);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(99, 15);
            this.lblEndDate.TabIndex = 3;
            this.lblEndDate.Text = "Tanggal Akhir:";
            // 
            // btnGenerateReport
            // 
            this.btnGenerateReport.Location = new System.Drawing.Point(590, 50);
            this.btnGenerateReport.Name = "btnGenerateReport";
            this.btnGenerateReport.Size = new System.Drawing.Size(180, 30);
            this.btnGenerateReport.TabIndex = 4;
            this.btnGenerateReport.Text = "Generate Laporan";
            this.btnGenerateReport.UseVisualStyleBackColor = true;
            this.btnGenerateReport.Click += new System.EventHandler(this.btnGenerateReport_Click);
            // 
            // dataGridView
            // 
            this.dataGridView.AllowUserToAddRows = false;
            this.dataGridView.AllowUserToDeleteRows = false;
            this.dataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.Location = new System.Drawing.Point(15, 90);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.ReadOnly = true;
            this.dataGridView.RowTemplate.Height = 25;
            this.dataGridView.Size = new System.Drawing.Size(870, 430);
            this.dataGridView.TabIndex = 5;
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExportCsv.Location = new System.Drawing.Point(15, 530);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(120, 30);
            this.btnExportCsv.TabIndex = 6;
            this.btnExportCsv.Text = "Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExportExcel.Location = new System.Drawing.Point(150, 530);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(120, 30);
            this.btnExportExcel.TabIndex = 7;
            this.btnExportExcel.Text = "Export Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPrint.Location = new System.Drawing.Point(285, 530);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(120, 30);
            this.btnPrint.TabIndex = 8;
            this.btnPrint.Text = "Print";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // lblTotalVehicles
            // 
            this.lblTotalVehicles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTotalVehicles.AutoSize = true;
            this.lblTotalVehicles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTotalVehicles.Location = new System.Drawing.Point(590, 537);
            this.lblTotalVehicles.Name = "lblTotalVehicles";
            this.lblTotalVehicles.Size = new System.Drawing.Size(145, 15);
            this.lblTotalVehicles.TabIndex = 9;
            this.lblTotalVehicles.Text = "Total Kendaraan: 0";
            // 
            // cmbReportType
            // 
            this.cmbReportType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbReportType.FormattingEnabled = true;
            this.cmbReportType.Items.AddRange(new object[] {
            "Semua Kendaraan",
            "Kendaraan Masuk Hari Ini",
            "Kendaraan Berdasarkan Jenis",
            "Laporan Harian"});
            this.cmbReportType.Location = new System.Drawing.Point(120, 50);
            this.cmbReportType.Name = "cmbReportType";
            this.cmbReportType.Size = new System.Drawing.Size(180, 23);
            this.cmbReportType.TabIndex = 10;
            // 
            // lblReportType
            // 
            this.lblReportType.AutoSize = true;
            this.lblReportType.Location = new System.Drawing.Point(15, 53);
            this.lblReportType.Name = "lblReportType";
            this.lblReportType.Size = new System.Drawing.Size(93, 15);
            this.lblReportType.TabIndex = 11;
            this.lblReportType.Text = "Jenis Laporan:";
            // 
            // lblFilterByPlate
            // 
            this.lblFilterByPlate.AutoSize = true;
            this.lblFilterByPlate.Location = new System.Drawing.Point(320, 53);
            this.lblFilterByPlate.Name = "lblFilterByPlate";
            this.lblFilterByPlate.Size = new System.Drawing.Size(64, 15);
            this.lblFilterByPlate.TabIndex = 12;
            this.lblFilterByPlate.Text = "Filter Plat:";
            // 
            // txtFilterByPlate
            // 
            this.txtFilterByPlate.Location = new System.Drawing.Point(390, 50);
            this.txtFilterByPlate.Name = "txtFilterByPlate";
            this.txtFilterByPlate.Size = new System.Drawing.Size(180, 23);
            this.txtFilterByPlate.TabIndex = 13;
            // 
            // ReportsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(900, 571);
            this.Controls.Add(this.txtFilterByPlate);
            this.Controls.Add(this.lblFilterByPlate);
            this.Controls.Add(this.lblReportType);
            this.Controls.Add(this.cmbReportType);
            this.Controls.Add(this.lblTotalVehicles);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.btnExportCsv);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.btnGenerateReport);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.lblStartDate);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.dtpStartDate);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "ReportsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Laporan Kendaraan";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReportsForm_FormClosing);
            this.Load += new System.EventHandler(this.ReportsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ReportsForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Set default values
                dtpStartDate.Value = DateTime.Today;
                dtpEndDate.Value = DateTime.Now;
                cmbReportType.SelectedIndex = 0;
                
                // Connect to database
                ConnectToDatabase();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error initializing reports form: {ex.Message}");
                MessageBox.Show($"Error initializing reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectToDatabase()
        {
            try
            {
                _connection = new SqlConnection(_connectionString);
                _connection.Open();
                _logger.Info("Connected to database for reports");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error connecting to database: {ex.Message}");
                throw;
            }
        }

        private void btnGenerateReport_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate inputs
                DateTime startDate = dtpStartDate.Value;
                DateTime endDate = dtpEndDate.Value;
                
                if (startDate > endDate)
                {
                    MessageBox.Show("Tanggal mulai harus sebelum tanggal akhir.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Build and execute query based on report type
                string sqlQuery = BuildSqlQuery();
                
                using (SqlCommand cmd = new SqlCommand(sqlQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    
                    if (!string.IsNullOrWhiteSpace(txtFilterByPlate.Text))
                    {
                        cmd.Parameters.AddWithValue("@PlateNumber", "%" + txtFilterByPlate.Text + "%");
                    }
                    
                    // Execute the query and fill the DataGridView
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        
                        dataGridView.DataSource = dataTable;
                        
                        // Update total count
                        lblTotalVehicles.Text = $"Total Kendaraan: {dataTable.Rows.Count}";
                        
                        _logger.Info($"Generated report: {cmbReportType.Text} with {dataTable.Rows.Count} records");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error generating report: {ex.Message}");
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string BuildSqlQuery()
        {
            StringBuilder sql = new StringBuilder();
            
            // Base select
            sql.Append("SELECT VehicleId, PlateNumber, VehicleType, EntryTime, OperatorName ");
            sql.Append("FROM VehicleEntries ");
            
            // Apply filters based on report type
            switch (cmbReportType.SelectedIndex)
            {
                case 0: // All vehicles
                    sql.Append("WHERE EntryTime BETWEEN @StartDate AND @EndDate ");
                    break;
                    
                case 1: // Today's vehicles
                    sql.Append("WHERE CONVERT(date, EntryTime) = CONVERT(date, GETDATE()) ");
                    break;
                    
                case 2: // By vehicle type
                    sql.Append("WHERE EntryTime BETWEEN @StartDate AND @EndDate ");
                    sql.Append("AND VehicleType = @VehicleType ");
                    break;
                    
                case 3: // Daily report
                    sql.Append("WHERE CONVERT(date, EntryTime) = CONVERT(date, @StartDate) ");
                    break;
            }
            
            // Apply plate number filter if provided
            if (!string.IsNullOrWhiteSpace(txtFilterByPlate.Text))
            {
                sql.Append("AND PlateNumber LIKE @PlateNumber ");
            }
            
            // Sort by entry time, newest first
            sql.Append("ORDER BY EntryTime DESC");
            
            return sql.ToString();
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diekspor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Export to CSV",
                    FileName = $"VehicleReport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
                };
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder csv = new StringBuilder();
                    
                    // Add headers
                    DataTable dt = (DataTable)dataGridView.DataSource;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        csv.Append(dt.Columns[i].ColumnName);
                        csv.Append(i < dt.Columns.Count - 1 ? "," : Environment.NewLine);
                    }
                    
                    // Add rows
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.Append("\"" + row[i].ToString().Replace("\"", "\"\"") + "\"");
                            csv.Append(i < dt.Columns.Count - 1 ? "," : Environment.NewLine);
                        }
                    }
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show("Data berhasil diekspor ke CSV.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _logger.Info($"Exported report to CSV: {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error exporting to CSV: {ex.Message}");
                MessageBox.Show($"Error exporting to CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diekspor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = "Export to Excel",
                    FileName = $"VehicleReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };
                
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Simple Excel export (CSV with .xlsx extension)
                    // In a real app, you would use a library like EPPlus or NPOI
                    StringBuilder csv = new StringBuilder();
                    
                    // Add headers
                    DataTable dt = (DataTable)dataGridView.DataSource;
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        csv.Append(dt.Columns[i].ColumnName);
                        csv.Append(i < dt.Columns.Count - 1 ? "," : Environment.NewLine);
                    }
                    
                    // Add rows
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.Append("\"" + row[i].ToString().Replace("\"", "\"\"") + "\"");
                            csv.Append(i < dt.Columns.Count - 1 ? "," : Environment.NewLine);
                        }
                    }
                    
                    File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                    
                    MessageBox.Show("Data berhasil diekspor ke Excel.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    _logger.Info($"Exported report to Excel: {saveFileDialog.FileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error exporting to Excel: {ex.Message}");
                MessageBox.Show($"Error exporting to Excel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk dicetak.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // In a real app, you would use a proper printing library
                // This is a simple message to show the functionality
                MessageBox.Show("Fungsi cetak akan tersedia dalam update selanjutnya.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _logger.Info("Print function requested (not implemented)");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error printing report: {ex.Message}");
                MessageBox.Show($"Error printing report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReportsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Close database connection
                if (_connection != null && _connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                    _connection.Dispose();
                    _logger.Info("Database connection closed for reports");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error closing database connection: {ex.Message}");
            }
        }

        // Controls
        private System.Windows.Forms.DateTimePicker dtpStartDate;
        private System.Windows.Forms.DateTimePicker dtpEndDate;
        private System.Windows.Forms.Label lblStartDate;
        private System.Windows.Forms.Label lblEndDate;
        private System.Windows.Forms.Button btnGenerateReport;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.Button btnExportCsv;
        private System.Windows.Forms.Button btnExportExcel;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Label lblTotalVehicles;
        private System.Windows.Forms.ComboBox cmbReportType;
        private System.Windows.Forms.Label lblReportType;
        private System.Windows.Forms.Label lblFilterByPlate;
        private System.Windows.Forms.TextBox txtFilterByPlate;
    }
} 