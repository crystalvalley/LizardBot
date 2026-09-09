using Discord;
using Discord.WebSocket;
using LizardBot.Configuration;
using Microsoft.Extensions.Options;

namespace LizardBot.Services;

/// <summary>
/// Periodically monitors managed servers and sends Discord notifications
/// when their online status changes.
/// </summary>
public sealed class ServerMonitorService(
    ServerStatusService statusService,
    DiscordSocketClient discordClient,
    IOptionsMonitor<MonitoringOptions> options,
    IConfiguration configuration,
    ILogger<ServerMonitorService> logger) : BackgroundService
{
    private readonly Dictionary<string, bool> _previousStates =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Continuously checks configured servers until the application stops.
    /// </summary>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckServersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // A monitoring failure should not terminate LizardBot.
                // Log the error and try again on the next interval.
                logger.LogError(
                    ex,
                    "An error occurred while monitoring servers.");
            }

            var interval = TimeSpan.FromSeconds(
                Math.Max(5, options.CurrentValue.IntervalSeconds));

            await Task.Delay(interval, stoppingToken);
        }
    }

    /// <summary>
    /// Checks all configured servers and detects online status changes.
    /// </summary>
    private async Task CheckServersAsync(
        CancellationToken cancellationToken)
    {
        var statuses =
            await statusService.GetAllStatusesAsync(cancellationToken);

        foreach (var status in statuses)
        {
            if (!_previousStates.TryGetValue(
                    status.Id,
                    out var previousOnline))
            {
                // The first check establishes the initial state.
                // Do not send a notification every time LizardBot starts.
                _previousStates[status.Id] = status.Online;

                logger.LogInformation(
                    "Initial server state recorded: {ServerName} = {State}",
                    status.Name,
                    status.Online ? "ONLINE" : "OFFLINE");

                continue;
            }

            if (previousOnline == status.Online)
                continue;

            _previousStates[status.Id] = status.Online;

            await NotifyStatusChangeAsync(
                status.Name,
                status.Online);
        }
    }

    /// <summary>
    /// Sends an online or offline notification to the configured Discord channel.
    /// </summary>
    private async Task NotifyStatusChangeAsync(
        string serverName,
        bool online)
    {
        var channelIdText =
            configuration["Discord:StatusChannelId"];

        if (!ulong.TryParse(channelIdText, out var channelId))
        {
            logger.LogWarning(
                "Discord StatusChannelId is not configured.");

            return;
        }

        if (discordClient.GetChannel(channelId)
            is not IMessageChannel channel)
        {
            // This can happen if Discord has not finished connecting yet,
            // or if the configured channel does not exist or is inaccessible.
            logger.LogWarning(
                "Discord status channel {ChannelId} could not be found.",
                channelId);

            return;
        }

        var message = online
            ? $"🟢 **{serverName} ONLINE**\n서버가 다시 응답하기 시작했습니다."
            : $"🔴 **{serverName} OFFLINE**\n서버가 응답하지 않습니다.";

        await channel.SendMessageAsync(message);

        logger.LogInformation(
            "Server status changed: {ServerName} = {State}",
            serverName,
            online ? "ONLINE" : "OFFLINE");
    }
}