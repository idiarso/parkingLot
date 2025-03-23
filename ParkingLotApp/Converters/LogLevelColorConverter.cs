using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ParkingLotApp.Converters
{
    public class LogLevelColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string logEntry)
            {
                if (logEntry.Contains("[ERROR]"))
                    return new SolidColorBrush(Colors.Red);
                if (logEntry.Contains("[AUTH]"))
                    return new SolidColorBrush(Colors.Yellow);
                return new SolidColorBrush(Colors.LightGray);
            }
            return new SolidColorBrush(Colors.White);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 