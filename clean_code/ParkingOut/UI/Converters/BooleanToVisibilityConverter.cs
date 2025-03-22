using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ParkingOut.UI
{
    /// <summary>
    /// Converts a boolean value to a Visibility value.
    /// </summary>
    /// <remarks>
    /// When the boolean is true, the converter returns Visibility.Visible.
    /// When the boolean is false, the converter returns Visibility.Collapsed.
    /// This behavior can be inverted with the converter parameter.
    /// </remarks>
    public class BooleanToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to a Visibility value.
        /// </summary>
        /// <param name="value">The boolean value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional parameter. Use "Inverse" to invert the conversion.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>Visibility.Visible if the boolean is true, or Visibility.Collapsed if the boolean is false (unless inverted).</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverted = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) ?? false;
            bool boolValue = value is bool b && b;
            
            if (isInverted)
                boolValue = !boolValue;
                
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a Visibility value back to a boolean value.
        /// </summary>
        /// <param name="value">The Visibility value to convert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">Optional parameter. Use "Inverse" to invert the conversion.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>True if the Visibility is Visible, False if the Visibility is Collapsed (unless inverted).</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isInverted = parameter?.ToString()?.Equals("Inverse", StringComparison.OrdinalIgnoreCase) ?? false;
            bool visibilityValue = value is Visibility v && v == Visibility.Visible;
            
            if (isInverted)
                visibilityValue = !visibilityValue;
                
            return visibilityValue;
        }
    }
} 