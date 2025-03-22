using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ParkingOut.Utils
{
    public class GradientPanel : Panel
    {
        public Color ColorTop { get; set; } = Color.FromArgb(42, 40, 60);  // Dark blue/purple
        public Color ColorBottom { get; set; } = Color.FromArgb(64, 62, 80); // Lighter purple
        public float GradientAngle { get; set; } = 90F;

        public GradientPanel()
        {
            // Set double buffering to reduce flicker
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle, ColorTop, ColorBottom, GradientAngle))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
            base.OnPaint(e);
        }
    }
} 