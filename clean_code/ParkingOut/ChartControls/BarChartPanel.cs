using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SimpleParkingAdmin
{
    public class BarChartPanel : Panel
    {
        private string title;
        private List<BarData> barData = new List<BarData>();
        
        // Colors
        private Color backgroundColor = Color.FromArgb(32, 34, 40);
        private Color[] barColors = new Color[] {
            Color.FromArgb(0, 200, 170),  // Teal
            Color.FromArgb(255, 168, 0),  // Orange
            Color.FromArgb(0, 120, 212)   // Blue
        };
        
        public BarChartPanel(string title = "Chart")
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
            this.Paint += BarChartPanel_Paint;
        }
        
        private void BarChartPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int padding = 15;
            
            // Define chart area
            Rectangle chartArea = new Rectangle(
                padding, 
                padding + 20, // Extra space for title
                this.Width - (padding * 2), 
                this.Height - (padding * 2) - 20);
            
            // Draw title
            using (Font titleFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.White))
            {
                g.DrawString(title, titleFont, titleBrush, new PointF(padding, 15));
            }
            
            // Draw subtitle
            using (Font subtitleFont = new Font("Segoe UI", 8))
            using (SolidBrush subtitleBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                g.DrawString("Connected equipment data view", subtitleFont, subtitleBrush, 
                    new PointF(padding, 35));
            }
            
            if (barData.Count == 0)
                return;
                
            // Draw horizontal scale lines and labels
            DrawHorizontalScale(g, chartArea);
            
            // Draw bars
            DrawBars(g, chartArea);
            
            // Draw day labels
            DrawDayLabels(g, chartArea);
        }
        
        private void DrawHorizontalScale(Graphics g, Rectangle chartArea)
        {
            using (Pen gridPen = new Pen(Color.FromArgb(50, 50, 55), 1))
            using (Font labelFont = new Font("Segoe UI", 8))
            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                // Draw horizontal lines and labels
                string[] labels = { "0 AM", "2 AM", "4 AM", "6 AM", "8 AM", "10 AM" };
                int lineCount = labels.Length;
                
                for (int i = 0; i < lineCount; i++)
                {
                    int y = chartArea.Bottom - (i * chartArea.Height / (lineCount - 1));
                    
                    // Draw line
                    g.DrawLine(gridPen, chartArea.Left, y, chartArea.Right, y);
                    
                    // Draw label
                    SizeF labelSize = g.MeasureString(labels[i], labelFont);
                    g.DrawString(labels[i], labelFont, labelBrush, 
                        new PointF(chartArea.Left - labelSize.Width - 5, y - (labelSize.Height / 2)));
                }
            }
        }
        
        private void DrawBars(Graphics g, Rectangle chartArea)
        {
            int barCount = barData.Count;
            int barsPerDay = 3; // Three bars for each day (one for each product)
            int barWidth = 10;
            int dayWidth = (chartArea.Width / barCount) * barsPerDay;
            
            for (int i = 0; i < barData.Count; i++)
            {
                var data = barData[i];
                int dayIndex = i / barsPerDay;
                int barIndex = i % barsPerDay;
                
                // Calculate bar position
                int barX = chartArea.Left + (dayIndex * dayWidth) + (barIndex * barWidth) + 20;
                
                // Draw each value as a color-coded bar
                for (int j = 0; j < data.Values.Length; j++)
                {
                    // Scale value to fit in chart area
                    float scaledValue = (float)data.Values[j] / 100 * chartArea.Height;
                    
                    // Create bar rectangle
                    Rectangle barRect = new Rectangle(
                        barX, 
                        (int)(chartArea.Bottom - scaledValue),
                        barWidth,
                        (int)scaledValue);
                    
                    // Draw bar with color
                    using (SolidBrush barBrush = new SolidBrush(barColors[j % barColors.Length]))
                    {
                        g.FillRectangle(barBrush, barRect);
                    }
                    
                    // Move to next bar position
                    barX += barWidth + 2;
                }
            }
        }
        
        private void DrawDayLabels(Graphics g, Rectangle chartArea)
        {
            int barsPerDay = 3;
            string[] dayLabels = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            int dayCount = barData.Count / barsPerDay;
            
            using (Font labelFont = new Font("Segoe UI", 8))
            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                int dayWidth = chartArea.Width / dayCount;
                
                for (int i = 0; i < dayCount; i++)
                {
                    int x = chartArea.Left + (i * dayWidth) + (dayWidth / 2) - 10;
                    
                    g.DrawString(dayLabels[i % dayLabels.Length], labelFont, labelBrush, 
                        new PointF(x, chartArea.Bottom + 5));
                }
            }
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
            barData.Clear();
            
            // Create sample data for 7 days with 3 products each
            Random random = new Random();
            string[] days = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
            
            for (int day = 0; day < days.Length; day++)
            {
                // For each day, add 3 sets of data (for 3 different products)
                for (int product = 0; product < 3; product++)
                {
                    // Generate 3 values for each bar series
                    int[] values = new int[3];
                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = random.Next(20, 80);
                    }
                    
                    barData.Add(new BarData(days[day], values));
                }
            }
        }
    }
    
    public class BarData
    {
        public string Label { get; set; }
        public int[] Values { get; set; }
        
        public BarData(string label, int[] values)
        {
            Label = label;
            Values = values;
        }
    }
} 