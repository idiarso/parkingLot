using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using MySql.Data.MySqlClient;
using System.Text.Json.Serialization;

namespace SyncTool
{
    /// <summary>
    /// Kelas utama untuk aplikasi SyncTool, utilitas command-line untuk sinkronisasi
    /// data offline ke database
    /// </summary>
    class Program
    {
        private static string connectionString = "";
        private static int successCount = 0;
        private static int failureCount = 0;
        private static bool verboseMode = false;
        private static bool forceSync = false;
        private static bool moveOnSuccess = true;
        private static string sourceDirectory = "";
        private static string archiveDirectory = "offline_data/archived";
        private static readonly string logDirectory = "logs";

        static async Task Main(string[] args)
        {
            Console.WriteLine("SyncTool - Utilitas Sinkronisasi Data Offline");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            if (!ParseArguments(args))
            {
                ShowHelp();
                return;
            }

            // Siapkan direktori log jika belum ada
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Siapkan direktori arsip jika belum ada dan moveOnSuccess diaktifkan
            if (moveOnSuccess && !Directory.Exists(archiveDirectory))
            {
                Directory.CreateDirectory(archiveDirectory);
            }

            // Baca konfigurasi database
            if (!ReadDatabaseConfig())
            {
                Console.WriteLine("Error: Tidak dapat membaca konfigurasi database.");
                return;
            }

            // Tes koneksi database
            if (!await TestDatabaseConnection())
            {
                Console.WriteLine("Error: Tidak dapat terhubung ke database. Periksa konfigurasi dan pastikan server database aktif.");
                return;
            }

            // Validasi direktori sumber
            if (string.IsNullOrEmpty(sourceDirectory) || !Directory.Exists(sourceDirectory))
            {
                Console.WriteLine($"Error: Direktori sumber '{sourceDirectory}' tidak ditemukan.");
                return;
            }

            // Mulai proses sinkronisasi
            await SyncOfflineData();

            // Tampilkan hasil
            Console.WriteLine();
            Console.WriteLine($"Proses sinkronisasi selesai. Berhasil: {successCount}, Gagal: {failureCount}");
        }

