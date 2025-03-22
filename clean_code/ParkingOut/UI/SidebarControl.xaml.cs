using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NLog;
using System.Linq;

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
        /// <param name="text">The text of the menu item.</param>
        /// <param name="iconPath">The icon path for the menu item.</param>
        /// <param name="tag">The tag identifying the menu item.</param>
        /// <returns>The created menu item.</returns>
        public MenuItem AddMenuItem(string text, string iconPath, string tag)
        {
            try
            {
                logger.Debug($"Adding menu item: {text} with tag {tag}");
                
                // Create menu item
                var menuItem = new MenuItem(text, iconPath, tag);
                
                // Create button for the menu item
                var button = new Button
                {
                    Style = (Style)FindResource("MenuButtonStyle"),
                    Tag = tag,
                    Margin = new Thickness(2, 5, 2, 5)
                };
                
                // Create grid for button content
                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                
                // Create icon
                var iconPath1 = new System.Windows.Shapes.Path
                {
                    Data = System.Windows.Media.Geometry.Parse(iconPath),
                    Fill = System.Windows.Media.Brushes.White,
                    Stretch = Stretch.Uniform,
                    Width = 16,
                    Height = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                // Create text block
                var textBlock = new TextBlock
                {
                    Text = text,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontSize = 14,
                    Margin = new Thickness(10, 0, 0, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                // Add elements to grid
                Grid.SetColumn(iconPath1, 0);
                Grid.SetColumn(textBlock, 1);
                grid.Children.Add(iconPath1);
                grid.Children.Add(textBlock);
                
                // Set button content
                button.Content = grid;
                
                // Add click handler
                button.Click += MenuButton_Click;
                
                // Add button to panel
                MenuItemsPanel.Children.Add(button);
                
                // Add button to collection
                _menuButtons.Add(button);
                
                logger.Debug($"Menu item added: {text}");
                
                return menuItem;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error adding menu item: {text}");
                throw;
            }
        }
        
        /// <summary>
        /// Gets a menu item by its tag.
        /// </summary>
        /// <param name="tag">The tag of the menu item to get.</param>
        /// <returns>The menu item with the specified tag, or null if not found.</returns>
        public MenuItem? GetMenuItem(string tag)
        {
            try
            {
                logger.Debug($"Getting menu item with tag: {tag}");
                
                // Create menu item based on tag
                var button = _menuButtons.FirstOrDefault(b => b.Tag.ToString() == tag);
                if (button == null)
                {
                    logger.Warn($"Menu item with tag {tag} not found");
                    return null;
                }
                
                var grid = button.Content as Grid;
                if (grid == null)
                {
                    logger.Warn($"Button content is not a Grid for tag {tag}");
                    return null;
                }
                
                var textBlock = grid.Children.OfType<TextBlock>().FirstOrDefault();
                if (textBlock == null)
                {
                    logger.Warn($"TextBlock not found in button content for tag {tag}");
                    return null;
                }
                
                var path = grid.Children.OfType<System.Windows.Shapes.Path>().FirstOrDefault();
                string iconPath = path?.Data?.ToString() ?? string.Empty;
                
                return new MenuItem(textBlock.Text, iconPath, tag);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error getting menu item with tag: {tag}");
                return null;
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
                AddMenuItem(menuItem.Text, menuItem.IconPath, menuItem.Tag);
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
                logger.Debug("Menu button clicked");
                
                var button = sender as Button;
                if (button == null)
                {
                    logger.Warn("Sender is not a Button");
                    return;
                }
                
                string tag = button.Tag.ToString() ?? string.Empty;
                
                var menuItem = GetMenuItem(tag);
                if (menuItem == null)
                {
                    logger.Warn($"Menu item with tag {tag} not found");
                    return;
                }
                
                // Set active menu item
                ActiveMenuItem = menuItem;
                
                // Raise event
                MenuItemClicked?.Invoke(this, menuItem);
                
                logger.Debug($"MenuItemClicked event raised for {menuItem.Text}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in MenuButton_Click");
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