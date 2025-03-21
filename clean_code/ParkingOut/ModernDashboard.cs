using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms.DataVisualization.Charting;
using SimpleParkingAdmin.Models;
using SimpleParkingAdmin.Utils;
using Serilog;
using Serilog.Events;

namespace SimpleParkingAdmin
{
    public partial class ModernDashboard : Form
    {
        private static readonly IAppLogger _logger = CustomLogManager.GetLogger();
        // Stats panels
        private readonly List<StatCard> statCards = new List<StatCard>();
        private readonly ChartPanel activityChart;
        private readonly RecentActivityPanel recentActivity;
        private User currentUser;
        
        // Dashboard stats data
        private int vehiclesInCount = 0;
        private int vehiclesOutCount = 0;
        private decimal totalRevenue = 0;
        private int todayVehiclesCount = 0;

        // Constructor
        public ModernDashboard(User user)
        {
            this.currentUser = user;
            this.BackColor = Color.FromArgb(245, 246, 250);
            this.Dock = DockStyle.Fill;
            this.Padding = new Padding(20);
            
            // Welcome panel
            WelcomePanel welcomePanel = new WelcomePanel(user);
            welcomePanel.Dock = DockStyle.Top;
            welcomePanel.Height = 80;
            this.Controls.Add(welcomePanel);
            
            // Stats container
            Panel statsContainer = new Panel();
            statsContainer.Height = 150;
            statsContainer.Dock = DockStyle.Top;
            statsContainer.BackColor = Color.Transparent;
            statsContainer.Margin = new Padding(0, 20, 0, 0);
            statsContainer.Padding = new Padding(0);
            
            // Create stat cards
            StatCard vehiclesInCard = new StatCard("Kendaraan Masuk", "0", CreateCarInIcon());
            StatCard vehiclesOutCard = new StatCard("Kendaraan Keluar", "0", CreateCarOutIcon());
            StatCard revenueCard = new StatCard("Pendapatan", "Rp 0", CreateMoneyIcon());
            StatCard todayVehiclesCard = new StatCard("Kendaraan Hari Ini", "0", CreateCalendarIcon());
            
            statCards.Add(vehiclesInCard);
            statCards.Add(vehiclesOutCard);
            statCards.Add(revenueCard);
            statCards.Add(todayVehiclesCard);
            
            // Arrange stats in a row
            TableLayoutPanel statsLayout = new TableLayoutPanel();
            statsLayout.ColumnCount = 4;
            statsLayout.RowCount = 1;
            statsLayout.Dock = DockStyle.Fill;
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            statsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            
            statsLayout.Controls.Add(vehiclesInCard, 0, 0);
            statsLayout.Controls.Add(vehiclesOutCard, 1, 0);
            statsLayout.Controls.Add(revenueCard, 2, 0);
            statsLayout.Controls.Add(todayVehiclesCard, 3, 0);
            
            statsContainer.Controls.Add(statsLayout);
            this.Controls.Add(statsContainer);
            
            // Main content panel (contains chart and recent activity)
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.Transparent;
            contentPanel.Padding = new Padding(0, 20, 0, 0);
            
            // Split content into two panels
            TableLayoutPanel contentLayout = new TableLayoutPanel();
            contentLayout.ColumnCount = 2;
            contentLayout.RowCount = 1;
            contentLayout.Dock = DockStyle.Fill;
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            
            // Chart panel
            activityChart = new ChartPanel();
            activityChart.Dock = DockStyle.Fill;
            activityChart.Margin = new Padding(0, 0, 10, 0);
            
            // Recent activity panel
            recentActivity = new RecentActivityPanel();
            recentActivity.Dock = DockStyle.Fill;
            recentActivity.Margin = new Padding(10, 0, 0, 0);
            
            contentLayout.Controls.Add(activityChart, 0, 0);
            contentLayout.Controls.Add(recentActivity, 1, 0);
            
            contentPanel.Controls.Add(contentLayout);
            this.Controls.Add(contentPanel);
            
            // Initial data load
            RefreshData();
        }
        
