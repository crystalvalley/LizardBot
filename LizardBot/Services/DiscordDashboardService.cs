using Discord;
using Discord.Net;
using Discord.WebSocket;
using LizardBot.Configuration;
using LizardBot.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection;

namespace LizardBot.Services;

/// <summary>
/// Maintains a persistent Discord message that displays
/// the current status of managed servers.
/// </summary>
public sealed class DiscordDashboardService(
    DiscordSocketClient discordClient,
    DashboardStateStore stateStore,
    IConfiguration configuration,
    IOptionsMonitor<DashboardOptions> dashboardOptions,
    ILogger<DiscordDashboardService> logger) : BackgroundService
{

    // Dashboard operations are serialized so that a monitoring update
    // and a Discord reconnect cannot modify the same message at once.
    private readonly SemaphoreSlim _dashboardLock = new(1, 1);

    private const string DashboardDivider =
    "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━";

    DateTimeOffset? _lastUpdateUtc;

    private DashboardState? _state;
    private IUserMessage? _dashboardMessage;
    private CancellationToken _stoppingToken;

    /// <summary>
    /// Initializes persistent dashboard state and waits for Discord
    /// to become ready before restoring or creating the dashboard message.
    /// </summary>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _stoppingToken = stoppingToken;

        // Dashboard state is initialized as soon as LizardBot starts.
        // This also creates dashboard.json on the first run.
        _state = await stateStore.LoadOrCreateAsync(stoppingToken);

        discordClient.Ready += OnDiscordReadyAsync;

        // The Discord client may already be connected if this hosted service
        // starts after DiscordBotService has completed its connection.
        if (discordClient.ConnectionState == ConnectionState.Connected)
        {
            await EnsureDashboardMessageAsync(stoppingToken);
        }

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Normal application shutdown.
        }
        finally
        {
            discordClient.Ready -= OnDiscordReadyAsync;
        }
    }

    /// <summary>
    /// Restores the dashboard message whenever the Discord client becomes ready.
    /// </summary>
    private async Task OnDiscordReadyAsync()
    {
        try
        {
            await EnsureDashboardMessageAsync(_stoppingToken);
        }
        catch (OperationCanceledException)
            when (_stoppingToken.IsCancellationRequested)
        {
            // LizardBot is shutting down.
        }
        catch (Exception ex)
        {
            // A temporary Discord failure should not terminate LizardBot.
            logger.LogError(
                ex,
                "Failed to initialize the Discord dashboard.");
        }
    }

    /// <summary>
    /// Updates the Discord dashboard when the configured refresh interval
    /// has elapsed or when an immediate refresh is explicitly requested.
    /// </summary>
    public async Task UpdateAsync(
        IReadOnlyList<ServerStatus> statuses,
        bool force = false,
        CancellationToken cancellationToken = default)
    {
        if (discordClient.ConnectionState != ConnectionState.Connected)
            return;

        var refreshInterval = TimeSpan.FromMinutes(
            Math.Max(
                1,
                dashboardOptions.CurrentValue.RefreshIntervalMinutes));

        if (!force &&
            _lastUpdateUtc.HasValue &&
            DateTimeOffset.UtcNow - _lastUpdateUtc.Value < refreshInterval)
        {
            return;
        }

        await _dashboardLock.WaitAsync(cancellationToken);

        try
        {
            var embeds = BuildDashboardEmbeds(statuses);

            var message = await GetOrCreateDashboardMessageAsync(
                embeds,
                cancellationToken);

            try
            {
                await message.ModifyAsync(properties =>
                {
                    properties.Embeds = embeds;
                });

                _lastUpdateUtc = DateTimeOffset.UtcNow;
            }
            catch (HttpException ex)
                when (ex.HttpCode == HttpStatusCode.NotFound)
            {
                // The dashboard message may have been manually deleted.
                // Create a replacement and persist the new Message ID.
                logger.LogInformation(
                    "Dashboard message was deleted. Creating a new one.");

                _dashboardMessage = null;

                if (_state is not null)
                    _state.MessageId = null;

                await CreateDashboardMessageAsync(
                    embeds,
                    cancellationToken);

                _lastUpdateUtc = DateTimeOffset.UtcNow;
            }
        }
        finally
        {
            _dashboardLock.Release();
        }
    }

    /// <summary>
    /// Ensures that a valid Discord dashboard message exists.
    /// </summary>
    private async Task EnsureDashboardMessageAsync(
        CancellationToken cancellationToken)
    {
        await _dashboardLock.WaitAsync(cancellationToken);

        try
        {
            await GetOrCreateDashboardMessageAsync(
                BuildInitializingEmbeds(),
                cancellationToken);
        }
        finally
        {
            _dashboardLock.Release();
        }
    }

    /// <summary>
    /// Returns the existing dashboard message when possible,
    /// or creates a new one when no valid message exists.
    /// </summary>
    private async Task<IUserMessage> GetOrCreateDashboardMessageAsync(
        Embed[] initialEmbeds,
        CancellationToken cancellationToken)
    {
        if (_dashboardMessage is not null)
            return _dashboardMessage;

        if (_state is null)
        {
            _state = await stateStore.LoadOrCreateAsync(
                cancellationToken);
        }

        var channel = await GetDashboardChannelAsync();

        if (_state.MessageId.HasValue)
        {
            try
            {
                var existingMessage = await channel.GetMessageAsync(
                    _state.MessageId.Value);

                // Only reuse the message when it was created by this bot.
                if (existingMessage is IUserMessage userMessage &&
                    userMessage.Author.Id == discordClient.CurrentUser.Id)
                {
                    _dashboardMessage = userMessage;

                    logger.LogInformation(
                        "Restored existing Discord dashboard message {MessageId}.",
                        userMessage.Id);

                    return userMessage;
                }
            }
            catch (HttpException ex)
                when (ex.HttpCode == HttpStatusCode.NotFound)
            {
                // The stored Message ID is valid state, but the actual
                // Discord message no longer exists.
                logger.LogInformation(
                    "Stored dashboard message {MessageId} no longer exists.",
                    _state.MessageId);
            }
        }

        return await CreateDashboardMessageAsync(
            initialEmbeds,
            cancellationToken);
    }

    /// <summary>
    /// Creates a new dashboard message and persists its Message ID.
    /// </summary>
    private async Task<IUserMessage> CreateDashboardMessageAsync(
        Embed[] embeds,
        CancellationToken cancellationToken)
    {
        if (_state is null)
        {
            _state = await stateStore.LoadOrCreateAsync(
                cancellationToken);
        }

        var channel = await GetDashboardChannelAsync();

        var message = await channel.SendMessageAsync(
            embeds: embeds);

        _dashboardMessage = message;
        _state.MessageId = message.Id;

        await stateStore.SaveAsync(
            _state,
            cancellationToken);

        logger.LogInformation(
            "Created Discord dashboard message {MessageId} in channel {ChannelId}.",
            message.Id,
            _state.ChannelId);

        return message;
    }

    /// <summary>
    /// Resolves the configured Discord dashboard channel.
    /// </summary>
    private async Task<IMessageChannel> GetDashboardChannelAsync()
    {
        if (_state is null)
        {
            throw new InvalidOperationException(
                "Dashboard state has not been initialized.");
        }

        var guildIdText = configuration["Discord:GuildId"];
        if (await discordClient.GetChannelAsync(_state.ChannelId)
            is not IMessageChannel channel)
        {
            throw new InvalidOperationException(
                $"Discord dashboard channel '{_state.ChannelId}' could not be found.");
        }

        return channel;
    }

    /// <summary>
    /// Creates the temporary embed shown before the first server status check.
    /// </summary>
    private Embed[] BuildInitializingEmbeds()
    {
        var options = dashboardOptions.CurrentValue;

        return
        [
            new EmbedBuilder()
            .WithTitle(options.Title)
            .WithDescription("⏳ 서버 상태를 불러오는 중입니다...")
            .WithFooter($"🦎 LizardBot v{GetBotVersion()}")
            .WithTimestamp(DateTimeOffset.UtcNow)
            .Build()
        ];
    }

    /// <summary>
    /// Builds all Discord embeds used by the server dashboard.
    /// The first embed contains the overall server status,
    /// followed by one embed for each configured notice.
    /// </summary>
    private Embed[] BuildDashboardEmbeds(
        IReadOnlyList<ServerStatus> statuses)
    {
        var options = dashboardOptions.CurrentValue;
        var embeds = new List<Embed>();

        embeds.Add(
            BuildOverviewEmbed(
                options,
                statuses));

        foreach (var notice in options.Notices)
        {
            embeds.Add(
                BuildNoticeEmbed(
                    notice,
                    statuses));
        }

        return embeds.ToArray();
    }

    /// <summary>
    /// Adds configured managed servers and their health checks
    /// to the dashboard embed.
    /// </summary>
    private void AddServerFields(
        EmbedBuilder embed,
        DashboardOptions options,
        IReadOnlyList<ServerStatus> statuses)
    {
        foreach (var displayOptions in options.Servers)
        {
            if (!displayOptions.Visible)
                continue;

            var status = statuses.FirstOrDefault(x =>
                string.Equals(
                    x.Id,
                    displayOptions.ServerId,
                    StringComparison.OrdinalIgnoreCase));

            if (status is null)
            {
                // Keep configuration mistakes visible instead of silently
                // hiding a server from the dashboard.
                embed.AddField(
                    $"⚠️ {displayOptions.ServerId}",
                    "설정된 서버를 찾을 수 없습니다.",
                    inline: false);

                continue;
            }

            var lines = new List<string>
        {
            status.Online
                ? "**상태** : 🟢 ONLINE"
                : "**상태** : 🔴 OFFLINE"
        };

            // ShowUptime will be handled after OnlineSince tracking is added.

            if (displayOptions.ShowServices &&
                status.Services.Count > 0)
            {
                lines.Add(string.Empty);

                foreach (var service in status.Services)
                {
                    lines.Add(
                        $"{(service.Online ? "✅" : "❌")} {service.Name}");
                }
            }

            embed.AddField(
                $"{(status.Online ? "🟢" : "🔴")} {status.Name}",
                string.Join(Environment.NewLine, lines),
                inline: false);
        }
    }

    /// <summary>
    /// Builds a dedicated embed for a configured notice or service.
    /// </summary>
    private Embed BuildNoticeEmbed(
        DashboardNoticeOptions notice,
        IReadOnlyList<ServerStatus> statuses)
    {
        var embed = new EmbedBuilder()
            .WithTitle(notice.Title);

        var description = new List<string>();

        if (!string.IsNullOrWhiteSpace(notice.Description))
            description.Add(notice.Description);

        var linkedStatus = GetLinkedStatus(
            notice,
            statuses);

        if (linkedStatus is not null)
        {
            if (description.Count > 0)
                description.Add(string.Empty);

            description.Add(
                $"📡 **상태**　{linkedStatus}");
        }

        // Use the same divider in every notice embed.
        // Besides separating status from service information,
        // it also gives short embeds a similar minimum visual width.
        if (description.Count > 0)
            description.Add(string.Empty);

        description.Add(DashboardDivider);

        embed.WithDescription(
            string.Join(
                Environment.NewLine,
                description));

        foreach (var field in notice.Fields)
        {
            embed.AddField(
                field.Name,
                field.Value,
                inline: false);
        }

        if (!string.IsNullOrWhiteSpace(notice.Footer))
            embed.WithFooter(notice.Footer);

        return embed.Build();
    }

    /// <summary>
    /// Resolves live server or service status for a notice
    /// linked to a managed server.
    /// </summary>
    private static string? GetLinkedStatus(
        DashboardNoticeOptions notice,
        IReadOnlyList<ServerStatus> statuses)
    {
        if (string.IsNullOrWhiteSpace(notice.ServerId))
            return null;

        var server = statuses.FirstOrDefault(x =>
            string.Equals(
                x.Id,
                notice.ServerId,
                StringComparison.OrdinalIgnoreCase));

        if (server is null)
            return "⚪ UNKNOWN";

        // If no specific service is configured,
        // display the overall server status.
        if (string.IsNullOrWhiteSpace(notice.ServiceName))
        {
            return server.Online
                ? "🟢 ONLINE"
                : "🔴 OFFLINE";
        }

        // A service cannot be online when its host itself is offline.
        if (!server.Online)
            return "🔴 OFFLINE";

        var service = server.Services.FirstOrDefault(x =>
            string.Equals(
                x.Name,
                notice.ServiceName,
                StringComparison.OrdinalIgnoreCase));

        if (service is null)
            return "⚪ UNKNOWN";

        return service.Online
            ? "🟢 ONLINE"
            : "🔴 OFFLINE";
    }

    /// <summary>
    /// Returns the version of the currently running LizardBot build.
    /// </summary>
    private static string GetBotVersion()
    {
        var assembly = Assembly.GetEntryAssembly();

        var version = assembly?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        if (string.IsNullOrWhiteSpace(version))
            return "unknown";

        // InformationalVersion may contain build metadata such as
        // "0.1.0+abc123". Only the human-readable version is shown.
        return version.Split('+')[0];
    }

    /// <summary>
    /// Builds the main dashboard embed containing general information
    /// and the current status of configured servers.
    /// </summary>
    private Embed BuildOverviewEmbed(
        DashboardOptions options,
        IReadOnlyList<ServerStatus> statuses)
    {
        var embed = new EmbedBuilder()
            .WithTitle(options.Title)
            .WithTimestamp(DateTimeOffset.UtcNow)
            .WithFooter($"🦎 LizardBot v{GetBotVersion()}");

        var description = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.Description))
            description.Add(options.Description);

        if (!string.IsNullOrWhiteSpace(options.Administrator))
        {
            description.Add(string.Empty);
            description.Add(
                $"🛠️ **관리자**　{options.Administrator}");
        }

        if (description.Count > 0)
            description.Add(string.Empty);

        description.Add(DashboardDivider);

        if (description.Count > 0)
        {
            embed.WithDescription(
                string.Join(Environment.NewLine, description));
        }

        foreach (var displayOptions in options.Servers)
        {
            if (!displayOptions.Visible)
                continue;

            var status = statuses.FirstOrDefault(x =>
                string.Equals(
                    x.Id,
                    displayOptions.ServerId,
                    StringComparison.OrdinalIgnoreCase));

            if (status is null)
            {
                embed.AddField(
                    $"⚠️ {displayOptions.ServerId}",
                    "설정된 서버를 찾을 수 없습니다.",
                    inline: false);

                continue;
            }

            var lines = new List<string>
        {
            status.Online
                ? "**상태**　🟢 ONLINE"
                : "**상태**　🔴 OFFLINE"
        };

            // Uptime will be added here when OnlineSince tracking is implemented.

            if (displayOptions.ShowServices &&
                status.Services.Count > 0)
            {
                lines.Add(string.Empty);

                foreach (var service in status.Services)
                {
                    lines.Add(
                        $"{(service.Online ? "✅" : "❌")} {service.Name}");
                }
            }

            embed.AddField(
                $"🖥️ {status.Name}",
                string.Join(Environment.NewLine, lines),
                inline: false);
        }

        return embed.Build();
    }
}