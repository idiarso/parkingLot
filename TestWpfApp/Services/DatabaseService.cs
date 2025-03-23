using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using System.Security.Cryptography;
using System.Text;

namespace TestWpfApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            // Default connection string - untuk production sebaiknya disimpan di konfigurasi
            _connectionString = "Host=localhost;Port=5432;Database=parkingdb;Username=postgres;Password=root@rsi";
        }

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Models.User?> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Cari user berdasarkan username
                    string commandText = "SELECT * FROM users WHERE username = @username";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("username", username);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                string storedPassword = reader["password"].ToString();
                                string storedSalt = reader["salt"].ToString();

                                // Jika password disimpan dalam bentuk hash
                                if (!string.IsNullOrEmpty(storedSalt))
                                {
                                    // Validasi password dengan salt
                                    string hashedPassword = HashPassword(password, storedSalt);
                                    if (hashedPassword == storedPassword)
                                    {
                                        // Login berhasil, buat objek User
                                        return new Models.User
                                        {
                                            Id = Convert.ToInt32(reader["id"]),
                                            Username = reader["username"].ToString(),
                                            DisplayName = reader["display_name"].ToString(),
                                            Role = reader["role"].ToString(),
                                            LastLogin = DateTime.Now
                                        };
                                    }
                                }
                                else
                                {
                                    // Fallback jika password disimpan tanpa hash (tidak disarankan untuk production)
                                    if (password == storedPassword)
                                    {
                                        return new Models.User
                                        {
                                            Id = Convert.ToInt32(reader["id"]),
                                            Username = reader["username"].ToString(),
                                            DisplayName = reader["display_name"].ToString(),
                                            Role = reader["role"].ToString(),
                                            LastLogin = DateTime.Now
                                        };
                                    }
                                }
                            }
                        }
                    }

                    // Update last login time jika berhasil login
                    // Dilakukan di method terpisah setelah login berhasil
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
            }

            return null;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    string commandText = "UPDATE users SET last_login = @lastLogin WHERE id = @userId";
                    using (var command = new NpgsqlCommand(commandText, connection))
                    {
                        command.Parameters.AddWithValue("lastLogin", DateTime.Now);
                        command.Parameters.AddWithValue("userId", userId);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating last login: {ex.Message}");
            }
        }

        // Metode untuk membuat hash password dengan salt
        private string HashPassword(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = Encoding.UTF8.GetBytes(saltedPassword);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        // Metode untuk membuat salt untuk password baru
        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Metode untuk membuat tabel users jika belum ada
        public async Task EnsureUserTableExistsAsync()
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Cek apakah tabel users sudah ada
                    string checkTableCommand = @"
                        SELECT EXISTS (
                            SELECT FROM information_schema.tables 
                            WHERE table_schema = 'public' 
                            AND table_name = 'users'
                        )";

                    bool tableExists = false;
                    using (var command = new NpgsqlCommand(checkTableCommand, connection))
                    {
                        tableExists = (bool)await command.ExecuteScalarAsync();
                    }

                    if (!tableExists)
                    {
                        // Buat tabel users jika belum ada
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

                        // Tambahkan user default (admin)
                        await CreateDefaultAdminUserAsync(connection);
                    }
                    else
                    {
                        // Periksa apakah ada user admin
                        string checkAdminCommand = "SELECT COUNT(*) FROM users WHERE username = 'admin'";
                        int adminCount = 0;
                        using (var command = new NpgsqlCommand(checkAdminCommand, connection))
                        {
                            adminCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                        }

                        if (adminCount == 0)
                        {
                            // Tambahkan user default jika belum ada
                            await CreateDefaultAdminUserAsync(connection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring user table exists: {ex.Message}");
            }
        }

        private async Task CreateDefaultAdminUserAsync(NpgsqlConnection connection)
        {
            // Untuk demo, buat user admin dengan password yang sama seperti di versi statis
            string salt = GenerateSalt();
            string hashedPassword = HashPassword("password123", salt);

            string insertCommand = @"
                INSERT INTO users (username, password, salt, display_name, role, last_login)
                VALUES (@username, @password, @salt, @displayName, @role, @lastLogin)";

            using (var command = new NpgsqlCommand(insertCommand, connection))
            {
                command.Parameters.AddWithValue("username", "admin");
                command.Parameters.AddWithValue("password", hashedPassword);
                command.Parameters.AddWithValue("salt", salt);
                command.Parameters.AddWithValue("displayName", "Administrator");
                command.Parameters.AddWithValue("role", "Admin");
                command.Parameters.AddWithValue("lastLogin", DBNull.Value);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
