using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using MySql.Data.MySqlClient;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Windows.Forms;
using SimpleParkingAdmin.Utils;

namespace SimpleParkingAdmin.Utils
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
            LoadConnectionString();
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
                _connectionString = "Server=localhost;Port=3306;Database=parkingdb;Uid=root;Pwd=root@rsi;AllowPublicKeyRetrieval=true;SslMode=none;DefaultAuthenticationPlugin=mysql_native_password;";
                _logger.Information("Using default connection string");
                
                // Test the connection
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.Information("Database connection test successful");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize database connection", ex);
                throw;
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
            get { return _connectionString; }
            set { _connectionString = value; }
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
                    if (part.Trim().StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                    {
                        pwd = part.Substring(4);
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
            return _connectionString;
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.Information("Database connection test successful");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Database connection test failed", ex);
                return false;
            }
        }

        /// <summary>
        /// Tests the database connection and returns an error message
        /// </summary>
        public static bool TestConnection(out string errorMessage)
        {
            errorMessage = string.Empty;
            
            if (_useWebSocket)
            {
                if (_isConnected && _webSocket.State == WebSocketState.Open)
                {
                    return true;
                }
                
                errorMessage = "WebSocket connection is not established";
                return false;
            }
            
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    _logger.Information("Database connection test successful");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to connect to database", ex);
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
                
                using (MySqlConnection connection = new MySqlConnection(connectionWithoutDb))
                {
                    connection.Open();
                    
                    // Create the database if it doesn't exist
                    using (MySqlCommand command = new MySqlCommand($"CREATE DATABASE IF NOT EXISTS `{databaseName}`", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                // Now connect to the specific database
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();

                    // Drop the users table if it exists
                    using (MySqlCommand command = new MySqlCommand("DROP TABLE IF EXISTS `users`", connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Create the users table with the correct schema
                    string createTableSql = @"
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";

                    using (MySqlCommand command = new MySqlCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    // Insert the default admin user
                    string insertSql = @"
INSERT INTO `users` (`username`, `password`, `nama`, `role`, `level`) 
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin', 'Admin');";

                    using (MySqlCommand command = new MySqlCommand(insertSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    
                    return true;
                }
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
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
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
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        var adapter = new MySqlDataAdapter(command);
                        var dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Database error occurred", ex);
                throw;
            }
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
                // Build the WHERE clause from parameters
                List<string> conditions = new List<string>();
                foreach (var param in parameters)
                {
                    string paramName = param.Key.StartsWith("@") ? param.Key.Substring(1) : param.Key;
                    string condition;
                    
                    if (param.Value == null)
                    {
                        condition = $"{paramName} IS NULL";
                    }
                    else if (param.Value is string)
                    {
                        condition = $"{paramName} = '{param.Value.ToString().Replace("'", "''")}'";
                    }
                    else if (param.Value is DateTime)
                    {
                        DateTime dateValue = (DateTime)param.Value;
                        condition = $"{paramName} = '{dateValue:yyyy-MM-dd HH:mm:ss}'";
                    }
                    else if (param.Value is bool)
                    {
                        condition = $"{paramName} = {((bool)param.Value ? "1" : "0")}";
                    }
                    else
                    {
                        condition = $"{paramName} = {param.Value}";
                    }
                    
                    conditions.Add(condition);
                }
                
                string whereClause = string.Join(" AND ", conditions);
                return ExecuteQueryWithWhere(sql, whereClause);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing query with parameters: {ex.Message}", ex);
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
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
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
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(
                        "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @tableName",
                        connection))
                    {
                        command.Parameters.AddWithValue("@tableName", tableName);
                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
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
                // Read and execute schema.sql
                string schemaPath = Path.Combine(Application.StartupPath, "Database", "schema.sql");
                if (File.Exists(schemaPath))
                {
                    string schemaSql = File.ReadAllText(schemaPath);
                    ExecuteNonQuery(schemaSql);
                    _logger.Information("Database structure initialized successfully");
                    return true;
                }
                else
                {
                    _logger.Information("Schema file not found");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize database structure", ex);
                return false;
            }
        }

        public static int GetLastInsertId()
        {
            return Convert.ToInt32(ExecuteScalar("SELECT LAST_INSERT_ID()"));
        }
        
        public static MySqlDataReader ExecuteReader(string sql)
        {
            MySqlConnection connection = new MySqlConnection(_connectionString);
            connection.Open();
            MySqlCommand command = new MySqlCommand(sql, connection);
            return command.ExecuteReader(CommandBehavior.CloseConnection);
        }
        
        public static string EscapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;
                
            return MySqlHelper.EscapeString(str);
        }
        
        // Additional methods needed by the application
        
        public static DataTable GetData(string tableName, string whereClause = "")
        {
            try
            {
                string sql = $"SELECT * FROM `{EscapeString(tableName)}`";
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
            try
            {
                using (MySqlConnection connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (MySqlCommand command = new MySqlCommand(sqlQuery, connection))
                    {
                        if (parameters != null)
                        {
                            foreach (var param in parameters)
                            {
                                string paramName = param.Key.StartsWith("@") ? param.Key : "@" + param.Key;
                                command.Parameters.AddWithValue(paramName, param.Value ?? DBNull.Value);
                            }
                        }
                        
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
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
            string sql = "SELECT * FROM settings WHERE setting_key LIKE 'network_%'";
            return ExecuteQuery(sql);
        }
        
        public static bool SaveSettings(string key, string value)
        {
            try
            {
                string sql = $"INSERT INTO settings (setting_key, setting_value) VALUES ('{EscapeString(key)}', '{EscapeString(value)}') " +
                             $"ON DUPLICATE KEY UPDATE setting_value = '{EscapeString(value)}'";
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
            catch
            {
                return defaultValue;
            }
        }

        public static bool IsUsingNetworkDatabase => _useWebSocket;

        public static bool AddColumnIfNotExists(string tableName, string columnName, string columnDefinition)
        {
            try
            {
                // Check if the column exists
                DataTable columns = ExecuteQuery($"SHOW COLUMNS FROM {tableName} LIKE '{columnName}'");
                
                if (columns.Rows.Count == 0)
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

        public static DataTable ExecuteQuery(string query, params MySqlParameter[] parameters)
        {
            var dataTable = new DataTable();
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                using (var command = new MySqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    using (var adapter = new MySqlDataAdapter(command))
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
    }
} 