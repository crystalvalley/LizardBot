using System.Reflection;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LizardBot.DiscordBot
{
    /// <summary>
    /// 커스텀 커맨드 서비스.
    /// <see cref="CommandService"/>를 참고.
    /// </summary>
    /// <param name="config">The <see cref="CommandServiceConfig"/> that shoud be inject.</param>
    public class LizardBotCommandService : CommandService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _provider;

        public LizardBotCommandService(CommandServiceConfig config, IServiceProvider provider)
        : base(config)
        {
            _logger = provider.GetRequiredService<ILogger<LizardBotCommandService>>();
            _provider = provider;
        }

        public async Task InitAsync()
        {
            await AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        public async Task<IResult> ExecuteAsync(SocketCommandContext context, int argPos) =>
            await ExecuteAsync(context, argPos, _provider);
    }
}
