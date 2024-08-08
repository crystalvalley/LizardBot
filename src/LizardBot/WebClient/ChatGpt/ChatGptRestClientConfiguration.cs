using LizardBot.Common.Utils;
using RestSharp;

namespace LizardBot.WebClient.ChatGpt
{
    public class ChatGptRestClientConfiguration : IRestClientConfiguration
    {
        public required RestClientOptions Options { get; init; }

        public required string SecretKey { get; init; }

        public RestClient CreateConfiguredClient()
        {
            throw new NotImplementedException();
        }
    }
}
