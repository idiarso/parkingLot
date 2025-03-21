using System;

namespace SimpleParkingAdmin.Utils
{
    public interface IAppLogger
    {
        void Information(string message);
        void Warning(string message);
        void Error(string message);
        void Error(string message, Exception ex);
        void Debug(string message);
        void Fatal(string message);
        void Fatal(string message, Exception ex);
    }
} 