using SpinDeck_Win_app.Communication;
using SpinDeck_Win_app.Services;
using System.Windows;
using System.Windows.Controls;

namespace SpinDeck_Win_app.Views
{
    public partial class SettingsView : UserControl
    {
        private readonly SerialManager _serialManager;
        private readonly SettingsManager _settingsManager;


        public SettingsView(
            SerialManager serialManager,
            SettingsManager settingsManager)
        {
            InitializeComponent();

            _serialManager = serialManager;
            _settingsManager = settingsManager;

            LoadSettings();
            RefreshPorts();
        }


        private void LoadSettings()
        {
            AppSettings settings = _settingsManager.Settings;

            DeviceNameTextBox.Text =
                settings.DeviceName;

            AutoConnectCheckBox.IsChecked =
                settings.AutoConnect;

            AutoStartCheckBox.IsChecked =
                settings.AutoStartApplication;

            StartMinimizedCheckBox.IsChecked =
                settings.StartMinimized;

            MinimizeToTrayCheckBox.IsChecked =
                settings.MinimizeToTray;
        }


        private void RefreshPorts()
        {
            DefaultPortComboBox.Items.Clear();

            string[] ports =
                _serialManager.GetAvailablePorts();

            foreach (string port in ports)
            {
                DefaultPortComboBox.Items.Add(port);
            }


            string savedPort =
                _settingsManager.Settings.DefaultPort;

            if (!string.IsNullOrWhiteSpace(savedPort))
            {
                if (!DefaultPortComboBox.Items.Contains(savedPort))
                {
                    DefaultPortComboBox.Items.Add(savedPort);
                }

                DefaultPortComboBox.SelectedItem = savedPort;
            }

            if (DefaultPortComboBox.SelectedItem == null &&
                DefaultPortComboBox.Items.Count > 0)
            {
                DefaultPortComboBox.SelectedIndex = 0;
            }
        }


        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            string deviceName =
                DeviceNameTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(deviceName))
            {
                MessageBox.Show(
                    "Device name cannot be empty.",
                    "Invalid settings",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }


            string defaultPort =
                DefaultPortComboBox.SelectedItem?
                    .ToString() ?? "";


            AppSettings settings = new AppSettings
            {
                DeviceName = deviceName,

                DefaultPort = defaultPort,

                AutoConnect =
                    AutoConnectCheckBox.IsChecked == true,

                AutoStartApplication =
                    AutoStartCheckBox.IsChecked == true,

                StartMinimized =
                    StartMinimizedCheckBox.IsChecked == true,

                MinimizeToTray =
                    MinimizeToTrayCheckBox.IsChecked == true
            };


            try
            {
                _settingsManager.Save(settings);

                if (settings.StartMinimized && settings.MinimizeToTray)
                {
                    MessageBox.Show(
                        "SpinDeck will start hidden and remain available in the system tray.",
                        "SpinDeck",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Settings saved successfully.",
                        "SpinDeck",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Failed to save settings:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}