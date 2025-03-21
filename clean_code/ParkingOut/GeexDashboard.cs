using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data;
using SimpleParkingAdmin.Models;

namespace SimpleParkingAdmin
{
    public class GeexDashboard : Panel
    {
        private User currentUser;
        private Panel mainArea;
        private Panel sidebarPanel;
        private Panel headerPanel;
        
        // UI components
        private List<StatCard> statCards = new List<StatCard>();
        private LineChartPanel serverRequestChart;
        private BarChartPanel serverStatusChart;
        private PieChartPanel chartSummaryPanel;
        private RecentProblemsPanel recentProblems;
        
        // Theme colors
        private readonly Color darkBackground = Color.FromArgb(24, 26, 32);
        private readonly Color cardBackground = Color.FromArgb(32, 34, 40);
        private readonly Color accentBlue = Color.FromArgb(0, 120, 212);
        private readonly Color accentPurple = Color.FromArgb(170, 0, 255);
        private readonly Color accentRed = Color.FromArgb(255, 99, 88);
        private readonly Color accentGreen = Color.FromArgb(0, 200, 170);
        private readonly Color textLight = Color.FromArgb(230, 230, 230);
        private readonly Color textDark = Color.FromArgb(180, 180, 180);
        
        public GeexDashboard(User user)
        {
            this.currentUser = user;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White; // Will be hidden by child panels
            
            InitializeComponents();
            LoadData();
        }
        
        private void InitializeComponents()
        {
            // Create the main container
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.ColumnCount = 2;
            mainLayout.RowCount = 1;
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.BackColor = darkBackground;
            
            // Sidebar
            sidebarPanel = CreateSidebar();
            
            // Main content area
            mainArea = new Panel();
            mainArea.Dock = DockStyle.Fill;
            mainArea.BackColor = Color.FromArgb(18, 18, 24);
            mainArea.Padding = new Padding(20);
            
            // Add header to main area
            headerPanel = CreateHeader();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 60;
            mainArea.Controls.Add(headerPanel);
            
            // Create content panel for dashboard items
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BackColor = Color.Transparent;
            contentPanel.Padding = new Padding(0, 15, 0, 0);
            mainArea.Controls.Add(contentPanel);
            
            // Add stat cards row
            Panel statCardsRow = CreateStatCardsRow();
            statCardsRow.Height = 130;
            statCardsRow.Dock = DockStyle.Top;
            contentPanel.Controls.Add(statCardsRow);
            
            // Add main chart area
            Panel chartContainer = new Panel();
            chartContainer.Dock = DockStyle.Top;
            chartContainer.Height = 300;
            chartContainer.Margin = new Padding(0, 20, 0, 0);
            chartContainer.BackColor = Color.Transparent;
            chartContainer.Padding = new Padding(0, 20, 0, 0);
            
            // Create server request chart
            serverRequestChart = new LineChartPanel("Server Request");
            serverRequestChart.Dock = DockStyle.Fill;
            chartContainer.Controls.Add(serverRequestChart);
            
            contentPanel.Controls.Add(chartContainer);
            
            // Add bottom row with pie chart and server status
            TableLayoutPanel bottomRow = new TableLayoutPanel();
            bottomRow.Dock = DockStyle.Fill;
            bottomRow.ColumnCount = 2;
            bottomRow.RowCount = 1;
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            bottomRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            bottomRow.Margin = new Padding(0, 20, 0, 0);
            
            // Bottom left panel - two cards side by side
            TableLayoutPanel bottomLeftPanel = new TableLayoutPanel();
            bottomLeftPanel.ColumnCount = 2;
            bottomLeftPanel.RowCount = 1;
            bottomLeftPanel.Dock = DockStyle.Fill;
            bottomLeftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomLeftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            
            // Visitors panel
            Panel visitorsPanel = CreateVisitorsPanel();
            visitorsPanel.Margin = new Padding(0, 0, 10, 0);
            bottomLeftPanel.Controls.Add(visitorsPanel, 0, 0);
            
            // Chart summary panel with pie chart
            chartSummaryPanel = new PieChartPanel("Chart Summary");
            chartSummaryPanel.Margin = new Padding(10, 0, 0, 0);
            bottomLeftPanel.Controls.Add(chartSummaryPanel, 1, 0);
            
            // Bottom right panel
            TableLayoutPanel bottomRightPanel = new TableLayoutPanel();
            bottomRightPanel.RowCount = 2;
            bottomRightPanel.ColumnCount = 1;
            bottomRightPanel.Dock = DockStyle.Fill;
            bottomRightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            bottomRightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            bottomRightPanel.Margin = new Padding(20, 0, 0, 0);
            
            // Server status panel
            serverStatusChart = new BarChartPanel("Server Status");
            serverStatusChart.Margin = new Padding(0, 0, 0, 10);
            bottomRightPanel.Controls.Add(serverStatusChart, 0, 0);
            
            // Recent problems panel
            recentProblems = new RecentProblemsPanel();
            recentProblems.Margin = new Padding(0, 10, 0, 0);
            bottomRightPanel.Controls.Add(recentProblems, 0, 1);
            
            bottomRow.Controls.Add(bottomLeftPanel, 0, 0);
            bottomRow.Controls.Add(bottomRightPanel, 1, 0);
            
            contentPanel.Controls.Add(bottomRow);
            
            // Add sidebar and main area to the layout
            mainLayout.Controls.Add(sidebarPanel, 0, 0);
            mainLayout.Controls.Add(mainArea, 1, 0);
            
            this.Controls.Add(mainLayout);
        }
        
