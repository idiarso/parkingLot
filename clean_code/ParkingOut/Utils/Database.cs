using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using ParkingOut.Utils;
using System.Text.RegularExpressions;
using NLog;

namespace ParkingOut.Utils
{
    public static class Database
    {
        private static string _connectionString;
        private static readonly IAppLogger _logger = new FileLogger();
        private static ClientWebSocket _webSocket;
        private static string _webSocketUrl = "ws://localhost:8181";
        private static bool _useWebSocket = false;
        private static readonly object _lockObject = new object();
        private static bool _isConnected = false;

        static Database()
        {
            try
            {
                _logger.Information("Initializing Database class...");
                LoadConnectionString();
                
                // Tambahkan property untuk menandakan status koneksi database
                try
                {
                    _logger.Information("Connection string: " + MaskPassword(_connectionString));
                    InitializeConnectionString(); // Memastikan database setup
                    
                    // Explicitly test connection to verify it's working
                    using (var conn = new NpgsqlConnection(_connectionString))
                    {
                        _logger.Information("Attempting to open database connection...");
                        conn.Open();
                        _logger.Information("Database connection opened successfully");
                        
                        // Check if we can query something simple
                        using (var cmd = new NpgsqlCommand("SELECT 1", conn))
                        {
                            cmd.ExecuteScalar();
                            _logger.Information("Successfully executed test query");
                        }
                    }
                    
                    IsDatabaseAvailable = true;
                    LastError = null;
                    _logger.Information("Database successfully initialized");
                }
                catch (Exception initEx)
                {
                    IsDatabaseAvailable = false;
                    
                    // Provide more specific error details
                    if (initEx is NpgsqlException)
                    {
                        _logger.Error($"PostgreSQL connection error: {initEx.Message}", initEx);
                        if (initEx.Message.Contains("does not exist"))
                        {
                            LastError = $"Database does not exist: {GetDatabaseName()}. Please create it first.";
                        }
                        else if (initEx.Message.Contains("password authentication failed"))
                        {
                            LastError = "Invalid username or password for database connection.";
                        }
                        else if (initEx.Message.Contains("No connection could be made"))
                        {
                            LastError = "Could not connect to PostgreSQL server. Please verify the server is running.";
                        }
                        else
                        {
                            LastError = "PostgreSQL connection error: " + initEx.Message;
                        }
                    }
                    else
                    {
                        LastError = initEx.Message;
                        _logger.Error($"Failed to initialize database connection: {initEx.Message}", initEx);
                    }
                    
                    // Tidak throw exception agar aplikasi bisa jalan tanpa database jika diperlukan
                }
            }
            catch (Exception ex)
            {
                IsDatabaseAvailable = false;
                LastError = ex.Message;
                _logger.Error("Critical error initializing Database class", ex);
            }
        }

