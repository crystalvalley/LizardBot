using LizardBot.WebClient.ChatGpt.Objects.Sub;
using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects
{
    public class GptRunObj
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("created_at")]
        public int CreateAt { get; set; }

        [JsonProperty("assistant_id")]
        public required string AssistantId { get; set; }

        [JsonProperty("thread_id")]
        public required string ThreadId { get; set; }

        [JsonProperty("status")]
        public required string Status { get; set; }

        [JsonProperty("started_at")]
        public int? StartedAt { get; set; }

        [JsonProperty("expires_at")]
        public int? ExpiresAt { get; set; }

        [JsonProperty("cancelled_at")]
        public int? CanceledAt { get; set; }

        [JsonProperty("failed_at")]
        public int? FailedAt { get; set; }

        [JsonProperty("completed_at")]
        public int? CompletedAt { get; set; }

        [JsonProperty("required_action")]
        public object? RequiredAction { get; set; }

        [JsonProperty("last_error")]
        public object? LastError { get; set; }

        [JsonProperty("model")]
        public required string Model { get; set; }

        [JsonProperty("instructions")]
        public required string Instructions { get; set; }

        [JsonProperty("tools")]
        public List<object> Tools { get; set; } = [];

        [JsonProperty("tool_resources")]
        public required object ToolResources { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = [];

        [JsonProperty("temperature")]
        public double? Temperature { get; set; }

        [JsonProperty("top_p")]
        public double? TopP { get; set; }

        [JsonProperty("max_completion_tokens")]
        public int? MaxCompletionTokens { get; set; }

        [JsonProperty("max_prompt_tokens")]
        public int? MaxPromptTokens { get; set; }

        [JsonProperty("truncation_strategy")]
        public Dictionary<string, object> TruncationStrategy { get; set; } = [];

        [JsonProperty("incomplete_details")]
        public object? IncompleteDetails { get; set; }

        [JsonProperty("usage")]
        public Usage? Usage { get; set; }

        [JsonProperty("tool_choice")]
        public required object ToolChoice { get; set; }

        [JsonProperty("parallel_tool_calls")]
        public bool ParallelToolCalls { get; set; }

        [JsonProperty("response_format")]
        public object? ResponseFormat { get; set; }
    }
}