        private Panel CreateSidebar()
        {
            Panel sidebar = new Panel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = darkBackground;
            sidebar.Padding = new Padding(0, 15, 0, 15);
            
            // Logo container
            Panel logoPanel = new Panel();
            logoPanel.Height = 60;
            logoPanel.Dock = DockStyle.Top;
            logoPanel.BackColor = Color.Transparent;
            logoPanel.Padding = new Padding(20, 0, 0, 0);
            
            // Logo image
            PictureBox logo = new PictureBox();
            logo.Image = CreateLogoImage();
            logo.SizeMode = PictureBoxSizeMode.Zoom;
            logo.Size = new Size(120, 40);
            logo.Location = new Point(20, 10);
            logoPanel.Controls.Add(logo);
            
            sidebar.Controls.Add(logoPanel);
            
            // Add menu items
            int buttonIndex = 0;
            
            // Demo button
            Button btnDemo = CreateMenuButton("Demo", buttonIndex++);
            btnDemo.BackColor = accentBlue; // Active button
            sidebar.Controls.Add(btnDemo);
            
            // Server Management button
            Button btnServer = CreateMenuButton("Server Management", buttonIndex++);
            sidebar.Controls.Add(btnServer);
            
            // Banking button
            Button btnBanking = CreateMenuButton("Banking", buttonIndex++);
            sidebar.Controls.Add(btnBanking);
            
            // Crypto button
            Button btnCrypto = CreateMenuButton("Crypto", buttonIndex++);
            sidebar.Controls.Add(btnCrypto);
            
            // Invoicing button
            Button btnInvoicing = CreateMenuButton("Invoicing", buttonIndex++);
            sidebar.Controls.Add(btnInvoicing);
            
            // Separator
            Panel separator = new Panel();
            separator.Height = 1;
            separator.Dock = DockStyle.Top;
            separator.BackColor = Color.FromArgb(50, 50, 50);
            separator.Margin = new Padding(20, 15, 20, 15);
            sidebar.Controls.Add(separator);
            
            // Layout button
            Button btnLayout = CreateMenuButton("Layout", buttonIndex++);
            sidebar.Controls.Add(btnLayout);
            
            // App button
            Button btnApp = CreateMenuButton("App", buttonIndex++);
            sidebar.Controls.Add(btnApp);
            
            // Features button
            Button btnFeatures = CreateMenuButton("Features", buttonIndex++);
            sidebar.Controls.Add(btnFeatures);
            
            // Pages button
            Button btnPages = CreateMenuButton("Pages", buttonIndex++);
            sidebar.Controls.Add(btnPages);
            
            return sidebar;
        }
        
