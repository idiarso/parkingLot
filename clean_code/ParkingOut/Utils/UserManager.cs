using System;
using ParkingOut.Models;

namespace ParkingOut.Utils
{
    public sealed class UserManager
    {
        private static readonly Lazy<UserManager> instance = new Lazy<UserManager>(() => new UserManager());
        private User currentUser;

        public static UserManager Instance => instance.Value;

        private UserManager() { }

        public User CurrentUser
        {
            get => currentUser;
            set => currentUser = value;
        }

        public bool IsAuthenticated => CurrentUser != null;

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(permission))
                return false;

            // Add permission checking logic here based on user role
            return true; // Temporary return true
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
} 