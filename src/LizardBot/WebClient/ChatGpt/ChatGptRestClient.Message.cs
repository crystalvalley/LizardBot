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

        public async Task<GptMessageObj?> CreateMessageAsync(string threadId, string message)
        {
            var request = new RestRequest($"/v1/threads/{threadId}/messages");
            request
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Authorization", _client.Config.SecretKey)
                .AddHeader("OpenAI-Beta", "assistants=v2");

            var body = new Dictionary<string, string>
            {
                { "role", "user" },
                { "content", message },
            };

            request.AddBody(body);
            var response = await _client.PostAsync(request);
            ArgumentNullException.ThrowIfNull(response.Content);
            return JsonConvert.DeserializeObject<GptMessageObj>(response.Content);
        }
    }
}
