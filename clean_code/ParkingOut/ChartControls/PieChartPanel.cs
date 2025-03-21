using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SimpleParkingAdmin
{
    public class PieChartPanel : Panel
    {
        private string title;
        private List<PieSlice> slices = new List<PieSlice>();
        
        // Colors
        private Color backgroundColor = Color.FromArgb(32, 34, 40);
        private Color textColor = Color.White;
        private Color legendTextColor = Color.FromArgb(180, 180, 180);
        
        public PieChartPanel(string title = "Chart")
        {
            this.title = title;
            this.BackColor = backgroundColor;
            this.Dock = DockStyle.Fill;
            
            // Enable double buffering to reduce flicker
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.UserPaint | 
                         ControlStyles.AllPaintingInWmPaint, true);
                         
            // Generate sample data on creation
            GenerateSampleData();
            
            // Handle paint event
            this.Paint += PieChartPanel_Paint;
        }
        
        private void PieChartPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int padding = 15;
            
            // Define chart area
            Rectangle chartArea = new Rectangle(
                padding, 
                padding + 30, // Extra space for title
                this.Width - (padding * 2), 
                this.Height - (padding * 2) - 30);
            
            // Draw title
            using (Font titleFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(textColor))
            {
                g.DrawString(title, titleFont, titleBrush, new PointF(padding, 15));
            }
            
            // Draw download report button
            DrawDownloadButton(g, chartArea);
            
            if (slices.Count == 0)
                return;
                
            // Calculate total value
            float total = 0;
            foreach (var slice in slices)
            {
                total += slice.Value;
            }
            
            if (total <= 0)
                return;
                
            // Draw pie chart
            Rectangle pieRect = new Rectangle(
                chartArea.Left, 
                chartArea.Top,
                chartArea.Width / 2, 
                chartArea.Height);
                
            float startAngle = 0;
            
            for (int i = 0; i < slices.Count; i++)
            {
                var slice = slices[i];
                float sweepAngle = (slice.Value / total) * 360;
                
                using (SolidBrush sliceBrush = new SolidBrush(slice.Color))
                {
                    g.FillPie(sliceBrush, pieRect, startAngle, sweepAngle);
                }
                
                startAngle += sweepAngle;
            }
            
            // Draw pie chart center (optional)
            int centerSize = pieRect.Width / 3;
            Rectangle centerRect = new Rectangle(
                pieRect.Left + (pieRect.Width - centerSize) / 2,
                pieRect.Top + (pieRect.Height - centerSize) / 2,
                centerSize,
                centerSize);
                
            using (SolidBrush centerBrush = new SolidBrush(backgroundColor))
            {
                g.FillEllipse(centerBrush, centerRect);
            }
            
            // Draw legend
            DrawLegend(g, chartArea, total);
        }
        
        private void DrawDownloadButton(Graphics g, Rectangle chartArea)
        {
            string buttonText = "Download Report";
            
            using (Font buttonFont = new Font("Segoe UI", 9))
            {
                SizeF textSize = g.MeasureString(buttonText, buttonFont);
                
                int buttonX = chartArea.Right - (int)textSize.Width - 20;
                int buttonY = 15;
                int buttonWidth = (int)textSize.Width + 20;
                int buttonHeight = 30;
                
                Rectangle buttonRect = new Rectangle(buttonX, buttonY, buttonWidth, buttonHeight);
                
                // Draw button with rounded corners
                using (GraphicsPath path = RoundedRectangle(buttonRect, 5))
                using (SolidBrush buttonBrush = new SolidBrush(Color.FromArgb(45, 45, 50)))
                {
                    g.FillPath(buttonBrush, path);
                }
                
                // Draw button text
                using (SolidBrush textBrush = new SolidBrush(textColor))
                {
                    g.DrawString(buttonText, buttonFont, textBrush, 
                        new PointF(buttonX + 10, buttonY + (buttonHeight - textSize.Height) / 2));
                }
            }
        }
        
        private void DrawLegend(Graphics g, Rectangle chartArea, float total)
        {
            int legendX = chartArea.Left + (chartArea.Width / 2) + 20;
            int legendY = chartArea.Top + 20;
            int itemHeight = 30;
            
            using (Font legendFont = new Font("Segoe UI", 10))
            using (Font percentFont = new Font("Segoe UI", 12, FontStyle.Bold))
            {
                for (int i = 0; i < slices.Count; i++)
                {
                    var slice = slices[i];
                    float percentage = (slice.Value / total) * 100;
                    
                    // Draw color box
                    Rectangle colorBox = new Rectangle(legendX, legendY, 15, 15);
                    using (SolidBrush boxBrush = new SolidBrush(slice.Color))
                    {
                        g.FillRectangle(boxBrush, colorBox);
                    }
                    
                    // Draw label and percentage
                    using (SolidBrush textBrush = new SolidBrush(legendTextColor))
                    using (SolidBrush percentBrush = new SolidBrush(textColor))
                    {
                        g.DrawString(slice.Label, legendFont, textBrush, 
                            new PointF(legendX + 25, legendY - 2));
                            
                        g.DrawString($"{percentage:0.0}%", percentFont, percentBrush, 
                            new PointF(legendX + 100, legendY - 2));
                    }
                    
                    legendY += itemHeight;
                }
            }
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
            slices.Clear();
            
            // Create sample pie slices
            slices.Add(new PieSlice("Direct", 35, Color.FromArgb(89, 54, 149)));
            slices.Add(new PieSlice("Social", 25, Color.FromArgb(237, 85, 101)));
            slices.Add(new PieSlice("Referral", 15, Color.FromArgb(250, 215, 89)));
            slices.Add(new PieSlice("Others", 25, Color.FromArgb(50, 168, 82)));
        }
    }
    
    public class PieSlice
    {
        public string Label { get; set; }
        public float Value { get; set; }
        public Color Color { get; set; }
        
        public PieSlice(string label, float value, Color color)
        {
            Label = label;
            Value = value;
            Color = color;
        }
    }
} 