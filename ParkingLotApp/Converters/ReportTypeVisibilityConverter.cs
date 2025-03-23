using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ParkingLotApp.Converters
{
    public class ReportTypeVisibilityConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string currentType && parameter is string expectedType)
            {
                return currentType.Equals(expectedType, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 