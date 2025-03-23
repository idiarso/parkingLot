using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestWpfApp.Models;
using TestWpfApp.Services.Interfaces;
using Npgsql;

namespace TestWpfApp.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;
        private readonly IAppLogger _logger;
        private bool _isConnected;

        public DatabaseService(IAppLogger logger)
        {
            _logger = logger;
            _connectionString = "Host=localhost;Port=5432;Database=parking_db;Username=postgres;Password=postgres";
            _isConnected = false;
        }

        public bool IsConnected => _isConnected;

        public async Task<bool> ConnectAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _isConnected = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Database connection error: {ex.Message}", ex);
                _isConnected = false;
                return false;
            }
        }

        public async Task<bool> IsCameraOnlineAsync()
        {
            // Simulate camera status check
            return true;
        }

        public async Task<bool> IsPrinterReadyAsync()
        {
            // Simulate printer status check
            return true;
        }

        public async Task<ParkingStatistics> GetParkingStatisticsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string commandText = @"
                        SELECT 
                            (SELECT COUNT(*) FROM parking_spots) as total_spots,
                            (SELECT COUNT(*) FROM parking_spots WHERE occupied = true) as occupied_spots,
                            (SELECT COUNT(*) FROM parking_spots WHERE occupied = false) as available_spots,
                            (SELECT COUNT(*) FROM transactions WHERE created_at >= CURRENT_DATE) as today_transactions,
                            (SELECT SUM(amount) FROM transactions WHERE created_at >= CURRENT_DATE) as today_revenue,
                            (SELECT SUM(amount) FROM transactions WHERE created_at >= CURRENT_DATE - INTERVAL '7 days') as week_revenue,
                            (SELECT SUM(amount) FROM transactions WHERE created_at >= CURRENT_DATE - INTERVAL '1 month') as month_revenue,
                            (SELECT COUNT(*) FROM transactions WHERE vehicle_type = 'car') as cars,
                            (SELECT COUNT(*) FROM transactions WHERE vehicle_type = 'motorcycle') as motorcycles,
                            (SELECT COUNT(*) FROM transactions WHERE vehicle_type = 'bus') as buses
                        ";

                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new ParkingStatistics
                                {
                                    TotalSpots = reader.GetInt32(0),
                                    OccupiedSpots = reader.GetInt32(1),
                                    AvailableSpots = reader.GetInt32(2),
                                    TodayTransactions = reader.GetInt32(3),
                                    TodayRevenue = reader.GetDecimal(4),
                                    WeekRevenue = reader.GetDecimal(5),
                                    MonthRevenue = reader.GetDecimal(6),
                                    VehicleTypes = new Dictionary<string, int>
                                    {
                                        { "car", reader.GetInt32(7) },
                                        { "motorcycle", reader.GetInt32(8) },
                                        { "bus", reader.GetInt32(9) }
                                    }
                                };
                            }
                        }
                    }
                }
                return new ParkingStatistics();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting parking statistics: {ex.Message}", ex);
                return new ParkingStatistics();
            }
        }

        public async Task<List<ActivityLogItem>> GetRecentActivityLogsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string commandText = "SELECT * FROM activity_logs ORDER BY created_at DESC LIMIT 10";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var logs = new List<ActivityLogItem>();
                            while (await reader.ReadAsync())
                            {
                                logs.Add(new ActivityLogItem
                                {
                                    Id = reader.GetInt32(0),
                                    Type = reader.GetString(1),
                                    Message = reader.GetString(2),
                                    CreatedAt = reader.GetDateTime(3)
                                });
                            }
                            return logs;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting activity logs: {ex.Message}", ex);
                return new List<ActivityLogItem>();
            }
        }

        public async Task<bool> LogActivityAsync(ActivityLogItem activity)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string commandText = "INSERT INTO activity_logs (type, message, created_at) VALUES (@type, @message, @created_at)";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("type", activity.Type);
                        command.Parameters.AddWithValue("message", activity.Message);
                        command.Parameters.AddWithValue("created_at", activity.CreatedAt);
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error logging activity: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<bool> EnsureUserTableExistsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Create users table if it doesn't exist
                    string createTableSql = @"
                        CREATE TABLE IF NOT EXISTS users (
                            id SERIAL PRIMARY KEY,
                            username VARCHAR(50) UNIQUE NOT NULL,
                            password_hash VARCHAR(255) NOT NULL,
                            salt VARCHAR(255) NOT NULL,
                            display_name VARCHAR(100) NOT NULL,
                            role VARCHAR(20) NOT NULL,
                            last_login TIMESTAMP,
                            created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                        )
                        ";

                    using (var command = new NpgsqlCommand(createTableSql, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }

                    // Create admin user if it doesn't exist
                    string checkAdminSql = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
                    using (var command = new NpgsqlCommand(checkAdminSql, connection))
                    {
                        int adminCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                        if (adminCount == 0)
                        {
                            string createAdminSql = @"
                                INSERT INTO users (username, password_hash, salt, display_name, role)
                                VALUES (@username, @password_hash, @salt, @display_name, @role)
                                ";

                            string password = "password123";
                            string salt = Guid.NewGuid().ToString();
                            string hashedPassword = HashPassword(password, salt);

                            using (var command = new NpgsqlCommand(createAdminSql, connection))
                            {
                                command.Parameters.AddWithValue("username", "admin");
                                command.Parameters.AddWithValue("password_hash", hashedPassword);
                                command.Parameters.AddWithValue("salt", salt);
                                command.Parameters.AddWithValue("display_name", "Administrator");
                                command.Parameters.AddWithValue("role", "admin");
                                await command.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error ensuring user table exists: {ex.Message}", ex);
                return false;
            }
        }

        public async Task<User?> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string commandText = "SELECT * FROM users WHERE username = @username";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("username", username);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string storedPassword = reader.GetString("password_hash");
                                string storedSalt = reader.GetString("salt");

                                if (!string.IsNullOrEmpty(storedSalt))
                                {
                                    string hashedPassword = HashPassword(password, storedSalt);
                                    if (hashedPassword == storedPassword)
                                    {
                                        return new User
                                        {
                                            Id = reader.GetInt32("id"),
                                            Username = reader.GetString("username"),
                                            DisplayName = reader.GetString("display_name"),
                                            Role = reader.GetString("role"),
                                            LastLogin = reader.GetDateTime("last_login")
                                        };
                                    }
                                }
                                else
                                {
                                    if (password == storedPassword)
                                    {
                                        return new User
                                        {
                                            Id = reader.GetInt32("id"),
                                            Username = reader.GetString("username"),
                                            DisplayName = reader.GetString("display_name"),
                                            Role = reader.GetString("role"),
                                            LastLogin = reader.GetDateTime("last_login")
                                        };
                                    }
                                }
                            }
                        }
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to authenticate user: {ex.Message}", ex);
                return null;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(User user)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string commandText = "UPDATE users SET last_login = @last_login WHERE id = @id";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("last_login", DateTime.Now);
                        command.Parameters.AddWithValue("id", user.Id);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to update last login: {ex.Message}", ex);
                return false;
            }
        }

        private string HashPassword(string password, string salt)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = System.Text.Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
