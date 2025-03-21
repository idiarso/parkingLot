using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using SimpleParkingAdmin.Models;

namespace SimpleParkingAdmin.Controls
{
    public class Sidebar : Panel
    {
        private readonly Form _parentForm;
        private readonly User _currentUser;
        private readonly Panel menuPanel;
        private readonly Button toggleButton;
        private bool isCollapsed = false;
        private string activeMenu = "";

        // Color scheme
        private readonly Color primaryColor = Color.FromArgb(24, 116, 205);
        private readonly Color textColor = Color.FromArgb(45, 52, 54);
        private readonly Color bgColor = Color.FromArgb(245, 246, 250);
        private readonly Color hoverColor = Color.FromArgb(214, 228, 250);
        private readonly Color activeColor = Color.FromArgb(24, 116, 205);

        private System.Windows.Forms.Timer _timer;

        public event EventHandler<string> MenuSelected;

        public Sidebar(Form parentForm, User currentUser)
        {
            _parentForm = parentForm;
            _currentUser = currentUser;

            // Setup sidebar panel
            this.Dock = DockStyle.Left;
            this.Width = 250;
            this.BackColor = bgColor;
            this.Padding = new Padding(10);

            // Create toggle button
            toggleButton = new Button
            {
                Text = "≡",
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = primaryColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            toggleButton.FlatAppearance.BorderSize = 0;
            toggleButton.Click += ToggleButton_Click;

            // Create menu panel
            menuPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };

            // Add controls
            this.Controls.Add(menuPanel);
            this.Controls.Add(toggleButton);

            // Create menu items
            CreateMenuItems();
        }

        private void CreateMenuItems()
        {
            var menuItems = new List<(string id, string text, string icon, bool requiresAdmin)>
            {
                ("dashboard", "Dashboard", "📊", false),
                ("entry", "Vehicle Entry", "🚗", false),
                ("exit", "Vehicle Exit", "🚙", false),
                ("member", "Member Management", "👥", false),
                ("report", "Reports", "📈", false),
                ("settings", "Settings", "⚙️", true)
            };

            int yPos = 10;
            foreach (var item in menuItems)
            {
                // Skip admin-only items for non-admin users
                if (item.requiresAdmin && !_currentUser.IsAdmin)
                    continue;

                var btn = CreateMenuItem(item.id, item.text, item.icon);
                btn.Location = new Point(0, yPos);
                menuPanel.Controls.Add(btn);
                yPos += btn.Height + 5;
            }
        }

        private Button CreateMenuItem(string id, string text, string icon)
        {
            var btn = new Button
            {
                Text = $"{icon}  {text}",
                TextAlign = ContentAlignment.MiddleLeft,
                Width = menuPanel.Width - 20,
                Height = 45,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10),
                Cursor = Cursors.Hand,
                Tag = id
            };

            btn.FlatAppearance.BorderSize = 0;
            btn.Padding = new Padding(20, 0, 0, 0);

            // Set initial colors
            UpdateButtonColors(btn, id == activeMenu);

            // Add hover effect
            btn.MouseEnter += (s, e) => {
                if (id != activeMenu)
                    btn.BackColor = hoverColor;
            };
            btn.MouseLeave += (s, e) => {
                UpdateButtonColors(btn, id == activeMenu);
            };

            // Add click handler
            btn.Click += (s, e) => {
                SetActiveMenu(id);
                MenuSelected?.Invoke(this, id);
            };

            return btn;
        }

        private void UpdateButtonColors(Button btn, bool isActive)
        {
            if (isActive)
            {
                btn.BackColor = activeColor;
                btn.ForeColor = Color.White;
            }
            else
            {
                btn.BackColor = bgColor;
                btn.ForeColor = textColor;
            }
        }

        private void ToggleButton_Click(object sender, EventArgs e)
        {
            isCollapsed = !isCollapsed;
            int targetWidth = isCollapsed ? 60 : 250;

            // Update toggle button text
            toggleButton.Text = isCollapsed ? "≡" : "≡";

            // Update menu items
            foreach (Button btn in menuPanel.Controls)
            {
                string id = btn.Tag.ToString();
                string text = GetMenuText(id);
                string icon = GetMenuIcon(id);
                btn.Text = isCollapsed ? icon : $"{icon}  {text}";
                btn.Width = targetWidth - 20;
                btn.TextAlign = isCollapsed ? ContentAlignment.MiddleCenter : ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(isCollapsed ? 0 : 20, 0, 0, 0);
            }

            // Animate sidebar width
            _timer = new System.Windows.Forms.Timer();
            _timer.Interval = 10;
            int step = (targetWidth - this.Width) / 10;
            
            _timer.Tick += (s, ev) => {
                if ((step > 0 && this.Width >= targetWidth) ||
                    (step < 0 && this.Width <= targetWidth))
                {
                    this.Width = targetWidth;
                    _timer.Stop();
                    _timer.Dispose();
                }
                else
                {
                    this.Width += step;
                }
            };
            
            _timer.Start();
        }

        public void SetActiveMenu(string menuId)
        {
            activeMenu = menuId;
            foreach (Button btn in menuPanel.Controls)
            {
                string id = btn.Tag.ToString();
                UpdateButtonColors(btn, id == menuId);
            }
        }

        private string GetMenuText(string id)
        {
            return id switch
            {
                "dashboard" => "Dashboard",
                "entry" => "Vehicle Entry",
                "exit" => "Vehicle Exit",
                "member" => "Member Management",
                "report" => "Reports",
                "settings" => "Settings",
                _ => id
            };
        }

        private string GetMenuIcon(string id)
        {
            return id switch
            {
                "dashboard" => "📊",
                "entry" => "🚗",
                "exit" => "🚙",
                "member" => "👥",
                "report" => "📈",
                "settings" => "⚙️",
                _ => "📋"
            };
        }
    }
} 