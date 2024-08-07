using LizardBot.WebClient.ChatGpt.Objects;
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
        /// 어시스턴트 생성.
        /// <see href="https://platform.openai.com/docs/api-reference/assistants/createAssistant"/>.
        /// </summary>
        /// <returns>생성된 어시스턴트 반환.</returns>
        public async Task<List<GptAssistant>> GetAssistantsAsync()
        {
            var request = new RestRequest("/v1/assistants");
            request
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Authorization", _client.Config.SecretKey)
                .AddHeader("OpenAI-Beta", "assistants=v2");

            var response = await _client.GetAsync(request);
            ArgumentNullException.ThrowIfNull(response.Content);
            var obj = JObject.Parse(response.Content)["data"];
            ArgumentNullException.ThrowIfNull(obj);
            return JsonConvert.DeserializeObject<List<GptAssistant>>(obj.ToString()) ?? [];
        }
    }
}
