using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Forms.DataVisualization.Charting;
using SimpleParkingAdmin.Utils;
using System.Drawing.Drawing2D;
using SimpleParkingAdmin.Models;
using SimpleParkingAdmin.Controls;
using SimpleParkingAdmin.Forms;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    public partial class DashboardForm : Form
    {
        private readonly IAppLogger _logger;
        private readonly User _currentUser;
        private System.Windows.Forms.Timer refreshTimer;
        private Panel mainContainer;
        private FlowLayoutPanel statsContainer;
        private Panel chartContainer;
        private Chart revenueChart;
        private Chart occupancyChart;
        private Panel contentPanel;
        private Sidebar sidebar;
        private ComboBox periodSelector;
        private DateTimePicker datePicker;
        private Panel mainPanel;
        private Panel headerPanel;
        private Label lblTitle;
        private Label lblWelcome;
        private Label lblDateTime;
        private System.Windows.Forms.Timer timer;

        // Modern color palette
        private readonly Color primaryColor = Color.FromArgb(24, 116, 205);
        private readonly Color secondaryColor = Color.FromArgb(45, 52, 54);
        private readonly Color accentColor = Color.FromArgb(9, 132, 227);
        private readonly Color successColor = Color.FromArgb(0, 184, 148);
        private readonly Color warningColor = Color.FromArgb(253, 203, 110);
        private readonly Color dangerColor = Color.FromArgb(214, 48, 49);
        private readonly Color textColor = Color.FromArgb(45, 52, 54);
        private readonly Color bgColor = Color.FromArgb(245, 246, 250);

        public DashboardForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = CustomLogManager.GetLogger();
            InitializeComponent();
            SetupDashboard();
            
            this.Text = "Dashboard - Parking Management System";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = bgColor;

            // Initialize refresh timer
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Interval = 30000; // 30 seconds
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DashboardForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 800);
            this.Name = "DashboardForm";
            this.Text = "Dashboard";
            this.ResumeLayout(false);
        }

        private void SetupDashboard()
        {
            try
            {
                // Create sidebar
                sidebar = new Sidebar(this, _currentUser);
                sidebar.MenuSelected += Sidebar_MenuSelected;
                this.Controls.Add(sidebar);

                // Content panel
                contentPanel = new Panel
                {
                    Dock = DockStyle.Fill,
                    BackColor = bgColor
                };

                // Main container with padding
                mainContainer = new Panel
                {
                    Dock = DockStyle.Fill,
                    Padding = new Padding(20),
                    BackColor = bgColor
                };

                // Stats Container
                CreateStatsContainer();

                // Chart Container
                CreateChartContainer();

                // Add containers to main container
                mainContainer.Controls.AddRange(new Control[] {
                    statsContainer,
                    chartContainer
                });

                contentPanel.Controls.Add(mainContainer);
                this.Controls.Add(contentPanel);

                // Set initial active menu
                sidebar.SetActiveMenu("dashboard");

                // Load initial data
                RefreshDashboard();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error setting up dashboard: {ex.Message}");
                MessageBox.Show("Failed to initialize dashboard.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Sidebar_MenuSelected(object sender, string menuId)
        {
            try
            {
                switch (menuId)
                {
                    case "dashboard":
                        ShowDashboard();
                        break;
                    case "entry":
                        OpenQuickEntry();
                        break;
                    case "exit":
                        OpenQuickExit();
                        break;
                    case "member":
                        OpenMemberManagement();
                        break;
                    case "report":
                        OpenReports();
                        break;
                    case "settings":
                        if (_currentUser.IsAdmin)
                        {
                            var settingsForm = new SettingsForm(_currentUser);
                            settingsForm.ShowDialog();
                            RefreshDashboard();
                        }
                        else
                        {
                            MessageBox.Show("Access denied. Only administrators can access settings.", "Access Denied",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error handling menu selection: {ex.Message}");
                MessageBox.Show("Failed to process menu selection.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowDashboard()
        {
            contentPanel.Controls.Clear();
            contentPanel.Controls.Add(mainContainer);
            RefreshDashboard();
        }

        private void CreateStatsContainer()
        {
            statsContainer = new FlowLayoutPanel
            {
                Height = 150,
                Dock = DockStyle.Top,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0, 10, 0, 10)
            };
        }

        private void CreateChartContainer()
        {
            chartContainer = new Panel
            {
                Height = 400,
                Dock = DockStyle.Top,
                Padding = new Padding(0, 10, 0, 10)
            };

            // Revenue Chart
            revenueChart = new Chart
            {
                Width = chartContainer.Width / 2 - 20,
                Height = chartContainer.Height - 20,
                Dock = DockStyle.Left,
                BackColor = Color.White
            };
            SetupRevenueChart();

            // Occupancy Chart
            occupancyChart = new Chart
            {
                Width = chartContainer.Width / 2 - 20,
                Height = chartContainer.Height - 20,
                Dock = DockStyle.Right,
                BackColor = Color.White
            };
            SetupOccupancyChart();

            chartContainer.Controls.AddRange(new Control[] { revenueChart, occupancyChart });
        }

        private void SetupRevenueChart()
        {
            revenueChart.Titles.Add(new Title("Revenue Trend", Docking.Top));
            
            var area = new ChartArea();
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.BackColor = Color.White;
            
            revenueChart.ChartAreas.Add(area);
            
            var series = new Series("Revenue")
            {
                ChartType = SeriesChartType.SplineArea,
                Color = Color.FromArgb(100, accentColor),
                BorderColor = accentColor,
                BorderWidth = 3
            };
            
            revenueChart.Series.Add(series);
        }

        private void SetupOccupancyChart()
        {
            occupancyChart.Titles.Add(new Title("Parking Occupancy", Docking.Top));
            
            var area = new ChartArea();
            area.AxisX.MajorGrid.LineColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.LightGray;
            area.BackColor = Color.White;
            
            occupancyChart.ChartAreas.Add(area);
            
            var series = new Series("Occupancy")
            {
                ChartType = SeriesChartType.Doughnut,
                Color = successColor
            };
            
            occupancyChart.Series.Add(series);
        }

        private void RefreshDashboard()
        {
            try
            {
                statsContainer.Controls.Clear();
                
                // Get statistics based on selected period
                DateTime startDate = GetStartDate();
                DateTime endDate = DateTime.Now;

                _logger.Debug($"Refreshing dashboard: startDate={startDate}, endDate={endDate}");

                string query = @"
                    SELECT 
                        COUNT(*) as total_vehicles,
                        SUM(CASE WHEN ""waktu_keluar"" IS NULL THEN 1 ELSE 0 END) as active_vehicles,       
                        SUM(CASE WHEN ""waktu_keluar"" IS NOT NULL THEN 1 ELSE 0 END) as completed_vehicles,
                        COALESCE(SUM(biaya), 0) as total_revenue,
                        COUNT(DISTINCT CASE WHEN nomor_kartu_member IS NOT NULL THEN nomor_kartu_member END) as member_visits
                    FROM t_parkir
                    WHERE waktu_masuk BETWEEN @startDate AND @endDate";

                _logger.Debug($"Executing dashboard summary query");

                Dictionary<string, object> parameters = new Dictionary<string, object>
                {
                    { "@startDate", startDate },
                    { "@endDate", endDate }
                };

                DataTable data = Database.GetData(query, parameters);
                
                _logger.Debug($"Dashboard query executed, rows returned: {data.Rows.Count}");

                if (data.Rows.Count > 0)
                {
                    DataRow row = data.Rows[0];
                    
                    // Calculate trends (simplified for example)
                    string vehicleTrend = "+5% vs last period";
                    string revenueTrend = "+8% vs last period";
                    string memberTrend = "+3% vs last period";
                    string occupancyTrend = "-2% vs last period";

                    // Add stat cards
                    AddStatCard("Total Vehicles", 
                        row["total_vehicles"].ToString(),
                        vehicleTrend, primaryColor);
                        
                    AddStatCard("Active Parkings", 
                        row["active_vehicles"].ToString(),
                        occupancyTrend, successColor);
                        
                    AddStatCard("Today's Revenue", 
                        $"Rp {Convert.ToDecimal(row["total_revenue"]):N0}",
                        revenueTrend, accentColor);
                        
                    AddStatCard("Member Visits", 
                        row["member_visits"].ToString(),
                        memberTrend, warningColor);

                    // Update charts
                    UpdateRevenueChart(startDate, endDate);
                    UpdateOccupancyChart();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error refreshing dashboard: {ex.Message}");
            }
        }

        private void UpdateRevenueChart(DateTime startDate, DateTime endDate)
        {
            try
            {
                string query = @"
                    SELECT 
                        DATE(waktu_masuk) as date,
                        SUM(biaya) as revenue
                    FROM t_parkir
                    WHERE waktu_masuk BETWEEN @startDate AND @endDate
                    GROUP BY DATE(waktu_masuk)
                    ORDER BY date";

                var parameters = new Dictionary<string, object>
                {
                    { "@startDate", startDate },
                    { "@endDate", endDate }
                };

                DataTable revenueData = Database.GetData(query, parameters);

                revenueChart.Series[0].Points.Clear();
                foreach (DataRow row in revenueData.Rows)
                {
                    revenueChart.Series[0].Points.AddXY(
                        Convert.ToDateTime(row["date"]).ToString("dd/MM"),
                        Convert.ToDecimal(row["revenue"]));
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating revenue chart: {ex.Message}");
            }
        }

        private void UpdateOccupancyChart()
        {
            try
            {
                string query = @"
                    SELECT
                        jenis_kendaraan,
                        COUNT(*) as count
                    FROM t_parkir
                    WHERE ""waktu_keluar"" IS NULL
                    GROUP BY jenis_kendaraan";

                DataTable occupancyData = Database.GetData(query);

                occupancyChart.Series[0].Points.Clear();
                foreach (DataRow row in occupancyData.Rows)
                {
                    var point = occupancyChart.Series[0].Points.Add(
                        Convert.ToDouble(row["count"]));
                    point.LegendText = row["jenis_kendaraan"].ToString();
                    point.Label = $"{row["count"]} ({row["jenis_kendaraan"]})";
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error updating occupancy chart: {ex.Message}");
            }
        }

        private DateTime GetStartDate()
        {
            return periodSelector.SelectedItem.ToString() switch
            {
                "Today" => DateTime.Today,
                "This Week" => DateTime.Today.AddDays(-7),
                "This Month" => DateTime.Today.AddMonths(-1),
                "Custom" => datePicker.Value.Date,
                _ => DateTime.Today
            };
        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            RefreshDashboard();
        }

        private void PeriodSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            datePicker.Visible = periodSelector.SelectedItem.ToString() == "Custom";
            RefreshDashboard();
        }

        private void DatePicker_ValueChanged(object sender, EventArgs e)
        {
            if (periodSelector.SelectedItem.ToString() == "Custom")
            {
                RefreshDashboard();
            }
        }

        private void OpenQuickEntry()
        {
            try
            {
                var form = new EntryForm(_currentUser);
                form.ShowDialog();
                RefreshDashboard();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error opening quick entry: {ex.Message}");
                MessageBox.Show("Failed to open quick entry form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenQuickExit()
        {
            try
            {
                var form = new ExitForm(_currentUser);
                form.ShowDialog();
                RefreshDashboard();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error opening quick exit: {ex.Message}");
                MessageBox.Show("Failed to open quick exit form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenMemberManagement()
        {
            try
            {
                var form = new MemberManagementForm(_currentUser);
                form.ShowDialog();
                RefreshDashboard();
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening member management", ex);
                MessageBox.Show("Failed to open member management form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenReports()
        {
            try
            {
                var form = new ReportForm(_currentUser);
                form.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger.Error("Error opening reports", ex);
                MessageBox.Show("Failed to open reports form.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            refreshTimer.Stop();
            refreshTimer.Dispose();
        }

        private void AddStatCard(string title, string value, string trend, Color color)
        {
            Panel card = new Panel
            {
                Width = 280,
                Height = 120,
                Margin = new Padding(10),
                BackColor = Color.White,
                Padding = new Padding(15)
            };

            // Add shadow effect
            card.Paint += (s, e) =>
            {
                using (var path = new GraphicsPath())
                {
                    var rect = new Rectangle(0, 0, card.Width, card.Height);
                    path.AddRectangle(rect);
                    using (var brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(20, 0, 0, 0);
                        brush.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillPath(brush, path);
                    }
                }
            };

            Label titleLabel = new Label
            {
                Text = title,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10),
                AutoSize = true
            };

            Label valueLabel = new Label
            {
                Text = value,
                ForeColor = color,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(0, 25)
            };

            Label trendLabel = new Label
            {
                Text = trend,
                ForeColor = trend.StartsWith("+") ? successColor : dangerColor,
                Font = new Font("Segoe UI", 9),
                AutoSize = true,
                Location = new Point(0, 60)
            };

            card.Controls.AddRange(new Control[] { titleLabel, valueLabel, trendLabel });
            statsContainer.Controls.Add(card);
        }
    }
} 