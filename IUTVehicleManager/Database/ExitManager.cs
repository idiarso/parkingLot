using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace IUTVehicleManager.Database
{
    public class ExitManager
    {
        private readonly DatabaseConnection _db;

        public ExitManager()
        {
            _db = DatabaseConnection.Instance;
        }

        public async Task<int> ProcessVehicleExit(string plateNumber, string exitPoint, int operatorId, string paymentMethod, decimal amount)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("sp_ProcessVehicleExit", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@PlateNumber", plateNumber);
                        command.Parameters.AddWithValue("@ExitPoint", exitPoint);
                        command.Parameters.AddWithValue("@OperatorID", operatorId);
                        command.Parameters.AddWithValue("@PaymentMethod", paymentMethod);
                        command.Parameters.AddWithValue("@Amount", amount);

                        // Execute the stored procedure
                        await command.ExecuteNonQueryAsync();

                        // Get the generated exit ID
                        command.CommandText = "SELECT SCOPE_IDENTITY()";
                        command.CommandType = CommandType.Text;
                        var result = await command.ExecuteScalarAsync();
                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing vehicle exit: {ex.Message}", ex);
            }
        }

        public async Task<decimal> CalculateParkingFee(string plateNumber)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        @"SELECT dbo.fn_CalculateParkingFee(et.EntryTime, vt.TypeID)
                          FROM Vehicles v
                          JOIN EntryTransactions et ON v.ID = et.VehicleID
                          JOIN VehicleTypes vt ON v.VehicleType = vt.TypeName
                          WHERE v.PlateNumber = @PlateNumber AND v.Status = 'IN'",
                        connection))
                    {
                        command.Parameters.AddWithValue("@PlateNumber", plateNumber);
                        var result = await command.ExecuteScalarAsync();
                        return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error calculating parking fee: {ex.Message}", ex);
            }
        }

        public async Task<DataTable> GetExitHistory(DateTime startDate, DateTime endDate)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        @"SELECT 
                            v.PlateNumber,
                            et.EntryTime,
                            ext.ExitTime,
                            ext.Duration,
                            ext.Amount,
                            ext.PaymentMethod,
                            ext.PaymentStatus
                          FROM ExitTransactions ext
                          JOIN EntryTransactions et ON ext.EntryTransactionID = et.ID
                          JOIN Vehicles v ON et.VehicleID = v.ID
                          WHERE ext.ExitTime BETWEEN @StartDate AND @EndDate
                          ORDER BY ext.ExitTime DESC",
                        connection))
                    {
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@EndDate", endDate);

                        var adapter = new SqlDataAdapter(command);
                        var table = new DataTable();
                        adapter.Fill(table);
                        return table;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting exit history: {ex.Message}", ex);
            }
        }

        public async Task<(DateTime entryTime, int duration, decimal amount)> GetExitInfo(string plateNumber)
        {
            try
            {
                using (var connection = _db.GetConnection())
                {
                    await connection.OpenAsync();
                    using (var command = new SqlCommand(
                        @"SELECT 
                            et.EntryTime,
                            DATEDIFF(MINUTE, et.EntryTime, GETDATE()) as Duration,
                            dbo.fn_CalculateParkingFee(et.EntryTime, vt.TypeID) as Amount
                          FROM Vehicles v
                          JOIN EntryTransactions et ON v.ID = et.VehicleID
                          JOIN VehicleTypes vt ON v.VehicleType = vt.TypeName
                          WHERE v.PlateNumber = @PlateNumber AND v.Status = 'IN'",
                        connection))
                    {
                        command.Parameters.AddWithValue("@PlateNumber", plateNumber);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return (
                                    reader.GetDateTime(0),
                                    reader.GetInt32(1),
                                    reader.GetDecimal(2)
                                );
                            }
                            throw new Exception("Vehicle exit information not found");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting exit info: {ex.Message}", ex);
            }
        }
    }
} 