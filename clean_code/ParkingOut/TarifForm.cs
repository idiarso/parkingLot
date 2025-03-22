using System;
using System.Data;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class TarifForm : Form
    {
        private bool isOldSchema = true;
        private TabControl tabTarif;
        private TabPage tabRegular;
        private TabPage tabSpecial;
        private DataGridView dgvSpecialRates;
        private Button btnAddSpecial;
        private Button btnEditSpecial;
        private Button btnDeleteSpecial;

        public TarifForm()
        {
            InitializeComponent();
        }

        private void TarifForm_Load(object sender, EventArgs e)
        {
            // Ensure tarif_khusus table exists
            EnsureTarifKhususTableExists();
            
            // Load data
            LoadTarif();
            
            // Setup tab control
            SetupTabControl();
            
            // Load special rates after tab control is set up
            LoadSpecialRates();
        }

        private void EnsureTarifKhususTableExists()
        {
            try 
            {
                // Check if table exists first
                if (!Database.TableExists("tarif_khusus"))
                {
                    // Create tarif_khusus table if it doesn't exist
                    string createTable = @"
                        CREATE TABLE IF NOT EXISTS `tarif_khusus` (
                          `id` int(11) NOT NULL AUTO_INCREMENT,
                          `jenis_kendaraan` varchar(50) NOT NULL,
                          `jenis_tarif` varchar(50) NOT NULL,
                          `jam_mulai` time DEFAULT NULL,
                          `jam_selesai` time DEFAULT NULL,
                          `hari` varchar(100) DEFAULT NULL,
                          `tarif_flat` decimal(10,2) DEFAULT NULL,
                          `deskripsi` varchar(255) DEFAULT NULL,
                          `status` tinyint(1) DEFAULT 1,
                          `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
                          `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                          PRIMARY KEY (`id`)
                        ) ENGINE=InnoDB DEFAULT CHARSET=utf8;";
                    
                    Database.ExecuteNonQuery(createTable);
                    MessageBox.Show("Tabel tarif_khusus berhasil dibuat.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal membuat tabel tarif_khusus: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Logger.Error(ex.Message);
            }
        }

        private void SetupTabControl()
        {
            // Create TabControl
            this.tabTarif = new TabControl();
            this.tabTarif.Dock = DockStyle.Fill;
            this.tabTarif.Location = new Point(12, 12);
            this.tabTarif.Size = new Size(760, 400);
            this.tabTarif.TabIndex = 0;

            // Create Regular Tab
            this.tabRegular = new TabPage();
            this.tabRegular.Text = "Tarif Reguler";
            this.tabRegular.Padding = new Padding(3);

            // Create Special Tab
            this.tabSpecial = new TabPage();
            this.tabSpecial.Text = "Tarif Khusus";
            this.tabSpecial.Padding = new Padding(3);

            // Add TabPages to TabControl
            this.tabTarif.Controls.Add(this.tabRegular);
            this.tabTarif.Controls.Add(this.tabSpecial);

            // Move dgvTarif to tabRegular
            this.dgvTarif.Dock = DockStyle.Fill;
            this.dgvTarif.Location = new Point(3, 3);
            this.dgvTarif.Size = new Size(752, 368);
            this.tabRegular.Controls.Add(this.dgvTarif);

            // Create DataGridView for Special Rates
            this.dgvSpecialRates = new DataGridView();
            this.dgvSpecialRates.AllowUserToAddRows = false;
            this.dgvSpecialRates.AllowUserToDeleteRows = false;
            this.dgvSpecialRates.Dock = DockStyle.Fill;
            this.dgvSpecialRates.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvSpecialRates.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSpecialRates.Location = new Point(3, 3);
            this.dgvSpecialRates.MultiSelect = false;
            this.dgvSpecialRates.Name = "dgvSpecialRates";
            this.dgvSpecialRates.ReadOnly = true;
            this.dgvSpecialRates.RowTemplate.Height = 25;
            this.dgvSpecialRates.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvSpecialRates.Size = new Size(752, 368);
            this.dgvSpecialRates.TabIndex = 0;
            this.tabSpecial.Controls.Add(this.dgvSpecialRates);

            // Create buttons for Special Tab
            this.btnAddSpecial = new Button();
            this.btnAddSpecial.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnAddSpecial.Location = new Point(12, 426);
            this.btnAddSpecial.Name = "btnAddSpecial";
            this.btnAddSpecial.Size = new Size(100, 30);
            this.btnAddSpecial.TabIndex = 6;
            this.btnAddSpecial.Text = "Tambah Khusus";
            this.btnAddSpecial.UseVisualStyleBackColor = true;
            this.btnAddSpecial.Click += new EventHandler(this.btnAddSpecial_Click);
            this.btnAddSpecial.Visible = false;

            this.btnEditSpecial = new Button();
            this.btnEditSpecial.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnEditSpecial.Location = new Point(118, 426);
            this.btnEditSpecial.Name = "btnEditSpecial";
            this.btnEditSpecial.Size = new Size(100, 30);
            this.btnEditSpecial.TabIndex = 7;
            this.btnEditSpecial.Text = "Edit Khusus";
            this.btnEditSpecial.UseVisualStyleBackColor = true;
            this.btnEditSpecial.Click += new EventHandler(this.btnEditSpecial_Click);
            this.btnEditSpecial.Visible = false;

            this.btnDeleteSpecial = new Button();
            this.btnDeleteSpecial.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnDeleteSpecial.Location = new Point(224, 426);
            this.btnDeleteSpecial.Name = "btnDeleteSpecial";
            this.btnDeleteSpecial.Size = new Size(100, 30);
            this.btnDeleteSpecial.TabIndex = 8;
            this.btnDeleteSpecial.Text = "Hapus Khusus";
            this.btnDeleteSpecial.UseVisualStyleBackColor = true;
            this.btnDeleteSpecial.Click += new EventHandler(this.btnDeleteSpecial_Click);
            this.btnDeleteSpecial.Visible = false;

            // Add TabControl to the form
            this.Controls.Add(this.tabTarif);
            this.Controls.Add(this.btnAddSpecial);
            this.Controls.Add(this.btnEditSpecial);
            this.Controls.Add(this.btnDeleteSpecial);

            // Add event handler for tab change
            this.tabTarif.SelectedIndexChanged += new EventHandler(this.tabTarif_SelectedIndexChanged);
        }

        private void tabTarif_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Show/hide buttons based on selected tab
            bool isRegularTab = tabTarif.SelectedTab == tabRegular;
            
            btnAdd.Visible = isRegularTab;
            btnEdit.Visible = isRegularTab;
            btnDelete.Visible = isRegularTab;
            
            btnAddSpecial.Visible = !isRegularTab;
            btnEditSpecial.Visible = !isRegularTab;
            btnDeleteSpecial.Visible = !isRegularTab;

            // Refresh data for selected tab
            if (isRegularTab)
            {
                LoadTarif();
            }
            else
            {
                LoadSpecialRates();
            }
        }

        private void LoadTarif()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                // Check which schema we're using
                DataTable tarif;
                
                try
                {
                    // Try old schema first (t_tarif)
                    string query = "SELECT id, jenis_kendaraan, tarif_perjam, tarif_berikutnya, denda_tiket_hilang FROM t_tarif";
                    tarif = Database.ExecuteQuery(query);
                    isOldSchema = true;
                }
                catch (Exception)
                {
                    try
                    {
                        // Try old schema without additional fields
                        string query = "SELECT id, jenis_kendaraan, tarif_perjam FROM t_tarif";
                        tarif = Database.ExecuteQuery(query);
                        isOldSchema = true;
                    }
                    catch (Exception)
                    {
                        // Try new schema (tariff)
                        string query = "SELECT id, vehicle_type as jenis_kendaraan, hourly_rate as tarif_perjam, active as status FROM tariff";
                        tarif = Database.ExecuteQuery(query);
                        isOldSchema = false;
                    }
                }
                
                dgvTarif.DataSource = tarif;
                
                // Format columns
                if (dgvTarif.Columns.Count > 0)
                {
                    if (dgvTarif.Columns.Contains("id"))
                        dgvTarif.Columns["id"].HeaderText = "ID";
                        
                    if (dgvTarif.Columns.Contains("jenis_kendaraan"))
                        dgvTarif.Columns["jenis_kendaraan"].HeaderText = "Jenis Kendaraan";
                        
                    if (dgvTarif.Columns.Contains("tarif_perjam"))
                    {
                        dgvTarif.Columns["tarif_perjam"].HeaderText = "Tarif Per Jam";
                        dgvTarif.Columns["tarif_perjam"].DefaultCellStyle.Format = "Rp#,##0";
                    }
                    
                    if (dgvTarif.Columns.Contains("tarif_berikutnya"))
                    {
                        dgvTarif.Columns["tarif_berikutnya"].HeaderText = "Tarif Berikutnya";
                        dgvTarif.Columns["tarif_berikutnya"].DefaultCellStyle.Format = "Rp#,##0";
                    }
                    
                    if (dgvTarif.Columns.Contains("denda_tiket_hilang"))
                    {
                        dgvTarif.Columns["denda_tiket_hilang"].HeaderText = "Denda Tiket Hilang";
                        dgvTarif.Columns["denda_tiket_hilang"].DefaultCellStyle.Format = "Rp#,##0";
                    }
                    
                    if (dgvTarif.Columns.Contains("status"))
                        dgvTarif.Columns["status"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat memuat data tarif: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (TarifEntryForm entryForm = new TarifEntryForm(isOldSchema))
            {
                if (entryForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTarif(); // Refresh data after adding
                }
            }
        }
        
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvTarif.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silakan pilih tarif yang ingin diedit.", 
                    "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int tarifId = Convert.ToInt32(dgvTarif.SelectedRows[0].Cells["id"].Value);
            string jenisKendaraan = dgvTarif.SelectedRows[0].Cells["jenis_kendaraan"].Value.ToString();
            decimal tarifPerjam = Convert.ToDecimal(dgvTarif.SelectedRows[0].Cells["tarif_perjam"].Value);
            
            decimal tarifBerikutnya = 0;
            if (dgvTarif.Columns.Contains("tarif_berikutnya") && dgvTarif.SelectedRows[0].Cells["tarif_berikutnya"].Value != DBNull.Value)
                tarifBerikutnya = Convert.ToDecimal(dgvTarif.SelectedRows[0].Cells["tarif_berikutnya"].Value);
                
            decimal dendaTiketHilang = 0;
            if (dgvTarif.Columns.Contains("denda_tiket_hilang") && dgvTarif.SelectedRows[0].Cells["denda_tiket_hilang"].Value != DBNull.Value)
                dendaTiketHilang = Convert.ToDecimal(dgvTarif.SelectedRows[0].Cells["denda_tiket_hilang"].Value);
                
            using (TarifEntryForm entryForm = new TarifEntryForm(isOldSchema, tarifId, jenisKendaraan, tarifPerjam, tarifBerikutnya, dendaTiketHilang))
            {
                if (entryForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTarif(); // Refresh data after editing
                }
            }
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvTarif.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silakan pilih tarif yang ingin dihapus.", 
                    "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            int tarifId = Convert.ToInt32(dgvTarif.SelectedRows[0].Cells["id"].Value);
            string jenisKendaraan = dgvTarif.SelectedRows[0].Cells["jenis_kendaraan"].Value.ToString();
            
            DialogResult result = MessageBox.Show(
                $"Anda yakin ingin menghapus tarif untuk {jenisKendaraan}?", 
                "Konfirmasi Hapus", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);
                
            if (result == DialogResult.Yes)
            {
                try
                {
                    string query;
                    if (isOldSchema)
                    {
                        query = $"DELETE FROM t_tarif WHERE id = {tarifId}";
                    }
                    else
                    {
                        query = $"DELETE FROM tariff WHERE id = {tarifId}";
                    }
                    
                    Database.ExecuteNonQuery(query);
                    
                    MessageBox.Show("Tarif berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTarif(); // Refresh data after deleting
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saat menghapus tarif: {ex.Message}", 
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadTarif();
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Windows Form Designer generated code

        private DataGridView dgvTarif;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnClose;

        private void InitializeComponent()
        {
            this.dgvTarif = new DataGridView();
            this.btnAdd = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.btnRefresh = new Button();
            this.btnClose = new Button();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvTarif)).BeginInit();
            this.SuspendLayout();
            
            // dgvTarif
            this.dgvTarif.AllowUserToAddRows = false;
            this.dgvTarif.AllowUserToDeleteRows = false;
            this.dgvTarif.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                | AnchorStyles.Left) 
                | AnchorStyles.Right)));
            this.dgvTarif.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvTarif.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvTarif.Location = new Point(12, 12);
            this.dgvTarif.MultiSelect = false;
            this.dgvTarif.Name = "dgvTarif";
            this.dgvTarif.ReadOnly = true;
            this.dgvTarif.RowTemplate.Height = 25;
            this.dgvTarif.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvTarif.Size = new Size(760, 400);
            this.dgvTarif.TabIndex = 0;
            
            // btnAdd
            this.btnAdd.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnAdd.Location = new Point(12, 426);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new Size(100, 30);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "Tambah";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new EventHandler(this.btnAdd_Click);
            
            // btnEdit
            this.btnEdit.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnEdit.Location = new Point(118, 426);
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.Size = new Size(100, 30);
            this.btnEdit.TabIndex = 2;
            this.btnEdit.Text = "Edit";
            this.btnEdit.UseVisualStyleBackColor = true;
            this.btnEdit.Click += new EventHandler(this.btnEdit_Click);
            
            // btnDelete
            this.btnDelete.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnDelete.Location = new Point(224, 426);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(100, 30);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Hapus";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);
            
            // btnRefresh
            this.btnRefresh.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.btnRefresh.Location = new Point(330, 426);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new Size(100, 30);
            this.btnRefresh.TabIndex = 4;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
            
            // btnClose
            this.btnClose.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
            this.btnClose.Location = new Point(672, 426);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new Size(100, 30);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Tutup";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            
            // TarifForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 468);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dgvTarif);
            this.MinimumSize = new Size(700, 500);
            this.Name = "TarifForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Manajemen Tarif Parkir";
            this.Load += new EventHandler(this.TarifForm_Load);
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvTarif)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private void LoadSpecialRates()
        {
            try
            {
                // Use status = true for PostgreSQL boolean
                string query = "SELECT id, jenis_kendaraan, jam_mulai, jam_selesai, " +
                               "hari, tarif_flat, deskripsi FROM tarif_khusus WHERE status = true ORDER BY jenis_kendaraan, jam_mulai";
                DataTable dt = Database.ExecuteQuery(query);
                dgvSpecialRates.DataSource = dt;
                
                // Format columns for better display
                if (dgvSpecialRates.Columns.Contains("id"))
                    dgvSpecialRates.Columns["id"].Visible = false;
                    
                if (dgvSpecialRates.Columns.Contains("tarif_flat"))
                {
                    dgvSpecialRates.Columns["tarif_flat"].HeaderText = "Flat Rate";
                    dgvSpecialRates.Columns["tarif_flat"].DefaultCellStyle.Format = "N0";
                    dgvSpecialRates.Columns["tarif_flat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                
                // Format other columns as needed
                if (dgvSpecialRates.Columns.Contains("jenis_kendaraan"))
                    dgvSpecialRates.Columns["jenis_kendaraan"].HeaderText = "Vehicle Type";
                if (dgvSpecialRates.Columns.Contains("jam_mulai"))
                    dgvSpecialRates.Columns["jam_mulai"].HeaderText = "Start Time";
                if (dgvSpecialRates.Columns.Contains("jam_selesai"))
                    dgvSpecialRates.Columns["jam_selesai"].HeaderText = "End Time";
                if (dgvSpecialRates.Columns.Contains("hari"))
                    dgvSpecialRates.Columns["hari"].HeaderText = "Day";
                if (dgvSpecialRates.Columns.Contains("deskripsi"))
                    dgvSpecialRates.Columns["deskripsi"].HeaderText = "Description";
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                MessageBox.Show("Error loading special rates: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddSpecial_Click(object sender, EventArgs e)
        {
            using (SpecialRateForm form = new SpecialRateForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadSpecialRates();
                }
            }
        }

        private void btnEditSpecial_Click(object sender, EventArgs e)
        {
            if (dgvSpecialRates.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvSpecialRates.SelectedRows[0].Cells["id"].Value);
                
                using (SpecialRateForm form = new SpecialRateForm(id))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadSpecialRates();
                    }
                }
            }
            else
            {
                MessageBox.Show("Silakan pilih tarif khusus yang akan diedit.", 
                    "Edit Tarif", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnDeleteSpecial_Click(object sender, EventArgs e)
        {
            if (dgvSpecialRates.SelectedRows.Count > 0)
            {
                int id = Convert.ToInt32(dgvSpecialRates.SelectedRows[0].Cells["id"].Value);
                string deskripsi = dgvSpecialRates.SelectedRows[0].Cells["deskripsi"].Value.ToString();
                
                DialogResult result = MessageBox.Show($"Anda yakin ingin menghapus tarif khusus '{deskripsi}'?", 
                    "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        string query = $"DELETE FROM tarif_khusus WHERE id = {id}";
                        Database.ExecuteNonQuery(query);
                        
                        MessageBox.Show("Tarif khusus berhasil dihapus.", 
                            "Hapus Tarif", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                        LoadSpecialRates();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saat menghapus tarif khusus: {ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Silakan pilih tarif khusus yang akan dihapus.", 
                    "Hapus Tarif", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
    
    // Form untuk menambah/edit tarif
    public class TarifEntryForm : Form
    {
        private bool isOldSchema;
        private int tarifId = 0;
        private bool isEdit = false;
        
        private TextBox txtJenisKendaraan;
        private TextBox txtTarifAwal;
        private TextBox txtTarifBerikutnya;
        private TextBox txtDendaTiketHilang;
        private Button btnSave;
        private Button btnCancel;
        private Label lblJenisKendaraan;
        private Label lblTarifAwal;
        private Label lblTarifBerikutnya;
        private Label lblDendaTiketHilang;
        private Label lblTitle;
        
        public TarifEntryForm(bool isOldSchema, int tarifId = 0, string jenisKendaraan = "", 
                            decimal tarifPerjam = 0, decimal tarifBerikutnya = 0, 
                            decimal dendaTiketHilang = 0)
        {
            this.isOldSchema = isOldSchema;
            this.tarifId = tarifId;
            this.isEdit = (tarifId > 0);
            
            InitializeComponent();
            
            if (this.isEdit)
            {
                lblTitle.Text = "Edit Tarif Parkir";
                txtJenisKendaraan.Text = jenisKendaraan;
                txtTarifAwal.Text = tarifPerjam.ToString("N0");
                txtTarifBerikutnya.Text = tarifBerikutnya.ToString("N0");
                txtDendaTiketHilang.Text = dendaTiketHilang.ToString("N0");
            }
        }
        
        private void InitializeComponent()
        {
            this.txtJenisKendaraan = new TextBox();
            this.txtTarifAwal = new TextBox();
            this.txtTarifBerikutnya = new TextBox();
            this.txtDendaTiketHilang = new TextBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.lblJenisKendaraan = new Label();
            this.lblTarifAwal = new Label();
            this.lblTarifBerikutnya = new Label();
            this.lblDendaTiketHilang = new Label();
            this.lblTitle = new Label();
            
            this.SuspendLayout();
            
            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(200, 25);
            this.lblTitle.Text = "Tambah Tarif Parkir";
            
            // lblJenisKendaraan
            this.lblJenisKendaraan.AutoSize = true;
            this.lblJenisKendaraan.Location = new Point(12, 50);
            this.lblJenisKendaraan.Name = "lblJenisKendaraan";
            this.lblJenisKendaraan.Size = new Size(96, 15);
            this.lblJenisKendaraan.Text = "Jenis Kendaraan:";
            
            // txtJenisKendaraan
            this.txtJenisKendaraan.Location = new Point(150, 47);
            this.txtJenisKendaraan.Name = "txtJenisKendaraan";
            this.txtJenisKendaraan.Size = new Size(222, 23);
            this.txtJenisKendaraan.TabIndex = 0;
            
            // lblTarifAwal
            this.lblTarifAwal.AutoSize = true;
            this.lblTarifAwal.Location = new Point(12, 79);
            this.lblTarifAwal.Name = "lblTarifAwal";
            this.lblTarifAwal.Size = new Size(75, 15);
            this.lblTarifAwal.Text = "Tarif Per Jam:";
            
            // txtTarifAwal
            this.txtTarifAwal.Location = new Point(150, 76);
            this.txtTarifAwal.Name = "txtTarifAwal";
            this.txtTarifAwal.Size = new Size(222, 23);
            this.txtTarifAwal.TabIndex = 1;
            this.txtTarifAwal.TextAlign = HorizontalAlignment.Right;
            this.txtTarifAwal.Leave += new EventHandler(this.txtTarifAwal_Leave);
            
            // lblTarifBerikutnya
            this.lblTarifBerikutnya.AutoSize = true;
            this.lblTarifBerikutnya.Location = new Point(12, 108);
            this.lblTarifBerikutnya.Name = "lblTarifBerikutnya";
            this.lblTarifBerikutnya.Size = new Size(93, 15);
            this.lblTarifBerikutnya.Text = "Tarif Berikutnya:";
            
            // txtTarifBerikutnya
            this.txtTarifBerikutnya.Location = new Point(150, 105);
            this.txtTarifBerikutnya.Name = "txtTarifBerikutnya";
            this.txtTarifBerikutnya.Size = new Size(222, 23);
            this.txtTarifBerikutnya.TabIndex = 2;
            this.txtTarifBerikutnya.TextAlign = HorizontalAlignment.Right;
            this.txtTarifBerikutnya.Leave += new EventHandler(this.txtTarifBerikutnya_Leave);
            
            // lblDendaTiketHilang
            this.lblDendaTiketHilang.AutoSize = true;
            this.lblDendaTiketHilang.Location = new Point(12, 137);
            this.lblDendaTiketHilang.Name = "lblDendaTiketHilang";
            this.lblDendaTiketHilang.Size = new Size(115, 15);
            this.lblDendaTiketHilang.Text = "Denda Tiket Hilang:";
            
            // txtDendaTiketHilang
            this.txtDendaTiketHilang.Location = new Point(150, 134);
            this.txtDendaTiketHilang.Name = "txtDendaTiketHilang";
            this.txtDendaTiketHilang.Size = new Size(222, 23);
            this.txtDendaTiketHilang.TabIndex = 3;
            this.txtDendaTiketHilang.TextAlign = HorizontalAlignment.Right;
            this.txtDendaTiketHilang.Leave += new EventHandler(this.txtDendaTiketHilang_Leave);
            
            // btnSave
            this.btnSave.Location = new Point(150, 172);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(100, 30);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Simpan";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            
            // btnCancel
            this.btnCancel.Location = new Point(272, 172);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new Size(100, 30);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Batal";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            // TarifEntryForm
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(384, 221);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.txtDendaTiketHilang);
            this.Controls.Add(this.lblDendaTiketHilang);
            this.Controls.Add(this.txtTarifBerikutnya);
            this.Controls.Add(this.lblTarifBerikutnya);
            this.Controls.Add(this.txtTarifAwal);
            this.Controls.Add(this.lblTarifAwal);
            this.Controls.Add(this.txtJenisKendaraan);
            this.Controls.Add(this.lblJenisKendaraan);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TarifEntryForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Tarif Parkir";
            
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        
        private void txtTarifAwal_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTarifAwal.Text))
            {
                if (decimal.TryParse(txtTarifAwal.Text.Replace(".", "").Replace(",", ""), out decimal value))
                {
                    txtTarifAwal.Text = value.ToString("N0");
                }
            }
        }
        
        private void txtTarifBerikutnya_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtTarifBerikutnya.Text))
            {
                if (decimal.TryParse(txtTarifBerikutnya.Text.Replace(".", "").Replace(",", ""), out decimal value))
                {
                    txtTarifBerikutnya.Text = value.ToString("N0");
                }
            }
        }
        
        private void txtDendaTiketHilang_Leave(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtDendaTiketHilang.Text))
            {
                if (decimal.TryParse(txtDendaTiketHilang.Text.Replace(".", "").Replace(",", ""), out decimal value))
                {
                    txtDendaTiketHilang.Text = value.ToString("N0");
                }
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtJenisKendaraan.Text))
                {
                    MessageBox.Show("Jenis Kendaraan tidak boleh kosong.", "Validasi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtJenisKendaraan.Focus();
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtTarifAwal.Text))
                {
                    MessageBox.Show("Tarif Per Jam tidak boleh kosong.", "Validasi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTarifAwal.Focus();
                    return;
                }
                
                decimal tarifAwal = 0;
                decimal tarifBerikutnya = 0;
                decimal dendaTiketHilang = 0;
                
                // Parse values with proper handling of thousands separators
                if (!decimal.TryParse(txtTarifAwal.Text.Replace(".", "").Replace(",", ""), 
                    NumberStyles.Any, CultureInfo.InvariantCulture, out tarifAwal))
                {
                    MessageBox.Show("Format Tarif Per Jam tidak valid.", "Validasi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTarifAwal.Focus();
                    return;
                }
                
                if (!string.IsNullOrWhiteSpace(txtTarifBerikutnya.Text))
                {
                    if (!decimal.TryParse(txtTarifBerikutnya.Text.Replace(".", "").Replace(",", ""), 
                        NumberStyles.Any, CultureInfo.InvariantCulture, out tarifBerikutnya))
                    {
                        MessageBox.Show("Format Tarif Berikutnya tidak valid.", "Validasi", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtTarifBerikutnya.Focus();
                        return;
                    }
                }
                
                if (!string.IsNullOrWhiteSpace(txtDendaTiketHilang.Text))
                {
                    if (!decimal.TryParse(txtDendaTiketHilang.Text.Replace(".", "").Replace(",", ""), 
                        NumberStyles.Any, CultureInfo.InvariantCulture, out dendaTiketHilang))
                    {
                        MessageBox.Show("Format Denda Tiket Hilang tidak valid.", "Validasi", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtDendaTiketHilang.Focus();
                        return;
                    }
                }
                
                string query;
                // Prepare the query based on the schema and action (add or edit)
                if (isOldSchema)
                {
                    // Cek struktur tabel terlebih dahulu
                    try {
                        // Check if tarif_awal column exists in t_tarif
                        string checkColumnQuery = "SHOW COLUMNS FROM t_tarif LIKE 'tarif_awal'";
                        DataTable columnCheck = Database.ExecuteQuery(checkColumnQuery);
                        bool hasTarifAwalColumn = columnCheck != null && columnCheck.Rows.Count > 0;

                        if (isEdit)
                        {
                            // Update existing tariff
                            if (hasTarifAwalColumn) {
                                query = $@"UPDATE t_tarif 
                                        SET jenis_kendaraan = '{txtJenisKendaraan.Text}', 
                                            tarif_awal = {tarifAwal}, 
                                            tarif_berikutnya = {tarifBerikutnya}, 
                                            denda_tiket_hilang = {dendaTiketHilang}
                                        WHERE id = {tarifId}";
                            } else {
                                query = $@"UPDATE t_tarif 
                                        SET jenis_kendaraan = '{txtJenisKendaraan.Text}', 
                                            tarif_perjam = {tarifAwal}, 
                                            tarif_berikutnya = {tarifBerikutnya}, 
                                            denda_tiket_hilang = {dendaTiketHilang}
                                        WHERE id = {tarifId}";
                            }
                        }
                        else
                        {
                            // Insert new tariff
                            if (hasTarifAwalColumn) {
                                query = $@"INSERT INTO t_tarif (jenis_kendaraan, tarif_awal, tarif_berikutnya, denda_tiket_hilang) 
                                        VALUES ('{txtJenisKendaraan.Text}', {tarifAwal}, {tarifBerikutnya}, {dendaTiketHilang})";
                            } else {
                                query = $@"INSERT INTO t_tarif (jenis_kendaraan, tarif_perjam, tarif_berikutnya, denda_tiket_hilang) 
                                        VALUES ('{txtJenisKendaraan.Text}', {tarifAwal}, {tarifBerikutnya}, {dendaTiketHilang})";
                            }
                        }
                    } catch {
                        // Fallback if column check fails
                        if (isEdit)
                        {
                            // Try using tarif_perjam as fallback
                            query = $@"UPDATE t_tarif 
                                    SET jenis_kendaraan = '{txtJenisKendaraan.Text}', 
                                        tarif_perjam = {tarifAwal}, 
                                        tarif_berikutnya = {tarifBerikutnya}, 
                                        denda_tiket_hilang = {dendaTiketHilang}
                                    WHERE id = {tarifId}";
                        }
                        else
                        {
                            // Try using tarif_perjam as fallback
                            query = $@"INSERT INTO t_tarif (jenis_kendaraan, tarif_perjam, tarif_berikutnya, denda_tiket_hilang) 
                                    VALUES ('{txtJenisKendaraan.Text}', {tarifAwal}, {tarifBerikutnya}, {dendaTiketHilang})";
                        }
                    }
                }
                else
                {
                    if (isEdit)
                    {
                        // Update existing tariff (new schema)
                        query = $@"UPDATE tariff 
                                SET vehicle_type = '{txtJenisKendaraan.Text}', 
                                    hourly_rate = {tarifAwal}
                                WHERE id = {tarifId}";
                    }
                    else
                    {
                        // Insert new tariff (new schema)
                        query = $@"INSERT INTO tariff (vehicle_type, hourly_rate, active) 
                                VALUES ('{txtJenisKendaraan.Text}', {tarifAwal}, 1)";
                    }
                }
                
                // Execute the query
                Database.ExecuteNonQuery(query);
                
                // Show success message
                MessageBox.Show(isEdit ? "Tarif berhasil diperbarui." : "Tarif baru berhasil ditambahkan.", 
                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Close the form with OK result
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menyimpan data: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }

    // Kelas form untuk menambah/edit tarif khusus
    public class SpecialRateForm : Form
    {
        private int rateId = 0;
        private ComboBox cmbVehicleType;
        private ComboBox cmbRateType;
        private DateTimePicker dtpStartTime;
        private DateTimePicker dtpEndTime;
        private CheckedListBox clbDays;
        private TextBox txtFlatRate;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        
        public SpecialRateForm(int id = 0)
        {
            this.rateId = id;
            InitializeComponent();
            
            if (id > 0)
            {
                this.Text = "Edit Tarif Khusus";
                LoadRateData();
            }
            else
            {
                this.Text = "Tambah Tarif Khusus";
            }
        }
        
        private void InitializeComponent()
        {
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            Label lblVehicleType = new Label();
            lblVehicleType.Text = "Jenis Kendaraan:";
            lblVehicleType.AutoSize = true;
            lblVehicleType.Location = new Point(20, 20);
            
            this.cmbVehicleType = new ComboBox();
            this.cmbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbVehicleType.Location = new Point(20, 40);
            this.cmbVehicleType.Size = new Size(200, 25);
            
            Label lblRateType = new Label();
            lblRateType.Text = "Jenis Tarif:";
            lblRateType.AutoSize = true;
            lblRateType.Location = new Point(20, 80);
            
            this.cmbRateType = new ComboBox();
            this.cmbRateType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbRateType.Location = new Point(20, 100);
            this.cmbRateType.Size = new Size(200, 25);
            this.cmbRateType.Items.AddRange(new object[] { "Jam Sibuk", "Hari Libur", "Acara Khusus" });
            
            Label lblTimeRange = new Label();
            lblTimeRange.Text = "Rentang Waktu:";
            lblTimeRange.AutoSize = true;
            lblTimeRange.Location = new Point(20, 140);
            
            this.dtpStartTime = new DateTimePicker();
            this.dtpStartTime.Format = DateTimePickerFormat.Time;
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Location = new Point(20, 160);
            this.dtpStartTime.Size = new Size(100, 25);
            
            Label lblTo = new Label();
            lblTo.Text = "s/d";
            lblTo.AutoSize = true;
            lblTo.Location = new Point(130, 165);
            
            this.dtpEndTime = new DateTimePicker();
            this.dtpEndTime.Format = DateTimePickerFormat.Time;
            this.dtpEndTime.ShowUpDown = true;
            this.dtpEndTime.Location = new Point(160, 160);
            this.dtpEndTime.Size = new Size(100, 25);
            
            Label lblDays = new Label();
            lblDays.Text = "Hari Berlaku:";
            lblDays.AutoSize = true;
            lblDays.Location = new Point(20, 200);
            
            this.clbDays = new CheckedListBox();
            this.clbDays.Location = new Point(20, 220);
            this.clbDays.Size = new Size(150, 100);
            this.clbDays.Items.AddRange(new object[] { 
                "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu", "Minggu" 
            });
            
            Label lblFlatRate = new Label();
            lblFlatRate.Text = "Tarif Flat:";
            lblFlatRate.AutoSize = true;
            lblFlatRate.Location = new Point(240, 220);
            
            this.txtFlatRate = new TextBox();
            this.txtFlatRate.Location = new Point(240, 240);
            this.txtFlatRate.Size = new Size(150, 25);
            this.txtFlatRate.KeyPress += new KeyPressEventHandler(this.txtFlatRate_KeyPress);
            
            Label lblDescription = new Label();
            lblDescription.Text = "Deskripsi:";
            lblDescription.AutoSize = true;
            lblDescription.Location = new Point(20, 340);
            
            this.txtDescription = new TextBox();
            this.txtDescription.Location = new Point(20, 360);
            this.txtDescription.Size = new Size(400, 25);
            
            this.btnSave = new Button();
            this.btnSave.Text = "Simpan";
            this.btnSave.Location = new Point(240, 410);
            this.btnSave.Size = new Size(90, 30);
            this.btnSave.Click += new EventHandler(this.btnSave_Click);
            
            this.btnCancel = new Button();
            this.btnCancel.Text = "Batal";
            this.btnCancel.Location = new Point(340, 410);
            this.btnCancel.Size = new Size(90, 30);
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);
            
            this.Controls.Add(lblVehicleType);
            this.Controls.Add(this.cmbVehicleType);
            this.Controls.Add(lblRateType);
            this.Controls.Add(this.cmbRateType);
            this.Controls.Add(lblTimeRange);
            this.Controls.Add(this.dtpStartTime);
            this.Controls.Add(lblTo);
            this.Controls.Add(this.dtpEndTime);
            this.Controls.Add(lblDays);
            this.Controls.Add(this.clbDays);
            this.Controls.Add(lblFlatRate);
            this.Controls.Add(this.txtFlatRate);
            this.Controls.Add(lblDescription);
            this.Controls.Add(this.txtDescription);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            
            // Load jenis kendaraan
            LoadVehicleTypes();
        }
        
        private void LoadVehicleTypes()
        {
            try
            {
                // Query untuk mengambil jenis kendaraan dari tabel tarif reguler
                string query = "SELECT DISTINCT jenis_kendaraan FROM t_tarif ORDER BY jenis_kendaraan";
                DataTable vehicleTypes = Database.ExecuteQuery(query);
                
                cmbVehicleType.Items.Clear();
                
                foreach (DataRow row in vehicleTypes.Rows)
                {
                    cmbVehicleType.Items.Add(row["jenis_kendaraan"].ToString());
                }
                
                if (cmbVehicleType.Items.Count > 0)
                {
                    cmbVehicleType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat memuat jenis kendaraan: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadRateData()
        {
            try
            {
                string query = $"SELECT * FROM tarif_khusus WHERE id = {rateId}";
                DataTable rate = Database.ExecuteQuery(query);
                
                if (rate.Rows.Count > 0)
                {
                    DataRow row = rate.Rows[0];
                    
                    // Set data
                    cmbVehicleType.Text = row["jenis_kendaraan"].ToString();
                    cmbRateType.Text = row["jenis_tarif"].ToString();
                    dtpStartTime.Value = Convert.ToDateTime(row["jam_mulai"]);
                    dtpEndTime.Value = Convert.ToDateTime(row["jam_selesai"]);
                    txtFlatRate.Text = row["tarif_flat"].ToString();
                    txtDescription.Text = row["deskripsi"].ToString();
                    
                    // Set hari
                    string[] days = row["hari"].ToString().Split(',');
                    for (int i = 0; i < clbDays.Items.Count; i++)
                    {
                        if (days.Contains(clbDays.Items[i].ToString()))
                        {
                            clbDays.SetItemChecked(i, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat memuat data tarif khusus: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void txtFlatRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Hanya izinkan angka dan kontrol
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                SaveRate();
            }
        }
        
        private bool ValidateInput()
        {
            if (cmbVehicleType.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih jenis kendaraan.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (cmbRateType.SelectedIndex == -1)
            {
                MessageBox.Show("Silakan pilih jenis tarif.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (clbDays.CheckedItems.Count == 0)
            {
                MessageBox.Show("Silakan pilih minimal satu hari berlaku.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (string.IsNullOrEmpty(txtFlatRate.Text))
            {
                MessageBox.Show("Silakan masukkan tarif flat.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            if (string.IsNullOrEmpty(txtDescription.Text))
            {
                MessageBox.Show("Silakan masukkan deskripsi tarif khusus.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            
            return true;
        }
        
        private void SaveRate()
        {
            try
            {
                // Buat string hari
                List<string> selectedDays = new List<string>();
                foreach (var item in clbDays.CheckedItems)
                {
                    selectedDays.Add(item.ToString());
                }
                string days = string.Join(",", selectedDays);
                
                // Format waktu
                string startTime = dtpStartTime.Value.ToString("HH:mm:ss");
                string endTime = dtpEndTime.Value.ToString("HH:mm:ss");
                
                string query = "";
                
                if (rateId > 0)
                {
                    // Update tarif yang sudah ada
                    query = $"UPDATE tarif_khusus SET " +
                           $"jenis_kendaraan = '{cmbVehicleType.Text}', " +
                           $"jenis_tarif = '{cmbRateType.Text}', " +
                           $"jam_mulai = '{startTime}', " +
                           $"jam_selesai = '{endTime}', " +
                           $"hari = '{days}', " +
                           $"tarif_flat = {txtFlatRate.Text}, " +
                           $"deskripsi = '{txtDescription.Text}' " +
                           $"WHERE id = {rateId}";
                }
                else
                {
                    // Tambah tarif baru
                    query = $"INSERT INTO tarif_khusus (jenis_kendaraan, jenis_tarif, jam_mulai, jam_selesai, hari, tarif_flat, deskripsi) " +
                           $"VALUES ('{cmbVehicleType.Text}', '{cmbRateType.Text}', '{startTime}', '{endTime}', '{days}', {txtFlatRate.Text}, '{txtDescription.Text}')";
                }
                
                Database.ExecuteNonQuery(query);
                
                MessageBox.Show("Tarif khusus berhasil disimpan.", 
                    "Simpan Tarif", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menyimpan tarif khusus: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 