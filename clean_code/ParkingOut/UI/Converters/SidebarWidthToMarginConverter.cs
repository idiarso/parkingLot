using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ParkingOut.UI
{
    /// <summary>
    /// Converts a sidebar width to a margin for the content area
    /// </summary>
    public class SidebarWidthToMarginConverter : IValueConverter
    {
        /// <summary>
        /// Converts a sidebar width value to a margin for the content area.
        /// </summary>
        /// <param name="value">The width value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>A Thickness object with the left margin set to the sidebar width.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return new Thickness(width, 0, 0, 0);
            }
            
            return new Thickness(0);
        }

        /// <summary>
        /// Converts a margin back to a sidebar width value.
        /// </summary>
        /// <param name="value">The margin to convert back.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The left margin value as a double.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Thickness thickness)
            {
                return thickness.Left;
            }
            
            return 0.0;
        }
    }
} 