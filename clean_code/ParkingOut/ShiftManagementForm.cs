using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class ShiftManagementForm : Form
    {
        public ShiftManagementForm()
        {
            InitializeComponent();
            LoadShifts();
        }

        private void LoadShifts()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                
                DataTable shifts;
                bool usedOldSchema = false;
                
                try
                {
                    // Try old schema first (t_shift)
                    string query = "SELECT id, nama_shift, jam_mulai, jam_selesai, status FROM t_shift ORDER BY jam_mulai";
                    shifts = Database.ExecuteQuery(query);
                    usedOldSchema = true;
                }
                catch (Exception)
                {
                    try
                    {
                        // Try old schema without status column
                        string query = "SELECT id, nama_shift, jam_mulai, jam_selesai FROM t_shift ORDER BY jam_mulai";
                        shifts = Database.ExecuteQuery(query);
                        
                        // Add status column with default value
                        shifts.Columns.Add("status", typeof(int));
                        foreach (DataRow row in shifts.Rows)
                        {
                            row["status"] = 1;
                        }
                        usedOldSchema = true;
                    }
                    catch (Exception)
                    {
                        // Try new schema (shifts)
                        string query = "SELECT id, name as nama_shift, start_time as jam_mulai, end_time as jam_selesai, active as status FROM shifts ORDER BY start_time";
                        shifts = Database.ExecuteQuery(query);
                        usedOldSchema = false;
                    }
                }
                
                dgvShifts.DataSource = shifts;

                // Format columns
                if (dgvShifts.Columns.Count > 0)
                {
                    dgvShifts.Columns["id"].Visible = false;
                    dgvShifts.Columns["nama_shift"].HeaderText = "Nama Shift";
                    dgvShifts.Columns["jam_mulai"].HeaderText = "Jam Mulai";
                    dgvShifts.Columns["jam_selesai"].HeaderText = "Jam Selesai";
                    dgvShifts.Columns["status"].HeaderText = "Status";
                }
                
                // Store the schema info for later use in editing/adding records
                this.Tag = usedOldSchema ? "old_schema" : "new_schema";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading shifts: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ClearFields()
        {
            txtShiftName.Clear();
            dtpStartTime.Value = DateTime.Today.AddHours(8); // Default 8 AM
            dtpEndTime.Value = DateTime.Today.AddHours(17); // Default 5 PM
            chkActive.Checked = true;
            btnSave.Tag = null;
            txtShiftName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtShiftName.Text))
                {
                    MessageBox.Show("Shift name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtShiftName.Focus();
                    return;
                }

                if (dtpEndTime.Value <= dtpStartTime.Value)
                {
                    MessageBox.Show("End time must be after start time.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    dtpEndTime.Focus();
                    return;
                }

                if (btnSave.Tag == null) // New shift
                {
                    string query = $@"INSERT INTO t_shift (nama_shift, jam_mulai, jam_selesai, status) 
                                    VALUES ('{txtShiftName.Text}', 
                                            '{dtpStartTime.Value:HH:mm:ss}', 
                                            '{dtpEndTime.Value:HH:mm:ss}', 
                                            {(chkActive.Checked ? 1 : 0)})";
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("Shift added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else // Update existing shift
                {
                    string query = $@"UPDATE t_shift 
                                    SET nama_shift = '{txtShiftName.Text}', 
                                        jam_mulai = '{dtpStartTime.Value:HH:mm:ss}', 
                                        jam_selesai = '{dtpEndTime.Value:HH:mm:ss}', 
                                        status = {(chkActive.Checked ? 1 : 0)}
                                    WHERE id = {btnSave.Tag}";
                    
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("Shift updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                ClearFields();
                LoadShifts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving shift: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void dgvShifts_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvShifts.Rows[e.RowIndex];
                txtShiftName.Text = row.Cells["nama_shift"].Value.ToString();
                dtpStartTime.Value = DateTime.Parse(row.Cells["jam_mulai"].Value.ToString());
                dtpEndTime.Value = DateTime.Parse(row.Cells["jam_selesai"].Value.ToString());
                chkActive.Checked = Convert.ToInt32(row.Cells["status"].Value) == 1;
                btnSave.Tag = row.Cells["id"].Value;
                txtShiftName.Focus();
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvShifts.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a shift to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this shift?", "Confirm Delete", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    int shiftId = Convert.ToInt32(dgvShifts.SelectedRows[0].Cells["id"].Value);
                    string query = $"DELETE FROM t_shift WHERE id = {shiftId}";
                    Database.ExecuteNonQuery(query);
                    MessageBox.Show("Shift deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadShifts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting shift: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #region Windows Form Designer generated code

        private DataGridView dgvShifts;
        private TextBox txtShiftName;
        private DateTimePicker dtpStartTime;
        private DateTimePicker dtpEndTime;
        private CheckBox chkActive;
        private Button btnSave;
        private Button btnClear;
        private Button btnDelete;
        private Label lblTitle;
        private Label lblShiftName;
        private Label lblStartTime;
        private Label lblEndTime;

        private void InitializeComponent()
        {
            this.dgvShifts = new DataGridView();
            this.txtShiftName = new TextBox();
            this.dtpStartTime = new DateTimePicker();
            this.dtpEndTime = new DateTimePicker();
            this.chkActive = new CheckBox();
            this.btnSave = new Button();
            this.btnClear = new Button();
            this.btnDelete = new Button();
            this.lblTitle = new Label();
            this.lblShiftName = new Label();
            this.lblStartTime = new Label();
            this.lblEndTime = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgvShifts)).BeginInit();
            this.SuspendLayout();

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblTitle.Location = new Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(190, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Shift Management";

            // lblShiftName
            this.lblShiftName.AutoSize = true;
            this.lblShiftName.Location = new Point(14, 44);
            this.lblShiftName.Name = "lblShiftName";
            this.lblShiftName.Size = new Size(70, 15);
            this.lblShiftName.TabIndex = 1;
            this.lblShiftName.Text = "Nama Shift:";

            // txtShiftName
            this.txtShiftName.Location = new Point(14, 62);
            this.txtShiftName.Name = "txtShiftName";
            this.txtShiftName.Size = new Size(250, 23);
            this.txtShiftName.TabIndex = 0;

            // lblStartTime
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new Point(14, 88);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new Size(70, 15);
            this.lblStartTime.TabIndex = 2;
            this.lblStartTime.Text = "Jam Mulai:";

            // dtpStartTime
            this.dtpStartTime.Format = DateTimePickerFormat.Time;
            this.dtpStartTime.Location = new Point(14, 106);
            this.dtpStartTime.Name = "dtpStartTime";
            this.dtpStartTime.ShowUpDown = true;
            this.dtpStartTime.Size = new Size(250, 23);
            this.dtpStartTime.TabIndex = 1;
            this.dtpStartTime.Value = DateTime.Today.AddHours(8);

            // lblEndTime
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new Point(14, 132);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new Size(70, 15);
            this.lblEndTime.TabIndex = 3;
            this.lblEndTime.Text = "Jam Selesai:";

            // dtpEndTime
            this.dtpEndTime.Format = DateTimePickerFormat.Time;
            this.dtpEndTime.Location = new Point(14, 150);
            this.dtpEndTime.Name = "dtpEndTime";
            this.dtpEndTime.ShowUpDown = true;
            this.dtpEndTime.Size = new Size(250, 23);
            this.dtpEndTime.TabIndex = 2;
            this.dtpEndTime.Value = DateTime.Today.AddHours(17);

            // chkActive
            this.chkActive.AutoSize = true;
            this.chkActive.Checked = true;
            this.chkActive.CheckState = CheckState.Checked;
            this.chkActive.Location = new Point(14, 179);
            this.chkActive.Name = "chkActive";
            this.chkActive.Size = new Size(60, 19);
            this.chkActive.TabIndex = 3;
            this.chkActive.Text = "Active";

            // btnSave
            this.btnSave.BackColor = Color.Green;
            this.btnSave.ForeColor = Color.White;
            this.btnSave.Location = new Point(14, 208);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new Size(120, 30);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnClear
            this.btnClear.Location = new Point(144, 208);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new Size(120, 30);
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // btnDelete
            this.btnDelete.BackColor = Color.Red;
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Location = new Point(14, 248);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new Size(250, 30);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "Delete Selected";
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new EventHandler(this.btnDelete_Click);

            // dgvShifts
            this.dgvShifts.AllowUserToAddRows = false;
            this.dgvShifts.AllowUserToDeleteRows = false;
            this.dgvShifts.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
                | AnchorStyles.Left) 
                | AnchorStyles.Right)));
            this.dgvShifts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvShifts.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvShifts.Location = new Point(285, 62);
            this.dgvShifts.MultiSelect = false;
            this.dgvShifts.Name = "dgvShifts";
            this.dgvShifts.ReadOnly = true;
            this.dgvShifts.RowTemplate.Height = 25;
            this.dgvShifts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dgvShifts.Size = new Size(479, 430);
            this.dgvShifts.TabIndex = 7;
            this.dgvShifts.CellDoubleClick += new DataGridViewCellEventHandler(this.dgvShifts_CellDoubleClick);

            // ShiftManagementForm
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(776, 504);
            this.Controls.Add(this.dgvShifts);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.chkActive);
            this.Controls.Add(this.dtpEndTime);
            this.Controls.Add(this.dtpStartTime);
            this.Controls.Add(this.txtShiftName);
            this.Controls.Add(this.lblEndTime);
            this.Controls.Add(this.lblStartTime);
            this.Controls.Add(this.lblShiftName);
            this.Controls.Add(this.lblTitle);
            this.MinimumSize = new Size(700, 500);
            this.Name = "ShiftManagementForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Shift Management";

            ((System.ComponentModel.ISupportInitialize)(this.dgvShifts)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
} 