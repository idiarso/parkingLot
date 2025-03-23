using System;

namespace TestWpfApp.Services.Interfaces
{
    public interface IAppLogger
    {
        void Debug(string message);
        void Debug(string message, Exception ex);
        void Info(string message);
        void Info(string message, Exception ex);
        void Warning(string message);
        void Warning(string message, Exception ex);
        void Error(string message);
        void Error(string message, Exception ex);
        void Fatal(string message);
        void Fatal(string message, Exception ex);
    }
}
