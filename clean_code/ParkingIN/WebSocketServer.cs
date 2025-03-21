using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using Fleck;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.WebSockets;
using MySql.Data.MySqlClient;
using ParkingIN.Utils;

namespace ParkingIN
{
    /// <summary>
    /// Kelas WebSocketServer menangani komunikasi realtime antara server ParkingIN 
    /// dan client web yang menampilkan status sistem dan notifikasi.
    /// Menggunakan pola Singleton untuk memastikan hanya ada satu instance server.
    /// </summary>
    public class WebSocketServer
    {
        // Singleton instance
        private static WebSocketServer _instance;
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
        
        // Hardware ports
        private System.IO.Ports.SerialPort gateControlPort;
        private System.IO.Ports.SerialPort loopDetectorPort;
        
        // Camera
        private VideoCaptureDevice _camera;
        private CameraSettings _cameraSettings;
        
        // Database connection
        private string _connectionString = "Server=localhost;Database=parking_system;Uid=root;Pwd=root@rsi;";
        
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
                // Double-check locking untuk thread safety
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
            
            // Initialize hardware ports
            gateControlPort = new System.IO.Ports.SerialPort("COM4", 9600);
            loopDetectorPort = new System.IO.Ports.SerialPort("COM3", 9600);
            
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
                    System.Windows.Forms.Application.StartupPath, 
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
                
