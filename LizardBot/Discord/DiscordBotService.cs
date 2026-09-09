using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace LizardBot.Discord;

public sealed class DiscordBotService(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IConfiguration configuration,
    ILogger<DiscordBotService> logger) : BackgroundService
{
    private bool _commandsRegistered;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client.Log += OnLogAsync;
        interactions.Log += OnLogAsync;

        client.Ready += OnReadyAsync;
        client.InteractionCreated += OnInteractionCreatedAsync;

        await interactions.AddModulesAsync(
            Assembly.GetExecutingAssembly(),
            services);

        var token = configuration["Discord:Token"];

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "Discord Bot Token is not configured.");

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task OnReadyAsync()
    {
        if (_commandsRegistered)
            return;

        var guildIdText = configuration["Discord:GuildId"];

        if (!ulong.TryParse(guildIdText, out var guildId))
            throw new InvalidOperationException(
                "Discord Guild ID is not configured.");

        await interactions.RegisterCommandsToGuildAsync(
            guildId,
            deleteMissing: true);

        _commandsRegistered = true;

        logger.LogInformation(
            "Discord commands registered to guild {GuildId}.",
            guildId);
    }

    private async Task OnInteractionCreatedAsync(
        SocketInteraction interaction)
    {
        var context =
            new SocketInteractionContext(client, interaction);

        var result = await interactions.ExecuteCommandAsync(
            context,
            services);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Interaction failed: {Error} - {Reason}",
                result.Error,
                result.ErrorReason);
        }
    }

    private Task OnLogAsync(LogMessage message)
    {
        logger.LogInformation(
            "[Discord] {Message}",
            message.ToString());

        return Task.CompletedTask;
    }
}