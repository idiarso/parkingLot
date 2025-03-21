using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.Threading;
using System.IO;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public partial class NotificationForm : Form
    {
        private System.Windows.Forms.Timer refreshTimer;
        private int refreshInterval = 60000; // 1 menit
        private int longParkingThreshold = 120; // 2 jam dalam menit
        private int criticalCapacity = 90; // 90% kapasitas
        private int warningCapacity = 75; // 75% kapasitas
        private readonly SoundPlayer alertSound;
        private readonly string logPath;
        private CheckBox chkEmailNotif;
        private CheckBox chkSmsNotif;
        private TextBox txtAdminEmail;
        private TextBox txtAdminPhone;
        private Button btnSaveNotifSettings;
        private DataGridView dgvNotifications;

        public NotificationForm()
        {
            InitializeComponent();
            InitializeNotificationsGrid();

            // Initialize log path
            logPath = Path.Combine(Application.StartupPath, "logs", "notification_log.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(logPath));

            // Initialize sound alert
            string soundPath = Path.Combine(Application.StartupPath, "Sounds", "alert.wav");
            try
            {
                if (File.Exists(soundPath))
                {
                    alertSound = new SoundPlayer(soundPath);
                }
                else
                {
                    // Menggunakan suara bawaan sistem
                    alertSound = new SoundPlayer(SystemSounds.Exclamation.ToString());
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error initializing sound alert: {ex.Message}");
            }

            // Initialize timer
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = refreshInterval;
            refreshTimer.Tick += RefreshTimer_Tick;
        }

        private void NotificationForm_Load(object sender, EventArgs e)
        {
            // Set up the DataGridView
            SetupDataGridView();

            // Load notification settings from config
            LoadNotificationSettings();

            // Initial check
            CheckLongParkedVehicles();
            CheckParkingCapacity();

            // Start the timer
            refreshTimer.Start();
        }

        private void NotificationForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Stop the timer
            refreshTimer.Stop();
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            // Check for long parked vehicles
            CheckLongParkedVehicles();

            // Check parking capacity
            CheckParkingCapacity();
        }

        private void SetupDataGridView()
        {
            // Configure the DataGridView for notifications
            dgvNotifications.AutoGenerateColumns = false;
            
            // Add columns
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTime",
                HeaderText = "Waktu",
                DataPropertyName = "NotificationTime",
                Width = 150
            });
            
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "Tipe",
                DataPropertyName = "NotificationType",
                Width = 100
            });
            
            dgvNotifications.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMessage",
                HeaderText = "Pesan",
                DataPropertyName = "Message",
                Width = 350
            });
            
            // Style
            dgvNotifications.RowHeadersVisible = false;
            dgvNotifications.AllowUserToAddRows = false;
            dgvNotifications.AllowUserToDeleteRows = false;
            dgvNotifications.AllowUserToResizeRows = false;
            dgvNotifications.ReadOnly = true;
            dgvNotifications.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvNotifications.MultiSelect = false;
            dgvNotifications.BackgroundColor = Color.White;
            dgvNotifications.BorderStyle = BorderStyle.None;
            dgvNotifications.RowTemplate.Height = 25;
        }

        private void LoadNotificationSettings()
        {
            try
            {
                // Load settings from database or config file
                string query = "SELECT value FROM settings WHERE setting_key = 'long_parking_threshold'";
                object result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int threshold))
                {
                    longParkingThreshold = threshold;
                }

                query = "SELECT value FROM settings WHERE setting_key = 'critical_capacity'";
                result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int critical))
                {
                    criticalCapacity = critical;
                }

                query = "SELECT value FROM settings WHERE setting_key = 'warning_capacity'";
                result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int warning))
                {
                    warningCapacity = warning;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading notification settings: {ex.Message}");
            }
        }

        private void CheckLongParkedVehicles()
        {
            try
            {
                // Query to find vehicles parked for longer than threshold
                string query = $@"
                    SELECT 
                        id, 
                        nomor_polisi, 
                        jenis_kendaraan, 
                        waktu_masuk, 
                        TIMESTAMPDIFF(MINUTE, waktu_masuk, NOW()) as durasi_menit
                    FROM 
                        t_parkir
                    WHERE 
                        waktu_keluar IS NULL
                        AND TIMESTAMPDIFF(MINUTE, waktu_masuk, NOW()) > {longParkingThreshold}
                    ORDER BY 
                        waktu_masuk ASC";

                DataTable longParkedVehicles = Database.ExecuteQuery(query);

                if (longParkedVehicles.Rows.Count > 0)
                {
                    foreach (DataRow row in longParkedVehicles.Rows)
                    {
                        string plate = row["nomor_polisi"].ToString();
                        string vehicleType = row["jenis_kendaraan"].ToString();
                        DateTime entryTime = Convert.ToDateTime(row["waktu_masuk"]);
                        int durationMinutes = Convert.ToInt32(row["durasi_menit"]);

                        // Format duration in hours and minutes
                        int hours = durationMinutes / 60;
                        int minutes = durationMinutes % 60;
                        string durationText = $"{hours} jam {minutes} menit";

                        // Check if this notification already exists
                        bool alreadyNotified = false;
                        foreach (DataGridViewRow notifRow in dgvNotifications.Rows)
                        {
                            if (notifRow.Cells["colMessage"].Value.ToString().Contains(plate) &&
                                notifRow.Cells["colType"].Value.ToString() == "Parkir Lama")
                            {
                                alreadyNotified = true;
                                break;
                            }
                        }

                        // Only add if not already notified
                        if (!alreadyNotified)
                        {
                            string message = $"Kendaraan {vehicleType} dengan plat nomor {plate} telah parkir selama {durationText}";
                            AddNotification("Parkir Lama", message);
                            PlayAlertSound();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error checking long parked vehicles: {ex.Message}");
            }
        }

        private void CheckParkingCapacity()
        {
            try
            {
                // Query to get parking capacity
                int totalCapacity = 0;
                try
                {
                    string capacityQuery = "SELECT value FROM settings WHERE setting_key = 'total_parking_capacity'";
                    object result = Database.ExecuteScalar(capacityQuery);
                    if (result != null && int.TryParse(result.ToString(), out int capacity))
                    {
                        totalCapacity = capacity;
                    }
                    else
                    {
                        // Default value if not set
                        totalCapacity = 100;
                    }
                }
                catch
                {
                    totalCapacity = 100;
                }

                // Query to count current parked vehicles
                string countQuery = "SELECT COUNT(*) FROM t_parkir WHERE waktu_keluar IS NULL";
                int currentCount = Convert.ToInt32(Database.ExecuteScalar(countQuery));

                // Calculate percentage
                int percentageFull = (int)((float)currentCount / totalCapacity * 100);

                // Check thresholds
                if (percentageFull >= criticalCapacity)
                {
                    // Critical capacity reached
                    string message = $"Kapasitas parkir hampir penuh! {currentCount}/{totalCapacity} slot terisi ({percentageFull}%)";
                    
                    // Only notify if this is a new critical notification
                    bool alreadyNotified = false;
                    foreach (DataGridViewRow row in dgvNotifications.Rows)
                    {
                        if (row.Cells["colType"].Value.ToString() == "Kapasitas Kritis")
                        {
                            // Already notified, update it
                            row.Cells["colTime"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            row.Cells["colMessage"].Value = message;
                            alreadyNotified = true;
                            break;
                        }
                    }

                    if (!alreadyNotified)
                    {
                        AddNotification("Kapasitas Kritis", message);
                        PlayAlertSound();
                    }
                }
                else if (percentageFull >= warningCapacity)
                {
                    // Warning capacity reached
                    string message = $"Kapasitas parkir mencapai {percentageFull}%. {currentCount}/{totalCapacity} slot terisi.";
                    
                    // Check if already notified
                    bool alreadyNotified = false;
                    foreach (DataGridViewRow row in dgvNotifications.Rows)
                    {
                        if (row.Cells["colType"].Value.ToString() == "Kapasitas Peringatan")
                        {
                            // Already notified, update it
                            row.Cells["colTime"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                            row.Cells["colMessage"].Value = message;
                            alreadyNotified = true;
                            break;
                        }
                    }

                    if (!alreadyNotified)
                    {
                        AddNotification("Kapasitas Peringatan", message);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error checking parking capacity: {ex.Message}");
            }
        }

        private void AddNotification(string type, string message)
        {
            try
            {
                // Add notification to DataGridView
                int rowIndex = dgvNotifications.Rows.Add();
                DataGridViewRow row = dgvNotifications.Rows[rowIndex];
                row.Cells["colTime"].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
                row.Cells["colType"].Value = type;
                row.Cells["colMessage"].Value = message;

                // Set row color based on type
                if (type == "Kapasitas Kritis")
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                else if (type == "Parkir Lama")
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                }
                else if (type == "Kapasitas Peringatan")
                {
                    row.DefaultCellStyle.BackColor = Color.LightSalmon;
                }

                // Log notification
                LogMessage($"[{type}] {message}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error adding notification: {ex.Message}");
            }
        }

        private void PlayAlertSound()
        {
            try
            {
                if (alertSound != null)
                {
                    alertSound.Play();
                }
                else
                {
                    SystemSounds.Exclamation.Play();
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error playing alert sound: {ex.Message}");
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                // Log to file
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
                File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            // Open notification settings form
            using (NotificationSettingsForm settingsForm = new NotificationSettingsForm())
            {
                if (settingsForm.ShowDialog() == DialogResult.OK)
                {
                    // Reload settings
                    LoadNotificationSettings();
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // Clear notifications
            dgvNotifications.Rows.Clear();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            // Manual refresh
            CheckLongParkedVehicles();
            CheckParkingCapacity();
        }

        private bool SendEmailNotification(string subject, string body, string recipient)
        {
            try
            {
                // Cek jika pengaturan email sudah dikonfigurasi
                string smtpServer = GetSetting("smtp_server", "");
                int smtpPort = Convert.ToInt32(GetSetting("smtp_port", "587"));
                string smtpUsername = GetSetting("smtp_username", "");
                string smtpPassword = GetSetting("smtp_password", "");
                string senderEmail = GetSetting("sender_email", "");
                
                if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpUsername) || 
                    string.IsNullOrEmpty(smtpPassword) || string.IsNullOrEmpty(senderEmail))
                {
                    Logger.Warning("Email settings not configured properly");
                    return false;
                }
                
                // Implementasi sederhana menggunakan System.Net.Mail
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(recipient);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = false;
                
                SmtpClient smtp = new SmtpClient(smtpServer, smtpPort);
                smtp.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtp.EnableSsl = true;
                
                // Kirim email secara asinkron untuk menghindari blocking UI
                Task.Run(() => {
                    try
                    {
                        smtp.Send(mail);
                        Logger.Info($"Email notification sent to {recipient}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Failed to send email: {ex.Message}");
                        return false;
                    }
                });
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending email notification: {ex.Message}");
                return false;
            }
        }

        private bool SendSmsNotification(string message, string phoneNumber)
        {
            try
            {
                // Cek jika pengaturan SMS sudah dikonfigurasi
                string smsApiKey = GetSetting("sms_api_key", "");
                string smsApiUrl = GetSetting("sms_api_url", "");
                
                if (string.IsNullOrEmpty(smsApiKey) || string.IsNullOrEmpty(smsApiUrl))
                {
                    Logger.Warning("SMS settings not configured properly");
                    return false;
                }
                
                // Implementasi sederhana menggunakan HttpClient (pastikan phoneNumber sudah dalam format yang benar)
                string formattedPhone = phoneNumber.StartsWith("+") ? phoneNumber : "+62" + phoneNumber.TrimStart('0');
                
                // Kirim SMS secara asinkron untuk menghindari blocking UI
                Task.Run(() => {
                    try
                    {
                        using (var client = new System.Net.Http.HttpClient())
                        {
                            var values = new Dictionary<string, string>
                            {
                                { "api_key", smsApiKey },
                                { "to", formattedPhone },
                                { "message", message }
                            };
                            
                            var content = new System.Net.Http.FormUrlEncodedContent(values);
                            var response = client.PostAsync(smsApiUrl, content).Result;
                            
                            if (response.IsSuccessStatusCode)
                            {
                                Logger.Info($"SMS notification sent to {phoneNumber}");
                                return true;
                            }
                            else
                            {
                                Logger.Warning($"Failed to send SMS: {response.StatusCode}");
                                return false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error sending SMS notification: {ex.Message}");
                        return false;
                    }
                });
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error sending SMS notification: {ex.Message}");
                return false;
            }
        }

        private string GetSetting(string key, string defaultValue)
        {
            try
            {
                string query = $"SELECT value FROM settings WHERE setting_key = '{key}'";
                object result = Database.ExecuteScalar(query);
                
                if (result != null)
                {
                    return result.ToString();
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Error getting setting {key}: {ex.Message}");
            }
            
            return defaultValue;
        }

        private bool SaveSetting(string key, string value, string description = null)
        {
            try
            {
                // Cek apakah setting sudah ada
                string query = $"SELECT COUNT(*) FROM settings WHERE setting_key = '{key}'";
                int count = Convert.ToInt32(Database.ExecuteScalar(query));
                
                if (count > 0)
                {
                    // Update setting yang sudah ada
                    query = $"UPDATE settings SET value = '{value}'";
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        query += $", description = '{description}'";
                    }
                    
                    query += $" WHERE setting_key = '{key}'";
                }
                else
                {
                    // Tambahkan setting baru
                    query = $"INSERT INTO settings (setting_key, value";
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        query += ", description";
                    }
                    
                    query += $") VALUES ('{key}', '{value}'";
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        query += $", '{description}'";
                    }
                    
                    query += ")";
                }
                
                Database.ExecuteNonQuery(query);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error saving setting {key}: {ex.Message}");
                return false;
            }
        }

        private void ShowNotification(string title, string message, NotificationType type)
        {
            // Tampilkan notifikasi di UI seperti biasa
            // ... existing notification code ...
            
            // Jika notifikasi penting, kirimkan juga melalui email dan/atau SMS
            if (type == NotificationType.Critical || type == NotificationType.Warning)
            {
                if (chkEmailNotif.Checked && !string.IsNullOrEmpty(txtAdminEmail.Text))
                {
                    SendEmailNotification($"[{type}] {title}", message, txtAdminEmail.Text);
                }
                
                if (chkSmsNotif.Checked && !string.IsNullOrEmpty(txtAdminPhone.Text))
                {
                    SendSmsNotification($"[{type}] {title}: {message}", txtAdminPhone.Text);
                }
            }
        }

        private enum NotificationType
        {
            Info,
            Warning,
            Critical
        }

        private void InitializeComponent()
        {
            // ... existing initialization code ...
            
            // Panel untuk pengaturan notifikasi
            Panel pnlNotifSettings = new Panel();
            pnlNotifSettings.BorderStyle = BorderStyle.FixedSingle;
            pnlNotifSettings.Location = new Point(12, 300);
            pnlNotifSettings.Size = new Size(400, 150);
            
            Label lblNotifSettings = new Label();
            lblNotifSettings.Text = "Pengaturan Notifikasi";
            lblNotifSettings.Font = new Font(lblNotifSettings.Font, FontStyle.Bold);
            lblNotifSettings.Location = new Point(10, 10);
            lblNotifSettings.AutoSize = true;
            
            this.chkEmailNotif = new CheckBox();
            this.chkEmailNotif.Text = "Kirim notifikasi via Email";
            this.chkEmailNotif.Location = new Point(10, 35);
            this.chkEmailNotif.AutoSize = true;
            this.chkEmailNotif.Checked = Convert.ToBoolean(GetSetting("email_notification_enabled", "false"));
            
            Label lblAdminEmail = new Label();
            lblAdminEmail.Text = "Email Admin:";
            lblAdminEmail.Location = new Point(30, 60);
            lblAdminEmail.AutoSize = true;
            
            this.txtAdminEmail = new TextBox();
            this.txtAdminEmail.Location = new Point(120, 57);
            this.txtAdminEmail.Size = new Size(250, 23);
            this.txtAdminEmail.Text = GetSetting("admin_email", "");
            
            this.chkSmsNotif = new CheckBox();
            this.chkSmsNotif.Text = "Kirim notifikasi via SMS";
            this.chkSmsNotif.Location = new Point(10, 85);
            this.chkSmsNotif.AutoSize = true;
            this.chkSmsNotif.Checked = Convert.ToBoolean(GetSetting("sms_notification_enabled", "false"));
            
            Label lblAdminPhone = new Label();
            lblAdminPhone.Text = "No. HP Admin:";
            lblAdminPhone.Location = new Point(30, 110);
            lblAdminPhone.AutoSize = true;
            
            this.txtAdminPhone = new TextBox();
            this.txtAdminPhone.Location = new Point(120, 107);
            this.txtAdminPhone.Size = new Size(150, 23);
            this.txtAdminPhone.Text = GetSetting("admin_phone", "");
            
            this.btnSaveNotifSettings = new Button();
            this.btnSaveNotifSettings.Text = "Simpan";
            this.btnSaveNotifSettings.Location = new Point(300, 107);
            this.btnSaveNotifSettings.Size = new Size(70, 23);
            this.btnSaveNotifSettings.Click += new EventHandler(this.btnSaveNotifSettings_Click);
            
            pnlNotifSettings.Controls.Add(lblNotifSettings);
            pnlNotifSettings.Controls.Add(this.chkEmailNotif);
            pnlNotifSettings.Controls.Add(lblAdminEmail);
            pnlNotifSettings.Controls.Add(this.txtAdminEmail);
            pnlNotifSettings.Controls.Add(this.chkSmsNotif);
            pnlNotifSettings.Controls.Add(lblAdminPhone);
            pnlNotifSettings.Controls.Add(this.txtAdminPhone);
            pnlNotifSettings.Controls.Add(this.btnSaveNotifSettings);
            
            this.Controls.Add(pnlNotifSettings);
            
            // ... existing code ...
        }

        private void btnSaveNotifSettings_Click(object sender, EventArgs e)
        {
            try
            {
                // Simpan pengaturan notifikasi
                SaveSetting("email_notification_enabled", chkEmailNotif.Checked.ToString().ToLower());
                SaveSetting("admin_email", txtAdminEmail.Text);
                SaveSetting("sms_notification_enabled", chkSmsNotif.Checked.ToString().ToLower());
                SaveSetting("admin_phone", txtAdminPhone.Text);
                
                MessageBox.Show("Pengaturan notifikasi berhasil disimpan.", 
                    "Simpan Pengaturan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menyimpan pengaturan: {ex.Message}", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CheckLongParking()
        {
            try
            {
                // Get long parked vehicles
                string query = $@"
                    SELECT 
                        id, 
                        nomor_polisi, 
                        jenis_kendaraan, 
                        waktu_masuk,
                        TIMESTAMPDIFF(MINUTE, waktu_masuk, NOW()) AS durasi_menit
                    FROM 
                        t_parkir
                    WHERE 
                        waktu_keluar IS NULL AND
                        TIMESTAMPDIFF(MINUTE, waktu_masuk, NOW()) > {longParkingThreshold}
                    ORDER BY 
                        waktu_masuk ASC";
                
                DataTable longParkedVehicles = Database.ExecuteQuery(query);
                
                // Ubah cara menampilkan notifikasi dengan memanggil method baru
                if (longParkedVehicles.Rows.Count > 0)
                {
                    string message = $"Terdapat {longParkedVehicles.Rows.Count} kendaraan yang sudah parkir terlalu lama.";
                    ShowNotification("Parkir Terlalu Lama", message, NotificationType.Warning);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error checking long parking: {ex.Message}");
            }
        }

        private void InitializeNotificationsGrid()
        {
            dgvNotifications = new DataGridView();
            dgvNotifications.Dock = DockStyle.Fill;
            this.Controls.Add(dgvNotifications);
            
            // Configure grid columns
            dgvNotifications.Columns.Add("Time", "Time");
            dgvNotifications.Columns.Add("Message", "Message");
            dgvNotifications.Columns.Add("Type", "Type");
            
            // Set grid properties
            dgvNotifications.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvNotifications.AllowUserToAddRows = false;
            dgvNotifications.ReadOnly = true;
        }
    }

    public class NotificationSettingsForm : Form
    {
        private NumericUpDown nudLongParkingThreshold;
        private NumericUpDown nudCriticalCapacity;
        private NumericUpDown nudWarningCapacity;
        private NumericUpDown nudTotalCapacity;
        private Button btnSave;
        private Button btnCancel;
        private Label lblLongParking;
        private Label lblCritical;
        private Label lblWarning;
        private Label lblTotal;

        public NotificationSettingsForm()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.nudLongParkingThreshold = new System.Windows.Forms.NumericUpDown();
            this.nudCriticalCapacity = new System.Windows.Forms.NumericUpDown();
            this.nudWarningCapacity = new System.Windows.Forms.NumericUpDown();
            this.nudTotalCapacity = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblLongParking = new System.Windows.Forms.Label();
            this.lblCritical = new System.Windows.Forms.Label();
            this.lblWarning = new System.Windows.Forms.Label();
            this.lblTotal = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudLongParkingThreshold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCriticalCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWarningCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalCapacity)).BeginInit();
            this.SuspendLayout();
            // 
            // nudLongParkingThreshold
            // 
            this.nudLongParkingThreshold.Location = new System.Drawing.Point(248, 20);
            this.nudLongParkingThreshold.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            this.nudLongParkingThreshold.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            this.nudLongParkingThreshold.Name = "nudLongParkingThreshold";
            this.nudLongParkingThreshold.Size = new System.Drawing.Size(75, 23);
            this.nudLongParkingThreshold.TabIndex = 0;
            this.nudLongParkingThreshold.Value = new decimal(new int[] { 120, 0, 0, 0 });
            // 
            // nudCriticalCapacity
            // 
            this.nudCriticalCapacity.Location = new System.Drawing.Point(248, 54);
            this.nudCriticalCapacity.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
            this.nudCriticalCapacity.Minimum = new decimal(new int[] { 50, 0, 0, 0 });
            this.nudCriticalCapacity.Name = "nudCriticalCapacity";
            this.nudCriticalCapacity.Size = new System.Drawing.Size(75, 23);
            this.nudCriticalCapacity.TabIndex = 1;
            this.nudCriticalCapacity.Value = new decimal(new int[] { 90, 0, 0, 0 });
            // 
            // nudWarningCapacity
            // 
            this.nudWarningCapacity.Location = new System.Drawing.Point(248, 88);
            this.nudWarningCapacity.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            this.nudWarningCapacity.Minimum = new decimal(new int[] { 30, 0, 0, 0 });
            this.nudWarningCapacity.Name = "nudWarningCapacity";
            this.nudWarningCapacity.Size = new System.Drawing.Size(75, 23);
            this.nudWarningCapacity.TabIndex = 2;
            this.nudWarningCapacity.Value = new decimal(new int[] { 75, 0, 0, 0 });
            // 
            // nudTotalCapacity
            // 
            this.nudTotalCapacity.Location = new System.Drawing.Point(248, 122);
            this.nudTotalCapacity.Maximum = new decimal(new int[] { 10000, 0, 0, 0 });
            this.nudTotalCapacity.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            this.nudTotalCapacity.Name = "nudTotalCapacity";
            this.nudTotalCapacity.Size = new System.Drawing.Size(75, 23);
            this.nudTotalCapacity.TabIndex = 3;
            this.nudTotalCapacity.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(167, 169);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 23);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Simpan";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(248, 169);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Batal";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblLongParking
            // 
            this.lblLongParking.AutoSize = true;
            this.lblLongParking.Location = new System.Drawing.Point(12, 22);
            this.lblLongParking.Name = "lblLongParking";
            this.lblLongParking.Size = new System.Drawing.Size(212, 15);
            this.lblLongParking.TabIndex = 6;
            this.lblLongParking.Text = "Batas waktu parkir lama (dalam menit):";
            // 
            // lblCritical
            // 
            this.lblCritical.AutoSize = true;
            this.lblCritical.Location = new System.Drawing.Point(12, 56);
            this.lblCritical.Name = "lblCritical";
            this.lblCritical.Size = new System.Drawing.Size(178, 15);
            this.lblCritical.TabIndex = 7;
            this.lblCritical.Text = "Kapasitas kritis (dalam persen): ";
            // 
            // lblWarning
            // 
            this.lblWarning.AutoSize = true;
            this.lblWarning.Location = new System.Drawing.Point(12, 90);
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.Size = new System.Drawing.Size(198, 15);
            this.lblWarning.TabIndex = 8;
            this.lblWarning.Text = "Kapasitas peringatan (dalam persen):";
            // 
            // lblTotal
            // 
            this.lblTotal.AutoSize = true;
            this.lblTotal.Location = new System.Drawing.Point(12, 124);
            this.lblTotal.Name = "lblTotal";
            this.lblTotal.Size = new System.Drawing.Size(142, 15);
            this.lblTotal.TabIndex = 9;
            this.lblTotal.Text = "Kapasitas parkir total (slot):";
            // 
            // NotificationSettingsForm
            // 
            this.ClientSize = new System.Drawing.Size(336, 207);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.lblCritical);
            this.Controls.Add(this.lblLongParking);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.nudTotalCapacity);
            this.Controls.Add(this.nudWarningCapacity);
            this.Controls.Add(this.nudCriticalCapacity);
            this.Controls.Add(this.nudLongParkingThreshold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NotificationSettingsForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pengaturan Notifikasi";
            ((System.ComponentModel.ISupportInitialize)(this.nudLongParkingThreshold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudCriticalCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudWarningCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudTotalCapacity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadSettings()
        {
            try
            {
                // Load settings from database
                string query = "SELECT value FROM settings WHERE setting_key = 'long_parking_threshold'";
                object result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int threshold))
                {
                    nudLongParkingThreshold.Value = threshold;
                }

                query = "SELECT value FROM settings WHERE setting_key = 'critical_capacity'";
                result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int critical))
                {
                    nudCriticalCapacity.Value = critical;
                }

                query = "SELECT value FROM settings WHERE setting_key = 'warning_capacity'";
                result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int warning))
                {
                    nudWarningCapacity.Value = warning;
                }

                query = "SELECT value FROM settings WHERE setting_key = 'total_parking_capacity'";
                result = Database.ExecuteScalar(query);
                if (result != null && int.TryParse(result.ToString(), out int total))
                {
                    nudTotalCapacity.Value = total;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                SaveSettingValue("long_parking_threshold", nudLongParkingThreshold.Value.ToString());
                SaveSettingValue("critical_capacity", nudCriticalCapacity.Value.ToString());
                SaveSettingValue("warning_capacity", nudWarningCapacity.Value.ToString());
                SaveSettingValue("total_parking_capacity", nudTotalCapacity.Value.ToString());

                MessageBox.Show("Pengaturan notifikasi berhasil disimpan.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettingValue(string key, string value, string description = null)
        {
            try
            {
                // Cek apakah setting sudah ada
                string query = $"SELECT COUNT(*) FROM settings WHERE setting_key = '{key}'";
                int count = Convert.ToInt32(Database.ExecuteScalar(query));
                
                if (count > 0)
                {
                    // Update setting yang sudah ada
                    query = $"UPDATE settings SET value = '{value}'";
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        query += $", description = '{description}'";
                    }
                    
                    query += $" WHERE setting_key = '{key}'";
                }
                else
                {
                    // Tambahkan setting baru
                    query = $"INSERT INTO settings (setting_key, value";
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        query += ", description";
                    }
                    
                    query += $") VALUES ('{key}', '{value}'";
                    
                    if (!string.IsNullOrEmpty(description))
                    {
                        query += $", '{description}'";
                    }
                    
                    query += ")";
                }
                
                Database.ExecuteNonQuery(query);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving setting {key}: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Validate warning < critical
            if (nudWarningCapacity.Value >= nudCriticalCapacity.Value)
            {
                MessageBox.Show("Kapasitas peringatan harus lebih kecil dari kapasitas kritis.", 
                    "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveSettings();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
} 