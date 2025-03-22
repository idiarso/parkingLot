using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;

namespace SimpleParkingAdmin
{
    public partial class MemberManagementForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;
        private string memberImagesPath;
        private DataGridView dgvMembers;

        public MemberManagementForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = new FileLogger();
            InitializeComponent();
            
            // Initialize path for member images
            memberImagesPath = Path.Combine(Application.StartupPath, "Images", "Members");
            if (!Directory.Exists(memberImagesPath))
            {
                Directory.CreateDirectory(memberImagesPath);
            }
            
            LoadVehicleTypes();
            LoadMembers();
        }

        private void LoadVehicleTypes()
        {
            try
            {
                // Use status = true for PostgreSQL boolean type
                string query = "SELECT jenis_kendaraan FROM t_tarif WHERE status = true ORDER BY jenis_kendaraan";
                DataTable dt = Database.ExecuteQuery(query);
                cmbVehicleType.Items.Clear();
                
                foreach (DataRow row in dt.Rows)
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
                _logger.Error("Failed to load vehicle types", ex);
                MessageBox.Show("Error loading vehicle types: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadMembers()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                string query = "";
                
                try {
                    // Mencoba mencari struktur tabel yang sesuai
                    string checkQuery = "SHOW COLUMNS FROM t_member LIKE 'nama_member'";
                    DataTable checkResult = Database.ExecuteQuery(checkQuery);
                    
                    if (checkResult != null && checkResult.Rows.Count > 0) {
                        // Struktur dengan nama_member
                        query = @"SELECT m.id, m.kode_member, m.nama_member, m.nomor_polisi, 
                                m.jenis_kendaraan, m.tanggal_daftar, m.tanggal_expired, m.status
                                FROM t_member m
                                ORDER BY m.tanggal_daftar DESC";
                    } else {
                        // Struktur dengan nama saja (sesuai SQL/create_compat_database.sql)
                        query = @"SELECT m.id, m.kode_member, m.nama as nama_member, m.nomor_polisi, 
                                m.jenis_kendaraan, m.created_at as tanggal_daftar, 
                                NULL as tanggal_expired, m.status
                                FROM t_member m
                                ORDER BY m.created_at DESC";
                    }
                } catch (Exception) {
                    // Fallback jika gagal memeriksa struktur
                    query = @"SELECT m.id, m.kode_member, 
                            COALESCE(m.nama_member, m.nama) as nama_member, 
                            m.nomor_polisi, m.jenis_kendaraan, 
                            COALESCE(m.tanggal_daftar, m.created_at) as tanggal_daftar, 
                            COALESCE(m.tanggal_expired, NULL) as tanggal_expired, 
                            m.status
                            FROM t_member m
                            ORDER BY COALESCE(m.tanggal_daftar, m.created_at) DESC";
                }
                
                DataTable members = Database.ExecuteQuery(query);
                dgvMembers.DataSource = members;

                // Format columns
                if (dgvMembers.Columns.Count > 0)
                {
                    dgvMembers.Columns["id"].Visible = false;
                    dgvMembers.Columns["kode_member"].HeaderText = "Kode Member";
                    dgvMembers.Columns["nama_member"].HeaderText = "Nama Member";
                    dgvMembers.Columns["nomor_polisi"].HeaderText = "Nomor Polisi";
                    dgvMembers.Columns["jenis_kendaraan"].HeaderText = "Jenis Kendaraan";
                    dgvMembers.Columns["tanggal_daftar"].HeaderText = "Tanggal Daftar";
                    dgvMembers.Columns["tanggal_daftar"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    
                    if (dgvMembers.Columns.Contains("tanggal_expired")) {
                        dgvMembers.Columns["tanggal_expired"].HeaderText = "Tanggal Expired";
                        dgvMembers.Columns["tanggal_expired"].DefaultCellStyle.Format = "dd/MM/yyyy";
                    }
                    
                    dgvMembers.Columns["status"].HeaderText = "Status";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading members: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearFields()
        {
            txtMemberCode.Clear();
            txtMemberName.Clear();
            txtPlateNumber.Clear();
            cmbVehicleType.SelectedIndex = cmbVehicleType.Items.Count > 0 ? 0 : -1;
            dtpRegistration.Value = DateTime.Today;
            dtpExpiration.Value = DateTime.Today.AddYears(1);
            chkActive.Checked = true;
            picVehicle.Image = null;
            btnSave.Tag = null;
            btnTakePicture.Enabled = true;
            txtMemberCode.ReadOnly = false;
            txtMemberCode.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtMemberCode.Text))
                {
                    MessageBox.Show("Member code is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMemberCode.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtMemberName.Text))
                {
                    MessageBox.Show("Member name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtMemberName.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPlateNumber.Text))
                {
                    MessageBox.Show("Plate number is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPlateNumber.Focus();
                    return;
                }

                if (cmbVehicleType.SelectedIndex == -1)
                {
                    MessageBox.Show("Vehicle type is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbVehicleType.Focus();
                    return;
                }

                if (dtpExpiration.Value <= DateTime.Today)
                {
                    MessageBox.Show("Expiration date must be in the future.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpExpiration.Focus();
                    return;
                }

                // Save vehicle image if available
                string imagePath = "";
                if (picVehicle.Image != null)
                {
                    string fileName = $"member_{txtMemberCode.Text}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                    imagePath = Path.Combine(memberImagesPath, fileName);
                    
                    picVehicle.Image.Save(imagePath);
                }

                if (btnSave.Tag == null) // New member
                {
                    // Check if member code already exists
                    string checkQuery = $"SELECT COUNT(*) FROM t_member WHERE kode_member = '{txtMemberCode.Text}'";
                    int count = Convert.ToInt32(Database.ExecuteScalar(checkQuery));
                    
                    if (count > 0)
                    {
                        MessageBox.Show("This member code already exists.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtMemberCode.Focus();
                        return;
                    }
                
                    string query = $@"INSERT INTO t_member (kode_member, nama_member, nomor_polisi, 
                                                         jenis_kendaraan, tanggal_daftar, tanggal_expired, 
                                                         foto_kendaraan, status) 
                                   VALUES ('{txtMemberCode.Text}', '{txtMemberName.Text}', '{txtPlateNumber.Text}', 
                                          '{cmbVehicleType.SelectedItem}', '{dtpRegistration.Value:yyyy-MM-dd}', 
                                          '{dtpExpiration.Value:yyyy-MM-dd}', '{imagePath}', {(chkActive.Checked ? 1 : 0)})";
                    
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("Member added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else // Update existing member
                {
                    string imageUpdate = !string.IsNullOrEmpty(imagePath) ? $", foto_kendaraan = '{imagePath}'" : "";
                    
                    string query = $@"UPDATE t_member 
                                   SET nama_member = '{txtMemberName.Text}', 
                                       nomor_polisi = '{txtPlateNumber.Text}', 
                                       jenis_kendaraan = '{cmbVehicleType.SelectedItem}', 
                                       tanggal_daftar = '{dtpRegistration.Value:yyyy-MM-dd}', 
                                       tanggal_expired = '{dtpExpiration.Value:yyyy-MM-dd}', 
                                       status = {(chkActive.Checked ? 1 : 0)}
                                       {imageUpdate}
                                   WHERE id = {btnSave.Tag}";
                    
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("Member updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearFields();
                LoadMembers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving member: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void dgvMembers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgvMembers.Rows[e.RowIndex];
                    int memberId = Convert.ToInt32(row.Cells["id"].Value);
                    
                    // Get full member data including image path
                    string query = $"SELECT * FROM t_member WHERE id = {memberId}";
                    DataTable memberData = Database.ExecuteQuery(query);
                    
                    if (memberData.Rows.Count > 0)
                    {
                        DataRow member = memberData.Rows[0];
                        
                        txtMemberCode.Text = member["kode_member"].ToString();
                        txtMemberName.Text = member["nama_member"].ToString();
                        txtPlateNumber.Text = member["nomor_polisi"].ToString();
                        
                        string vehicleType = member["jenis_kendaraan"].ToString();
                        for (int i = 0; i < cmbVehicleType.Items.Count; i++)
                        {
                            if (cmbVehicleType.Items[i].ToString() == vehicleType)
                            {
                                cmbVehicleType.SelectedIndex = i;
                                break;
                            }
                        }
                        
                        dtpRegistration.Value = Convert.ToDateTime(member["tanggal_daftar"]);
                        dtpExpiration.Value = Convert.ToDateTime(member["tanggal_expired"]);
                        chkActive.Checked = Convert.ToInt32(member["status"]) == 1;
                        
                        // Load image if available
                        string imagePath = member["foto_kendaraan"].ToString();
                        if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                        {
                            using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                            {
                                picVehicle.Image = Image.FromStream(stream);
                            }
                        }
                        else
                        {
                            picVehicle.Image = null;
                        }
                        
                        btnSave.Tag = memberId;
                        txtMemberCode.ReadOnly = true; // Don't allow changing member code
                        btnTakePicture.Enabled = true;
                        txtMemberName.Focus();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading member data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvMembers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a member to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this member?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int memberId = Convert.ToInt32(dgvMembers.SelectedRows[0].Cells["id"].Value);
                    
                    // Check if this member has parking history
                    string checkQuery = $@"SELECT COUNT(*) FROM t_parkir 
                                         WHERE kode_member = (SELECT kode_member FROM t_member WHERE id = {memberId})";
                    int useCount = Convert.ToInt32(Database.ExecuteScalar(checkQuery));
                    
                    if (useCount > 0)
                    {
                        if (MessageBox.Show($"This member has {useCount} parking records. Instead of deleting, would you like to deactivate this member?", 
                            "Member In Use", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            string deactivateQuery = $"UPDATE t_member SET status = 0 WHERE id = {memberId}";
                            Database.ExecuteNonQuery(deactivateQuery);
                            MessageBox.Show("Member deactivated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadMembers();
                            return;
                        }
                        else
                        {
                            return; // User chose not to deactivate
                        }
                    }
                    
                    // Get the image path before deleting
                    string imageQuery = $"SELECT foto_kendaraan FROM t_member WHERE id = {memberId}";
                    object imagePathObj = Database.ExecuteScalar(imageQuery);
                    string imagePath = imagePathObj != null ? imagePathObj.ToString() : "";
                    
                    // Delete the member
                    string query = $"DELETE FROM t_member WHERE id = {memberId}";
                    Database.ExecuteNonQuery(query);
                    
                    // Delete the image file if it exists
                    if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
                    {
                        File.Delete(imagePath);
                    }
                    
                    MessageBox.Show("Member deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadMembers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting member: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnTakePicture_Click(object sender, EventArgs e)
        {
            try
            {
                // Use OpenFileDialog for simplicity
                // In a real implementation, this would connect to a camera
                using (OpenFileDialog dlg = new OpenFileDialog())
                {
                    dlg.Title = "Select Vehicle Image";
                    dlg.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp)|*.jpg;*.jpeg;*.png;*.bmp|All files (*.*)|*.*";
                    
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        picVehicle.Image = Image.FromFile(dlg.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error taking picture: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = txtSearch.Text.Trim();
                
                if (string.IsNullOrEmpty(searchTerm))
                {
                    LoadMembers();
                    return;
                }
                
                string query = $@"SELECT m.id, m.kode_member, m.nama_member, m.nomor_polisi, 
                               m.jenis_kendaraan, m.tanggal_daftar, m.tanggal_expired, m.status
                               FROM t_member m
                               WHERE m.kode_member LIKE '%{searchTerm}%' 
                               OR m.nama_member LIKE '%{searchTerm}%'
                               OR m.nomor_polisi LIKE '%{searchTerm}%'
                               ORDER BY m.tanggal_daftar DESC";
                
                DataTable searchResults = Database.ExecuteQuery(query);
                dgvMembers.DataSource = searchResults;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching members: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSearch_Click(sender, e);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        #region Windows Form Designer generated code

        private TextBox txtMemberCode;
        private TextBox txtMemberName;
        private TextBox txtPlateNumber;
        private ComboBox cmbVehicleType;
        private DateTimePicker dtpRegistration;
        private DateTimePicker dtpExpiration;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnClear;
        private Button btnDelete;
        private Button btnTakePicture;
        private PictureBox picVehicle;
        private Label lblTitle;
        private Label lblMemberCode;
        private Label lblMemberName;
        private Label lblPlateNumber;
        private Label lblVehicleType;
        private Label lblRegistration;
        private Label lblExpiration;
        private Label lblVehicleImage;
        private TextBox txtSearch;
        private Button btnSearch;

        private void InitializeComponent()
        {
            this.dgvMembers = new DataGridView();
            this.txtMemberCode = new TextBox();
            this.txtMemberName = new TextBox();
            this.txtPlateNumber = new TextBox();
            this.cmbVehicleType = new ComboBox();
            this.dtpRegistration = new DateTimePicker();
            this.dtpExpiration = new DateTimePicker();
            this.chkActive = new CheckBox();
            this.btnSave = new Button();
            this.btnClear = new Button();
            this.btnDelete = new Button();
            this.btnTakePicture = new Button();
            this.picVehicle = new PictureBox();
            this.lblTitle = new Label();
            this.lblMemberCode = new Label();
            this.lblMemberName = new Label();
            this.lblPlateNumber = new Label();
            this.lblVehicleType = new Label();
            this.lblRegistration = new Label();
            this.lblExpiration = new Label();
            this.lblVehicleImage = new Label();
            this.txtSearch = new TextBox();
            this.btnSearch = new Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picVehicle)).BeginInit();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(197, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Member Management";

            // txtSearch
            this.txtSearch.Location = new Point(285, 33);
            this.txtSearch.Name = "txtSearch";
            this.txtSearch.Size = new Size(294, 23);
            this.txtSearch.TabIndex = 17;
            this.txtSearch.KeyDown += new KeyEventHandler(this.txtSearch_KeyDown);

            // btnSearch
            this.btnSearch.Location = new Point(585, 32);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new Size(75, 25);
            this.btnSearch.TabIndex = 18;
            this.btnSearch.Text = "Search";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new EventHandler(this.btnSearch_Click);

            // lblMemberCode
            this.lblMemberCode.AutoSize = true;
            this.lblMemberCode.Location = new Point(14, 44);
            this.lblMemberCode.Name = "lblMemberCode";
            this.lblMemberCode.Size = new Size(86, 15);
            this.lblMemberCode.TabIndex = 1;
            this.lblMemberCode.Text = "Kode Member:";

            // txtMemberCode
            this.txtMemberCode.Location = new Point(14, 62);
            this.txtMemberCode.Name = "txtMemberCode";
            this.txtMemberCode.Size = new Size(250, 23);
            this.txtMemberCode.TabIndex = 0;

            // lblMemberName
            this.lblMemberName.AutoSize = true;
            this.lblMemberName.Location = new Point(14, 88);
            this.lblMemberName.Name = "lblMemberName";
            this.lblMemberName.Size = new Size(90, 15);
            this.lblMemberName.TabIndex = 2;
            this.lblMemberName.Text = "Nama Member:";

            // txtMemberName
            this.txtMemberName.Location = new Point(14, 106);
            this.txtMemberName.Name = "txtMemberName";
            this.txtMemberName.Size = new Size(250, 23);
            this.txtMemberName.TabIndex = 1;

            // lblPlateNumber
            this.lblPlateNumber.AutoSize = true;
            this.lblPlateNumber.Location = new Point(14, 132);
            this.lblPlateNumber.Name = "lblPlateNumber";
            this.lblPlateNumber.Size = new Size(81, 15);
            this.lblPlateNumber.TabIndex = 3;
            this.lblPlateNumber.Text = "Nomor Polisi:";

            // txtPlateNumber
            this.txtPlateNumber.Location = new Point(14, 150);
            this.txtPlateNumber.Name = "txtPlateNumber";
            this.txtPlateNumber.Size = new Size(250, 23);
            this.txtPlateNumber.TabIndex = 2;

            // lblVehicleType
            this.lblVehicleType.AutoSize = true;
            this.lblVehicleType.Location = new Point(14, 176);
            this.lblVehicleType.Name = "lblVehicleType";
            this.lblVehicleType.Size = new Size(98, 15);
            this.lblVehicleType.TabIndex = 4;
            this.lblVehicleType.Text = "Jenis Kendaraan:";

            // cmbVehicleType
            this.cmbVehicleType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbVehicleType.FormattingEnabled = true;
            this.cmbVehicleType.Location = new Point(14, 194);
            this.cmbVehicleType.Name = "cmbVehicleType";
            this.cmbVehicleType.Size = new Size(250, 23);
            this.cmbVehicleType.TabIndex = 3;

            // lblRegistration
            this.lblRegistration.AutoSize = true;
            this.lblRegistration.Location = new Point(14, 220);
            this.lblRegistration.Name = "lblRegistration";
            this.lblRegistration.Size = new Size(88, 15);
            this.lblRegistration.TabIndex = 5;
            this.lblRegistration.Text = "Tanggal Daftar:";

            // dtpRegistration
            this.dtpRegistration.Format = DateTimePickerFormat.Short;
            this.dtpRegistration.Location = new Point(14, 238);
            this.dtpRegistration.Name = "dtpRegistration";
            this.dtpRegistration.Size = new Size(250, 23);
            this.dtpRegistration.TabIndex = 4;
            this.dtpRegistration.Value = DateTime.Today;

            // lblExpiration
            this.lblExpiration.AutoSize = true;
            this.lblExpiration.Location = new Point(14, 264);
            this.lblExpiration.Name = "lblExpiration";
            this.lblExpiration.Size = new Size(98, 15);
            this.lblExpiration.TabIndex = 6;
            this.lblExpiration.Text = "Tanggal Expired:";

            // dtpExpiration
            this.dtpExpiration.Format = DateTimePickerFormat.Short;
            this.dtpExpiration.Location = new Point(14, 282);
            this.dtpExpiration.Name = "dtpExpiration";
            this.dtpExpiration.Size = new Size(250, 23);
            this.dtpExpiration.TabIndex = 5;
            this.dtpExpiration.Value = DateTime.Today.AddYears(1);

            // lblVehicleImage
            this.lblVehicleImage.AutoSize = true;
            this.lblVehicleImage.Location = new Point(14, 308);
            this.lblVehicleImage.Name = "lblVehicleImage";
            this.lblVehicleImage.Size = new Size(100, 15);
            this.lblVehicleImage.TabIndex = 7;
            this.lblVehicleImage.Text = "Foto Kendaraan:";

            // picVehicle
            this.picVehicle.BorderStyle = BorderStyle.FixedSingle;
            this.picVehicle.Location = new Point(14, 326);
            this.picVehicle.Name = "picVehicle";
            this.picVehicle.Size = new Size(250, 150);
            this.picVehicle.SizeMode = PictureBoxSizeMode.Zoom;
            this.picVehicle.TabIndex = 8;
            this.picVehicle.TabStop = false;

            // btnTakePicture
            this.btnTakePicture.Location = new Point(14, 482);
            this.btnTakePicture.Name = "btnTakePicture";
            this.btnTakePicture.Size = new Size(250, 30);
            this.btnTakePicture.TabIndex = 6;
            this.btnTakePicture.Text = "Browse Image";
            this.btnTakePicture.UseVisualStyleBackColor = true;
            this.btnTakePicture.Click += new EventHandler(this.btnTakePicture_Click);

            // chkActive
            this.chkActive.AutoSize = true;
            this.chkActive.Checked = true;
            this.chkActive.CheckState = CheckState.Checked;
            this.chkActive.Location = new Point(14, 518);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new Size(60, 19);
            this.chkActive.TabIndex = 7;
            this.chkActive.Text = "Active";
            this.chkActive.UseVisualStyleBackColor = true;

            // btnSave
            this.btnSave.BackColor = Color.Green;
            this.btnSave.ForeColor = Color.White;
            this.btnSave.Location = new Point(14, 548);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(120, 30);
            this.btnSave.TabIndex = 8;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnClear
            this.btnClear.Location = new Point(144, 548);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new Size(120, 30);
            this.btnClear.TabIndex = 9;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // btnDelete
            this.btnDelete.BackColor = Color.Red;
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(14, 588);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(250, 30);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            // dgvMembers
            this.dgvMembers.AllowUserToAddRows = false;
            this.dgvMembers.AllowUserToDeleteRows = false;
            this.dgvMembers.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                | AnchorStyles.Left) 
                | AnchorStyles.Right)));
            this.dgvMembers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvMembers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMembers.Location = new Point(285, 62);
            this.dgvMembers.MultiSelect = false;
            this.dgvMembers.Name = "dgvMembers";
            this.dgvMembers.ReadOnly = true;
            this.dgvMembers.RowTemplate.Height = 25;
            this.dgvMembers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvMembers.Size = new Size(479, 556);
            this.dgvMembers.TabIndex = 16;
            this.dgvMembers.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvMembers_CellDoubleClick);

            // MemberManagementForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(776, 630);
            this.Controls.Add(this.dgvMembers);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.txtSearch);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.btnTakePicture);
            this.Controls.Add(this.picVehicle);
            this.Controls.Add(this.dtpExpiration);
            this.Controls.Add(this.dtpRegistration);
            this.Controls.Add(this.cmbVehicleType);
            this.Controls.Add(this.txtPlateNumber);
            this.Controls.Add(this.txtMemberName);
            this.Controls.Add(this.txtMemberCode);
            this.Controls.Add(this.lblVehicleImage);
            this.Controls.Add(this.lblExpiration);
            this.Controls.Add(this.lblRegistration);
            this.Controls.Add(this.lblVehicleType);
            this.Controls.Add(this.lblPlateNumber);
            this.Controls.Add(this.lblMemberName);
            this.Controls.Add(this.lblMemberCode);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new Size(700, 650);
            this.Name = "MemberManagementForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Member Management";

            ((System.ComponentModel.ISupportInitialize)(this.dgvMembers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picVehicle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 