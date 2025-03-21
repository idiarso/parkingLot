using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Timers;
using System.IO;
using IUTVehicleManager.Config;

namespace IUTVehicleManager.Database
{
    public class SyncManager
    {
        private readonly DatabaseConnection _db;
        private readonly Timer _syncTimer;
        private readonly string _terminalType;
        private readonly int _syncInterval;
        private readonly int _maxRetries;
        private int _retryCount;
        private const string IMAGE_BACKUP_PATH = "ImageBackup";

        public event EventHandler<string> SyncStatusChanged;

        public SyncManager()
        {
            _db = DatabaseConnection.Instance;
            _terminalType = DatabaseConfig.GetTerminalType();
            _syncInterval = DatabaseConfig.GetSyncInterval();
            _maxRetries = DatabaseConfig.GetMaxRetries();
            _retryCount = 0;

            _syncTimer = new Timer(_syncInterval);
            _syncTimer.Elapsed += async (s, e) => await SyncData();

            // Buat direktori backup jika belum ada
            if (!Directory.Exists(IMAGE_BACKUP_PATH))
            {
                Directory.CreateDirectory(IMAGE_BACKUP_PATH);
            }
        }

        public void StartSync()
        {
            _syncTimer.Start();
            OnSyncStatusChanged("Synchronization started");
        }

        public void StopSync()
        {
            _syncTimer.Stop();
            OnSyncStatusChanged("Synchronization stopped");
        }

