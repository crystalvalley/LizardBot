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
        private readonly string _messageCompleted = "event: thread.message.completed";

        public async Task<GptMessage?> CreateRunAsync(string threadId, string assistantId)
        {
            GptMessage? message = null;
            var request = new RestRequest($"/v1/threads/{threadId}/runs");
            request
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Authorization", _client.Config.SecretKey)
                .AddHeader("OpenAI-Beta", "assistants=v2");

            Dictionary<string, object> body = new()
            {
                { "assistant_id", assistantId },
                { "stream", true },
            };

            request.AdvancedResponseWriter = (response, request) =>
            {
                if (!response.IsSuccessStatusCode) throw new Exception($"뭔가 실패함 : {response}");
                var stream = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                using (var reader = new StreamReader(stream))
                {
                    string line = string.Empty;
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine() ?? string.Empty;
                        if (line == _messageCompleted)
                        {
                            line = reader.ReadLine() ?? throw new ArgumentNullException("null");
                            message = JsonConvert.DeserializeObject<GptMessage>(line[6..]);
                        }
                    }
                }

                return new RestResponse(request);
            };

            request.AddBody(body);

            var response = await _client.PostAsync(request);
            return message;
        }
    }
}
