using System;
using System.Data;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class UsersForm : Form
    {
        public UsersForm()
        {
            InitializeComponent();
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                string query = "SELECT id, username, nama, role FROM users ORDER BY username";
                DataTable users = Database.ExecuteQuery(query);
                
                dgvUsers.DataSource = users;
                
                // Format columns
                if (dgvUsers.Columns.Count > 0)
                {
                    if (dgvUsers.Columns.Contains("id"))
                        dgvUsers.Columns["id"].HeaderText = "ID";
                        
                    if (dgvUsers.Columns.Contains("username"))
                        dgvUsers.Columns["username"].HeaderText = "Username";
                        
                    if (dgvUsers.Columns.Contains("nama"))
                        dgvUsers.Columns["nama"].HeaderText = "Nama";
                        
                    if (dgvUsers.Columns.Contains("role"))
                        dgvUsers.Columns["role"].HeaderText = "Level";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat memuat data pengguna: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fitur Tambah User sedang dalam pengembangan.", 
                "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silakan pilih pengguna yang ingin diedit.", 
                    "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            MessageBox.Show("Fitur Edit User sedang dalam pengembangan.", 
                "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Silakan pilih pengguna yang ingin dihapus.", 
                    "Perhatian", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            MessageBox.Show("Fitur Hapus User sedang dalam pengembangan.", 
                "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsers();
        }
        
        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Windows Form Designer generated code

        private DataGridView dgvUsers;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnRefresh;
        private Button btnClose;

        private void InitializeComponent()
        {
            this.dgvUsers = new DataGridView();
            this.btnAdd = new Button();
            this.btnEdit = new Button();
            this.btnDelete = new Button();
            this.btnRefresh = new Button();
            this.btnClose = new Button();
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();
            
            // dgvUsers
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                | AnchorStyles.Left) 
                | AnchorStyles.Right)));
            this.dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Location = new Point(12, 12);
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.RowTemplate.Height = 25;
            this.dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new Size(760, 400);
            this.dgvUsers.TabIndex = 0;
            
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
            
            // UsersForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 468);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnEdit);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.dgvUsers);
            this.MinimumSize = new Size(700, 500);
            this.Name = "UsersForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Manajemen User";
            this.Load += new EventHandler(this.UsersForm_Load);
            
            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion
    }
} 