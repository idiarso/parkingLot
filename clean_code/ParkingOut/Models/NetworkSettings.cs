using System;
using System.Data;
using System.Collections.Generic;
using ParkingOut.Utils;

namespace ParkingOut.Models
{
    public class NetworkSettings
    {
        public bool UseWebSocket { get; set; }
        public string WebSocketUrl { get; set; }
        public string ServerAddress { get; set; }
        public int Port { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public NetworkSettings()
        {
            // Default values
            UseWebSocket = false;
            WebSocketUrl = "ws://localhost:8181";
            ServerAddress = "localhost";
            Port = 5432;
            DatabaseName = "parkirdb";
            Username = "postgres";
            Password = "root@rsi";
        }
        
        public string GetConnectionString()
        {
            return $"Host={ServerAddress};Port={Port};Database={DatabaseName};Username={Username};Password={Password};";
        }
        
        public static NetworkSettings FromDataTable(DataTable dt)
        {
            var settings = new NetworkSettings();
            
            if (dt == null || dt.Rows.Count == 0)
                return settings;
                
            foreach (DataRow row in dt.Rows)
            {
                string key = row["setting_key"].ToString();
                string value = row["setting_value"].ToString();
                
                switch (key)
                {
                    case "network_use_websocket":
                        settings.UseWebSocket = value == "1" || value.ToLower() == "true";
                        break;
                    case "network_websocket_url":
                        settings.WebSocketUrl = value;
                        break;
                    case "network_server":
                        settings.ServerAddress = value;
                        break;
                    case "network_port":
                        if (int.TryParse(value, out int port))
                            settings.Port = port;
                        break;
                    case "network_database":
                        settings.DatabaseName = value;
                        break;
                    case "network_username":
                        settings.Username = value;
                        break;
                    case "network_password":
                        settings.Password = value;
                        break;
                }
            }
            
            return settings;
        }
    }
} 