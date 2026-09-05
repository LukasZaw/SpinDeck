using System.IO.Ports;

namespace SpinDeck_Win_app.Communication
{
    public enum DeviceConnectionState
    {
        Disconnected,
        Connected,
        Ready,
        Syncing,
        Synchronized
    }

    public class SerialManager
    {
        public const int DefaultBaudRate = 115200;
        private SerialPort? _serialPort;
        private readonly object _serialLock = new();
        private string _receiveBuffer = string.Empty;

        public bool IsConnected => _serialPort != null && _serialPort.IsOpen;

        public DeviceConnectionState State { get; private set; } =
            DeviceConnectionState.Disconnected;

        public event Action<string>? MessageReceived;

        public event Action? Connected;

        public event Action? Disconnected;

        public event Action<string>? Error;

        public event Action<DeviceConnectionState>? StateChanged;

        public string[] GetAvailablePorts()
        {
            return SerialPort.GetPortNames();
        }

        public void Connect(string portName, int baudRate = DefaultBaudRate)
        {
            if (IsConnected)
                Disconnect();

            try
            {
                _serialPort = new SerialPort(portName, baudRate);

                _serialPort.NewLine = "\n";
                _serialPort.ReadTimeout = 500;
                _serialPort.WriteTimeout = 500;

                _serialPort.DataReceived += SerialPort_DataReceived;

                _serialPort.Open();
                _receiveBuffer = string.Empty;

                SetState(DeviceConnectionState.Connected);
                Connected?.Invoke();
            }
            catch (Exception ex)
            {
                SetState(DeviceConnectionState.Disconnected);
                Error?.Invoke(ex.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_serialPort != null)
                {
                    _serialPort.DataReceived -= SerialPort_DataReceived;

                    if (_serialPort.IsOpen)
                        _serialPort.Close();

                    _serialPort.Dispose();
                    _serialPort = null;
                }

                SetState(DeviceConnectionState.Disconnected);
                Disconnected?.Invoke();
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.Message);
            }
        }

        public void Send(string message)
        {
            try
            {
                lock (_serialLock)
                {
                    if (!IsConnected)
                    {
                        Error?.Invoke("The device is not connected.");
                        return;
                    }

                    _serialPort!.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.Message);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null || !_serialPort.IsOpen)
                    return;

                _receiveBuffer += _serialPort.ReadExisting();

                int newlineIndex;
                while ((newlineIndex = _receiveBuffer.IndexOf('\n')) >= 0)
                {
                    string message = _receiveBuffer[..newlineIndex].Trim();
                    _receiveBuffer = _receiveBuffer[(newlineIndex + 1)..];

                    if (!string.IsNullOrEmpty(message))
                    {
                        if (message == "DEVICE:READY" ||
                            message == "PONG")
                        {
                            SetState(DeviceConnectionState.Ready);
                        }

                        MessageReceived?.Invoke(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Error?.Invoke(ex.Message);
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        internal void SetSynchronizationState(DeviceConnectionState state)
        {
            if (state != DeviceConnectionState.Syncing &&
                state != DeviceConnectionState.Synchronized &&
                state != DeviceConnectionState.Ready)
            {
                throw new ArgumentOutOfRangeException(nameof(state));
            }

            SetState(state);
        }

        private void SetState(DeviceConnectionState state)
        {
            if (State == state)
                return;

            State = state;
            StateChanged?.Invoke(state);
        }
    }
}