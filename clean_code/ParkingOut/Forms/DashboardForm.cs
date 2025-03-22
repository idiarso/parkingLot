using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ParkingOut.Utils;
using ParkingOut.Models;

namespace ParkingOut.Forms
{
    public partial class DashboardForm : Form
    {
        private readonly User _currentUser;
        private readonly IAppLogger _logger;
        private ModernSidebar sidebar;
        private Panel contentPanel;

        // For demo purposes - would be replaced with actual forms
        private Form _activeForm = null;

        public DashboardForm(User currentUser)
        {
            _currentUser = currentUser;
            _logger = new FileLogger();
            InitializeComponent();
            SetupSidebar();
        }

        private void InitializeComponent()
        {
            // Basic form setup
            this.Text = "ParkingOut - Dashboard";
            this.Size = new Size(1024, 768);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(800, 600);

            // Content panel for child forms
            contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240)
            };
            
            // Add content panel to form
            this.Controls.Add(contentPanel);
        }

        private void SetupSidebar()
        {
            try
            {
                // Create the sidebar
                sidebar = new ModernSidebar
                {
                    Dock = DockStyle.Left,
                    ApplicationName = "ParkingOut",
                    SidebarTopColor = Color.FromArgb(42, 40, 60),     // Dark blue/purple
                    SidebarBottomColor = Color.FromArgb(64, 62, 80)   // Lighter purple
                };

                // Try to load a logo from the application directory
                string logoPath = Path.Combine(Application.StartupPath, "logo.png");
                if (File.Exists(logoPath))
                {
                    sidebar.LogoImage = Image.FromFile(logoPath);
                }

                // Add menu items - in a real implementation, you would add proper resources for icons
                sidebar.AddMenuItem("Dashboard", null, "btnDashboard");
                sidebar.AddMenuItem("Vehicle Entry", null, "btnVehicleEntry");
                sidebar.AddMenuItem("Vehicle Exit", null, "btnVehicleExit");
                sidebar.AddMenuItem("Member Management", null, "btnMemberManagement");
                sidebar.AddMenuItem("Reports", null, "btnReports");

                // Set the active menu item
                sidebar.SetActiveMenuItem("btnDashboard");

                // Handle menu item clicks
                sidebar.MenuItemClicked += Sidebar_MenuItemClicked;

                // Add the sidebar to the form (before the content panel in Z-order)
                this.Controls.Add(sidebar);
                this.Controls.SetChildIndex(sidebar, 0);
                this.Controls.SetChildIndex(contentPanel, 1);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to setup sidebar", ex);
                MessageBox.Show("Error setting up the sidebar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Sidebar_MenuItemClicked(object sender, string menuName)
        {
            try
            {
                // Handle menu item clicks and open corresponding forms
                switch (menuName)
                {
                    case "btnDashboard":
                        OpenChildForm(new Form() { Text = "Dashboard", BackColor = Color.White }); // Replace with your dashboard form
                        break;
                    case "btnVehicleEntry":
                        // OpenChildForm(new EntryForm(_currentUser));
                        MessageBox.Show("Opening Vehicle Entry Form");
                        break;
                    case "btnVehicleExit":
                        // OpenChildForm(new ExitForm(_currentUser));
                        MessageBox.Show("Opening Vehicle Exit Form");
                        break;
                    case "btnMemberManagement":
                        // OpenChildForm(new MemberForm(_currentUser));
                        MessageBox.Show("Opening Member Management Form");
                        break;
                    case "btnReports":
                        OpenChildForm(new ReportForm(_currentUser));
                        break;
                    default:
                        _logger.Warning($"Unknown menu item clicked: {menuName}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error handling menu click for {menuName}", ex);
                MessageBox.Show($"Error opening form: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenChildForm(Form childForm)
        {
            try
            {
                // Close the currently active form if one exists
                if (_activeForm != null)
                {
                    _activeForm.Close();
                }

                _activeForm = childForm;
                childForm.TopLevel = false;
                childForm.FormBorderStyle = FormBorderStyle.None;
                childForm.Dock = DockStyle.Fill;
                
                // Add the child form to the content panel
                contentPanel.Controls.Add(childForm);
                contentPanel.Tag = childForm;
                
                // Bring to front and show the form
                childForm.BringToFront();
                childForm.Show();
                
                // Update the form title
                this.Text = $"ParkingOut - {childForm.Text}";
            }
            catch (Exception ex)
            {
                _logger.Error($"Error opening child form: {childForm.Text}", ex);
                throw;
            }
        }
    }
} 