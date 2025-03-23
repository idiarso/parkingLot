using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace ParkingLotApp.Converters
{
    public class BoolToConnectionColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isConnected)
            {
                return isConnected ? new SolidColorBrush(Color.Parse("#4CAF50")) : new SolidColorBrush(Color.Parse("#F44336"));
            }
            return new SolidColorBrush(Color.Parse("#FFC107")); // Warning color for unknown state
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 