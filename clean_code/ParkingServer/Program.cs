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
                string connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    Console.WriteLine("Testing database connection...");
                    connection.Open();
                    Console.WriteLine("Database connection successful.");
                }
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