                // Jika tidak ada file konfigurasi atau konfigurasi tidak valid, gunakan default
                LogInfo($"Using default WebSocket address: {DEFAULT_WS_ADDRESS}");
                return DEFAULT_WS_ADDRESS;
            }
            catch (Exception ex)
            {
                LogError($"Error reading WebSocket config, using default: {ex.Message}");
                return DEFAULT_WS_ADDRESS;
            }
        }
        
        /// <summary>
        /// Memulai WebSocket server jika belum berjalan
        /// </summary>
        public bool Start()
        {
            try
            {
                if (IsRunning)
                {
                    LogWarning("Attempted to start WebSocket server that is already running");
                    return false;
                }
                
                LogInfo($"Starting WebSocket server on {_webSocketAddress}");
                
                // Configure Fleck WebSocket Server with retry logic
                int retryCount = 0;
                const int maxRetries = 3;
                bool success = false;
                
                while (retryCount < maxRetries && !success)
                {
                    try
                    {
                        _fleckServer = new Fleck.WebSocketServer(_webSocketAddress);
                        FleckLog.Level = (Fleck.LogLevel)LogLevel.Debug;
                        
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
                                }
                                LogInfo($"New connection from {socket.ConnectionInfo.ClientIpAddress}");
                                
                                // Send welcome message
                                socket.Send(JsonConvert.SerializeObject(new
                                {
                                    type = "welcome",
                                    message = "Connected to WebSocket server",
                                    timestamp = DateTime.Now
                                }));
                            };
                            
                            socket.OnClose = () =>
                            {
                                lock (_lockObject)
                                {
                                    _allSockets.Remove(socket);
                                    _activeConnections--;
                                    _clientLastActivity.Remove(socket.ConnectionInfo.Id);
                                }
                                LogInfo($"Connection closed from {socket.ConnectionInfo.ClientIpAddress}");
                            };
                            
                            socket.OnMessage = message =>
                            {
                                try
                                {
                                    // Parse message from client
                                    var messageObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                                    
                                    if (messageObj != null && messageObj.TryGetValue("type", out object typeObj))
                                    {
                                        string messageType = typeObj.ToString();
                                        switch (messageType)
                                        {
                                            case "ping":
                                                // Respond to ping for latency testing
                                                socket.Send(JsonConvert.SerializeObject(new
                                                {
                                                    type = "pong",
                                                    timestamp = DateTime.Now
                                                }));
                                                break;
                                                
                                            case "get_status":
                                                // Send current system status
                                                SendSystemStatusToClient(socket);
                                                break;
                                                
                                            case "heartbeat":
                                                // Update last activity time
                                                _clientLastActivity[socket.ConnectionInfo.Id] = DateTime.Now;
                                                break;
                                                
                                            default:
                                                LogWarning($"Unknown message type received: {messageType}");
                                                SendErrorMessageToClient(socket, $"Unknown message type: {messageType}");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        LogWarning("Invalid message format received");
                                        SendErrorMessageToClient(socket, "Invalid message format. Message must include 'type' field.");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    LogError($"Error processing message: {ex.Message}");
                                    SendErrorMessageToClient(socket, "Error processing your request. Invalid message format.");
                                }
                            };
                            
                            socket.OnError = error =>
                            {
                                LogError($"Socket error for {socket.ConnectionInfo.ClientIpAddress}: {error.Message}");
                                HandleClientError(socket, error);
                            };
                        });
                        
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        LogError($"Failed to start Fleck server (attempt {retryCount}/{maxRetries}): {ex.Message}");
                        
                        if (retryCount < maxRetries)
                        {
                            Thread.Sleep(1000); // Wait 1 second before retry
                            continue;
                        }
                        
                        throw new Exception($"Failed to start Fleck server after {maxRetries} attempts: {ex.Message}");
                    }
                }
                
                if (success)
                {
                    // Start heartbeat timer
                    _heartbeatTimer = new System.Threading.Timer(HeartbeatCallback, null, 30000, 30000);
                    
                    _isRunning = true;
                    LogInfo("WebSocket server started successfully");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error starting WebSocket server: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Mulai timer untuk heartbeat berkala ke semua klien
        /// </summary>
        private void StartHeartbeatTimer()
        {
            try
            {
                // Jalankan heartbeat setiap 30 detik
                _heartbeatTimer = new System.Threading.Timer(SendHeartbeat, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
                LogInfo("Heartbeat timer started");
            }
            catch (Exception ex)
            {
                LogError($"Failed to start heartbeat timer: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Sends a heartbeat message to all connected clients to keep the connection alive
        /// </summary>
        private void SendHeartbeat(object state)
        {
            try
            {
                // Add null check to prevent NullReferenceException if _allSockets is empty
                if (_allSockets == null)
                {
                    LogWarning("SendHeartbeat called but _allSockets is null");
                    return;
                }

                var heartbeat = new { 
                    type = "heartbeat", 
                    timestamp = DateTime.Now,
                    connectedClients = _allSockets.Count
                };
                var message = JsonConvert.SerializeObject(heartbeat);
                
                // Use BroadcastMessage with message type for improved logging
                BroadcastMessage(message, "heartbeat");
                
                _lastHeartbeat = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogError($"Error in SendHeartbeat: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Mengirim pesan selamat datang ke klien yang baru terhubung
        /// </summary>
        private void SendWelcomeMessage(IWebSocketConnection socket)
        {
            try
            {
                socket.Send(JsonConvert.SerializeObject(new
                {
                    type = "welcome",
                    message = "Welcome to ParkingIN Monitoring System",
                    serverTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    connectionId = socket.ConnectionInfo.Id.ToString()
                }));
            }
            catch (Exception ex)
            {
                LogError($"Error sending welcome message: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Menangani pesan yang dikirim oleh klien
        /// </summary>
        private void HandleClientMessage(IWebSocketConnection socket, string message)
        {
            try
            {
                LogInfo($"Received message from {socket.ConnectionInfo.ClientIpAddress}: {message}");
                
                // Parse pesan dari klien
                var messageObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(message);
                
                if (messageObj != null && messageObj.TryGetValue("type", out object typeObj))
                {
                    string type = typeObj.ToString();
                    
                    switch (type.ToLower())
                    {
                        case "ping":
                            // Respons ping-pong untuk latency testing
                            socket.Send(JsonConvert.SerializeObject(new
                            {
                                type = "pong",
                                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                            }));
                            break;
                            
                        case "get_status":
                            // Klien meminta refresh status
                            SendSystemStatusToClient(socket);
                            break;
                            
                        default:
                            LogWarning($"Unknown message type: {type}");
                            SendErrorMessageToClient(socket, $"Unknown message type: {type}");
                            break;
                    }
                }
                else
                {
                    LogWarning("Invalid message format received");
                    SendErrorMessageToClient(socket, "Invalid message format. Message must include 'type' field.");
                }
            }
            catch (Exception ex)
            {
                LogError($"Error processing client message '{message}': {ex.Message}");
                SendErrorMessageToClient(socket, "Error processing your request. Invalid message format.");
            }
        }
        
        /// <summary>
        /// Mengirim pesan error ke klien
        /// </summary>
        private void SendErrorMessageToClient(IWebSocketConnection socket, string errorMessage)
        {
            try
            {
                socket.Send(JsonConvert.SerializeObject(new
                {
                    type = "error",
                    message = errorMessage,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }));
            }
            catch (Exception ex)
            {
                LogError($"Error sending error message to client: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Mengirim status sistem terkini ke satu klien tertentu
        /// </summary>
        private void SendSystemStatusToClient(IWebSocketConnection socket)
        {
            try
            {
                var statusData = GetCurrentSystemStatus();
                socket.Send(JsonConvert.SerializeObject(statusData));
            }
            catch (Exception ex)
            {
                LogError($"Error sending system status to client: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Mengirim notifikasi kepada semua client bahwa ada kendaraan baru masuk
        /// </summary>
        public void NotifyVehicleEntry(string licensePlate, DateTime entryTime)
        {
            try
            {
                if (!IsRunning)
                {
                    LogWarning("Cannot notify vehicle entry: WebSocket server not running");
                    return;
                }
                
                // Get additional vehicle information
                string vehicleType = GetVehicleType(licensePlate);
                string ticketNumber = GenerateTicketNumber(entryTime);
                
                var message = new
                {
                    type = "vehicle_entry",
                    licensePlate = licensePlate,
                    vehicleType = vehicleType,
                    ticketNumber = ticketNumber,
                    entryTime = entryTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    status = new
                    {
                        gate = "opening",
                        camera = "active",
                        printer = "ready"
                    }
                };
                
                // Broadcast with retry logic
                int retryCount = 0;
                const int MAX_RETRIES = 3;
                bool broadcastSuccess = false;
                
                while (!broadcastSuccess && retryCount < MAX_RETRIES)
                {
                    try
                    {
                        BroadcastMessage(JsonConvert.SerializeObject(message));
                        broadcastSuccess = true;
                        LogInfo($"Vehicle entry notification broadcast for {licensePlate}");
                        
                        // Update system status after successful notification
                        BroadcastSystemStatus();
                    }
                    catch (Exception ex)
                    {
                        retryCount++;
                        LogError($"Error broadcasting vehicle entry notification (attempt {retryCount}/{MAX_RETRIES}): {ex.Message}");
                        
                        if (retryCount < MAX_RETRIES)
                        {
                            Thread.Sleep(1000); // Wait before retry
                        }
                        else
                        {
                            LogError($"Failed to broadcast vehicle entry notification after {MAX_RETRIES} attempts");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error in NotifyVehicleEntry: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// Get vehicle type from database
        /// </summary>
        private string GetVehicleType(string licensePlate)
        {
            try
            {
                string query = $"SELECT jenis_kendaraan FROM t_parkir WHERE nomor_polisi = '{licensePlate}' ORDER BY waktu_masuk DESC LIMIT 1";
                object result = Database.ExecuteScalar(query);
                return result?.ToString() ?? "Unknown";
            }
            catch (Exception ex)
            {
                LogError($"Error getting vehicle type: {ex.Message}");
                return "Unknown";
            }
        }
        
        /// <summary>
        /// Generate ticket number based on entry time
        /// </summary>
        private string GenerateTicketNumber(DateTime entryTime)
        {
            return $"TICKET_{entryTime:yyyyMMddHHmmss}";
        }
        
        /// <summary>
        /// Broadcast pesan ke semua klien yang terhubung
        /// </summary>
        private void BroadcastMessage(string message, string messageType = "unknown")
        {
            try
            {
                // Add null check for _allSockets
                if (_allSockets == null)
                {
                    LogWarning($"BroadcastMessage called with type '{messageType}' but _allSockets is null");
                    return;
                }

                // Create a copy of the collection to avoid modification during enumeration
                List<IWebSocketConnection> socketsToNotify;
                lock (_lockObject)
                {
                    socketsToNotify = _allSockets.ToList();
                }

                int successCount = 0;
                foreach (var socket in socketsToNotify)
                {
                    try
                    {
                        if (socket.IsAvailable)
                        {
                            socket.Send(message);
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error sending {messageType} message to client {socket.ConnectionInfo.ClientIpAddress}: {ex.Message}");
                    }
                }

                if (socketsToNotify.Count > 0)
                {
                    LogInfo($"Broadcast {messageType} message to {successCount}/{socketsToNotify.Count} clients");
                }
            }
            catch (Exception ex)
            {
                // Include message type in the error log for better debugging context
                LogError($"Error broadcasting {messageType} message: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Check if an error is potentially recoverable
        /// </summary>
        private bool IsRecoverableError(Exception ex)
        {
            // Network-related errors that might be temporary
            if (ex is System.Net.Sockets.SocketException ||
                ex is System.IO.IOException ||
                ex.Message.Contains("Connection reset") ||
                ex.Message.Contains("Network error") ||
                ex.Message.Contains("Connection refused"))
            {
                return true;
            }
            
            // WebSocket-specific errors that might be temporary
            if (ex.Message.Contains("WebSocket connection is not in the Open state") ||
                ex.Message.Contains("WebSocket connection is not available"))
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Handle client connection errors
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
                
                if (_connectionErrors[socket.ConnectionInfo.Id] >= _maxReconnectAttempts)
                {
                    socket.Close();
                }
            }
        }
        
        /// <summary>
        /// Remove a client from the active connections
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
        /// Ekstrak tipe pesan dari objek pesan
        /// </summary>
        private string GetMessageType(object message)
        {
            try
            {
                Type type = message.GetType();
                var property = type.GetProperty("type");
                if (property != null)
                {
                    return property.GetValue(message)?.ToString() ?? "unknown";
                }
                return "unknown";
            }
            catch
            {
                return "unknown";
            }
        }
        
        /// <summary>
        /// Mendapatkan status sistem saat ini untuk dikirim ke klien
        /// </summary>
        private object GetCurrentSystemStatus()
        {
            try
            {
                int vehiclesInside = GetVehiclesInsideCount();
                int todayEntries = GetTodayEntriesCount();
                
                return new
                {
                    type = "system_status",
                    status = new
                    {
                        server = "online",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        db_connection = IsDatabaseConnected() ? "connected" : "disconnected",
                        devices = GetDevicesStatus()
                    },
                    statistics = new
                    {
                        vehicles_inside = vehiclesInside,
                        today_entries = todayEntries,
                        active_connections = ActiveConnections,
                        total_connections = TotalConnections
                    }
                };
            }
            catch (Exception ex)
            {
                LogError($"Error getting system status: {ex.Message}");
                return new
                {
                    type = "system_status",
                    status = new
                    {
                        server = "online",
                        time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                        db_connection = "unknown",
                        devices = new object[] { }
                    },
                    statistics = new
                    {
                        vehicles_inside = -1,
                        today_entries = -1,
                        active_connections = ActiveConnections,
                        total_connections = TotalConnections
                    },
                    error = $"Error getting complete system status: {ex.Message}"
                };
            }
        }
        
        /// <summary>
        /// Broadcast status sistem terkini ke semua klien
        /// </summary>
        public void BroadcastSystemStatus()
        {
            try
            {
                if (!IsRunning)
                {
                    LogWarning("Cannot broadcast system status: WebSocket server not running");
                    return;
                }
                
                var statusData = GetCurrentSystemStatus();
                BroadcastMessage(JsonConvert.SerializeObject(statusData));
            }
            catch (Exception ex)
            {
                LogError($"Error broadcasting system status: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Memeriksa apakah database terhubung
        /// </summary>
        private bool IsDatabaseConnected()
        {
            try
            {
                using (var connection = new MySql.Data.MySqlClient.MySqlConnection(_connectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError($"Database connection error: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Mendapatkan jumlah kendaraan yang masih di dalam parkir
        /// </summary>
        private int GetVehiclesInsideCount()
        {
            try
            {
                // Query untuk menghitung kendaraan yang masih di dalam
                const string query = "SELECT COUNT(*) FROM t_parkir WHERE status = 'MASUK'";
                object result = Database.ExecuteScalar(query);
                
                if (result != null && int.TryParse(result.ToString(), out int count))
                {
                    return count;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogError($"Error counting vehicles inside: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// Mendapatkan jumlah kendaraan yang masuk hari ini
        /// </summary>
        private int GetTodayEntriesCount()
        {
            try
            {
                // Mendapatkan tanggal hari ini dalam format yang sesuai dengan database
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                
                // Query untuk menghitung kendaraan yang masuk hari ini
                string query = $"SELECT COUNT(*) FROM t_parkir WHERE DATE(waktu_masuk) = '{today}'";
                object result = Database.ExecuteScalar(query);
                
                if (result != null && int.TryParse(result.ToString(), out int count))
                {
                    return count;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogError($"Error counting today's entries: {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// Mendapatkan status perangkat yang terhubung (kamera, gate, printer)
        /// </summary>
        private object[] GetDevicesStatus()
        {
            try
            {
                var devices = new List<object>();
                
                // Camera status
                devices.Add(new
                {
                    name = "camera",
                    status = IsCameraActive() ? "active" : "inactive",
                    last_frame = GetLastFrameTime(),
                    resolution = GetCameraResolution(),
                    fps = GetCameraFPS()
                });
                
                // Gate status
                devices.Add(new
                {
                    name = "gate",
                    status = GetGateStatus(),
                    last_action = GetLastGateAction(),
                    position = GetGatePosition(),
                    error_count = GetGateErrorCount()
                });
                
                // Printer status
                devices.Add(new
                {
                    name = "printer",
                    status = IsPrinterReady() ? "ready" : "error",
                    last_print = GetLastPrintTime(),
                    paper_status = GetPrinterPaperStatus(),
                    error_message = GetPrinterErrorMessage()
                });
                
                // Loop detector status
                devices.Add(new
                {
                    name = "loop_detector",
                    status = IsLoopDetectorActive() ? "active" : "inactive",
                    last_detection = GetLastDetectionTime(),
                    sensitivity = GetLoopDetectorSensitivity(),
                    error_count = GetLoopDetectorErrorCount()
                });
                
                return devices.ToArray();
            }
            catch (Exception ex)
            {
                LogError($"Error getting devices status: {ex.Message}");
                return new object[] { };
            }
        }
        
        /// <summary>
        /// Check if camera is active and functioning
        /// </summary>
        private bool IsCameraActive()
        {
            try
            {
                // Check if camera process is running
                var processes = System.Diagnostics.Process.GetProcessesByName("ParkingIN");
                if (processes.Length == 0) return false;
                
                // Check camera configuration
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "camera.ini");
                if (!File.Exists(configPath)) return false;
                
                // Additional camera checks can be added here
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error checking camera status: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get timestamp of last camera frame
        /// </summary>
        private string GetLastFrameTime()
        {
            try
            {
                string imagesDir = Path.Combine(System.Windows.Forms.Application.StartupPath, "Images", "Entry");
                if (!Directory.Exists(imagesDir)) return "never";
                
                var files = Directory.GetFiles(imagesDir, "*.jpg")
                                    .OrderByDescending(f => f)
                                    .FirstOrDefault();
                                    
                if (files == null) return "never";
                
                return File.GetLastWriteTime(files).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                LogError($"Error getting last frame time: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get camera resolution
        /// </summary>
        private string GetCameraResolution()
        {
            try
            {
                // Read from camera config
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "camera.ini");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    var resolution = lines.FirstOrDefault(l => l.StartsWith("Resolution="));
                    return resolution?.Split('=')[1] ?? "unknown";
                }
                return "unknown";
            }
            catch (Exception ex)
            {
                LogError($"Error getting camera resolution: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get camera FPS
        /// </summary>
        private int GetCameraFPS()
        {
            try
            {
                // Read from camera config
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "camera.ini");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    var fps = lines.FirstOrDefault(l => l.StartsWith("FPS="));
                    if (int.TryParse(fps?.Split('=')[1], out int result))
                    {
                        return result;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogError($"Error getting camera FPS: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Get current gate status
        /// </summary>
        private string GetGateStatus()
        {
            try
            {
                // Check gate control port
                if (gateControlPort != null && gateControlPort.IsOpen)
                {
                    return "connected";
                }
                return "disconnected";
            }
            catch (Exception ex)
            {
                LogError($"Error getting gate status: {ex.Message}");
                return "error";
            }
        }
        
        /// <summary>
        /// Get timestamp of last gate action
        /// </summary>
        private string GetLastGateAction()
        {
            try
            {
                // Read from gate log
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "gate.log");
                if (File.Exists(logPath))
                {
                    var lines = File.ReadAllLines(logPath)
                                   .Where(l => l.Contains("GATE_ACTION"))
                                   .OrderByDescending(l => l)
                                   .FirstOrDefault();
                                   
                    if (lines != null)
                    {
                        var parts = lines.Split('|');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
                return "never";
            }
            catch (Exception ex)
            {
                LogError($"Error getting last gate action: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get current gate position
        /// </summary>
        private string GetGatePosition()
        {
            try
            {
                // Read from gate log
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "gate.log");
                if (File.Exists(logPath))
                {
                    var lines = File.ReadAllLines(logPath)
                                   .Where(l => l.Contains("GATE_STATUS"))
                                   .OrderByDescending(l => l)
                                   .FirstOrDefault();
                                   
                    if (lines != null)
                    {
                        var parts = lines.Split('|');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
                return "unknown";
            }
            catch (Exception ex)
            {
                LogError($"Error getting gate position: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get gate error count from log
        /// </summary>
        private int GetGateErrorCount()
        {
            try
            {
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "gate.log");
                if (File.Exists(logPath))
                {
                    return File.ReadAllLines(logPath)
                               .Count(l => l.Contains("GATE_ERROR"));
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogError($"Error getting gate error count: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Check if printer is ready
        /// </summary>
        private bool IsPrinterReady()
        {
            try
            {
                // Check printer configuration
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "printer.ini");
                if (!File.Exists(configPath)) return false;
                
                // Check if printer is installed
                var printerName = GetConfiguredPrinterName();
                if (string.IsNullOrEmpty(printerName)) return false;
                
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Error checking printer status: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get timestamp of last print job
        /// </summary>
        private string GetLastPrintTime()
        {
            try
            {
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "printer.log");
                if (File.Exists(logPath))
                {
                    var lines = File.ReadAllLines(logPath)
                                   .Where(l => l.Contains("PRINT_SUCCESS"))
                                   .OrderByDescending(l => l)
                                   .FirstOrDefault();
                                   
                    if (lines != null)
                    {
                        var parts = lines.Split('|');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
                return "never";
            }
            catch (Exception ex)
            {
                LogError($"Error getting last print time: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get printer paper status
        /// </summary>
        private string GetPrinterPaperStatus()
        {
            try
            {
                // Check printer status through Windows API
                var printerName = GetConfiguredPrinterName();
                if (string.IsNullOrEmpty(printerName)) return "unknown";
                
                // Additional printer status checks can be added here
                return "ok";
            }
            catch (Exception ex)
            {
                LogError($"Error getting printer paper status: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get printer error message from log
        /// </summary>
        private string GetPrinterErrorMessage()
        {
            try
            {
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "printer.log");
                if (File.Exists(logPath))
                {
                    var lines = File.ReadAllLines(logPath)
                                   .Where(l => l.Contains("PRINTER_ERROR"))
                                   .OrderByDescending(l => l)
                                   .FirstOrDefault();
                                   
                    if (lines != null)
                    {
                        var parts = lines.Split('|');
                        if (parts.Length > 2)
                        {
                            return parts[2].Trim();
                        }
                    }
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                LogError($"Error getting printer error message: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Check if loop detector is active
        /// </summary>
        private bool IsLoopDetectorActive()
        {
            try
            {
                // Check loop detector port
                if (loopDetectorPort != null && loopDetectorPort.IsOpen)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                LogError($"Error checking loop detector status: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get timestamp of last vehicle detection
        /// </summary>
        private string GetLastDetectionTime()
        {
            try
            {
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "loop_detector.log");
                if (File.Exists(logPath))
                {
                    var lines = File.ReadAllLines(logPath)
                                   .Where(l => l.Contains("VEHICLE_DETECTED"))
                                   .OrderByDescending(l => l)
                                   .FirstOrDefault();
                                   
                    if (lines != null)
                    {
                        var parts = lines.Split('|');
                        if (parts.Length > 1)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
                return "never";
            }
            catch (Exception ex)
            {
                LogError($"Error getting last detection time: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get loop detector sensitivity setting
        /// </summary>
        private string GetLoopDetectorSensitivity()
        {
            try
            {
                // Read from loop detector config
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "loop_detector.ini");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    var sensitivity = lines.FirstOrDefault(l => l.StartsWith("Sensitivity="));
                    return sensitivity?.Split('=')[1] ?? "unknown";
                }
                return "unknown";
            }
            catch (Exception ex)
            {
                LogError($"Error getting loop detector sensitivity: {ex.Message}");
                return "unknown";
            }
        }
        
        /// <summary>
        /// Get loop detector error count from log
        /// </summary>
        private int GetLoopDetectorErrorCount()
        {
            try
            {
                string logPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "logs", "loop_detector.log");
                if (File.Exists(logPath))
                {
                    return File.ReadAllLines(logPath)
                               .Count(l => l.Contains("LOOP_DETECTOR_ERROR"));
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogError($"Error getting loop detector error count: {ex.Message}");
                return 0;
            }
        }
        
        /// <summary>
        /// Get configured printer name from config file
        /// </summary>
        private string GetConfiguredPrinterName()
        {
            try
            {
                string configPath = Path.Combine(System.Windows.Forms.Application.StartupPath, "config", "printer.ini");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    var name = lines.FirstOrDefault(l => l.StartsWith("Name="));
                    return name?.Split('=')[1] ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                LogError($"Error getting configured printer name: {ex.Message}");
                return string.Empty;
            }
        }
        
        /// <summary>
        /// Menghentikan WebSocket server dan melepaskan resources
        /// </summary>
        public void Stop()
        {
            if (_fleckServer == null || !IsRunning)
            {
                return;
            }

            try
            {
                LogInfo("Stopping WebSocket server");
                
                // Set flag to false first to prevent new connections
                IsRunning = false;
                
                // Stop heartbeat timer
                if (_heartbeatTimer != null)
                {
                    ((System.Threading.Timer)_heartbeatTimer).Change(Timeout.Infinite, Timeout.Infinite);
                    _heartbeatTimer = null;
                }

                // Send shutdown message
                BroadcastMessage(JsonConvert.SerializeObject(new
                {
                    type = "server_shutdown",
                    message = "Server is shutting down",
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                }), "shutdown");

                // Close all connections
                CloseAllConnections();
                
                // Stop server
                if (_fleckServer != null)
                {
                    _fleckServer.Dispose();
                    _fleckServer = null;
                }
                
                LogInfo("WebSocket server stopped");

                // Properly clean up _allSockets collection using lock
                lock (_lockObject)
                {
                    if (_allSockets != null)
                    {
                        _allSockets.Clear();
                        _allSockets = null;
                    }
                    ActiveConnections = 0;
                }
            }
            catch (Exception ex)
            {
                LogError($"Error stopping WebSocket server: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Closes all open WebSocket connections
        /// </summary>
        private void CloseAllConnections()
        {
            try
            {
                // Create a copy of the collection to avoid modification during enumeration
                List<IWebSocketConnection> socketsToClose;
                lock (_lockObject)
                {
                    if (_allSockets == null)
                    {
                        return;
                    }
                    socketsToClose = _allSockets.ToList();
                }

                // Close each connection with a proper shutdown message
                foreach (var socket in socketsToClose)
                {
                    try
                    {
                        if (socket.IsAvailable)
                        {
                            socket.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"Error closing client connection: {ex.Message}");
                    }
                }

                LogInfo($"Closed {socketsToClose.Count} connections");
            }
            catch (Exception ex)
            {
                LogError($"Error in CloseAllConnections: {ex.Message}", ex);
            }
        }
        
        #region Logging
        
        /// <summary>
        /// Log informasi umum tentang WebSocketServer
        /// </summary>
        private void LogInfo(string message)
        {
            try
            {
                string logPath = Path.Combine(
                    System.Windows.Forms.Application.StartupPath, 
                    "logs", 
                    "system.log");
                    
                string directory = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WebSocket] INFO: {message}\n";
                File.AppendAllText(logPath, logMessage);
            }
            catch
            {
                // Ignore errors in logging, to prevent cascading failures
            }
        }
        
        /// <summary>
        /// Log peringatan tentang WebSocketServer
        /// </summary>
        private void LogWarning(string message)
        {
            try
            {
                var logMessage = $"[WARNING] {DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
                File.AppendAllText(Path.Combine("logs", "websocket.log"), logMessage + Environment.NewLine);
                Debug.WriteLine(logMessage);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing to log file: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Log error tentang WebSocketServer
        /// </summary>
        private void LogError(string message, Exception ex = null)
        {
            try
            {
                string logDir = Path.Combine(Application.StartupPath, "logs");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                
                string logPath = Path.Combine(logDir, $"websocket_{DateTime.Now:yyyyMMdd}.log");
                
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WebSocket] ERROR: {message}\n";
                File.AppendAllText(logPath, logMessage);
                
                // Log exception details if available
                if (ex != null)
                {
                    string exceptionDetails = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [WebSocket] EXCEPTION: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}\n";
                    File.AppendAllText(logPath, exceptionDetails);
                }
                
                Debug.WriteLine($"WebSocket ERROR: {message}");
            }
            catch
            {
                // Suppress errors in the error logger
                Debug.WriteLine($"Error in WebSocket logger: {message}");
            }
        }
        
        #endregion
        
        private void HeartbeatCallback(object state)
        {
            try
            {
                var now = DateTime.Now;
                var inactiveThreshold = TimeSpan.FromSeconds(90);
                var inactiveClients = new List<IWebSocketConnection>();
                
                // Find inactive clients
                lock (_lockObject)
                {
                    inactiveClients = _allSockets.Where(s => 
                        (now - _clientLastActivity[s.ConnectionInfo.Id]) > inactiveThreshold).ToList();
                }
                
                // Handle inactive clients
                foreach (var client in inactiveClients)
                {
                    try
                    {
                        string clientIp = client.ConnectionInfo.ClientIpAddress;
                        LogWarning($"Client {clientIp} inactive for more than {inactiveThreshold.TotalSeconds} seconds");
                        
                        // Try to send a ping to check if client is still alive
                        client.Send(JsonConvert.SerializeObject(new
                        {
                            type = "ping",
                            timestamp = now
                        }));
                        
                        // If client doesn't respond within 5 seconds, remove it
                        Thread.Sleep(5000);
                        
                        if ((now - _clientLastActivity[client.ConnectionInfo.Id]) > inactiveThreshold)
                        {
                            LogWarning($"Client {clientIp} failed to respond to ping, removing connection");
                            RemoveClient(client);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error handling inactive client: {ex.Message}");
                        RemoveClient(client);
                    }
                }
                
                _lastHeartbeat = now;
            }
            catch (Exception ex)
            {
                LogError($"Error in heartbeat callback: {ex.Message}");
            }
        }

        private void InitializeCamera()
        {
            try
            {
                // Load camera settings from config file
                string configPath = Path.Combine(Application.StartupPath, "config", "camera.cfg");
                _cameraSettings = CameraSettings.LoadFromFile(configPath);
                
                // Initialize camera based on camera type
                if (_cameraSettings.CameraType == 0) // Local webcam
                {
                    // If device ID is empty, try to get the first available camera
                    if (string.IsNullOrEmpty(_cameraSettings.DeviceId))
                    {
                        var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                        if (videoDevices.Count > 0)
                        {
                            _cameraSettings.DeviceId = videoDevices[0].MonikerString;
                        }
                        else
                        {
                            LogWarning("No local webcam devices found.");
                            return;
                        }
                    }
                    
                    _camera = new VideoCaptureDevice(_cameraSettings.DeviceId);
                    _camera.VideoResolution = _camera.VideoCapabilities.FirstOrDefault();
                    _camera.NewFrame += Camera_NewFrame;
                    _camera.Start();
                    LogInfo($"Local webcam initialized successfully with device ID: {_cameraSettings.DeviceId}");
                }
                else // IP Camera (Type == 1)
                {
                    LogWarning("IP camera support is not fully implemented. Using local webcam fallback if available.");
                    
                    // Attempt to use local webcam as fallback
                    var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                    if (videoDevices.Count > 0)
                    {
                        _camera = new VideoCaptureDevice(videoDevices[0].MonikerString);
                        _camera.VideoResolution = _camera.VideoCapabilities.FirstOrDefault();
                        _camera.NewFrame += Camera_NewFrame;
                        _camera.Start();
                        LogInfo("Using local webcam as fallback");
                    }
                    else
                    {
                        LogWarning("No camera devices available");
                    }
                }
            }
            catch (Exception ex)
            {
                LogError($"Error initializing camera: {ex.Message}");
            }
        }
        
        private void Camera_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                // Process the new frame here
                // You might want to add implementation to handle the frame such as:
                // - Store it for later use
                // - Process it for license plate recognition
                // - etc.
                
                // For now, we'll just log that we received a frame
                LogInfo("New camera frame received");
            }
            catch (Exception ex)
            {
                LogError($"Error processing camera frame: {ex.Message}");
            }
        }
    }
} 