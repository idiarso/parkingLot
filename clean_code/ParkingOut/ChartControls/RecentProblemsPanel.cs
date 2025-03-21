using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SimpleParkingAdmin
{
    public class RecentProblemsPanel : Panel
    {
        private List<ProblemItem> problemItems = new List<ProblemItem>();
        
        // Colors
        private Color backgroundColor = Color.FromArgb(32, 34, 40);
        private Color textColor = Color.White;
        private Color subtitleColor = Color.FromArgb(180, 180, 180);
        
        public RecentProblemsPanel()
        {
            this.BackColor = backgroundColor;
            this.Dock = DockStyle.Fill;
            
            // Enable double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.UserPaint | 
                         ControlStyles.AllPaintingInWmPaint, true);
                         
            // Generate sample data
            GenerateSampleData();
            
            // Handle paint event
            this.Paint += RecentProblemsPanel_Paint;
        }
        
        private void RecentProblemsPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int padding = 15;
            
            // Draw title
            using (Font titleFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(textColor))
            {
                g.DrawString("Recent Problems", titleFont, titleBrush, new PointF(padding, 15));
            }
            
            // Draw subtitle
            using (Font subtitleFont = new Font("Segoe UI", 8))
            using (SolidBrush subtitleBrush = new SolidBrush(subtitleColor))
            {
                g.DrawString("Monitores diam atque docem temporibus", subtitleFont, subtitleBrush, 
                    new PointF(padding, 35));
            }
            
            // Draw problem items
            int y = 60;
            foreach (var item in problemItems)
            {
                // Draw item
                y = DrawProblemItem(g, item, padding, y);
                y += 10; // Add space between items
            }
        }
        
        private int DrawProblemItem(Graphics g, ProblemItem item, int x, int y)
        {
            // Draw service icon
            int iconSize = 40;
            Rectangle iconRect = new Rectangle(x, y, iconSize, iconSize);
            
            // Draw circular background
            using (SolidBrush iconBgBrush = new SolidBrush(Color.FromArgb(45, 45, 50)))
            {
                g.FillEllipse(iconBgBrush, iconRect);
            }
            
            // Draw icon letter
            using (Font iconFont = new Font("Segoe UI", 16, FontStyle.Bold))
            using (SolidBrush iconTextBrush = new SolidBrush(textColor))
            {
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                
                g.DrawString(item.IconLetter, iconFont, iconTextBrush, 
                    new RectangleF(iconRect.X, iconRect.Y, iconRect.Width, iconRect.Height), sf);
            }
            
            // Draw service name
            using (Font nameFont = new Font("Segoe UI", 10, FontStyle.Bold))
            using (SolidBrush nameBrush = new SolidBrush(textColor))
            {
                g.DrawString(item.ServiceName, nameFont, nameBrush, new PointF(x + iconSize + 10, y));
            }
            
            // Draw URL
            using (Font urlFont = new Font("Segoe UI", 8))
            using (SolidBrush urlBrush = new SolidBrush(subtitleColor))
            {
                g.DrawString(item.Url, urlFont, urlBrush, new PointF(x + iconSize + 10, y + 20));
            }
            
            // Draw status button
            string statusText = item.Status;
            Color statusColor;
            
            switch (item.Status.ToLower())
            {
                case "online":
                    statusColor = Color.FromArgb(30, 195, 139);
                    break;
                case "offline":
                    statusColor = Color.FromArgb(255, 99, 88);
                    break;
                case "warning":
                    statusColor = Color.FromArgb(255, 168, 0);
                    break;
                default:
                    statusColor = Color.FromArgb(0, 120, 212);
                    break;
            }
            
            using (Font buttonFont = new Font("Segoe UI", 8))
            {
                SizeF textSize = g.MeasureString(statusText, buttonFont);
                
                int buttonWidth = (int)textSize.Width + 20;
                int buttonHeight = 24;
                int buttonX = this.Width - buttonWidth - 20;
                int buttonY = y + (iconSize - buttonHeight) / 2;
                
                Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
                
                // Draw button with rounded corners
                using (GraphicsPath path = RoundedRectangle(buttonRect, 5))
                using (SolidBrush buttonBrush = new SolidBrush(statusColor))
                {
                    g.FillPath(buttonBrush, path);
                }
                
                // Draw button text
                using (SolidBrush textBrush = new SolidBrush(Color.White))
                {
                    g.DrawString(statusText, buttonFont, textBrush, 
                        new PointF(buttonX + 10, buttonY + (buttonHeight - textSize.Height) / 2));
                }
            }
            
            return y + iconSize;
        }
        
        private GraphicsPath RoundedRectangle(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            
            // Top left corner
            path.AddArc(rect.X, rect.Y, radius * 2, radius * 2, 180, 90);
            
            // Top edge and top right corner
            path.AddArc(rect.Right - radius * 2, rect.Y, radius * 2, radius * 2, 270, 90);
            
            // Right edge and bottom right corner
            path.AddArc(rect.Right - radius * 2, rect.Bottom - radius * 2, radius * 2, radius * 2, 0, 90);
            
            // Bottom edge and bottom left corner
            path.AddArc(rect.X, rect.Bottom - radius * 2, radius * 2, radius * 2, 90, 90);
            
            path.CloseAllFigures();
            return path;
        }
        
        public void LoadData()
        {
            // Load real data or regenerate sample data
            GenerateSampleData();
            this.Invalidate();
        }
        
        private void GenerateSampleData()
        {
            // Clear existing data
            problemItems.Clear();
            
            // Add sample problems
            problemItems.Add(new ProblemItem("G", "Google", "https://www.google.com", "Online"));
            problemItems.Add(new ProblemItem("F", "Facebook", "https://www.facebook.com", "Online"));
            problemItems.Add(new ProblemItem("Y", "Youtube", "https://www.youtube.com", "Warning"));
        }
    }
    
    public class ProblemItem
    {
        public string IconLetter { get; set; }
        public string ServiceName { get; set; }
        public string Url { get; set; }
        public string Status { get; set; }
        
        public ProblemItem(string iconLetter, string serviceName, string url, string status)
        {
            IconLetter = iconLetter;
            ServiceName = serviceName;
            Url = url;
            Status = status;
        }
    }
} 