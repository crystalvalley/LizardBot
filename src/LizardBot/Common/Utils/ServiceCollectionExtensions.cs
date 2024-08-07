using LizardBot.Common.Exceptions;
using LizardBot.WebClient.ChatGpt;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RestSharp;

namespace LizardBot.Common.Utils
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 비동기 초기화가 필요한 클래스의 AddSingleton등록을 위한 메소드.
        /// </summary>
        /// <typeparam name="TService">등록 대상의 타입.</typeparam>
        /// <param name="services">서비스 콜렉션.</param>
        /// <param name="implementationFactory">등록을 위한 펑션.</param>
        /// <returns>서비스 콜레션.</returns>
        public static IServiceCollection AddSingletonWithAsyncInit<TService>(
            this IServiceCollection services,
            Func<IServiceProvider, Task<TService>> implementationFactory)
            where TService : class
        {
            services.AddSingleton<TService>(provider =>
            {
                var instance = implementationFactory(provider).GetAwaiter().GetResult();
                return instance;
            });
            return services;
        }

        public static IServiceCollection AddRestClients(this IServiceCollection collection, IConfigurationSection config)
        {
            collection.AddTransient(_ =>
            {
                var option = new ChatGptRestClientConfiguration()
                {
                    Options = new RestClientOptions()
                    {
                        Timeout = TimeSpan.FromMinutes(5),
                        BaseUrl = new Uri(config["BaseAddress"] ?? throw new NoSettingDataException("ConnectionString")),
                    },
                    SecretKey = $"Bearer {config["SecretKey"] ?? throw new NoSettingDataException("SecretKey")}",
                };
                return new RestClientWrapper<ChatGptRestClientConfiguration>(option);
            });
            collection.AddScoped<ChatGptRestClient>();
            return collection;
        }
    }
}
