using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SimpleParkingAdmin.UI
{
    /// <summary>
    /// Converts a boolean value to its inverse.
    /// </summary>
    public class BooleanInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }
    }

    /// <summary>
    /// Converts a boolean value to a Visibility enum value.
    /// If parameter is "Inverse", the conversion is inverted.
    /// </summary>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter as string == "Inverse";
            if (value is bool boolValue)
            {
                boolValue = isInverse ? !boolValue : boolValue;
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverse = parameter as string == "Inverse";
            if (value is Visibility visibility)
            {
                bool result = visibility == Visibility.Visible;
                return isInverse ? !result : result;
            }
            return false;
        }
    }

    /// <summary>
    /// Converts a sidebar width value to a margin for the main content.
    /// Used to dynamically adjust the content panel margin when the sidebar collapses or expands.
    /// </summary>
    public class SidebarWidthToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return new Thickness(width, 0, 0, 0);
            }
            return new Thickness(250, 0, 0, 0); // Default margin
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return thickness.Left;
            }
            return 250.0; // Default width
        }
    }
} 