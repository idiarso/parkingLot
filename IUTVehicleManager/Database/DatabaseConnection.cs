using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using IUTVehicleManager.Config;

namespace IUTVehicleManager.Database
{
    public class DatabaseConnection
    {
        private static string _connectionString;
        private static DatabaseConnection _instance;
        private static readonly object _lock = new object();

        private DatabaseConnection()
        {
            // Get connection string based on terminal type
            string terminalType = DatabaseConfig.GetTerminalType();
            _connectionString = terminalType switch
            {
                "GETIN" => DatabaseConfig.GetConnectionString("GetInConnection"),
                "GETOUT" => DatabaseConfig.GetConnectionString("GetOutConnection"),
                _ => throw new ArgumentException($"Invalid terminal type: {terminalType}")
            };
        }

        public static DatabaseConnection Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new DatabaseConnection();
                        }
                    }
                }
                return _instance;
            }
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
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

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public SqlConnection GetCentralConnection()
        {
            return new SqlConnection(DatabaseConfig.GetConnectionString("CentralConnection"));
        }

        public static void SetConnectionString(string server, string database, bool trustedConnection = true)
        {
            _connectionString = $"Server={server};Database={database};Trusted_Connection={trustedConnection};";
        }
    }
} 