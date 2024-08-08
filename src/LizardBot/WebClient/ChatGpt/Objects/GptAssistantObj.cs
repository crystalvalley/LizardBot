using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LizardBot.WebClient.ChatGpt.Objects
{
    [ToString]
    public class GptAssistantObj
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("created_at")]
        public required int CreatedAt { get; set; }

        [JsonProperty("name")]
        public string? Name { get; set; }

        [JsonProperty("description")]
        public string? Description { get; set; }

        [JsonProperty("model")]
        public required string Model { get; set; }

        [JsonProperty("instructions")]
        public string? Instructions { get; set; }

        [JsonProperty("tools")]
        public List<object> Tools { get; set; } = [];

        [JsonProperty("tool_resources")]
        public object? ToolResource { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = [];

        [JsonProperty("temperature")]
        public double? Temperature { get; set; }

        [JsonProperty("top_p")]
        public double? TopP { get; set; }

        [JsonProperty("response_format")]
        public object? ResponseFormat { get; set; }
    }
}
