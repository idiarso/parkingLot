using System;
using System.Data;

namespace SimpleParkingAdmin
{
    public class NetworkSettings
    {
        public string ServerIP { get; set; } = "localhost";
        public string DatabaseName { get; set; } = "parking_system";
        public string Username { get; set; } = "root";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 3306;
        
        public int ServerPort { get { return Port; } set { Port = value; } }
        
        /// <summary>
        /// Creates a network settings object from database settings
        /// </summary>
        public static NetworkSettings FromDataTable(DataTable dataTable)
        {
            NetworkSettings settings = new NetworkSettings();
            
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                // Check if this is a key-value settings table format
                if (dataTable.Columns.Contains("setting_key") && dataTable.Columns.Contains("setting_value"))
                {
                    foreach (DataRow row in dataTable.Rows)
                    {
                        string key = row["setting_key"].ToString();
                        string value = row["setting_value"].ToString();
                        
                        switch (key)
                        {
                            case "network_server_ip":
                                settings.ServerIP = value;
                                break;
                            case "network_database_name":
                                settings.DatabaseName = value;
                                break;
                            case "network_username":
                                settings.Username = value;
                                break;
                            case "network_password":
                                settings.Password = value;
                                break;
                            case "network_port":
                                if (int.TryParse(value, out int port))
                                {
                                    settings.Port = port;
                                }
                                break;
                        }
                    }
                }
                // Direct column mapping format
                else
                {
                    var row = dataTable.Rows[0];
                    
                    if (dataTable.Columns.Contains("ServerIP"))
                        settings.ServerIP = row["ServerIP"]?.ToString() ?? "localhost";
                    else if (dataTable.Columns.Contains("server_ip"))
                        settings.ServerIP = row["server_ip"]?.ToString() ?? "localhost";
                        
                    if (dataTable.Columns.Contains("DatabaseName"))
                        settings.DatabaseName = row["DatabaseName"]?.ToString() ?? "parking_system";
                    else if (dataTable.Columns.Contains("database_name"))
                        settings.DatabaseName = row["database_name"]?.ToString() ?? "parking_system";
                        
                    if (dataTable.Columns.Contains("Username"))
                        settings.Username = row["Username"]?.ToString() ?? "root";
                    else if (dataTable.Columns.Contains("username"))
                        settings.Username = row["username"]?.ToString() ?? "root";
                        
                    if (dataTable.Columns.Contains("Password"))
                        settings.Password = row["Password"]?.ToString() ?? "";
                    else if (dataTable.Columns.Contains("password"))
                        settings.Password = row["password"]?.ToString() ?? "";
                    
                    int port = 3306;
                    if (dataTable.Columns.Contains("ServerPort") && 
                        int.TryParse(row["ServerPort"]?.ToString(), out port))
                    {
                        settings.Port = port;
                    }
                    else if (dataTable.Columns.Contains("server_port") && 
                        int.TryParse(row["server_port"]?.ToString(), out port))
                    {
                        settings.Port = port;
                    }
                    else if (dataTable.Columns.Contains("Port") && 
                        int.TryParse(row["Port"]?.ToString(), out port))
                    {
                        settings.Port = port;
                    }
                }
            }
            
            return settings;
        }
        
        /// <summary>
        /// Get connection string from settings
        /// </summary>
        public string GetConnectionString()
        {
            return $"Server={ServerIP};Database={DatabaseName};Uid={Username};Pwd={Password};Port={Port};CharSet=utf8mb4;SslMode=none;";
        }
    }
} 