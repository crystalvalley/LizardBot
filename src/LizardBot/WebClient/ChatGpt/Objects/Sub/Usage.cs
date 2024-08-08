using Newtonsoft.Json;

namespace LizardBot.WebClient.ChatGpt.Objects.Sub
{
    public class Usage
    {
        [JsonProperty("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonProperty("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonProperty("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
