using System;
using System.Data;
using System.Configuration;
using Npgsql;

namespace ParkingApp
{
    public class DatabaseHelper
    {
        private static string connectionString = ConfigurationManager.ConnectionStrings["ParkingDBConnection"].ConnectionString;

        // Create a connection to PostgreSQL
        public static NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection(connectionString);
        }

        // Execute a query that returns no results
        public static int ExecuteNonQuery(string query, NpgsqlParameter[] parameters = null)
        {
            using (NpgsqlConnection connection = CreateConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    int result = command.ExecuteNonQuery();
                    return result;
                }
            }
        }

        // Execute a query that returns a single value
        public static object ExecuteScalar(string query, NpgsqlParameter[] parameters = null)
        {
            using (NpgsqlConnection connection = CreateConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
        }

        // Execute a query that returns a DataTable
        public static DataTable ExecuteDataTable(string query, NpgsqlParameter[] parameters = null)
        {
            using (NpgsqlConnection connection = CreateConnection())
            {
                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    connection.Open();
                    DataTable dt = new DataTable();
                    using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(command))
                    {
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        // Create parameter with proper PostgreSQL format
        public static NpgsqlParameter CreateParameter(string name, object value)
        {
            return new NpgsqlParameter("@" + name, value ?? DBNull.Value);
        }
    }
}