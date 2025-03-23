using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ParkingLotApp.Converters
{
    public class DictionaryValueConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Dictionary<string, int> dictionary && parameter is string key)
            {
                // Jika kunci ditemukan, kembalikan nilainya
                if (dictionary.TryGetValue(key, out int result))
                {
                    return result;
                }
                
                // Jika kunci tidak ditemukan, kembalikan 0
                return 0;
            }
            
            // Default value jika input tidak valid
            return 0;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 