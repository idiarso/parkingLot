using System;
using System.IO.Ports;
using System.Threading.Tasks;

namespace IUTVehicleManager.Services
{
    public class GateControlService
    {
        private SerialPort? gatePort;
        private bool isGateOpen = false;
        private const string OPEN_GATE_COMMAND = "OPEN";
        private const string CLOSE_GATE_COMMAND = "CLOSE";
        private const int BAUD_RATE = 9600;

        public event EventHandler<bool> GateStatusChanged;

        public GateControlService()
        {
            InitializeGatePort();
        }

        private void InitializeGatePort()
        {
            try
            {
                // Get available ports
                string[] ports = SerialPort.GetPortNames();
                if (ports.Length == 0)
                {
                    throw new Exception("No serial ports available");
                }

                // Try to connect to the first available port
                gatePort = new SerialPort(ports[0], BAUD_RATE);
                gatePort.Open();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to initialize gate control: {ex.Message}");
            }
        }

        public async Task OpenGateAsync()
        {
            if (isGateOpen) return;

            try
            {
                if (gatePort == null || !gatePort.IsOpen)
                {
                    InitializeGatePort();
                }

                // Send open command
                await Task.Run(() =>
                {
                    gatePort?.Write(OPEN_GATE_COMMAND);
                    System.Threading.Thread.Sleep(1000); // Wait for command to be processed
                });

                isGateOpen = true;
                GateStatusChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to open gate: {ex.Message}");
            }
        }

        public async Task CloseGateAsync()
        {
            if (!isGateOpen) return;

            try
            {
                if (gatePort == null || !gatePort.IsOpen)
                {
                    InitializeGatePort();
                }

                // Send close command
                await Task.Run(() =>
                {
                    gatePort?.Write(CLOSE_GATE_COMMAND);
                    System.Threading.Thread.Sleep(1000); // Wait for command to be processed
                });

                isGateOpen = false;
                GateStatusChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to close gate: {ex.Message}");
            }
        }

        public bool IsGateOpen()
        {
            return isGateOpen;
        }

        public void Dispose()
        {
            if (gatePort != null && gatePort.IsOpen)
            {
                gatePort.Close();
                gatePort.Dispose();
            }
        }
    }
} 