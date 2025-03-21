using System;
using System.IO.Ports;
using System.Threading;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Collections.Generic;
using Timer = System.Windows.Forms.Timer;

namespace ParkingIN
{
    /// <summary>
    /// Kelas untuk mengelola koneksi dan komunikasi dengan mikrokontroler ATMEL AVR melalui RS232
    /// </summary>
    public class AVRController
    {
        // Konstanta
        private const int DEFAULT_TIMEOUT = 5000;        // Timeout default 5 detik
        private const int PING_INTERVAL = 2000;          // Interval ping 2 detik
        private const int MAX_RECONNECT_ATTEMPTS = 3;    // Maksimal percobaan reconnect
        private const int COMMAND_TIMEOUT = 3000;        // Timeout untuk menunggu respons dari perintah

        // Variabel komunikasi serial
        private SerialPort serialPort;
        private SerialWatchdog watchdog;
        private readonly Control invokeControl;
        private bool isInitialized;
        private bool isConnected;
        private readonly StringBuilder receiveBuffer = new();
        private readonly Queue<string> messageQueue = new();
        private readonly Dictionary<string, Action<string>> commandCallbacks = new();
        private readonly Timer commandTimeoutTimer;

        // Variabel status perangkat
        private bool vehicleDetected = false;
        private GateStatus currentGateStatus = GateStatus.Unknown;
        private string lastCommandSent = string.Empty;
        private DateTime lastCommandTime = DateTime.MinValue;

        // Event handlers
        public event EventHandler<VehicleDetectionEventArgs> VehicleDetectionChanged;
        public event EventHandler<GateStatusEventArgs> GateStatusChanged;
        public event EventHandler<ConnectionStatusEventArgs> ConnectionStatusChanged;
        public event EventHandler<LogMessageEventArgs> LogMessage;

        /// <summary>
        /// Status gate
        /// </summary>
        public enum GateStatus
        {
            Unknown,
            Opening,
            Open,
            Closing,
            Closed,
            Error
        }

        /// <summary>
        /// Buat instance baru dari AVRController
        /// </summary>
        /// <param name="control">Control untuk InvokeRequired</param>
        public AVRController(Control control)
        {
            invokeControl = control;
            commandTimeoutTimer = new System.Windows.Forms.Timer();
            commandTimeoutTimer.Tick += (s, e) => {
                if (invokeControl.InvokeRequired)
                    invokeControl.Invoke(new Action(() => CommandTimeoutCallback(s, e)));
                else
                    CommandTimeoutCallback(s, e);
            };
            commandTimeoutTimer.Interval = COMMAND_TIMEOUT;
            commandTimeoutTimer.Enabled = false;
        }

