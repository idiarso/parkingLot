using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace ParkingLotApp.Helpers
{
    public static class MainThreadHelper
    {
        public static async Task InvokeOnMainThreadAsync(Action action)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
            }
            else
            {
                await Dispatcher.UIThread.InvokeAsync(action);
            }
        }

        public static async Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                return func();
            }
            else
            {
                return await Dispatcher.UIThread.InvokeAsync(func);
            }
        }
    }
} 