using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects
{
    public class FileObj
    {
        [JsonProperty("id")]
        public required string Id { get; set; }

        [JsonProperty("bytes")]
        public required int Bytes { get; set; }

        [JsonProperty("created_at")]
        public required int CreatedAt { get; set; }

        [JsonProperty("filename")]
        public required string FileName { get; set; }

        [JsonProperty("object")]
        public required string Object { get; set; }

        [JsonProperty("purpose")]
        public required string Purpose { get; set; }
    }
}
