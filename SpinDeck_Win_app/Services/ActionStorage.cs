using SpinDeck_Win_app.Models;
using System.IO;
using System.Text.Json;

namespace SpinDeck_Win_app.Services
{
    public class ActionStorage
    {
        private readonly string _directoryPath;
        private readonly string _filePath;

        public ActionStorage()
        {
            _directoryPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData),
                "SpinDeck"
            );

            _filePath = Path.Combine(
                _directoryPath,
                "actions.json"
            );
        }


        // =====================================================
        // LOAD
        // =====================================================

        public List<ActionConfiguration> Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new List<ActionConfiguration>();
                }

                string json =
                    File.ReadAllText(_filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<ActionConfiguration>();
                }

                var actions =
                    JsonSerializer.Deserialize<
                        List<ActionConfiguration>>(json);

                return actions ??
                       new List<ActionConfiguration>();
            }
            catch
            {
                return new List<ActionConfiguration>();
            }
        }


        // =====================================================
        // SAVE
        // =====================================================

        public void Save(
            IEnumerable<ActionConfiguration> actions)
        {
            Directory.CreateDirectory(
                _directoryPath
            );

            var options =
                new JsonSerializerOptions
                {
                    WriteIndented = true
                };

            string json =
                JsonSerializer.Serialize(
                    actions,
                    options
                );

            File.WriteAllText(
                _filePath,
                json
            );
        }
    }
}