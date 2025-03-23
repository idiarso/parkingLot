using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ParkingLotApp.Converters
{
    public class TimeFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime time)
            {
                return time.ToString("HH:mm");
            }
            return "N/A";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 