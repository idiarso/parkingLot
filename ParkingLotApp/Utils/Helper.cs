using System;
using System.Text.RegularExpressions;

namespace ParkingLotApp.Utils
{
    public static class Helper
    {
        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalDays >= 1)
            {
                return $"{(int)duration.TotalDays} hari {duration.Hours} jam {duration.Minutes} menit";
            }
            else if (duration.TotalHours >= 1)
            {
                return $"{duration.Hours} jam {duration.Minutes} menit";
            }
            else
            {
                return $"{duration.Minutes} menit";
            }
        }

        public static bool IsValidLicensePlate(string licensePlate)
        {
            if (string.IsNullOrWhiteSpace(licensePlate))
                return false;

            // Simplified pattern for example purposes - adjust as needed
            var pattern = @"^[A-Z0-9\s-]{4,12}$";
            return Regex.IsMatch(licensePlate, pattern);
        }

        public static string FormatCurrency(decimal amount)
        {
            return $"Rp {amount:N0}";
        }
    }
} 