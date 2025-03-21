using System;
using System.Data;
using Npgsql;
using System.Windows.Forms;

namespace ParkingIN.Utils
{
    public static class ConnectionTest
    {
        public static bool TestDatabaseConnection()
        {
            try
            {
                using (var connection = Database.GetConnection())
                {
                    connection.Open();
                    
                    // Test basic query
                    using (var cmd = new NpgsqlCommand("SELECT 1", connection))
                    {
                        cmd.ExecuteScalar();
                    }

                    // Test user table query
                    using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM t_user", connection))
                    {
                        int userCount = Convert.ToInt32(cmd.ExecuteScalar());
                        MessageBox.Show($"Connection successful!\nFound {userCount} users in database.", 
                            "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Connection failed!\nError: {ex.Message}", 
                    "Connection Test", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
} 