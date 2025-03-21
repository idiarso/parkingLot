using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using Npgsql;

namespace ParkingIN.Utils
{
    /// <summary>
    /// A simplified database helper class that avoids complex dependencies
    /// </summary>
    public static class SimpleDatabaseHelper
    {
        private static string _connectionString;

        static SimpleDatabaseHelper()
        {
            try
            {
                // Get connection string from config file
                var connectionStringConfig = ConfigurationManager.ConnectionStrings["ParkingDBConnection"];
                if (connectionStringConfig != null)
                {
                    _connectionString = connectionStringConfig.ConnectionString;
                }
                else
                {
                    // Default connection string if not in config
                    _connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
                }
                
                // Log to a simple text file for troubleshooting
                LogToFile("Database connection string initialized: " + _connectionString);
            }
            catch (Exception ex)
            {
                LogToFile("Error initializing connection string: " + ex.Message);
                
                // Fall back to default connection string
                _connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
            }
        }
        
        /// <summary>
        /// Simple file logging mechanism
        /// </summary>
        private static void LogToFile(string message)
        {
            try
            {
                string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                
                string logFile = Path.Combine(logDirectory, "database.log");
                using (StreamWriter writer = File.AppendText(logFile))
                {
                    writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                }
            }
            catch
            {
                // Ignore logging errors
            }
        }
        
        /// <summary>
        /// Tests the database connection
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT 1";
                        command.ExecuteScalar();
                    }
                }
                
                LogToFile("Database connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                LogToFile("Database connection test failed: " + ex.Message);
                return false;
            }
        }
        
        /// <summary>
        /// Verifies user login credentials
        /// </summary>
        public static bool VerifyLogin(string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = @"
                            SELECT id, username, nama, role, status 
                            FROM t_user 
                            WHERE username = @username AND password = @password AND status = true";
                        
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Set user properties
                                LoginForm.CurrentUser.Id = reader.GetInt32(reader.GetOrdinal("id"));
                                LoginForm.CurrentUser.UserId = reader.GetInt32(reader.GetOrdinal("id"));
                                LoginForm.CurrentUser.Username = reader.GetString(reader.GetOrdinal("username"));
                                LoginForm.CurrentUser.NamaLengkap = !reader.IsDBNull(reader.GetOrdinal("nama")) ? 
                                    reader.GetString(reader.GetOrdinal("nama")) : "";
                                LoginForm.CurrentUser.Role = reader.GetString(reader.GetOrdinal("role"));
                                LoginForm.CurrentUser.Status = reader.GetBoolean(reader.GetOrdinal("status")) ? 1 : 0;
                                
                                // Log successful login
                                LogToFile($"User {username} logged in successfully");
                                return true;
                            }
                            
                            LogToFile($"Login failed for user {username}");
                            errorMessage = "Invalid username or password";
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Login error: {ex.Message}";
                LogToFile($"Login error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Executes a query and returns the results as a DataTable
        /// </summary>
        public static DataTable GetData(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                            }
                        }

                        using (var adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
                
                LogToFile($"Query executed successfully: {query}");
                return dt;
            }
            catch (Exception ex)
            {
                LogToFile($"Error executing query: {ex.Message}");
                throw;
            }
        }
    }
} 