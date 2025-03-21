using System;
using System.Drawing;
using System.IO.Ports;
using System.Windows.Forms;

namespace ParkingIN
{
    /// <summary>
    /// Kelas untuk memantau koneksi serial dan mendeteksi ketika mikrokontroler terputus
    /// </summary>
    public class SerialWatchdog
    {
        // Constants
        private const int DEFAULT_WATCHDOG_TIMEOUT_MS = 5000; // 5 detik timeout default
        private const int PING_INTERVAL_MS = 2000;            // 2 detik interval ping
        private const int RECONNECT_ATTEMPTS = 3;             // Jumlah percobaan reconnect
        private const int RECONNECT_DELAY_MS = 1000;          // Delay antar percobaan reconnect

        // Variabel instance
        private readonly SerialPort serialPort;
        private readonly System.Windows.Forms.Timer watchdogTimer;
        private readonly System.Windows.Forms.Timer pingTimer;
        private readonly Control invokeControl;
        private readonly int watchdogTimeoutMs;
        private readonly Action<bool> connectionStatusChanged;
        private readonly Action<string, Color> updateStatusCallback;
        private readonly Action<string> logMessageCallback;

        private DateTime lastDataTime;
        private bool isConnected;
        private int reconnectAttempts;

        /// <summary>
        /// Membuat instance SerialWatchdog baru
        /// </summary>
        /// <param name="port">SerialPort yang akan dipantau</param>
        /// <param name="invokeControl">Control untuk InvokeRequired</param>
        /// <param name="connectionStatusChanged">Callback saat status koneksi berubah</param>
        /// <param name="updateStatusCallback">Callback untuk update status UI</param>
        /// <param name="logMessageCallback">Callback untuk mencatat pesan log</param>
        /// <param name="timeoutMs">Timeout dalam milidetik</param>
        public SerialWatchdog(
            SerialPort port,
            Control invokeControl,
            Action<bool> connectionStatusChanged,
            Action<string, Color> updateStatusCallback,
            Action<string> logMessageCallback,
            int timeoutMs = DEFAULT_WATCHDOG_TIMEOUT_MS)
        {
            this.serialPort = port ?? throw new ArgumentNullException(nameof(port));
            this.invokeControl = invokeControl ?? throw new ArgumentNullException(nameof(invokeControl));
            this.connectionStatusChanged = connectionStatusChanged ?? throw new ArgumentNullException(nameof(connectionStatusChanged));
            this.updateStatusCallback = updateStatusCallback ?? throw new ArgumentNullException(nameof(updateStatusCallback));
            this.logMessageCallback = logMessageCallback ?? throw new ArgumentNullException(nameof(logMessageCallback));
            this.watchdogTimeoutMs = timeoutMs;

            watchdogTimer = new() { Interval = watchdogTimeoutMs };
            watchdogTimer.Tick += (s, e) => WatchdogCallback();

            pingTimer = new() { Interval = PING_INTERVAL_MS };
            pingTimer.Tick += (s, e) => PingCallback();

            lastDataTime = DateTime.Now;
            isConnected = false;
            reconnectAttempts = 0;
        }

        /// <summary>
        /// Memulai watchdog
        /// </summary>
        public void Start()
        {
            if (!serialPort.IsOpen)
            {
                try
                {
                    serialPort.Open();
                    LogMessage("Serial port opened successfully", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    LogMessage($"Failed to open serial port: {ex.Message}", LogLevel.Error);
                    return;
                }
            }

            watchdogTimer.Start();
            pingTimer.Start();
            UpdateConnectionStatus(true);
        }

        /// <summary>
        /// Menghentikan watchdog
        /// </summary>
        public void Stop()
        {
            watchdogTimer.Stop();
            pingTimer.Stop();

            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                    LogMessage("Serial port closed", LogLevel.Info);
                }
                catch (Exception ex)
                {
                    LogMessage($"Error closing serial port: {ex.Message}", LogLevel.Error);
                }
            }

            UpdateConnectionStatus(false);
        }

        /// <summary>
        /// Dipanggil ketika data diterima dari mikrokontroler
        /// </summary>
        public void DataReceived()
        {
            // Update waktu terakhir data diterima
            lastDataTime = DateTime.Now;
            
            // Jika sebelumnya terputus, tandai sebagai terhubung kembali
            if (!isConnected)
            {
                isConnected = true;
                reconnectAttempts = 0;
                UpdateConnectionStatus(true);
                UpdateStatus("Mikrokontroler terhubung kembali", Color.Green);
                LogMessage("Koneksi mikrokontroler dipulihkan");
            }
            
            // Reset watchdog timer
            if (watchdogTimer != null)
            {
                watchdogTimer.Stop();
                watchdogTimer.Interval = watchdogTimeoutMs;
                watchdogTimer.Start();
            }
        }

        /// <summary>
        /// Callback untuk watchdog timer
        /// </summary>
        private void WatchdogCallback()
        {
            if ((DateTime.Now - lastDataTime).TotalMilliseconds > watchdogTimeoutMs)
            {
                LogMessage("Watchdog timeout - no data received", LogLevel.Warning);
                TryReconnect();
            }
        }

        /// <summary>
        /// Callback untuk ping timer
        /// </summary>
        private void PingCallback()
        {
            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Write("PING\r\n");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error sending ping: {ex.Message}", LogLevel.Error);
                    TryReconnect();
                }
            }
        }

        /// <summary>
        /// Mencoba menyambungkan kembali ke mikrokontroler
        /// </summary>
        private void TryReconnect()
        {
            if (reconnectAttempts >= RECONNECT_ATTEMPTS)
            {
                LogMessage("Max reconnection attempts reached", LogLevel.Error);
                Stop();
                return;
            }

            reconnectAttempts++;
            LogMessage($"Attempting to reconnect (attempt {reconnectAttempts}/{RECONNECT_ATTEMPTS})", LogLevel.Warning);

            if (serialPort.IsOpen)
            {
                try
                {
                    serialPort.Close();
                }
                catch (Exception ex)
                {
                    LogMessage($"Error closing port for reconnect: {ex.Message}", LogLevel.Error);
                }
            }

            System.Threading.Thread.Sleep(RECONNECT_DELAY_MS);

            try
            {
                serialPort.Open();
                lastDataTime = DateTime.Now;
                reconnectAttempts = 0;
                UpdateConnectionStatus(true);
                LogMessage("Reconnection successful", LogLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"Reconnection failed: {ex.Message}", LogLevel.Error);
                TryReconnect();
            }
        }

        /// <summary>
        /// Update callback status koneksi dengan thread safety
        /// </summary>
        private void UpdateConnectionStatus(bool connected)
        {
            if (isConnected != connected)
            {
                isConnected = connected;
                invokeControl.BeginInvoke(new Action(() => connectionStatusChanged(connected)));
            }
        }

        /// <summary>
        /// Update status UI dengan thread safety
        /// </summary>
        private void UpdateStatus(string message, Color color)
        {
            invokeControl.BeginInvoke(new Action(() => updateStatusCallback(message, color)));
        }

        /// <summary>
        /// Log pesan dengan thread safety
        /// </summary>
        private void LogMessage(string message, LogLevel level = LogLevel.Info)
        {
            invokeControl.BeginInvoke(new Action(() => logMessageCallback($"[SerialWatchdog] {message}")));
        }
    }

    /// <summary>
    /// Level log yang didukung
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }
}