        /// <summary>
        /// Parse argumen command line
        /// </summary>
        private static bool ParseArguments(string[] args)
        {
            if (args.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i].ToLower();
                switch (arg)
                {
                    case "--source":
                        if (i + 1 < args.Length)
                        {
                            sourceDirectory = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("Error: Nilai direktori sumber tidak diberikan.");
                            return false;
                        }
                        break;

                    case "--archive":
                        if (i + 1 < args.Length)
                        {
                            archiveDirectory = args[i + 1];
                            i++;
                        }
                        else
                        {
                            Console.WriteLine("Error: Nilai direktori arsip tidak diberikan.");
                            return false;
                        }
                        break;

                    case "--verbose":
                    case "-v":
                        verboseMode = true;
                        break;

                    case "--force-sync":
                        forceSync = true;
                        break;

                    case "--no-move":
                        moveOnSuccess = false;
                        break;

                    case "--help":
                    case "-h":
                        return false;

                    default:
                        Console.WriteLine($"Argumen tidak dikenal: {arg}");
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Tampilkan bantuan penggunaan
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("Penggunaan: SyncTool [OPSI]");
            Console.WriteLine();
            Console.WriteLine("Opsi:");
            Console.WriteLine("  --source DIR       Direktori sumber berisi file data offline (wajib)");
            Console.WriteLine("  --archive DIR      Direktori untuk menyimpan file yang berhasil disinkronkan");
            Console.WriteLine("                     Default: offline_data/archived");
            Console.WriteLine("  --verbose, -v      Tampilkan informasi detail tentang proses sinkronisasi");
            Console.WriteLine("  --force-sync       Paksa sinkronisasi meskipun file sudah ditandai sebagai disinkronkan");
            Console.WriteLine("  --no-move          Jangan memindahkan file setelah sinkronisasi berhasil");
            Console.WriteLine("  --help, -h         Tampilkan bantuan ini");
            Console.WriteLine();
            Console.WriteLine("Contoh:");
            Console.WriteLine("  SyncTool --source offline_data/ParkingIN");
            Console.WriteLine("  SyncTool --source offline_data/ParkingOut --verbose --force-sync");
        }

        /// <summary>
        /// Baca konfigurasi database dari file
        /// </summary>
        private static bool ReadDatabaseConfig()
        {
            try
            {
                string configPath = "config/database.ini";
                if (!File.Exists(configPath))
                {
                    Log("Error: File konfigurasi database tidak ditemukan di " + configPath, LogLevel.Error);
                    return false;
                }

                var lines = File.ReadAllLines(configPath);
                string server = "localhost";
                string port = "5432";
                string database = "parkirdb";
                string username = "root";
                string password = "root@rsi";

                bool inDatabaseSection = false;
                foreach (var line in lines)
                {
                    string trimmedLine = line.Trim();
                    
                    if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                    {
                        inDatabaseSection = trimmedLine.Equals("[Database]", StringComparison.OrdinalIgnoreCase);
                        continue;
                    }

                    if (!inDatabaseSection || string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    {
                        continue;
                    }

                    var parts = trimmedLine.Split('=');
                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    string key = parts[0].Trim();
                    string value = parts[1].Trim();

                    switch (key.ToLower())
                    {
                        case "server":
                            server = value;
                            break;
                        case "port":
                            port = value;
                            break;
                        case "database":
                            database = value;
                            break;
                        case "username":
                            username = value;
                            break;
                        case "password":
                            password = value;
                            break;
                    }
                }

                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(database))
                {
                    Log("Error: Konfigurasi database tidak lengkap", LogLevel.Error);
                    return false;
                }

                connectionString = $"Server={server};Port={port};Database={database};Uid={username};Pwd={password};";
                Log($"Konfigurasi database berhasil dibaca. Server: {server}, Database: {database}", LogLevel.Info);
                return true;
            }
            catch (Exception ex)
            {
                Log($"Error membaca konfigurasi database: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Tes koneksi ke database
        /// </summary>
        private static async Task<bool> TestDatabaseConnection()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    Log("Koneksi database berhasil", LogLevel.Info);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log($"Error koneksi database: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Sinkronisasi data offline ke database
        /// </summary>
        private static async Task SyncOfflineData()
        {
            try
            {
                // Cari semua file JSON di direktori sumber
                string[] files = Directory.GetFiles(sourceDirectory, "*.json");
                Log($"Menemukan {files.Length} file di {sourceDirectory}", LogLevel.Info);

                if (files.Length == 0)
                {
                    Console.WriteLine("Tidak ada file offline yang ditemukan untuk disinkronkan.");
                    return;
                }

                Console.WriteLine($"Memulai sinkronisasi {files.Length} file...");
                
                // Proses setiap file secara berurutan
                foreach (var file in files)
                {
                    await ProcessFile(file);
                }
            }
            catch (Exception ex)
            {
                Log($"Error selama proses sinkronisasi: {ex.Message}", LogLevel.Error);
                failureCount++;
            }
        }

        /// <summary>
        /// Proses satu file offline
        /// </summary>
        private static async Task ProcessFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            Log($"Memproses file: {fileName}", LogLevel.Info);

            try
            {
                // Baca konten file
                string jsonContent = await File.ReadAllTextAsync(filePath);
                
                // Tentukan jenis file berdasarkan nama file
                if (fileName.StartsWith("parking_entry_"))
                {
                    await SyncParkingEntryData(jsonContent, fileName);
                }
                else if (fileName.StartsWith("parking_exit_"))
                {
                    await SyncParkingExitData(jsonContent, fileName);
                }
                else
                {
                    Log($"Format file tidak dikenal: {fileName}", LogLevel.Warning);
                    failureCount++;
                }
            }
            catch (Exception ex)
            {
                Log($"Error memproses file {fileName}: {ex.Message}", LogLevel.Error);
                failureCount++;
            }
        }

        /// <summary>
        /// Sinkronisasi data kendaraan masuk
        /// </summary>
        private static async Task SyncParkingEntryData(string jsonContent, string fileName)
        {
            try
            {
                var entryData = JsonSerializer.Deserialize<ParkingEntryData>(jsonContent);
                
                // Periksa apakah data sudah disinkronkan dan tidak dalam mode force
                if (!forceSync && entryData.SyncStatus?.ToUpper() == "SYNCED")
                {
                    Log($"File {fileName} sudah disinkronkan sebelumnya (status: {entryData.SyncStatus}). Gunakan --force-sync untuk memaksa sinkronisasi.", LogLevel.Info);
                    return;
                }
                
                // Cetak detail data jika dalam mode verbose
                if (verboseMode)
                {
                    Log($"Detail data masuk: No Kendaraan: {entryData.VehicleNumber}, Waktu: {entryData.EntryTime}, Jenis: {entryData.VehicleType}", LogLevel.Debug);
                }

                // Sinkronkan ke database
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Cek apakah data sudah ada di database
                    string checkSql = "SELECT COUNT(*) FROM t_parkir WHERE nomor_kendaraan = @nopol AND waktu_masuk = @waktuMasuk";
                    
                    using (var checkCommand = new MySqlCommand(checkSql, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@nopol", entryData.VehicleNumber);
                        checkCommand.Parameters.AddWithValue("@waktuMasuk", DateTime.Parse(entryData.EntryTime));
                        
                        long count = (long)await checkCommand.ExecuteScalarAsync();
                        
                        if (count > 0 && !forceSync)
                        {
                            Log($"Data untuk kendaraan {entryData.VehicleNumber} pada {entryData.EntryTime} sudah ada di database", LogLevel.Warning);
                            return;
                        }
                    }
                    
                    // Insert data ke database
                    string insertSql = @"
                        INSERT INTO t_parkir (nomor_kendaraan, waktu_masuk, jenis_kendaraan, foto_masuk, status, sync_id)
                        VALUES (@nopol, @waktuMasuk, @jenisKendaraan, @fotoMasuk, 0, @syncId)
                        ON DUPLICATE KEY UPDATE 
                        jenis_kendaraan = @jenisKendaraan,
                        foto_masuk = @fotoMasuk,
                        sync_id = @syncId";
                    
                    using (var insertCommand = new MySqlCommand(insertSql, connection))
                    {
                        string syncId = Guid.NewGuid().ToString();
                        
                        insertCommand.Parameters.AddWithValue("@nopol", entryData.VehicleNumber);
                        insertCommand.Parameters.AddWithValue("@waktuMasuk", DateTime.Parse(entryData.EntryTime));
                        insertCommand.Parameters.AddWithValue("@jenisKendaraan", entryData.VehicleType);
                        insertCommand.Parameters.AddWithValue("@fotoMasuk", entryData.ImagePath ?? "");
                        insertCommand.Parameters.AddWithValue("@syncId", syncId);
                        
                        await insertCommand.ExecuteNonQueryAsync();
                    }
                }

                // Update status file
                entryData.SyncStatus = "SYNCED";
                string updatedJson = JsonSerializer.Serialize(entryData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Path.Combine(sourceDirectory, fileName), updatedJson);
                
                // Pindahkan file ke direktori arsip jika sukses dan moveOnSuccess diaktifkan
                if (moveOnSuccess)
                {
                    string targetPath = Path.Combine(archiveDirectory, fileName);
                    File.Move(Path.Combine(sourceDirectory, fileName), targetPath, true);
                    Log($"File {fileName} dipindahkan ke {archiveDirectory}", LogLevel.Info);
                }
                
                Log($"Berhasil sinkronisasi data masuk untuk kendaraan {entryData.VehicleNumber}", LogLevel.Info);
                successCount++;
            }
            catch (Exception ex)
            {
                Log($"Error sinkronisasi data masuk dari file {fileName}: {ex.Message}", LogLevel.Error);
                failureCount++;
            }
        }

        /// <summary>
        /// Sinkronisasi data kendaraan keluar
        /// </summary>
        private static async Task SyncParkingExitData(string jsonContent, string fileName)
        {
            try
            {
                var exitData = JsonSerializer.Deserialize<ParkingExitData>(jsonContent);
                
                // Periksa apakah data sudah disinkronkan dan tidak dalam mode force
                if (!forceSync && exitData.SyncStatus?.ToUpper() == "SYNCED")
                {
                    Log($"File {fileName} sudah disinkronkan sebelumnya (status: {exitData.SyncStatus}). Gunakan --force-sync untuk memaksa sinkronisasi.", LogLevel.Info);
                    return;
                }
                
                // Cetak detail data jika dalam mode verbose
                if (verboseMode)
                {
                    Log($"Detail data keluar: No Kendaraan: {exitData.VehicleNumber}, Waktu Keluar: {exitData.ExitTime}, Biaya: {exitData.ParkingFee}", LogLevel.Debug);
                }

                // Sinkronkan ke database
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Update data kendaraan di database
                    string updateSql = @"
                        UPDATE t_parkir 
                        SET waktu_keluar = @waktuKeluar,
                            biaya = @biaya,
                            foto_keluar = @fotoKeluar,
                            status = 1,
                            sync_id = @syncId
                        WHERE nomor_kendaraan = @nopol
                        AND waktu_masuk = @waktuMasuk";
                    
                    using (var updateCommand = new MySqlCommand(updateSql, connection))
                    {
                        string syncId = Guid.NewGuid().ToString();
                        
                        updateCommand.Parameters.AddWithValue("@nopol", exitData.VehicleNumber);
                        updateCommand.Parameters.AddWithValue("@waktuMasuk", DateTime.Parse(exitData.EntryTime));
                        updateCommand.Parameters.AddWithValue("@waktuKeluar", DateTime.Parse(exitData.ExitTime));
                        updateCommand.Parameters.AddWithValue("@biaya", exitData.ParkingFee);
                        updateCommand.Parameters.AddWithValue("@fotoKeluar", exitData.ExitImagePath ?? "");
                        updateCommand.Parameters.AddWithValue("@syncId", syncId);
                        
                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        
                        if (rowsAffected == 0)
                        {
                            Log($"Tidak ada data ditemukan untuk kendaraan {exitData.VehicleNumber} yang masuk pada {exitData.EntryTime}", LogLevel.Warning);
                            failureCount++;
                            return;
                        }
                    }
                }

                // Update status file
                exitData.SyncStatus = "SYNCED";
                string updatedJson = JsonSerializer.Serialize(exitData, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Path.Combine(sourceDirectory, fileName), updatedJson);
                
                // Pindahkan file ke direktori arsip jika sukses dan moveOnSuccess diaktifkan
                if (moveOnSuccess)
                {
                    string targetPath = Path.Combine(archiveDirectory, fileName);
                    File.Move(Path.Combine(sourceDirectory, fileName), targetPath, true);
                    Log($"File {fileName} dipindahkan ke {archiveDirectory}", LogLevel.Info);
                }
                
                Log($"Berhasil sinkronisasi data keluar untuk kendaraan {exitData.VehicleNumber}", LogLevel.Info);
                successCount++;
            }
            catch (Exception ex)
            {
                Log($"Error sinkronisasi data keluar dari file {fileName}: {ex.Message}", LogLevel.Error);
                failureCount++;
            }
        }

        /// <summary>
        /// Simpan pesan log ke file dan tampilkan ke konsol
        /// </summary>
        private static void Log(string message, LogLevel level)
        {
            string levelStr = level.ToString().ToUpper();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] [{levelStr}] {message}";
            
            // Tampilkan ke konsol
            if (level == LogLevel.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(logMessage);
                Console.ResetColor();
            }
            else if (level == LogLevel.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(logMessage);
                Console.ResetColor();
            }
            else if (level == LogLevel.Debug && verboseMode)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(logMessage);
                Console.ResetColor();
            }
            else if (level == LogLevel.Info)
            {
                Console.WriteLine(logMessage);
            }
            
            // Tulis ke file log
            string logFile = Path.Combine(logDirectory, $"sync_tool_{DateTime.Now:yyyyMMdd}.log");
            File.AppendAllText(logFile, logMessage + Environment.NewLine);
        }
    }

