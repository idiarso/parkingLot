using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class UserManagementForm : Form
    {
        public UserManagementForm()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                // Try different approaches to load users with better error handling
                string query;
                DataTable users = null;
                bool success = false;
                
                // Approach 1: Try with t_user table including status column
                try
                {
                    query = "SELECT id, username, nama as nama_lengkap, role, status FROM t_user ORDER BY username";
                    users = Database.ExecuteQuery(query);
                    success = true;
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Could not load users with status column: {ex.Message}");
                }
                
                // Approach 2: Try with t_user table without status column
                if (!success)
                {
                    try
                    {
                        query = "SELECT id, username, nama as nama_lengkap, role, 1 as status FROM t_user ORDER BY username";
                        users = Database.ExecuteQuery(query);
                        success = true;
                        
                        // Try to add status column to t_user
                        try
                        {
                            Database.AddColumnIfNotExists("t_user", "status", "BOOLEAN DEFAULT TRUE");
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning($"Could not add status column to t_user: {ex.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Could not load users from t_user table: {ex.Message}");
                    }
                }
                
                // Approach 3: Try with new schema (users table)
                if (!success)
                {
                    try
                    {
                        query = "SELECT id, username, nama, role, status FROM users ORDER BY username";
                        users = Database.ExecuteQuery(query);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to load users from any known table: {ex.Message}");
                        MessageBox.Show($"Error loading users: {ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                if (success && users != null)
                {
                    dgvUsers.DataSource = users;
    
                    // Format columns
                    if (dgvUsers.Columns.Count > 0)
                    {
                        dgvUsers.Columns["id"].Visible = false;
                        dgvUsers.Columns["username"].HeaderText = "Username";
                        dgvUsers.Columns["nama"].HeaderText = "Nama";
                        dgvUsers.Columns["role"].HeaderText = "Role";
                        dgvUsers.Columns["status"].HeaderText = "Status";
                        
                        // Format status column to show text
                        dgvUsers.Columns["status"].DefaultCellStyle.Format = "Aktif;Tidak Aktif;";
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex, "LoadUsers");
                MessageBox.Show($"Error loading users: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearFields()
        {
            txtUsername.Clear();
            txtPassword.Clear();
            txtFullName.Clear();
            cmbRole.SelectedIndex = -1;
            chkActive.Checked = true;
            txtUsername.Enabled = true;
            btnSave.Tag = null;
            txtUsername.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtUsername.Text))
                {
                    MessageBox.Show("Username is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUsername.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtFullName.Text))
                {
                    MessageBox.Show("Full name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtFullName.Focus();
                    return;
                }

                if (cmbRole.SelectedIndex == -1)
                {
                    MessageBox.Show("Role is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    cmbRole.Focus();
                    return;
                }

                if (btnSave.Tag == null) // New user
                {
                    if (string.IsNullOrWhiteSpace(txtPassword.Text))
                    {
                        MessageBox.Show("Password is required for new user.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtPassword.Focus();
                        return;
                    }

                    // Determine which schema we're using
                    bool useNewSchema = CheckForNewSchema();
                    string query;
                    
                    if (useNewSchema)
                    {
                        // Hash the password for the new schema
                        string hashedPassword = HashPassword(txtPassword.Text);
                        
                        // New schema (users table)
                        query = $@"INSERT INTO users (username, password, nama, role, active)
                            VALUES ('{txtUsername.Text}', '{hashedPassword}', '{txtFullName.Text}', '{cmbRole.Text}', 1)";
                    }
                    else
                    {
                        // Old schema (t_user table)
                        query = $@"INSERT INTO t_user (username, password, nama, role, status) 
                                 VALUES ('{txtUsername.Text}', '{txtPassword.Text}', '{txtFullName.Text}', 
                                         '{cmbRole.SelectedItem}', {(chkActive.Checked ? 1 : 0)})";
                    }
                    
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("User added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else // Update existing user
                {
                    // Determine which schema we're using
                    bool useNewSchema = CheckForNewSchema();
                    string query;
                    
                    if (useNewSchema)
                    {
                        string passwordUpdate = string.IsNullOrWhiteSpace(txtPassword.Text) ? "" : 
                                              $", password = CASE WHEN '{txtPassword.Text}' = '' THEN password ELSE '{txtPassword.Text}' END";
                        
                        // New schema (users table)
                        query = $@"UPDATE users 
                                 SET nama = '{txtFullName.Text}',
                                     role = '{cmbRole.Text}',
                                     active = {(chkActive.Checked ? 1 : 0)},
                                     password = CASE WHEN '{txtPassword.Text}' = '' THEN password ELSE '{txtPassword.Text}' END
                                 WHERE id = {btnSave.Tag}";
                    }
                    else
                    {
                        string passwordUpdate = string.IsNullOrWhiteSpace(txtPassword.Text) ? "" : 
                                              $", password = '{txtPassword.Text}'";
                        
                        // Old schema (t_user table)
                        query = $@"UPDATE t_user 
                                 SET nama = '{txtFullName.Text}', 
                                     role = '{cmbRole.SelectedItem}', 
                                     status = {(chkActive.Checked ? 1 : 0)}
                                     {passwordUpdate}
                                 WHERE id = {btnSave.Tag}";
                    }
                    
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("User updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearFields();
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void dgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];
                txtUsername.Text = row.Cells["username"].Value.ToString();
                txtFullName.Text = row.Cells["nama"].Value.ToString();
                cmbRole.SelectedItem = row.Cells["role"].Value.ToString();
                chkActive.Checked = Convert.ToInt32(row.Cells["status"].Value) == 1;
                btnSave.Tag = row.Cells["id"].Value;
                txtUsername.Enabled = false;
                txtPassword.Clear();
                txtUsername.Focus();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a user to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this user?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int userId = Convert.ToInt32(dgvUsers.SelectedRows[0].Cells["id"].Value);
                    
                    // Determine which schema we're using
                    bool useNewSchema = CheckForNewSchema();
                    string query;
                    
                    if (useNewSchema)
                    {
                        // New schema (users table)
                        query = $"DELETE FROM users WHERE id = {userId}";
                    }
                    else
                    {
                        // Old schema (t_user table)
                        query = $"DELETE FROM t_user WHERE id = {userId}";
                    }
                    
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("User deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUsers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool CheckForNewSchema()
        {
            try
            {
                // Check if the users table exists (new schema)
                string query = "SHOW TABLES LIKE 'users'";
                DataTable dt = Database.ExecuteQuery(query);
                
                if (dt != null && dt.Rows.Count > 0)
                {
                    return true; // New schema (users table exists)
                }
                else
                {
                    return false; // Old schema (t_user table)
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error checking schema: {ex.Message}");
                // If there's an error, default to the old schema for safety
                return false;
            }
        }

        #region Windows Form Designer generated code

        private DataGridView dgvUsers;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private TextBox txtFullName;
        private ComboBox cmbRole;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnClear;
        private Button btnDelete;
        private Label lblTitle;
        private Label lblUsername;
        private Label lblPassword;
        private Label lblFullName;
        private Label lblRole;

        private void InitializeComponent()
        {
            this.dgvUsers = new DataGridView();
            this.txtUsername = new TextBox();
            this.txtPassword = new TextBox();
            this.txtFullName = new TextBox();
            this.cmbRole = new ComboBox();
            this.chkActive = new CheckBox();
            this.btnSave = new Button();
            this.btnClear = new Button();
            this.btnDelete = new Button();
            this.lblTitle = new Label();
            this.lblUsername = new Label();
            this.lblPassword = new Label();
            this.lblFullName = new Label();
            this.lblRole = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).BeginInit();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(190, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "User Management";

            // lblUsername
            this.lblUsername.AutoSize = true;
            this.lblUsername.Location = new Point(14, 44);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new Size(60, 15);
            this.lblUsername.TabIndex = 1;
            this.lblUsername.Text = "Username:";

            // txtUsername
            this.txtUsername.Location = new Point(14, 62);
            this.txtUsername.Name = "txtUsername";
            this.txtUsername.Size = new Size(250, 23);
            this.txtUsername.TabIndex = 0;

            // lblPassword
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new Point(14, 88);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new Size(60, 15);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password:";

            // txtPassword
            this.txtPassword.Location = new Point(14, 106);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new Size(250, 23);
            this.txtPassword.TabIndex = 1;

            // lblFullName
            this.lblFullName.AutoSize = true;
            this.lblFullName.Location = new Point(14, 132);
            this.lblFullName.Name = "lblFullName";
            this.lblFullName.Size = new Size(70, 15);
            this.lblFullName.TabIndex = 3;
            this.lblFullName.Text = "Full Name:";

            // txtFullName
            this.txtFullName.Location = new Point(14, 150);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new Size(250, 23);
            this.txtFullName.TabIndex = 2;

            // lblRole
            this.lblRole.AutoSize = true;
            this.lblRole.Location = new Point(14, 176);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new Size(35, 15);
            this.lblRole.TabIndex = 4;
            this.lblRole.Text = "Role:";

            // cmbRole
            this.cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbRole.Location = new Point(14, 194);
            this.cmbRole.Name = "cmbRole";
            this.cmbRole.Size = new Size(250, 23);
            this.cmbRole.TabIndex = 3;
            this.cmbRole.Items.AddRange(new object[] { "Admin", "Operator" });

            // chkActive
            this.chkActive.AutoSize = true;
            this.chkActive.Checked = true;
            this.chkActive.CheckState = CheckState.Checked;
            this.chkActive.Location = new Point(14, 223);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new Size(60, 19);
            this.chkActive.TabIndex = 4;
            this.chkActive.Text = "Active";

            // btnSave
            this.btnSave.BackColor = Color.Green;
            this.btnSave.ForeColor = Color.White;
            this.btnSave.Location = new Point(14, 248);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(120, 30);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnClear
            this.btnClear.Location = new Point(144, 248);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new Size(120, 30);
            this.btnClear.TabIndex = 6;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // btnDelete
            this.btnDelete.BackColor = Color.Red;
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(14, 288);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(250, 30);
            this.btnDelete.TabIndex = 7;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            // dgvUsers
            this.dgvUsers.AllowUserToAddRows = false;
            this.dgvUsers.AllowUserToDeleteRows = false;
            this.dgvUsers.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                | AnchorStyles.Left) 
                | AnchorStyles.Right)));
            this.dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvUsers.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUsers.Location = new Point(285, 62);
            this.dgvUsers.MultiSelect = false;
            this.dgvUsers.Name = "dgvUsers";
            this.dgvUsers.ReadOnly = true;
            this.dgvUsers.RowTemplate.Height = 25;
            this.dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvUsers.Size = new Size(479, 430);
            this.dgvUsers.TabIndex = 8;
            this.dgvUsers.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvUsers_CellDoubleClick);

            // UserManagementForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(776, 504);
            this.Controls.Add(this.dgvUsers);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.cmbRole);
            this.Controls.Add(this.txtFullName);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUsername);
            this.Controls.Add(this.lblRole);
            this.Controls.Add(this.lblFullName);
            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.lblUsername);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new Size(700, 500);
            this.Name = "UserManagementForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "User Management";

            ((System.ComponentModel.ISupportInitialize)(this.dgvUsers)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 