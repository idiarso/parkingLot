using System;
using System.Globalization;
using System.Windows.Data;

namespace ParkingOut.UI
{
    /// <summary>
    /// Converts a boolean value to its inverse value (true to false, false to true)
    /// </summary>
    public class BooleanInverterConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean value to its inverse.
        /// </summary>
        /// <param name="value">The boolean value to invert.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The inverse of the input boolean value.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue;
            }
            
            return value;
        }

        /// <summary>
        /// Converts the inverse boolean value back to the original value.
        /// </summary>
        /// <param name="value">The inverse boolean value to convert back.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter (not used).</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>The inverse of the input boolean value.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // The conversion back is identical to the conversion
            return Convert(value, targetType, parameter, culture);
        }
    }
} 