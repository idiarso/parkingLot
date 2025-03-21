using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using ParkingIN.Utils;

namespace ParkingIN.Models
{
    public static class UserManager
    {
        /// <summary>
        /// Verifies user login credentials
        /// </summary>
        public static bool VerifyLogin(string username, string password)
        {
            try
            {
                // Use the Database utility to execute a query
                var parameters = new Dictionary<string, object>
                {
                    { "username", username },
                    { "password", password }
                };

                // Query to check if user exists with given credentials
                var query = @"
                    SELECT id, username, nama, role, status 
                    FROM t_user 
                    WHERE username = @username AND password = @password AND status = 1";

                var dt = Database.GetData(query, parameters);
                
                if (dt != null && dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    
                    // Set current user properties
                    LoginForm.CurrentUser.Id = Convert.ToInt32(row["id"]);
                    LoginForm.CurrentUser.UserId = Convert.ToInt32(row["id"]);
                    LoginForm.CurrentUser.Username = row["username"].ToString();
                    LoginForm.CurrentUser.NamaLengkap = Convert.IsDBNull(row["nama"]) ? "" : row["nama"].ToString();
                    LoginForm.CurrentUser.Role = row["role"].ToString();
                    LoginForm.CurrentUser.Status = Convert.ToInt32(row["status"]);
                    
                    // Update last login timestamp
                    UpdateLastLogin(LoginForm.CurrentUser.Id);
                    
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error during login: {ex.Message}", "Login Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return false;
            }
        }
        
        /// <summary>
        /// Updates the last login timestamp for a user
        /// </summary>
        private static void UpdateLastLogin(int userId)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    { "userId", userId }
                };
                
                var query = "UPDATE t_user SET last_login = CURRENT_TIMESTAMP WHERE id = @userId";
                Database.ExecuteNonQuery(query, parameters);
            }
            catch (Exception)
            {
                // Ignore errors updating last login - not critical
            }
        }
        
        /// <summary>
        /// Gets all users in the system
        /// </summary>
        public static List<User> GetAllUsers()
        {
            var users = new List<User>();
            
            try
            {
                var query = "SELECT id, username, nama, role, status, last_login, created_at FROM t_user ORDER BY username";
                var dt = Database.GetData(query);
                
                foreach (DataRow row in dt.Rows)
                {
                    var user = new User
                    {
                        Id = Convert.ToInt32(row["id"]),
                        UserId = Convert.ToInt32(row["id"]),
                        Username = row["username"].ToString(),
                        NamaLengkap = Convert.IsDBNull(row["nama"]) ? "" : row["nama"].ToString(),
                        Role = row["role"].ToString(),
                        Status = Convert.ToInt32(row["status"]),
                        CreatedAt = Convert.ToDateTime(row["created_at"])
                    };
                    
                    users.Add(user);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Error retrieving users: {ex.Message}", "Error", 
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            
            return users;
        }
    }
} 