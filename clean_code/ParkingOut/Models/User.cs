using System;

namespace ParkingOut.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Nama { get; set; }
        public string Role { get; set; }
        public string Level { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public bool IsAdmin => Role?.ToLower() == "admin";
        
        public bool HasPermission(string permission)
        {
            // Basic permission implementation - can be expanded
            if (IsAdmin) return true;
            
            // Role-based permissions
            switch (Role?.ToLower())
            {
                case "admin":
                    return true;
                case "operator":
                    return permission.ToLower() == "view" || permission.ToLower() == "entry";
                default:
                    return false;
            }
        }
    }
} 