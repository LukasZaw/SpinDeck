using SpinDeck_Win_app.Communication;
using SpinDeck_Win_app.Services;
using SpinDeck_Win_app.Views;
using System.ComponentModel;
using System.Windows;
using Forms = System.Windows.Forms;

namespace SpinDeck_Win_app
{
    public partial class MainWindow : Window
    {
        private readonly SerialManager _serialManager;
        private readonly ActionManager _actionManager;
        private readonly ActionExecutor _actionExecutor;
        private readonly SettingsManager _settingsManager;

        private readonly DeviceView _deviceView;
        private readonly ActionsView _actionsView;
        private readonly SettingsView _settingsView;
        private readonly Forms.NotifyIcon _notifyIcon;
        private bool _allowClose;

        // CONSTRUCTOR
        public MainWindow()
        {
            InitializeComponent();

            // COMMUNICATION
            _serialManager = new SerialManager();


            // ACTION MANAGER
            _actionManager = new ActionManager(_serialManager);
            _actionExecutor = new ActionExecutor();

            _settingsManager = new SettingsManager();
            _settingsManager.SettingsChanged += OnSettingsChanged;

            _notifyIcon = CreateNotifyIcon();

            // VIEWS
            _deviceView = new DeviceView(_serialManager);

            _actionsView = new ActionsView(_actionManager);

            _settingsView = new SettingsView(_serialManager, _settingsManager);



            // ESP32 EVENTS
            _serialManager.MessageReceived +=
                OnSerialMessageReceived;

            _serialManager.Connected +=
                OnSerialConnected;

            _actionManager.Error +=
                OnActionManagerError;

            if (_settingsManager.Settings.AutoConnect && !string.IsNullOrWhiteSpace(_settingsManager.Settings.DefaultPort))
            {
                _serialManager.Connect(_settingsManager.Settings.DefaultPort);
            }

            // START VIEW
            ShowDeviceView();

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= MainWindow_Loaded;
            ApplyWindowSettings(_settingsManager.Settings);
        }

        private Forms.NotifyIcon CreateNotifyIcon()
        {
            var showMenuItem = new Forms.ToolStripMenuItem("Show SpinDeck");
            showMenuItem.Click += (_, _) => ShowFromTray();

            var exitMenuItem = new Forms.ToolStripMenuItem("Exit");
            exitMenuItem.Click += (_, _) => ExitApplication();

            var contextMenu = new Forms.ContextMenuStrip();
            contextMenu.Items.Add(showMenuItem);
            contextMenu.Items.Add(new Forms.ToolStripSeparator());
            contextMenu.Items.Add(exitMenuItem);

            var notifyIcon = new Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(
                    Environment.ProcessPath ?? string.Empty)
                    ?? System.Drawing.SystemIcons.Application,
                Text = "SpinDeck",
                ContextMenuStrip = contextMenu,
                Visible = _settingsManager.Settings.MinimizeToTray
            };

            notifyIcon.DoubleClick += (_, _) => ShowFromTray();
            return notifyIcon;
        }

        private void OnSettingsChanged(AppSettings settings)
        {
            Dispatcher.BeginInvoke(() => ApplyWindowSettings(settings));
        }

        private void ApplyWindowSettings(AppSettings settings)
        {
            _notifyIcon.Visible = settings.MinimizeToTray;

            if (settings.StartMinimized)
            {
                WindowState = WindowState.Minimized;
            }

            if (settings.StartMinimized && settings.MinimizeToTray)
            {
                HideToTray();
            }
        }

        private void ShowFromTray()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }

        private void HideToTray()
        {
            Hide();
        }

        private void ExitApplication()
        {
            _allowClose = true;
            _notifyIcon.Visible = false;
            Close();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized &&
                _settingsManager.Settings.MinimizeToTray)
            {
                HideToTray();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_allowClose && _settingsManager.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                HideToTray();
                return;
            }

            base.OnClosing(e);
        }



        // SERIAL MESSAGE
        private void OnSerialMessageReceived(
            string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                switch (message)
                {
                    case "EVENT:ENCODER_RIGHT":

                        _actionManager.Next();
                        break;

                    case "EVENT:ENCODER_LEFT":

                        _actionManager.Previous();
                        break;


                    case "EVENT:BUTTON_CLICK":

                        ActionExecutionResult result =
                            _actionExecutor.Execute(_actionManager.CurrentAction);

                        if (!result.Succeeded)
                        {
                            System.Windows.MessageBox.Show(
                                result.ErrorMessage,
                                "Action could not be executed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }

                        break;


                    case "EVENT:BUTTON_LONG_PRESS":

                        // Rezerwa na przyszłe funkcje.

                        break;
                }
            });
        }



        // ESP32 CONNECTED
        private void OnSerialConnected()
        {
            Dispatcher.Invoke(() =>
            {
                _actionManager.SyncWithDevice();
            });
        }

        private void OnActionManagerError(string message)
        {
            Dispatcher.BeginInvoke(() =>
            {
                System.Windows.MessageBox.Show(
                    message,
                    "ESP32 communication error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            });
        }


        // DEVICE VIEW
        private void ShowDeviceView()
        {
            MainContent.Content = _deviceView;
            SetActiveNavigationButton(DeviceButton);
        }


        // ACTIONS VIEW
        private void ShowActionsView()
        {
            MainContent.Content = _actionsView;
            SetActiveNavigationButton(ActionsButton);
        }


        // SETTINGS VIEW
        private void ShowSettingsView()
        {
            MainContent.Content = _settingsView;
            SetActiveNavigationButton(SettingsButton);
        }

        private void SetActiveNavigationButton(System.Windows.Controls.Button activeButton)
        {
            DeviceButton.ClearValue(BackgroundProperty);
            ActionsButton.ClearValue(BackgroundProperty);
            SettingsButton.ClearValue(BackgroundProperty);

            activeButton.Background =
                (System.Windows.Media.Brush)FindResource("SidebarHoverBrush");
        }



        // LEFT MENU
        private void DeviceButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDeviceView();
        }


        private void ActionsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowActionsView();
        }


        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowSettingsView();
        }



        // WINDOW CLOSE
        protected override void OnClosed(EventArgs e)
        {
            _serialManager.MessageReceived -=
                OnSerialMessageReceived;

            _serialManager.Connected -=
                OnSerialConnected;

            _actionManager.Error -=
                OnActionManagerError;

            _settingsManager.SettingsChanged -=
                OnSettingsChanged;

            _serialManager.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();

            base.OnClosed(e);
        }
    }
}