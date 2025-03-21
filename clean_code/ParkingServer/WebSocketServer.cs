using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Fleck;
using System.Diagnostics;
using Newtonsoft.Json;
using Npgsql;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace ParkingServer
{
    /// <summary>
    /// Kelas WebSocketServer menangani komunikasi realtime antara server ParkingIN 
    /// dan client web yang menampilkan status sistem dan notifikasi.
    /// Menggunakan pola Singleton untuk memastikan hanya ada satu instance server.
    /// </summary>
    public class WebSocketServer
    {
        // Singleton instance
        private static WebSocketServer? _instance;
        private static readonly object _lockObject = new object();
        
        // Konfigurasi WebSocket
        private const string DEFAULT_WS_ADDRESS = "ws://0.0.0.0:8181";
        private readonly string _webSocketAddress;
        
        // Pelacakan koneksi 
        private IWebSocketServer _fleckServer;
        private List<IWebSocketConnection> _allSockets;
        private bool _isRunning;
        private System.Threading.Timer _heartbeatTimer;
        private int _totalConnections;
        private int _activeConnections;
        private DateTime _lastHeartbeat;
        private readonly Dictionary<Guid, DateTime> _clientLastActivity;
        private readonly Dictionary<Guid, int> _reconnectAttempts;
        private readonly Dictionary<Guid, int> _connectionErrors;
        private readonly int _maxReconnectAttempts = 3;
        private readonly TimeSpan _reconnectDelay = TimeSpan.FromSeconds(5);
        
        // Database connection
        private string _connectionString = "Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;";
        
        // Thread-safe property untuk status server
        public bool IsRunning
        {
            get { lock (_lockObject) { return _isRunning; } }
            private set { lock (_lockObject) { _isRunning = value; } }
        }
        
        // Thread-safe property untuk jumlah koneksi aktif
        public int ActiveConnections
        {
            get { lock (_lockObject) { return _activeConnections; } }
            private set { lock (_lockObject) { _activeConnections = value; } }
        }
        
        // Thread-safe property untuk total koneksi yang pernah terjadi
        public int TotalConnections
        {
            get { lock (_lockObject) { return _totalConnections; } }
            private set { lock (_lockObject) { _totalConnections = value; } }
        }
        
        // Singleton getter instance
        public static WebSocketServer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new WebSocketServer();
                        }
                    }
                }
                return _instance;
            }
        }
        
        // Private constructor untuk memastikan Singleton pattern
        private WebSocketServer(int port = 8181)
        {
            _allSockets = new List<IWebSocketConnection>();
            _isRunning = false;
            _totalConnections = 0;
            _activeConnections = 0;
            _lastHeartbeat = DateTime.Now;
            _connectionErrors = new Dictionary<Guid, int>();
            _reconnectAttempts = new Dictionary<Guid, int>();
            _clientLastActivity = new Dictionary<Guid, DateTime>();
            
            // Inisialisasi dengan nilai default
            _fleckServer = new Fleck.WebSocketServer($"ws://0.0.0.0:{port}");
            _heartbeatTimer = new System.Threading.Timer(SendHeartbeat, null, Timeout.Infinite, Timeout.Infinite);
            
            // Use default or read from configuration
            _webSocketAddress = GetConfiguredWebSocketAddress();
            if (string.IsNullOrEmpty(_webSocketAddress))
            {
                _webSocketAddress = $"ws://0.0.0.0:{port}";
            }
            
            LogInfo("WebSocket server instance initialized");
        }
        
        /// <summary>
        /// Membaca alamat WebSocket dari file konfigurasi
        /// </summary>
        private string GetConfiguredWebSocketAddress()
        {
            try
            {
                string configPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    "config", 
                    "websocket.ini");
                
                if (File.Exists(configPath))
                {
                    string[] lines = File.ReadAllLines(configPath);
                    foreach (string line in lines)
                    {
                        string trimmedLine = line.Trim();
                        if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#") || trimmedLine.StartsWith("["))
                            continue;
                        
                        string[] parts = trimmedLine.Split('=');
                        if (parts.Length != 2)
                            continue;
                        
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        
                        if (key.Equals("Address", StringComparison.OrdinalIgnoreCase))
                        {
                            LogInfo($"Using configured WebSocket address: {value}");
                            return value;
                        }
                    }
                }
                
                return DEFAULT_WS_ADDRESS;
            }
            catch (Exception ex)
            {
                LogError($"Error reading WebSocket configuration: {ex.Message}");
                return DEFAULT_WS_ADDRESS;
            }
        }
        
        /// <summary>
        /// Memulai WebSocket server
        /// </summary>
        public bool Start()
        {
            if (IsRunning)
            {
                LogWarning("WebSocket server already running");
                return true;
            }
            
            try
            {
                // Dapatkan port dari alamat WebSocket
                int port = 8181; // Default port
                var match = Regex.Match(_webSocketAddress, @":(\d+)$");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int parsedPort))
                {
                    port = parsedPort;
                }
                
                LogInfo($"Starting WebSocket server on port {port}");
                _fleckServer = new Fleck.WebSocketServer(_webSocketAddress);
                
                // Configure server
                _fleckServer.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        lock (_lockObject)
                        {
                            _allSockets.Add(socket);
                            _totalConnections++;
                            _activeConnections++;
                            _clientLastActivity[socket.ConnectionInfo.Id] = DateTime.Now;
                            
                            LogInfo($"Client connected: {socket.ConnectionInfo.ClientIpAddress}. Active connections: {_activeConnections}");
                            
                            // Send welcome message
                            SendWelcomeMessage(socket);
                        }
                    };
                    
                    socket.OnClose = () =>
                    {
                        RemoveClient(socket);
                    };
                    
                    socket.OnMessage = message =>
                    {
                        try
                        {
                            HandleClientMessage(socket, message);
                        }
                        catch (Exception ex)
                        {
                            HandleClientError(socket, ex);
                        }
                    };
                    
                    socket.OnError = ex =>
                    {
                        HandleClientError(socket, ex);
                    };
                });
                
                IsRunning = true;
                LogInfo($"WebSocket server started on {_webSocketAddress}");
                
                // Start heartbeat timer
                StartHeartbeatTimer();
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to start WebSocket server: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Menghentikan WebSocket server
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
            {
                LogWarning("WebSocket server is not running");
                return;
            }

            try
            {
                // Stop heartbeat timer
                _heartbeatTimer?.Change(Timeout.Infinite, Timeout.Infinite);

                // Close all connections
                lock (_lockObject)
                {
                    foreach (var socket in _allSockets.ToList())
                    {
                        try
                        {
                            socket.Close();
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error closing socket: {ex.Message}");
                        }
                    }
                    _allSockets.Clear();
                    _activeConnections = 0;
                }

                // Dispose Fleck server
                _fleckServer?.Dispose();
                _fleckServer = null;

                IsRunning = false;
                LogInfo("WebSocket server stopped");
            }
            catch (Exception ex)
            {
                LogError($"Error stopping WebSocket server: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Memulai timer untuk heartbeat
        /// </summary>
        private void StartHeartbeatTimer()
        {
            // Send heartbeat every 30 seconds
            _heartbeatTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }
        
        /// <summary>
        /// Mengirim heartbeat ke semua client
        /// </summary>
        private void SendHeartbeat(object? state)
        {
            try
            {
                if (!IsRunning) return;
                
                var heartbeat = new { type = "heartbeat", timestamp = DateTime.Now };
                var message = JsonConvert.SerializeObject(heartbeat);
                
                BroadcastMessage(message);
                _lastHeartbeat = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogError($"Error sending heartbeat: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Mengirim pesan selamat datang ke client baru
        /// </summary>
        private void SendWelcomeMessage(IWebSocketConnection socket)
        {
            try
            {
                var welcome = new { 
                    type = "welcome",
                    message = "Connected to Parking System WebSocket Server",
                    timestamp = DateTime.Now
                };
                
                var message = JsonConvert.SerializeObject(welcome);
                socket.Send(message);
            }
            catch (Exception ex)
            {
                LogError($"Error sending welcome message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Menangani pesan dari client
        /// </summary>
        private void HandleClientMessage(IWebSocketConnection socket, string message)
        {
            try
            {
                var data = JsonConvert.DeserializeObject<dynamic>(message);
                if (data == null) return;
                
                string messageType = data.type?.ToString() ?? "";
                
                switch (messageType.ToLower())
                {
                    case "heartbeat":
                        HandleHeartbeat(socket);
                        break;
                    case "vehicle_entry":
                        HandleVehicleEntry(socket, data);
                        break;
                    case "vehicle_exit":
                        HandleVehicleExit(socket, data);
                        break;
                    default:
                        LogWarning($"Unknown message type: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error handling client message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Menangani heartbeat dari client
        /// </summary>
        private void HandleHeartbeat(IWebSocketConnection socket)
        {
            lock (_lockObject)
            {
                _clientLastActivity[socket.ConnectionInfo.Id] = DateTime.Now;
                _connectionErrors[socket.ConnectionInfo.Id] = 0;
                
                var response = new { type = "heartbeat_ack", timestamp = DateTime.Now };
                socket.Send(JsonConvert.SerializeObject(response));
            }
        }
        
        /// <summary>
        /// Menangani notifikasi masuk kendaraan dari client
        /// </summary>
        private void HandleVehicleEntry(IWebSocketConnection socket, dynamic data)
        {
            try
            {
                string plateNumber = data.plate_number?.ToString() ?? "";
                string vehicleType = data.vehicle_type?.ToString() ?? "";
                DateTime entryTime = DateTime.Now;
                
                // Simpan ke database
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO parking_transactions 
                                 (plate_number, vehicle_type, entry_time, status) 
                                 VALUES (@plate, @type, @entry, 'active')";
                    
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@plate", plateNumber);
                        cmd.Parameters.AddWithValue("@type", vehicleType);
                        cmd.Parameters.AddWithValue("@entry", entryTime);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                // Broadcast ke semua client
                var broadcast = new
                {
                    type = "vehicle_entry",
                    plate_number = plateNumber,
                    vehicle_type = vehicleType,
                    entry_time = entryTime
                };
                
                BroadcastMessage(JsonConvert.SerializeObject(broadcast));
            }
            catch (Exception ex)
            {
                LogError($"Error handling vehicle entry: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Menangani notifikasi keluar kendaraan dari client
        /// </summary>
        private void HandleVehicleExit(IWebSocketConnection socket, dynamic data)
        {
            try
            {
                string plateNumber = data.plate_number?.ToString() ?? "";
                DateTime exitTime = DateTime.Now;
                
                // Update database
                using (var conn = new NpgsqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE parking_transactions 
                                 SET exit_time = @exit, status = 'completed' 
                                 WHERE plate_number = @plate AND status = 'active'";
                    
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@plate", plateNumber);
                        cmd.Parameters.AddWithValue("@exit", exitTime);
                        cmd.ExecuteNonQuery();
                    }
                }
                
                // Broadcast ke semua client
                var broadcast = new
                {
                    type = "vehicle_exit",
                    plate_number = plateNumber,
                    exit_time = exitTime
                };
                
                BroadcastMessage(JsonConvert.SerializeObject(broadcast));
            }
            catch (Exception ex)
            {
                LogError($"Error handling vehicle exit: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Mengirim pesan ke semua client
        /// </summary>
        private void BroadcastMessage(string message)
        {
            lock (_lockObject)
            {
                foreach (var socket in _allSockets.ToList())
                {
                    try
                    {
                        socket.Send(message);
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error broadcasting message: {ex.Message}");
                        RemoveClient(socket);
                    }
                }
            }
        }
        
        /// <summary>
        /// Menangani error dari client
        /// </summary>
        private void HandleClientError(IWebSocketConnection socket, Exception ex)
        {
            lock (_lockObject)
            {
                if (!_connectionErrors.ContainsKey(socket.ConnectionInfo.Id))
                {
                    _connectionErrors[socket.ConnectionInfo.Id] = 0;
                }
                
                _connectionErrors[socket.ConnectionInfo.Id]++;
                LogError($"Client error ({socket.ConnectionInfo.ClientIpAddress}): {ex.Message}");
                
                if (_connectionErrors[socket.ConnectionInfo.Id] >= 3)
                {
                    socket.Close();
                }
            }
        }
        
        /// <summary>
        /// Menghapus client dari daftar
        /// </summary>
        private void RemoveClient(IWebSocketConnection socket)
        {
            lock (_lockObject)
            {
                _allSockets.Remove(socket);
                _activeConnections--;
                _clientLastActivity.Remove(socket.ConnectionInfo.Id);
                _connectionErrors.Remove(socket.ConnectionInfo.Id);
                _reconnectAttempts.Remove(socket.ConnectionInfo.Id);
                
                LogInfo($"Client disconnected: {socket.ConnectionInfo.ClientIpAddress}. Active connections: {_activeConnections}");
            }
        }
        
        /// <summary>
        /// Mencatat pesan informasi
        /// </summary>
        private void LogInfo(string message)
        {
            Console.WriteLine($"[INFO] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        
        /// <summary>
        /// Mencatat pesan peringatan
        /// </summary>
        private void LogWarning(string message)
        {
            Console.WriteLine($"[WARN] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
        
        /// <summary>
        /// Mencatat pesan error
        /// </summary>
        private void LogError(string message)
        {
            Console.WriteLine($"[ERROR] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }
}