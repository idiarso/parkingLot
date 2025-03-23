using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ParkingLotApp.Converters
{
    public class ActionTypeColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string action)
            {
                return action.ToLower() switch
                {
                    "entry" => new SolidColorBrush(Color.Parse("#4CAF50")), // Green
                    "exit" => new SolidColorBrush(Color.Parse("#F44336")),  // Red
                    "payment" => new SolidColorBrush(Color.Parse("#2196F3")), // Blue
                    _ => new SolidColorBrush(Color.Parse("#9E9E9E")) // Gray for unknown
                };
            }
            
            return new SolidColorBrush(Color.Parse("#9E9E9E")); // Gray default
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 