using LizardBot.Common.Utils;
using LizardBot.WebClient.ChatGpt.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace LizardBot.WebClient.ChatGpt
{
    /// <summary>
    /// ChatGpt용 RestClient.
    /// </summary>
    public partial class ChatGptRestClient
    {
        /// <summary>
        /// 스레드 생성.
        /// <see href="https://platform.openai.com/docs/api-reference/threads/createThread"/>.
        /// </summary>
        /// <returns>생성된 스레드 반환.</returns>
        public async Task<GptThread?> CreateThreadAsync()
        {
            var request = new RestRequest("/v1/threads");
            request
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Authorization", _client.Config.SecretKey)
                .AddHeader("OpenAI-Beta", "assistants=v2");

            var response = await _client.PostAsync(request);
            ArgumentNullException.ThrowIfNull(response.Content);
            return JsonConvert.DeserializeObject<GptThread>(response.Content);
        }
    }
}