        // Method to refresh dashboard data
        public void RefreshData()
        {
            try
            {
                // Load stats from database
                LoadStats();
                
                // Update UI elements with loaded data
                UpdateUI();
                
                // Load chart data
                activityChart.LoadData();
                
                // Load recent activity
                recentActivity.LoadData();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error refreshing dashboard: {ex.Message}");
            }
        }
        
        private void LoadStats()
        {
            try
            {
                // Get vehicles currently in the parking lot
                string queryIn = "SELECT COUNT(*) FROM t_parkir WHERE status = 'MASUK'";
                vehiclesInCount = Convert.ToInt32(Database.ExecuteScalar(queryIn));
                
                // Get vehicles that have left today
                string queryOut = "SELECT COUNT(*) FROM t_parkir WHERE status = 'KELUAR' AND DATE(waktu_keluar) = CURDATE()";
                vehiclesOutCount = Convert.ToInt32(Database.ExecuteScalar(queryOut));
                
                // Get total revenue for today
                string queryRevenue = "SELECT COALESCE(SUM(tarif), 0) FROM t_parkir WHERE status = 'KELUAR' AND DATE(waktu_keluar) = CURDATE()";
                totalRevenue = Convert.ToDecimal(Database.ExecuteScalar(queryRevenue));
                
                // Get total vehicles for today
                string queryToday = "SELECT COUNT(*) FROM t_parkir WHERE DATE(waktu_masuk) = CURDATE()";
                todayVehiclesCount = Convert.ToInt32(Database.ExecuteScalar(queryToday));
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading dashboard stats: {ex.Message}");
            }
        }
        
        private void UpdateUI()
        {
            // Update stat cards with loaded data
            statCards[0].Value = vehiclesInCount.ToString();
            statCards[1].Value = vehiclesOutCount.ToString();
            statCards[2].Value = $"Rp {totalRevenue:N0}";
            statCards[3].Value = todayVehiclesCount.ToString();
        }
        
        // Method to create a parking icon (P in blue circle)
        private Image CreateCarInIcon()
        {
            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw circle
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(0, 120, 215)))
                {
                    g.FillEllipse(brush, 4, 4, 56, 56);
                }
                
