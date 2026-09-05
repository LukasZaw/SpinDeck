using System.Text.Json.Serialization;

namespace SpinDeck_Win_app.Models
{
    public class ActionConfiguration
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public ActionType Type { get; set; }

        public string Value { get; set; } = "";

        [JsonIgnore]
        public string TypeDisplayName
        {
            get
            {
                return Type switch
                {
                    ActionType.Browser => "Website",
                    ActionType.Application => "Application",
                    _ => "Unknown"
                };
            }
        }
    }


    public enum ActionType
    {
        Browser,
        Application
    }
}