        private Button CreateMenuButton(string text, int index)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Top;
            btn.Height = 40;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = darkBackground;
            btn.ForeColor = textLight;
            btn.Font = new Font("Segoe UI", 9F);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(20, 0, 0, 0);
            btn.Cursor = Cursors.Hand;
            btn.Tag = index;
            
            // Hover effects
            btn.MouseEnter += (s, e) => {
                if (btn.BackColor != accentBlue) // If not active
                    btn.BackColor = Color.FromArgb(40, 40, 45);
            };
            
            btn.MouseLeave += (s, e) => {
                if (btn.BackColor != accentBlue) // If not active
                    btn.BackColor = darkBackground;
            };
            
            return btn;
        }
        
        private Panel CreateHeader()
        {
            Panel header = new Panel();
            header.BackColor = Color.Transparent;
            
            // Dashboard title
            Label lblTitle = new Label();
            lblTitle.Text = "Dashboard";
            lblTitle.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(0, 5);
            lblTitle.AutoSize = true;
            header.Controls.Add(lblTitle);
            
            // Welcome message
            Label lblWelcome = new Label();
            lblWelcome.Text = $"Welcome to Geex Modern Admin Dashboard";
            lblWelcome.Font = new Font("Segoe UI", 9);
            lblWelcome.ForeColor = textDark;
            lblWelcome.Location = new Point(0, 35);
            lblWelcome.AutoSize = true;
            header.Controls.Add(lblWelcome);
            
            // Right-side controls
            Panel rightControls = new Panel();
            rightControls.Dock = DockStyle.Right;
            rightControls.Width = 200;
            rightControls.BackColor = Color.Transparent;
            
            // Search button
            Button btnSearch = new Button();
            btnSearch.Text = "🔍";
            btnSearch.FlatStyle = FlatStyle.Flat;
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Size = new Size(40, 40);
            btnSearch.Location = new Point(50, 10);
            btnSearch.BackColor = Color.Transparent;
            btnSearch.ForeColor = textLight;
            btnSearch.Cursor = Cursors.Hand;
            rightControls.Controls.Add(btnSearch);
            
            // Notifications button
            Button btnNotifications = new Button();
            btnNotifications.Text = "🔔";
            btnNotifications.FlatStyle = FlatStyle.Flat;
            btnNotifications.FlatAppearance.BorderSize = 0;
            btnNotifications.Size = new Size(40, 40);
            btnNotifications.Location = new Point(100, 10);
            btnNotifications.BackColor = Color.Transparent;
            btnNotifications.ForeColor = textLight;
            btnNotifications.Cursor = Cursors.Hand;
            rightControls.Controls.Add(btnNotifications);
            
            // User profile button
            Button btnProfile = new Button();
            btnProfile.Text = "👤";
            btnProfile.FlatStyle = FlatStyle.Flat;
            btnProfile.FlatAppearance.BorderSize = 0;
            btnProfile.Size = new Size(40, 40);
            btnProfile.Location = new Point(150, 10);
            btnProfile.BackColor = Color.Transparent;
            btnProfile.ForeColor = textLight;
            btnProfile.Cursor = Cursors.Hand;
            rightControls.Controls.Add(btnProfile);
            
            header.Controls.Add(rightControls);
            
            return header;
        }
        
