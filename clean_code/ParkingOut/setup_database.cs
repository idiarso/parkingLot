using System;
using System.IO;
using System.Xml.Serialization;
using MySql.Data.MySqlClient;
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
                
                Console.Write("Port (default: 3306): ");
                string portStr = Console.ReadLine();
                int port = string.IsNullOrEmpty(portStr) ? 3306 : int.Parse(portStr);
                
                Console.Write("Database (default: parking_system): ");
                string database = Console.ReadLine();
                if (string.IsNullOrEmpty(database)) database = "parking_system";
                
                Console.Write("Username (default: root): ");
                string username = Console.ReadLine();
                if (string.IsNullOrEmpty(username)) username = "root";
                
                Console.Write("Password: ");
                string password = Console.ReadLine();
                
                // Build connection string
                string connectionString = $"Server={server};Port={port};Database={database};Uid={username};Pwd={password};Allow User Variables=True;SslMode=none;AllowPublicKeyRetrieval=true;";
                
                // Test the connection
                Console.WriteLine("\nTesting connection...");
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Successfully connected to the database!");
                    
                    // Create database if it doesn't exist
                    CreateDatabaseIfNotExists(connection, database);
                    
                    // Create users table and add default user
                    CreateDefaultUser(connection);
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
        
        static void CreateDatabaseIfNotExists(MySqlConnection connection, string databaseName)
        {
            try
            {
                Console.WriteLine($"\nChecking if database '{databaseName}' exists...");
                
                string createDbQuery = $"CREATE DATABASE IF NOT EXISTS `{databaseName}`";
                using (MySqlCommand cmd = new MySqlCommand(createDbQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
                
                Console.WriteLine($"Database '{databaseName}' is ready.");
                
                // Use the database
                string useDbQuery = $"USE `{databaseName}`";
                using (MySqlCommand cmd = new MySqlCommand(useDbQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating/using database: {ex.Message}");
                throw;
            }
        }
        
        static void CreateDefaultUser(MySqlConnection connection)
        {
            try
            {
                Console.WriteLine("\nCreating default user tables and admin account...");
                
                // Define queries to create tables and add default user
                string[] setupQueries = new string[]
                {
                    // Create users table (new schema)
                    @"CREATE TABLE IF NOT EXISTS users (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        username VARCHAR(50) NOT NULL UNIQUE,
                        password VARCHAR(255) NOT NULL,
                        nama VARCHAR(100),
                        role VARCHAR(20) NOT NULL,
                        active TINYINT(1) DEFAULT 1
                    )",
                    
                    // Create t_user table (old schema)
                    @"CREATE TABLE IF NOT EXISTS t_user (
                        id INT AUTO_INCREMENT PRIMARY KEY,
                        username VARCHAR(50) NOT NULL UNIQUE,
                        password VARCHAR(100) NOT NULL,
                        nama VARCHAR(100),
                        role VARCHAR(20) DEFAULT 'operator',
                        status INT DEFAULT 1,
                        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
                    )",
                    
                    // Add default admin to users table
                    @"INSERT IGNORE INTO users (username, password, nama, role)
                      VALUES ('admin', 'admin123', 'Administrator', 'admin')",
                    
                    // Add default admin to t_user table
                    @"INSERT IGNORE INTO t_user (username, password, nama, role, status)
                      VALUES ('admin', 'admin123', 'Administrator', 'admin', 1)"
                };
                
                // Execute each query
                foreach (string query in setupQueries)
                {
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
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
                    Port = port
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
                
                Console.WriteLine($"Settings saved to {settingsPath}");
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
        public string DatabaseName { get; set; } = "parking_system";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 3306;
    }
} 