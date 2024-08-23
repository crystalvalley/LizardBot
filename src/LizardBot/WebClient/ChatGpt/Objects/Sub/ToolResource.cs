using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects.Sub
{
    public class ToolResource
    {
        [JsonProperty("file_search")]
        public FileSearch? FileSearch { get; set; }
    }

    public class FileSearch
    {
        [JsonProperty("vector_store_ids")]
        public List<string> VectorStoreIds { get; set; } = [];
    }
}
