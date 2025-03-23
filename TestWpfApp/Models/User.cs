using System;

namespace TestWpfApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsAuthenticated => !string.IsNullOrEmpty(Username);
    }
}
