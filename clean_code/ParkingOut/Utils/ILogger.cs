using System;

namespace ParkingOut.Utils
{
    public interface IAppLogger
    {
        void Information(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Error(string message, Exception ex);
        void Debug(string message);
        void Fatal(string message);
        void Fatal(string message, Exception ex);
    }
} 