using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
using System.Reflection;

namespace LizardBot.Discord;

/// <summary>
/// Maintains the Discord connection and dispatches incoming interactions
/// such as slash commands to the appropriate command modules.
/// </summary>
public sealed class DiscordBotService(
    DiscordSocketClient client,
    InteractionService interactions,
    IServiceProvider services,
    IConfiguration configuration,
    ILogger<DiscordBotService> logger) : BackgroundService
{

    // Discord may raise the Ready event more than once after reconnecting.
    // This flag prevents slash commands from being registered repeatedly.
    private bool _commandsRegistered;

    /// <summary>
    /// Starts the Discord connection and keeps it alive for the lifetime
    /// of the ASP.NET Core application.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Forward Discord.Net logs to the ASP.NET Core logging system.
        client.Log += OnLogAsync;
        interactions.Log += OnLogAsync;

        // Ready is raised after Discord finishes establishing the gateway connection.
        client.Ready += OnReadyAsync;

        // Every incoming slash command or other Discord interaction
        // is passed to InteractionService for command execution.
        client.InteractionCreated += OnInteractionCreatedAsync;

        // Discover InteractionModuleBase<T> classes in this assembly.
        // This allows command modules such as GeneralModule or ServerModule
        // to be registered automatically through dependency injection.
        await interactions.AddModulesAsync(Assembly.GetExecutingAssembly(), services);

        // The bot token should not be stored directly in appsettings.json
        // when the repository is public. In development it can be provided
        // through .NET User Secrets, and in production through environment variables.
        var token = configuration["Discord:Token"];

        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("Discord Bot Token is not configured.");
        }

        await client.LoginAsync(TokenType.Bot, token);
        await client.StartAsync();

        // BackgroundService must remain alive while the application is running.
        // The DiscordSocketClient itself handles the gateway connection in the background.
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
    /// <summary>
    /// Registers Discord application commands after the client becomes ready.
    /// </summary>
    private async Task OnReadyAsync()
    {
        if (_commandsRegistered)
            return;

        var guildIdText = configuration["Discord:GuildId"];

        if (!ulong.TryParse(guildIdText, out var guildId))
        {
            throw new InvalidOperationException("Discord Guild ID is not configured.");
        }

        // LizardBot is primarily designed for a specific community,
        // so its commands are registered only to the configured Discord guild.
        //
        // Guild commands are useful when a bot should expose commands only in
        // selected guilds, even if the bot itself is installed in multiple guilds.
        // To support multiple selected guilds, register the commands once for
        // each guild. If the commands should be available everywhere the bot is
        // installed, register them as global commands instead.
        //
        // Guild commands also become available much faster than global commands,
        // which makes them convenient during development and testing.
        //
        // Remove previously registered guild commands that no longer exist
        // in the current InteractionService modules.
        await interactions.RegisterCommandsToGuildAsync(guildId, deleteMissing: true);

        _commandsRegistered = true;

        logger.LogInformation("Discord commands registered to guild {GuildId}.", guildId);
    }

    /// <summary>
    /// Passes an incoming Discord interaction to InteractionService.
    /// </summary>
    private async Task OnInteractionCreatedAsync(SocketInteraction interaction)
    {
        var context = new SocketInteractionContext(
            client,
            interaction);

        // InteractionService locates the matching command module,
        // resolves its dependencies through DI, and executes the command.
        var result = await interactions.ExecuteCommandAsync(context, services);

        if (!result.IsSuccess)
        {
            logger.LogWarning(
                "Interaction failed: {Error} - {Reason}",
                result.Error,
                result.ErrorReason);
        }
    }

    /// <summary>
    /// Bridges Discord.Net log messages into ILogger.
    /// </summary>
    private Task OnLogAsync(LogMessage message)
    {
        logger.LogInformation("[Discord] {Message}", message.ToString());

        return Task.CompletedTask;
    }
}