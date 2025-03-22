using System;
using System.IO;
using System.Xml.Serialization;
using Npgsql;
using SimpleParkingAdmin.Utils;
using Serilog;
using Serilog.Events;
using System.Windows.Forms;

namespace SimpleParkingAdmin
{
    public partial class setup_database : Form
    {
        private readonly IAppLogger _logger = CustomLogManager.GetLogger();

        static void Main()
        {
            Console.WriteLine("ParkingOut Database Setup Utility");
            Console.WriteLine("==================================");
            
            try
            {
                // Prompt for connection details
                Console.Write("Server (default: localhost): ");
                string server = Console.ReadLine();
                if (string.IsNullOrEmpty(server)) server = "localhost";
                
                Console.Write("Port (default: 5432): ");
                string portStr = Console.ReadLine();
                int port = string.IsNullOrEmpty(portStr) ? 5432 : int.Parse(portStr);
                
                Console.Write("Database (default: parkirdb): ");
                string database = Console.ReadLine();
                if (string.IsNullOrEmpty(database)) database = "parkirdb";
                
                Console.Write("Username (default: postgres): ");
                string username = Console.ReadLine();
                if (string.IsNullOrEmpty(username)) username = "postgres";
                
                Console.Write("Password: ");
                string password = Console.ReadLine();
                
                // Build connection string
                string connectionString = $"Host={server};Port={port};Database=postgres;Username={username};Password={password};";
                
                // Test the connection
                Console.WriteLine("\nTesting connection...");
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Successfully connected to the database!");
                    
                    // Create database if it doesn't exist
                    CreateDatabaseIfNotExists(connection, database);
                    
                    // Connect to the new database
                    string dbConnectionString = $"Host={server};Port={port};Database={database};Username={username};Password={password};";
                    using (var dbConnection = new NpgsqlConnection(dbConnectionString))
                    {
                        dbConnection.Open();
                        
                        // Create users table and add default user
                        CreateDefaultUser(dbConnection);
                    }
                }
                
                // Save connection settings
                SaveSettings(server, port, database, username, password);
                
                Console.WriteLine("\nDatabase setup completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError: {ex.Message}");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        
        static void CreateDatabaseIfNotExists(NpgsqlConnection connection, string databaseName)
        {
            try
            {
                Console.WriteLine($"\nChecking if database '{databaseName}' exists...");
                
                // Check if database exists
                using (var cmd = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname", connection))
                {
                    cmd.Parameters.AddWithValue("@dbname", databaseName);
                    var result = cmd.ExecuteScalar();
                    bool dbExists = (result != null && result != DBNull.Value);
                    
                    if (!dbExists)
                    {
                        Console.WriteLine($"Database '{databaseName}' does not exist. Creating...");
                        
                        // Close any existing connections to postgres database
                        connection.Close();
                        
                        // Extract password from connection string since NpgsqlConnection doesn't expose Password property
                        string password = "";
                        var connStringParts = connection.ConnectionString.Split(';');
                        foreach (var part in connStringParts)
                        {
                            if (part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                            {
                                password = part.Substring(part.IndexOf('=') + 1);
                                break;
                            }
                        }
                        
                        using (var adminConn = new NpgsqlConnection($"Host={connection.Host};Port={connection.Port};Database=postgres;Username={connection.UserName};Password={password}"))
                        {
                            adminConn.Open();
                            // In PostgreSQL, database names cannot be parameterized
                            using (var createDbCmd = new NpgsqlCommand($"CREATE DATABASE {databaseName}", adminConn))
                            {
                                createDbCmd.ExecuteNonQuery();
                            }
                        }
                        
                        // Reopen the original connection
                        connection.Open();
                    }
                    else
                    {
                        Console.WriteLine($"Database '{databaseName}' already exists.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating database: {ex.Message}");
                throw;
            }
        }
        
        static void CreateDefaultUser(NpgsqlConnection connection)
        {
            try
            {
                Console.WriteLine("\nCreating default user tables and admin account...");
                
                // Define queries to create tables and add default user
                string[] setupQueries = new string[]
                {
                    // Create users table (new schema)
                    @"CREATE TABLE IF NOT EXISTS users (
                        id SERIAL PRIMARY KEY,
                        username VARCHAR(50) NOT NULL,
                        password VARCHAR(255) NOT NULL,
                        nama VARCHAR(100),
                        role VARCHAR(20) NOT NULL,
                        active BOOLEAN DEFAULT TRUE,
                        CONSTRAINT username_unique UNIQUE (username)
                    )",
                    
                    // Create t_user table (old schema)
                    @"CREATE TABLE IF NOT EXISTS t_user (
                        id SERIAL PRIMARY KEY,
                        username VARCHAR(50) NOT NULL,
                        password VARCHAR(100) NOT NULL,
                        nama VARCHAR(100),
                        role VARCHAR(20) DEFAULT 'operator',
                        status BOOLEAN DEFAULT TRUE,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        CONSTRAINT t_user_username_unique UNIQUE (username)
                    )",
                    
                    // Add default admin to users table
                    @"INSERT INTO users (username, password, nama, role)
                      VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin')
                      ON CONFLICT (username) DO NOTHING",
                    
                    // Add default admin to t_user table
                    @"INSERT INTO t_user (username, password, nama, role, status)
                      VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin', TRUE)
                      ON CONFLICT (username) DO NOTHING"
                };
                
                // Execute each query
                foreach (string query in setupQueries)
                {
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
                
                Console.WriteLine("Default admin user created with username 'admin' and password 'admin123'");
                Console.WriteLine("IMPORTANT: Change this password after first login!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up default user: {ex.Message}");
                throw;
            }
        }
        
        static void SaveSettings(string server, int port, string database, string username, string password)
        {
            try
            {
                Console.WriteLine("\nSaving connection settings...");
                
                // Create DatabaseNetworkSettings object
                var settings = new DatabaseNetworkSettings
                {
                    ServerIP = server,
                    DatabaseName = database,
                    Username = username,
                    Password = password,
                    Port = port,
                    DatabaseType = "PostgreSQL"
                };
                
                // Ensure config directory exists
                string configDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config");
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir);
                }
                
                // Save settings to XML file
                string settingsPath = Path.Combine(configDir, "network_settings.xml");
                var serializer = new XmlSerializer(typeof(DatabaseNetworkSettings));
                
                using (var writer = new StreamWriter(settingsPath))
                {
                    serializer.Serialize(writer, settings);
                }
                
                // Save settings to INI file for backward compatibility
                string iniPath = Path.Combine(configDir, "network.ini");
                using (var writer = new StreamWriter(iniPath))
                {
                    writer.WriteLine("# Konfigurasi untuk database jaringan");
                    writer.WriteLine("# Set DB_NETWORK=true untuk menggunakan database jaringan");
                    writer.WriteLine("DB_NETWORK=false");
                    writer.WriteLine($"DB_SERVER={server}");
                    writer.WriteLine($"DB_PORT={port}");
                    writer.WriteLine($"DB_USER={username}");
                    writer.WriteLine($"DB_PASSWORD={password}");
                    writer.WriteLine($"DB_NAME={database}");
                    writer.WriteLine($"DB_TYPE=PostgreSQL");
                }
                
                Console.WriteLine($"Settings saved to {settingsPath} and {iniPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving settings: {ex.Message}");
                throw;
            }
        }
    }
    
    [Serializable]
    public class DatabaseNetworkSettings
    {
        public string ServerIP { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "parkirdb";
        public string Username { get; set; } = "postgres";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 5432;
        public string DatabaseType { get; set; } = "PostgreSQL";
    }
} 