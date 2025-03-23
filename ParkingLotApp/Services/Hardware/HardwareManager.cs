using System;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using OpenCvSharp;
using System.IO;
using System.Text;
using ParkingLotApp.Services.Interfaces;

namespace ParkingLotApp.Services.Hardware
{
    public class HardwareManager : IDisposable
    {
        private SerialPort? _serialPort;
        private VideoCapture? _camera;
        private readonly Dictionary<string, string> _config;
        private readonly string _imageSavePath;
        private bool _isDisposed;
        private readonly SemaphoreSlim _gateBarrierLock = new(1, 1);
        private readonly ILogger _logger;
        private CancellationTokenSource? _cameraCancellationTokenSource;

        public event EventHandler<string>? BarcodeScanned;
        public event EventHandler<bool>? GateStatusChanged;
        public event EventHandler<byte[]>? ImageCaptured;

        public HardwareManager(ILogger logger)
        {
            _logger = logger;
            try
            {
                _config = LoadConfiguration();
                _imageSavePath = _config.ContainsKey("SavePath") ? _config["SavePath"] : "captured_images";
                
                Directory.CreateDirectory(_imageSavePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hardware configuration error: {ex.Message}");
                Task.Run(() => _logger.LogErrorAsync("Hardware configuration error", ex));
                throw;
            }
        }

        private Dictionary<string, string> LoadConfiguration()
        {
            var config = new Dictionary<string, string>();
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "hardware.ini");
                if (File.Exists(configPath))
                {
                    foreach (var line in File.ReadAllLines(configPath))
                    {
                        var parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            config[parts[0].Trim()] = parts[1].Trim();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load hardware configuration: {ex.Message}");
            }
            return config;
        }

        public async Task InitializeSerialPortAsync()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

                _serialPort = new SerialPort
                {
                    PortName = _config.GetValueOrDefault("SerialPort", "COM1"),
                    BaudRate = int.Parse(_config.GetValueOrDefault("BaudRate", "9600")),
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.ErrorReceived += SerialPort_ErrorReceived;

                await Task.Run(() => _serialPort.Open());
                await _logger.LogInfoAsync("Serial port initialized successfully");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Failed to initialize serial port: {ex.Message}", ex);
                throw;
            }
        }

        public async Task InitializeCameraAsync()
        {
            try
            {
                int cameraIndex = 0;
                if (_config.TryGetValue("CameraIndex", out var indexStr) && int.TryParse(indexStr, out var index))
                {
                    cameraIndex = index;
                }

                _camera = new VideoCapture(cameraIndex);
                if (_camera.IsOpened())
                {
                    _cameraCancellationTokenSource = new CancellationTokenSource();
                    
                    // Configure camera resolution if specified
                    if (_config.TryGetValue("CameraResolution", out var resolution))
                    {
                        var parts = resolution.Split('x');
                        if (parts.Length == 2 && int.TryParse(parts[0], out var width) && int.TryParse(parts[1], out var height))
                        {
                            _camera.Set(VideoCaptureProperties.FrameWidth, width);
                            _camera.Set(VideoCaptureProperties.FrameHeight, height);
                        }
                    }
                    
                    await _logger.LogInfoAsync("Camera initialized successfully");
                }
                else
                {
                    await _logger.LogErrorAsync("Failed to initialize camera", new Exception("Camera could not be opened"));
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to initialize camera", ex);
            }
        }

        private async Task ProcessCameraFramesAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && _camera != null && _camera.IsOpened())
                {
                    using (var frame = new Mat())
                    {
                        if (_camera.Read(frame) && !frame.Empty())
                        {
                            // Process the frame here - e.g., barcode detection
                            
                            // Convert to byte array for event
                            byte[] imageBytes;
                            using (var ms = new MemoryStream())
                            {
                                // Encode to jpg
                                Cv2.ImEncode(".jpg", frame, out byte[] buf);
                                imageBytes = buf;
                            }
                            
                            ImageCaptured?.Invoke(this, imageBytes);
                        }
                        
                        await Task.Delay(33, cancellationToken); // ~30 FPS
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Error processing camera frames", ex);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort == null || !_serialPort.IsOpen) return;

            try
            {
                var data = _serialPort.ReadLine().Trim();
                if (data.StartsWith("BARCODE:"))
                {
                    var barcode = data.Substring(8);
                    BarcodeScanned?.Invoke(this, barcode);
                }
                else if (data.StartsWith("GATE:"))
                {
                    var status = data.Substring(5) == "OPEN";
                    GateStatusChanged?.Invoke(this, status);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading serial data: {ex.Message}");
                Task.Run(async () => await _logger.LogErrorAsync("Error reading serial data", ex));
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            string errorMsg = $"Serial port error: {e.EventType}";
            Task.Run(async () => await _logger.LogErrorAsync(errorMsg));
        }

        public async Task OpenGateBarrierAsync()
        {
            await _gateBarrierLock.WaitAsync();
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes("GATE:OPEN\n"));
                    await _logger.LogInfoAsync("Gate barrier open command sent");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to open gate barrier", ex);
                throw;
            }
            finally
            {
                _gateBarrierLock.Release();
            }
        }

        public async Task CloseGateBarrierAsync()
        {
            await _gateBarrierLock.WaitAsync();
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes("GATE:CLOSE\n"));
                    await _logger.LogInfoAsync("Gate barrier close command sent");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to close gate barrier", ex);
                throw;
            }
            finally
            {
                _gateBarrierLock.Release();
            }
        }

        public async Task<string> CaptureImageAsync()
        {
            try
            {
                if (_camera == null || !_camera.IsOpened())
                {
                    await InitializeCameraAsync();
                    
                    if (_camera == null || !_camera.IsOpened())
                    {
                        throw new InvalidOperationException("Camera not available");
                    }
                    
                    // Give the camera some time to initialize
                    await Task.Delay(500);
                }
                
                using (var frame = new Mat())
                {
                    if (_camera.Read(frame) && !frame.Empty())
                    {
                        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        string filename = $"capture_{timestamp}.jpg";
                        string filePath = Path.Combine(_imageSavePath, filename);
                        
                        frame.SaveImage(filePath);
                        
                        // Convert to byte array for event if needed
                        Cv2.ImEncode(".jpg", frame, out byte[] imageBytes);
                        ImageCaptured?.Invoke(this, imageBytes);
                        
                        return filePath;
                    }
                    else
                    {
                        throw new InvalidOperationException("Failed to capture image");
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Failed to capture image", ex);
                return string.Empty;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _serialPort?.Close();
                    _serialPort?.Dispose();
                    
                    if (_camera != null)
                    {
                        _cameraCancellationTokenSource?.Cancel();
                        _camera.Release();
                        _camera.Dispose();
                    }
                    
                    _cameraCancellationTokenSource?.Dispose();
                    _gateBarrierLock.Dispose();
                }
                
                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
} 