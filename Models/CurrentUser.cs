namespace SimpleParkingAdmin.Models
{
    public static class CurrentUser
    {
        public static int Id { get; private set; }
        public static string Username { get; private set; }
        public static string FullName { get; private set; }
        public static string Role { get; private set; }

        public static void SetCurrentUser(int id, string username, string fullName, string role)
        {
            Id = id;
            Username = username;
            FullName = fullName;
            Role = role;
        }

        public static void Clear()
        {
            Id = 0;
            Username = null;
            FullName = null;
            Role = null;
        }
    }
} 