using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects
{
    [ToString]
    public class GptMessageObj
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("created_at")]
        public int CreateAt { get; set; }

        [JsonProperty("thread_id")]
        public required string GptThreadId { get; set; }

        [JsonProperty("status")]
        public required string Status { get; set; }

        [JsonProperty("incomplete_details")]
        public object? IncompleteDetails { get; set; }

        [JsonProperty("completed_at")]
        public int? CompletedAt { get; set; }

        [JsonProperty("incomplete_at")]
        public int? IncompleteAt { get; set; }

        [JsonProperty("role")]
        public required string Role { get; set; }

        [JsonProperty("content")]
        public List<object> Content { get; set; } = [];

        [JsonProperty("assistant_id")]
        public string? AssistantId { get; set; }

        [JsonProperty("attachments")]
        public List<object> Attachments { get; set; } = [];

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = [];
    }
}