        private Panel CreateStatCardsRow()
        {
            TableLayoutPanel statCardsPanel = new TableLayoutPanel();
            statCardsPanel.Dock = DockStyle.Fill;
            statCardsPanel.ColumnCount = 3;
            statCardsPanel.RowCount = 1;
            statCardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            statCardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            statCardsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            
            // Memory usage card with purple sparkline
            StatCard memoryCard = new StatCard("Memory", "200TB", "+2.5%", CreateSparkline(accentPurple));
            memoryCard.Margin = new Padding(0, 0, 10, 0);
            
            // Visitors card with red sparkline
            StatCard visitorsCard = new StatCard("Visitors", "87,245k", "-4.4%", CreateSparkline(accentRed));
            visitorsCard.Margin = new Padding(10, 0, 10, 0);
            
            // Users card with teal sparkline
            StatCard usersCard = new StatCard("New Users", "4,750", "+2.5%", CreateSparkline(accentGreen));
            usersCard.Margin = new Padding(10, 0, 0, 0);
            
            statCardsPanel.Controls.Add(memoryCard, 0, 0);
            statCardsPanel.Controls.Add(visitorsCard, 1, 0);
            statCardsPanel.Controls.Add(usersCard, 2, 0);
            
            statCards.Add(memoryCard);
            statCards.Add(visitorsCard);
            statCards.Add(usersCard);
            
            return statCardsPanel;
        }
        
        private Panel CreateVisitorsPanel()
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Fill;
            panel.BackColor = cardBackground;
            panel.Padding = new Padding(20);
            
            Label lblTitle = new Label();
            lblTitle.Text = "Visitors";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.Location = new Point(20, 15);
            lblTitle.AutoSize = true;
            panel.Controls.Add(lblTitle);
            
            Label lblVisitors = new Label();
            lblVisitors.Text = "98,425k";
            lblVisitors.Font = new Font("Segoe UI", 24, FontStyle.Bold);
            lblVisitors.ForeColor = Color.White;
            lblVisitors.Location = new Point(20, 50);
            lblVisitors.AutoSize = true;
            panel.Controls.Add(lblVisitors);
            
            Label lblPercentage = new Label();
            lblPercentage.Text = "+2.5%";
            lblPercentage.Font = new Font("Segoe UI", 9);
            lblPercentage.ForeColor = accentGreen;
            lblPercentage.Location = new Point(20, 90);
            lblPercentage.AutoSize = true;
            panel.Controls.Add(lblPercentage);
            
            Label lblPeriod = new Label();
            lblPeriod.Text = "From last week";
            lblPeriod.Font = new Font("Segoe UI", 8);
            lblPeriod.ForeColor = textDark;
            lblPeriod.Location = new Point(65, 91);
            lblPeriod.AutoSize = true;
            panel.Controls.Add(lblPeriod);
            
            Button btnMore = new Button();
            btnMore.Text = "View More";
            btnMore.FlatStyle = FlatStyle.Flat;
            btnMore.FlatAppearance.BorderColor = Color.FromArgb(60, 60, 60);
            btnMore.BackColor = Color.Transparent;
            btnMore.ForeColor = textLight;
            btnMore.Size = new Size(80, 28);
            btnMore.Location = new Point(panel.Width - 110, 15);
            btnMore.Font = new Font("Segoe UI", 8);
            btnMore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            panel.Controls.Add(btnMore);
            
            // Round the corners
            ApplyRoundedCorners(panel);
            
