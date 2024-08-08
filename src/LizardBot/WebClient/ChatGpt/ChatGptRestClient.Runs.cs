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
        private readonly string _runCompleted = "event: thread.run.completed";

        public async Task<(GptMessageObj?, GptRunObj?)> CreateRunAsync(string threadId, string assistantId)
        {
            GptMessageObj? message = null;
            GptRunObj? run = null;
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
                            _logger.LogInformation("message complete : {}", line);
                            message = JsonConvert.DeserializeObject<GptMessageObj>(line[6..]);
                        }

                        if (line == _runCompleted)
                        {
                            line = reader.ReadLine() ?? throw new ArgumentNullException("null");
                            _logger.LogInformation("run complete : {}", line);
                            try
                            {
                                run = JsonConvert.DeserializeObject<GptRunObj>(line[6..]);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                                Console.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }

                return new RestResponse(request);
            };

            request.AddBody(body);

            var response = await _client.PostAsync(request);
            return (message, run);
        }
    }
}
