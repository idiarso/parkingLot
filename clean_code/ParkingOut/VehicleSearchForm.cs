using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin.Forms
{
    public partial class VehicleSearchForm : Form
    {
        private TextBox txtPlateNumber;
        private DateTimePicker dtpStartDate;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbVehicleType;
        private ComboBox cmbStatus;
        private Button btnSearch;
        private Button btnClear;
        private Button btnExport;
        private Button btnClose;
        private DataGridView dgvResults;
        private Label lblTotalResults;
        private readonly IAppLogger _logger;
        
        public VehicleSearchForm()
        {
            _logger = new FileLogger();
            InitializeComponent();
            LoadVehicleTypes();
            InitStatusFilter();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Pencarian Kendaraan";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            Panel pnlFilter = new Panel();
            pnlFilter.BorderStyle = BorderStyle.FixedSingle;
            pnlFilter.Location = new Point(12, 12);
            pnlFilter.Size = new Size(970, 100);
            
            Label lblPlateNumber = new Label();
            lblPlateNumber.Text = "Nomor Polisi:";
            lblPlateNumber.AutoSize = true;
            lblPlateNumber.Location = new Point(10, 15);
            
            this.txtPlateNumber = new TextBox();
            this.txtPlateNumber.Location = new Point(100, 12);
            this.txtPlateNumber.Size = new Size(150, 23);
            this.txtPlateNumber.CharacterCasing = CharacterCasing.Upper;
            
            Label lblVehicleType = new Label();
            lblVehicleType.Text = "Jenis:";
            lblVehicleType.AutoSize = true;
            lblVehicleType.Location = new Point(270, 15);
            
            this.cmbVehicleType = new ComboBox();
            this.cmbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbVehicleType.Location = new Point(320, 12);
            this.cmbVehicleType.Size = new Size(150, 23);
            
            Label lblStatus = new Label();
            lblStatus.Text = "Status:";
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(490, 15);
            
            this.cmbStatus = new ComboBox();
            this.cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbStatus.Location = new Point(540, 12);
            this.cmbStatus.Size = new Size(150, 23);
            
            Label lblDateRange = new Label();
            lblDateRange.Text = "Rentang Waktu:";
            lblDateRange.AutoSize = true;
            lblDateRange.Location = new Point(10, 50);
            
            this.dtpStartDate = new DateTimePicker();
            this.dtpStartDate.Format = DateTimePickerFormat.Custom;
            this.dtpStartDate.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpStartDate.Location = new Point(100, 47);
            this.dtpStartDate.Size = new Size(150, 23);
            this.dtpStartDate.Value = DateTime.Today;
            
            Label lblTo = new Label();
            lblTo.Text = "s/d";
            lblTo.AutoSize = true;
            lblTo.Location = new Point(260, 50);
            
            this.dtpEndDate = new DateTimePicker();
            this.dtpEndDate.Format = DateTimePickerFormat.Custom;
            this.dtpEndDate.CustomFormat = "dd/MM/yyyy HH:mm";
            this.dtpEndDate.Location = new Point(280, 47);
            this.dtpEndDate.Size = new Size(150, 23);
            this.dtpEndDate.Value = DateTime.Now;
            
            this.btnSearch = new Button();
            this.btnSearch.Text = "Cari";
            this.btnSearch.Location = new Point(710, 12);
            this.btnSearch.Size = new Size(80, 30);
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);
            
            this.btnClear = new Button();
            this.btnClear.Text = "Reset";
            this.btnClear.Location = new Point(800, 12);
            this.btnClear.Size = new Size(80, 30);
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            
            this.btnExport = new Button();
            this.btnExport.Text = "Export";
            this.btnExport.Location = new Point(890, 12);
            this.btnExport.Size = new Size(70, 30);
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new EventHandler(this.btnExport_Click);
            
            pnlFilter.Controls.Add(lblPlateNumber);
            pnlFilter.Controls.Add(this.txtPlateNumber);
            pnlFilter.Controls.Add(lblVehicleType);
            pnlFilter.Controls.Add(this.cmbVehicleType);
            pnlFilter.Controls.Add(lblStatus);
            pnlFilter.Controls.Add(this.cmbStatus);
            pnlFilter.Controls.Add(lblDateRange);
            pnlFilter.Controls.Add(this.dtpStartDate);
            pnlFilter.Controls.Add(lblTo);
            pnlFilter.Controls.Add(this.dtpEndDate);
            pnlFilter.Controls.Add(this.btnSearch);
            pnlFilter.Controls.Add(this.btnClear);
            pnlFilter.Controls.Add(this.btnExport);
            
            this.dgvResults = new DataGridView();
            this.dgvResults.Location = new Point(12, 120);
            this.dgvResults.Size = new Size(970, 400);
            this.dgvResults.AllowUserToAddRows = false;
            this.dgvResults.ReadOnly = true;
            this.dgvResults.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvResults.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvResults.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvResults_CellDoubleClick);
            
            this.lblTotalResults = new Label();
            this.lblTotalResults.AutoSize = true;
            this.lblTotalResults.Location = new Point(12, 530);
            this.lblTotalResults.Text = "Total Data: 0";
            
            this.btnClose = new Button();
            this.btnClose.Text = "Tutup";
            this.btnClose.Location = new Point(900, 530);
            this.btnClose.Size = new Size(80, 30);
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            
            this.Controls.Add(pnlFilter);
            this.Controls.Add(this.dgvResults);
            this.Controls.Add(this.lblTotalResults);
            this.Controls.Add(this.btnClose);
            
            this.Load += new EventHandler(this.VehicleSearchForm_Load);
        }
        
        private void LoadVehicleTypes()
        {
            try
            {
                string query = "SELECT DISTINCT jenis_kendaraan FROM t_parkir ORDER BY jenis_kendaraan";
                DataTable dt = Database.ExecuteQuery(query);
                
                cmbVehicleType.Items.Clear();
                cmbVehicleType.Items.Add("Semua");
                
                foreach (DataRow row in dt.Rows)
                {
                    cmbVehicleType.Items.Add(row["jenis_kendaraan"].ToString());
                }
                
                cmbVehicleType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat memuat jenis kendaraan: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                cmbVehicleType.Items.Clear();
                cmbVehicleType.Items.Add("Semua");
                cmbVehicleType.SelectedIndex = 0;
            }
        }
        
        private void InitStatusFilter()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Semua");
            cmbStatus.Items.Add("Aktif (Belum Keluar)");
            cmbStatus.Items.Add("Selesai (Sudah Keluar)");
            cmbStatus.SelectedIndex = 0;
        }
        
        private void VehicleSearchForm_Load(object sender, EventArgs e)
        {
            // Set default date range to today
            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Now;
            
            // Initially search with default parameters
            SearchVehicles();
        }
        
        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchVehicles();
        }
        
        private void SearchVehicles()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                // Build the query based on filters
                string whereClause = "";
                List<string> conditions = new List<string>();
                
                // Filter by plate number
                if (!string.IsNullOrEmpty(txtPlateNumber.Text.Trim()))
                {
                    conditions.Add($"nomor_polisi LIKE '%{txtPlateNumber.Text.Trim()}%'");
                }
                
                // Filter by vehicle type
                if (cmbVehicleType.SelectedIndex > 0) // Not "Semua"
                {
                    conditions.Add($"jenis_kendaraan = '{cmbVehicleType.SelectedItem}'");
                }
                
                // Filter by status
                if (cmbStatus.SelectedIndex == 1) // Aktif
                {
                    conditions.Add("waktu_keluar IS NULL");
                }
                else if (cmbStatus.SelectedIndex == 2) // Selesai
                {
                    conditions.Add("waktu_keluar IS NOT NULL");
                }
                
                // Filter by date range
                DateTime startDate = dtpStartDate.Value;
                DateTime endDate = dtpEndDate.Value;
                
                conditions.Add($"waktu_masuk BETWEEN '{startDate:yyyy-MM-dd HH:mm:ss}' AND '{endDate:yyyy-MM-dd HH:mm:ss}'");
                
                // Combine all conditions
                if (conditions.Count > 0)
                {
                    whereClause = "WHERE " + string.Join(" AND ", conditions);
                }
                
                // Final query
                string query = $@"
                    SELECT 
                        id,
                        nomor_polisi,
                        jenis_kendaraan,
                        waktu_masuk,
                        waktu_keluar,
                        CASE WHEN waktu_keluar IS NULL THEN 'Aktif' ELSE 'Selesai' END AS status,
                        CASE WHEN waktu_keluar IS NOT NULL THEN
                            TIMESTAMPDIFF(MINUTE, waktu_masuk, waktu_keluar)
                        ELSE
                            TIMESTAMPDIFF(MINUTE, waktu_masuk, NOW())
                        END AS durasi_menit,
                        slot_id,
                        biaya
                    FROM t_parkir
                    {whereClause}
                    ORDER BY waktu_masuk DESC";
                
                DataTable results = Database.ExecuteQuery(query);
                
                // Display results
                dgvResults.DataSource = results;
                
                // Format columns
                FormatResultsGrid();
                
                // Update total count
                lblTotalResults.Text = $"Total Data: {results.Rows.Count}";
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to search vehicles", ex);
                MessageBox.Show("Failed to search vehicles. Please check the logs for details.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        
        private void FormatResultsGrid()
        {
            if (dgvResults.Columns.Count > 0)
            {
                // Set column headers
                dgvResults.Columns["id"].HeaderText = "ID";
                dgvResults.Columns["nomor_polisi"].HeaderText = "Nomor Polisi";
                dgvResults.Columns["jenis_kendaraan"].HeaderText = "Jenis Kendaraan";
                dgvResults.Columns["waktu_masuk"].HeaderText = "Waktu Masuk";
                dgvResults.Columns["waktu_keluar"].HeaderText = "Waktu Keluar";
                dgvResults.Columns["status"].HeaderText = "Status";
                dgvResults.Columns["durasi_menit"].HeaderText = "Durasi (menit)";
                dgvResults.Columns["slot_id"].HeaderText = "Slot ID";
                dgvResults.Columns["biaya"].HeaderText = "Biaya (Rp)";
                
                // Format values
                foreach (DataGridViewRow row in dgvResults.Rows)
                {
                    // Format status cell color
                    if (row.Cells["status"].Value.ToString() == "Aktif")
                    {
                        row.Cells["status"].Style.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        row.Cells["status"].Style.BackColor = Color.LightSalmon;
                    }
                    
                    // Format duration as hours and minutes
                    if (int.TryParse(row.Cells["durasi_menit"].Value.ToString(), out int minutes))
                    {
                        int hours = minutes / 60;
                        int remainingMinutes = minutes % 60;
                        row.Cells["durasi_menit"].Value = $"{hours} jam {remainingMinutes} menit";
                    }
                    
                    // Format currency
                    if (row.Cells["biaya"].Value != DBNull.Value && decimal.TryParse(row.Cells["biaya"].Value.ToString(), out decimal fee))
                    {
                        row.Cells["biaya"].Value = string.Format("{0:N0}", fee);
                    }
                }
            }
        }
        
        private void btnClear_Click(object sender, EventArgs e)
        {
            // Reset all filters
            txtPlateNumber.Clear();
            cmbVehicleType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            dtpStartDate.Value = DateTime.Today;
            dtpEndDate.Value = DateTime.Now;
            
            // Search again with reset filters
            SearchVehicles();
        }
        
        private void dgvResults_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                // Get selected vehicle data
                int vehicleId = Convert.ToInt32(dgvResults.Rows[e.RowIndex].Cells["id"].Value);
                
                // Show vehicle details form
                ShowVehicleDetails(vehicleId);
            }
        }
        
        private void ShowVehicleDetails(int vehicleId)
        {
            try
            {
                string query = $"SELECT * FROM t_parkir WHERE id = {vehicleId}";
                DataTable vehicleData = Database.ExecuteQuery(query);
                
                if (vehicleData.Rows.Count > 0)
                {
                    DataRow row = vehicleData.Rows[0];
                    
                    // Create a details message
                    string details = "===== DETAIL KENDARAAN =====\n\n";
                    details += $"ID: {row["id"]}\n";
                    details += $"Nomor Polisi: {row["nomor_polisi"]}\n";
                    details += $"Jenis Kendaraan: {row["jenis_kendaraan"]}\n";
                    details += $"Waktu Masuk: {Convert.ToDateTime(row["waktu_masuk"]).ToString("dd/MM/yyyy HH:mm:ss")}\n";
                    
                    if (row["waktu_keluar"] != DBNull.Value)
                    {
                        details += $"Waktu Keluar: {Convert.ToDateTime(row["waktu_keluar"]).ToString("dd/MM/yyyy HH:mm:ss")}\n";
                        
                        // Calculate duration
                        DateTime entryTime = Convert.ToDateTime(row["waktu_masuk"]);
                        DateTime exitTime = Convert.ToDateTime(row["waktu_keluar"]);
                        TimeSpan duration = exitTime - entryTime;
                        
                        details += $"Durasi: {(int)duration.TotalHours} jam {duration.Minutes} menit\n";
                        
                        // Show fee
                        if (row["biaya"] != DBNull.Value)
                        {
                            decimal fee = Convert.ToDecimal(row["biaya"]);
                            details += $"Biaya: Rp{fee:N0}\n";
                        }
                    }
                    else
                    {
                        details += "Waktu Keluar: -\n";
                        details += "Status: Masih Parkir (Aktif)\n";
                        
                        // Calculate current duration
                        DateTime entryTime = Convert.ToDateTime(row["waktu_masuk"]);
                        TimeSpan duration = DateTime.Now - entryTime;
                        
                        details += $"Durasi Saat Ini: {(int)duration.TotalHours} jam {duration.Minutes} menit\n";
                    }
                    
                    // Show slot ID if available
                    if (row["slot_id"] != DBNull.Value && !string.IsNullOrEmpty(row["slot_id"].ToString()))
                    {
                        details += $"Slot ID: {row["slot_id"]}\n";
                    }
                    
                    // Check if there's a photo
                    if (row["foto_masuk"] != DBNull.Value && !string.IsNullOrEmpty(row["foto_masuk"].ToString()))
                    {
                        string photoPath = row["foto_masuk"].ToString();
                        
                        if (File.Exists(photoPath))
                        {
                            // We could show this in a more sophisticated details form with an image viewer
                            details += $"Foto Kendaraan: Tersedia\n";
                        }
                    }
                    
                    // Show details
                    MessageBox.Show(details, "Detail Kendaraan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Data kendaraan tidak ditemukan.", 
                        "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menampilkan detail kendaraan: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvResults.Rows.Count == 0)
                {
                    MessageBox.Show("Tidak ada data untuk diekspor.", 
                        "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                
                // Create save file dialog
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV Files (*.csv)|*.csv|Excel Files (*.xlsx)|*.xlsx";
                saveDialog.Title = "Export Data";
                saveDialog.FileName = $"ParkingReport_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}";
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Get the file extension
                    string extension = Path.GetExtension(saveDialog.FileName).ToLower();
                    
                    if (extension == ".csv")
                    {
                        ExportToCsv(saveDialog.FileName);
                    }
                    else if (extension == ".xlsx")
                    {
                        ExportToExcel(saveDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat mengekspor data: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ExportToCsv(string filename)
        {
            try
            {
                // Create a string builder for the CSV content
                StringBuilder sb = new StringBuilder();
                
                // Add headers
                for (int i = 0; i < dgvResults.Columns.Count; i++)
                {
                    sb.Append(dgvResults.Columns[i].HeaderText);
                    if (i < dgvResults.Columns.Count - 1)
                        sb.Append(",");
                }
                sb.AppendLine();
                
                // Add rows
                foreach (DataGridViewRow row in dgvResults.Rows)
                {
                    for (int i = 0; i < dgvResults.Columns.Count; i++)
                    {
                        // Add quotes around field and escape any quotes within
                        string field = row.Cells[i].Value?.ToString() ?? "";
                        field = field.Replace("\"", "\"\""); // Escape quotes
                        sb.Append($"\"{field}\"");
                        
                        if (i < dgvResults.Columns.Count - 1)
                            sb.Append(",");
                    }
                    sb.AppendLine();
                }
                
                // Write to file
                File.WriteAllText(filename, sb.ToString());
                
                MessageBox.Show($"Data berhasil diekspor ke {filename}", 
                    "Ekspor Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saat membuat file CSV: {ex.Message}");
            }
        }
        
        private void ExportToExcel(string filename)
        {
            try
            {
                // For Excel export, we can use a library like EPPlus or Microsoft.Office.Interop.Excel
                // But for simplicity, we'll just create a CSV that Excel can open
                ExportToCsv(filename.Replace(".xlsx", ".csv"));
                
                // Note: For a more sophisticated Excel export with formatting, 
                // you would need to add a reference to an Excel library
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saat membuat file Excel: {ex.Message}");
            }
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
} 