                // Draw car icon
                using (Pen pen = new Pen(Color.White, 3))
                {
                    // Car body
                    g.DrawRectangle(pen, 18, 25, 28, 15);
                    
                    // Car wheels
                    g.DrawEllipse(pen, 20, 35, 8, 8);
                    g.DrawEllipse(pen, 36, 35, 8, 8);
                    
                    // Car roof
                    g.DrawLine(pen, 22, 25, 28, 18);
                    g.DrawLine(pen, 28, 18, 36, 18);
                    g.DrawLine(pen, 36, 18, 42, 25);
                    
                    // Arrow pointing in
                    g.DrawLine(pen, 48, 26, 36, 26);
                    g.DrawLine(pen, 42, 20, 36, 26);
                    g.DrawLine(pen, 42, 32, 36, 26);
                }
            }
            return bmp;
        }
        
        private Image CreateCarOutIcon()
        {
            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw circle
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(76, 175, 80))) // Green
                {
                    g.FillEllipse(brush, 4, 4, 56, 56);
                }
                
                // Draw car icon
                using (Pen pen = new Pen(Color.White, 3))
                {
                    // Car body
                    g.DrawRectangle(pen, 18, 25, 28, 15);
                    
                    // Car wheels
                    g.DrawEllipse(pen, 20, 35, 8, 8);
                    g.DrawEllipse(pen, 36, 35, 8, 8);
                    
                    // Car roof
                    g.DrawLine(pen, 22, 25, 28, 18);
                    g.DrawLine(pen, 28, 18, 36, 18);
                    g.DrawLine(pen, 36, 18, 42, 25);
                    
                    // Arrow pointing out
                    g.DrawLine(pen, 36, 26, 48, 26);
                    g.DrawLine(pen, 42, 20, 48, 26);
                    g.DrawLine(pen, 42, 32, 48, 26);
                }
            }
            return bmp;
        }
        
        private Image CreateMoneyIcon()
        {
            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw circle
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(255, 152, 0))) // Orange
                {
                    g.FillEllipse(brush, 4, 4, 56, 56);
                }
                
                // Draw $ symbol
                using (Font font = new Font("Arial", 32, FontStyle.Bold))
                {
                    using (StringFormat sf = new StringFormat())
                    {
                        sf.Alignment = StringAlignment.Center;
                        sf.LineAlignment = StringAlignment.Center;
                        
                        using (SolidBrush textBrush = new SolidBrush(Color.White))
                        {
                            g.DrawString("$", font, textBrush, 
                                new RectangleF(4, 4, 56, 56), sf);
                        }
                    }
                }
            }
            return bmp;
        }
        
        private Image CreateCalendarIcon()
        {
            Bitmap bmp = new Bitmap(64, 64);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                // Draw circle
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(156, 39, 176))) // Purple
                {
                    g.FillEllipse(brush, 4, 4, 56, 56);
                }
                
                // Draw calendar icon
                using (Pen pen = new Pen(Color.White, 2))
                {
                    // Calendar outline
                    g.DrawRectangle(pen, 20, 18, 24, 28);
                    
                    // Calendar header
                    g.DrawLine(pen, 20, 26, 44, 26);
                    
                    // Calendar hangers
                    g.DrawLine(pen, 26, 18, 26, 14);
                    g.DrawLine(pen, 38, 18, 38, 14);
                    
                    // Calendar lines
                    g.DrawLine(pen, 20, 34, 44, 34);
                    g.DrawLine(pen, 32, 26, 32, 46);
                }
            }
            return bmp;
        }
        
        // Inner classes for dashboard components
        
        // Welcome panel class
        private class WelcomePanel : Panel
        {
            public WelcomePanel(User user)
            {
                this.BackColor = Color.FromArgb(255, 255, 255);
                this.Padding = new Padding(20, 15, 20, 15);
                
                // Add welcome text with user name
                Label lblWelcome = new Label();
                lblWelcome.Text = $"Selamat Datang, {user.NamaLengkap}";
                lblWelcome.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                lblWelcome.ForeColor = Color.FromArgb(50, 50, 50);
                lblWelcome.AutoSize = true;
                lblWelcome.Location = new Point(20, 15);
                this.Controls.Add(lblWelcome);
                
                // Add date
                Label lblDate = new Label();
                lblDate.Text = DateTime.Now.ToString("dddd, dd MMMM yyyy");
                lblDate.Font = new Font("Segoe UI", 10);
                lblDate.ForeColor = Color.FromArgb(100, 100, 100);
                lblDate.AutoSize = true;
                lblDate.Location = new Point(20, 45);
                this.Controls.Add(lblDate);
                
                // Add level indicator
                Label lblLevel = new Label();
                lblLevel.Text = $"Level: {user.Level}";
                lblLevel.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblLevel.ForeColor = GetLevelColor(user.Level);
                lblLevel.AutoSize = true;
                lblLevel.Location = new Point(this.Width - 150, 30);
                lblLevel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
                this.Controls.Add(lblLevel);
                
                // Round the panel corners
                this.Paint += (s, e) => {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 10;
                        Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                        
                        // Top left corner
                        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        // Top right corner
                        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        // Bottom right corner
                        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        // Bottom left corner
                        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        
                        path.CloseAllFigures();
                        
                        // Fill panel
                        using (SolidBrush brush = new SolidBrush(this.BackColor))
                        {
                            g.FillPath(brush, path);
                        }
                        
                        // Draw border
                        using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                };
            }
            
            private Color GetLevelColor(string level)
            {
                switch (level?.ToUpper())
                {
                    case "ADMIN":
                        return Color.FromArgb(0, 120, 215); // Blue
                    case "SUPERVISOR":
                        return Color.FromArgb(76, 175, 80); // Green
                    case "OPERATOR":
                        return Color.FromArgb(255, 152, 0); // Orange
                    default:
                        return Color.FromArgb(100, 100, 100); // Gray
                }
            }
        }
        
        // Stat card class
        private class StatCard : Panel
        {
            private Label lblTitle;
            private Label lblValue;
            private PictureBox iconBox;
            
            public string Value
            {
                get { return lblValue.Text; }
                set { lblValue.Text = value; }
            }
            
            public StatCard(string title, string value, Image icon = null)
            {
                this.BackColor = Color.White;
                this.Padding = new Padding(15);
                this.Margin = new Padding(8);
                this.Height = 120;
                
                // Icon
                this.iconBox = new PictureBox();
                this.iconBox.Size = new Size(48, 48);
                this.iconBox.Location = new Point(15, 30);
                this.iconBox.SizeMode = PictureBoxSizeMode.Zoom;
                this.iconBox.Image = icon ?? CreateDefaultIcon();
                this.iconBox.BackColor = Color.Transparent;
                this.Controls.Add(this.iconBox);
                
                // Title
                this.lblTitle = new Label();
                this.lblTitle.Text = title;
                this.lblTitle.Font = new Font("Segoe UI", 9);
                this.lblTitle.ForeColor = Color.FromArgb(120, 120, 120);
                this.lblTitle.Location = new Point(75, 20);
                this.lblTitle.AutoSize = true;
                this.Controls.Add(this.lblTitle);
                
                // Value
                this.lblValue = new Label();
                this.lblValue.Text = value;
                this.lblValue.Font = new Font("Segoe UI", 20, FontStyle.Bold);
                this.lblValue.ForeColor = Color.FromArgb(60, 60, 60);
                this.lblValue.Location = new Point(75, 45);
                this.lblValue.AutoSize = true;
                this.Controls.Add(this.lblValue);
                
                // Round the panel corners
                this.Paint += (s, e) => {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 8;
                        Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                        
                        // Top left corner
                        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        // Top right corner
                        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        // Bottom right corner
                        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        // Bottom left corner
                        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        
                        path.CloseAllFigures();
                        
                        // Fill panel
                        using (SolidBrush brush = new SolidBrush(this.BackColor))
                        {
                            g.FillPath(brush, path);
                        }
                        
                        // Draw border
                        using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                };
            }
            
            private Image CreateDefaultIcon()
            {
                Bitmap bmp = new Bitmap(48, 48);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    // Draw circle
                    using (SolidBrush brush = new SolidBrush(Color.FromArgb(120, 120, 120)))
                    {
                        g.FillEllipse(brush, 4, 4, 40, 40);
                    }
                    
                    // Draw info symbol (i)
                    using (Font font = new Font("Arial", 24, FontStyle.Bold))
                    {
                        using (StringFormat sf = new StringFormat())
                        {
                            sf.Alignment = StringAlignment.Center;
                            sf.LineAlignment = StringAlignment.Center;
                            
                            using (SolidBrush textBrush = new SolidBrush(Color.White))
                            {
                                g.DrawString("i", font, textBrush, 
                                    new RectangleF(4, 0, 40, 48), sf);
                            }
                        }
                    }
                }
                return bmp;
            }
        }
        
        // Chart panel
        private class ChartPanel : Panel
        {
            private Dictionary<DateTime, int> hourlyData = new Dictionary<DateTime, int>();
            
            public ChartPanel()
            {
                this.BackColor = Color.White;
                this.Padding = new Padding(20);
                
                // Add title
                Label lblTitle = new Label();
                lblTitle.Text = "Aktivitas Hari Ini (Per Jam)";
                lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                lblTitle.ForeColor = Color.FromArgb(60, 60, 60);
                lblTitle.AutoSize = true;
                lblTitle.Location = new Point(20, 15);
                this.Controls.Add(lblTitle);
                
                // Round the panel corners
                this.Paint += (s, e) => {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 10;
                        Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                        
                        // Top left corner
                        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        // Top right corner
                        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        // Bottom right corner
                        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        // Bottom left corner
                        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        
                        path.CloseAllFigures();
                        
                        // Fill panel
                        using (SolidBrush brush = new SolidBrush(this.BackColor))
                        {
                            g.FillPath(brush, path);
                        }
                        
                        // Draw border
                        using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                        {
                            g.DrawPath(pen, path);
                        }
                        
                        // Draw chart if data is loaded
                        if (hourlyData.Count > 0)
                        {
                            DrawChart(g, new Rectangle(40, 60, this.Width - 80, this.Height - 100));
                        }
                    }
                };
            }
            
            public void LoadData()
            {
                try
                {
                    hourlyData.Clear();
                    
                    // Get hourly count of vehicles entering
                    string query = @"
                        SELECT 
                            HOUR(waktu_masuk) as hour,
                            COUNT(*) as count
                        FROM 
                            t_parkir
                        WHERE 
                            DATE(waktu_masuk) = CURDATE()
                        GROUP BY 
                            HOUR(waktu_masuk)
                        ORDER BY 
                            hour";
                    
                    DataTable dt = Database.GetData(query);
                    
                    // Initialize hours with zero count
                    DateTime now = DateTime.Now;
                    DateTime startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
                    
                    for (int hour = 0; hour <= now.Hour; hour++)
                    {
                        hourlyData[startOfDay.AddHours(hour)] = 0;
                    }
                    
                    // Fill in actual data
                    if (dt != null)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            int hour = Convert.ToInt32(row["hour"]);
                            int count = Convert.ToInt32(row["count"]);
                            
                            DateTime hourKey = startOfDay.AddHours(hour);
                            hourlyData[hourKey] = count;
                        }
                    }
                    
                    // Refresh chart
                    this.Invalidate();
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error loading chart data: {ex.Message}");
                }
            }
            
            private void DrawChart(Graphics g, Rectangle chartRect)
            {
                try
                {
                    if (hourlyData.Count == 0) return;
                    
                    // Calculate max value for scaling
                    int maxValue = 1; // Minimum to avoid division by zero
                    foreach (var count in hourlyData.Values)
                    {
                        if (count > maxValue) maxValue = count;
                    }
                    
                    // Add 20% to max for better visualization
                    maxValue = (int)(maxValue * 1.2);
                    if (maxValue < 5) maxValue = 5;
                    
                    // Calculate bar width
                    int barCount = hourlyData.Count;
                    int barWidth = (chartRect.Width - (barCount * 10)) / barCount;
                    if (barWidth < 20) barWidth = 20;
                    
                    // Draw axes
                    using (Pen axisPen = new Pen(Color.FromArgb(200, 200, 200), 1))
                    {
                        // X axis
                        g.DrawLine(axisPen, 
                            chartRect.Left, chartRect.Bottom, 
                            chartRect.Right, chartRect.Bottom);
                        
                        // Y axis
                        g.DrawLine(axisPen, 
                            chartRect.Left, chartRect.Top, 
                            chartRect.Left, chartRect.Bottom);
                    }
                    
                    // Draw Y axis labels
                    using (Font labelFont = new Font("Segoe UI", 8))
                    {
                        for (int i = 0; i <= 4; i++)
                        {
                            int yValue = (maxValue * i) / 4;
                            int y = chartRect.Bottom - ((chartRect.Height * i) / 4);
                            
                            // Draw grid line
                            using (Pen gridPen = new Pen(Color.FromArgb(240, 240, 240), 1))
                            {
                                g.DrawLine(gridPen, 
                                    chartRect.Left, y, 
                                    chartRect.Right, y);
                            }
                            
                            // Draw label
                            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                            {
                                g.DrawString(yValue.ToString(), labelFont, labelBrush,
                                    new PointF(chartRect.Left - 25, y - 8));
                            }
                        }
                    }
                    
                    // Draw bars
                    int index = 0;
                    foreach (var entry in hourlyData)
                    {
                        DateTime hour = entry.Key;
                        int count = entry.Value;
                        
                        // Calculate bar height
                        int barHeight = (int)((count * chartRect.Height) / (float)maxValue);
                        
                        // Calculate bar position
                        int x = chartRect.Left + 10 + (index * (barWidth + 10));
                        int y = chartRect.Bottom - barHeight;
                        
                        // Draw bar
                        using (SolidBrush barBrush = new SolidBrush(Color.FromArgb(0, 120, 215, 180)))
                        {
                            g.FillRectangle(barBrush, x, y, barWidth, barHeight);
                        }
                        
                        // Draw X axis label (hour)
                        using (Font labelFont = new Font("Segoe UI", 8))
                        {
                            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
                            {
                                g.DrawString(hour.ToString("HH:00"), labelFont, labelBrush,
                                    new PointF(x, chartRect.Bottom + 5));
                            }
                        }
                        
                        // Draw value on top of bar if it's not zero
                        if (count > 0)
                        {
                            using (Font valueFont = new Font("Segoe UI", 8, FontStyle.Bold))
                            {
                                using (SolidBrush valueBrush = new SolidBrush(Color.FromArgb(60, 60, 60)))
                                {
                                    SizeF textSize = g.MeasureString(count.ToString(), valueFont);
                                    g.DrawString(count.ToString(), valueFont, valueBrush,
                                        new PointF(x + (barWidth - textSize.Width) / 2, y - textSize.Height - 2));
                                }
                            }
                        }
                        
                        index++;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error drawing chart: {ex.Message}");
                }
            }
        }
        
        // Recent activity panel
        private class RecentActivityPanel : Panel
        {
            private ListView listView;
            
            public RecentActivityPanel()
            {
                this.BackColor = Color.White;
                this.Padding = new Padding(20);
                
                // Add title
                Label lblTitle = new Label();
                lblTitle.Text = "Aktivitas Terbaru";
                lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                lblTitle.ForeColor = Color.FromArgb(60, 60, 60);
                lblTitle.AutoSize = true;
                lblTitle.Location = new Point(20, 15);
                this.Controls.Add(lblTitle);
                
                // Create ListView for recent activity
                listView = new ListView();
                listView.View = View.Details;
                listView.FullRowSelect = true;
                listView.GridLines = false;
                listView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
                listView.BorderStyle = BorderStyle.None;
                listView.Location = new Point(20, 50);
                listView.Size = new Size(this.Width - 40, this.Height - 70);
                listView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                
                // Add columns
                listView.Columns.Add("Waktu", 80);
                listView.Columns.Add("Kendaraan", 90);
                listView.Columns.Add("Status", 70);
                
                // Set custom appearance
                listView.Font = new Font("Segoe UI", 9);
                listView.BackColor = Color.White;
                listView.ForeColor = Color.FromArgb(60, 60, 60);
                
                this.Controls.Add(listView);
                
                // Round the panel corners
                this.Paint += (s, e) => {
                    Graphics g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    
                    using (GraphicsPath path = new GraphicsPath())
                    {
                        int radius = 10;
                        Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
                        
                        // Top left corner
                        path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
                        // Top right corner
                        path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
                        // Bottom right corner
                        path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
                        // Bottom left corner
                        path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
                        
                        path.CloseAllFigures();
                        
                        // Fill panel
                        using (SolidBrush brush = new SolidBrush(this.BackColor))
                        {
                            g.FillPath(brush, path);
                        }
                        
                        // Draw border
                        using (Pen pen = new Pen(Color.FromArgb(230, 230, 230), 1))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                };
            }
            
            public void LoadData()
            {
                try
                {
                    listView.Items.Clear();
                    
                    // Get most recent parking activities
                    string query = @"
                        (SELECT 
                            waktu_masuk AS waktu, 
                            nomor_polisi, 
                            'MASUK' AS status
                        FROM 
                            t_parkir
                        WHERE 
                            DATE(waktu_masuk) = CURDATE()
                        ORDER BY 
                            waktu_masuk DESC
                        LIMIT 10)
                        
                        UNION ALL
                        
                        (SELECT 
                            waktu_keluar AS waktu, 
                            nomor_polisi, 
                            'KELUAR' AS status
                        FROM 
                            t_parkir
                        WHERE 
                            waktu_keluar IS NOT NULL AND
                            DATE(waktu_keluar) = CURDATE()
                        ORDER BY 
                            waktu_keluar DESC
                        LIMIT 10)
                        
                        ORDER BY 
                            waktu DESC
                        LIMIT 15";
                    
                    DataTable dt = Database.GetData(query);
                    
                    if (dt != null)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            DateTime time = Convert.ToDateTime(row["waktu"]);
                            string licensePlate = row["nomor_polisi"].ToString();
                            string status = row["status"].ToString();
                            
                            ListViewItem item = new ListViewItem(time.ToString("HH:mm"));
                            item.SubItems.Add(licensePlate);
                            item.SubItems.Add(status);
                            
                            // Set item color based on status
                            if (status == "MASUK")
                            {
                                item.ForeColor = Color.FromArgb(0, 120, 215); // Blue
                            }
                            else
                            {
                                item.ForeColor = Color.FromArgb(76, 175, 80); // Green
                            }
                            
                            listView.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error loading recent activity: {ex.Message}");
                }
            }
        }
    }
} 