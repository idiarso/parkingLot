using System;
using System.Data;
using Npgsql;

namespace ParkingApp
{
    public class ParkingManager
    {
        // Record vehicle entry
        public static int RecordEntry(string plateNumber, string vehicleType, int operatorId, string imagePath = null)
        {
            // PostgreSQL uses RETURNING to get the inserted ID
            string query = @"
                INSERT INTO t_parkir (no_plat, jenis_kendaraan, waktu_masuk, operator_masuk, image_masuk) 
                VALUES (@plateNumber, @vehicleType, NOW(), @operatorId, @imagePath)
                RETURNING id";
            
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@plateNumber", plateNumber),
                new NpgsqlParameter("@vehicleType", vehicleType),
                new NpgsqlParameter("@operatorId", operatorId),
                new NpgsqlParameter("@imagePath", imagePath ?? (object)DBNull.Value)
            };
            
            // Return the new parking record ID
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(query, parameters));
        }
        
        // Record vehicle exit and calculate fee
        public static bool RecordExit(int parkingId, int operatorId, out decimal fee, string imagePath = null)
        {
            fee = 0;
            
            try
            {
                // First, get entry time and vehicle type
                string getEntryQuery = "SELECT waktu_masuk, jenis_kendaraan FROM t_parkir WHERE id = @parkingId";
                NpgsqlParameter[] getParams = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@parkingId", parkingId)
                };
                
                DataTable dt = DatabaseHelper.ExecuteDataTable(getEntryQuery, getParams);
                
                if (dt.Rows.Count == 0)
                    return false;
                
                DateTime entryTime = Convert.ToDateTime(dt.Rows[0]["waktu_masuk"]);
                string vehicleType = dt.Rows[0]["jenis_kendaraan"].ToString();
                
                // Calculate parking duration in hours
                TimeSpan duration = DateTime.Now - entryTime;
                double hours = Math.Ceiling(duration.TotalHours);
                
                // Get tariff from t_tarif table
                string getTariffQuery = "SELECT tarif_awal, tarif_per_jam FROM t_tarif WHERE jenis_kendaraan = @vehicleType";
                NpgsqlParameter[] tariffParams = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@vehicleType", vehicleType)
                };
                
                DataTable tariffDt = DatabaseHelper.ExecuteDataTable(getTariffQuery, tariffParams);
                
                if (tariffDt.Rows.Count == 0)
                    return false;
                
                decimal initialFee = Convert.ToDecimal(tariffDt.Rows[0]["tarif_awal"]);
                decimal hourlyFee = Convert.ToDecimal(tariffDt.Rows[0]["tarif_per_jam"]);
                
                // Calculate total fee
                fee = initialFee + (hourlyFee * (decimal)(hours - 1));
                
                // Update parking record
                string updateQuery = @"
                    UPDATE t_parkir 
                    SET waktu_keluar = NOW(), 
                        operator_keluar = @operatorId, 
                        biaya = @fee,
                        status = 'KELUAR',
                        image_keluar = @imagePath
                    WHERE id = @parkingId";
                
                NpgsqlParameter[] updateParams = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@operatorId", operatorId),
                    new NpgsqlParameter("@fee", fee),
                    new NpgsqlParameter("@parkingId", parkingId),
                    new NpgsqlParameter("@imagePath", imagePath ?? (object)DBNull.Value)
                };
                
                int result = DatabaseHelper.ExecuteNonQuery(updateQuery, updateParams);
                
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error recording exit: " + ex.Message);
                return false;
            }
        }
        
        // Get active parking records
        public static DataTable GetActiveParkingRecords()
        {
            // PostgreSQL query using ISNULL instead of MySQL's IFNULL
            string query = @"
                SELECT p.id, p.no_plat, p.jenis_kendaraan, p.waktu_masuk, 
                       u.nama AS operator_masuk, p.image_masuk
                FROM t_parkir p
                JOIN t_user u ON p.operator_masuk = u.id
                WHERE p.waktu_keluar IS NULL
                ORDER BY p.waktu_masuk DESC";
            
            return DatabaseHelper.ExecuteDataTable(query);
        }
        
        // Get parking record by plate number
        public static DataTable GetParkingByPlateNumber(string plateNumber)
        {
            // PostgreSQL uses ILIKE for case-insensitive search
            string query = @"
                SELECT p.id, p.no_plat, p.jenis_kendaraan, p.waktu_masuk
                FROM t_parkir p
                WHERE p.waktu_keluar IS NULL AND p.no_plat ILIKE @plateNumber
                ORDER BY p.waktu_masuk DESC";
            
            NpgsqlParameter[] parameters = new NpgsqlParameter[]
            {
                new NpgsqlParameter("@plateNumber", "%" + plateNumber + "%")
            };
            
            return DatabaseHelper.ExecuteDataTable(query, parameters);
        }
    }
} 