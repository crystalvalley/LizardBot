using RestSharp;

namespace LizardBot.Common.Utils
{
    public class RestClientWrapper<T>
        : RestClient
        where T : IRestClientConfiguration
    {
        public T Config { get; }

        public RestClientWrapper(T config)
            : base(config.Options)
        {
            Config = config;
        }
    }
}
