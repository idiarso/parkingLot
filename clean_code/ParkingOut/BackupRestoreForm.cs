using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Npgsql;
using System.Collections.Generic;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin.Forms
{
    public partial class BackupRestoreForm : Form
    {
        private readonly string backupDirectory;
        private string backupPath;
        private string dbName = "parkirdb";
        private string dbUser = "postgres";
        private string dbPassword = "root@rsi";
        private string dbHost = "localhost";
        private readonly IAppLogger _logger;
        
        public BackupRestoreForm()
        {
            _logger = new FileLogger();
            InitializeComponent();
            
            // Initialize backup directory
            backupDirectory = Path.Combine(Application.StartupPath, "Backups");
            if (!Directory.Exists(backupDirectory))
            {
                Directory.CreateDirectory(backupDirectory);
            }
            
            // Set default backup filename
            txtBackupFile.Text = Path.Combine(backupDirectory, $"parkingdb_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql");
            
            // Create backup directory
            backupPath = Path.Combine(Application.StartupPath, "Backup");
            if (!Directory.Exists(backupPath))
            {
                Directory.CreateDirectory(backupPath);
            }
        }
        
        private void BackupRestoreForm_Load(object sender, EventArgs e)
        {
            // Load existing backup files
            LoadBackupFiles();
            
            // Load database settings
            LoadDatabaseSettings();
        }
        
        private void LoadBackupFiles()
        {
            try
            {
                lbBackupFiles.Items.Clear();
                
                // Get all .sql files in backup directory
                string[] files = Directory.GetFiles(backupDirectory, "*.sql");
                
                // Sort by newest first
                Array.Sort(files, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));
                
                foreach (string file in files)
                {
                    lbBackupFiles.Items.Add(Path.GetFileName(file));
                }
                
                // Enable restore button only if there are backup files
                btnRestore.Enabled = lbBackupFiles.Items.Count > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading backup files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LoadDatabaseSettings()
        {
            try
            {
                // Try to load settings from Database class
                var connectionStringParts = GetConnectionStringParts();
                if (connectionStringParts.ContainsKey("Server"))
                    dbHost = connectionStringParts["Server"];
                
                if (connectionStringParts.ContainsKey("Database"))
                    dbName = connectionStringParts["Database"];
                
                if (connectionStringParts.ContainsKey("Uid"))
                    dbUser = connectionStringParts["Uid"];
                
                if (connectionStringParts.ContainsKey("Pwd"))
                    dbPassword = connectionStringParts["Pwd"];
                
                // Update UI
                txtDbName.Text = dbName;
                txtDbUser.Text = dbUser;
                txtDbPassword.Text = dbPassword;
                txtDbHost.Text = dbHost;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading database settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private Dictionary<string, string> GetConnectionStringParts()
        {
            // Helper method to parse connection string
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            // Use reflection to get connection string from Database class
            string connectionString = Database.GetConnectionString();
            
            if (string.IsNullOrEmpty(connectionString))
                return result;
            
            // Parse connection string
            foreach (string part in connectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(part))
                    continue;
                
                string[] keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    result[keyValue[0].Trim()] = keyValue[1].Trim();
                }
            }
            
            return result;
        }
        
        private void btnBrowseBackup_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*",
                DefaultExt = "sql",
                AddExtension = true,
                InitialDirectory = backupDirectory,
                FileName = $"parkingdb_backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql"
            };
            
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtBackupFile.Text = saveFileDialog.FileName;
            }
        }
        
        private void btnBrowseRestore_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*",
                InitialDirectory = backupDirectory
            };
            
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                txtRestoreFile.Text = openFileDialog.FileName;
            }
        }
        
        private void btnBackup_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                
                // Update database settings
                dbName = txtDbName.Text.Trim();
                dbUser = txtDbUser.Text.Trim();
                dbPassword = txtDbPassword.Text.Trim();
                dbHost = txtDbHost.Text.Trim();
                
                // Create backup filename with timestamp
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string backupFileName = $"backup_{timestamp}.sql";
                string backupFilePath = Path.Combine(backupPath, backupFileName);
                
                // Execute MySQL dump command
                Process process = new Process();
                process.StartInfo.FileName = "mysqldump";
                process.StartInfo.Arguments = $"--host={dbHost} --user={dbUser} --password={dbPassword} --databases {dbName} --result-file=\"{backupFilePath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                
                if (string.IsNullOrEmpty(dbPassword))
                {
                    process.StartInfo.Arguments = $"--host={dbHost} --user={dbUser} --databases {dbName} --result-file=\"{backupFilePath}\"";
                }
                
                process.Start();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Error executing mysqldump. Exit code: {process.ExitCode}. Error: {error}");
                }
                
                // Refresh backup list
                LoadBackupFiles();
                
                MessageBox.Show($"Database backup completed successfully. File saved to: {backupFilePath}", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        
        private void btnRestore_Click(object sender, EventArgs e)
        {
            try
            {
                if (lbBackupFiles.SelectedItem == null)
                {
                    MessageBox.Show("Please select a backup to restore.", "No Backup Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                string backupFile = Path.Combine(backupDirectory, lbBackupFiles.SelectedItem.ToString());
                string backupName = Path.GetFileNameWithoutExtension(backupFile);
                
                DialogResult result = MessageBox.Show(
                    $"Are you sure you want to restore the database from backup '{backupName}'?\nThis will overwrite all current data in the database.",
                    "Confirm Restore",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);
                
                if (result == DialogResult.Yes)
                {
                    Cursor = Cursors.WaitCursor;
                    Application.DoEvents();
                    
                    // Update database settings
                    dbName = txtDbName.Text.Trim();
                    dbUser = txtDbUser.Text.Trim();
                    dbPassword = txtDbPassword.Text.Trim();
                    dbHost = txtDbHost.Text.Trim();
                    
                    // Execute MySQL restore command
                    Process process = new Process();
                    process.StartInfo.FileName = "mysql";
                    process.StartInfo.Arguments = $"--host={dbHost} --user={dbUser} --password={dbPassword} < \"{backupFile}\"";
                    process.StartInfo.UseShellExecute = true;
                    process.StartInfo.CreateNoWindow = false;
                    
                    if (string.IsNullOrEmpty(dbPassword))
                    {
                        process.StartInfo.Arguments = $"--host={dbHost} --user={dbUser} < \"{backupFile}\"";
                    }
                    
                    process.Start();
                    process.WaitForExit();
                    
                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Error executing MySQL restore. Exit code: {process.ExitCode}.");
                    }
                    
                    MessageBox.Show("Database restored successfully.", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error restoring backup: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        
        private void btnScheduleBackup_Click(object sender, EventArgs e)
        {
            try
            {
                // Schedule backup using Windows Task Scheduler
                if (rdoDaily.Checked)
                {
                    ScheduleBackup("DAILY", dtpScheduleTime.Value.ToString("HH:mm"));
                }
                else if (rdoWeekly.Checked)
                {
                    ScheduleBackup("WEEKLY", dtpScheduleTime.Value.ToString("HH:mm"));
                }
                else if (rdoMonthly.Checked)
                {
                    ScheduleBackup("MONTHLY", dtpScheduleTime.Value.ToString("HH:mm"));
                }
                
                MessageBox.Show("Backup terjadwal berhasil diatur.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat mengatur backup terjadwal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void ScheduleBackup(string frequency, string time)
        {
            // Get application path
            string appPath = Application.ExecutablePath;
            string backupPath = Path.Combine(backupDirectory, $"parkingdb_backup_%date:~-4,4%%date:~-7,2%%date:~-10,2%.sql");
            
            string taskName = "ParkingSystemBackup";
            
            // Create batch file for backup
            string batchFile = Path.Combine(backupDirectory, "backup.bat");
            File.WriteAllText(batchFile, $"@echo off\r\n" +
                                       $"echo Running scheduled backup for Parking System...\r\n" +
                                       $"echo Backup file: {backupPath}\r\n" +
                                       $"\"{appPath}\" --backup \"{backupPath}\"\r\n" +
                                       $"echo Backup completed.\r\n");
            
            // Create task using schtasks command
            string command = $"schtasks /create /tn {taskName} /tr \"{batchFile}\" /sc {frequency} /st {time} /f";
            
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode != 0)
                {
                    throw new Exception($"Failed to schedule backup: {error}");
                }
            }
        }
        
        private void lbBackupFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lbBackupFiles.SelectedItem != null)
            {
                txtRestoreFile.Text = Path.Combine(backupDirectory, lbBackupFiles.SelectedItem.ToString());
            }
        }
        
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadBackupFiles();
        }
        
        private void btnDeleteBackup_Click(object sender, EventArgs e)
        {
            if (lbBackupFiles.SelectedItem == null)
            {
                MessageBox.Show("Silakan pilih file backup yang akan dihapus.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            string fileToDelete = Path.Combine(backupDirectory, lbBackupFiles.SelectedItem.ToString());
            
            DialogResult result = MessageBox.Show(
                $"Apakah Anda yakin ingin menghapus file backup ini?\n{lbBackupFiles.SelectedItem}",
                "Konfirmasi Hapus",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            
            if (result == DialogResult.Yes)
            {
                try
                {
                    File.Delete(fileToDelete);
                    MessageBox.Show("File backup berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadBackupFiles();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saat menghapus file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Designer-generated code
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabBackup = new System.Windows.Forms.TabPage();
            this.tabRestore = new System.Windows.Forms.TabPage();
            this.tabSchedule = new System.Windows.Forms.TabPage();
            this.txtBackupFile = new System.Windows.Forms.TextBox();
            this.btnBrowseBackup = new System.Windows.Forms.Button();
            this.btnBackup = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtRestoreFile = new System.Windows.Forms.TextBox();
            this.btnBrowseRestore = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.lbBackupFiles = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.btnDeleteBackup = new System.Windows.Forms.Button();
            this.rdoDaily = new System.Windows.Forms.RadioButton();
            this.rdoWeekly = new System.Windows.Forms.RadioButton();
            this.rdoMonthly = new System.Windows.Forms.RadioButton();
            this.dtpScheduleTime = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.btnScheduleBackup = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.txtDbName = new System.Windows.Forms.TextBox();
            this.txtDbUser = new System.Windows.Forms.TextBox();
            this.txtDbPassword = new System.Windows.Forms.TextBox();
            this.txtDbHost = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabBackup.SuspendLayout();
            this.tabRestore.SuspendLayout();
            this.tabSchedule.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabBackup);
            this.tabControl1.Controls.Add(this.tabRestore);
            this.tabControl1.Controls.Add(this.tabSchedule);
            this.tabControl1.Location = new System.Drawing.Point(12, 46);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(560, 303);
            this.tabControl1.TabIndex = 0;
            // 
            // tabBackup
            // 
            this.tabBackup.Controls.Add(this.btnBackup);
            this.tabBackup.Controls.Add(this.btnBrowseBackup);
            this.tabBackup.Controls.Add(this.txtBackupFile);
            this.tabBackup.Controls.Add(this.label1);
            this.tabBackup.Location = new System.Drawing.Point(4, 24);
            this.tabBackup.Name = "tabBackup";
            this.tabBackup.Padding = new System.Windows.Forms.Padding(3);
            this.tabBackup.Size = new System.Drawing.Size(552, 275);
            this.tabBackup.TabIndex = 0;
            this.tabBackup.Text = "Backup";
            this.tabBackup.UseVisualStyleBackColor = true;
            // 
            // tabRestore
            // 
            this.tabRestore.Controls.Add(this.btnDeleteBackup);
            this.tabRestore.Controls.Add(this.btnRefresh);
            this.tabRestore.Controls.Add(this.label3);
            this.tabRestore.Controls.Add(this.lbBackupFiles);
            this.tabRestore.Controls.Add(this.btnRestore);
            this.tabRestore.Controls.Add(this.btnBrowseRestore);
            this.tabRestore.Controls.Add(this.txtRestoreFile);
            this.tabRestore.Controls.Add(this.label2);
            this.tabRestore.Location = new System.Drawing.Point(4, 24);
            this.tabRestore.Name = "tabRestore";
            this.tabRestore.Padding = new System.Windows.Forms.Padding(3);
            this.tabRestore.Size = new System.Drawing.Size(552, 275);
            this.tabRestore.TabIndex = 1;
            this.tabRestore.Text = "Restore";
            this.tabRestore.UseVisualStyleBackColor = true;
            // 
            // tabSchedule
            // 
            this.tabSchedule.Controls.Add(this.btnScheduleBackup);
            this.tabSchedule.Controls.Add(this.label4);
            this.tabSchedule.Controls.Add(this.dtpScheduleTime);
            this.tabSchedule.Controls.Add(this.rdoMonthly);
            this.tabSchedule.Controls.Add(this.rdoWeekly);
            this.tabSchedule.Controls.Add(this.rdoDaily);
            this.tabSchedule.Location = new System.Drawing.Point(4, 24);
            this.tabSchedule.Name = "tabSchedule";
            this.tabSchedule.Size = new System.Drawing.Size(552, 275);
            this.tabSchedule.TabIndex = 2;
            this.tabSchedule.Text = "Backup Terjadwal";
            this.tabSchedule.UseVisualStyleBackColor = true;
            // 
            // txtBackupFile
            // 
            this.txtBackupFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBackupFile.Location = new System.Drawing.Point(15, 34);
            this.txtBackupFile.Name = "txtBackupFile";
            this.txtBackupFile.Size = new System.Drawing.Size(450, 23);
            this.txtBackupFile.TabIndex = 0;
            // 
            // btnBrowseBackup
            // 
            this.btnBrowseBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseBackup.Location = new System.Drawing.Point(471, 34);
            this.btnBrowseBackup.Name = "btnBrowseBackup";
            this.btnBrowseBackup.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseBackup.TabIndex = 1;
            this.btnBrowseBackup.Text = "Browse...";
            this.btnBrowseBackup.UseVisualStyleBackColor = true;
            this.btnBrowseBackup.Click += new System.EventHandler(this.btnBrowseBackup_Click);
            // 
            // btnBackup
            // 
            this.btnBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBackup.Location = new System.Drawing.Point(471, 63);
            this.btnBackup.Name = "btnBackup";
            this.btnBackup.Size = new System.Drawing.Size(75, 23);
            this.btnBackup.TabIndex = 2;
            this.btnBackup.Text = "Backup";
            this.btnBackup.UseVisualStyleBackColor = true;
            this.btnBackup.Click += new System.EventHandler(this.btnBackup_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "File Backup (SQL):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "File Restore (SQL):";
            // 
            // txtRestoreFile
            // 
            this.txtRestoreFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRestoreFile.Location = new System.Drawing.Point(15, 34);
            this.txtRestoreFile.Name = "txtRestoreFile";
            this.txtRestoreFile.Size = new System.Drawing.Size(450, 23);
            this.txtRestoreFile.TabIndex = 5;
            // 
            // btnBrowseRestore
            // 
            this.btnBrowseRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseRestore.Location = new System.Drawing.Point(471, 34);
            this.btnBrowseRestore.Name = "btnBrowseRestore";
            this.btnBrowseRestore.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseRestore.TabIndex = 6;
            this.btnBrowseRestore.Text = "Browse...";
            this.btnBrowseRestore.UseVisualStyleBackColor = true;
            this.btnBrowseRestore.Click += new System.EventHandler(this.btnBrowseRestore_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestore.Location = new System.Drawing.Point(471, 63);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(75, 23);
            this.btnRestore.TabIndex = 7;
            this.btnRestore.Text = "Restore";
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // lbBackupFiles
            // 
            this.lbBackupFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbBackupFiles.FormattingEnabled = true;
            this.lbBackupFiles.ItemHeight = 15;
            this.lbBackupFiles.Location = new System.Drawing.Point(15, 107);
            this.lbBackupFiles.Name = "lbBackupFiles";
            this.lbBackupFiles.Size = new System.Drawing.Size(450, 154);
            this.lbBackupFiles.TabIndex = 8;
            this.lbBackupFiles.SelectedIndexChanged += new System.EventHandler(this.lbBackupFiles_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 15);
            this.label3.TabIndex = 9;
            this.label3.Text = "File Backup Tersedia:";
            // 
            // btnRefresh
            // 
            this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRefresh.Location = new System.Drawing.Point(471, 107);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(75, 23);
            this.btnRefresh.TabIndex = 10;
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnDeleteBackup
            // 
            this.btnDeleteBackup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteBackup.Location = new System.Drawing.Point(471, 136);
            this.btnDeleteBackup.Name = "btnDeleteBackup";
            this.btnDeleteBackup.Size = new System.Drawing.Size(75, 23);
            this.btnDeleteBackup.TabIndex = 11;
            this.btnDeleteBackup.Text = "Hapus";
            this.btnDeleteBackup.UseVisualStyleBackColor = true;
            this.btnDeleteBackup.Click += new System.EventHandler(this.btnDeleteBackup_Click);
            // 
            // rdoDaily
            // 
            this.rdoDaily.AutoSize = true;
            this.rdoDaily.Checked = true;
            this.rdoDaily.Location = new System.Drawing.Point(20, 20);
            this.rdoDaily.Name = "rdoDaily";
            this.rdoDaily.Size = new System.Drawing.Size(85, 19);
            this.rdoDaily.TabIndex = 0;
            this.rdoDaily.TabStop = true;
            this.rdoDaily.Text = "Setiap Hari";
            this.rdoDaily.UseVisualStyleBackColor = true;
            // 
            // rdoWeekly
            // 
            this.rdoWeekly.AutoSize = true;
            this.rdoWeekly.Location = new System.Drawing.Point(20, 45);
            this.rdoWeekly.Name = "rdoWeekly";
            this.rdoWeekly.Size = new System.Drawing.Size(106, 19);
            this.rdoWeekly.TabIndex = 1;
            this.rdoWeekly.Text = "Setiap Minggu";
            this.rdoWeekly.UseVisualStyleBackColor = true;
            // 
            // rdoMonthly
            // 
            this.rdoMonthly.AutoSize = true;
            this.rdoMonthly.Location = new System.Drawing.Point(20, 70);
            this.rdoMonthly.Name = "rdoMonthly";
            this.rdoMonthly.Size = new System.Drawing.Size(94, 19);
            this.rdoMonthly.TabIndex = 2;
            this.rdoMonthly.Text = "Setiap Bulan";
            this.rdoMonthly.UseVisualStyleBackColor = true;
            // 
            // dtpScheduleTime
            // 
            this.dtpScheduleTime.CustomFormat = "HH:mm";
            this.dtpScheduleTime.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dtpScheduleTime.Location = new System.Drawing.Point(105, 110);
            this.dtpScheduleTime.Name = "dtpScheduleTime";
            this.dtpScheduleTime.ShowUpDown = true;
            this.dtpScheduleTime.Size = new System.Drawing.Size(70, 23);
            this.dtpScheduleTime.TabIndex = 3;
            this.dtpScheduleTime.Value = new System.DateTime(2023, 1, 1, 23, 0, 0, 0);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 114);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(79, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Waktu Backup:";
            // 
            // btnScheduleBackup
            // 
            this.btnScheduleBackup.Location = new System.Drawing.Point(20, 150);
            this.btnScheduleBackup.Name = "btnScheduleBackup";
            this.btnScheduleBackup.Size = new System.Drawing.Size(155, 30);
            this.btnScheduleBackup.TabIndex = 5;
            this.btnScheduleBackup.Text = "Atur Backup Terjadwal";
            this.btnScheduleBackup.UseVisualStyleBackColor = true;
            this.btnScheduleBackup.Click += new System.EventHandler(this.btnScheduleBackup_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.panel1.Controls.Add(this.label5);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 40);
            this.panel1.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(12, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(213, 21);
            this.label5.TabIndex = 0;
            this.label5.Text = "Backup dan Restore Database";
            // 
            // txtDbName
            // 
            this.txtDbName.Location = new System.Drawing.Point(15, 34);
            this.txtDbName.Name = "txtDbName";
            this.txtDbName.Size = new System.Drawing.Size(450, 23);
            this.txtDbName.TabIndex = 12;
            // 
            // txtDbUser
            // 
            this.txtDbUser.Location = new System.Drawing.Point(15, 63);
            this.txtDbUser.Name = "txtDbUser";
            this.txtDbUser.Size = new System.Drawing.Size(450, 23);
            this.txtDbUser.TabIndex = 13;
            // 
            // txtDbPassword
            // 
            this.txtDbPassword.Location = new System.Drawing.Point(15, 92);
            this.txtDbPassword.Name = "txtDbPassword";
            this.txtDbPassword.Size = new System.Drawing.Size(450, 23);
            this.txtDbPassword.TabIndex = 14;
            // 
            // txtDbHost
            // 
            this.txtDbHost.Location = new System.Drawing.Point(15, 121);
            this.txtDbHost.Name = "txtDbHost";
            this.txtDbHost.Size = new System.Drawing.Size(450, 23);
            this.txtDbHost.TabIndex = 15;
            // 
            // BackupRestoreForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 361);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tabControl1);
            this.Name = "BackupRestoreForm";
            this.Text = "Backup dan Restore Database";
            this.Load += new System.EventHandler(this.BackupRestoreForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabBackup.ResumeLayout(false);
            this.tabBackup.PerformLayout();
            this.tabRestore.ResumeLayout(false);
            this.tabRestore.PerformLayout();
            this.tabSchedule.ResumeLayout(false);
            this.tabSchedule.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabBackup;
        private System.Windows.Forms.TabPage tabRestore;
        private System.Windows.Forms.TabPage tabSchedule;
        private System.Windows.Forms.Button btnBackup;
        private System.Windows.Forms.Button btnBrowseBackup;
        private System.Windows.Forms.TextBox txtBackupFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDeleteBackup;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListBox lbBackupFiles;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnBrowseRestore;
        private System.Windows.Forms.TextBox txtRestoreFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnScheduleBackup;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dtpScheduleTime;
        private System.Windows.Forms.RadioButton rdoMonthly;
        private System.Windows.Forms.RadioButton rdoWeekly;
        private System.Windows.Forms.RadioButton rdoDaily;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtDbName;
        private System.Windows.Forms.TextBox txtDbUser;
        private System.Windows.Forms.TextBox txtDbPassword;
        private System.Windows.Forms.TextBox txtDbHost;
    }
} 