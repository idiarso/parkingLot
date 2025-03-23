using System;

namespace ParkingOut.Utils.Services
{
    public interface IAppLogger
    {
        void LogError(string message);
        void LogError(string message, Exception ex);
        void LogInfo(string message);
        void LogWarning(string message);
    }
}
