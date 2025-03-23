using System;
using System.Threading.Tasks;
using Npgsql;

namespace TestWpfApp
{
    public class DatabaseCheck
    {
        private static readonly string _connectionString = "Host=localhost;Port=5432;Database=parkingdb;Username=postgres;Password=root@rsi";

        public static async Task<string> CheckDatabaseConnection()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return "Database connection successful!";
                }
            }
            catch (Exception ex)
            {
                return $"Database connection error: {ex.Message}";
            }
        }

        public static async Task<string> CheckUsersTable()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if users table exists
                    string checkTableQuery = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'users')";
                    bool tableExists;

                    using (var command = new NpgsqlCommand(checkTableQuery, connection))
                    {
                        tableExists = (bool)await command.ExecuteScalarAsync();
                    }

                    if (!tableExists)
                    {
                        return "The 'users' table does not exist in the database.";
                    }

                    // Check if admin user exists
                    string checkAdminQuery = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
                    int adminCount;

                    using (var command = new NpgsqlCommand(checkAdminQuery, connection))
                    {
                        adminCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    if (adminCount == 0)
                    {
                        return "The admin user does not exist in the users table.";
                    }

                    // Get admin user details for debugging
                    string getUserQuery = "SELECT * FROM users WHERE username = 'admin'";
                    string userDetails = "";

                    using (var command = new NpgsqlCommand(getUserQuery, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                userDetails = $"User ID: {reader["id"]}, " +
                                              $"Username: {reader["username"]}, " +
                                              $"Has Password: {!string.IsNullOrEmpty(reader["password"].ToString())}, " + 
                                              $"Has Salt: {!string.IsNullOrEmpty(reader["salt"]?.ToString() ?? "")}, " +
                                              $"Display Name: {reader["display_name"]}, " +
                                              $"Role: {reader["role"]}";
                            }
                        }
                    }

                    return $"The 'users' table exists and admin user found. {userDetails}";
                }
            }
            catch (Exception ex)
            {
                return $"Error checking users table: {ex.Message}";
            }
        }

        // Create the default admin user with simple password (no hashing)
        public static async Task<string> CreateAdminUser()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Check if table exists, if not create it
                    string checkTableQuery = "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'users')";
                    bool tableExists;

                    using (var command = new NpgsqlCommand(checkTableQuery, connection))
                    {
                        tableExists = (bool)await command.ExecuteScalarAsync();
                    }

                    if (!tableExists)
                    {
                        // Create users table
                        string createTableCommand = @"
                            CREATE TABLE users (
                                id SERIAL PRIMARY KEY,
                                username VARCHAR(50) NOT NULL UNIQUE,
                                password VARCHAR(255) NOT NULL,
                                salt VARCHAR(50),
                                display_name VARCHAR(100),
                                role VARCHAR(20),
                                last_login TIMESTAMP
                            )";

                        using (var command = new NpgsqlCommand(createTableCommand, connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // Check if admin user exists
                    string checkAdminQuery = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
                    int adminCount;

                    using (var command = new NpgsqlCommand(checkAdminQuery, connection))
                    {
                        adminCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                    }

                    if (adminCount > 0)
                    {
                        // Delete existing admin user (for testing purposes)
                        string deleteCommand = "DELETE FROM users WHERE username = 'admin'";
                        using (var command = new NpgsqlCommand(deleteCommand, connection))
                        {
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    // Create admin user with direct password (no hashing for now)
                    string insertCommand = @"
                        INSERT INTO users (username, password, display_name, role, last_login)
                        VALUES (@username, @password, @displayName, @role, @lastLogin)";

                    using (var command = new NpgsqlCommand(insertCommand, connection))
                    {
                        command.Parameters.AddWithValue("username", "admin");
                        command.Parameters.AddWithValue("password", "password123"); // Direct password without hashing
                        command.Parameters.AddWithValue("displayName", "Administrator");
                        command.Parameters.AddWithValue("role", "Admin");
                        command.Parameters.AddWithValue("lastLogin", DBNull.Value);

                        await command.ExecuteNonQueryAsync();
                    }

                    return "Admin user created successfully with direct password!";
                }
            }
            catch (Exception ex)
            {
                return $"Error creating admin user: {ex.Message}";
            }
        }
    }
}
