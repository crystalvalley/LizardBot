using RestSharp;

namespace LizardBot.Common.Utils
{
    public interface IRestClientConfiguration
    {
        public RestClientOptions Options { get; }
    }
}
