using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using ParkingIN.Utils;
using Npgsql;
using System.Collections.Generic;

namespace ParkingIN
{
    public partial class LogViewerForm : Form
    {
        private DataGridView dgvLogs;
        private ComboBox cmbFilter;
        private Button btnRefresh;
        private Button btnExport;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private System.Windows.Forms.Timer refreshTimer;
        private Label lblStatus;
        
        public LogViewerForm()
        {
            InitializeComponent();
            InitializeTimer();
            LoadInitialData();
        }

        private void InitializeComponent()
        {
            this.Text = "Realtime Log Viewer";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Create controls
            dgvLogs = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };

            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                Padding = new Padding(10)
            };

            cmbFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 150,
                Location = new Point(10, 20)
            };
            cmbFilter.Items.AddRange(new object[] { "All Actions", "Login", "Logout", "Create", "Update", "Delete" });
            cmbFilter.SelectedIndex = 0;
            cmbFilter.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;

            dtpStart = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Width = 150,
                Location = new Point(170, 20)
            };

            dtpEnd = new DateTimePicker
            {
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "yyyy-MM-dd HH:mm",
                Width = 150,
                Location = new Point(330, 20)
            };

            btnRefresh = new Button
            {
                Text = "Refresh",
                Width = 100,
                Location = new Point(490, 19),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += BtnRefresh_Click;

            btnExport = new Button
            {
                Text = "Export",
                Width = 100,
                Location = new Point(600, 19),
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnExport.FlatAppearance.BorderSize = 0;
            btnExport.Click += BtnExport_Click;

            lblStatus = new Label
            {
                AutoSize = true,
                Location = new Point(710, 23),
                ForeColor = Color.Gray
            };

            // Add controls
            topPanel.Controls.AddRange(new Control[] { 
                cmbFilter, dtpStart, dtpEnd, btnRefresh, btnExport, lblStatus 
            });

            this.Controls.Add(dgvLogs);
            this.Controls.Add(topPanel);

            // Set column headers
            dgvLogs.Columns.AddRange(new DataGridViewColumn[] {
                new DataGridViewTextBoxColumn { Name = "id", HeaderText = "ID", Width = 70 },
                new DataGridViewTextBoxColumn { Name = "timestamp", HeaderText = "Timestamp", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "username", HeaderText = "User", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "action", HeaderText = "Action", Width = 100 },
                new DataGridViewTextBoxColumn { Name = "description", HeaderText = "Description", Width = 300 }
            });
        }

        private void InitializeTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 5000; // Refresh every 5 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private async void LoadInitialData()
        {
            dtpStart.Value = DateTime.Now.Date;
            dtpEnd.Value = DateTime.Now;
            await RefreshLogData();
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            await RefreshLogData();
        }

        private async void BtnRefresh_Click(object sender, EventArgs e)
        {
            await RefreshLogData();
        }

        private async Task RefreshLogData()
        {
            try
            {
                string actionFilter = cmbFilter.SelectedItem.ToString();
                string query = @"
                    SELECT l.id, l.created_at as timestamp, u.username, l.action, l.description
                    FROM t_log l
                    LEFT JOIN t_user u ON l.user_id = u.id
                    WHERE l.created_at BETWEEN @start AND @end
                    " + (actionFilter != "All Actions" ? "AND l.action = @action" : "") + @"
                    ORDER BY l.created_at DESC
                    LIMIT 1000";

                var parameters = new Dictionary<string, object>
                {
                    { "start", dtpStart.Value },
                    { "end", dtpEnd.Value }
                };

                if (actionFilter != "All Actions")
                {
                    parameters.Add("action", actionFilter.ToUpper());
                }

                var dt = await Task.Run(() => SimpleDatabaseHelper.GetData(query, parameters));
                
                dgvLogs.Invoke((MethodInvoker)delegate
                {
                    dgvLogs.DataSource = dt;
                    lblStatus.Text = $"Last updated: {DateTime.Now:HH:mm:ss}";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing logs: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            Task.Run(async () => await RefreshLogData());
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Filter = "CSV file (*.csv)|*.csv";
                    sfd.FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(sfd.FileName))
                        {
                            // Write headers
                            var headers = new List<string>();
                            foreach (DataGridViewColumn col in dgvLogs.Columns)
                            {
                                headers.Add(col.HeaderText);
                            }
                            sw.WriteLine(string.Join(",", headers));

                            // Write data rows
                            foreach (DataGridViewRow row in dgvLogs.Rows)
                            {
                                var cells = new List<string>();
                                foreach (DataGridViewCell cell in row.Cells)
                                {
                                    cells.Add($"\"{cell.Value}\"");
                                }
                                sw.WriteLine(string.Join(",", cells));
                            }
                        }

                        MessageBox.Show("Log data exported successfully!", "Success", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting logs: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            refreshTimer.Stop();
            base.OnFormClosing(e);
        }
    }
}
