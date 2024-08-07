using LizardBot.Common.Utils;
using Microsoft.Extensions.Logging;

namespace LizardBot.WebClient.ChatGpt
{
    /// <summary>
    /// ChatGpt용 RestClient.
    /// </summary>
    public partial class ChatGptRestClient
    {
        private readonly ILogger _logger;
        private readonly RestClientWrapper<ChatGptRestClientConfiguration> _client;

        public ChatGptRestClient(ILogger<ChatGptRestClient> logger, RestClientWrapper<ChatGptRestClientConfiguration> client)
        {
            _logger = logger;
            _client = client;
        }
    }
}
