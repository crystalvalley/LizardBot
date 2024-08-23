using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects
{
    public class VectorStoreFileObj
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("usage_bytes")]
        public required int UsageBytes { get; set; }

        [JsonProperty("created_at")]
        public required int CreatedAt { get; set; }

        [JsonProperty("vector_store_id")]
        public required string VectorStoreId { get; set; }

        [JsonProperty("status")]
        public required string Status { get; set; }

        [JsonProperty("last_error")]
        public object? LastError { get; set; }

        [JsonProperty("chunking_strategy")]
        public object? ChunkingStrategy { get; set; }
    }
}