            return panel;
        }

        private Image CreateSparkline(Color color)
        {
            // Create a simple sparkline image for demonstration
            Bitmap bmp = new Bitmap(80, 40);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Generate some random points for the sparkline
                Random rnd = new Random();
                Point[] points = new Point[10];
                for (int i = 0; i < 10; i++)
                {
                    points[i] = new Point(i * 8, 20 + rnd.Next(-15, 15));
                }
                
                using (Pen pen = new Pen(color, 2))
                {
                    g.DrawLines(pen, points);
                }
            }
            return bmp;
        }
        
        private Image CreateLogoImage()
        {
            // Create a simple colorful logo
            Bitmap bmp = new Bitmap(120, 40);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                
                // Draw colorful circles
                int size = 20;
                int spacing = 5;
                
                Color[] colors = { accentPurple, accentBlue, accentGreen };
                
                for (int i = 0; i < 3; i++)
                {
                    using (SolidBrush brush = new SolidBrush(colors[i]))
                    {
                        g.FillEllipse(brush, 10 + (i * (size + spacing)), 10, size, size);
                    }
                }
                
                // Add text
                using (Font font = new Font("Segoe UI", 14, FontStyle.Bold))
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    g.DrawString("Geex", font, brush, new PointF(65, 8));
                }
            }
            return bmp;
        }
        
        private void ApplyRoundedCorners(Control control)
        {
            control.Paint += (s, e) => {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                
                using (GraphicsPath path = new GraphicsPath())
                {
                    int radius = 8;
                    Rectangle rect = new Rectangle(0, 0, control.Width - 1, control.Height - 1);
                    
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
                    using (SolidBrush brush = new SolidBrush(control.BackColor))
                    {
                        g.FillPath(brush, path);
                    }
                    
                    // Draw border
                    using (Pen pen = new Pen(Color.FromArgb(45, 45, 50), 1))
                    {
                        g.DrawPath(pen, path);
                    }
                }
            };
        }
        
        private void LoadData()
        {
            // Load sample data for the charts and panels
            serverRequestChart.LoadData();
            serverStatusChart.LoadData();
            chartSummaryPanel.LoadData();
            recentProblems.LoadData();
        }
        
        // Stat Card Control
        private class StatCard : Panel
        {
            private Label lblTitle;
            private Label lblValue;
            private Label lblPercentage;
            private PictureBox sparkline;
            
            public StatCard(string title, string value, string percentage, Image sparklineImage)
            {
                this.BackColor = Color.FromArgb(32, 34, 40);
                this.Padding = new Padding(20);
                
                // Title
                this.lblTitle = new Label();
                this.lblTitle.Text = title;
                this.lblTitle.Font = new Font("Segoe UI", 9);
                this.lblTitle.ForeColor = Color.FromArgb(180, 180, 180);
                this.lblTitle.Location = new Point(20, 15);
                this.lblTitle.AutoSize = true;
                this.Controls.Add(this.lblTitle);
                
                // Value
                this.lblValue = new Label();
                this.lblValue.Text = value;
                this.lblValue.Font = new Font("Segoe UI", 22, FontStyle.Bold);
                this.lblValue.ForeColor = Color.White;
                this.lblValue.Location = new Point(20, 40);
                this.lblValue.AutoSize = true;
                this.Controls.Add(this.lblValue);
                
                // Percentage
                this.lblPercentage = new Label();
                this.lblPercentage.Text = percentage;
                this.lblPercentage.AutoSize = true;
                this.lblPercentage.Font = new Font("Segoe UI", 9);
                this.lblPercentage.Location = new Point(20, 80);
                
                // Set color based on trend
                if (percentage.StartsWith("+"))
                    this.lblPercentage.ForeColor = Color.FromArgb(0, 200, 170); // Green
                else
                    this.lblPercentage.ForeColor = Color.FromArgb(255, 99, 88); // Red
                    
                this.Controls.Add(this.lblPercentage);
                
                // Sparkline
                this.sparkline = new PictureBox();
                this.sparkline.Image = sparklineImage;
                this.sparkline.SizeMode = PictureBoxSizeMode.Zoom;
                this.sparkline.Size = new Size(80, 40);
                this.sparkline.Location = new Point(this.Width - 100, 40);
                this.sparkline.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                this.sparkline.BackColor = Color.Transparent;
                this.Controls.Add(this.sparkline);
                
                // Round the corners
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
                        using (Pen pen = new Pen(Color.FromArgb(45, 45, 50), 1))
                        {
                            g.DrawPath(pen, path);
                        }
                    }
                };
            }
        }
    }
} 