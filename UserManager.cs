using System;
using System.Data;
using Npgsql;

namespace ParkingApp
{
    public class UserManager
    {
        // Verify user login credentials
        public static bool VerifyLogin(string username, string password)
        {
            // SQL query updated for PostgreSQL 
            // Notice the parameter syntax using @ instead of ?
            string query = "SELECT COUNT(*) FROM t_user WHERE username = @username AND password = @password AND status = 1";
            
            // Create PostgreSQL parameters
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@username", username),
                new NpgsqlParameter("@password", password)
            };
            
            // Execute query and convert result to integer
            int result = Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
            
            return result > 0;
        }
        
        // Get user details by username
        public static DataTable GetUserByUsername(string username)
        {
            // SQL query for PostgreSQL
            string query = "SELECT id, username, nama, role, email FROM t_user WHERE username = @username";
            
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@username", username)
            };
            
            return DatabaseHelper.ExecuteDataTable(query, parameters);
        }
        
        // Update last login time
        public static void UpdateLastLogin(int userId)
        {
            // PostgreSQL uses NOW() function similar to MySQL
            string query = "UPDATE t_user SET last_login = NOW() WHERE id = @userId";
            
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@userId", userId)
            };
            
            DatabaseHelper.ExecuteNonQuery(query, parameters);
        }
    }
} 