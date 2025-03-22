using System;
using System.IO;
using System.Threading;
using Npgsql;

namespace ParkingServer
{
    class Program
    {
        private static bool _isRunning = true;
        private static WebSocketServer _server = WebSocketServer.Instance;

        static void Main(string[] args)
        {
            Console.Title = "Parking System Server";
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("================================================================");
            Console.WriteLine("                PARKING SYSTEM SERVER");
            Console.WriteLine("================================================================");
            Console.ResetColor();
            
            // Pastikan direktori logs ada
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            try
            {
                // Inisialisasi koneksi database
                InitializeDatabaseConnection();
                
                // Mulai WebSocket server
                bool started = _server.Start();
                
                if (started)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("WebSocket server running at: ws://0.0.0.0:8181");
                    Console.WriteLine("PostgreSQL database connected.");
                    Console.WriteLine("Server running. Press Ctrl+C to stop.");
                    Console.ResetColor();
                    
                    // Register interrupt handler
                    Console.CancelKeyPress += (sender, e) => {
                        e.Cancel = true;
                        _isRunning = false;
                    };
                    
                    // Keep server running
                    while (_isRunning)
                    {
                        Thread.Sleep(1000);
                        
                        // Tampilkan status setiap 10 detik
                        if (DateTime.Now.Second % 10 == 0)
                        {
                            Console.WriteLine($"[{DateTime.Now}] Active connections: {_server.ActiveConnections}");
                        }
                    }
                    
                    // Shutdown server
                    Console.WriteLine("Shutting down server...");
                    _server.Stop();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to start WebSocket server. Check logs for details.");
                    Console.ResetColor();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Unhandled server error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Console.ResetColor();
                
                LogError($"Unhandled server error: {ex.Message}\n{ex.StackTrace}");
            }
            
            Console.WriteLine("Server stopped. Press Enter to exit.");
            Console.ReadLine();
        }
        
        private static void InitializeDatabaseConnection()
        {
            try
            {
                Console.WriteLine("Memeriksa instalasi PostgreSQL...");
                // Coba deteksi apakah PostgreSQL terinstal
                if (!IsPostgreSqlInstalled())
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("PostgreSQL tidak terdeteksi pada sistem ini.");
                    Console.WriteLine("Pastikan PostgreSQL terinstal dan service sedang berjalan.");
                    Console.ResetColor();
                    
                    LogError("PostgreSQL tidak terdeteksi pada sistem ini.");
                    
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("WARNING: Server akan berjalan tanpa koneksi database.");
                    Console.WriteLine("Beberapa fungsionalitas mungkin terbatas.");
                    Console.ResetColor();
                    return;
                }
                
                string connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
                
                Console.WriteLine("Testing database connection...");
                
                // Coba koneksi ke database utama
                if (TestDatabaseConnection(connectionString))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Koneksi ke database parkirdb berhasil.");
                    Console.ResetColor();
                    return;
                }
                
                // Jika gagal, coba buat database
                Console.WriteLine("Mencoba membuat database parkirdb...");
                if (CreateParkirDatabase())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Database parkirdb berhasil dibuat.");
                    Console.ResetColor();
                    
                    // Test koneksi lagi
                    if (TestDatabaseConnection(connectionString))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Koneksi ke database parkirdb berhasil setelah pembuatan.");
                        Console.ResetColor();
                        return;
                    }
                }
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Gagal terhubung ke database parkirdb setelah pembuatan.");
                Console.ResetColor();
                
                LogError("Gagal terhubung ke database parkirdb setelah pembuatan.");
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNING: Server akan berjalan tanpa koneksi database.");
                Console.WriteLine("Beberapa fungsionalitas mungkin terbatas.");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Database connection error: {ex.Message}");
                Console.ResetColor();
                
                LogError($"Database connection error: {ex.Message}\n{ex.StackTrace}");
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("WARNING: Server will start without database connection.");
                Console.WriteLine("Some functionality may be limited.");
                Console.ResetColor();
            }
        }
        
        private static bool IsPostgreSqlInstalled()
        {
            try
            {
                // Coba koneksi ke service PostgreSQL tanpa database
                using (NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Port=5432;Username=postgres;Password=root@rsi;"))
                {
                    connection.Open();
                    connection.Close();
                    return true;
                }
            }
            catch (Npgsql.NpgsqlException ex)
            {
                if (ex.Message.Contains("connection refused"))
                {
                    LogError("PostgreSQL service tidak berjalan. Pastikan PostgreSQL terinstal dan service aktif.");
                    return false;
                }
                else if (ex.Message.Contains("password authentication"))
                {
                    // Jika mendapat error password, berarti server berjalan tetapi kredensial salah
                    return true;
                }
                
                LogError($"Error saat memeriksa instalasi PostgreSQL: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error saat memeriksa instalasi PostgreSQL: {ex.Message}");
                return false;
            }
        }
        
        private static bool TestDatabaseConnection(string connectionString)
        {
            try
            {
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Gagal terhubung ke database: {ex.Message}");
                return false;
            }
        }
        
        private static bool CreateParkirDatabase()
        {
            try
            {
                // Gunakan koneksi ke database postgres default untuk membuat database parkirdb
                string defaultConn = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=root@rsi;";
                
                using (NpgsqlConnection connection = new NpgsqlConnection(defaultConn))
                {
                    connection.Open();
                    Console.WriteLine("Koneksi ke database default postgres berhasil.");
                    
                    // Cek apakah database parkirdb sudah ada
                    using (NpgsqlCommand command = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'parkirdb'", connection))
                    {
                        var result = command.ExecuteScalar();
                        
                        // Jika database belum ada, buat baru
                        if (result == null || result == DBNull.Value)
                        {
                            Console.WriteLine("Database parkirdb belum ada, mencoba membuat...");
                            using (NpgsqlCommand createCmd = new NpgsqlCommand("CREATE DATABASE parkirdb", connection))
                            {
                                createCmd.ExecuteNonQuery();
                                Console.WriteLine("Database parkirdb berhasil dibuat.");
                                return true;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Database parkirdb sudah ada.");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Gagal membuat database: {ex.Message}");
                Console.ResetColor();
                
                LogError($"Gagal membuat database: {ex.Message}");
                return false;
            }
        }
        
        private static void LogError(string message)
        {
            try
            {
                string logFile = Path.Combine("logs", $"error_{DateTime.Now:yyyyMMdd}.log");
                File.AppendAllText(logFile, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}\n");
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Failed to write to log file.");
                Console.ResetColor();
            }
        }
    }
}