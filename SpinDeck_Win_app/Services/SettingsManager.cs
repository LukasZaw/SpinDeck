using System.IO;
using Microsoft.Win32;
using System.Text.Json;

namespace SpinDeck_Win_app.Services
{
    public class AppSettings
    {
        public string DeviceName { get; set; } = "SpinDeck";

        public string DefaultPort { get; set; } = "";

        public bool AutoConnect { get; set; } = true;

        public bool AutoStartApplication { get; set; } = false;

        public bool StartMinimized { get; set; } = false;

        public bool MinimizeToTray { get; set; } = true;
    }


    public class SettingsManager
    {
        private readonly string _settingsDirectory;
        private readonly string _settingsFile;

        public AppSettings Settings { get; private set; }

        public event Action<AppSettings>? SettingsChanged;

        private const string StartupRegistryKey =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        private const string StartupValueName = "SpinDeck";

        public SettingsManager()
        {
            _settingsDirectory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "SpinDeck");

            _settingsFile = Path.Combine(
                _settingsDirectory,
                "settings.json");

            Settings = Load();
        }


        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_settingsFile))
                {
                    return new AppSettings();
                }

                string json = File.ReadAllText(_settingsFile);

                return JsonSerializer.Deserialize<AppSettings>(json)
                       ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }


        public void Save(AppSettings settings)
        {
            Settings = settings;

            Directory.CreateDirectory(_settingsDirectory);

            string json = JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

            File.WriteAllText(
                _settingsFile,
                json);

            UpdateStartupRegistration(settings.AutoStartApplication);
            SettingsChanged?.Invoke(Settings);
        }

        private static void UpdateStartupRegistration(bool enabled)
        {
            using RegistryKey? key =
                Registry.CurrentUser.CreateSubKey(StartupRegistryKey);

            if (key == null)
            {
                throw new InvalidOperationException(
                    "Windows startup settings could not be opened.");
            }

            if (enabled)
            {
                string executablePath =
                    Environment.ProcessPath
                    ?? throw new InvalidOperationException(
                        "The application path for Windows startup could not be determined.");

                key.SetValue(
                    StartupValueName,
                    $"\"{executablePath}\"");
            }
            else
            {
                key.DeleteValue(StartupValueName, false);
            }
        }
    }
}