using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SimpleParkingAdmin.Utils
{
    public class ModernMenuButton : Button
    {
        // Fields
        private bool _isActive = false;
        private Color _activeColor = Color.FromArgb(37, 116, 181);  // Blue
        private Color _inactiveColor = Color.Transparent;
        private Color _hoverColor = Color.FromArgb(60, 60, 80);     // Darker shade
        private Color _pressedColor = Color.FromArgb(37, 116, 181); // Blue
        private Color _textColor = Color.White;
        private int _borderRadius = 8;
        private int _iconSize = 24;
        private int _indicatorThickness = 4;

        // Properties with default values
        [Category("ModernUI")]
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;
                this.Invalidate(); // Force redraw
            }
        }

        [Category("ModernUI")]
        public Color ActiveColor
        {
            get { return _activeColor; }
            set
            {
                _activeColor = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public Color InactiveColor
        {
            get { return _inactiveColor; }
            set
            {
                _inactiveColor = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public Color HoverColor
        {
            get { return _hoverColor; }
            set
            {
                _hoverColor = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public Color PressedColor
        {
            get { return _pressedColor; }
            set
            {
                _pressedColor = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public Color TextForeColor
        {
            get { return _textColor; }
            set
            {
                _textColor = value;
                this.ForeColor = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public int BorderRadius
        {
            get { return _borderRadius; }
            set
            {
                _borderRadius = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public int IconSize
        {
            get { return _iconSize; }
            set
            {
                _iconSize = value;
                this.Invalidate();
            }
        }

        [Category("ModernUI")]
        public int IndicatorThickness
        {
            get { return _indicatorThickness; }
            set
            {
                _indicatorThickness = value;
                this.Invalidate();
            }
        }

        // Constructor
        public ModernMenuButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Size = new Size(180, 45);
            this.BackColor = _inactiveColor;
            this.ForeColor = _textColor;
            this.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
            this.TextAlign = ContentAlignment.MiddleLeft;
            this.ImageAlign = ContentAlignment.MiddleLeft;
            this.TextImageRelation = TextImageRelation.ImageBeforeText;
            
            // Set padding for text and image
            this.Padding = new Padding(10, 0, 0, 0);
            
            // Enable double buffering to reduce flicker
            this.SetStyle(ControlStyles.UserPaint | 
                          ControlStyles.AllPaintingInWmPaint | 
                          ControlStyles.OptimizedDoubleBuffer, true);
        }

        // Methods
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Get the current background color based on state
            Color currentBackColor = this.BackColor;

            if (_isActive)
            {
                // Draw active state
                using (SolidBrush bgBrush = new SolidBrush(_activeColor))
                {
                    // Create rounded rectangle path for background
                    Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
                    using (GraphicsPath path = CreateRoundedRectangle(rect, _borderRadius))
                    {
                        g.FillPath(bgBrush, path);
                    }
                }

                // Draw indicator on the left side
                using (SolidBrush indicatorBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(indicatorBrush, 0, 0, _indicatorThickness, this.Height);
                }
            }
            else
            {
                // Draw inactive state but with hover/pressed effects
                using (SolidBrush bgBrush = new SolidBrush(currentBackColor))
                {
                    // Create rounded rectangle path for background
                    Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
                    using (GraphicsPath path = CreateRoundedRectangle(rect, _borderRadius))
                    {
                        g.FillPath(bgBrush, path);
                    }
                }
            }

            // Draw icon if present
            if (this.Image != null)
            {
                // Calculate icon placement
                int iconLeft = this.Padding.Left;
                int iconTop = (this.Height - _iconSize) / 2;
                
                // Draw icon
                g.DrawImage(this.Image, new Rectangle(iconLeft, iconTop, _iconSize, _iconSize));
                
                // Calculate text placement
                Rectangle textRect = new Rectangle(
                    iconLeft + _iconSize + 10, 
                    0, 
                    this.Width - (iconLeft + _iconSize + 10), 
                    this.Height);
                
                // Draw text
                using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
                using (StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(this.Text, this.Font, textBrush, textRect, sf);
                }
            }
            else
            {
                // Draw only text
                Rectangle textRect = new Rectangle(
                    this.Padding.Left, 
                    0, 
                    this.Width - this.Padding.Left, 
                    this.Height);
                
                using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
                using (StringFormat sf = new StringFormat() { LineAlignment = StringAlignment.Center })
                {
                    g.DrawString(this.Text, this.Font, textBrush, textRect, sf);
                }
            }
        }

        // Helper method to create rounded rectangle
        private GraphicsPath CreateRoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            
            // Check if radius is valid (not too large for the rectangle)
            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;
            
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));
            
            // Top left corner
            path.AddArc(arc, 180, 90);
            
            // Top right corner
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            
            // Bottom right corner
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            
            // Bottom left corner
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            
            path.CloseFigure();
            return path;
        }

        // Override mouse events to update appearance
        protected override void OnMouseEnter(EventArgs e)
        {
            if (!_isActive)
                this.BackColor = _hoverColor;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (!_isActive)
                this.BackColor = _inactiveColor;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!_isActive)
                this.BackColor = _pressedColor;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (!_isActive)
                this.BackColor = _hoverColor;
            base.OnMouseUp(e);
        }
    }
} 