using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ParkingLotApp.Converters
{
    public class CurrencyFormatConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return $"Rp {amount:N0}";
            }
            return "Rp 0";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 