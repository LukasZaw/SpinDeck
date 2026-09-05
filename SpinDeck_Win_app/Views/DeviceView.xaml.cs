using SpinDeck_Win_app.Communication;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SpinDeck_Win_app.Views
{
    public partial class DeviceView : UserControl
    {
        private readonly SerialManager _serialManager;


        public DeviceView(
            SerialManager serialManager)
        {
            InitializeComponent();

            _serialManager =
                serialManager;


            _serialManager.MessageReceived +=
                OnMessageReceived;

            _serialManager.Connected +=
                OnConnected;

            _serialManager.Disconnected +=
                OnDisconnected;

            _serialManager.StateChanged +=
                OnStateChanged;

            _serialManager.Error +=
                OnError;


            RefreshPorts();
        }



        // =====================================================
        // PORTS
        // =====================================================

        private void RefreshPorts()
        {
            PortComboBox.Items.Clear();

            string[] ports =
                _serialManager.GetAvailablePorts();

            foreach (string port in ports)
            {
                PortComboBox.Items.Add(port);
            }

            if (PortComboBox.Items.Count > 0)
            {
                PortComboBox.SelectedIndex = 0;

                AddLog(
                    $"Number of COM Ports found: {ports.Length}"
                );
            }
            else
            {
                AddLog("COM Ports not found.");
            }
        }


        private void RefreshButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            RefreshPorts();
        }


        // =====================================================
        // CONNECTION
        // =====================================================

        private void ConnectButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_serialManager.IsConnected)
            {
                _serialManager.Disconnect();

                return;
            }


            if (PortComboBox.SelectedItem == null)
            {
                MessageBox.Show(
                    "COM Ports not found.\n\n" +
                    "Connect ESP32 and Refresh.",
                    "No devices",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );

                return;
            }


            string port = PortComboBox.SelectedItem.ToString()!;


            _serialManager.Connect(port, 115200);
        }




        // =====================================================
        // CONNECTION EVENTS
        // =====================================================
        private void OnConnected()
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text = "Connected";

                StatusIndicator.Fill =
                    new SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            46,
                            160,
                            67
                        )
                    );

                ConnectButton.Content =
                    "Disconnect";


                PortComboBox.IsEnabled = false;


                AddLog("Connected to ESP32.");


                // Confirm the connection and trigger the ready handshake.
                _serialManager.Send("PING");
            });
        }


        private void OnDisconnected()
        {
            Dispatcher.Invoke(() =>
            {
                StatusText.Text =
                    "Disconnected";

                StatusIndicator.Fill =
                    new SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            153,
                            153,
                            153
                        )
                    );

                ConnectButton.Content =
                    "Connect";


                PortComboBox.IsEnabled = true;


                AddLog("Disconnected from ESP32.");
            });
        }

        private void OnStateChanged(DeviceConnectionState state)
        {
            Dispatcher.Invoke(() =>
            {
                string status = state switch
                {
                    DeviceConnectionState.Connected => "Connected",
                    DeviceConnectionState.Ready => "Ready",
                    DeviceConnectionState.Syncing => "Syncing",
                    DeviceConnectionState.Synchronized => "Synchronized",
                    _ => "Disconnected"
                };

                System.Windows.Media.Color color = state switch
                {
                    DeviceConnectionState.Ready => System.Windows.Media.Color.FromRgb(46, 130, 180),
                    DeviceConnectionState.Syncing => System.Windows.Media.Color.FromRgb(230, 150, 40),
                    DeviceConnectionState.Synchronized => System.Windows.Media.Color.FromRgb(46, 160, 67),
                    DeviceConnectionState.Connected => System.Windows.Media.Color.FromRgb(120, 120, 120),
                    _ => System.Windows.Media.Color.FromRgb(153, 153, 153)
                };

                StatusText.Text = status;
                StatusIndicator.Fill = new SolidColorBrush(color);

                if (state == DeviceConnectionState.Syncing)
                {
                    AddLog("Synchronizing actions with ESP32...");
                }
                else if (state == DeviceConnectionState.Synchronized)
                {
                    AddLog("Actions synchronized with ESP32.");
                }
            });
        }


        private void OnError(string message)
        {
            Dispatcher.Invoke(() =>
            {
                AddLog(
                    $"ERROR: {message}"
                );
            });
        }


        // =====================================================
        // ESP32 MESSAGES
        // =====================================================

        private void OnMessageReceived(
            string message)
        {
            Dispatcher.Invoke(() =>
            {
                AddLog(
                    $"ESP32 → {message}"
                );
            });
        }


        // =====================================================
        // COMMANDS
        // =====================================================

        private void PingButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_serialManager.IsConnected)
            {
                AddLog(
                    "Cannot send PING - no connection."
                );

                return;
            }


            AddLog(
                "PC → PING"
            );


            _serialManager.Send(
                "PING"
            );
        }


        private void ScreenTestButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!_serialManager.IsConnected)
            {
                AddLog(
                    "Cannot start test - no connection."
                );

                return;
            }


            AddLog(
                "PC → SCREEN:TEST"
            );


            _serialManager.Send(
                "SCREEN:TEST"
            );
        }


        // =====================================================
        // LOG
        // =====================================================

        private void AddLog(string message)
        {
            LogTextBox.AppendText(
                $"[{DateTime.Now:HH:mm:ss}] {message}\n"
            );

            LogTextBox.ScrollToEnd();
        }


        // =====================================================
        // CLEANUP
        // =====================================================

        public void Disconnect()
        {
            if (_serialManager.IsConnected)
            {
                _serialManager.Disconnect();
            }
        }
    }
}