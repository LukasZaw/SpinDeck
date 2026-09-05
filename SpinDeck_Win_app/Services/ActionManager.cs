using SpinDeck_Win_app.Communication;
using SpinDeck_Win_app.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace SpinDeck_Win_app.Services
{
    public class ActionManager
    {
        private readonly ActionStorage _storage;
        private readonly SerialManager _serialManager;
        private readonly DispatcherTimer _selectionSendTimer;
        private readonly DispatcherTimer _syncTimeoutTimer;
        private readonly Dispatcher _dispatcher;
        private readonly HashSet<int> _pendingActionIndexes = new();
        private bool _clearConfirmed;
        private bool _ignoreNextReadyState;
        private int _expectedSelectedIndex;

        private static readonly TimeSpan SyncTimeout =
            TimeSpan.FromSeconds(5);

        public ObservableCollection<ActionConfiguration> Actions { get; }

        public int CurrentIndex { get; private set; }


        public ActionConfiguration? CurrentAction
        {
            get
            {
                if (Actions.Count == 0)
                    return null;

                return Actions[CurrentIndex];
            }
        }


        public event Action? ActionsChanged;

        public event Action<ActionConfiguration?>? CurrentActionChanged;

        public event Action<string>? Error;


        public ActionManager(
            SerialManager serialManager)
        {
            _serialManager = serialManager;
            _dispatcher = Dispatcher.CurrentDispatcher;

            _serialManager.MessageReceived += OnDeviceMessageReceived;
            _serialManager.StateChanged += OnDeviceStateChanged;

            _selectionSendTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(30)
            };
            _selectionSendTimer.Tick += SelectionSendTimer_Tick;

            _syncTimeoutTimer = new DispatcherTimer
            {
                Interval = SyncTimeout
            };
            _syncTimeoutTimer.Tick += SyncTimeoutTimer_Tick;

            _storage =
                new ActionStorage();

            Actions =
                new ObservableCollection<ActionConfiguration>();

            Load();
        }


        // =====================================================
        // LOAD
        // =====================================================

        public void Load()
        {
            Actions.Clear();

            var savedActions =
                _storage.Load();

            foreach (var action in savedActions)
            {
                Actions.Add(action);
            }

            CurrentIndex = 0;

            ActionsChanged?.Invoke();

            CurrentActionChanged?.Invoke(
                CurrentAction
            );
        }


        // =====================================================
        // SAVE
        // =====================================================

        private void Save()
        {
            _storage.Save(Actions);
        }


        // =====================================================
        // ADD
        // =====================================================

        public void Add(
            ActionConfiguration action)
        {
            int newId = 1;

            if (Actions.Count > 0)
            {
                newId =
                    Actions.Max(x => x.Id) + 1;
            }

            action.Id = newId;

            Actions.Add(action);

            Save();

            ActionsChanged?.Invoke();

            SyncWithDevice();
        }


        // =====================================================
        // EDIT
        // =====================================================

        public void Edit(
            ActionConfiguration action)
        {
            Save();

            ActionsChanged?.Invoke();

            SyncWithDevice();
        }


        // =====================================================
        // DELETE
        // =====================================================

        public void Remove(
            ActionConfiguration action)
        {
            int removedIndex =
                Actions.IndexOf(action);

            if (removedIndex < 0)
                return;

            Actions.Remove(action);


            if (Actions.Count == 0)
            {
                CurrentIndex = 0;
            }
            else if (CurrentIndex >= Actions.Count)
            {
                CurrentIndex =
                    Actions.Count - 1;
            }
            else if (removedIndex < CurrentIndex)
            {
                CurrentIndex--;
            }


            Save();

            ActionsChanged?.Invoke();

            CurrentActionChanged?.Invoke(
                CurrentAction
            );

            SyncWithDevice();
        }


        // =====================================================
        // NEXT
        // =====================================================

        public void Next()
        {
            if (Actions.Count == 0)
                return;


            CurrentIndex++;


            if (CurrentIndex >= Actions.Count)
            {
                CurrentIndex = 0;
            }


            CurrentActionChanged?.Invoke(
                CurrentAction
            );


            ScheduleCurrentActionSend();
        }


        // =====================================================
        // PREVIOUS
        // =====================================================

        public void Previous()
        {
            if (Actions.Count == 0)
                return;


            CurrentIndex--;


            if (CurrentIndex < 0)
            {
                CurrentIndex =
                    Actions.Count - 1;
            }


            CurrentActionChanged?.Invoke(
                CurrentAction
            );


            ScheduleCurrentActionSend();
        }


        // =====================================================
        // SYNC
        // =====================================================

        public void SyncWithDevice()
        {
            if (!_serialManager.IsConnected ||
                (_serialManager.State != DeviceConnectionState.Ready &&
                 _serialManager.State != DeviceConnectionState.Synchronized))
                return;

            StartSynchronization();
        }

        private void StartSynchronization()
        {
            _selectionSendTimer.Stop();

            _syncTimeoutTimer.Stop();

            _clearConfirmed = false;
            _pendingActionIndexes.Clear();

            for (int i = 0; i < Actions.Count; i++)
            {
                _pendingActionIndexes.Add(i);
            }

            _expectedSelectedIndex =
                Actions.Count == 0 ? -1 : CurrentIndex;

            _serialManager.SetSynchronizationState(
                DeviceConnectionState.Syncing);

            _syncTimeoutTimer.Start();


            // Clear previous action
            _serialManager.Send("ACTION:CLEAR");


            for (int i = 0; i < Actions.Count; i++)
            {
                SendAction(i, Actions[i]);
            }


            SendCurrentAction();
        }


        // =====================================================
        // SEND ACTION
        // =====================================================

        private void SendAction(
            int index,
            ActionConfiguration action)
        {
            string type =
                action.Type == ActionType.Browser
                    ? "Browser"
                    : "Application";


            string message =
                $"ACTION:SET|{index}|{action.Name}|{type}|{action.Value}";


            _serialManager.Send(
                message
            );
        }


        // =====================================================
        // SEND CURRENT
        // =====================================================

        private void SendCurrentAction()
        {
            if (!_serialManager.IsConnected ||
                (_serialManager.State != DeviceConnectionState.Syncing &&
                 _serialManager.State != DeviceConnectionState.Synchronized))
                return;


            if (Actions.Count == 0)
            {
                _serialManager.Send(
                    "ACTION:SELECT|-1"
                );

                return;
            }


            _serialManager.Send(
                $"ACTION:SELECT|{CurrentIndex}"
            );
        }

        private void ScheduleCurrentActionSend()
        {
            _selectionSendTimer.Stop();
            _selectionSendTimer.Start();
        }

        private void SelectionSendTimer_Tick(object? sender, EventArgs e)
        {
            _selectionSendTimer.Stop();
            SendCurrentAction();
        }

        private void OnDeviceStateChanged(DeviceConnectionState state)
        {
            _dispatcher.BeginInvoke(() =>
            {
                if (state == DeviceConnectionState.Ready)
                {
                    if (_ignoreNextReadyState)
                    {
                        _ignoreNextReadyState = false;
                        return;
                    }

                    SyncWithDevice();
                }
                else if (state == DeviceConnectionState.Disconnected)
                {
                    _selectionSendTimer.Stop();
                    _syncTimeoutTimer.Stop();
                }
            });
        }

        private void OnDeviceMessageReceived(string message)
        {
            _dispatcher.BeginInvoke(() => HandleDeviceMessage(message));
        }

        private void HandleDeviceMessage(string message)
        {
            if (_serialManager.State != DeviceConnectionState.Syncing)
                return;

            if (message == "OK:ACTION:CLEAR")
            {
                _clearConfirmed = true;
            }
            else if (message.StartsWith("OK:ACTION:SET|", StringComparison.Ordinal))
            {
                string indexText = message["OK:ACTION:SET|".Length..];

                if (int.TryParse(indexText, out int index))
                {
                    _pendingActionIndexes.Remove(index);
                }
            }
            else if (message.StartsWith("OK:ACTION:SELECT|", StringComparison.Ordinal))
            {
                string indexText = message["OK:ACTION:SELECT|".Length..];

                if (int.TryParse(indexText, out int index) &&
                    index == _expectedSelectedIndex)
                {
                    TryCompleteSynchronization();
                }
            }
            else if (message.StartsWith("ERROR:", StringComparison.Ordinal))
            {
                FailSynchronization(
                    $"ESP32 rejected synchronization: {message}");
            }
        }

        private void TryCompleteSynchronization()
        {
            if (!_clearConfirmed || _pendingActionIndexes.Count > 0)
                return;

            _syncTimeoutTimer.Stop();
            _serialManager.SetSynchronizationState(
                DeviceConnectionState.Synchronized);
        }

        private void SyncTimeoutTimer_Tick(object? sender, EventArgs e)
        {
            _syncTimeoutTimer.Stop();

            if (_serialManager.State == DeviceConnectionState.Syncing)
            {
                FailSynchronization(
                    "Synchronization with ESP32 timed out.");
            }
        }

        private void FailSynchronization(string message)
        {
            _syncTimeoutTimer.Stop();

            if (_serialManager.IsConnected)
            {
                _ignoreNextReadyState = true;
                _serialManager.SetSynchronizationState(
                    DeviceConnectionState.Ready);
            }

            Error?.Invoke(message);
        }
    }
}