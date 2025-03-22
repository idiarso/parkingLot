using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ParkingOut.Utils
{
    public class ModernSidebar : UserControl
    {
        // Fields
        private GradientPanel pnlSidebar;
        private Panel pnlLogo;
        private PictureBox pbLogo;
        private Label lblAppName;
        private Button btnToggle;
        private FlowLayoutPanel pnlMenu;
        private List<ModernMenuButton> menuButtons;
        private System.Windows.Forms.Timer animationTimer;
        private int expandedWidth = 220;
        private int collapsedWidth = 60;
        private int targetWidth;
        private bool isCollapsed = false;
        
        // Events
        public event EventHandler<string> MenuItemClicked;

        // Properties
        [Category("ModernUI")]
        public int ExpandedWidth
        {
            get { return expandedWidth; }
            set
            {
                expandedWidth = value;
                if (!isCollapsed)
                {
                    Width = expandedWidth;
                    pnlSidebar.Width = expandedWidth;
                }
            }
        }

        [Category("ModernUI")]
        public int CollapsedWidth
        {
            get { return collapsedWidth; }
            set
            {
                collapsedWidth = value;
                if (isCollapsed)
                {
                    Width = collapsedWidth;
                    pnlSidebar.Width = collapsedWidth;
                }
            }
        }

        [Category("ModernUI")]
        public string ApplicationName
        {
            get { return lblAppName.Text; }
            set { lblAppName.Text = value; }
        }

        [Category("ModernUI")]
        public Image LogoImage
        {
            get { return pbLogo.Image; }
            set { pbLogo.Image = value; }
        }

        [Category("ModernUI")]
        public bool IsCollapsed
        {
            get { return isCollapsed; }
            set
            {
                if (isCollapsed != value)
                {
                    ToggleSidebar();
                }
            }
        }

        [Category("ModernUI")]
        public Color SidebarTopColor
        {
            get { return pnlSidebar.ColorTop; }
            set { pnlSidebar.ColorTop = value; }
        }

        [Category("ModernUI")]
        public Color SidebarBottomColor
        {
            get { return pnlSidebar.ColorBottom; }
            set { pnlSidebar.ColorBottom = value; }
        }

        // Constructor
        public ModernSidebar()
        {
            InitializeComponent();
            menuButtons = new List<ModernMenuButton>();
            SetupAnimation();
        }

        private void InitializeComponent()
        {
            // Main sidebar container
            pnlSidebar = new GradientPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5),
                ColorTop = Color.FromArgb(42, 40, 60),
                ColorBottom = Color.FromArgb(64, 62, 80)
            };
            
            // Logo panel
            pnlLogo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.Transparent
            };
            
            // Logo image
            pbLogo = new PictureBox
            {
                Size = new Size(40, 40),
                Location = new Point(10, 10),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
            
            // Application name label
            lblAppName = new Label
            {
                Text = "ParkingOut",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(60, 15),
                Size = new Size(150, 30),
                BackColor = Color.Transparent,
                AutoSize = false
            };
            
            // Toggle button
            btnToggle = new Button
            {
                FlatStyle = FlatStyle.Flat,
                Size = new Size(30, 30),
                Location = new Point(expandedWidth - 40, 15),
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                Text = "≡" // Hamburger icon
            };
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            btnToggle.ForeColor = Color.White;
            btnToggle.Click += BtnToggle_Click;
            
            // Menu container
            pnlMenu = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(5, 10, 5, 10),
                BackColor = Color.Transparent
            };
            
            // Add controls to the logo panel
            pnlLogo.Controls.Add(pbLogo);
            pnlLogo.Controls.Add(lblAppName);
            pnlLogo.Controls.Add(btnToggle);
            
            // Add controls to the sidebar
            pnlSidebar.Controls.Add(pnlMenu);
            pnlSidebar.Controls.Add(pnlLogo);
            
            // Configure the user control
            this.Size = new Size(expandedWidth, 600);
            this.Controls.Add(pnlSidebar);
        }

        // Animation setup
        private void SetupAnimation()
        {
            animationTimer = new System.Windows.Forms.Timer
            {
                Interval = 10 // adjust for speed
            };
            animationTimer.Tick += AnimationTimer_Tick;
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (pnlSidebar.Width == targetWidth)
            {
                animationTimer.Stop();
                return;
            }

            // Smoothly animate width change
            int step = 20;
            if (pnlSidebar.Width < targetWidth)
            {
                pnlSidebar.Width += step;
                this.Width += step;
                
                if (pnlSidebar.Width > targetWidth)
                {
                    pnlSidebar.Width = targetWidth;
                    this.Width = targetWidth;
                }
            }
            else
            {
                pnlSidebar.Width -= step;
                this.Width -= step;
                
                if (pnlSidebar.Width < targetWidth)
                {
                    pnlSidebar.Width = targetWidth;
                    this.Width = targetWidth;
                }
            }

            // Update toggle button position
            btnToggle.Left = pnlSidebar.Width - 40;
        }

        // Toggle sidebar expansion
        public void ToggleSidebar()
        {
            isCollapsed = !isCollapsed;
            targetWidth = isCollapsed ? collapsedWidth : expandedWidth;
            
            // Update UI elements based on collapsed state
            lblAppName.Visible = !isCollapsed;
            
            // Update menu buttons
            foreach (ModernMenuButton btn in menuButtons)
            {
                if (isCollapsed)
                {
                    btn.Text = "";
                }
                else
                {
                    btn.Text = btn.Tag?.ToString() ?? "";
                }
            }
            
            // Start animation
            animationTimer.Start();
        }

        private void BtnToggle_Click(object sender, EventArgs e)
        {
            ToggleSidebar();
        }

        // Add a menu item to the sidebar
        public ModernMenuButton AddMenuItem(string text, Image icon = null, string name = null)
        {
            ModernMenuButton btn = new ModernMenuButton
            {
                Text = text,
                Image = icon,
                Tag = text, // Store the full text for when we expand again
                Margin = new Padding(0, 5, 0, 0),
                Width = pnlMenu.Width - 10,
                Name = name ?? "btn" + text.Replace(" ", "")
            };
            
            btn.Click += (s, e) => 
            {
                foreach (ModernMenuButton menuBtn in menuButtons)
                {
                    menuBtn.IsActive = (menuBtn == btn);
                }
                
                // Raise the event with the clicked item's name
                MenuItemClicked?.Invoke(this, btn.Name);
            };
            
            // If sidebar is collapsed, don't show text
            if (isCollapsed)
            {
                btn.Text = "";
            }
            
            menuButtons.Add(btn);
            pnlMenu.Controls.Add(btn);
            return btn;
        }

        // Set active menu item by name
        public void SetActiveMenuItem(string name)
        {
            foreach (ModernMenuButton btn in menuButtons)
            {
                btn.IsActive = (btn.Name == name);
            }
        }
        
        // Clear all menu items
        public void ClearMenuItems()
        {
            menuButtons.Clear();
            pnlMenu.Controls.Clear();
        }
    }
} 