        private async Task SyncData()
        {
            try
            {
                if (_terminalType == "GETIN")
                {
                    await SyncEntryData();
                }
                else if (_terminalType == "GETOUT")
                {
                    await SyncExitData();
                }

                _retryCount = 0;
                OnSyncStatusChanged($"Last sync: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _retryCount++;
                string errorMessage = $"Sync error: {ex.Message}";
                
                if (_retryCount >= _maxRetries)
                {
                    await LogSyncError(errorMessage);
                    _retryCount = 0;
                }
                
                OnSyncStatusChanged($"{errorMessage} (Attempt {_retryCount} of {_maxRetries})");
            }
        }

        private async Task SyncEntryData()
        {
            using var localConn = _db.GetConnection();
            using var centralConn = _db.GetCentralConnection();
            
            await localConn.OpenAsync();
            await centralConn.OpenAsync();

            using var transaction = centralConn.BeginTransaction();
            try
            {
                // Get pending entries with images
                using (var command = new SqlCommand(
                    @"SELECT e.ID, e.VehicleID, e.EntryTime, e.TicketNumber, e.OperatorID,
                             ei.ImagePath, ei.ImageData
                      FROM EntryTransactions e
                      LEFT JOIN EntryImages ei ON e.ID = ei.EntryTransactionID 
                      WHERE e.SyncStatus = 'PENDING'", localConn))
                {
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        int entryId = reader.GetInt32(0);
                        
                        // Insert entry transaction
                        using var centralCmd = new SqlCommand(
                            @"INSERT INTO EntryTransactions_Sync 
                              (OriginalID, VehicleID, EntryTime, TicketNumber, OperatorID, SyncStatus, SyncTime)
                              VALUES (@OriginalID, @VehicleID, @EntryTime, @TicketNumber, @OperatorID, 'SYNCED', GETDATE());
                              SELECT SCOPE_IDENTITY();",
                            centralConn, transaction);

                        centralCmd.Parameters.AddWithValue("@OriginalID", entryId);
                        centralCmd.Parameters.AddWithValue("@VehicleID", reader.GetInt32(1));
                        centralCmd.Parameters.AddWithValue("@EntryTime", reader.GetDateTime(2));
                        centralCmd.Parameters.AddWithValue("@TicketNumber", reader.GetString(3));
                        centralCmd.Parameters.AddWithValue("@OperatorID", reader.GetInt32(4));

                        var syncedId = Convert.ToInt32(await centralCmd.ExecuteScalarAsync());

                        // Handle image synchronization
                        if (!reader.IsDBNull(5)) // If has image
                        {
                            string imagePath = reader.GetString(5);
                            byte[] imageData = (byte[])reader.GetValue(6);

                            // Backup image locally
                            string backupPath = Path.Combine(IMAGE_BACKUP_PATH, $"entry_{entryId}_{DateTime.Now:yyyyMMddHHmmss}.jpg");
                            File.WriteAllBytes(backupPath, imageData);

                            // Insert image to central
                            using var imageCmd = new SqlCommand(
                                @"INSERT INTO EntryImages_Sync
                                  (EntryTransactionID, ImagePath, ImageData, SyncStatus, SyncTime)
                                  VALUES (@EntryTransactionID, @ImagePath, @ImageData, 'SYNCED', GETDATE())",
                                centralConn, transaction);

                            imageCmd.Parameters.AddWithValue("@EntryTransactionID", syncedId);
                            imageCmd.Parameters.AddWithValue("@ImagePath", imagePath);
                            imageCmd.Parameters.AddWithValue("@ImageData", imageData);

                            await imageCmd.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Update sync status
                using (var command = new SqlCommand(
                    @"UPDATE EntryTransactions 
                      SET SyncStatus = 'SYNCED' 
                      WHERE SyncStatus = 'PENDING'", localConn))
                {
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
                
                // Cleanup successful synced image backups older than 7 days
                CleanupOldImageBackups();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private void CleanupOldImageBackups()
        {
            try
            {
                var files = Directory.GetFiles(IMAGE_BACKUP_PATH, "*.jpg");
                var cutoffDate = DateTime.Now.AddDays(-7);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"Warning: Failed to cleanup old image backups: {ex.Message}");
            }
        }

        private async Task SyncExitData()
        {
            using var localConn = _db.GetConnection();
            using var centralConn = _db.GetCentralConnection();
            
            await localConn.OpenAsync();
            await centralConn.OpenAsync();

            using var transaction = centralConn.BeginTransaction();
            try
            {
                // Get pending exits
                using (var command = new SqlCommand(
                    @"SELECT ID, EntryTransactionID, ExitTime, Duration, Amount, PaymentMethod, PaymentStatus, OperatorID 
                      FROM ExitTransactions 
                      WHERE SyncStatus = 'PENDING'", localConn))
                {
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        // Insert to central database
                        using var centralCmd = new SqlCommand(
                            @"INSERT INTO ExitTransactions_Sync 
                              (OriginalID, EntryTransactionID, ExitTime, Duration, Amount, PaymentMethod, PaymentStatus, OperatorID, SyncStatus, SyncTime)
                              VALUES (@OriginalID, @EntryTransactionID, @ExitTime, @Duration, @Amount, @PaymentMethod, @PaymentStatus, @OperatorID, 'SYNCED', GETDATE())",
                            centralConn, transaction);

                        centralCmd.Parameters.AddWithValue("@OriginalID", reader.GetInt32(0));
                        centralCmd.Parameters.AddWithValue("@EntryTransactionID", reader.GetInt32(1));
                        centralCmd.Parameters.AddWithValue("@ExitTime", reader.GetDateTime(2));
                        centralCmd.Parameters.AddWithValue("@Duration", reader.GetInt32(3));
                        centralCmd.Parameters.AddWithValue("@Amount", reader.GetDecimal(4));
                        centralCmd.Parameters.AddWithValue("@PaymentMethod", reader.GetString(5));
                        centralCmd.Parameters.AddWithValue("@PaymentStatus", reader.GetString(6));
                        centralCmd.Parameters.AddWithValue("@OperatorID", reader.GetInt32(7));

                        await centralCmd.ExecuteNonQueryAsync();
                    }
                }

                // Update sync status
                using (var command = new SqlCommand(
                    @"UPDATE ExitTransactions 
                      SET SyncStatus = 'SYNCED' 
                      WHERE SyncStatus = 'PENDING'", localConn))
                {
                    await command.ExecuteNonQueryAsync();
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private async Task LogSyncError(string errorMessage)
        {
            try
            {
                using var centralConn = _db.GetCentralConnection();
                await centralConn.OpenAsync();

                using var command = new SqlCommand("sp_LogSyncError", centralConn)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@TableName", _terminalType == "GETIN" ? "EntryTransactions" : "ExitTransactions");
                command.Parameters.AddWithValue("@ErrorMessage", errorMessage);

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                OnSyncStatusChanged($"Failed to log sync error: {ex.Message}");
            }
        }

        private void OnSyncStatusChanged(string status)
        {
            SyncStatusChanged?.Invoke(this, status);
        }

        public async Task<DataTable> GetSyncStatus()
        {
            try
            {
                using var connection = _db.GetCentralConnection();
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    @"SELECT 
                        TableName,
                        LastSyncTime,
                        PendingRecords,
                        SyncStatus
                      FROM SyncStatus
                      ORDER BY LastSyncTime DESC",
                    connection);

                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);
                return table;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting sync status: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidateSync()
        {
            try
            {
                using var connection = _db.GetCentralConnection();
                await connection.OpenAsync();
                
                using var command = new SqlCommand(
                    @"SELECT COUNT(1) 
                      FROM SyncErrors 
                      WHERE ResolvedAt IS NULL",
                    connection);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) == 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating sync: {ex.Message}", ex);
            }
        }
    }
} 