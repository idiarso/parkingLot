using System;

namespace ParkingIN.Models
{
    public class User
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string NamaLengkap { get; set; }
        public string Role { get; set; }
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        public bool IsAdmin => Role?.Equals("Admin", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsOperator => Role?.Equals("Operator", StringComparison.OrdinalIgnoreCase) ?? false;
        public bool IsSupervisor => Role?.Equals("Supervisor", StringComparison.OrdinalIgnoreCase) ?? false;
        
        public override string ToString()
        {
            return $"{Username} ({NamaLengkap})";
        }
    }
} 