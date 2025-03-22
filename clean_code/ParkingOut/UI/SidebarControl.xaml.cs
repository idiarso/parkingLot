using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NLog;

namespace ParkingOut.UI
{
    /// <summary>
    /// Interaction logic for SidebarControl.xaml
    /// </summary>
    public partial class SidebarControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        #region Events

        /// <summary>
        /// Event that is raised when a menu item is clicked.
        /// </summary>
        public event EventHandler<ParkingOut.UI.MenuItem>? MenuItemClicked;

        /// <summary>
        /// Event that is raised when the logout button is clicked.
        /// </summary>
        public event EventHandler? LogoutClicked;

        #endregion
        
        #region Private Fields

        private const double ExpandedWidth = 250;
        private const double CollapsedWidth = 60;
        private bool _isCollapsed;
        private string _title = "ParkingOut";
        private string _userName = "Administrator";
        private ImageSource? _logoSource;
        private List<System.Windows.Controls.Button> _menuButtons = new List<System.Windows.Controls.Button>();
        private ParkingOut.UI.MenuItem? _activeMenuItem;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether the sidebar is collapsed.
        /// </summary>
        public bool IsCollapsed
        {
            get => _isCollapsed;
            set
            {
                if (_isCollapsed != value)
                {
                    _isCollapsed = value;
                    OnPropertyChanged(nameof(IsCollapsed));
                    AnimateSidebar();
                }
            }
        }

        /// <summary>
        /// Gets or sets the title displayed in the sidebar header.
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        /// <summary>
        /// Gets or sets the user name displayed in the sidebar footer.
        /// </summary>
        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged(nameof(UserName));
                }
            }
        }

        /// <summary>
        /// Gets or sets the logo image source.
        /// </summary>
        public ImageSource? LogoSource
        {
            get => _logoSource;
            set
            {
                if (_logoSource != value)
                {
                    _logoSource = value;
                    OnPropertyChanged(nameof(LogoSource));
                }
            }
        }

        /// <summary>
        /// Gets or sets the active menu item.
        /// </summary>
        public ParkingOut.UI.MenuItem? ActiveMenuItem
        {
            get => _activeMenuItem;
            set
            {
                if (_activeMenuItem != value)
                {
                    // Deactivate the previous item
                    if (_activeMenuItem != null)
                        _activeMenuItem.IsActive = false;
                    
                    _activeMenuItem = value;
                    
                    // Activate the new item
                    if (_activeMenuItem != null)
                        _activeMenuItem.IsActive = true;
                    
                    OnPropertyChanged(nameof(ActiveMenuItem));
                }
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SidebarControl"/> class.
        /// </summary>
        public SidebarControl()
        {
            try
            {
                InitializeComponent();
                DataContext = this;
                Width = ExpandedWidth;
                PrepareAnimations();
                logger.Debug("SidebarControl initialized");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error initializing SidebarControl");
                System.Windows.MessageBox.Show($"Error initializing sidebar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a menu item to the sidebar.
        /// </summary>
        /// <param name="menuItem">The menu item to add.</param>
        public void AddMenuItem(ParkingOut.UI.MenuItem menuItem)
        {
            if (menuItem == null)
                throw new ArgumentNullException(nameof(menuItem));

            try
            {
                var button = new System.Windows.Controls.Button
                {
                    Content = menuItem.Text,
                    Tag = menuItem.IconPath,
                    Style = FindResource("MenuItemStyle") as Style,
                    DataContext = menuItem
                };

                button.Click += MenuButton_Click;
                _menuButtons.Add(button);
                MenuItemsPanel.Children.Add(button);
                logger.Debug("Added menu item: {MenuItem}", menuItem.Text);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error adding menu item: {MenuItem}", menuItem.Text);
                throw;
            }
        }

        /// <summary>
        /// Adds multiple menu items to the sidebar.
        /// </summary>
        /// <param name="menuItems">The menu items to add.</param>
        public void AddMenuItems(IEnumerable<ParkingOut.UI.MenuItem> menuItems)
        {
            if (menuItems == null)
                throw new ArgumentNullException(nameof(menuItems));

            foreach (var menuItem in menuItems)
            {
                AddMenuItem(menuItem);
            }
        }

        /// <summary>
        /// Toggles the sidebar between collapsed and expanded states.
        /// </summary>
        public void ToggleSidebar()
        {
            IsCollapsed = !IsCollapsed;
        }

        /// <summary>
        /// Sets the active menu item.
        /// </summary>
        /// <param name="menuItem">The menu item to set as active.</param>
        public void SetActiveMenuItem(ParkingOut.UI.MenuItem menuItem)
        {
            ActiveMenuItem = menuItem;
        }

        /// <summary>
        /// Clears all menu items from the sidebar.
        /// </summary>
        public void ClearMenuItems()
        {
            try
            {
                foreach (var button in _menuButtons)
                {
                    button.Click -= MenuButton_Click;
                }

                _menuButtons.Clear();
                MenuItemsPanel.Children.Clear();
                ActiveMenuItem = null;
                logger.Debug("Cleared all menu items");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error clearing menu items");
                throw;
            }
        }

        #endregion

        #region Private Methods

        private void AnimateSidebar()
        {
            try
            {
                var targetWidth = IsCollapsed ? CollapsedWidth : ExpandedWidth;
                var widthAnimation = new DoubleAnimation
                {
                    To = targetWidth,
                    Duration = TimeSpan.FromMilliseconds(250),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                BeginAnimation(WidthProperty, widthAnimation);
                logger.Debug("Sidebar animation: {IsCollapsed}", IsCollapsed ? "Collapsed" : "Expanded");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error animating sidebar");
            }
        }

        private void PrepareAnimations()
        {
            // Set initial width without animation
            Width = ExpandedWidth;
        }

        #endregion

        #region Event Handlers

        private void ToggleSidebarButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ToggleSidebar();
        }

        private void LogoutButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            LogoutClicked?.Invoke(this, EventArgs.Empty);
        }

        private void MenuButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            try 
            {
                if (sender is System.Windows.Controls.Button button && button.DataContext is ParkingOut.UI.MenuItem menuItem)
                {
                    ActiveMenuItem = menuItem;
                    MenuItemClicked?.Invoke(this, menuItem);
                    logger.Debug("Menu item clicked: {MenuItem}", menuItem.Text);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling menu button click");
                System.Windows.MessageBox.Show($"Error handling menu: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        /// <summary>
        /// Event that is raised when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 