using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace SimpleParkingAdmin
{
    public class LineChartPanel : Panel
    {
        private string title;
        private List<ChartDataPoint> dataPoints1 = new List<ChartDataPoint>();
        private List<ChartDataPoint> dataPoints2 = new List<ChartDataPoint>();
        
        // Colors
        private Color backgroundColor = Color.FromArgb(32, 34, 40);
        private Color gridColor = Color.FromArgb(50, 50, 55);
        private Color line1Color = Color.FromArgb(0, 200, 170); // Teal
        private Color line2Color = Color.FromArgb(255, 168, 0); // Orange
        
        // Legend
        private bool showLegend = true;
        private string series1Name = "Product One";
        private string series2Name = "Product Two";
        
        public LineChartPanel(string title = "Chart")
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
            this.Paint += LineChartPanel_Paint;
        }
        
        private void LineChartPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int padding = 30;
            int legendHeight = showLegend ? 30 : 0;
            
            // Define chart area
            Rectangle chartArea = new Rectangle(
                padding, 
                padding + 20, // Extra space for title
                this.Width - (padding * 2), 
                this.Height - (padding * 2) - 20 - legendHeight);
            
            // Draw title
            using (Font titleFont = new Font("Segoe UI", 12, FontStyle.Bold))
            using (SolidBrush titleBrush = new SolidBrush(Color.White))
            {
                g.DrawString(title, titleFont, titleBrush, new PointF(padding, 15));
            }
            
            // Draw chart border
            using (Pen borderPen = new Pen(gridColor, 1))
            {
                g.DrawRectangle(borderPen, chartArea);
            }
            
            // Draw grid lines
            DrawGrid(g, chartArea);
            
            // Draw axis labels
            DrawAxisLabels(g, chartArea);
            
            // Draw data lines
            if (dataPoints1.Count > 1)
                DrawDataLine(g, chartArea, dataPoints1, line1Color);
                
            if (dataPoints2.Count > 1)
                DrawDataLine(g, chartArea, dataPoints2, line2Color);
            
            // Draw legend
            if (showLegend)
                DrawLegend(g, chartArea);
        }
        
        private void DrawGrid(Graphics g, Rectangle chartArea)
        {
            using (Pen gridPen = new Pen(gridColor, 1))
            {
                // Vertical grid lines
                int vLines = 7;
                for (int i = 0; i <= vLines; i++)
                {
                    int x = chartArea.Left + (i * chartArea.Width / vLines);
                    g.DrawLine(gridPen, x, chartArea.Top, x, chartArea.Bottom);
                }
                
                // Horizontal grid lines
                int hLines = 4;
                for (int i = 0; i <= hLines; i++)
                {
                    int y = chartArea.Top + (i * chartArea.Height / hLines);
                    g.DrawLine(gridPen, chartArea.Left, y, chartArea.Right, y);
                }
            }
        }
        
        private void DrawAxisLabels(Graphics g, Rectangle chartArea)
        {
            using (Font labelFont = new Font("Segoe UI", 8))
            using (SolidBrush labelBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                // X-axis labels
                string[] xLabels = { "14:00", "14:10", "14:20", "14:30", "14:40", "14:50", "15:00", "15:10", "15:20", "15:30" };
                
                int vLines = 7;
                for (int i = 0; i <= vLines; i++)
                {
                    int x = chartArea.Left + (i * chartArea.Width / vLines);
                    int labelIndex = (int)((float)i / vLines * (xLabels.Length - 1));
                    
                    string label = xLabels[labelIndex];
                    SizeF labelSize = g.MeasureString(label, labelFont);
                    
                    g.DrawString(label, labelFont, labelBrush, 
                        new PointF(x - (labelSize.Width / 2), chartArea.Bottom + 5));
                }
                
                // Y-axis labels
                string[] yLabels = { "0", "25", "50", "75" };
                
                int hLines = 3;
                for (int i = 0; i <= hLines; i++)
                {
                    int y = chartArea.Bottom - (i * chartArea.Height / hLines);
                    string label = yLabels[i];
                    SizeF labelSize = g.MeasureString(label, labelFont);
                    
                    g.DrawString(label, labelFont, labelBrush, 
                        new PointF(chartArea.Left - labelSize.Width - 5, y - (labelSize.Height / 2)));
                }
            }
        }
        
        private void DrawDataLine(Graphics g, Rectangle chartArea, List<ChartDataPoint> points, Color lineColor)
        {
            if (points.Count < 2)
                return;
                
            // Find min and max values
            float minX = points[0].X;
            float maxX = points[0].X;
            float minY = points[0].Y;
            float maxY = points[0].Y;
            
            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y);
                maxY = Math.Max(maxY, point.Y);
            }
            
            // Ensure maxY is at least 10 to avoid division by zero
            maxY = Math.Max(maxY, 10);
            
            // Create points for drawing
            Point[] linePoints = new Point[points.Count];
            
            for (int i = 0; i < points.Count; i++)
            {
                // Convert data point to screen coordinates
                float xRatio = (points[i].X - minX) / (maxX - minX);
                float yRatio = (points[i].Y - minY) / (maxY - minY);
                
                int x = chartArea.Left + (int)(xRatio * chartArea.Width);
                int y = chartArea.Bottom - (int)(yRatio * chartArea.Height);
                
                linePoints[i] = new Point(x, y);
            }
            
            // Draw the line
            using (Pen linePen = new Pen(lineColor, 3))
            {
                linePen.LineJoin = LineJoin.Round;
                g.DrawCurve(linePen, linePoints, 0.5f);
            }
            
            // Draw points
            using (SolidBrush pointBrush = new SolidBrush(lineColor))
            {
                foreach (var point in linePoints)
                {
                    g.FillEllipse(pointBrush, point.X - 4, point.Y - 4, 8, 8);
                }
            }
        }
        
        private void DrawLegend(Graphics g, Rectangle chartArea)
        {
            int legendY = chartArea.Bottom + 20;
            int legendX = chartArea.Left + (chartArea.Width / 2) - 100;
            
            // Draw series 1 legend
            using (SolidBrush markerBrush = new SolidBrush(line1Color))
            using (Font legendFont = new Font("Segoe UI", 9))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                g.FillRectangle(markerBrush, legendX, legendY, 12, 12);
                g.DrawString(series1Name, legendFont, textBrush, new PointF(legendX + 20, legendY - 2));
            }
            
            // Draw series 2 legend
            using (SolidBrush markerBrush = new SolidBrush(line2Color))
            using (Font legendFont = new Font("Segoe UI", 9))
            using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                g.FillRectangle(markerBrush, legendX + 120, legendY, 12, 12);
                g.DrawString(series2Name, legendFont, textBrush, new PointF(legendX + 140, legendY - 2));
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
            dataPoints1.Clear();
            dataPoints2.Clear();
            
            // Create sample data with random trend
            Random random = new Random();
            int pointCount = 20;
            
            for (int i = 0; i < pointCount; i++)
            {
                float x = i;
                
                // Series 1 - smooth wave
                float y1 = 25 + (float)(Math.Sin(i * 0.4) * 15) + random.Next(-5, 6);
                dataPoints1.Add(new ChartDataPoint(x, Math.Max(0, y1)));
                
                // Series 2 - different pattern
                float y2 = 20 + (float)(Math.Cos(i * 0.3) * 12) + random.Next(-4, 5);
                dataPoints2.Add(new ChartDataPoint(x, Math.Max(0, y2)));
            }
        }
    }
    
    public class ChartDataPoint
    {
        public float X { get; set; }
        public float Y { get; set; }
        
        public ChartDataPoint(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
} 