using System;
using System.Data;

namespace ParkingOut.Models
{
    public class NetworkSettings
    {
        public string DatabaseHost { get; set; } = "localhost";
        public int DatabasePort { get; set; } = 5432;
        public string DatabaseName { get; set; } = "parking_db";
        public string DatabaseUser { get; set; } = "postgres";
        public string DatabasePassword { get; set; } = "";

        public string GetConnectionString()
        {
            return $"Host={DatabaseHost};Port={DatabasePort};Database={DatabaseName};Username={DatabaseUser};Password={DatabasePassword}";
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
                    case "network_server":
                        settings.DatabaseHost = value;
                        break;
                    case "network_port":
                        if (int.TryParse(value, out int port))
                            settings.DatabasePort = port;
                        break;
                    case "network_database":
                        settings.DatabaseName = value;
                        break;
                    case "network_username":
                        settings.DatabaseUser = value;
                        break;
                    case "network_password":
                        settings.DatabasePassword = value;
                        break;
                }
            }
            
            return settings;
        }
    }
}