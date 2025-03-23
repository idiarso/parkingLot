using System;
using System.Collections.Generic;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ParkingOut.Models;

namespace ParkingOut.Utils
{
    public sealed class UserManager
    {
        private static readonly Lazy<UserManager> instance = new Lazy<UserManager>(() => new UserManager());
        private User currentUser;
        private readonly IAppLogger _logger = CustomLogManager.GetLogger();

        public static UserManager Instance => instance.Value;

        private UserManager() { }

        public User CurrentUser
        {
            get => currentUser;
            set => currentUser = value;
        }

        public bool IsAuthenticated => CurrentUser != null;

        /// <summary>
        /// Authenticates a user against the database
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Plain text password</param>
        /// <returns>User object if authentication successful, null otherwise</returns>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            try
            {
                _logger.Information($"Attempting to authenticate user: {username}");

                // Hash password using SHA-256
                string hashedPassword = ComputeSHA256Hash(password);

                // Query user from database
                var parameters = new Dictionary<string, object>
                {
                    { "@username", username },
                    { "@password", hashedPassword }
                };

                DataTable result = Database.ExecuteQuery(
                    "SELECT * FROM users WHERE username = @username AND password = @password AND status = TRUE", 
                    parameters);

                // If user found
                if (result != null && result.Rows.Count > 0)
                {
                    _logger.Information($"User {username} authenticated successfully");
                    DataRow userRow = result.Rows[0];

                    // Create user object
                    User user = new User
                    {
                        Id = Convert.ToInt32(userRow["id"]),
                        Username = username,
                        Nama = userRow["nama"]?.ToString(),
                        Role = userRow["role"]?.ToString(),
                        Level = userRow["level"]?.ToString(),
                        Status = Convert.ToBoolean(userRow["status"]),
                        LastLogin = DateTime.Now
                    };

                    // Update last login
                    Database.ExecuteNonQuery(
                        "UPDATE users SET last_login = NOW() WHERE id = @id",
                        new Dictionary<string, object> { { "@id", user.Id } });

                    // Set as current user
                    CurrentUser = user;
                    return user;
                }

                _logger.Warning($"Authentication failed for user {username}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error during authentication for user {username}: {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// Ensures the users table exists in the database
        /// </summary>
        public void EnsureUsersTableExists()
        {
            try
            {
                _logger.Info("Checking if users table exists");
                
                // Check if users table exists
                var tableExists = Database.ExecuteQuery(
                    "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'users')");
                
                if (tableExists.Rows.Count == 0 || Convert.ToBoolean(tableExists.Rows[0][0]) == false)
                {
                    _logger.Info("Users table does not exist, creating it");
                    
                    // Create users table
                    Database.ExecuteNonQuery(@"
                        CREATE TABLE users (
                            id SERIAL PRIMARY KEY,
                            username VARCHAR(50) NOT NULL UNIQUE,
                            password VARCHAR(100) NOT NULL,
                            nama VARCHAR(100),
                            role VARCHAR(20) NOT NULL,
                            level VARCHAR(20),
                            status BOOLEAN DEFAULT TRUE,
                            last_login TIMESTAMP,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        )");
                    
                    // Create default admin user
                    var hashedPassword = ComputeSHA256Hash("admin");
                    Database.ExecuteNonQuery(@"
                        INSERT INTO users (username, password, nama, role, level, status)
                        VALUES (@username, @password, @nama, @role, @level, @status)",
                        new Dictionary<string, object>
                        {
                            { "@username", "admin" },
                            { "@password", hashedPassword },
                            { "@nama", "Administrator" },
                            { "@role", "admin" },
                            { "@level", "super" },
                            { "@status", true }
                        });
                    
                    _logger.Info("Users table created with default admin user");
                }
                else
                {
                    _logger.Info("Users table already exists");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error ensuring users table exists: {ex.Message}", ex);
            }
        }

        private void CreateDefaultAdminUser()
        {
            try
            {
                _logger.Information("Creating default admin user...");

                // Hash for password123
                string hashedPassword = ComputeSHA256Hash("password123");

                var parameters = new Dictionary<string, object>
                {
                    { "@username", "admin" },
                    { "@password", hashedPassword },
                    { "@nama", "Administrator" },
                    { "@role", "Admin" },
                    { "@level", "Super" }
                };

                Database.ExecuteNonQuery(
                    "INSERT INTO users (username, password, nama, role, level, status) VALUES (@username, @password, @nama, @role, @level, TRUE)",
                    parameters);

                _logger.Information("Default admin user created successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating default admin user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Computes SHA-256 hash for a password
        /// </summary>
        public string ComputeSHA256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(permission))
                return false;

            // Admin has all permissions
            if (CurrentUser.IsAdmin)
                return true;

            // Check permissions based on role and level
            switch (CurrentUser.Role?.ToLower())
            {
                case "admin":
                    return true;
                case "operator":
                    // Basic operators can only view and perform entry/exit operations
                    return permission.ToLower() == "view" || 
                           permission.ToLower() == "entry" || 
                           permission.ToLower() == "exit";
                case "manager":
                    // Managers can do everything except user management
                    return permission.ToLower() != "user_management";
                default:
                    return false;
            }
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}