        /// <summary>
        /// Inisialisasi koneksi dengan mikrokontroler
        /// </summary>
        /// <param name="portName">Nama port serial</param>
        /// <param name="baudRate">Baud rate (default 9600)</param>
        /// <returns>True jika berhasil</returns>
        public bool Initialize(string portName, int baudRate = 9600)
        {
            try
            {
                // Buat dan konfigurasi objek SerialPort
                serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudRate,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    Handshake = Handshake.None,
                    ReadTimeout = 500,
                    WriteTimeout = 500,
                    NewLine = "\r\n"
                };

                // Event handler untuk data diterima
                serialPort.DataReceived += SerialPort_DataReceived;

                // Buat watchdog
                watchdog = new SerialWatchdog(
                    serialPort,
                    invokeControl,
                    OnConnectionStatusChanged,
                    UpdateStatus,
                    message => OnLogMessage(message),
                    DEFAULT_TIMEOUT
                );

                isInitialized = true;
                OnLogMessage("Kontroler AVR diinisialisasi pada port " + portName);
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage("Error inisialisasi kontroler AVR: " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Mulai koneksi dengan mikrokontroler
        /// </summary>
        /// <returns>True jika berhasil</returns>
        public bool Connect()
        {
            if (!isInitialized)
            {
                OnLogMessage("Kontroler belum diinisialisasi", LogLevel.Error);
                return false;
            }

            try
            {
                // Mulai watchdog
                watchdog.Start();
                
                // Tunda sejenak untuk pastikan koneksi terbuka
                Thread.Sleep(500);
                
                // Kirim perintah GET_STATUS untuk memeriksa koneksi
                SendCommand("GET_STATUS", response => {
                    ParseStatusResponse(response);
                });
                
                isConnected = true;
                OnLogMessage("Terhubung ke kontroler AVR");
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage("Error koneksi ke kontroler AVR: " + ex.Message, LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Tutup koneksi dengan mikrokontroler
        /// </summary>
        public void Disconnect()
        {
            if (watchdog != null)
            {
                watchdog.Stop();
            }
            
            if (commandTimeoutTimer != null)
            {
                commandTimeoutTimer.Stop();
            }
            
            isConnected = false;
            OnConnectionStatusChanged(false);
            OnLogMessage("Terputus dari kontroler AVR");
        }

        /// <summary>
        /// Perintah untuk membuka gate
        /// </summary>
        /// <returns>True jika perintah berhasil dikirim</returns>
        public bool OpenGate()
        {
            return SendCommand("OPEN_GATE", response => {
                if (response.StartsWith("GATE_STATUS:OPENING"))
                {
                    UpdateGateStatus(GateStatus.Opening);
                }
            });
        }

        /// <summary>
        /// Perintah untuk menutup gate
        /// </summary>
        /// <returns>True jika perintah berhasil dikirim</returns>
        public bool CloseGate()
        {
            return SendCommand("CLOSE_GATE", response => {
                if (response.StartsWith("GATE_STATUS:CLOSING"))
                {
                    UpdateGateStatus(GateStatus.Closing);
                }
                else if (response.StartsWith("GATE_ERROR"))
                {
                    UpdateGateStatus(GateStatus.Error);
                    OnLogMessage("Error menutup gate: " + response, LogLevel.Error);
                }
            });
        }

        /// <summary>
        /// Perintah untuk mendapatkan status
        /// </summary>
        /// <returns>True jika perintah berhasil dikirim</returns>
        public bool GetStatus()
        {
            return SendCommand("GET_STATUS", response => {
                ParseStatusResponse(response);
            });
        }

        /// <summary>
        /// Kirim ping ke mikrokontroler untuk cek koneksi
        /// </summary>
        /// <returns>True jika perintah berhasil dikirim</returns>
        public bool SendPing()
        {
            return SendCommand("PING", response => {
                if (response == "PONG")
                {
                    // Koneksi OK
                    OnConnectionStatusChanged(true);
                }
            });
        }

        /// <summary>
        /// Kirim perintah ke mikrokontroler dengan callback untuk respons
        /// </summary>
        /// <param name="command">Perintah yang akan dikirim</param>
        /// <param name="responseCallback">Callback untuk respons</param>
        /// <returns>True jika perintah berhasil dikirim</returns>
        private bool SendCommand(string command, Action<string> responseCallback = null)
        {
            if (!isConnected || serialPort == null || !serialPort.IsOpen)
            {
                OnLogMessage("Tidak dapat mengirim perintah: tidak terhubung", LogLevel.Error);
                return false;
            }

            try
            {
                // Simpan perintah terakhir
                lastCommandSent = command;
                lastCommandTime = DateTime.Now;
                
                // Tambahkan callback jika ada
                if (responseCallback != null)
                {
                    lock (commandCallbacks)
                    {
                        commandCallbacks[command] = responseCallback;
                    }
                    
                    // Set timeout untuk command
                    commandTimeoutTimer.Start();
                }
                
                // Kirim perintah
                serialPort.WriteLine(command);
                OnLogMessage($"Perintah dikirim: {command}");
                
                return true;
            }
            catch (Exception ex)
            {
                OnLogMessage($"Error mengirim perintah {command}: {ex.Message}", LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Event handler untuk serial port data received
        /// </summary>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Baca data yang tersedia
                string data = serialPort.ReadExisting();
                
                // Notifikasi watchdog bahwa data diterima
                watchdog.DataReceived();
                
                // Proses data
                ProcessReceivedData(data);
            }
            catch (Exception ex)
            {
                OnLogMessage("Error saat menerima data serial: " + ex.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// Proses data yang diterima dari serial port
        /// </summary>
        /// <param name="data">Data yang diterima</param>
        private void ProcessReceivedData(string data)
        {
            // Tambahkan data ke buffer
            receiveBuffer.Append(data);
            
            // Cek apakah ada pesan lengkap
            string bufferStr = receiveBuffer.ToString();
            int lineEndIndex;
            
            // Proses semua pesan yang lengkap
            while ((lineEndIndex = bufferStr.IndexOf('\r')) >= 0 || 
                   (lineEndIndex = bufferStr.IndexOf('\n')) >= 0)
            {
                // Ekstrak pesan
                string message = bufferStr[..lineEndIndex].Trim();
                
                // Hapus pesan dari buffer
                receiveBuffer.Remove(0, lineEndIndex + 1);
                bufferStr = receiveBuffer.ToString();
                
                // Log pesan yang diterima
                OnLogMessage($"Pesan diterima: {message}");
                
                // Proses pesan
                InvokeOnMainThread(() => ProcessMessage(message));
            }
        }

        /// <summary>
        /// Proses pesan dari mikrokontroler
        /// </summary>
        /// <param name="message">Pesan yang diterima</param>
        private void ProcessMessage(string message)
        {
            // Reset command timeout timer
            commandTimeoutTimer.Stop();
            
            // Cek apakah ini respons untuk perintah terakhir
            bool isResponse = false;
            
            lock (commandCallbacks)
            {
                if (commandCallbacks.TryGetValue(lastCommandSent, out var callback))
                {
                    // Ini adalah respons dari perintah kita
                    // Panggil callback
                    commandCallbacks.Remove(lastCommandSent);
                    callback?.Invoke(message);
                    isResponse = true;
                }
            }
            
            // Jika bukan respons, proses sebagai notifikasi
            if (!isResponse)
            {
                if (message.StartsWith("VEHICLE_DETECTED"))
                {
                    UpdateVehicleDetection(true);
                }
                else if (message.StartsWith("NO_VEHICLE"))
                {
                    UpdateVehicleDetection(false);
                }
                else if (message.StartsWith("GATE_STATUS:"))
                {
                    string status = message["GATE_STATUS:".Length..].Trim();
                    switch (status.ToUpper())
                    {
                        case "OPENING":
                            UpdateGateStatus(GateStatus.Opening);
                            break;
                        case "OPEN":
                            UpdateGateStatus(GateStatus.Open);
                            break;
                        case "CLOSING":
                            UpdateGateStatus(GateStatus.Closing);
                            break;
                        case "CLOSED":
                            UpdateGateStatus(GateStatus.Closed);
                            break;
                    }
                }
                else if (message.StartsWith("GATE_ERROR:"))
                {
                    UpdateGateStatus(GateStatus.Error);
                    OnLogMessage("Gate error: " + message, LogLevel.Error);
                }
                else if (message == "PARKING_CONTROLLER_READY")
                {
                    OnLogMessage("Mikrokontroler siap", LogLevel.Info);
                    // Request status setelah controller ready
                    GetStatus();
                }
                else if (message == "CONNECTION_LOST")
                {
                    OnConnectionStatusChanged(false);
                    OnLogMessage("Koneksi hilang menurut mikrokontroler", LogLevel.Warning);
                }
                else if (message == "CONNECTION_RESTORED")
                {
                    OnConnectionStatusChanged(true);
                    OnLogMessage("Koneksi dipulihkan menurut mikrokontroler", LogLevel.Info);
                }
            }
        }

        /// <summary>
        /// Parse respons GET_STATUS
        /// </summary>
        /// <param name="response">Respons dari mikrokontroler</param>
        private void ParseStatusResponse(string response)
        {
            if (response.StartsWith("STATUS:"))
            {
                string statusStr = response["STATUS:".Length..];
                string[] parts = statusStr.Split(',');
                
                foreach (string part in parts)
                {
                    string[] keyVal = part.Split('=');
                    if (keyVal.Length == 2)
                    {
                        string key = keyVal[0].Trim();
                        string value = keyVal[1].Trim();
                        
                        if (key == "VEHICLE")
                        {
                            UpdateVehicleDetection(value == "DETECTED");
                        }
                        else if (key == "GATE")
                        {
                            switch (value.ToUpper())
                            {
                                case "OPENING":
                                    UpdateGateStatus(GateStatus.Opening);
                                    break;
                                case "OPEN":
                                    UpdateGateStatus(GateStatus.Open);
                                    break;
                                case "CLOSING":
                                    UpdateGateStatus(GateStatus.Closing);
                                    break;
                                case "CLOSED":
                                    UpdateGateStatus(GateStatus.Closed);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update status deteksi kendaraan
        /// </summary>
        /// <param name="detected">Apakah kendaraan terdeteksi</param>
        private void UpdateVehicleDetection(bool detected)
        {
            if (vehicleDetected != detected)
            {
                vehicleDetected = detected;
                OnVehicleDetectionChanged(detected);
            }
        }

        /// <summary>
        /// Update status gate
        /// </summary>
        /// <param name="status">Status gate baru</param>
        private void UpdateGateStatus(GateStatus status)
        {
            if (currentGateStatus != status)
            {
                currentGateStatus = status;
                OnGateStatusChanged(status);
            }
        }

        /// <summary>
        /// Callback untuk timeout command
        /// </summary>
        private void CommandTimeoutCallback(object sender, EventArgs e)
        {
            try
            {
                InvokeOnMainThread(() => {
                    OnLogMessage($"Timeout menunggu respons untuk perintah: {lastCommandSent}", LogLevel.Warning);
                    
                    // Clear callback
                    lock (commandCallbacks)
                    {
                        if (commandCallbacks.ContainsKey(lastCommandSent))
                        {
                            commandCallbacks.Remove(lastCommandSent);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                OnLogMessage("Error di CommandTimeoutCallback: " + ex.Message, LogLevel.Error);
            }
        }

        /// <summary>
        /// Jembatan untuk callback ConnectionStatusChanged
        /// </summary>
        private void OnConnectionStatusChanged(bool connected)
        {
            isConnected = connected;
            ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs(connected));
            
            if (connected)
            {
                // Jika koneksi dipulihkan, refresh status
                GetStatus();
            }
        }

        /// <summary>
        /// Jembatan untuk callback VehicleDetectionChanged
        /// </summary>
        private void OnVehicleDetectionChanged(bool detected)
        {
            VehicleDetectionChanged?.Invoke(this, new VehicleDetectionEventArgs(detected));
        }

        /// <summary>
        /// Jembatan untuk callback GateStatusChanged
        /// </summary>
        private void OnGateStatusChanged(GateStatus status)
        {
            GateStatusChanged?.Invoke(this, new GateStatusEventArgs(status));
        }

        /// <summary>
        /// Jembatan untuk LogMessage, dengan level default Info
        /// </summary>
        private void OnLogMessage(string message, LogLevel level = LogLevel.Info)
        {
            LogMessage?.Invoke(this, new LogMessageEventArgs(message, level));
        }

        /// <summary>
        /// Update status UI
        /// </summary>
        private void UpdateStatus(string message, Color color)
        {
            // Implementasi sebagai kebutuhan, mungkin perlu label di form
            OnLogMessage("Status: " + message);
        }

        /// <summary>
        /// Helper untuk menjalankan aksi di thread UI
        /// </summary>
        private void InvokeOnMainThread(Action action)
        {
            if (invokeControl != null && invokeControl.InvokeRequired)
            {
                invokeControl.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }

    /// <summary>
    /// Event args untuk deteksi kendaraan
    /// </summary>
    public class VehicleDetectionEventArgs : EventArgs
    {
        public bool VehicleDetected { get; private set; }

        public VehicleDetectionEventArgs(bool detected)
        {
            VehicleDetected = detected;
        }
    }

    /// <summary>
    /// Event args untuk status gate
    /// </summary>
    public class GateStatusEventArgs : EventArgs
    {
        public AVRController.GateStatus Status { get; private set; }

        public GateStatusEventArgs(AVRController.GateStatus status)
        {
            Status = status;
        }
    }

    /// <summary>
    /// Event args untuk status koneksi
    /// </summary>
    public class ConnectionStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; private set; }

        public ConnectionStatusEventArgs(bool connected)
        {
            IsConnected = connected;
        }
    }

    /// <summary>
    /// Event args untuk log message
    /// </summary>
    public class LogMessageEventArgs : EventArgs
    {
        public string Message { get; private set; }
        public LogLevel Level { get; private set; }

        public LogMessageEventArgs(string message, LogLevel level = LogLevel.Info)
        {
            Message = message;
            Level = level;
        }
    }
}