using System;
using TestWpfApp.Models;

namespace TestWpfApp
{
    /// <summary>
    /// Singleton class to maintain user session information across the application
    /// </summary>
    public static class UserSession
    {
        private static Models.User _currentUser;
        private static bool _isUserLoggedIn;

        public static Models.User CurrentUser
        {
            get => _currentUser ?? (_currentUser = new Models.User());
            set => _currentUser = value;
        }

        public static bool IsUserLoggedIn
        {
            get => _isUserLoggedIn || (CurrentUser?.IsAuthenticated ?? false);
            set => _isUserLoggedIn = value;
        }

        public static void Logout()
        {
            _currentUser = new Models.User();
            _isUserLoggedIn = false;
        }
    }
}
