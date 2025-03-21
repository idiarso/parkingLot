using System;
using System.Windows.Forms;
using Npgsql;
using System.Configuration;

namespace ParkingIN
{
    public static class ConnectionTest
    {
        public static void TestDatabaseConnection()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["ParkingDBConnection"].ConnectionString;
            
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    MessageBox.Show("Successfully connected to PostgreSQL database!", 
                                    "Connection Test", 
                                    MessageBoxButtons.OK, 
                                    MessageBoxIcon.Information);
                    
                    // Test a simple query
                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT current_timestamp", connection))
                    {
                        object result = command.ExecuteScalar();
                        MessageBox.Show($"Server timestamp: {result}", 
                                        "Query Test", 
                                        MessageBoxButtons.OK, 
                                        MessageBoxIcon.Information);
                    }
                    
                    // Try to query user table to test schema
                    try 
                    {
                        using (NpgsqlCommand command = new NpgsqlCommand("SELECT COUNT(*) FROM t_user", connection))
                        {
                            int userCount = Convert.ToInt32(command.ExecuteScalar());
                            MessageBox.Show($"Found {userCount} users in database", 
                                           "Schema Test", 
                                           MessageBoxButtons.OK, 
                                           MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Schema test failed: {ex.Message}\n\nMake sure the t_user table exists in your PostgreSQL database.",
                                       "Schema Test Failed",
                                       MessageBoxButtons.OK,
                                       MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to PostgreSQL database!\n\nError: {ex.Message}\n\nCheck your connection string in App.config.",
                               "Connection Failed",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
            }
        }
    }
} 