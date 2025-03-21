using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Data;
using SimpleParkingAdmin.Models;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin
{
    public class DashboardPanel : Panel
    {
        private Color _borderColor = Color.FromArgb(0, 120, 215);
        private int _borderRadius = 10;
        private int _borderThickness = 1;
        private bool _showTitle = true;
        private string _title = "Dashboard Panel";
        private Font _titleFont = new Font("Segoe UI", 12F, FontStyle.Bold);
        private Color _titleBackColor = Color.FromArgb(0, 120, 215);
        private Color _titleForeColor = Color.White;
        
        public DashboardPanel()
        {
            this.BackColor = Color.White;
            this.Margin = new Padding(10);
            this.Padding = new Padding(10);
            this.Paint += DashboardPanel_Paint;
        }
        
        public DashboardPanel(User currentUser) : this()
        {
            InitializeDashboard(currentUser);
        }
        
        private void InitializeDashboard(User currentUser)
        {
            // Set up dashboard layout
            this.Dock = DockStyle.Fill;
            
            // Welcome message
            Label lblWelcome = new Label();
            lblWelcome.Text = $"Welcome, {currentUser.NamaLengkap}";
            lblWelcome.Font = new Font("Segoe UI", 18, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(60, 60, 60);
            lblWelcome.Location = new Point(20, 20);
            lblWelcome.AutoSize = true;
            this.Controls.Add(lblWelcome);
            
            // Add statistics cards
            int cardWidth = 220;
            int cardHeight = 120;
            int cardSpacing = 20;
            int startX = 20;
            int startY = 80;
            
            // Create cards for various statistics
            StatisticsCard vehiclesInCard = new StatisticsCard
            {
                Title = "Vehicles In",
                Value = "0",
                Description = "Today's entries",
                Location = new Point(startX, startY),
                Size = new Size(cardWidth, cardHeight),
                TitleBackColor = Color.FromArgb(41, 128, 185)
            };
            
            StatisticsCard vehiclesOutCard = new StatisticsCard
            {
                Title = "Vehicles Out",
                Value = "0",
                Description = "Today's exits",
                Location = new Point(startX + cardWidth + cardSpacing, startY),
                Size = new Size(cardWidth, cardHeight),
                TitleBackColor = Color.FromArgb(39, 174, 96)
            };
            
            StatisticsCard parkingSpacesCard = new StatisticsCard
            {
                Title = "Parking Spaces",
                Value = "100",
                Description = "Available spaces",
                Location = new Point(startX + (cardWidth + cardSpacing) * 2, startY),
                Size = new Size(cardWidth, cardHeight),
                TitleBackColor = Color.FromArgb(192, 57, 43)
            };
            
            StatisticsCard revenueCard = new StatisticsCard
            {
                Title = "Revenue",
                Value = "$0",
                Description = "Today's revenue",
                Location = new Point(startX + (cardWidth + cardSpacing) * 3, startY),
                Size = new Size(cardWidth, cardHeight),
                TitleBackColor = Color.FromArgb(142, 68, 173)
            };
            
            this.Controls.Add(vehiclesInCard);
            this.Controls.Add(vehiclesOutCard);
            this.Controls.Add(parkingSpacesCard);
            this.Controls.Add(revenueCard);
            
            // Add a panel for recent activity - Using standard Panel instead of DashboardPanel to avoid recursion
            Panel recentActivityPanel = new Panel
            {
                Location = new Point(startX, startY + cardHeight + cardSpacing),
                Size = new Size((cardWidth + cardSpacing) * 2 - cardSpacing, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Add a header label for the recent activity panel
            Label recentActivityHeader = new Label
            {
                Text = "Recent Activity",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(10, 0, 0, 0)
            };
            recentActivityPanel.Controls.Add(recentActivityHeader);
            
            // Add a panel for parking status visualization - Using standard Panel instead of DashboardPanel to avoid recursion
            Panel parkingStatusPanel = new Panel
            {
                Location = new Point(startX + (cardWidth + cardSpacing) * 2, startY + cardHeight + cardSpacing),
                Size = new Size((cardWidth + cardSpacing) * 2 - cardSpacing, 300),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Add a header label for the parking status panel
            Label parkingStatusHeader = new Label
            {
                Text = "Parking Status",
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 120, 215),
                Padding = new Padding(10, 0, 0, 0)
            };
            parkingStatusPanel.Controls.Add(parkingStatusHeader);
            
            this.Controls.Add(recentActivityPanel);
            this.Controls.Add(parkingStatusPanel);
            
            // Add dummy data to the panels for visualization
            LoadDummyData(recentActivityPanel, parkingStatusPanel);
        }
        
        private void LoadDummyData(Panel recentActivityPanel, Panel parkingStatusPanel)
        {
            try
            {
                // Add dummy recent activity list
                ListView activityList = new ListView();
                activityList.View = View.Details;
                activityList.Dock = DockStyle.Fill;
                activityList.Margin = new Padding(10);
                activityList.FullRowSelect = true;
                
                // Add columns
                activityList.Columns.Add("Time", 80);
                activityList.Columns.Add("Event", 150);
                activityList.Columns.Add("Details", 200);
                
                try
                {
                    // Try to get actual recent activity from database
                    string query = "SELECT DATE_FORMAT(time_in, '%H:%i') as time, 'Vehicle Entry' as event, " +
                                "CONCAT('Car - ', plate_number) as details FROM parking_transactions " +
                                "ORDER BY time_in DESC LIMIT 5";
                    
                    DataTable result = Database.ExecuteQuery(query);
                    
                    if (result != null && result.Rows.Count > 0)
                    {
                        foreach (DataRow row in result.Rows)
                        {
                            string[] item = {
                                row["time"].ToString(),
                                row["event"].ToString(),
                                row["details"].ToString()
                            };
                            activityList.Items.Add(new ListViewItem(item));
                        }
                    }
                    else
                    {
                        // Add dummy items if no database entries found
                        string[] item1 = { "09:15", "Vehicle Entry", "Car - B1234CD" };
                        string[] item2 = { "09:30", "Vehicle Exit", "Car - A5678EF" };
                        string[] item3 = { "10:05", "Payment", "$5.00 - B9012GH" };
                        
                        activityList.Items.Add(new ListViewItem(item1));
                        activityList.Items.Add(new ListViewItem(item2));
                        activityList.Items.Add(new ListViewItem(item3));
                    }
                }
                catch (Exception)
                {
                    // Fallback to dummy data if database query fails
                    string[] item1 = { "09:15", "Vehicle Entry", "Car - B1234CD" };
                    string[] item2 = { "09:30", "Vehicle Exit", "Car - A5678EF" };
                    string[] item3 = { "10:05", "Payment", "$5.00 - B9012GH" };
                    
                    activityList.Items.Add(new ListViewItem(item1));
                    activityList.Items.Add(new ListViewItem(item2));
                    activityList.Items.Add(new ListViewItem(item3));
                }
                
                recentActivityPanel.Controls.Add(activityList);
                
                // Add a dummy parking status visualization
                Label parkingLabel = new Label();
                parkingLabel.Text = "Parking Space Visualization";
                parkingLabel.Dock = DockStyle.Top;
                parkingLabel.Height = 30;
                parkingLabel.TextAlign = ContentAlignment.MiddleCenter;
                
                Panel parkingVisual = new Panel();
                parkingVisual.Dock = DockStyle.Fill;
                parkingVisual.Padding = new Padding(10);
                parkingVisual.Paint += (s, e) => {
                    try
                    {
                        // Draw a simple grid of parking spaces
                        Graphics g = e.Graphics;
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        
                        int rows = 5;
                        int cols = 6;
                        int cellWidth = (parkingVisual.Width - 20) / cols;
                        int cellHeight = (parkingVisual.Height - 20) / rows;
                        
                        Random rand = new Random(123); // Fixed seed for consistent results
                        
                        for (int row = 0; row < rows; row++)
                        {
                            for (int col = 0; col < cols; col++)
                            {
                                int x = 10 + col * cellWidth;
                                int y = 10 + row * cellHeight;
                                
                                // Randomly determine if space is occupied
                                bool occupied = rand.Next(100) < 40; // 40% chance of being occupied
                                
                                Color spaceColor = occupied ? Color.FromArgb(231, 76, 60) : Color.FromArgb(46, 204, 113);
                                
                                using (SolidBrush brush = new SolidBrush(spaceColor))
                                {
                                    g.FillRectangle(brush, x, y, cellWidth - 5, cellHeight - 5);
                                }
                                
                                // Draw the space number
                                string spaceNumber = (row * cols + col + 1).ToString();
                                using (SolidBrush textBrush = new SolidBrush(Color.White))
                                {
                                    g.DrawString(spaceNumber, new Font("Arial", 10), textBrush, 
                                                x + (cellWidth - 5) / 2 - 10, y + (cellHeight - 5) / 2 - 10);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore paint errors
                    }
                };
                
                parkingStatusPanel.Controls.Add(parkingVisual);
                parkingStatusPanel.Controls.Add(parkingLabel);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in DashboardPanel: {ex.Message}");
                Logger.Error($"Stack trace: {ex.StackTrace}");
            }
        }
        
        private void DashboardPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw the rounded border
            using (GraphicsPath borderPath = CreateRoundedRectangle(0, 0, Width - 1, Height - 1, _borderRadius))
            using (Pen borderPen = new Pen(_borderColor, _borderThickness))
            {
                g.DrawPath(borderPen, borderPath);
            }
            
            // If showing title, draw the title bar
            if (_showTitle)
            {
                using (GraphicsPath titlePath = CreateRoundedRectangle(0, 0, Width - 1, 40, new int[] { _borderRadius, _borderRadius, 0, 0 }))
                using (SolidBrush titleBrush = new SolidBrush(_titleBackColor))
                {
                    g.FillPath(titleBrush, titlePath);
                    
                    // Draw title text
                    using (SolidBrush textBrush = new SolidBrush(_titleForeColor))
                    {
                        // Draw centered text
                        SizeF textSize = g.MeasureString(_title, _titleFont);
                        g.DrawString(_title, _titleFont, textBrush, 
                            new PointF(10, (40 - textSize.Height) / 2));
                    }
                }
            }
        }
        
        // Properties
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }
        
        public int BorderRadius
        {
            get { return _borderRadius; }
            set { _borderRadius = value; Invalidate(); }
        }
        
        public int BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = value; Invalidate(); }
        }
        
        public bool ShowTitle
        {
            get { return _showTitle; }
            set { _showTitle = value; Invalidate(); }
        }
        
        public string Title
        {
            get { return _title; }
            set { _title = value; Invalidate(); }
        }
        
        public Font TitleFont
        {
            get { return _titleFont; }
            set { _titleFont = value; Invalidate(); }
        }
        
        public Color TitleBackColor
        {
            get { return _titleBackColor; }
            set { _titleBackColor = value; Invalidate(); }
        }
        
        public Color TitleForeColor
        {
            get { return _titleForeColor; }
            set { _titleForeColor = value; Invalidate(); }
        }
        
        // Helper methods
        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            return CreateRoundedRectangle(x, y, width, height, new int[] { radius, radius, radius, radius });
        }
        
        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int[] cornerRadius)
        {
            int topLeftRadius = cornerRadius[0];
            int topRightRadius = cornerRadius[1];
            int bottomRightRadius = cornerRadius[2];
            int bottomLeftRadius = cornerRadius[3];
            
            GraphicsPath path = new GraphicsPath();
            
            // Top left corner
            if (topLeftRadius > 0)
            {
                path.AddArc(x, y, topLeftRadius * 2, topLeftRadius * 2, 180, 90);
            }
            else
            {
                path.AddLine(x, y, x, y);
            }
            
            // Top edge
            path.AddLine(x + topLeftRadius, y, x + width - topRightRadius, y);
            
            // Top right corner
            if (topRightRadius > 0)
            {
                path.AddArc(x + width - topRightRadius * 2, y, topRightRadius * 2, topRightRadius * 2, 270, 90);
            }
            else
            {
                path.AddLine(x + width, y, x + width, y);
            }
            
            // Right edge
            path.AddLine(x + width, y + topRightRadius, x + width, y + height - bottomRightRadius);
            
            // Bottom right corner
            if (bottomRightRadius > 0)
            {
                path.AddArc(x + width - bottomRightRadius * 2, y + height - bottomRightRadius * 2, 
                    bottomRightRadius * 2, bottomRightRadius * 2, 0, 90);
            }
            else
            {
                path.AddLine(x + width, y + height, x + width, y + height);
            }
            
            // Bottom edge
            path.AddLine(x + width - bottomRightRadius, y + height, x + bottomLeftRadius, y + height);
            
            // Bottom left corner
            if (bottomLeftRadius > 0)
            {
                path.AddArc(x, y + height - bottomLeftRadius * 2, bottomLeftRadius * 2, bottomLeftRadius * 2, 90, 90);
            }
            else
            {
                path.AddLine(x, y + height, x, y + height);
            }
            
            // Left edge
            path.AddLine(x, y + height - bottomLeftRadius, x, y + topLeftRadius);
            
            path.CloseFigure();
            return path;
        }
    }

    public class StatisticsCard : Panel
    {
        private string _value = "0";
        private string _description = "Description";
        private Image _icon = null;
        private string _title = "";
        private Color _titleBackColor = Color.FromArgb(0, 120, 215);
        private Color _borderColor = Color.FromArgb(0, 120, 215);
        private int _borderRadius = 10;
        private int _borderThickness = 1;
        
        public StatisticsCard()
        {
            this.Height = 120;
            this.Width = 240;
            this.BackColor = Color.White;
            this.Paint += StatisticsCard_Paint;
        }
        
        private void StatisticsCard_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            // Draw the rounded border
            using (GraphicsPath borderPath = CreateRoundedRectangle(0, 0, Width - 1, Height - 1, _borderRadius))
            using (Pen borderPen = new Pen(_borderColor, _borderThickness))
            {
                g.DrawPath(borderPen, borderPath);
            }
            
            // Draw value
            using (Font valueFont = new Font("Segoe UI", 24, FontStyle.Bold))
            using (SolidBrush valueBrush = new SolidBrush(Color.FromArgb(0, 120, 215)))
            {
                g.DrawString(_value, valueFont, valueBrush, new PointF(15, 25));
            }
            
            // Draw description
            using (Font descFont = new Font("Segoe UI", 10))
            using (SolidBrush descBrush = new SolidBrush(Color.FromArgb(100, 100, 100)))
            {
                g.DrawString(_description, descFont, descBrush, new PointF(15, 70));
            }
            
            // Draw icon if available
            if (_icon != null)
            {
                g.DrawImage(_icon, new Rectangle(Width - 60, 30, 48, 48));
            }
        }
        
        // Helper method to create rounded rectangle
        private GraphicsPath CreateRoundedRectangle(int x, int y, int width, int height, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            // Top left corner
            path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            
            // Top edge
            path.AddLine(x + radius, y, x + width - radius, y);
            
            // Top right corner
            path.AddArc(x + width - radius * 2, y, radius * 2, radius * 2, 270, 90);
            
            // Right edge
            path.AddLine(x + width, y + radius, x + width, y + height - radius);
            
            // Bottom right corner
            path.AddArc(x + width - radius * 2, y + height - radius * 2, radius * 2, radius * 2, 0, 90);
            
            // Bottom edge
            path.AddLine(x + width - radius, y + height, x + radius, y + height);
            
            // Bottom left corner
            path.AddArc(x, y + height - radius * 2, radius * 2, radius * 2, 90, 90);
            
            // Left edge
            path.AddLine(x, y + height - radius, x, y + radius);
            
            path.CloseFigure();
            return path;
        }
        
        public string Value
        {
            get { return _value; }
            set { _value = value; Invalidate(); }
        }
        
        public string Description
        {
            get { return _description; }
            set { _description = value; Invalidate(); }
        }
        
        public Image Icon
        {
            get { return _icon; }
            set { _icon = value; Invalidate(); }
        }
        
        public string Title
        {
            get { return _title; }
            set { _title = value; Invalidate(); }
        }
        
        public Color TitleBackColor
        {
            get { return _titleBackColor; }
            set { _titleBackColor = value; Invalidate(); }
        }
        
        public Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; Invalidate(); }
        }
    }
} 