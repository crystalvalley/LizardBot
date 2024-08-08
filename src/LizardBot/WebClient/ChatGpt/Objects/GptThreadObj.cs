using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects
{
    [ToString]
    public class GptThreadObj
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("created_at")]
        public required int CreateAt { get; set; }

        [JsonProperty("tool_resources", NullValueHandling = NullValueHandling.Ignore)]
        public object? ToolResources { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = [];
    }
}
