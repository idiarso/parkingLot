using System;

namespace SimpleParkingAdmin.Models
{
    public class User
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string NamaLengkap { get; set; }
        public string Level { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLogin { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        
        public bool IsAdmin => Level?.ToLower() == "admin";
        public bool IsOperator => Level?.ToLower() == "operator";
        public bool IsSupervisor => Level?.ToLower() == "supervisor";
        
        public override string ToString()
        {
            return $"{Username} ({Level})";
        }
    }
} 