        private static void LoadConnectionString()
        {
            try
            {
                _connectionString = ConfigurationManager.ConnectionStrings["ParkingDatabase"]?.ConnectionString;
                
                if (string.IsNullOrEmpty(_connectionString))
                {
                    string configPath = Path.Combine(Application.StartupPath, "config.json");
                    if (File.Exists(configPath))
                    {
                        var config = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(configPath));
                        _connectionString = config.ConnectionString;
                    }
                    else
                    {
                        throw new Exception("No connection string found in configuration");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to load connection string", ex);
                throw;
            }
        }
        
        private static void InitializeConnectionString()
        {
            try
            {
                // Coba deteksi apakah PostgreSQL terinstal
                bool postgresInstalled = IsPostgreSqlInstalled();
                if (!postgresInstalled)
                {
                    throw new Exception("PostgreSQL tidak terdeteksi pada sistem ini. Pastikan PostgreSQL terinstal dan service sedang berjalan.");
                }

                _logger.Information("Mencoba inisialisasi koneksi database parkirdb...");
                // Try primary database (parkirdb)
                _connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
                _logger.Information($"String koneksi: {_connectionString.Replace("root@rsi", "****")}");
                
                if (TestConnection(out string errorMsg))
                {
                    _logger.Information("Koneksi ke parkirdb berhasil");
                    return;
                }
                
                _logger.Warning($"Gagal terhubung ke parkirdb: {errorMsg}");
                
                // Tampilkan pesan detil tentang error
                _logger.Information("Mencoba deteksi masalah koneksi secara detil...");
                
                // Cek apakah PostgreSQL berjalan di port yang benar
                try
                {
                    using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=root@rsi;Database=postgres;"))
                    {
                        connection.Open();
                        _logger.Information("Koneksi ke database default postgres berhasil, berarti server PostgreSQL berjalan dengan baik");
                        connection.Close();
                    }
                }
                catch (NpgsqlException npgEx)
                {
                    _logger.Error($"Tidak bisa terhubung ke database postgres: {npgEx.Message}");
                    if (npgEx.Message.Contains("connection refused"))
                    {
                        _logger.Error("PostgreSQL server tidak berjalan di port 5432. Periksa apakah service aktif.");
                        throw new Exception("PostgreSQL server tidak berjalan di port 5432. Pastikan service PostgreSQL aktif.");
                    }
                    else if (npgEx.Message.Contains("password authentication failed"))
                    {
                        _logger.Error("Password untuk user postgres salah. Gunakan password yang benar.");
                        throw new Exception("Password untuk user postgres salah. Silakan reset password PostgreSQL atau gunakan password yang benar.");
                    }
                    throw;
                }
                
                // Fallback to default postgres database and create parkirdb
                string defaultConn = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=root@rsi;";
                try
                {
                    using (var connection = new NpgsqlConnection(defaultConn))
                    {
                        connection.Open();
                        _logger.Information("Koneksi ke database postgres berhasil");
                        
                        using (var command = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'parkirdb'", connection))
                        {
                            var result = command.ExecuteScalar();
                            if (result == null || result == DBNull.Value)
                            {
                                _logger.Information("Membuat database parkirdb...");
                                using (var createCmd = new NpgsqlCommand("CREATE DATABASE parkirdb", connection))
                                {
                                    try {
                                        createCmd.ExecuteNonQuery();
                                        _logger.Information("Database parkirdb berhasil dibuat");
                                    }
                                    catch (Exception createEx) {
                                        _logger.Error($"Gagal membuat database parkirdb: {createEx.Message}");
                                        throw new Exception($"Gagal membuat database parkirdb: {createEx.Message}");
                                    }
                                }
                            }
                            else
                            {
                                _logger.Information("Database parkirdb sudah ada");
                            }
                        }
                    }
                }
                catch (Exception pgEx)
                {
                    _logger.Error($"Gagal terhubung ke database postgres: {pgEx.Message}");
                    throw new Exception($"Gagal terhubung ke database postgres. Pastikan server PostgreSQL berjalan dan kredensial login benar: {pgEx.Message}");
                }
                
                // Set final connection string and verify
                _connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
                _logger.Information("Mencoba koneksi akhir ke parkirdb...");
                if (!TestConnection(out errorMsg))
                {
                    throw new Exception($"Gagal terhubung ke parkirdb setelah pembuatan: {errorMsg}");
                }
                
                // Ensure schema is applied
                _logger.Information("Memastikan struktur database untuk parkirdb...");
                if (!EnsureDatabaseStructure())
                {
                    throw new Exception("Gagal menginisialisasi struktur database untuk parkirdb");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Tidak dapat membuat atau terhubung ke database: {ex.Message}", ex);
                throw;
            }
        }
        
        private static bool IsPostgreSqlInstalled()
        {
            try
            {
                // Coba koneksi ke service PostgreSQL
                using (var connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=root@rsi;"))
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                if (ex.Message.Contains("connection refused"))
                {
                    _logger.Error("PostgreSQL service tidak berjalan. Pastikan PostgreSQL terinstal dan service aktif.");
                    return false;
                }
                else if (ex.Message.Contains("password authentication"))
                {
                    // Jika mendapat error password, berarti server berjalan tetapi kredensial salah
                    return true;
                }
                
                _logger.Error($"Error saat memeriksa instalasi PostgreSQL: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saat memeriksa instalasi PostgreSQL: {ex.Message}");
                return false;
            }
        }
        
        private static async Task InitializeWebSocketAsync()
        {
            try
            {
                _webSocket = new ClientWebSocket();
                await _webSocket.ConnectAsync(new Uri(_webSocketUrl), CancellationToken.None);
                _isConnected = _webSocket.State == WebSocketState.Open;
                
                if (_isConnected)
                {
                    _logger.Information("WebSocket connection established");
                    
                    // Start receiving messages in background
                    _ = ReceiveWebSocketMessagesAsync();
                }
                else
                {
                    _logger.Warning("WebSocket connection failed");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize WebSocket connection", ex);
                _isConnected = false;
            }
        }
        
        private static async Task ReceiveWebSocketMessagesAsync()
        {
            var buffer = new byte[4096];
            
            try
            {
                while (_webSocket.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        _isConnected = false;
                        _logger.Information("WebSocket connection closed");
                        break;
                    }
                    
                    // Process received message
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.Debug($"Received WebSocket message: {message}");
                    
                    // Process the message if needed
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error receiving WebSocket messages", ex);
                _isConnected = false;
                
                // Try to reconnect
                await ReconnectWebSocketAsync();
            }
        }
        
        private static async Task ReconnectWebSocketAsync()
        {
            try
            {
                if (_webSocket != null)
                {
                    if (_webSocket.State == WebSocketState.Open)
                    {
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Reconnecting", CancellationToken.None);
                    }
                    
                    _webSocket.Dispose();
                }
                
                await Task.Delay(5000); // Wait 5 seconds before reconnecting
                await InitializeWebSocketAsync();
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to reconnect WebSocket", ex);
            }
        }
        
        private static async Task<string> SendWebSocketRequestAsync(string requestType, object data)
        {
            if (!_isConnected || _webSocket.State != WebSocketState.Open)
            {
                await ReconnectWebSocketAsync();
                
                if (!_isConnected)
                {
                    throw new Exception("WebSocket connection is not established");
                }
            }
            
            try
            {
                var request = new
                {
                    type = requestType,
                    data = data
                };
                
                string requestJson = JsonConvert.SerializeObject(request);
                var buffer = Encoding.UTF8.GetBytes(requestJson);
                
                await _webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                
                // For simplicity, we're not waiting for response in this implementation
                // A real implementation would wait for a specific response message
                return "{}";
            }
            catch (Exception ex)
            {
                _logger.Error($"Error sending WebSocket request: {requestType}", ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the current connection string
        /// </summary>
        public static string ConnectionString
        {
            get
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    LoadConnectionString();
                }
                return _connectionString;
            }
        }

        /// <summary>
        /// Extracts the password from the connection string for security logging
        /// </summary>
        public static string GetPasswordFromConnectionString()
        {
            try
            {
                string pwd = "";
                string[] parts = _connectionString.Split(';');
                foreach (string part in parts)
                {
                    if (part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                    {
                        pwd = part.Substring(8);
                        break;
                    }
                }
                return pwd;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Gets the current connection string (for compatibility with older code)
        /// </summary>
        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        /// <summary>
        /// Tests the database connection (backwards compatibility method)
        /// </summary>
        public static bool TestConnection()
        {
            string errorMessage;
            return TestConnection(out errorMessage);
        }

        /// <summary>
        /// Tests the database connection and provides error information
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(_connectionString))
                {
                    errorMessage = "Connection string kosong atau belum diinisialisasi";
                    _logger.Error(errorMessage);
                    return false;
                }

                _logger.Information($"Menguji koneksi dengan string: {_connectionString.Replace(GetPasswordFromConnectionString(), "****")}");
                
                try
                {
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        _logger.Information("Berhasil terhubung ke database");
                        
                        // Check if 'settings' table exists using information_schema
                        using (var command = new NpgsqlCommand(
                            "SELECT 1 FROM information_schema.tables WHERE table_name = 'settings' AND table_schema = 'public'", 
                            connection))
                        {
                            var result = command.ExecuteScalar();
                            if (result == null || result == DBNull.Value)
                            {
                                _logger.Warning("Tabel 'settings' tidak ditemukan dalam database");
                                
                                // Cek apakah tabel lain ada
                                using (var cmd2 = new NpgsqlCommand(
                                    "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' LIMIT 5", 
                                    connection))
                                {
                                    using (var reader = cmd2.ExecuteReader())
                                    {
                                        bool hasTables = false;
                                        StringBuilder tables = new StringBuilder();
                                        
                                        while (reader.Read())
                                        {
                                            hasTables = true;
                                            tables.Append(reader.GetString(0)).Append(", ");
                                        }
                                        
                                        if (hasTables)
                                        {
                                            errorMessage = $"Terhubung ke database tetapi tabel 'settings' tidak ditemukan. Database memiliki tabel: {tables.ToString().TrimEnd(',', ' ')}. Schema mungkin belum diimport atau tidak lengkap.";
                                        }
                                        else
                                        {
                                            errorMessage = "Terhubung ke database tetapi tidak ada tabel. Schema belum diimport.";
                                        }
                                    }
                                }
                                
                                _logger.Warning(errorMessage);
                                _logger.Information("Mencoba apply schema secara otomatis");
                                
                                // Mencoba import schema
                                return false;
                            }
                        }
                        
                        _logger.Information("Tes koneksi database berhasil");
                        errorMessage = string.Empty;
                        return true;
                    }
                }
                catch (NpgsqlException npgEx)
                {
                    string detailMessage = $"PostgreSQL Error: {npgEx.Message}";
                    
                    if (npgEx.Message.Contains("does not exist"))
                    {
                        detailMessage += ". Database tidak ada - perlu dibuat terlebih dahulu.";
                    }
                    else if (npgEx.Message.Contains("password authentication"))
                    {
                        detailMessage += ". Password salah atau pengguna tidak memiliki akses.";
                    }
                    else if (npgEx.Message.Contains("connection refused"))
                    {
                        detailMessage += ". Server PostgreSQL tidak berjalan atau tidak menerima koneksi di port yang ditentukan.";
                    }
                    else if (npgEx.Message.Contains("timeout"))
                    {
                        detailMessage += ". Koneksi timeout. Pastikan firewall tidak memblokir koneksi.";
                    }
                    
                    errorMessage = detailMessage;
                    _logger.Error("Tes koneksi database gagal", npgEx);
                    return false;
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"General error: {ex.Message}";
                _logger.Error("Tes koneksi database gagal", ex);
                return false;
            }
        }

        /// <summary>
        /// Tests a connection with a specific connection string
        /// </summary>
        /// <param name="connectionString">The connection string to test</param>
        /// <param name="errorMessage">Error message if connection fails</param>
        /// <returns>True if connection succeeds, otherwise false</returns>
        public static bool TestConnectionWithString(string connectionString, out string errorMessage)
        {
            try
            {
                // Return true if connection can be established
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    errorMessage = string.Empty;
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to test database connection with provided string: {ex.Message}", ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Ensures the database exists and creates it if needed
        /// </summary>
        public static bool EnsureDatabaseExists(out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (_useWebSocket)
            {
                errorMessage = "Cannot create database when using WebSocket mode";
                return false;
            }
            
            try
            {
                string databaseName = GetDatabaseNameFromConnectionString();
                
                if (string.IsNullOrEmpty(databaseName))
                {
                    errorMessage = "Database name not found in connection string";
                    return false;
                }
                
                // Create a connection string without the database name
                string connectionWithoutDb = RemoveDatabaseFromConnectionString();
                
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionWithoutDb))
                {
                    connection.Open();
                    
                    // Check if database exists in PostgreSQL way
                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @dbname", connection))
                    {
                        command.Parameters.AddWithValue("@dbname", databaseName);
                        object result = command.ExecuteScalar();
                        
                        // If database doesn't exist, create it
                        if (result == null || result == DBNull.Value)
                        {
                            // In PostgreSQL, you can't use parameters for database names
                            using (NpgsqlCommand createDbCmd = new NpgsqlCommand($"CREATE DATABASE {databaseName}", connection))
                            {
                                createDbCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // Now connect to the specific database
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();

                    // Check if users table exists in PostgreSQL way
                    using (NpgsqlCommand checkTableCmd = new NpgsqlCommand("SELECT to_regclass('users')", connection))
                    {
                        object tableExists = checkTableCmd.ExecuteScalar();
                        
                        if (tableExists == null || tableExists == DBNull.Value)
                        {
                            // Create the users table with PostgreSQL syntax
                            string createTableSql = @"
CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    password VARCHAR(255) NOT NULL,
    nama VARCHAR(100) DEFAULT NULL,
    role VARCHAR(20) DEFAULT 'operator',
    level VARCHAR(20) DEFAULT 'Operator',
    last_login TIMESTAMP DEFAULT NULL,
    status BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT username_unique UNIQUE (username)
);";

                            using (NpgsqlCommand command = new NpgsqlCommand(createTableSql, connection))
                            {
                                command.ExecuteNonQuery();
                            }

                            // Insert the default admin user
                            string insertSql = @"
INSERT INTO users (username, password, nama, role, level) 
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin', 'Admin');";

                            using (NpgsqlCommand command = new NpgsqlCommand(insertSql, connection))
                            {
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to ensure database exists", ex);
                errorMessage = ex.Message;
                return false;
            }
        }

        private static string GetDatabaseNameFromConnectionString()
        {
            try
            {
                string dbName = "";
                string[] parts = _connectionString.Split(';');
                foreach (string part in parts)
                {
                    if (part.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase))
                    {
                        dbName = part.Substring(9);
                        break;
                    }
                }
                return dbName;
            }
            catch
            {
                return "";
            }
        }

        private static string RemoveDatabaseFromConnectionString()
        {
            try
            {
                string result = "";
                string[] parts = _connectionString.Split(';');
                foreach (string part in parts)
                {
                    if (!part.Trim().StartsWith("Database=", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(part))
                    {
                        result += part + ";";
                    }
                }
                return result;
            }
            catch
            {
                return _connectionString;
            }
        }

        private static string GetCreateTablesScript()
        {
            try
            {
                string schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Database", "schema_mysql.sql");
                if (File.Exists(schemaPath))
                {
                    return File.ReadAllText(schemaPath);
                }
                else
                {
                    _logger.Error($"Schema file not found at: {schemaPath}");
                    return @"
DROP TABLE IF EXISTS `users`;

CREATE TABLE `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL,
  `nama` varchar(100) DEFAULT NULL,
  `role` varchar(20) DEFAULT 'operator',
  `level` varchar(20) DEFAULT 'Operator',
  `last_login` datetime DEFAULT NULL,
  `status` tinyint(1) DEFAULT 1,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

INSERT IGNORE INTO `users` (`username`, `password`, `nama`, `role`, `level`) 
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin', 'Admin');
";
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error reading schema file", ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a non-query SQL command
        /// </summary>
        public static int ExecuteNonQuery(string sql, Dictionary<string, object> parameters = null)
        {
            if (_useWebSocket)
            {
                try
                {
                    var request = new
                    {
                        sql = sql,
                        parameters = parameters
                    };
                    
                    var response = SendWebSocketRequestAsync("executeNonQuery", request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                    
                    if (result.ContainsKey("affectedRows") && result["affectedRows"] is int affectedRows)
                    {
                        return affectedRows;
                    }
                    
                    return 0;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error executing non-query via WebSocket: {sql}", ex);
                    throw;
                }
            }
            
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        return command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing non-query: {sql}", ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL query and returns a DataTable with no parameters
        /// </summary>
        public static DataTable ExecuteQuery(string sql)
        {
            var logger = LogManager.GetCurrentClassLogger();
            DataTable table = new DataTable();

            try
            {
                string connectionString = GetConnectionString();
                
                // Convert MySQL query to PostgreSQL query
                sql = ConvertMySqlQueryToPgSql(sql);
                logger.Debug($"Executing query: {sql}");

                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(table);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Database error executing query: {ex.Message}", ex);
                throw;
            }

            return table;
        }

        /// <summary>
        /// Executes a SQL query with a WHERE clause and returns a DataTable
        /// </summary>
        public static DataTable ExecuteQueryWithWhere(string sql, string whereClause)
        {
            if (string.IsNullOrEmpty(whereClause))
            {
                return ExecuteQuery(sql);
            }
            
            string fullSql = sql;
            if (!sql.ToUpper().Contains("WHERE") && !string.IsNullOrEmpty(whereClause))
            {
                fullSql += " WHERE " + whereClause;
            }
            else if (!string.IsNullOrEmpty(whereClause))
            {
                fullSql += " AND " + whereClause;
            }
            
            return ExecuteQuery(fullSql);
        }

        /// <summary>
        /// Executes a query with parameters provided as a dictionary
        /// </summary>
        /// <param name="sql">The SQL query to execute</param>
        /// <param name="parameters">Dictionary of parameters</param>
        /// <returns>DataTable containing the query results</returns>
        public static DataTable ExecuteQueryWithParams(string sql, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return ExecuteQuery(sql);
            }

            try
            {
                // Use proper parameterized queries
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        // Add parameters to the command
                        int paramIndex = 1;
                        foreach (var param in parameters)
                        {
                            // Create parameter properly for PostgreSQL
                            var npgParam = new NpgsqlParameter();
                            
                            // Parameter naming based on whether it's a named or positional parameter
                            if (param.Key.StartsWith("@"))
                            {
                                // For named parameters like @username, use the name directly
                                npgParam.ParameterName = param.Key;
                            }
                            else
                            {
                                // For regular parameters, use position ($1, $2, etc.)
                                npgParam.ParameterName = $"${paramIndex}";
                                paramIndex++;
                            }
                            
                            npgParam.Value = param.Value ?? DBNull.Value;
                            command.Parameters.Add(npgParam);
                            
                            // Log for debugging
                            _logger.Debug($"Added parameter {npgParam.ParameterName}: {(param.Value ?? "NULL")}");
                        }
                        
                        // Log the final SQL and parameter count for debugging
                        _logger.Debug($"Executing SQL: {sql} with {command.Parameters.Count} parameters");

                        DataTable result = new DataTable();
                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            adapter.Fill(result);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing query with parameters: {ex.Message} | SQL: {sql}", ex);
                throw;
            }
        }

        /// <summary>
        /// Executes a SQL scalar query and returns a single value
        /// </summary>
        public static object ExecuteScalar(string sql, Dictionary<string, object> parameters = null)
        {
            if (_useWebSocket)
            {
                try
                {
                    var request = new
                    {
                        sql = sql,
                        parameters = parameters
                    };
                    
                    var response = SendWebSocketRequestAsync("executeScalar", request).GetAwaiter().GetResult();
                    var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);
                    
                    if (result.ContainsKey("result"))
                    {
                        return result["result"];
                    }
                    
                    return null;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error executing scalar query via WebSocket: {sql}", ex);
                    throw;
                }
            }
            
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                            }
                        }
                        return command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing scalar query: {sql}", ex);
                throw;
            }
        }

        public static bool TableExists(string tableName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(
                        "SELECT to_regclass(@tableName)", connection))
                    {
                        command.Parameters.AddWithValue("@tableName", tableName);
                        object result = command.ExecuteScalar();
                        return result != null && result != DBNull.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to check if table {tableName} exists", ex);
                return false;
            }
        }

        public static bool EnsureDatabaseStructure()
        {
            try
            {
                _logger.Information("Memulai inisialisasi struktur database...");
                
                // Try to use PostgreSQL schema first
                string postgresSchemaPath = Path.Combine(Application.StartupPath, "Database", "schema_postgresql.sql");
                _logger.Information($"Mencari file schema di: {postgresSchemaPath}");
                
                // Fall back to generic schema if PostgreSQL-specific doesn't exist
                if (!File.Exists(postgresSchemaPath))
                {
                    _logger.Warning($"File schema PostgreSQL tidak ditemukan di {postgresSchemaPath}");
                    postgresSchemaPath = Path.Combine(Application.StartupPath, "Database", "schema.sql");
                    _logger.Information($"Mencoba file schema generic di: {postgresSchemaPath}");
                }
                
                if (File.Exists(postgresSchemaPath))
                {
                    _logger.Information($"File schema ditemukan: {postgresSchemaPath}");
                    string schemaSql = File.ReadAllText(postgresSchemaPath);
                    
                    if (string.IsNullOrWhiteSpace(schemaSql))
                    {
                        _logger.Error("File schema kosong");
                        return false;
                    }
                    
                    _logger.Information($"Memulai eksekusi script schema ({schemaSql.Length} karakter)...");
                    
                    // Execute the entire script as a single command
                    using (var connection = new NpgsqlConnection(_connectionString))
                    {
                        connection.Open();
                        
                        // Gunakan pendekatan yang lebih baik - split script berdasarkan GO atau ;
                        foreach (var batch in SplitSqlBatches(schemaSql))
                        {
                            if (string.IsNullOrWhiteSpace(batch))
                                continue;
                                
                            try
                            {
                                using (var command = new NpgsqlCommand(batch, connection))
                                {
                                    command.CommandTimeout = 60; // 1 menit timeout
                                    command.ExecuteNonQuery();
                                }
                            }
                            catch (NpgsqlException ex)
                            {
                                // Hanya log error tetapi lanjutkan eksekusi
                                // Ini karena beberapa statement mungkin gagal jika objek sudah ada
                                _logger.Warning($"Error saat mengeksekusi batch SQL: {ex.Message}");
                            }
                        }
                        
                        // Verifikasi bahwa tabel penting ada
                        bool tablesExist = VerifyRequiredTablesExist(connection);
                        if (!tablesExist)
                        {
                            _logger.Error("Schema berhasil dieksekusi tetapi tabel yang dibutuhkan tidak ditemukan");
                            return false;
                        }
                        
                        _logger.Information("Database structure initialized successfully");
                    }
                    
                    return true;
                }
                else
                {
                    _logger.Error($"File schema tidak ditemukan di {postgresSchemaPath}");
                    
                    // Coba buat schema minimal untuk tabel settings jika tidak ada
                    _logger.Warning("Mencoba membuat tabel settings minimal...");
                    try
                    {
                        using (var connection = new NpgsqlConnection(_connectionString))
                        {
                            connection.Open();
                            using (var command = new NpgsqlCommand(@"
                            CREATE TABLE IF NOT EXISTS settings (
                              id SERIAL PRIMARY KEY,
                              setting_key VARCHAR(100) NOT NULL,
                              setting_value VARCHAR(255) NOT NULL,
                              deskripsi VARCHAR(255) DEFAULT NULL,
                              created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                              updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                              CONSTRAINT setting_key_unique UNIQUE (setting_key)
                            );
                            
                            INSERT INTO settings (setting_key, setting_value, deskripsi)
                            VALUES ('app_name', 'ParkingOut', 'Nama Aplikasi')
                            ON CONFLICT (setting_key) DO NOTHING;
                            ", connection))
                            {
                                command.ExecuteNonQuery();
                                _logger.Information("Tabel settings minimal berhasil dibuat");
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Gagal membuat tabel settings minimal: {ex.Message}", ex);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saat menginisialisasi struktur database: {ex.Message}", ex);
                return false;
            }
        }

        // Split SQL script into batches that can be executed separately
        private static IEnumerable<string> SplitSqlBatches(string sqlScript)
        {
            // Split by semicolons, typically used as separators
            // But avoid splitting inside quotes
            var result = new List<string>();
            var batch = new StringBuilder();
            bool inQuote = false;
            char quoteChar = '"';
            
            foreach (char c in sqlScript)
            {
                if (!inQuote && c == ';')
                {
                    result.Add(batch.ToString());
                    batch.Clear();
                    continue;
                }
                
                if (c == '\'' || c == '"')
                {
                    if (inQuote && c == quoteChar)
                    {
                        inQuote = false;
                    }
                    else if (!inQuote)
                    {
                        inQuote = true;
                        quoteChar = c;
                    }
                }
                
                batch.Append(c);
            }
            
            if (batch.Length > 0)
            {
                result.Add(batch.ToString());
            }
            
            return result;
        }

        // Verify that required tables exist
        private static bool VerifyRequiredTablesExist(NpgsqlConnection connection)
        {
            string[] requiredTables = new[] { "settings", "users", "t_parkir", "t_tarif" };
            foreach (var table in requiredTables)
            {
                using (var command = new NpgsqlCommand(
                    "SELECT 1 FROM information_schema.tables WHERE table_name = @tableName AND table_schema = 'public'", 
                    connection))
                {
                    command.Parameters.AddWithValue("@tableName", table);
                    var result = command.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                    {
                        _logger.Error($"Table {table} doesn't exist after schema application");
                        return false;
                    }
                }
            }
            
            return true;
        }

        public static int GetLastInsertId()
        {
            // PostgreSQL uses RETURNING clause or currval() function to get last inserted id
            return Convert.ToInt32(ExecuteScalar("SELECT lastval()"));
        }
        
        public static NpgsqlDataReader ExecuteReader(string sql)
        {
            NpgsqlConnection connection = new NpgsqlConnection(_connectionString);
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand(sql, connection);
            return command.ExecuteReader();
        }
        
        public static string EscapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
                
            // Jangan gunakan EscapeSqlIdentifier yang untuk nama kolom/tabel
            // Gunakan Replace untuk escape string value
            return str.Replace("'", "''");
        }
        
        // Additional methods needed by the application
        
        public static DataTable GetData(string tableName, string whereClause = "")
        {
            try
            {
                // Ubah dari backticks (`) ke double quotes untuk PostgreSQL
                string sql = $"SELECT * FROM \"{EscapeString(tableName)}\"";
                if (!string.IsNullOrEmpty(whereClause))
                {
                    sql += " WHERE " + whereClause;
                }
                return ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting data from table {tableName}", ex);
                throw;
            }
        }
        
        public static DataTable GetData(string sqlQuery, Dictionary<string, object> parameters)
        {
            // Adapt the query for PostgreSQL before execution
            sqlQuery = AdaptQueryForPostgreSQL(sqlQuery);
            
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(_connectionString))
                {
                    connection.Open();
                    using (NpgsqlCommand command = new NpgsqlCommand(sqlQuery, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                string paramName = param.Key.StartsWith("@") ? param.Key : "@" + param.Key;
                                command.Parameters.AddWithValue(paramName, param.Value ?? DBNull.Value);
                            }
                        }
                        
                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            return dataTable;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing query with parameters: {sqlQuery}", ex);
                throw;
            }
        }
        
        public static DataTable LoadNetworkSettings()
        {
            try 
            {
                string sql = "SELECT * FROM settings WHERE setting_key LIKE 'network_%'";
                return ExecuteQuery(sql);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error loading network settings: {ex.Message}", ex);
                // Return empty DataTable instead of throwing
                return new DataTable();
            }
        }
        
        /// <summary>
        /// Loads network settings and returns a NetworkSettings object from Models namespace
        /// </summary>
        public static ParkingOut.Models.NetworkSettings LoadNetworkSettingsObject()
        {
            try
            {
                DataTable settingsTable = LoadNetworkSettings();
                return ParkingOut.Models.NetworkSettings.FromDataTable(settingsTable);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error converting network settings to object: {ex.Message}", ex);
                // Return default settings
                return new ParkingOut.Models.NetworkSettings();
            }
        }
        
        public static bool SaveSettings(string key, string value)
        {
            try
            {
                // Ganti ON DUPLICATE KEY UPDATE dengan PostgreSQL UPSERT syntax
                string sql = $"INSERT INTO settings (setting_key, setting_value) VALUES ('{EscapeString(key)}', '{EscapeString(value)}') " +
                             $"ON CONFLICT (setting_key) DO UPDATE SET setting_value = '{EscapeString(value)}'";
                ExecuteNonQuery(sql);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error saving setting {key}: {ex.Message}", ex);
                return false;
            }
        }
        
        public static string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                string sql = $"SELECT setting_value FROM settings WHERE setting_key = '{EscapeString(key)}'";
                object result = ExecuteScalar(sql);
                return result != null && result != DBNull.Value ? result.ToString() : defaultValue;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting setting {key}: {ex.Message}", ex);
                return defaultValue;
            }
        }

        public static bool IsUsingNetworkDatabase => _useWebSocket;

        public static bool AddColumnIfNotExists(string tableName, string columnName, string columnDefinition)
        {
            try
            {
                // Check if the column exists in PostgreSQL way
                string checkSql = @"
                SELECT 1 FROM information_schema.columns 
                WHERE table_name = @tableName AND column_name = @columnName";
                
                var parameters = new Dictionary<string, object>
                {
                    { "@tableName", tableName },
                    { "@columnName", columnName }
                };
                
                object result = ExecuteScalar(checkSql, parameters);
                
                if (result == null || result == DBNull.Value)
                {
                    // Column doesn't exist, add it
                    string sql = $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}";
                    ExecuteNonQuery(sql);
                    _logger.Information($"Added column {columnName} to table {tableName}");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error adding column {columnName} to table {tableName}", ex);
                return false;
            }
        }

        /// <summary>
        /// Executes a SQL query with a Dictionary of parameters and returns a DataTable
        /// </summary>
        public static DataTable ExecuteQuery(string sql, Dictionary<string, object> parameters)
        {
            // Just call our existing method with parameters
            return ExecuteQueryWithParams(sql, parameters);
        }

        /// <summary>
        /// Executes a SQL query with a Dictionary of parameters using a WHERE clause and returns a DataTable
        /// </summary>
        public static DataTable ExecuteQueryWithWhere(string sql, Dictionary<string, object> parameters)
        {
            // This method uses ExecuteQueryWithParams which already handles the parameters
            return ExecuteQueryWithParams(sql, parameters);
        }

        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
            _logger.Information($"Connection string updated");
        }

        public static DataTable ExecuteQuery(string query, params NpgsqlParameter[] parameters)
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                using (var command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    using (var adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dataTable);
                    }
                }
                _logger.Debug($"Query executed successfully: {query}");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing query: {query}", ex);
                throw;
            }
            return dataTable;
        }

        /// <summary>
        /// Gets a value indicating whether the database is available
        /// </summary>
        public static bool IsDatabaseAvailable { get; private set; } = false;

        /// <summary>
        /// Gets the last error message encountered during database initialization
        /// </summary>
        public static string LastError { get; private set; } = null;

        /// <summary>
        /// Returns whether the database has been successfully initialized (backward compatibility)
        /// </summary>
        public static bool IsDatabaseInitialized => IsDatabaseAvailable;

        // Helper method to get database name from connection string
        private static string GetDatabaseName()
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(_connectionString);
                return builder.Database;
            }
            catch
            {
                return "unknown";
            }
        }

        // Helper method to mask password in connection string for logging
        private static string MaskPassword(string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                string password = builder.Password;
                if (!string.IsNullOrEmpty(password))
                {
                    return connectionString.Replace(password, "********");
                }
                return connectionString;
            }
            catch
            {
                return "Could not mask password in connection string";
            }
        }

        // New method to adapt queries for PostgreSQL
        private static string AdaptQueryForPostgreSQL(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                return sql;
            
            // Use local logger to avoid modifying static readonly field
            var localLogger = LogManager.GetCurrentClassLogger();
            
            try
            {
                localLogger.Debug($"Original SQL: {sql}");
                
                // PostgreSQL uses double quotes for identifiers while MySQL uses backticks
                // Replace backticks with double quotes
                sql = Regex.Replace(sql, @"`([^`]*)`", "\"$1\"");
                
                // IMPORTANT: Only handle "waktu_keluar" if it actually appears in the query
                bool hasWaktuKeluar = sql.Contains("waktu_keluar");
                
                // Common column mappings that need to be fixed
                Dictionary<string, string> columnMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // t_parkir table mappings - conditionally add these based on query content
                    { "m.nama_member", "m.nama_pemilik" },
                    { "p.tarif", "p.biaya" },
                    { "member_id", "nomor_kartu" },
                    { "p.member_id", "p.nomor_kartu" }, 
                    { "card_id", "nomor_kartu" },
                    { "p.nomor_kartu_member", "p.nomor_kartu" }, // Fix for reports
                    { "nomor_kartu_member", "nomor_kartu" }, // Fix for member reference
                    { "m.id", "m.member_id" }, // Member ID field mapping
                    { "m.nama", "m.nama_pemilik" }, // Member name field mapping
                    { "jumlah_member", "jumlah_member" }, // Keep aggregated field names unchanged

                    // Other mappings for vehicle types
                    { "vehicle_type", "jenis_kendaraan" },
                    // User table mappings
                    { "name", "nama" },
                    { "start_time", "jam_mulai" },
                    { "end_time", "jam_selesai" }
                };
                
                // Only add waktu_keluar handling if it exists in the query
                if (hasWaktuKeluar)
                {
                    columnMappings.Add("status", "CASE WHEN \"waktu_keluar\" IS NULL THEN 'MASUK' ELSE 'KELUAR' END");
                    columnMappings.Add("waktu_keluar", "\"waktu_keluar\"");
                }
                
                // Replace column references - handle with caution to avoid replacing substrings
                foreach (var mapping in columnMappings)
                {
                    // Use regex to replace full column names or aliases
                    string pattern = $@"(?<!\w){Regex.Escape(mapping.Key)}(?!\w)";
                    sql = Regex.Replace(sql, pattern, mapping.Value);
                }
                
                // Only apply waktu_keluar specific handling if it exists in the query
                if (hasWaktuKeluar)
                {
                    // Escape column names in WHERE clauses for PostgreSQL
                    sql = Regex.Replace(sql, @"(WHERE|AND|OR)\s+(\w+)\.?(\w+)\s*(=|!=|<>|>|<|>=|<=|IS NULL|IS NOT NULL)", 
                        m => {
                            string clause = m.Groups[1].Value;
                            string tableAlias = m.Groups[2].Value;
                            string columnName = m.Groups[3].Value;
                            string operator_ = m.Groups[4].Value;
                            
                            // Add double quotes to the column name if it's "waktu_keluar"
                            if (columnName.Equals("waktu_keluar", StringComparison.OrdinalIgnoreCase))
                            {
                                if (!string.IsNullOrEmpty(tableAlias) && tableAlias != "AND" && tableAlias != "OR")
                                    return $"{clause} {tableAlias}.\"waktu_keluar\" {operator_}";
                                else
                                    return $"{clause} \"waktu_keluar\" {operator_}";
                            }
                            
                            // Return the original if it's not waktu_keluar
                            return m.Value;
                        }, RegexOptions.IgnoreCase);
                    
                    // Fix IS NULL/IS NOT NULL clauses specifically for waktu_keluar
                    sql = Regex.Replace(sql, @"waktu_keluar\s+(IS NULL|IS NOT NULL)", "\"waktu_keluar\" $1", RegexOptions.IgnoreCase);
                }
                
                // Convert Integer 1/0 to TRUE/FALSE for boolean fields in WHERE clauses
                // This handles things like "WHERE status = 1" to "WHERE status = TRUE"
                sql = Regex.Replace(sql, @"(WHERE|AND|OR)\s+(\w+\.)?(\w+)\s*=\s*1\b", "$1 $2$3 = TRUE", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"(WHERE|AND|OR)\s+(\w+\.)?(\w+)\s*=\s*0\b", "$1 $2$3 = FALSE", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"(WHERE|AND|OR)\s+(\w+\.)?(\w+)\s*!=\s*1\b", "$1 $2$3 != TRUE", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"(WHERE|AND|OR)\s+(\w+\.)?(\w+)\s*!=\s*0\b", "$1 $2$3 != FALSE", RegexOptions.IgnoreCase);
                
                // MySQL to PostgreSQL function conversions
                sql = Regex.Replace(sql, @"IFNULL\s*\(", "COALESCE(", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"CURDATE\s*\(\)", "CURRENT_DATE", RegexOptions.IgnoreCase);
                
                // Fix DATE(), YEAR(), MONTH() functions for PostgreSQL
                sql = Regex.Replace(sql, @"DATE\s*\(([^)]+)\)", "DATE($1)", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"YEAR\s*\(([^)]+)\)", "EXTRACT(YEAR FROM $1)", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"MONTH\s*\(([^)]+)\)", "EXTRACT(MONTH FROM $1)", RegexOptions.IgnoreCase);
                
                // Fix GROUP BY for PostgreSQL when using date functions
                sql = Regex.Replace(sql, @"GROUP BY DATE\(([^)]+)\)", "GROUP BY DATE($1)", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"GROUP BY YEAR\(([^)]+)\), MONTH\(([^)]+)\)", 
                                       "GROUP BY EXTRACT(YEAR FROM $1), EXTRACT(MONTH FROM $2)", RegexOptions.IgnoreCase);
                
                // Fix ORDER BY for PostgreSQL when using date functions
                sql = Regex.Replace(sql, @"ORDER BY DATE\(([^)]+)\)", "ORDER BY DATE($1)", RegexOptions.IgnoreCase);
                sql = Regex.Replace(sql, @"ORDER BY YEAR\(([^)]+)\), MONTH\(([^)]+)\)", 
                                       "ORDER BY EXTRACT(YEAR FROM $1), EXTRACT(MONTH FROM $2)", RegexOptions.IgnoreCase);
                
                // Fix table names (if needed)
                sql = sql.Replace("shifts", "t_shift");
                sql = sql.Replace("tariff", "t_tarif");
                sql = sql.Replace("t_member", "m_member");
                
                // Add debug logging for the adapted query
                localLogger.Debug($"Adapted SQL for PostgreSQL: {sql}");
                
                return sql;
            }
            catch (Exception ex)
            {
                localLogger.Error($"Error adapting query for PostgreSQL: {ex.Message}");
                return sql;
            }
        }

        // Add this method near the ProcessPgSqlPostgreMessage method
        public static string ConvertMySqlQueryToPgSql(string query)
        {
            var logger = LogManager.GetCurrentClassLogger();
            try
            {
                // Map column names between databases - add any additional ones here
                var columnMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    // Base table mappings for incorrect names
                    { "waktu_keluar", "waktu_masuk" }, // Handle waktu_keluar references
                    { "p.waktu_keluar", "p.waktu_masuk" },
                    { "member_id", "nomor_kartu" },
                    { "p.member_id", "p.nomor_kartu" },
                    { "m.member_id", "m.member_id" }, // Keep m.member_id as is
                    { "tarif", "biaya" },
                    { "p.tarif", "p.biaya" }
                };

                // Apply column mappings
                foreach (var mapping in columnMappings)
                {
                    // Use regex to replace column names while preserving context
                    query = Regex.Replace(
                        query,
                        $@"\b{Regex.Escape(mapping.Key)}\b(?!\s*\()",  // Match whole word, not as function names
                        mapping.Value,
                        RegexOptions.IgnoreCase
                    );
                }

                // Replace MySQL syntax with PostgreSQL equivalents
                query = query.Replace("IFNULL(", "COALESCE(");

                // Log the converted query
                logger.Debug($"Converted query: {query}");

                return query;
            }
            catch (Exception ex)
            {
                logger.Error($"Error converting MySQL query to PostgreSQL: {ex.Message}", ex);
                return query; // Return original query if conversion fails
            }
        }
    }
} 