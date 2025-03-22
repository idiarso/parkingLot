using System;

namespace ParkingOut.Utils
{
    public static class CustomLogManager
    {
        private static readonly IAppLogger _logger = new FileLogger();

        public static IAppLogger GetLogger()
        {
            return _logger;
        }
    }
} 