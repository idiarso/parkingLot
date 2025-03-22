using System;
using System.ComponentModel;
using System.Windows.Media;

namespace ParkingOut.UI
{
    /// <summary>
    /// Represents a menu item in the sidebar.
    /// </summary>
    public class MenuItem : INotifyPropertyChanged
    {
        #region Private Fields

        private string? _text;
        private string? _iconPath;
        private string? _tag;
        private bool _isActive;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the text displayed for the menu item.
        /// </summary>
        public string Text
        {
            get { return _text ?? string.Empty; }
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged(nameof(Text));
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon path for the menu item.
        /// </summary>
        public string IconPath
        {
            get { return _iconPath ?? string.Empty; }
            set
            {
                if (_iconPath != value)
                {
                    _iconPath = value;
                    OnPropertyChanged(nameof(IconPath));
                }
            }
        }

        /// <summary>
        /// Gets or sets the tag that identifies this menu item.
        /// </summary>
        public string Tag
        {
            get { return _tag ?? string.Empty; }
            set
            {
                if (_tag != value)
                {
                    _tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged(nameof(IsActive));
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        public MenuItem()
        {
            Text = "Menu Item";
            IconPath = null;
            Tag = string.Empty;
            IsActive = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class with specified text, icon, and tag.
        /// </summary>
        /// <param name="text">The display text for the menu item.</param>
        /// <param name="iconPath">The icon path data for the menu item.</param>
        /// <param name="tag">The tag identifying the menu item.</param>
        public MenuItem(string text, string iconPath, string tag)
        {
            Text = text;
            IconPath = iconPath;
            Tag = tag;
            IsActive = false;
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