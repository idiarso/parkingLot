using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using SimpleParkingAdmin.Utils;
using SimpleParkingAdmin.Models;

// Custom ProgressBar extension to support colors
public static class ModifyProgressBarColor
{
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
    static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr w, IntPtr l);
    
    public static void SetState(this ProgressBar pBar, int state)
    {
        SendMessage(pBar.Handle, 1040, (IntPtr)state, IntPtr.Zero);
    }
}

namespace SimpleParkingAdmin
{
    public partial class ParkingCapacityForm : Form
    {
        private int totalMotorCapacity = 100;
        private int totalCarCapacity = 50;
        private int usedMotorSlots = 0;
        private int usedCarSlots = 0;
        private System.Windows.Forms.Timer refreshTimer;
        
        public ParkingCapacityForm()
        {
            InitializeComponent();
            
            // Set up refresh timer
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000; // 30 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
        }
        
        private void ParkingCapacityForm_Load(object sender, EventArgs e)
        {
            // Load saved capacity settings
            LoadCapacitySettings();
            
            // Load current parking status
            RefreshParkingStatus();
            
            // Start timer
            refreshTimer.Start();
        }
        
        private void ParkingCapacityForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop timer
            refreshTimer.Stop();
        }
        
        private void LoadCapacitySettings()
        {
            try
            {
                // Try to load settings from database
                string motorQuery = "SELECT value FROM settings WHERE setting_key = 'motor_capacity'";
                string carQuery = "SELECT value FROM settings WHERE setting_key = 'car_capacity'";
                
                object motorResult = Database.ExecuteScalar(motorQuery);
                object carResult = Database.ExecuteScalar(carQuery);
                
                if (motorResult != null && int.TryParse(motorResult.ToString(), out int motorCapacity))
                {
                    totalMotorCapacity = motorCapacity;
                }
                
                if (carResult != null && int.TryParse(carResult.ToString(), out int carCapacity))
                {
                    totalCarCapacity = carCapacity;
                }
                
                // Update controls
                nudMotorCapacity.Value = totalMotorCapacity;
                nudCarCapacity.Value = totalCarCapacity;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading capacity settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void RefreshParkingStatus()
        {
            try
            {
                // Query for vehicle counts by type
                string query = @"
                    SELECT 
                        jenis_kendaraan, 
                        COUNT(*) as count
                    FROM 
                        t_parkir 
                    WHERE 
                        waktu_keluar IS NULL
                    GROUP BY 
                        jenis_kendaraan";
                
                DataTable result = Database.ExecuteQuery(query);
                
                // Reset counters
                usedMotorSlots = 0;
                usedCarSlots = 0;
                
                // Count vehicles by type
                foreach (DataRow row in result.Rows)
                {
                    string vehicleType = row["jenis_kendaraan"].ToString().ToLower();
                    int count = Convert.ToInt32(row["count"]);
                    
                    if (vehicleType.Contains("motor"))
                    {
                        usedMotorSlots += count;
                    }
                    else if (vehicleType.Contains("mobil") || vehicleType.Contains("car"))
                    {
                        usedCarSlots += count;
                    }
                    else
                    {
                        // Unknown type, add to cars as default
                        usedCarSlots += count;
                    }
                }
                
                // Update labels
                lblMotorUsed.Text = $"{usedMotorSlots} dari {totalMotorCapacity}";
                lblCarUsed.Text = $"{usedCarSlots} dari {totalCarCapacity}";
                
                // Calculate percentages
                int motorPercentage = (int)((double)usedMotorSlots / totalMotorCapacity * 100);
                int carPercentage = (int)((double)usedCarSlots / totalCarCapacity * 100);
                
                // Update progress bars
                UpdateProgressBar(pbarMotor, motorPercentage);
                UpdateProgressBar(pbarCar, carPercentage);
                
                // Update availability labels
                lblMotorAvailable.Text = $"{totalMotorCapacity - usedMotorSlots} slot tersedia";
                lblCarAvailable.Text = $"{totalCarCapacity - usedCarSlots} slot tersedia";
                
                // Update status text and color based on capacity
                SetCapacityStatusText(lblMotorStatus, motorPercentage);
                SetCapacityStatusText(lblCarStatus, carPercentage);
                
                // Update timestamp
                lblLastUpdate.Text = $"Terakhir diperbarui: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing parking status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void UpdateProgressBar(ProgressBar progressBar, int percentage)
        {
            progressBar.Value = Math.Min(percentage, 100);
            
            if (percentage < 75)
            {
                progressBar.SetState(1); // Green
            }
            else if (percentage < 90)
            {
                progressBar.SetState(3); // Yellow
            }
            else
            {
                progressBar.SetState(2); // Red
            }
        }
        
        private void SetCapacityStatusText(Label label, int percentage)
        {
            if (percentage < 75)
            {
                label.Text = "Tersedia";
                label.ForeColor = Color.Green;
            }
            else if (percentage < 90)
            {
                label.Text = "Hampir Penuh";
                label.ForeColor = Color.DarkOrange;
            }
            else
            {
                label.Text = "Penuh";
                label.ForeColor = Color.Red;
            }
        }
        
        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshParkingStatus();
        }
        
        private void btnSaveCapacity_Click(object sender, EventArgs e)
        {
            try
            {
                // Update capacity values
                totalMotorCapacity = (int)nudMotorCapacity.Value;
                totalCarCapacity = (int)nudCarCapacity.Value;
                
                // Save to database
                SaveCapacitySetting("motor_capacity", totalMotorCapacity.ToString());
                SaveCapacitySetting("car_capacity", totalCarCapacity.ToString());
                
                // Refresh parking status
                RefreshParkingStatus();
                
                MessageBox.Show("Kapasitas parkir berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving capacity settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void SaveCapacitySetting(string key, string value)
        {
            try
            {
                // Check if setting exists
                string checkQuery = $"SELECT COUNT(*) FROM settings WHERE setting_key = '{key}'";
                int count = Convert.ToInt32(Database.ExecuteScalar(checkQuery));
                
                string query;
                if (count > 0)
                {
                    // Update existing setting
                    query = $"UPDATE settings SET value = '{value}' WHERE setting_key = '{key}'";
                }
                else
                {
                    // Insert new setting
                    query = $"INSERT INTO settings (setting_key, value) VALUES ('{key}', '{value}')";
                }
                
                Database.ExecuteNonQuery(query);
            }
            catch
            {
                // Try to create settings table if it doesn't exist
                try
                {
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS settings (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            setting_key VARCHAR(50) NOT NULL UNIQUE,
                            value TEXT,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                            updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                        )";
                    
                    Database.ExecuteNonQuery(createTableQuery);
                    
                    // Try again
                    string query = $"INSERT INTO settings (setting_key, value) VALUES ('{key}', '{value}')";
                    Database.ExecuteNonQuery(query);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error creating settings table: {ex.Message}");
                }
            }
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshParkingStatus();
        }
        
        private void btnReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Apakah Anda yakin ingin mengatur ulang kapasitas parkir ke default (Motor: 100, Mobil: 50)?",
                "Konfirmasi Reset",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.Yes)
            {
                nudMotorCapacity.Value = 100;
                nudCarCapacity.Value = 50;
                btnSaveCapacity.PerformClick();
            }
        }

        #region Designer-generated code
        
        private Panel panel1;
        private Label lblTitle;
        private TabControl tabControl1;
        private TabPage tabStatus;
        private TabPage tabSettings;
        private Label lblMotorCapacity;
        private Label lblCarCapacity;
        private NumericUpDown nudMotorCapacity;
        private NumericUpDown nudCarCapacity;
        private Button btnSaveCapacity;
        private Label lblLastUpdate;
        private Button btnRefresh;
        private Button btnReset;
        private GroupBox grpMotor;
        private ProgressBar pbarMotor;
        private Label lblMotorUsed;
        private Label lblMotorStatus;
        private GroupBox grpCar;
        private ProgressBar pbarCar;
        private Label lblCarUsed;
        private Label lblCarStatus;
        private Label lblMotorAvailable;
        private Label lblCarAvailable;
        
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.lblLastUpdate = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.grpCar = new System.Windows.Forms.GroupBox();
            this.lblCarAvailable = new System.Windows.Forms.Label();
            this.lblCarStatus = new System.Windows.Forms.Label();
            this.lblCarUsed = new System.Windows.Forms.Label();
            this.pbarCar = new System.Windows.Forms.ProgressBar();
            this.grpMotor = new System.Windows.Forms.GroupBox();
            this.lblMotorAvailable = new System.Windows.Forms.Label();
            this.lblMotorStatus = new System.Windows.Forms.Label();
            this.lblMotorUsed = new System.Windows.Forms.Label();
            this.pbarMotor = new System.Windows.Forms.ProgressBar();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSaveCapacity = new System.Windows.Forms.Button();
            this.nudCarCapacity = new System.Windows.Forms.NumericUpDown();
            this.nudMotorCapacity = new System.Windows.Forms.NumericUpDown();
            this.lblCarCapacity = new System.Windows.Forms.Label();
            this.lblMotorCapacity = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabStatus.SuspendLayout();
            this.grpCar.SuspendLayout();
            this.grpMotor.SuspendLayout();
            this.tabSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCarCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotorCapacity)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panel1.Controls.Add(this.lblTitle);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(684, 40);
            this.panel1.TabIndex = 0;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(132, 21);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Kapasitas Parkir";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabStatus);
            this.tabControl1.Controls.Add(this.tabSettings);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 40);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(684, 371);
            this.tabControl1.TabIndex = 1;
            // 
            // tabStatus
            // 
            this.tabStatus.Controls.Add(this.lblLastUpdate);
            this.tabStatus.Controls.Add(this.btnRefresh);
            this.tabStatus.Controls.Add(this.grpCar);
            this.tabStatus.Controls.Add(this.grpMotor);
            this.tabStatus.Location = new System.Drawing.Point(4, 24);
            this.tabStatus.Name = "tabStatus";
            this.tabStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tabStatus.Size = new System.Drawing.Size(676, 343);
            this.tabStatus.TabIndex = 0;
            this.tabStatus.Text = "Status Parkir";
            this.tabStatus.UseVisualStyleBackColor = true;
            // 
            // lblLastUpdate
            // 
            this.lblLastUpdate.AutoSize = true;
            this.lblLastUpdate.Location = new System.Drawing.Point(8, 317);
            this.lblLastUpdate.Name = "lblLastUpdate";
            this.lblLastUpdate.Size = new System.Drawing.Size(219, 15);
            this.lblLastUpdate.TabIndex = 3;
            this.lblLastUpdate.Text = "Terakhir diperbarui: 00/00/0000 00:00:00";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(571, 312);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(97, 25);
            this.btnRefresh.TabIndex = 2;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // grpCar
            // 
            this.grpCar.Controls.Add(this.lblCarAvailable);
            this.grpCar.Controls.Add(this.lblCarStatus);
            this.grpCar.Controls.Add(this.lblCarUsed);
            this.grpCar.Controls.Add(this.pbarCar);
            this.grpCar.Location = new System.Drawing.Point(8, 171);
            this.grpCar.Name = "grpCar";
            this.grpCar.Size = new System.Drawing.Size(660, 138);
            this.grpCar.TabIndex = 1;
            this.grpCar.TabStop = false;
            this.grpCar.Text = "Mobil";
            // 
            // lblCarAvailable
            // 
            this.lblCarAvailable.AutoSize = true;
            this.lblCarAvailable.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCarAvailable.Location = new System.Drawing.Point(20, 95);
            this.lblCarAvailable.Name = "lblCarAvailable";
            this.lblCarAvailable.Size = new System.Drawing.Size(95, 17);
            this.lblCarAvailable.TabIndex = 3;
            this.lblCarAvailable.Text = "0 slot tersedia";
            // 
            // lblCarStatus
            // 
            this.lblCarStatus.AutoSize = true;
            this.lblCarStatus.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblCarStatus.ForeColor = System.Drawing.Color.Green;
            this.lblCarStatus.Location = new System.Drawing.Point(563, 63);
            this.lblCarStatus.Name = "lblCarStatus";
            this.lblCarStatus.Size = new System.Drawing.Size(71, 20);
            this.lblCarStatus.TabIndex = 2;
            this.lblCarStatus.Text = "Tersedia";
            // 
            // lblCarUsed
            // 
            this.lblCarUsed.AutoSize = true;
            this.lblCarUsed.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblCarUsed.Location = new System.Drawing.Point(20, 25);
            this.lblCarUsed.Name = "lblCarUsed";
            this.lblCarUsed.Size = new System.Drawing.Size(78, 17);
            this.lblCarUsed.TabIndex = 1;
            this.lblCarUsed.Text = "0 dari 0 slot";
            // 
            // pbarCar
            // 
            this.pbarCar.Location = new System.Drawing.Point(20, 45);
            this.pbarCar.Name = "pbarCar";
            this.pbarCar.Size = new System.Drawing.Size(620, 38);
            this.pbarCar.TabIndex = 0;
            // 
            // grpMotor
            // 
            this.grpMotor.Controls.Add(this.lblMotorAvailable);
            this.grpMotor.Controls.Add(this.lblMotorStatus);
            this.grpMotor.Controls.Add(this.lblMotorUsed);
            this.grpMotor.Controls.Add(this.pbarMotor);
            this.grpMotor.Location = new System.Drawing.Point(8, 17);
            this.grpMotor.Name = "grpMotor";
            this.grpMotor.Size = new System.Drawing.Size(660, 138);
            this.grpMotor.TabIndex = 0;
            this.grpMotor.TabStop = false;
            this.grpMotor.Text = "Motor";
            // 
            // lblMotorAvailable
            // 
            this.lblMotorAvailable.AutoSize = true;
            this.lblMotorAvailable.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMotorAvailable.Location = new System.Drawing.Point(20, 95);
            this.lblMotorAvailable.Name = "lblMotorAvailable";
            this.lblMotorAvailable.Size = new System.Drawing.Size(95, 17);
            this.lblMotorAvailable.TabIndex = 3;
            this.lblMotorAvailable.Text = "0 slot tersedia";
            // 
            // lblMotorStatus
            // 
            this.lblMotorStatus.AutoSize = true;
            this.lblMotorStatus.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblMotorStatus.ForeColor = System.Drawing.Color.Green;
            this.lblMotorStatus.Location = new System.Drawing.Point(563, 63);
            this.lblMotorStatus.Name = "lblMotorStatus";
            this.lblMotorStatus.Size = new System.Drawing.Size(71, 20);
            this.lblMotorStatus.TabIndex = 2;
            this.lblMotorStatus.Text = "Tersedia";
            // 
            // lblMotorUsed
            // 
            this.lblMotorUsed.AutoSize = true;
            this.lblMotorUsed.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblMotorUsed.Location = new System.Drawing.Point(20, 25);
            this.lblMotorUsed.Name = "lblMotorUsed";
            this.lblMotorUsed.Size = new System.Drawing.Size(78, 17);
            this.lblMotorUsed.TabIndex = 1;
            this.lblMotorUsed.Text = "0 dari 0 slot";
            // 
            // pbarMotor
            // 
            this.pbarMotor.Location = new System.Drawing.Point(20, 45);
            this.pbarMotor.Name = "pbarMotor";
            this.pbarMotor.Size = new System.Drawing.Size(620, 38);
            this.pbarMotor.TabIndex = 0;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.btnReset);
            this.tabSettings.Controls.Add(this.btnSaveCapacity);
            this.tabSettings.Controls.Add(this.nudCarCapacity);
            this.tabSettings.Controls.Add(this.nudMotorCapacity);
            this.tabSettings.Controls.Add(this.lblCarCapacity);
            this.tabSettings.Controls.Add(this.lblMotorCapacity);
            this.tabSettings.Location = new System.Drawing.Point(4, 24);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(676, 343);
            this.tabSettings.TabIndex = 1;
            this.tabSettings.Text = "Pengaturan Kapasitas";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(313, 128);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(100, 33);
            this.btnReset.TabIndex = 5;
            this.btnReset.Text = "Reset Default";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSaveCapacity
            // 
            this.btnSaveCapacity.Location = new System.Drawing.Point(207, 128);
            this.btnSaveCapacity.Name = "btnSaveCapacity";
            this.btnSaveCapacity.Size = new System.Drawing.Size(100, 33);
            this.btnSaveCapacity.TabIndex = 4;
            this.btnSaveCapacity.Text = "Simpan";
            this.btnSaveCapacity.UseVisualStyleBackColor = true;
            this.btnSaveCapacity.Click += new System.EventHandler(this.btnSaveCapacity_Click);
            // 
            // nudCarCapacity
            // 
            this.nudCarCapacity.Location = new System.Drawing.Point(207, 78);
            this.nudCarCapacity.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudCarCapacity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudCarCapacity.Name = "nudCarCapacity";
            this.nudCarCapacity.Size = new System.Drawing.Size(120, 23);
            this.nudCarCapacity.TabIndex = 3;
            this.nudCarCapacity.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // nudMotorCapacity
            // 
            this.nudMotorCapacity.Location = new System.Drawing.Point(207, 43);
            this.nudMotorCapacity.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudMotorCapacity.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudMotorCapacity.Name = "nudMotorCapacity";
            this.nudMotorCapacity.Size = new System.Drawing.Size(120, 23);
            this.nudMotorCapacity.TabIndex = 2;
            this.nudMotorCapacity.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // lblCarCapacity
            // 
            this.lblCarCapacity.AutoSize = true;
            this.lblCarCapacity.Location = new System.Drawing.Point(32, 80);
            this.lblCarCapacity.Name = "lblCarCapacity";
            this.lblCarCapacity.Size = new System.Drawing.Size(154, 15);
            this.lblCarCapacity.TabIndex = 1;
            this.lblCarCapacity.Text = "Kapasitas Parkir Mobil (slot):";
            // 
            // lblMotorCapacity
            // 
            this.lblMotorCapacity.AutoSize = true;
            this.lblMotorCapacity.Location = new System.Drawing.Point(32, 45);
            this.lblMotorCapacity.Name = "lblMotorCapacity";
            this.lblMotorCapacity.Size = new System.Drawing.Size(159, 15);
            this.lblMotorCapacity.TabIndex = 0;
            this.lblMotorCapacity.Text = "Kapasitas Parkir Motor (slot):";
            // 
            // ParkingCapacityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 411);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "ParkingCapacityForm";
            this.Text = "Kapasitas Parkir";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ParkingCapacityForm_FormClosing);
            this.Load += new System.EventHandler(this.ParkingCapacityForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabStatus.ResumeLayout(false);
            this.tabStatus.PerformLayout();
            this.grpCar.ResumeLayout(false);
            this.grpCar.PerformLayout();
            this.grpMotor.ResumeLayout(false);
            this.grpMotor.PerformLayout();
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudCarCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMotorCapacity)).EndInit();
            this.ResumeLayout(false);

        }
        
        #endregion
    }
} 