    /// <summary>
    /// Level log yang didukung
    /// </summary>
    enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Model data untuk kendaraan masuk
    /// </summary>
    class ParkingEntryData
    {
        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; }
        
        [JsonPropertyName("vehicle_number")]
        public string VehicleNumber { get; set; }
        
        [JsonPropertyName("entry_time")]
        public string EntryTime { get; set; }
        
        [JsonPropertyName("vehicle_type")]
        public string VehicleType { get; set; }
        
        [JsonPropertyName("image_path")]
        public string ImagePath { get; set; }
        
        [JsonPropertyName("sync_status")]
        public string SyncStatus { get; set; }
    }

    /// <summary>
    /// Model data untuk kendaraan keluar
    /// </summary>
    class ParkingExitData
    {
        [JsonPropertyName("transaction_id")]
        public string TransactionId { get; set; }
        
        [JsonPropertyName("vehicle_number")]
        public string VehicleNumber { get; set; }
        
        [JsonPropertyName("entry_time")]
        public string EntryTime { get; set; }
        
        [JsonPropertyName("exit_time")]
        public string ExitTime { get; set; }
        
        [JsonPropertyName("parking_fee")]
        public decimal ParkingFee { get; set; }
        
        [JsonPropertyName("exit_image_path")]
        public string ExitImagePath { get; set; }
        
        [JsonPropertyName("sync_status")]
        public string SyncStatus { get; set; }
    }
} 