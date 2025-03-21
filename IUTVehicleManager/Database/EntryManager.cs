using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IUTVehicleManager.Database
{
    public class EntryManager
    {
        private readonly DatabaseConnection _db;

        public EntryManager()
        {
            _db = DatabaseConnection.Instance;
        }

        public async Task<int> ProcessVehicleEntry(string plateNumber, int vehicleTypeId, int priorityId, string entryPoint, int operatorId)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_ProcessVehicleEntry", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Generate ticket number
                        string ticketNumber = $"TKT{DateTime.Now:yyyyMMddHHmmss}";

                        // Add parameters
                        command.Parameters.AddWithValue("@PlateNumber", plateNumber);
                        command.Parameters.AddWithValue("@VehicleTypeID", vehicleTypeId);
                        command.Parameters.AddWithValue("@PriorityID", priorityId);
                        command.Parameters.AddWithValue("@EntryPoint", entryPoint);
                        command.Parameters.AddWithValue("@OperatorID", operatorId);
                        command.Parameters.AddWithValue("@TicketNumber", ticketNumber);

                        // Execute the stored procedure
                        await command.ExecuteNonQueryAsync();

                        // Get the generated entry ID
                        command.CommandText = "SELECT SCOPE_IDENTITY()";
                        command.CommandType = CommandType.Text;
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing vehicle entry: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetCurrentVehicles()
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand("SELECT * FROM vw_CurrentParkingStatus", connection))
                    {
                        var adapter = new SqlDataAdapter(command);
                        var table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting current vehicles: {ex.Message}", ex);
            }
        }

        public async Task<bool> IsVehicleInside(string plateNumber)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        "SELECT COUNT(1) FROM Vehicles WHERE PlateNumber = @PlateNumber AND Status = 'IN'",
                        connection))
                    {
                        command.Parameters.AddWithValue("@PlateNumber", plateNumber);
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result) > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking vehicle status: {ex.Message}", ex);
            }
        }

        public async Task<(DateTime entryTime, string ticketNumber)> GetVehicleEntryInfo(string plateNumber)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        @"SELECT TOP 1 et.EntryTime, et.TicketNumber 
                          FROM EntryTransactions et
                          JOIN Vehicles v ON et.VehicleID = v.ID
                          WHERE v.PlateNumber = @PlateNumber AND v.Status = 'IN'
                          ORDER BY et.EntryTime DESC",
                        connection))
                    {
                        command.Parameters.AddWithValue("@PlateNumber", plateNumber);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return (
                                    reader.GetDateTime(0),
                                    reader.GetString(1)
                                );
                            }
                            throw new Exception("Vehicle entry information not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting vehicle entry info: {ex.Message}", ex);
            }
        }
    }
} 