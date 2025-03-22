using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ParkingOut.UI
{
    /// <summary>
    /// Represents a menu item in the sidebar.
    /// </summary>
    public class MenuItem : INotifyPropertyChanged
    {
        #region Fields

        private string _text;
        private string _iconPath;
        private string _tag;
        private bool _isActive;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the text of the menu item.
        /// </summary>
        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon path for the menu item.
        /// </summary>
        public string IconPath
        {
            get => _iconPath;
            set
            {
                if (_iconPath != value)
                {
                    _iconPath = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the tag identifying the menu item.
        /// </summary>
        public string Tag
        {
            get => _tag;
            set
            {
                if (_tag != value)
                {
                    _tag = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the menu item is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        /// <param name="text">The text of the menu item.</param>
        /// <param name="iconPath">The icon path for the menu item.</param>
        /// <param name="tag">The tag identifying the menu item.</param>
        public MenuItem(string text, string iconPath, string tag)
        {
            _text = text;
            _iconPath = iconPath;
            _tag = tag;
            _isActive = false;
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 