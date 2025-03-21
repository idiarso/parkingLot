using System;
using System.Data;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;
using Serilog;

namespace SimpleParkingAdmin
{
    public partial class UserForm : Form
    {
        private readonly IAppLogger _logger = new FileLogger();
        private DataGridView dgvUsers;
        
        public UserForm()
        {
            InitializeComponent();
            LoadUsers();
        }
        
        private void InitializeComponent()
        {
            this.Text = "User Management";
            this.Size = new System.Drawing.Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // Create the DataGridView
            dgvUsers = new DataGridView();
            dgvUsers.Dock = DockStyle.Fill;
            dgvUsers.AllowUserToAddRows = false;
            dgvUsers.AllowUserToDeleteRows = false;
            dgvUsers.ReadOnly = true;
            dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            
            // Add the DataGridView to the form
            this.Controls.Add(dgvUsers);
        }
        
        private void LoadUsers()
        {
            try
            {
                string query = "";
                DataTable users = null;
                bool success = false;
                
                // Approach 1: Try with all fields including status
                try
                {
                    query = "SELECT id, username, nama, email, role, status FROM t_user";
                    users = Database.ExecuteQuery(query);
                    success = true;
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Could not load users with status column: {ex.Message}");
                }
                
                // Approach 2: Try without status if it failed
                if (!success)
                {
                    try
                    {
                        query = "SELECT id, username, nama, email, role, 1 as status FROM t_user";
                        users = Database.ExecuteQuery(query);
                        success = true;
                        
                        // Add status column to t_user if it doesn't exist
                        Database.AddColumnIfNotExists("t_user", "status", "BOOLEAN DEFAULT TRUE");
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Failed to load users", ex);
                        MessageBox.Show($"Error loading users: {ex.Message}", 
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                
                if (success && users != null)
                {
                    dgvUsers.DataSource = users;
                    
                    // Format the datagrid
                    if (dgvUsers.Columns.Count > 0)
                    {
                        dgvUsers.Columns["id"].Visible = false;
                        dgvUsers.Columns["username"].HeaderText = "Username";
                        dgvUsers.Columns["nama"].HeaderText = "Nama Lengkap";
                        dgvUsers.Columns["role"].HeaderText = "Role";
                        dgvUsers.Columns["email"].HeaderText = "Email";
                        
                        if (dgvUsers.Columns.Contains("status"))
                        {
                            dgvUsers.Columns["status"].HeaderText = "Status";
                            // Format the status column to show text instead of 1/0
                            dgvUsers.Columns["status"].DefaultCellStyle.Format = "Aktif;Tidak Aktif;";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error loading users", ex);
                MessageBox.Show($"Error loading users: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 