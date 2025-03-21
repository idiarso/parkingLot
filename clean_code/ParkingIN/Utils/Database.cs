using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;
using System.IO;
using Serilog;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace ParkingIN.Utils
{
    public static class Database
    {
        private static string _connectionString;
        private static readonly ILogger _logger;
        private static ClientWebSocket _webSocket;
        private static string _webSocketUrl = "ws://localhost:8181";
        private static bool _useWebSocket = false; // Default to not use WebSocket
        private static readonly object _lockObject = new object();
        private static bool _isConnected = false;

        static Database()
        {
            try
            {
                // Initialize logger first to ensure we can log errors
                try
                {
                    _logger = Log.ForContext("SourceContext", "Database");
                }
                catch
                {
                    // If logger initialization fails, continue without it
                    // We'll handle output differently
                }

                // Initialize connection string from config
                try
                {
                    var connectionStringConfig = ConfigurationManager.ConnectionStrings["ParkingDBConnection"];
                    if (connectionStringConfig != null)
                    {
                        _connectionString = connectionStringConfig.ConnectionString;
                        LogInfo("Database connection string initialized from config");
                    }
                    else
                    {
                        // Fall back to default
                        InitializeConnectionString();
                    }
                }
                catch (Exception ex)
                {
                    LogError($"Failed to read connection string from config: {ex.Message}");
                    InitializeConnectionString();
                }
                
                // Try to initialize WebSocket if needed, but don't let failures stop database initialization
                try
                {
                    // Check if we should use WebSocket instead of direct database connection
                    if (ConfigurationManager.AppSettings["UseWebSocket"] != null)
                    {
                        bool useWs = false;
                        if (bool.TryParse(ConfigurationManager.AppSettings["UseWebSocket"], out useWs))
                        {
                            _useWebSocket = useWs;
                        }
                    }
                    
                    if (_useWebSocket)
                    {
                        string websocketUrl = ConfigurationManager.AppSettings["WebSocketUrl"];
                        if (!string.IsNullOrEmpty(websocketUrl))
                        {
                            _webSocketUrl = websocketUrl;
                            LogInfo($"Using WebSocket at {_webSocketUrl}");
                            
                            // Initialize WebSocket connection
                            Task.Run(async () => await InitializeWebSocketAsync()).Wait(TimeSpan.FromSeconds(5));
                        }
                    }
                    else
                    {
                        LogInfo("Using direct database connection");
                    }
                }
                catch (Exception ex)
                {
                    // Don't let WebSocket initialization failure stop database initialization
                    LogError($"WebSocket initialization failed: {ex.Message}");
                    _useWebSocket = false;
                }
            }
            catch (Exception ex)
            {
                LogError($"Critical error in Database initialization: {ex.Message}");
                MessageBox.Show($"Database initialization error: {ex.Message}\n\nStack trace: {ex.StackTrace}", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Attempt to initialize with default connection string as a last resort
                try
                {
                    InitializeConnectionString();
                }
                catch
                {
                    // At this point we can't do much more
                }
            }
        }

        // Helper methods for logging to handle the case where Serilog might not be initialized
        private static void LogInfo(string message)
        {
            try
            {
                _logger?.Information(message);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private static void LogError(string message, Exception ex = null)
        {
            try
            {
                if (ex != null)
                    _logger?.Error(ex, message);
                else
                    _logger?.Error(message);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        /// <summary>
        /// Ensures the database exists and creates it if needed
        /// </summary>
        public static bool EnsureDatabaseExists(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                // Test connection to PostgreSQL server
                if (!TestConnection(out errorMessage))
                {
                    LogError($"Failed to connect to PostgreSQL server: {errorMessage}");
                    return false;
                }
                LogInfo("Successfully connected to PostgreSQL server");

                // Get database name from connection string
                string databaseName = GetDatabaseName(_connectionString);
                if (string.IsNullOrEmpty(databaseName))
                {
                    errorMessage = "Database name not found in connection string";
                    LogError(errorMessage);
                    return false;
                }

                // Create database if it doesn't exist (need admin connection to postgres database)
                string serverConnectionString = RemoveDatabaseFromConnectionString(_connectionString);
                using (var connection = new NpgsqlConnection(serverConnectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        // Check if database exists
                        command.CommandText = "SELECT 1 FROM pg_database WHERE datname = @dbname";
                        command.Parameters.AddWithValue("@dbname", databaseName);
                        
                        bool exists = command.ExecuteScalar() != null;
                        
                        if (!exists)
                        {
                            // Create the database if it doesn't exist
                            using (var createCmd = connection.CreateCommand())
                            {
                                createCmd.CommandText = $"CREATE DATABASE {databaseName}";
                                createCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // Initialize database structure
                if (!EnsureDatabaseStructure(out errorMessage))
                {
                    LogError($"Failed to initialize database structure: {errorMessage}");
                    return false;
                }

                LogInfo("Database initialization completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                LogError("Failed to ensure database exists", ex);
                return false;
            }
        }

        private static string GetDatabaseName(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            return builder.Database;
        }

        private static string RemoveDatabaseFromConnectionString(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            builder.Database = "postgres"; // Connect to default postgres database
            return builder.ConnectionString;
        }

        private static bool EnsureDatabaseStructure(out string errorMessage)
        {
            errorMessage = string.Empty;
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        // Create users table
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS t_user (
                                id SERIAL PRIMARY KEY,
                                username VARCHAR(50) NOT NULL,
                                password VARCHAR(255) NOT NULL,
                                nama VARCHAR(100) DEFAULT NULL,
                                role VARCHAR(20) DEFAULT 'OPERATOR',
                                status INTEGER DEFAULT 1,
                                last_login TIMESTAMP DEFAULT NULL,
                                email VARCHAR(100) DEFAULT NULL,
                                created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                                UNIQUE (username)
                            )";
                        command.ExecuteNonQuery();

                        // Create t_tarif table
                        command.CommandText = @"
                            CREATE TABLE IF NOT EXISTS t_tarif (
                                id SERIAL PRIMARY KEY,
                                jenis_kendaraan VARCHAR(50) NOT NULL,
                                tarif_awal DECIMAL(10,2) NOT NULL DEFAULT 0.00,
                                tarif_per_jam DECIMAL(10,2) NOT NULL DEFAULT 0.00
                            )";
                        command.ExecuteNonQuery();

                        // Insert default admin user if not exists
                        command.CommandText = @"
                            INSERT INTO t_user (username, password, nama, role, status)
                            SELECT 'admin', 'admin', 'Administrator', 'ADMIN', 1
                            WHERE NOT EXISTS (SELECT 1 FROM t_user WHERE username = 'admin')";
                        command.ExecuteNonQuery();

                        // Insert default vehicle types if not exists
                        command.CommandText = @"
                            INSERT INTO t_tarif (jenis_kendaraan, tarif_awal, tarif_per_jam) 
                            SELECT 'Mobil', 5000.00, 2000.00
                            WHERE NOT EXISTS (SELECT 1 FROM t_tarif WHERE jenis_kendaraan = 'Mobil')";
                        command.ExecuteNonQuery();
                        
                        command.CommandText = @"
                            INSERT INTO t_tarif (jenis_kendaraan, tarif_awal, tarif_per_jam) 
                            SELECT 'Motor', 2000.00, 1000.00
                            WHERE NOT EXISTS (SELECT 1 FROM t_tarif WHERE jenis_kendaraan = 'Motor')";
                        command.ExecuteNonQuery();
                    }
                }

                LogInfo("Database structure initialized successfully");
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                LogError("Failed to initialize database structure", ex);
                return false;
            }
        }

        private static void InitializeConnectionString()
        {
            try
            {
                _connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=postgres;";
                LogInfo("Using default connection string");
                
                // Test the connection - but don't throw if it fails
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        LogInfo("Database connection test successful");
                    }
                }
                catch (Exception ex)
                {
                    LogError("Default connection test failed", ex);
                    // Continue anyway - the app might need to display a connection settings dialog
                }
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize database connection", ex);
                // Don't rethrow - this would cause the static constructor to fail
            }
        }

        private static async Task InitializeWebSocketAsync()
        {
            try
            {
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(_webSocketUrl), CancellationToken.None);
                _isConnected = true;
                LogInfo("WebSocket connection established successfully");
            }
            catch (Exception ex)
            {
                LogError("Failed to initialize WebSocket connection", ex);
                _isConnected = false;
                // Don't rethrow - WebSocket is optional
            }
        }

        /// <summary>
        /// Gets the current connection string
        /// </summary>
        public static string ConnectionString
        {
            get { return _connectionString; }
            set { _connectionString = value; }
        }

        /// <summary>
        /// Gets the current connection string (for compatibility with older code)
        /// </summary>
        public static string GetConnectionString()
        {
            return _connectionString;
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        public static bool TestConnection()
        {
            string errorMessage;
            return TestConnection(out errorMessage);
        }

        /// <summary>
        /// Tests the database connection and returns any error message
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
                return true;
            }
            catch (NpgsqlException ex)
            {
                errorMessage = ex.Message;
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Executes a query and returns the results as a DataTable
        /// </summary>
        public static DataTable GetData(string query, Dictionary<string, object> parameters = null)
        {
            DataTable dt = new DataTable();
            try
            {
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
            }
            catch (Exception ex)
            {
                LogError("Error executing query", ex);
                throw;
            }
            return dt;
        }

        /// <summary>
        /// Executes a scalar query and returns the first column of the first row
        /// </summary>
        public static object ExecuteScalar(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
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

                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error executing scalar query", ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a non-query command and returns the number of rows affected
        /// </summary>
        public static bool ExecuteNonQuery(string query, Dictionary<string, object> parameters = null)
        {
            try
            {
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

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Error executing non-query", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets a new database connection
        /// </summary>
        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
} 