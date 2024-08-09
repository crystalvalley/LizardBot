using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LizardBot.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LizardBot.DiscordBot.DiscordHandler
{
    public class CommandHandler : IHostedService
    {
        private readonly ILogger _logger;
        private readonly LizardBotClient _client;
        private readonly LizardBotCommandService _command;

        private readonly string _prefix;

        public CommandHandler(ILogger<CommandHandler> logger, LizardBotClient client, LizardBotCommandService command, IConfiguration config)
        {
            _logger = logger;
            _client = client;
            _command = command;
            _prefix = config["Prefix"] ?? throw new NoSettingDataException("Prefix");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceived;
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StopAsync(cancellationToken);
        }

        private async Task OnMessageReceived(SocketMessage message)
        {
            if (message is not SocketUserMessage socketUserMessage) return;
            if (socketUserMessage.Source != MessageSource.User) return;
            int argPos = 0;

            SocketCommandContext? context = new SocketCommandContext(_client, socketUserMessage);
            if (!socketUserMessage.HasCharPrefix('!', ref argPos)) return;

            await _command.ExecuteAsync(context, argPos);
        }
    }
}
